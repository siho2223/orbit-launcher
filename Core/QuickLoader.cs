using System.Net;
using System.Security.Cryptography;
using System.Text.Json.Nodes;
using System.Xml;
using System.Xml.Linq;
using System.Text.RegularExpressions;
namespace Orbit.Core;
// All loader work runs in a private staging root; existing profiles and content are never replaced.
public sealed class QuickLoader:IDisposable {
 readonly HttpClient http;
 public QuickLoader(HttpMessageHandler? handler=null){http=new(handler??new HttpClientHandler{AllowAutoRedirect=false,AutomaticDecompression=DecompressionMethods.All}){Timeout=TimeSpan.FromSeconds(60)};http.DefaultRequestHeaders.UserAgent.ParseAdd("ORBIT/0.12.0");}
 public void Dispose()=>http.Dispose();
 async Task<string> Text(string url,CancellationToken ct){using var response=await http.GetAsync(url,HttpCompletionOption.ResponseHeadersRead,ct);response.EnsureSuccessStatusCode();using var stream=await response.Content.ReadAsStreamAsync(ct);using var data=new MemoryStream();byte[] buffer=new byte[16384];int size;while((size=await stream.ReadAsync(buffer,ct))>0){if(data.Length+size>4*1024*1024)throw new IOException("Loader metadata is too large.");data.Write(buffer,0,size);}return System.Text.Encoding.UTF8.GetString(data.ToArray());}
 public async Task<string> ResolveVersionAsync(string minecraft,string loader,CancellationToken ct=default){MojangInstaller.ValidateId(minecraft);if(loader=="Fabric"){var data=JsonNode.Parse(await Text("https://meta.fabricmc.net/v2/versions/loader/"+Uri.EscapeDataString(minecraft),ct)) as JsonArray;var item=data?.FirstOrDefault(x=>x?["loader"]?["stable"]?.GetValue<bool>()==true)??data?.FirstOrDefault();return item?["loader"]?["version"]?.ToString()??throw new IOException("Fabric is not available for Minecraft "+minecraft);}
 if(loader!="Forge")throw new InvalidDataException("Choose Forge or Fabric.");var promos=JsonNode.Parse(await Text("https://files.minecraftforge.net/net/minecraftforge/forge/promotions_slim.json",ct))?["promos"];string version=promos?[minecraft+"-recommended"]?.ToString()??promos?[minecraft+"-latest"]?.ToString()??throw new IOException("Forge is not available for Minecraft "+minecraft);string expected=minecraft+"-"+version;
 using var reader=XmlReader.Create(new StringReader(await Text("https://maven.minecraftforge.net/net/minecraftforge/forge/maven-metadata.xml",ct)),new XmlReaderSettings{DtdProcessing=DtdProcessing.Prohibit,XmlResolver=null,MaxCharactersInDocument=4*1024*1024});var values=XDocument.Load(reader).Descendants("version").Select(x=>x.Value).ToArray();string build=values.FirstOrDefault(v=>v==expected)??values.FirstOrDefault(v=>v.StartsWith(expected+"-",StringComparison.Ordinal))??throw new IOException("Forge build metadata is unavailable.");return build[(minecraft.Length+1)..];}
 public async Task<string> InstallAsync(string root,string minecraft,string loader,IProgress<InstallProgress>? vanillaProgress=null,IProgress<ModProgress>? loaderProgress=null,CancellationToken ct=default){
 root=Path.GetFullPath(root);MojangInstaller.ValidateId(minecraft);string version=await ResolveVersionAsync(minecraft,loader,ct);if(!Regex.IsMatch(version,"^[A-Za-z0-9.+_-]{1,100}$"))throw new InvalidDataException("Invalid loader version.");
 using(var mojang=new MojangInstaller()){var remote=(await mojang.CatalogAsync(ct)).FirstOrDefault(v=>v.Id==minecraft)??throw new IOException("Minecraft version is not available.");await mojang.InstallAsync(root,remote,vanillaProgress,ct);}
 return await InstallPreparedAsync(root,minecraft,loader,version,loaderProgress,ct);
 }
 public async Task<string> InstallPreparedAsync(string root,string minecraft,string loader,string version,IProgress<ModProgress>? loaderProgress=null,CancellationToken ct=default,string? runtimeRoot=null){
 root=Path.GetFullPath(root);MojangInstaller.ValidateId(minecraft);if(loader is not("Forge" or "Fabric")||!Regex.IsMatch(version,"^[A-Za-z0-9.+_-]{1,100}$"))throw new InvalidDataException("Invalid loader selection.");
 string stagingParent=Path.Combine(root,".orbit-loader-staging");Directory.CreateDirectory(stagingParent);string stage=Path.Combine(stagingParent,Guid.NewGuid().ToString("N"));Directory.CreateDirectory(stage);
 try{string vanilla=Path.Combine(stage,"versions",minecraft);Directory.CreateDirectory(vanilla);foreach(string extension in new[]{".json",".jar"})File.Copy(Path.Combine(root,"versions",minecraft,minecraft+extension),Path.Combine(vanilla,minecraft+extension));
 using var installer=new Modpacks();string id=await installer.InstallLoaderAsync(stage,new PackIndex(loader+" "+minecraft,minecraft,loader=="Fabric"?"fabric-loader":"forge",version,Array.Empty<PackFile>()),loaderProgress,ct,runtimeRoot??root);ct.ThrowIfCancellationRequested();var resolved=Versions.Resolve(stage,id);if(!resolved.Chain.Contains(minecraft))throw new IOException("Loader profile has an unexpected parent.");
 // Libraries first; the profile is published last, atomically per file.
 Promote(Path.Combine(stage,"libraries"),Path.Combine(root,"libraries"),ct);Promote(Path.Combine(stage,"versions",id),Path.Combine(root,"versions",id),ct);return id;
 }finally{string checkedStage=Path.GetFullPath(stage);if(checkedStage.StartsWith(Path.GetFullPath(stagingParent)+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase)&&Directory.Exists(checkedStage))Directory.Delete(checkedStage,true);}
 }
 public static void Promote(string source,string destination,CancellationToken ct){if(!Directory.Exists(source))return;var files=Directory.EnumerateFiles(source,"*",SearchOption.AllDirectories).ToArray();
 foreach(var file in files){ct.ThrowIfCancellationRequested();if((File.GetAttributes(file)&FileAttributes.ReparsePoint)!=0)throw new IOException("Linked loader files are not allowed.");string target=Versions.Within(destination,Path.GetRelativePath(source,file));if(File.Exists(target)&&!SHA256.HashData(File.ReadAllBytes(file)).SequenceEqual(SHA256.HashData(File.ReadAllBytes(target))))throw new IOException("Existing loader file differs; nothing was overwritten: "+Path.GetFileName(target));}
 foreach(var file in files.OrderBy(f=>f.EndsWith(".json",StringComparison.OrdinalIgnoreCase))){ct.ThrowIfCancellationRequested();string target=Versions.Within(destination,Path.GetRelativePath(source,file));if(File.Exists(target))continue;Directory.CreateDirectory(Path.GetDirectoryName(target)!);string temporary=target+".orbit-"+Guid.NewGuid().ToString("N")+".tmp";try{File.Copy(file,temporary);ct.ThrowIfCancellationRequested();File.Move(temporary,target,false);}finally{if(File.Exists(temporary))File.Delete(temporary);}}
 }
}

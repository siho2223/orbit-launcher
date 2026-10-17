using System.IO.Compression;
using System.Text.Json;
namespace Orbit.Core;
public sealed record ExportProgress(int Files, int TotalFiles, long Bytes, long TotalBytes);
public sealed record ExportResult(int Files, long Bytes);
public static class PackExporter {
 static readonly string[] Folders={"mods","config","defaultconfigs","shaderpacks","resourcepacks","scripts","kubejs","crafttweaker","patchouli_books"};
 static bool Linked(string path)=>(File.GetAttributes(path)&FileAttributes.ReparsePoint)!=0;
 public static async Task<ExportResult> ExportAsync(string source,string destination,VersionEntry version,IProgress<ExportProgress>? progress=null,CancellationToken ct=default){
  source=Path.GetFullPath(source);destination=Path.GetFullPath(destination);
  if(!Directory.Exists(source))throw new DirectoryNotFoundException("Instance folder does not exist.");
  if(Linked(source))throw new IOException("Export from a linked instance folder is not supported.");
  string prefix=source.TrimEnd(Path.DirectorySeparatorChar)+Path.DirectorySeparatorChar;
  if(destination.StartsWith(prefix,StringComparison.OrdinalIgnoreCase))throw new IOException("Save the ZIP outside the instance folder.");
  var files=new List<(string Full,string Relative,long Size)>();
  void Walk(string dir){ct.ThrowIfCancellationRequested();if(Linked(dir))throw new IOException("Linked folders cannot be exported: "+Path.GetFileName(dir));foreach(var path in Directory.EnumerateFileSystemEntries(dir)){ct.ThrowIfCancellationRequested();if(Linked(path))throw new IOException("Linked files cannot be exported: "+Path.GetFileName(path));if(Directory.Exists(path))Walk(path);else{string rel=Path.GetRelativePath(source,path).Replace('\\','/');files.Add((path,rel,new FileInfo(path).Length));}}}
  foreach(var folder in Folders){string dir=Path.Combine(source,folder);if(Directory.Exists(dir))Walk(dir);}
  foreach(string name in new[]{"options.txt","optionsof.txt","optionsshaders.txt"}){string full=Path.Combine(source,name);if(File.Exists(full)){if(Linked(full))throw new IOException("Linked settings cannot be exported.");files.Add((full,name,new FileInfo(full).Length));}}
  if(files.Count==0)throw new IOException("There are no mods, configs, shaders or resource packs to export.");
  long lastReport=0;long total=files.Sum(f=>f.Size),done=0;int count=0;string temp=destination+".partial-"+Guid.NewGuid().ToString("N");
  try{
   await using(var output=new FileStream(temp,FileMode.CreateNew,FileAccess.Write,FileShare.None,81920,true)){
    using var zip=new ZipArchive(output,ZipArchiveMode.Create,true);
    foreach(var f in files.OrderBy(f=>f.Relative,StringComparer.Ordinal)){
     ct.ThrowIfCancellationRequested();progress?.Report(new(count,files.Count,done,total));
     var entry=zip.CreateEntry("overrides/"+f.Relative,CompressionLevel.Optimal);
     await using var input=new FileStream(f.Full,FileMode.Open,FileAccess.Read,FileShare.Read,81920,true);
     if(input.Length!=f.Size)throw new IOException("Instance files changed during export. Try again after closing the game.");
     await using(var target=entry.Open()){byte[] buffer=new byte[81920];int n;while((n=await input.ReadAsync(buffer,ct))>0){await target.WriteAsync(buffer.AsMemory(0,n),ct);done+=n;if(Environment.TickCount64-lastReport>80){progress?.Report(new(count,files.Count,done,total));lastReport=Environment.TickCount64;}}}
     count++;
    }
    using(var writer=new StreamWriter(zip.CreateEntry("orbit-pack.json").Open()))await writer.WriteAsync(JsonSerializer.Serialize(new{formatVersion=1,name=version.Title,minecraft=version.BaseVersion,loader=version.Loader,profile=version.Id,java=version.JavaMajor,content="overrides",files=count},new JsonSerializerOptions{WriteIndented=true}));
    using(var writer=new StreamWriter(zip.CreateEntry("README.txt").Open()))await writer.WriteAsync($"{version.Title}\nMinecraft: {version.BaseVersion}\nLoader: {version.Loader}\nProfile: {version.Id}\nJava: {version.JavaMajor}\n\nInstall the matching Minecraft and loader version in your launcher, then copy the CONTENTS of overrides into its instance folder. This is a content ZIP, not a .mrpack or CurseForge import archive.\nGame binaries, Java, accounts, logs, screenshots and saved worlds are not included.\n");
   }
   ct.ThrowIfCancellationRequested();File.Move(temp,destination,true);progress?.Report(new(count,files.Count,done,total));return new(count,done);
  }finally{if(File.Exists(temp))File.Delete(temp);}
 }
}

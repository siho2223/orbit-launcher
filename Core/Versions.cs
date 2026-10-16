using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
namespace Orbit.Core;
public sealed record VersionEntry(string Id,string Loader,string BaseVersion,int JavaMajor){public bool Installed{get;init;}=true;public RemoteVersion? Remote{get;init;}public string StateLabel=>Installed?"УСТАНОВЛЕНО":"↓  СКАЧАТЬ";public string Title=>Id;public string Subtitle=>Installed?$"{Loader}  ·  Java {JavaMajor}":$"Mojang  ·  {(Remote?.Type=="release"?"Релиз":"Снапшот")}  ·  {Remote?.Released:dd.MM.yyyy}";public string Initial=>Loader switch{"NeoForge"=>"N","Forge"=>"F","Fabric"=>"F","Quilt"=>"Q",_=>"M"};}
public sealed class ResolvedVersion {
 public required string Id{get;init;} public string MainClass="",Jar="",Assets="",Type="release",JavaComponent="jre-legacy";public int JavaMajor=8;
 public List<JsonNode> GameArgs=new(),JvmArgs=new();public Dictionary<string,JsonObject> Libraries=new();public List<string> Chain=new();
}
public static class Versions {
 public static string Within(string root,string relative){string prefix=Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar)+Path.DirectorySeparatorChar;string full=Path.GetFullPath(Path.Combine(prefix,relative.Replace('/',Path.DirectorySeparatorChar)));if(!full.StartsWith(prefix,StringComparison.OrdinalIgnoreCase))throw new InvalidDataException("Путь выходит за пределы папки игры.");return full;}
 public static ResolvedVersion Resolve(string root,string id){var result=new ResolvedVersion{Id=id};var seen=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
  void Load(string current){if(!seen.Add(current)||seen.Count>20)throw new InvalidDataException("Цикл наследования версий.");if(current.IndexOfAny(new[]{'/','\\',':'})>=0||current is "." or "..")throw new InvalidDataException("Некорректный ID версии.");
   string file=Within(Path.Combine(root,"versions"),Path.Combine(current,current+".json"));var d=JsonNode.Parse(File.ReadAllText(file))!.AsObject();if(d["inheritsFrom"] is {} parent)Load(parent.ToString());result.Chain.Add(current);
   if(d["mainClass"] is {} main)result.MainClass=main.ToString();if(d["type"] is {} type)result.Type=type.ToString();if(d["assetIndex"]?["id"] is {} assets)result.Assets=assets.ToString();else if(d["assets"] is {} old)result.Assets=old.ToString();
   if(d["javaVersion"]?["majorVersion"] is {} major)result.JavaMajor=(int)major.GetValue<double>();if(d["javaVersion"]?["component"] is {} component)result.JavaComponent=component.ToString();
   string jarId=d["jar"]?.ToString()??current;if(jarId.IndexOfAny(new[]{'/','\\',':'})>=0)throw new InvalidDataException("Некорректный JAR.");string jar=Within(Path.Combine(root,"versions"),Path.Combine(jarId,jarId+".jar"));if(File.Exists(jar)&&(d["downloads"]?["client"]!=null||result.Jar==""))result.Jar=jar;
   if(d["arguments"]?["game"] is JsonArray game)result.GameArgs.AddRange(game.Select(n=>n!.DeepClone()));else if(d["minecraftArguments"] is {} legacy){result.GameArgs.Clear();result.GameArgs.AddRange(Tokenize(legacy.ToString()).Select(s=>(JsonNode)JsonValue.Create(s)!));}
   if(d["arguments"]?["jvm"] is JsonArray jvm)result.JvmArgs.AddRange(jvm.Select(n=>n!.DeepClone()));
   if(d["libraries"] is JsonArray libs)foreach(var node in libs.OfType<JsonObject>()){if(!LaunchEngine.Allowed(node["rules"]))continue;string name=node["name"]?.ToString()??throw new InvalidDataException("Библиотека без имени.");var parts=name.Split(':');string key=parts.Length>=3?parts[0]+":"+parts[1]+(parts.Length>3?":"+parts[3]:""):name;result.Libraries[key]=(JsonObject)node.DeepClone();}
  }
  Load(id);if(result.MainClass=="")throw new InvalidDataException("Нет mainClass.");return result;
 }
 public static List<VersionEntry> Scan(string root,Action<string>? warning=null){string dir=Path.Combine(root,"versions");var result=new List<VersionEntry>();if(!Directory.Exists(dir))return result;foreach(string folder in Directory.EnumerateDirectories(dir)){string id=Path.GetFileName(folder);if(!File.Exists(Path.Combine(folder,id+".json")))continue;try{var v=Resolve(root,id);string desc=(id+" "+v.MainClass+" "+string.Join(" ",v.Libraries.Keys)).ToLowerInvariant();string loader=desc.Contains("neoforge")?"NeoForge":desc.Contains("fabric")?"Fabric":desc.Contains("quilt")?"Quilt":desc.Contains("forge")?"Forge":"Vanilla";result.Add(new(id,loader,v.Chain[0],v.JavaMajor));}catch(Exception e){warning?.Invoke(id+": "+e.Message);}}return result.OrderBy(v=>v.Loader!="Vanilla").ThenByDescending(v=>v.Id,StringComparer.OrdinalIgnoreCase).ToList();}
 public static IEnumerable<string> Tokenize(string s)=>Regex.Matches(s,"\"([^\"]*)\"|'([^']*)'|([^\\s]+)").Select(m=>m.Groups[1].Success?m.Groups[1].Value:m.Groups[2].Success?m.Groups[2].Value:m.Groups[3].Value);
}

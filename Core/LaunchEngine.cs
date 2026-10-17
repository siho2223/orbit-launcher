using System.Diagnostics;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
namespace Orbit.Core;

public sealed record JavaRuntime(string Path,int Major);
public sealed class LaunchPlan {
 public required ResolvedVersion Version{get;init;} public required string Java{get;init;} public required string GameDirectory{get;init;} public required string Natives{get;init;}
 public List<string> Arguments=new(),Missing=new(),NativeArchives=new();public HashSet<string> SensitiveValues=new(StringComparer.Ordinal);
 public string Summary=>Missing.Count==0?$"Готово · Java {Version.JavaMajor} · {Version.Libraries.Count} библиотек":$"Не хватает файлов: {Missing.Count}";
}
public static class LaunchEngine {
 public static string OfflineUuid(string name){byte[] b=MD5.HashData(Encoding.UTF8.GetBytes("OfflinePlayer:"+name));b[6]=(byte)((b[6]&15)|48);b[8]=(byte)((b[8]&63)|128);return Convert.ToHexString(b).ToLowerInvariant();}
 public static bool Allowed(JsonNode? rules){if(rules is not JsonArray a||a.Count==0)return true;bool allowed=false;foreach(var rule in a){if(rule==null)continue;bool match=true;if(rule["os"] is {} os){if(os["name"] is {} n&&n.ToString()!="windows")match=false;if(os["arch"] is {} arch){string x=arch.ToString();match&=x=="x86_64"||x=="amd64"||x=="x64";}if(os["version"] is {} version)match&=Regex.IsMatch(Environment.OSVersion.Version.ToString(),version.ToString(),RegexOptions.None,TimeSpan.FromSeconds(1));}if(rule["features"] is JsonObject features)foreach(var f in features){bool actual=f.Key=="has_custom_resolution";if(f.Value?.GetValue<bool>()!=actual)match=false;}if(match)allowed=rule["action"]?.ToString()=="allow";}return allowed;}
 public static IEnumerable<string> ArgumentValues(IEnumerable<JsonNode> nodes){foreach(var n in nodes){if(n is JsonValue){yield return n.ToString();continue;}if(!Allowed(n["rules"]))continue;var value=n["value"]??n["values"];if(value is JsonArray a){foreach(var v in a)yield return v!.ToString();}else if(value!=null)yield return value.ToString();}}
 public static string MavenPath(string name){string[] a=name.Split(':');if(a.Length<3)throw new InvalidDataException("Неверное имя библиотеки: "+name);string version=a[2],ext="jar";if(version.Contains('@')){var e=version.Split('@');version=e[0];ext=e[1];}string classifier=a.Length>3?"-"+a[3]:"";return a[0].Replace('.','/')+"/"+a[1]+"/"+version+"/"+a[1]+"-"+version+classifier+"."+ext;}
 public static JavaRuntime? ReadJava(string path){if(!File.Exists(path))return null;try{string release=System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(path))!,"release");string text=File.ReadAllText(release);var m=Regex.Match(text,"JAVA_VERSION=\"(1\\.)?(\\d+)");if(m.Success)return new(path,int.Parse(m.Groups[2].Value));}catch{}return null;}
 public static JavaRuntime? FindJava(string root,int major,string custom=""){
  if(custom.Length>0){var chosen=ReadJava(custom);if(chosen==null)throw new InvalidDataException("Не удалось определить Java. Выбери java.exe в папке bin установленной Java.");if(chosen.Major!=major)throw new InvalidDataException($"Нужна Java {major}, выбрана Java {chosen.Major}.");return chosen;}
  var roots=new[]{Path.Combine(root,"runtime"),Path.Combine(root,"jre"),Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),"Java"),Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),"Eclipse Adoptium"),Environment.GetEnvironmentVariable("JAVA_HOME")??""};
  foreach(string folder in roots.Where(Directory.Exists))try{foreach(string release in Directory.EnumerateFiles(folder,"release",SearchOption.AllDirectories)){var java=ReadJava(Path.Combine(Path.GetDirectoryName(release)!,"bin","java.exe"));if(java?.Major==major)return java;}}catch(UnauthorizedAccessException){}
  return null;
 }
 public static string InstanceDirectory(LauncherSettings settings,string id)=>File.Exists(Path.Combine(settings.GameRoot,".orbit-pack.json"))?Path.GetFullPath(settings.GameRoot):settings.Isolated?Path.Combine(LauncherSettings.DataRoot,"instances",Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(Path.GetFullPath(settings.GameRoot)+"|"+id)))[..16]):Path.GetFullPath(settings.GameRoot);
 public static LaunchPlan Build(LauncherSettings s,string id,MicrosoftSession? session=null){
  if(!Regex.IsMatch(s.Nickname,"^[A-Za-z0-9_]{3,16}$"))throw new InvalidDataException("Ник: 3–16 латинских букв, цифр или подчёркиваний.");
  string root=Path.GetFullPath(s.GameRoot);var v=Versions.Resolve(root,id);var java=FindJava(root,v.JavaMajor,s.JavaPath);string game=InstanceDirectory(s,id);
  var plan=new LaunchPlan{Version=v,Java=java?.Path??"",GameDirectory=game,Natives=Path.Combine(LauncherSettings.DataRoot,"natives",Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(root+"|"+id)))[..16])};
  if(java==null)plan.Missing.Add("Java "+v.JavaMajor+" (укажи путь в настройках)");if(!File.Exists(v.Jar))plan.Missing.Add("Клиентский JAR версии "+id);
  var classpath=new List<string>();string libroot=Path.Combine(root,"libraries");
  void AddFile(string path,bool native=false){if(!File.Exists(path))plan.Missing.Add(path);else {if(!classpath.Contains(path))classpath.Add(path);if(native&&!plan.NativeArchives.Contains(path))plan.NativeArchives.Add(path);}}
  foreach(var lib in v.Libraries.Values){if(!Allowed(lib["rules"]))continue;string name=lib["name"]!.ToString();
   // Some TLauncher metadata omits architecture rules for alternative LWJGL native classifiers.
   if(name.Contains(":natives-")&&(name.Contains("macos")||name.Contains("linux")||name.Contains("arm64")||name.Contains("windows-x86")))continue;
   var artifact=lib["downloads"]?["artifact"];if(artifact!=null||lib["natives"]==null){string relative=artifact?["path"]?.ToString()??MavenPath(name);AddFile(Versions.Within(libroot,relative),name.Contains("natives-windows"));}
   if(lib["natives"]?["windows"] is {} nativeName){string classifier=nativeName.ToString().Replace("${arch}","64");string relative=lib["downloads"]?["classifiers"]?[classifier]?["path"]?.ToString()??MavenPath(name+":"+classifier);AddFile(Versions.Within(libroot,relative),true);}
  }
  if(v.Jar!="")classpath.Add(v.Jar);string assets=Path.Combine(root,"assets");if(v.Assets!=""&&!File.Exists(Versions.Within(assets,"indexes/"+v.Assets+".json")))plan.Missing.Add("Индекс ресурсов: "+v.Assets);
  if(session!=null){if(session.Expires<=DateTimeOffset.UtcNow)throw new IOException("Microsoft session expired. Sign in again.");plan.SensitiveValues.Add(session.AccessToken);}
  var values=new Dictionary<string,string>{{"auth_player_name",session?.Name??s.Nickname},{"version_name",id},{"game_directory",game},{"assets_root",assets},{"assets_index_name",v.Assets},{"auth_uuid",session?.Uuid??OfflineUuid(s.Nickname)},{"auth_access_token",session?.AccessToken??"0"},{"auth_session",session==null?"0":"token:"+session.AccessToken+":"+session.Uuid},{"user_type",session==null?"legacy":"msa"},{"version_type",v.Type},{"user_properties","{}"},{"clientid",session==null?"":MicrosoftSignIn.ConfiguredClientId},{"auth_xuid",session?.Xuid??""},{"natives_directory",plan.Natives},{"launcher_name","Orbit"},{"launcher_version","0.3.0"},{"classpath",string.Join(Path.PathSeparator,classpath)},{"classpath_separator",Path.PathSeparator.ToString()},{"library_directory",libroot},{"resolution_width",s.GameWidth.ToString()},{"resolution_height",s.GameHeight.ToString()},{"game_assets",Path.Combine(assets,"virtual",v.Assets)}};
  string Expand(string input)=>Regex.Replace(input,@"\$\{([^}]+)\}",m=>values.TryGetValue(m.Groups[1].Value,out string? val)?val:throw new InvalidDataException("Неизвестный параметр версии: "+m.Value));
  plan.Arguments.Add("-Xms512M");plan.Arguments.Add("-Xmx"+Math.Clamp(s.MemoryMb,1024,32768)+"M");
  if(v.JvmArgs.Count>0)plan.Arguments.AddRange(ArgumentValues(v.JvmArgs).Select(Expand));else plan.Arguments.AddRange(new[]{"-Djava.library.path="+plan.Natives,"-Dminecraft.launcher.brand=Orbit","-cp",values["classpath"]});
  plan.Arguments.Add(v.MainClass);plan.Arguments.AddRange(ArgumentValues(v.GameArgs).Select(Expand));if(s.Fullscreen&&!plan.Arguments.Contains("--fullscreen"))plan.Arguments.Add("--fullscreen");return plan;
 }
 public static List<string> CheckAssets(LauncherSettings s,LaunchPlan plan){var missing=new List<string>();if(plan.Version.Assets=="")return missing;string assets=Path.Combine(s.GameRoot,"assets");string index=Versions.Within(assets,"indexes/"+plan.Version.Assets+".json");if(!File.Exists(index))return missing;var objects=JsonNode.Parse(File.ReadAllText(index))?["objects"]?.AsObject();if(objects==null)return missing;foreach(var item in objects){string hash=item.Value?["hash"]?.ToString()??"";if(!Regex.IsMatch(hash,"^[a-f0-9]{40}$"))throw new InvalidDataException("Неверный хеш ресурса.");if(!File.Exists(Versions.Within(assets,"objects/"+hash[..2]+"/"+hash)))missing.Add(item.Key);}return missing;}
 public static void ExtractNatives(LaunchPlan plan){Directory.CreateDirectory(plan.Natives);foreach(string file in plan.NativeArchives){using var zip=ZipFile.OpenRead(file);foreach(var entry in zip.Entries){if(entry.Name==""||entry.FullName.StartsWith("META-INF/",StringComparison.OrdinalIgnoreCase))continue;if(!entry.Name.EndsWith(".dll",StringComparison.OrdinalIgnoreCase))continue;string target=Versions.Within(plan.Natives,entry.FullName);Directory.CreateDirectory(Path.GetDirectoryName(target)!);entry.ExtractToFile(target,true);}}}
 public static string QuoteJavaArg(string arg)=>"\""+arg.Replace("\\","\\\\").Replace("\"","\\\"").Replace("\r","\\r").Replace("\n","\\n").Replace("\t","\\t")+"\"";
 public static Process Start(LaunchPlan plan)=>Process.Start(PrepareStart(plan))??throw new IOException("Java не запустилась.");
 public static ProcessStartInfo PrepareStart(LaunchPlan plan){if(plan.Missing.Count>0)throw new InvalidOperationException(plan.Summary);ExtractNatives(plan);Directory.CreateDirectory(plan.GameDirectory);var start=new ProcessStartInfo(plan.Java){WorkingDirectory=plan.GameDirectory,UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true};
  if(plan.Version.JavaMajor>=9){string args=Path.Combine(plan.Natives,"launch.args");var safe=new List<string>();var sensitive=new List<string>();for(int i=0;i<plan.Arguments.Count;i++){string value=plan.Arguments[i];if(plan.SensitiveValues.Any(secret=>value.Contains(secret,StringComparison.Ordinal))){if(safe.Count>0&&safe[^1].StartsWith("--",StringComparison.Ordinal)){sensitive.Add(safe[^1]);safe.RemoveAt(safe.Count-1);}sensitive.Add(value);}else safe.Add(value);}File.WriteAllLines(args,safe.Select(QuoteJavaArg),new UTF8Encoding(false));start.ArgumentList.Add("@"+args);foreach(string value in sensitive)start.ArgumentList.Add(value);}else {if(plan.Arguments.Sum(a=>a.Length+3)>30000)throw new InvalidOperationException("Слишком длинный путь для Java 8. Перенеси папку игры в более короткий путь.");foreach(string arg in plan.Arguments)start.ArgumentList.Add(arg);}
  return start;
 }
}

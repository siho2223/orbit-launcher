using System.Text.Json;
namespace Orbit.Core;
public sealed class LauncherSettings {
 public string GameRoot {get;set;}=DefaultRoot(); public string Nickname {get;set;}="Player";
 public string Version {get;set;}="1.21.1"; public string JavaPath {get;set;}="";
 public bool SkinEnabled {get;set;} public string SkinModel {get;set;}="classic";
 public int MemoryMb {get;set;}=4096; public bool ReduceMotion {get;set;} public bool Isolated {get;set;}=true;
 public string Language {get;set;}="en";
 public int GameWidth {get;set;}=1280; public int GameHeight {get;set;}=720;
 public bool Fullscreen {get;set;} public bool MinimizeOnLaunch {get;set;}
 public bool RememberWindow {get;set;}=true; public bool ScrollLog {get;set;}=true;
 public double WindowWidth {get;set;}=1320; public double WindowHeight {get;set;}=880;
 public string ActiveSkinPreset {get;set;}="";
 public static string DataRoot=>Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"OrbitLauncher");
 public static string DefaultRoot()=>Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),".minecraft");
 public static LauncherSettings Load(string? directory=null){try{return JsonSerializer.Deserialize<LauncherSettings>(File.ReadAllText(Path.Combine(directory??DataRoot,"settings.json")))??new();}catch{return new();}}
 public void Save(string? directory=null){MemoryMb=Math.Clamp(MemoryMb,1024,32768);Directory.CreateDirectory(directory??DataRoot);string p=Path.Combine(directory??DataRoot,"settings.json");File.WriteAllText(p+".tmp",JsonSerializer.Serialize(this,new JsonSerializerOptions{WriteIndented=true}));File.Move(p+".tmp",p,true);}
}

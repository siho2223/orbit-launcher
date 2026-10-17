using System.Text.Json;
using Orbit.Core;
static class InstanceChecks {
 public static int Run(string root){
  root=Path.Combine(Path.GetFullPath(root),Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);int count=0;void Check(bool ok,string msg){if(!ok)throw new Exception(msg);Console.WriteLine("PASS "+msg);count++;}
  foreach(string name in new[]{"Pack A","Pack B"}){var pack=Path.Combine(root,name);var dir=Path.Combine(pack,"versions","shared-profile");Directory.CreateDirectory(dir);File.WriteAllText(Path.Combine(dir,"shared-profile.json"),"{\"id\":\"shared-profile\",\"mainClass\":\"fixture.ForgeMain\",\"libraries\":[],\"javaVersion\":{\"majorVersion\":17}}");File.WriteAllText(Path.Combine(pack,".orbit-pack.json"),JsonSerializer.Serialize(new InstalledPack("shared-profile",name,"project","release","1.20.1")));}
  var found=Modpacks.Scan(root);Check(found.Count==2,"Same-profile modpacks scan as separate instances");Check(found.All(v=>v.BaseVersion=="1.20.1"),"Pack metadata supplies actual Minecraft version");Check(found.Select(v=>v.RootOverride).Distinct().Count()==2,"Pack roots remain distinct");foreach(var v in found)Check(LaunchEngine.InstanceDirectory(new LauncherSettings{GameRoot=v.RootOverride!,Isolated=true},v.Id)==v.RootOverride,"Pack files resolve to correct root: "+v.Title);
  Console.WriteLine($"PASS {count} instance checks");return 0;
 }
}

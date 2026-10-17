using System.IO.Compression;
using System.Text.Json;
using Orbit.Core;
static class ExportChecks {
 sealed class InlineProgress(Action<ExportProgress> callback):IProgress<ExportProgress>{public void Report(ExportProgress value)=>callback(value);}
 public static int Run(string root){
  root=Path.GetFullPath(root);Directory.CreateDirectory(root);var source=Path.Combine(root,"instance-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(source);
  int count=0;void Check(bool ok,string label){if(!ok)throw new Exception(label);count++;Console.WriteLine("PASS "+label);}
  void Put(string relative,string text){var p=Path.Combine(source,relative);Directory.CreateDirectory(Path.GetDirectoryName(p)!);File.WriteAllText(p,text);}
  var v=new VersionEntry("forge-1.20.1-47.2.0","Forge","1.20.1",17){DisplayName="Friend pack"};
  foreach(var path in new[]{"mods/example.jar","config/nested/example.toml","shaderpacks/example.zip","resourcepacks/example.zip","scripts/example.zs","kubejs/server_scripts/example.js","options.txt"})Put(path,"fixture "+path);
  foreach(var path in new[]{"accounts.json","launcher_profiles.json","logs/latest.log","screenshots/private.png","saves/MyWorld/level.dat","versions/test/test.jar"})Put(path,"must not share");
  var zip=Path.Combine(root,"pack-"+Guid.NewGuid().ToString("N")+".zip");var result=PackExporter.ExportAsync(source,zip,v).GetAwaiter().GetResult();Check(result.Files==7,"All requested content folders included");
  using(var z=ZipFile.OpenRead(zip)){
   Check(z.Entries.Count==9,"Only content, metadata and friend instructions included");
   foreach(var path in new[]{"mods/example.jar","config/nested/example.toml","shaderpacks/example.zip","resourcepacks/example.zip","scripts/example.zs","kubejs/server_scripts/example.js","options.txt"}){using var reader=new StreamReader(z.GetEntry("overrides/"+path)!.Open());Check(reader.ReadToEnd()=="fixture "+path,"Content preserved: "+path);}
   Check(!z.Entries.Any(e=>e.FullName.Contains("accounts")||e.FullName.Contains("saves/")||e.FullName.Contains("logs/")||e.FullName.Contains("versions/")),"Private and game files excluded");
   using var meta=new StreamReader(z.GetEntry("orbit-pack.json")!.Open());var doc=JsonDocument.Parse(meta.ReadToEnd());Check(doc.RootElement.GetProperty("profile").GetString()==v.Id&&doc.RootElement.GetProperty("minecraft").GetString()=="1.20.1","Exact version and loader profile recorded");
  }
  var original=File.ReadAllBytes(zip);using var cancelled=new CancellationTokenSource();cancelled.Cancel();try{PackExporter.ExportAsync(source,zip,v,null,cancelled.Token).GetAwaiter().GetResult();throw new Exception("Expected cancellation");}catch(OperationCanceledException){}Check(File.ReadAllBytes(zip).SequenceEqual(original),"Cancelled export preserves existing archive");
  try{PackExporter.ExportAsync(source,Path.Combine(source,"mods/out.zip"),v).GetAwaiter().GetResult();throw new Exception("Expected destination rejection");}catch(IOException){}Check(!File.Exists(Path.Combine(source,"mods/out.zip")),"Destination inside instance rejected");
  using(var mid=new CancellationTokenSource()){try{PackExporter.ExportAsync(source,zip,v,new InlineProgress(p=>{if(p.Files>=1)mid.Cancel();}),mid.Token).GetAwaiter().GetResult();throw new Exception("Expected active export cancellation");}catch(OperationCanceledException){}Check(File.ReadAllBytes(zip).SequenceEqual(original),"Cancellation during packing preserves existing archive");}
  var failSource=Path.Combine(source,"config/nested/example.toml");using(var locked=new FileStream(failSource,FileMode.Open,FileAccess.ReadWrite,FileShare.None)){try{PackExporter.ExportAsync(source,zip,v).GetAwaiter().GetResult();throw new Exception("Expected locked file failure");}catch(IOException){}}
  Check(File.ReadAllBytes(zip).SequenceEqual(original),"Read failure preserves existing archive");Check(!Directory.GetFiles(root,"*.partial-*").Any(),"Failure removes temporary archive");
  Console.WriteLine($"PASS {count} export checks");return 0;
 }
}

using System.Text.Json;
namespace Orbit.Core;
public sealed record SkinPreset(string Id,string Title,string Model);
public static class SkinPresets {
 static string Root=>Path.Combine(LauncherSettings.DataRoot,"skin-presets");
 public static IReadOnlyList<SkinPreset> List(string? directory=null){string root=directory??Root;if(!Directory.Exists(root))return Array.Empty<SkinPreset>();var result=new List<SkinPreset>();foreach(var file in Directory.GetFiles(root,"*.json")){try{var p=JsonSerializer.Deserialize<SkinPreset>(File.ReadAllText(file));if(p!=null&&p.Id==Path.GetFileNameWithoutExtension(file)&&File.Exists(Image(p.Id,directory)))result.Add(p);}catch(IOException){}catch(JsonException){}catch(InvalidDataException){}}return result.OrderBy(x=>x.Title).ToArray();}
 public static string Image(string id,string? directory=null){if(!Guid.TryParseExact(id,"N",out _))throw new InvalidDataException("Invalid preset ID.");return Path.Combine(directory??Root,id+".png");}
 public static SkinPreset Save(string title,string model,byte[] png,string? directory=null){string root=directory??Root;if(string.IsNullOrWhiteSpace(title)||title.Length>60)throw new InvalidDataException("Enter a name (1–60 characters).");if(model is not("classic" or "slim"))throw new InvalidDataException("Invalid skin model.");if(png.Length>4*1024*1024)throw new InvalidDataException("Skin is too large.");LocalSkins.Validate(png);Directory.CreateDirectory(root);var preset=new SkinPreset(Guid.NewGuid().ToString("N"),title.Trim(),model);File.WriteAllBytes(Image(preset.Id,directory),png);File.WriteAllText(Path.Combine(root,preset.Id+".json"),JsonSerializer.Serialize(preset));return preset;}
 public static void Delete(string id,string? directory=null){File.Delete(Image(id,directory));File.Delete(Path.ChangeExtension(Image(id,directory),".json"));}
}

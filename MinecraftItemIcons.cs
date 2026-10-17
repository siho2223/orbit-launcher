using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
namespace OrbitLauncher;
// Read original icons from an installed client, without distributing or modifying game files.
internal static class MinecraftItemIcons {
 internal static readonly IReadOnlyDictionary<string,string> Items=new Dictionary<string,string>{
  ["packs"]="diamond_pickaxe",["skins"]="armor_stand",["worlds"]="pack",

  ["wave"]="totem_of_undying",["dance"]="music_disc_cat",["cheer"]="firework_rocket"
 };
 static string cachedArchive="";static DateTime cachedWrite;static IReadOnlyDictionary<string,BitmapSource> cached=new Dictionary<string,BitmapSource>();
 internal static string SourceArchive=>cachedArchive;
 // The resource-pack thumbnail is cropped. Use the complete window icon instead.
 static BitmapSource? LoadWorldIcon(string root){
  try{
   string assets=Path.Combine(root,"assets"),indexes=Path.Combine(assets,"indexes");if(!Directory.Exists(indexes))return null;
   foreach(string file in Directory.EnumerateFiles(indexes,"*.json").OrderByDescending(File.GetLastWriteTimeUtc)){
    try{
     if(new FileInfo(file).Length>32*1024*1024)continue;
     using var stream=File.OpenRead(file);using var json=JsonDocument.Parse(stream);if(!json.RootElement.TryGetProperty("objects",out var objects))continue;
     foreach(string name in new[]{"icons/icon_128x128.png","icons/icon_32x32.png","minecraft/icons/icon_32x32.png"}){
      if(!objects.TryGetProperty(name,out var entry)||!entry.TryGetProperty("hash",out var hashValue))continue;
      string hash=hashValue.GetString()??"";if(!Regex.IsMatch(hash,"^[a-f0-9]{40}$"))continue;
      string path=Path.Combine(assets,"objects",hash[..2],hash);if(!File.Exists(path)||new FileInfo(path).Length>1024*1024)continue;
      using var imageStream=File.OpenRead(path);var bitmap=new PngBitmapDecoder(imageStream,BitmapCreateOptions.PreservePixelFormat,BitmapCacheOption.OnLoad).Frames[0];if(bitmap.PixelWidth>256||bitmap.PixelHeight>256)continue;bitmap.Freeze();return bitmap;
     }
    }catch(Exception e)when(e is IOException or JsonException or InvalidDataException or NotSupportedException){}
   }
  }catch(Exception e)when(e is IOException or UnauthorizedAccessException){}
  return null;
 }
 internal static IReadOnlyDictionary<string,BitmapSource> Load(string root,string preferred){
  try{
   string versions=Path.Combine(root,"versions");if(!Directory.Exists(versions))return new Dictionary<string,BitmapSource>();
   var candidates=Directory.EnumerateDirectories(versions).Select(dir=>Path.Combine(dir,Path.GetFileName(dir)+".jar")).Where(File.Exists)
    .OrderByDescending(path=>Path.GetFileNameWithoutExtension(path)==preferred).ThenByDescending(File.GetLastWriteTimeUtc);
   foreach(string path in candidates){
    DateTime write=File.GetLastWriteTimeUtc(path);if(path==cachedArchive&&write==cachedWrite)return cached;
    try{
     using var archive=ZipFile.OpenRead(path);var images=new Dictionary<string,BitmapSource>();
     foreach(var item in Items){
      string[] names=item.Value=="pack"?new[]{"pack.png"}:new[]{"assets/minecraft/textures/item/"+item.Value+".png","assets/minecraft/textures/items/"+item.Value+".png"};
      var entry=names.Select(archive.GetEntry).FirstOrDefault(e=>e!=null);if(entry==null||entry.Length>1024*1024)continue;
      using var stream=entry.Open();using var data=new MemoryStream();stream.CopyTo(data);data.Position=0;
      var bitmap=new PngBitmapDecoder(data,BitmapCreateOptions.PreservePixelFormat,BitmapCacheOption.OnLoad).Frames[0];
      if(bitmap.PixelWidth>256||bitmap.PixelHeight>256)continue;bitmap.Freeze();images[item.Key]=bitmap;
     }
     if(images.ContainsKey("packs")&&images.ContainsKey("worlds")){var fullBlock=LoadWorldIcon(root);if(fullBlock!=null)images["worlds"]=fullBlock;cachedArchive=path;cachedWrite=write;cached=images;return images;}
    }catch(Exception e)when(e is IOException or InvalidDataException or NotSupportedException or System.IO.FileFormatException){}
   }
  }catch(Exception e)when(e is IOException or UnauthorizedAccessException or ArgumentException){}
  return new Dictionary<string,BitmapSource>();
 }
}

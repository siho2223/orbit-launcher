using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Orbit.Core;
using SkiaSharp;
namespace OrbitLauncher;
public static class ModIcons {
 public static readonly DependencyProperty UrlProperty=DependencyProperty.RegisterAttached("Url",typeof(string),typeof(ModIcons),new PropertyMetadata("",Changed));
 public static string GetUrl(DependencyObject target)=>(string)target.GetValue(UrlProperty);
 public static void SetUrl(DependencyObject target,string value)=>target.SetValue(UrlProperty,value);
 static readonly HttpClient http=new(new HttpClientHandler{AllowAutoRedirect=false}){Timeout=TimeSpan.FromSeconds(15)};
 static readonly SemaphoreSlim slots=new(4);
 static readonly ConcurrentDictionary<string,Lazy<Task<BitmapSource?>>> images=new();
 static readonly string cache=Path.Combine(LauncherSettings.DataRoot,"cache","mod-icons");
 static ModIcons(){http.DefaultRequestHeaders.UserAgent.ParseAdd("ORBIT/0.4.1 (local Minecraft launcher)");}
 static async void Changed(DependencyObject target,DependencyPropertyChangedEventArgs e){if(target is not Image image)return;image.Source=null;if(e.NewValue is not string url||url.Length==0)return;try{var bitmap=await LoadAsync(url);if(GetUrl(image)==url)image.Source=bitmap;}catch{/* Keep the fallback if this individual image is unavailable. */}}
 public static async Task<BitmapSource?> LoadAsync(string url){if(images.Count>256)images.Clear();var result=await images.GetOrAdd(url,key=>new Lazy<Task<BitmapSource?>>(()=>Task.Run(()=>FetchAsync(key)))).Value;if(result==null)images.TryRemove(url,out _);return result;}
 public static async Task WaitForPendingAsync(){await Task.WhenAll(images.Values.Where(v=>v.IsValueCreated).Select(v=>v.Value));}
 static BitmapSource DecodePng(byte[] bytes){using var stream=new MemoryStream(bytes);var image=new BitmapImage();image.BeginInit();image.CacheOption=BitmapCacheOption.OnLoad;image.DecodePixelWidth=96;image.StreamSource=stream;image.EndInit();image.Freeze();return image;}
 static async Task<BitmapSource?> FetchAsync(string url){await slots.WaitAsync().ConfigureAwait(false);try{var uri=ModrinthClient.DownloadUri(url);string key=Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(url)));string path=Path.Combine(cache,key+".png");
  if(File.Exists(path)){try{return DecodePng(await File.ReadAllBytesAsync(path).ConfigureAwait(false));}catch{File.Delete(path);}}
  using var timeout=new CancellationTokenSource(TimeSpan.FromSeconds(15));var ct=timeout.Token;using var response=await http.GetAsync(uri,HttpCompletionOption.ResponseHeadersRead,ct).ConfigureAwait(false);response.EnsureSuccessStatusCode();using var input=await response.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);using var raw=new MemoryStream();byte[] buffer=new byte[16384];int n;while((n=await input.ReadAsync(buffer,ct).ConfigureAwait(false))>0){if(raw.Length+n>5*1024*1024)throw new InvalidDataException("Icon too large");raw.Write(buffer,0,n);}
  using var data=SKData.CreateCopy(raw.ToArray());using var codec=SKCodec.Create(data)??throw new InvalidDataException("Unsupported icon");if(codec.Info.Width<=0||codec.Info.Height<=0||codec.Info.Width>4096||codec.Info.Height>4096)throw new InvalidDataException("Icon dimensions too large");using var original=SKBitmap.Decode(codec)??throw new InvalidDataException("Icon decode failed");
  int w=Math.Max(1,Math.Min(96,original.Width)),h=Math.Max(1,(int)Math.Round(original.Height*(double)w/original.Width));if(h>96){h=96;w=Math.Max(1,(int)Math.Round(original.Width*96d/original.Height));}
  using var scaled=new SKBitmap(w,h);using(var canvas=new SKCanvas(scaled)){canvas.Clear(SKColors.Transparent);canvas.DrawBitmap(original,new SKRect(0,0,w,h),new SKSamplingOptions(SKFilterMode.Linear));}
  using var thumbnail=SKImage.FromBitmap(scaled);using var encoded=thumbnail.Encode(SKEncodedImageFormat.Png,100);byte[] png=encoded.ToArray();var result=DecodePng(png);
  try{Directory.CreateDirectory(cache);await File.WriteAllBytesAsync(path+".tmp",png).ConfigureAwait(false);File.Move(path+".tmp",path,true);TrimCache();}catch(IOException){}catch(UnauthorizedAccessException){}return result;
 }catch{return null;}finally{slots.Release();}}
 static void TrimCache(){var files=new DirectoryInfo(cache).GetFiles("*.png").OrderByDescending(f=>f.LastWriteTimeUtc);long size=0;foreach(var file in files){size+=file.Length;if(size>50L*1024*1024)try{file.Delete();}catch(IOException){}}}
}

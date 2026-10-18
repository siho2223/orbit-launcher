using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using Orbit.Core;
namespace OrbitLauncher;
public partial class MainWindow {
 WebView2? brawlBrowser;bool brawlInitializing,brawlWebReady;string lastBrawlState="",brawlSkinSignature="",brawlSkinData="";readonly DispatcherTimer brawlSyncTimer=new(){Interval=TimeSpan.FromMilliseconds(400)};Task brawlInitTask=Task.CompletedTask;readonly TaskCompletionSource brawlPageReady=new(TaskCreationOptions.RunContinuationsAsynchronously);
 static readonly HashSet<string> BrawlAssets=new(StringComparer.Ordinal){"index.html","app.js","app.css","app.js.LEGAL.txt","fonts/Geist-Variable.woff2","fonts/Unbounded-latin.woff2"};
 void InitializeMillida(){settings.AccountKind="offline";brawlSyncTimer.Tick+=(s,e)=>SyncBrawlState();brawlSyncTimer.Start();Closed+=(s,e)=>{quickLoaderCts?.Cancel();brawlSyncTimer.Stop();microsoftCts?.Cancel();brawlBrowser?.Dispose();};}
 void UpdateMillidaVisibility(){if(BrawlWebHost==null)return;bool enabled=settings.Theme=="brawl stars";BrawlWebHost.Visibility=enabled?Visibility.Visible:Visibility.Collapsed;if(enabled&&!brawlInitializing&&brawlBrowser==null){brawlInitTask=EnsureBrawlWebAsync();}SyncBrawlState();}
 async Task EnsureBrawlWebAsync(){
  if(brawlInitializing||brawlBrowser!=null)return;brawlInitializing=true;
  try{
   var folder=suppressProfileSave?Path.Combine(Path.GetTempPath(),"ORBIT-preview-webview-"+Environment.ProcessId):Path.Combine(LauncherSettings.DataRoot,"webview");
   var environment=await CoreWebView2Environment.CreateAsync(null,folder,suppressProfileSave?new CoreWebView2EnvironmentOptions("--disable-backgrounding-occluded-windows --disable-renderer-backgrounding --disable-features=CalculateNativeWinOcclusion"):null);
   brawlBrowser=new WebView2{DefaultBackgroundColor=System.Drawing.Color.FromArgb(255,35,29,47)};BrawlWebHost.Children.Add(brawlBrowser);await brawlBrowser.EnsureCoreWebView2Async(environment);
   var core=brawlBrowser.CoreWebView2;var options=core.Settings;options.AreHostObjectsAllowed=false;options.AreDefaultContextMenusEnabled=false;options.IsStatusBarEnabled=false;options.AreDevToolsEnabled=false;options.IsPasswordAutosaveEnabled=false;options.IsGeneralAutofillEnabled=false;options.AreBrowserAcceleratorKeysEnabled=false;
   core.NavigationStarting+=(s,e)=>{if(!IsBrawlOrigin(e.Uri))e.Cancel=true;};core.NewWindowRequested+=(s,e)=>e.Handled=true;core.PermissionRequested+=(s,e)=>e.State=CoreWebView2PermissionState.Deny;core.DownloadStarting+=(s,e)=>e.Cancel=true;
   core.AddWebResourceRequestedFilter("*",CoreWebView2WebResourceContext.All);
   core.WebResourceRequested+=(s,e)=>{if(Uri.TryCreate(e.Request.Uri,UriKind.Absolute,out var uri)&&uri.Host=="orbit-ui.local"&&uri.Scheme=="https"){
    string name=Uri.UnescapeDataString(uri.AbsolutePath).TrimStart('/');if(name=="")name="index.html";
    if(BrawlAssets.Contains(name)){var resource=Application.GetResourceStream(new Uri("/BrawlUI/dist/"+name,UriKind.Relative));if(resource!=null){var bytes=new MemoryStream();resource.Stream.CopyTo(bytes);resource.Stream.Dispose();bytes.Position=0;string type=name.EndsWith(".js")?"application/javascript":name.EndsWith(".css")?"text/css":name.EndsWith(".woff2")?"font/woff2":"text/html";e.Response=environment.CreateWebResourceResponse(bytes,200,"OK","Content-Type: "+type+"; charset=utf-8\r\nX-Content-Type-Options: nosniff");return;}}
    e.Response=environment.CreateWebResourceResponse(null,404,"Not Found","");
   }else if(e.ResourceContext!=CoreWebView2WebResourceContext.Image||!Uri.TryCreate(e.Request.Uri,UriKind.Absolute,out var image)||image.Scheme!="https"||image.Host!="cdn.modrinth.com"){e.Response=environment.CreateWebResourceResponse(null,403,"Forbidden","");}};
   core.WebMessageReceived+=BrawlMessage;core.NavigationCompleted+=(s,e)=>{if(!e.IsSuccess)Status("Brawl UI could not load: "+e.WebErrorStatus,true);};core.Navigate("https://orbit-ui.local/index.html");
  }catch(Exception error){Status(Loc.Language=="ru"?"Не удалось открыть новый интерфейс Brawl. Проверь наличие Microsoft Edge WebView2 Runtime.":"Brawl UI could not start. Microsoft Edge WebView2 Runtime is required.",true);Log("Brawl UI: "+error.GetType().Name);BrawlWebHost.Visibility=Visibility.Collapsed;brawlBrowser?.Dispose();brawlBrowser=null;}
  finally{brawlInitializing=false;}
 }
 static bool IsBrawlOrigin(string value)=>Uri.TryCreate(value,UriKind.Absolute,out var uri)&&uri.Scheme=="https"&&uri.Host=="orbit-ui.local"&&uri.IsDefaultPort&&uri.UserInfo=="";
 static string PngData(BitmapSource image){var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(image));using var stream=new MemoryStream();encoder.Save(stream);return "data:image/png;base64,"+Convert.ToBase64String(stream.ToArray());}
 string BrawlIcon(string name){if(TryFindResource("MinecraftIcon_"+name) is BitmapSource image)return PngData(image);return "";}
 string BrawlSkin(){string signature=(pendingSkin!=null?Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(pendingSkin)):File.Exists(LocalSkins.SkinPath)?File.GetLastWriteTimeUtc(LocalSkins.SkinPath).Ticks.ToString():"fallback")+skinModel;if(signature!=brawlSkinSignature){brawlSkinSignature=signature;try{brawlSkinData=pendingSkin!=null?PngData(DecodeSkin(pendingSkin)):File.Exists(LocalSkins.SkinPath)?PngData(DecodeSkin(File.ReadAllBytes(LocalSkins.SkinPath))):PngData(LobbyAvatar());}catch{brawlSkinData=PngData(LobbyAvatar());}}return brawlSkinData;}
 void SyncBrawlState(){
  if(!brawlWebReady||brawlBrowser?.CoreWebView2==null)return;
  try{
   var entries=versions.Select((v,index)=>new{key=index.ToString(),title=v.Title,loader=v.Loader,java=v.JavaMajor,kind=v.VisualKind,color=v.TileColor,foreground=v.TileForeground,selected=SelectedChoice!=null&&InstanceSelectionConverter.Same(v,SelectedChoice)}).ToArray();
   var catalogVersions=choices.Select((v,index)=>new{key=index.ToString(),id=v.Id,title=v.Title,installed=v.Installed,loader=v.Loader,kind=v.VisualKind,color=v.TileColor,foreground=v.TileForeground,snapshot=v.Remote?.Type=="snapshot"}).ToArray();
   var loaderCatalogVersions=quickLoaderVersions.ToDictionary(pair=>pair.Key,pair=>VersionCatalog.ForLoader(remoteVersions,versions,pair.Value,pair.Key).Select(v=>new{key=v.Installed?choices.FindIndex(c=>InstanceSelectionConverter.Same(c,v)).ToString():"",id=v.Id,title=v.Title,minecraft=v.BaseVersion,installed=v.Installed,loader=v.Loader,kind=v.VisualKind,color=v.TileColor,foreground=v.TileForeground}).ToArray());
   var projects=ModsResults.Items.OfType<ModProject>().Select(p=>new{id=p.Id,title=p.Title,description=p.Description,author=p.Author,icon=p.IconUrl}).ToArray();
   var worlds=Directory.Exists(filesFolder)&&page=="Worlds"?Directory.GetDirectories(filesFolder).Select(Path.GetFileName).ToArray():Array.Empty<string>();
   var presets=SkinPresets.List().Select(p=>new{id=p.Id,title=p.Title,face=PresetFace(p)}).ToArray();
   var payload=new{page,pageTitle=PageTitle.Text,theme=settings.Theme,palette=settings.ThemePalette,language=Loc.Language,reduce=settings.ReduceMotion,paused=settings.Theme!="brawl stars"||WindowState==WindowState.Minimized||game is {HasExited:false},nickname=NicknameBox.Text,offlineNickname=settings.OfflineNickname,skin=BrawlSkin(),selectedKind=SelectedChoice?.VisualKind??"available",selectedBase=SelectedChoice?.BaseVersion??"1.21.1",quickCatalogLoader,quickCatalogLoading,quickCatalogError,quickVersions=quickLoaderVersions,quickStatus=quickLoaderStatus,quickBusy=quickLoaderCts!=null,quickIndeterminate=quickLoaderIndeterminate,slim=skinModel=="slim",face=LobbyFace.Source is BitmapSource face?PngData(face):"",icons=new{packs=BrawlIcon("packs"),skins=BrawlIcon("skins"),worlds=BrawlIcon("worlds"),wave=BrawlIcon("wave"),dance=BrawlIcon("dance"),cheer=BrawlIcon("cheer"),forge=PngData(LoaderArtwork.Forge),fabric=PngData(LoaderArtwork.Fabric)},versions=entries,catalogVersions,loaderCatalogVersions,selectedTitle=SelectedChoice?.Title??"",play=PlayButton.Content?.ToString()??"PLAY",canPlay=PlayButton.IsEnabled,status=StatusText.Text,busy=installing||modBusy,progress=DownloadBar.Value,exporting=exportCts!=null,exportStatus=ExportStatus.Text,exportProgress=ExportBar.Value,projects,catalog=catalogType,modQuery=ModsQuery.Text,modsStatus=ModsStatus.Text,canInstall=ModCanInstall,previous=modsOffset>0,next=modsOffset+20<modsTotal,pages=$"{modsOffset+1}–{Math.Min(modsOffset+20,modsTotal)} / {modsTotal}",root=RootBox.Text,java=JavaBox.Text,memory=(int)RamSlider.Value,gameWidth=GameWidthBox.Text,gameHeight=GameHeightBox.Text,fullscreen=FullscreenBox.IsChecked==true,minimize=MinimizeGameBox.IsChecked==true,remember=RememberWindowBox.IsChecked==true,scrollLog=ScrollLogBox.IsChecked==true,isolated=IsolatedBox.IsChecked==true,log=LogBox.Text.Length>18000?LogBox.Text[^18000..]:LogBox.Text,presets,worlds};
   string json=JsonSerializer.Serialize(payload);if(json==lastBrawlState)return;lastBrawlState=json;brawlBrowser.CoreWebView2.PostWebMessageAsJson(json);
  }catch(Exception error)when(error is IOException or InvalidOperationException or System.Runtime.InteropServices.COMException){Log("Brawl state: "+error.GetType().Name);}
 }
 static string PresetFace(SkinPreset preset){try{var png=new BitmapImage();using var stream=File.OpenRead(SkinPresets.Image(preset.Id));png.BeginInit();png.CacheOption=BitmapCacheOption.OnLoad;png.StreamSource=stream;png.EndInit();return PngData(new CroppedBitmap(png,new Int32Rect(8,8,8,8)));}catch{return "";}}
 async void BrawlMessage(object? sender,CoreWebView2WebMessageReceivedEventArgs e){if(!IsBrawlOrigin(e.Source)||e.WebMessageAsJson.Length>16000)return;try{using var doc=JsonDocument.Parse(e.WebMessageAsJson);await HandleBrawlAction(doc.RootElement);}catch(Exception error){Status(error is JsonException?"Invalid UI message.":error.Message,true);}SyncBrawlState();}
 async Task HandleBrawlAction(JsonElement message){
  string Str(string key)=>message.TryGetProperty(key,out var value)&&value.ValueKind==JsonValueKind.String?value.GetString()??"":"";string action=Str("action");var args=new RoutedEventArgs();VersionEntry? Target(){return int.TryParse(Str("key"),out int index)&&index>=0&&index<choices.Count?choices[index]:null;}
  switch(action){
   case "ready":brawlWebReady=true;UpdateLobbyIdle();brawlPageReady.TrySetResult();lastBrawlState="";break;
   case "navigate":string target=Str("page");if(target is "Home" or "Library" or "Mods" or "Skins" or "Settings" or "Logs" or "Worlds")Navigate(target);break;
   case "select":if(Target() is {} selected){VersionCard_Click(new Button{Tag=selected},args);}break;
   case "folder":if(Target() is {} folder)InstanceFolder_Click(new Button{Tag=folder},args);break;
   case "export":if(Target() is {} export)ExportPack_Click(new Button{Tag=export},args);break;
   case "exportCancel":ExportCancel_Click(this,args);break;
   case "play":Play_Click(this,args);break;
   case "versions":await LoadCatalogAsync();break;case "refresh":await RefreshVersions();break;
   case "catalog":string type=Str("type");if(type is "mod" or "modpack" or "resourcepack" or "shader"){if(type==catalogType)Navigate("Mods");else{Navigate("Mods");CatalogType_Click(new Button{Tag=type},args);}}break;
   case "search":if(Str("query").Length<=200)ModsQuery.Text=Str("query");break;
   case "install":if(Str("type")==catalogType&&ModsResults.Items.OfType<ModProject>().FirstOrDefault(p=>p.Id==Str("id")) is {} project)ModInstall_Click(new Button{Tag=project},args);break;
   case "previous":if(modsOffset>0){modsOffset=Math.Max(0,modsOffset-20);await SearchModsAsync(false);}break;
   case "next":if(modsOffset+20<modsTotal){modsOffset+=20;await SearchModsAsync(false);}break;
   case "retry":await LoadModsPageAsync();break;
   case "cancel":ModsCancel_Click(this,args);break;
   case "theme":string theme=Str("value");if(theme is "default" or "brawl stars"){settings.Theme=theme;ApplyTheme();if(!suppressProfileSave)settings.Save();}break;
   case "palette":string palette=Str("value");if(palette is "purple" or "green" or "orange" or "blue" or "pink"){settings.ThemePalette=palette;ApplyTheme();if(!suppressProfileSave)settings.Save();}break;
   case "language":string language=Str("value");if(language is "en" or "ru"){Loc.Language=language;Loc.ApplyResources();UpdateThemeLabels();settings.Language=language;if(!suppressProfileSave)settings.Save();Navigate(page);}break;
   case "setting":string name=Str("name");if(message.TryGetProperty("value",out var value)){if(name=="memory"&&value.TryGetInt32(out int memory))RamSlider.Value=Math.Clamp(memory,1024,32768);if(name=="gameWidth"&&value.TryGetInt32(out int width))GameWidthBox.Text=width.ToString();if(name=="gameHeight"&&value.TryGetInt32(out int height))GameHeightBox.Text=height.ToString();if(value.ValueKind is JsonValueKind.True or JsonValueKind.False){if(name=="reduce"){MotionBox.IsChecked=value.GetBoolean();settings.ReduceMotion=value.GetBoolean();ApplyMotionPreference();}if(name=="isolated")IsolatedBox.IsChecked=value.GetBoolean();if(name=="fullscreen")FullscreenBox.IsChecked=value.GetBoolean();if(name=="minimize")MinimizeGameBox.IsChecked=value.GetBoolean();if(name=="remember")RememberWindowBox.IsChecked=value.GetBoolean();if(name=="scrollLog")ScrollLogBox.IsChecked=value.GetBoolean();}}break;
   case "browseRoot":BrowseRoot_Click(this,args);break;case "browseJava":BrowseJava_Click(this,args);break;case "saveSettings":SaveSettings_Click(this,args);break;
   case "skinModel":if(Str("value") is "classic" or "slim")SkinModel_Click(new Button{Tag=Str("value")},args);break;case "disableSkin":ResetSkin_Click(this,args);break;case "chooseSkin":ChooseSkin_Click(this,args);break;case "saveSkin":ApplySkin_Click(this,args);break;
   case "savePreset":if(!suppressProfileSave){PresetNameBox.Text=Str("name");SavePreset_Click(this,args);}break;
   case "preset":var preset=SkinPresets.List().FirstOrDefault(p=>p.Id==Str("id"));if(preset!=null){PresetCard_Click(new Button{Tag=preset},args);}break;
   case "verify":Verify_Click(this,args);break;case "logFolder":LogFolder_Click(this,args);break;case "worldFolder":FilesFolder_Click(this,args);break;
   case "prepareQuickLoader":await PrepareQuickLoaderAsync(Str("loader"));break;case "quickLoader":await QuickInstallLoaderAsync(Str("loader"),Str("minecraft"));break;case "cancelLoader":quickLoaderCts?.Cancel();break;case "cancelDownload":CancelDownload_Click(this,args);break;case "authCancel":microsoftCts?.Cancel();break;case "offline":UseOfflineProfile(Str("nickname"));break;case "signout":UseOfflineProfile(settings.OfflineNickname);break;
  }
 }
 internal async Task<bool> CaptureMillidaPreview(string output){if(settings.Theme!="brawl stars"||BrawlWebHost.Visibility!=Visibility.Visible)return false;await brawlInitTask;if(brawlBrowser?.CoreWebView2==null)return false;await brawlPageReady.Task.WaitAsync(TimeSpan.FromSeconds(30));SyncBrawlState();await Task.Delay(1800);File.WriteAllText(output+".dom.json",await brawlBrowser.CoreWebView2.ExecuteScriptAsync("JSON.stringify({ready:window.orbitReady,errors:window.orbitErrors,text:document.body.innerText,html:document.documentElement.outerHTML.slice(0,1200)})"));Directory.CreateDirectory(Path.GetDirectoryName(output)!);using var stream=File.Create(output);await brawlBrowser.CoreWebView2.CapturePreviewAsync(CoreWebView2CapturePreviewImageFormat.Png,stream);return true;}
}

// Adapted from XMCL AppSideBarClassic/HomeHeader/UserProfileDialog, MIT. See ThirdParty/XMCL-MIT.txt.
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Orbit.Core;
namespace OrbitLauncher;
public partial class MainWindow {
 string xmclCachedRoot="";BitmapSource? xmclCachedPanorama;
 IInputElement? xmclPreviousFocus;string xmclPanoramaArchive="";DateTime xmclPanoramaWrite;
 void XmclSurface_SizeChanged(object sender,SizeChangedEventArgs e){if(sender is Border border)border.Clip=new RectangleGeometry(new Rect(0,0,border.ActualWidth,border.ActualHeight),border.CornerRadius.TopLeft,border.CornerRadius.TopLeft);}
 void InitializeXmcl(){PreviewKeyDown+=(s,e)=>{if(e.Key==Key.Escape&&XmclProfileOverlay.IsVisible){CloseXmclProfile();e.Handled=true;}else if(e.Key==Key.K&&Keyboard.Modifiers==ModifierKeys.Control){CloseXmclProfile();Navigate("Library");SearchBox.Focus();e.Handled=true;}};}
 static string ReleaseVersion=>typeof(MainWindow).Assembly.GetName().Version?.ToString(3)??"preview";
 void UpdateXmclLabels(){var r=Application.Current.Resources;bool ru=Loc.Language=="ru";r["XmclProfileLabel"]=ru?"Профиль":"Profile";r["XmclLocalProfile"]=ru?"Локальный профиль":"Offline profile";r["XmclSelectedLabel"]=ru?"ВЫБРАННАЯ СБОРКА":"SELECTED INSTANCE";r["XmclAuthLabel"]=ru?"Способ входа":"Authentication";r["XmclDone"]=ru?"Готово":"Done";r["XmclUpdateTitle"]=ru?$"Обновление ORBIT {ReleaseVersion}":$"ORBIT {ReleaseVersion} update";r["XmclUpdateDetail"]=ru?"Новая тема Default, быстрая установка Forge и Fabric, понятный список версий и экспорт сборок в ZIP.":"New Default theme, quick Forge and Fabric installation, clearer version lists and ZIP instance export.";}
 void ApplyXmclLayout(){
  bool xmcl=settings.Theme!="brawl stars";XmclBackdrop.Visibility=xmcl?Visibility.Visible:Visibility.Collapsed;
  HomeVersions.ItemTemplate=(DataTemplate)FindResource(xmcl?"XmclInstanceCard":"VersionCard");AllVersions.ItemTemplate=HomeVersions.ItemTemplate;
  HomeVersions.ItemsPanel=(ItemsPanelTemplate)FindResource(xmcl?"XmclGrid":"OrbitWrap");AllVersions.ItemsPanel=HomeVersions.ItemsPanel;
  if(!xmcl){CloseXmclProfile();return;}
  SidebarColumn.Width=new GridLength(288);PageHost.Margin=new Thickness(24,18,24,12);PageHeading.Visibility=page=="Home"?Visibility.Collapsed:Visibility.Visible;PageHost.RowDefinitions[0].Height=new GridLength(page=="Home"?0:62);SettingsContent.HorizontalAlignment=HorizontalAlignment.Stretch;
  LaunchRow.Height=new GridLength(108);LaunchBar.Padding=new Thickness(24,10,24,12);LaunchNicknameField.Visibility=Visibility.Collapsed;
  if(LaunchNicknameField.Parent is Grid launch){launch.ColumnDefinitions[0].Width=new GridLength(0);launch.ColumnDefinitions[1].Width=new GridLength(0);launch.ColumnDefinitions[4].Width=new GridLength(208);}
  WindowBrand.Text="ORBIT";WindowBrand.FontSize=11;RefreshSkin();RefreshXmclPanorama();
 }
 void RefreshXmclPanorama(){
  try{string currentRoot=Path.GetFullPath(settings.GameRoot);if(xmclCachedRoot==currentRoot&&xmclCachedPanorama!=null){XmclBackgroundImage.Source=xmclCachedPanorama;return;}
   string jar=MinecraftItemIcons.SourceArchive;string expected=Path.GetFullPath(Path.Combine(settings.GameRoot,"versions"))+Path.DirectorySeparatorChar;
   if(string.IsNullOrEmpty(jar)||!jar.StartsWith(expected,StringComparison.OrdinalIgnoreCase)||!File.Exists(jar)){SetXmclPanorama(LoadXmclPanoramaAsset());return;}
   var written=File.GetLastWriteTimeUtc(jar);if(jar==xmclPanoramaArchive&&written==xmclPanoramaWrite)return;
   using var archive=ZipFile.OpenRead(jar);var entry=archive.GetEntry("assets/minecraft/textures/gui/title/background/panorama_0.png");
   XmclBackgroundImage.Source=null;if(entry==null||entry.Length>4*1024*1024){SetXmclPanorama(LoadXmclPanoramaAsset());return;}
   using var stream=entry.Open();var image=new PngBitmapDecoder(stream,BitmapCreateOptions.PreservePixelFormat,BitmapCacheOption.OnLoad).Frames[0];if(image.PixelWidth>2048||image.PixelHeight>2048)return;image.Freeze();SetXmclPanorama(image);xmclPanoramaArchive=jar;xmclPanoramaWrite=written;
  }catch(Exception e)when(e is IOException or InvalidDataException or NotSupportedException or UnauthorizedAccessException or System.IO.FileFormatException){XmclBackgroundImage.Source=null;}
 }
 void SetXmclPanorama(BitmapSource? image){XmclBackgroundImage.Source=image;xmclCachedRoot=Path.GetFullPath(settings.GameRoot);xmclCachedPanorama=image;}
 BitmapSource? LoadXmclPanoramaAsset(){
  string assets=Path.Combine(settings.GameRoot,"assets"),indexes=Path.Combine(assets,"indexes");if(!Directory.Exists(indexes))return null;
  foreach(var index in Directory.EnumerateFiles(indexes,"*.json").OrderByDescending(File.GetLastWriteTimeUtc)){
   try{if(new FileInfo(index).Length>32*1024*1024)continue;using var json=JsonDocument.Parse(File.ReadAllText(index));if(!json.RootElement.TryGetProperty("objects",out var objects)||!objects.TryGetProperty("minecraft/textures/gui/title/background/panorama_0.png",out var item))continue;string hash=item.GetProperty("hash").GetString()??"";if(!Regex.IsMatch(hash,"^[a-f0-9]{40}$"))continue;string path=Path.Combine(assets,"objects",hash[..2],hash);if(!File.Exists(path)||new FileInfo(path).Length>4*1024*1024)continue;using var stream=File.OpenRead(path);var image=new PngBitmapDecoder(stream,BitmapCreateOptions.PreservePixelFormat,BitmapCacheOption.OnLoad).Frames[0];if(image.PixelWidth>2048||image.PixelHeight>2048)continue;image.Freeze();return image;}catch(Exception e)when(e is IOException or JsonException or InvalidDataException or NotSupportedException or System.IO.FileFormatException){}
  }return null;
 }
 internal void XmclInstance_Click(object sender,RoutedEventArgs e){VersionCard_Click(sender,e);Navigate("Home");}
 void XmclProfile_Click(object sender,RoutedEventArgs e){if(XmclProfileOverlay.IsVisible){CloseXmclProfile();return;}VersionPopup.IsOpen=false;xmclPreviousFocus=Keyboard.FocusedElement;XmclProfileError.Text="";XmclProfileOverlay.Visibility=Visibility.Visible;XmclProfileOverlay.BeginAnimation(OpacityProperty,null);XmclProfileOverlay.Opacity=1;if(!settings.ReduceMotion)XmclProfileOverlay.BeginAnimation(OpacityProperty,new System.Windows.Media.Animation.DoubleAnimation(.6,1,TimeSpan.FromMilliseconds(180)));App.FinishReveal(XmclProfileOverlay,null,200);XmclProfileNickname.Focus();XmclProfileNickname.SelectAll();}
 void CloseXmclProfile(){if(XmclProfileOverlay.Visibility!=Visibility.Visible)return;XmclProfileOverlay.Visibility=Visibility.Collapsed;if(xmclPreviousFocus is UIElement {IsVisible:true} focus)focus.Focus();xmclPreviousFocus=null;}
 void XmclProfileClose_Click(object sender,RoutedEventArgs e)=>CloseXmclProfile();
 void XmclProfileOutside_Click(object sender,MouseButtonEventArgs e){if(ReferenceEquals(sender,e.OriginalSource))CloseXmclProfile();}
 void XmclProfileSkins_Click(object sender,RoutedEventArgs e){CloseXmclProfile();Navigate("Skins");}
 void XmclProfileDone_Click(object sender,RoutedEventArgs e){if(!Regex.IsMatch(NicknameBox.Text.Trim(),"^[A-Za-z0-9_]{3,16}$")){XmclProfileError.Text=Loc.Language=="ru"?"Ник: 3–16 латинских букв, цифр или _.":"Use 3–16 letters, digits or _.";return;}if(!suppressProfileSave)PersistNickname();CloseXmclProfile();}
 void XmclProject_Click(object sender,RoutedEventArgs e){try{Process.Start(new ProcessStartInfo("https://github.com/siho2223/orbit-launcher"){UseShellExecute=true});}catch(Exception error){Status(error.Message,true);}}
}

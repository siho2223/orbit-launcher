using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Orbit.Core;
namespace OrbitLauncher;
public partial class MainWindow {
 async Task CheckAppearanceAsync(string root){
  var report=new List<string>();void Check(bool value,string label){if(!value)throw new Exception(label);report.Add("PASS "+label);}
  string oldTheme=settings.Theme,oldPalette=settings.ThemePalette;var oldChoice=selectedChoice;var originalClient=modrinth;
  try{
   foreach(string theme in new[]{"default","default 2","brawl stars"}){
    settings.Theme=theme;ApplyTheme();Navigate("Home");UpdateLayout();
    Check((theme=="default"?HomePage:theme=="default 2"?HomePage:(FrameworkElement)LobbyPage).Visibility==Visibility.Visible,"Home layout: "+theme);
    Check(theme=="brawl stars"?LobbyPlayButton.IsVisible&&LobbyVersionButton.IsVisible:PlayButton.IsVisible&&VersionPickerButton.IsVisible,"Game controls remain accessible: "+theme);
    if(theme=="brawl stars"){
     Check(LaunchBar.Visibility==Visibility.Collapsed&&LobbyLaunchPanel.ActualWidth<ActualWidth*.8,"Lobby launch controls form a compact centered panel");
     Check(ReferenceEquals(VersionPopup.PlacementTarget,LobbyVersionButton),"Version popup follows the visible lobby button");
     VersionPopup.IsOpen=true;await Task.Delay(150);Check(VersionPopup.Child.IsVisible,"Version picker opens from lobby with normal toolbar hidden");VersionPopup.IsOpen=false;
     LobbyPlayButton.ApplyTemplate();Check(LobbyPlayButton.Template.FindName("Frame",LobbyPlayButton) is PixelFrame,"Lobby buttons use the stepped inset frame");
    }
    Navigate("Settings");Check(theme=="brawl stars"?(CornerRadius)Application.Current.Resources["Radius14"]==new CornerRadius(3):(CornerRadius)Application.Current.Resources["Radius14"]==new CornerRadius(8)&&((SolidColorBrush)Application.Current.Resources["Brush00E5A0"]).Color==ThemeColor("#4CAF50"),"Page controls follow selected theme: "+theme);
Check(SettingsPage.IsVisible&&(theme=="brawl stars"?BrawlHeading.IsVisible&&ThemeBackdrop.IsVisible&&!SidebarShell.IsVisible:SidebarShell.IsVisible),"Can return to settings: "+theme);
   }
   settings.Theme="brawl stars";var accents=new HashSet<Color>();
   foreach(string palette in new[]{"green","purple","orange","blue","pink"}){settings.ThemePalette=palette;ApplyTheme();accents.Add(((SolidColorBrush)Application.Current.Resources["LobbyAccent"]).Color);Check(LobbyBackground.Palette==palette&&PalettePanel.Visibility==Visibility.Visible,"Lobby palette: "+palette);}
   Check(accents.Count==5,"Five distinct lobby palette accents");
   settings.Theme="default";ApplyTheme();Check(((SolidColorBrush)Application.Current.Resources["Brush00E5A0"]).Color==(Color)ColorConverter.ConvertFromString("#4CAF50"),"XMCL default restored without Brawl palette residue");
   string store=Path.Combine(root,"settings-fixture");new LauncherSettings{Theme="brawl stars",ThemePalette="orange"}.Save(store);var loaded=LauncherSettings.Load(store);Check(loaded.Theme=="brawl stars"&&loaded.ThemePalette=="orange","Theme selection survives settings round trip");
   Loc.Language="en";Loc.ApplyResources();UpdateThemeLabels();Check((string)Application.Current.Resources["ThemeLabel"]=="Appearance","English appearance labels");Loc.Language="ru";Loc.ApplyResources();UpdateThemeLabels();Check((string)Application.Current.Resources["ThemeLabel"]=="Оформление","Russian appearance labels");
   settings.Theme="brawl stars";ApplyTheme();Navigate("Home");UpdateLayout();
   Check(!Visuals(LobbyPage).OfType<Button>().Any(b=>b.Content is string text&&text is "↶" or "↷"),"Lobby rotation arrows removed");
   double yaw=LobbySkinViewport.Yaw;LobbySkinViewport.DragBy(80);Check(Math.Abs(LobbySkinViewport.Yaw-(yaw+52)%360)<.001,"Horizontal dragging rotates character");LobbySkinViewport.DragBy(-80);
   LobbySkinViewport.DragBy(360/.65);Check(Math.Abs((LobbySkinViewport.Yaw-yaw)%360)<.001,"Rotation wraps through a full circle");
   foreach(string emote in new[]{"wave","dance","cheer"}){
    LobbySkinViewport.PlayEmote(emote);await Task.Delay(500);Check(LobbySkinViewport.ActiveEmote==emote&&LobbySkinViewport.HasPoseAnimation&&Math.Abs(LobbySkinViewport.RightArmAngle)>5,"Animated character pose: "+emote);LobbySkinViewport.StopEmote();
   }
   Check(!LobbySkinViewport.HasPoseAnimation&&LobbySkinViewport.RightArmAngle==0,"Stopping an emote restores neutral pose");
   LobbySkinViewport.PlayEmote("dance",true);Check(!LobbySkinViewport.HasPoseAnimation,"Reduced motion disables emote movement");
   LobbySkinViewport.PlayEmote("wave");Navigate("Settings");UpdateLayout();Check(!LobbySkinViewport.HasPoseAnimation,"Leaving lobby stops emote clocks");
   Check(!SidebarShell.IsVisible&&BrawlHeading.IsVisible&&ThemeBackdrop.IsVisible&&BrawlProfilePanel.IsVisible,"Settings stay inside the Brawl theme shell");
   var themeCard=Visuals(SettingsPage).OfType<PixelFrame>().First();Check(themeCard.Stepped,"Settings cards use themed inset frames");
   foreach(string icon in new[]{"settings","library","back","wave","dance","cheer","worlds","packs","skins"}){
    using var resource=Application.GetResourceStream(new Uri("pack://application:,,,/Assets/Brawl/"+icon+".png")).Stream;Check(resource.Length>0,"Ready-made PNG embedded: "+icon);
   }
   Navigate("Home");bool wasLoading=catalogLoading;catalogLoading=true;
   try{LobbyVersionButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));Check(VersionPopup.IsOpen,"Version button opens picker");LobbyVersionButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));Check(!VersionPopup.IsOpen,"Same version button closes picker");}finally{catalogLoading=wasLoading;VersionPopup.IsOpen=false;}
   settings.Theme="default";ApplyTheme();Navigate("Settings");Check(SidebarShell.IsVisible&&!BrawlHeading.IsVisible&&!themeCard.Stepped,"Returning to default restores standard shell and cards");
   Navigate("Skins");UpdateLayout();Check(!Visuals(SkinsPage).OfType<Button>().Any(b=>b.Content is string t&&t==Loc.T("Применить шаблон")),"No redundant apply-preset button");
   var fixtures=Enumerable.Range(0,16).Select(i=>new SkinPresetCard(new SkinPreset(Guid.NewGuid().ToString("N"),"Fixture "+i,"classic"),"Fixture "+i,false)).ToArray();PresetCards.ItemsSource=fixtures;UpdateLayout();SkinsPage.ScrollToTop();await Task.Delay(100);
   var last=Visuals(PresetCards).OfType<Button>().Last();last.BringIntoView();await Task.Delay(100);Check(SkinsPage.VerticalOffset<1,"Preset BringIntoView does not jump the skins page");RefreshPresets();
   var fake=new AppearanceHttp();using var client=new ModrinthClient(fake);modrinth=client;
   // Category metadata is deliberately unavailable; browsing must still work without a selected game.
   selectedChoice=null;catalogType="mod";modsReady=false;await OpenModsAsync();Check(ModsResults.Items.Count==1,"Search succeeds when category endpoint fails and no game is selected");
   foreach(string type in new[]{"resourcepack","shader","modpack","mod"}){catalogType=type;await OpenModsAsync();Check(ModsResults.Items.OfType<ModProject>().Single().Id==type,"Catalog tab loads: "+type);}
   fake.HoldOld=true;modsUpdating=true;ModsQuery.Text="old";modsUpdating=false;var old=SearchModsAsync(true);await fake.OldStarted.Task.WaitAsync(TimeSpan.FromSeconds(2));
   catalogType="shader";modsUpdating=true;ModsQuery.Text="";modsUpdating=false;await SearchModsAsync(true);fake.ReleaseOld.TrySetResult();await old;Check(ModsResults.Items.OfType<ModProject>().Single().Id=="shader","Late response cannot replace the newly selected tab");
   fake.TransientFailures=2;await SearchModsAsync(true);Check(fake.TransientFailures==0&&ModsResults.Items.Count==1,"Transient API errors retry and recover");
   report.Add("No games launched; user settings and skin files were not written.");
  }finally{modrinth=originalClient;settings.Theme=oldTheme;settings.ThemePalette=oldPalette;selectedChoice=oldChoice;ApplyTheme();File.WriteAllLines(Path.Combine(root,"appearance-checks.txt"),report);}
 }
 sealed class AppearanceHttp:HttpMessageHandler {
  public bool HoldOld;public int TransientFailures;
  public TaskCompletionSource OldStarted=new(TaskCreationOptions.RunContinuationsAsynchronously),ReleaseOld=new(TaskCreationOptions.RunContinuationsAsynchronously);
  protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct){
   string url=Uri.UnescapeDataString(request.RequestUri!.ToString());
   if(url.Contains("tag/category"))return new(HttpStatusCode.BadRequest);
   if(HoldOld&&url.Contains("query=old")){OldStarted.TrySetResult();await ReleaseOld.Task;}
   if(TransientFailures>0){TransientFailures--;return new(HttpStatusCode.ServiceUnavailable);}
   string type=new[]{"resourcepack","shader","modpack"}.FirstOrDefault(t=>url.Contains("project_type:"+t))??"mod";
   string body=JsonSerializer.Serialize(new{hits=new[]{new{project_id=type,slug=type,title=type,description="Fixture",author="Test",downloads=1,categories=Array.Empty<string>(),icon_url=""}},total_hits=1});
   return new(HttpStatusCode.OK){Content=new StringContent(body,Encoding.UTF8,"application/json")};
  }
 }
}

using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Orbit.Core;
namespace OrbitLauncher;
public partial class MainWindow {
 async Task CheckXmclAsync(string root){
  var report=new List<string>();void Check(bool ok,string name){if(!ok)throw new Exception(name);report.Add("PASS "+name);}
  void Capture(string name){UpdateLayout();var image=new RenderTargetBitmap((int)ActualWidth,(int)ActualHeight,96,96,PixelFormats.Pbgra32);image.Render(this);var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(image));using var stream=File.Create(Path.Combine(root,name+".png"));encoder.Save(stream);}
  string oldTheme=settings.Theme;var oldChoice=selectedChoice;double oldWidth=Width,oldHeight=Height;
  try{
   NicknameBox.Text="Player";RootBox.Text=@"C:\Games\Minecraft";
   Check(Application.Current.Resources["XmclUpdateTitle"].ToString()!.Contains(ReleaseVersion),"Update card follows the current assembly version");
   settings.Theme="default 2";ApplyTheme();Check(settings.Theme=="default","Legacy default 2 migrates to default");Check(ThemeBox.Items.Count==2,"Only Default and Old theme are offered");Check(new LauncherSettings().Theme=="brawl stars","Fresh settings select the game lobby Default");Check(ThemeBox.Items.OfType<ThemeChoice>().Select(t=>t.Title).SequenceEqual(new[]{"Default","Old theme"}),"Public theme names are Default and Old theme");Check(XmclRailInstances.Items.Count==versions.Count,"Old theme rail includes every installed version");
   Navigate("Home");UpdateLayout();await Task.Delay(800);Check(SidebarColumn.Width.Value==288&&SidebarShell.IsVisible,"Widened labelled rail and player panel visible");Check(HomeVersions.ItemTemplate==FindResource("XmclInstanceCard"),"Compact instance layout on home");Check(XmclSidebarSkin.Children.Count>0,"Sidebar renders the actual skin model");Check(!LaunchNicknameField.IsVisible,"Nickname is edited in profile instead of duplicated in footer");Check(VersionPickerButton.IsVisible&&PlayButton.IsVisible,"Version picker and PLAY remain accessible");
   report.Add("Panorama loaded: "+(XmclBackgroundImage.Source!=null));Capture("xmcl-home");
   if(versions.Count>1){var card=Visuals(HomeVersions).OfType<Button>().Last(x=>x.Tag is VersionEntry&&x.Style==FindResource("CardButton"));var expected=(VersionEntry)card.Tag;card.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));Check(CurrentInstance!=null&&InstanceSelectionConverter.Same(CurrentInstance,expected),"Copied template click reaches real instance selection");}
   XmclProfile_Click(this,new RoutedEventArgs());for(int attempt=0;attempt<10&&(XmclProfileOverlay.ActualWidth<ActualWidth-2||XmclProfileOverlay.Opacity<.99);attempt++){await Task.Delay(100);UpdateLayout();}Check(XmclProfileOverlay.ActualWidth>=ActualWidth-2&&XmclProfileOverlay.Opacity>.95,"Profile dialog is visibly rendered over the full window");Check(XmclProfileOverlay.IsVisible&&XmclProfileNickname.IsKeyboardFocusWithin,"Profile opens with nickname focus");Capture("xmcl-profile");XmclProfileClose_Click(this,new RoutedEventArgs());Check(!XmclProfileOverlay.IsVisible,"Profile close button dismisses dialog");
   XmclProfile_Click(this,new RoutedEventArgs());XmclProfileSkins_Click(this,new RoutedEventArgs());Check(SkinsPage.IsVisible&&!XmclProfileOverlay.IsVisible,"Profile skin action opens functional skin settings");
   Navigate("Home");VersionPickerOpen_Click(VersionPickerButton,new RoutedEventArgs());Check(VersionPopup.IsOpen,"Version picker opens in the new layout");VersionPickerOpen_Click(VersionPickerButton,new RoutedEventArgs());Check(!VersionPopup.IsOpen,"Same picker button closes it");
   Navigate("Settings");UpdateLayout();await Task.Delay(650);Check(SettingsPage.IsVisible&&ThemeBox.IsVisible&&SettingsPage.Opacity>.99,"Settings are visibly rendered after the transition");Capture("xmcl-settings");
   settings.Theme="brawl stars";ApplyTheme();Navigate("Home");Check(LobbyPage.IsVisible&&!SidebarShell.IsVisible&&!XmclBackdrop.IsVisible,"Brawl keeps its separate lobby");settings.Theme="default";ApplyTheme();Navigate("Library");Check(AllVersions.ItemTemplate==FindResource("XmclInstanceCard")&&!BrawlFooter.IsVisible&&SidebarShell.IsVisible,"Switching back restores default across pages");
   Width=1080;Height=760;Navigate("Home");UpdateLayout();await Task.Delay(800);Check(PlayButton.ActualWidth>=180&&PlayButton.IsVisible&&PageHost.ActualWidth>=700,"Controls fit the minimum window size");Capture("xmcl-compact");
  }finally{CloseXmclProfile();Width=oldWidth;Height=oldHeight;selectedChoice=oldChoice;settings.Theme=oldTheme;UpdateChoices(oldChoice?.Id);ApplyTheme();Navigate("Home");File.WriteAllLines(Path.Combine(root,"xmcl-checks.txt"),report);}
 }
}

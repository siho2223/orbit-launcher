using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Orbit.Core;
namespace OrbitLauncher;
public partial class MainWindow {
 async Task CheckRefinementAsync(string root){
  var report=new List<string>();void Check(bool value,string message){if(!value)throw new Exception(message);report.Add("PASS "+message);}
  void Capture(string name){UpdateLayout();var image=new RenderTargetBitmap((int)ActualWidth,(int)ActualHeight,96,96,PixelFormats.Pbgra32);image.Render(this);var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(image));using var f=File.Create(Path.Combine(root,name+".png"));png.Save(f);}
  var oldVersions=versions;var oldChoice=selectedChoice;string oldTheme=settings.Theme,oldPalette=settings.ThemePalette;bool oldReduced=settings.ReduceMotion;string oldQuery=SearchBox.Text;
  try{
   foreach(string theme in new[]{"default","default 2","brawl stars"}){
    settings.Theme=theme;settings.ThemePalette="purple";settings.ReduceMotion=false;ApplyTheme();ApplyMotionPreference();Navigate("Library");UpdateLayout();await Task.Delay(80);
    var items=Visuals(AllVersions).OfType<ContentPresenter>().Where(x=>x.Content is VersionEntry).ToArray();Check(items.Length>0,"Installed cards visible: "+theme);if(theme=="brawl stars")Check(Visuals(AllVersions).OfType<PixelFrame>().Any(x=>x.Stepped),"Brawl cards keep stepped frames");else Check(AllVersions.ItemTemplate==FindResource("XmclInstanceCard"),"Default cards use compact XMCL layout: "+theme);Check(items.Any(x=>x.Opacity<.99),"Card reveal is visibly active: "+theme);Capture(theme.Replace(' ','-')+"-enter");
    await Task.Delay(1100);UpdateLayout();Capture(theme.Replace(' ','-')+"-settled");report.Add("Card opacity: "+string.Join(",",items.Select(x=>x.Opacity)));Check(items.All(x=>Math.Abs(x.Opacity-1)<.001),"All cards finish fully visible: "+theme);
    var card=Visuals(AllVersions).OfType<Button>().First(x=>x.Tag is VersionEntry&&x.Style==FindResource("CardButton"));typeof(App).GetMethod("ButtonEnter",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(Application.Current,new object[]{card,new MouseEventArgs(Mouse.PrimaryDevice,0)});await Task.Delay(200);Check(card.RenderTransform is ScaleTransform scale&&scale.ScaleX==1&&scale.ScaleY==1,"Hover preserves card bounds: "+theme);
    var second=versions.Last();VersionCard_Click(new Button{Tag=second},new RoutedEventArgs());Check(CurrentInstance!=null&&InstanceSelectionConverter.Same(CurrentInstance,second)&&HeroInstanceName.Text==second.Title,"Selected card and home summary follow selection: "+theme);
    settings.ReduceMotion=true;ApplyMotionPreference();Check(items.All(x=>x.Opacity==1&&x.RenderTransform is TranslateTransform t&&t.Y==0),"Reduced motion resets visible cards: "+theme);
    Navigate("Settings");await Task.Delay(40);Check(SettingsPage.Opacity==1,"Reduced motion settings are immediate: "+theme);Capture(theme.Replace(' ','-')+"-settings");
    settings.ReduceMotion=false;ApplyMotionPreference();Navigate("Home");Navigate("Library");Navigate("Settings");Navigate("Home");await Task.Delay(800);Check((theme=="brawl stars"?(FrameworkElement)LobbyPage:HomePage).Opacity==1,"Rapid navigation leaves current page visible: "+theme);
    Capture(theme.Replace(' ','-')+"-home");
   }
   SearchBox.Text="__orbit_no_match__";await RefreshVersions();Check(AllVersions.Items.Count==0,"Refreshing the library preserves the active search filter");SearchBox.Text="";
   var first=new VersionEntry("same-loader-id","Forge","1.20.1",17){RootOverride=Path.Combine(root,"fixture-a"),DisplayName="Pack A"};var secondPack=first with{RootOverride=Path.Combine(root,"fixture-b"),DisplayName="Pack B"};versions=new(){first,secondPack};selectedChoice=secondPack;UpdateChoices(secondPack.Id);Check(SelectedChoice?.RootOverride==secondPack.RootOverride,"Same version IDs preserve the correct instance root");Check(CurrentInstance?.Title=="Pack B","Hero displays the correct duplicate-ID pack");
   Check((Visibility)new InstanceSelectionConverter().Convert(new object[]{first,secondPack},typeof(Visibility),null!,System.Globalization.CultureInfo.InvariantCulture)==Visibility.Collapsed,"Different instance roots are not highlighted as the same selection");
   settings.Theme="brawl stars";settings.ReduceMotion=false;ApplyTheme();ApplyMotionPreference();Navigate("Home");Check(AvatarDragSurface.RenderTransform.HasAnimatedProperties,"Lobby idle starts on home");Navigate("Library");Check(!AvatarDragSurface.RenderTransform.HasAnimatedProperties,"Lobby idle stops away from home");
   settings.ReduceMotion=true;ApplyMotionPreference();Navigate("Home");Check(!AvatarDragSurface.RenderTransform.HasAnimatedProperties&&!LobbyBackground.HasAnimatedProperties&&!ThemeBackdrop.HasAnimatedProperties,"Reduced motion stops lobby and background animation");
  }finally{versions=oldVersions;selectedChoice=oldChoice;settings.Theme=oldTheme;settings.ThemePalette=oldPalette;settings.ReduceMotion=oldReduced;SearchBox.Text=oldQuery;UpdateChoices(oldChoice?.Id);ApplyTheme();ApplyMotionPreference();Navigate("Home");File.WriteAllLines(Path.Combine(root,"refinement-checks.txt"),report);}
 }
}

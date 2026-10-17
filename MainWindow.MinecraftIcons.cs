using System.Windows;
using System.Windows.Media.Imaging;
namespace OrbitLauncher;
public partial class MainWindow {
 void RefreshMinecraftIcons(){
  var icons=MinecraftItemIcons.Load(settings.GameRoot,SelectedChoice?.BaseVersion??settings.Version);
  foreach(string name in MinecraftItemIcons.Items.Keys){
   if(icons.TryGetValue(name,out var image))Resources["MinecraftIcon_"+name]=image;
   else Resources["MinecraftIcon_"+name]=new BitmapImage(new Uri("pack://application:,,,/Assets/Brawl/"+name+".png"));
  }
  RefreshXmclPanorama();
  foreach(string name in new[]{"settings","back","library"})Resources["MinecraftIcon_"+name]=new BitmapImage(new Uri("pack://application:,,,/Assets/Brawl/"+name+".png"));
 }
}

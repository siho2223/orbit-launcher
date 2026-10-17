using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Orbit.Core;
namespace OrbitLauncher;
internal static class LoaderArtwork {
 internal static readonly BitmapSource Fabric=Load("fabric");
 internal static readonly BitmapSource Forge=Load("forge",true);
 static BitmapSource Load(string name,bool crop=false){var image=new BitmapImage(new Uri("pack://application:,,,/Assets/Loaders/"+name+".png"));BitmapSource result=crop?new CroppedBitmap(image,new Int32Rect(0,0,80,image.PixelHeight)):image;result.Freeze();return result;}
 internal static ImageSource Icon(VersionEntry? version)=>version?.VisualKind switch{"forge"=>Forge,"fabric"=>Fabric,_=>Application.Current.MainWindow?.TryFindResource("MinecraftIcon_worlds") as ImageSource??Fabric};
}
public sealed class VersionVisualConverter:IValueConverter {
 public object Convert(object value,Type targetType,object parameter,CultureInfo culture){var v=value as VersionEntry;return parameter?.ToString() switch{"Icon"=>LoaderArtwork.Icon(v),"Foreground"=>new SolidColorBrush((Color)ColorConverter.ConvertFromString(v?.TileForeground??"#FFFFFF")),_=>new SolidColorBrush((Color)ColorConverter.ConvertFromString(v?.TileColor??"#30323E"))};}
 public object ConvertBack(object value,Type type,object parameter,CultureInfo culture)=>Binding.DoNothing;
}
public partial class MainWindow {
 void InitializeVersionArtwork(){Resources["LoaderForge"]=LoaderArtwork.Forge;Resources["LoaderFabric"]=LoaderArtwork.Fabric;}
}

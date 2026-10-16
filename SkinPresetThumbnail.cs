using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Orbit.Core;
namespace OrbitLauncher;
public sealed class SkinPresetThumbnail:FrameworkElement {
 public static readonly DependencyProperty PresetProperty=DependencyProperty.Register(nameof(Preset),typeof(SkinPreset),typeof(SkinPresetThumbnail),new FrameworkPropertyMetadata(null,FrameworkPropertyMetadataOptions.AffectsRender,Changed));
 public SkinPreset? Preset {get=>(SkinPreset?)GetValue(PresetProperty);set=>SetValue(PresetProperty,value);}
 BitmapSource? image;
 static void Changed(DependencyObject d,DependencyPropertyChangedEventArgs e){var view=(SkinPresetThumbnail)d;view.image=null;if(e.NewValue is SkinPreset preset)try{using var stream=File.OpenRead(SkinPresets.Image(preset.Id));var decoder=new PngBitmapDecoder(stream,BitmapCreateOptions.PreservePixelFormat,BitmapCacheOption.OnLoad);view.image=decoder.Frames[0];view.image.Freeze();}catch(IOException){}catch(NotSupportedException){}catch(FileFormatException){}view.InvalidateVisual();}
 public SkinPresetThumbnail(){RenderOptions.SetBitmapScalingMode(this,BitmapScalingMode.NearestNeighbor);IsHitTestVisible=false;}
 protected override void OnRender(DrawingContext dc){base.OnRender(dc);if(image==null)return;bool modern=image.PixelHeight==64,slim=modern&&Preset?.Model=="slim";double arm=slim?3:4,scale=Math.Min(ActualWidth/(8+2*arm+2),ActualHeight/34),left=(ActualWidth-(8+2*arm)*scale)/2,top=(ActualHeight-32*scale)/2;
 void Part(int u,int v,int w,int h,double x,double y){var crop=new CroppedBitmap(image,new Int32Rect(u,v,w,h));crop.Freeze();dc.DrawImage(crop,new Rect(left+x*scale,top+y*scale,w*scale,h*scale));}
 int aw=(int)arm;Part(8,8,8,8,arm,0);Part(20,20,8,12,arm,8);Part(44,20,aw,12,0,8);Part(modern?36:44,modern?52:20,aw,12,arm+8,8);Part(4,20,4,12,arm,20);Part(modern?20:4,modern?52:20,4,12,arm+4,20);
 Part(40,8,8,8,arm,0);if(modern){Part(20,36,8,12,arm,8);Part(44,36,aw,12,0,8);Part(52,52,aw,12,arm+8,8);Part(4,36,4,12,arm,20);Part(4,52,4,12,arm+4,20);}
 }
}

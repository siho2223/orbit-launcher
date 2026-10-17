using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
namespace OrbitLauncher;
// A stepped frame with an inset face, top highlight and a deep lower edge.
public sealed class PixelFrame:Border {
 public static readonly DependencyProperty SteppedProperty=DependencyProperty.Register(nameof(Stepped),typeof(bool),typeof(PixelFrame),new FrameworkPropertyMetadata(true,FrameworkPropertyMetadataOptions.AffectsRender));
 public bool Stepped{get=>(bool)GetValue(SteppedProperty);set=>SetValue(SteppedProperty,value);}
 public static readonly DependencyProperty HighlightProperty=DependencyProperty.Register(nameof(Highlight),typeof(bool),typeof(PixelFrame),new FrameworkPropertyMetadata(false,FrameworkPropertyMetadataOptions.AffectsRender));
 public static readonly DependencyProperty PressedProperty=DependencyProperty.Register(nameof(Pressed),typeof(bool),typeof(PixelFrame),new FrameworkPropertyMetadata(false,FrameworkPropertyMetadataOptions.AffectsRender));
 public bool Highlight{get=>(bool)GetValue(HighlightProperty);set=>SetValue(HighlightProperty,value);}
 public bool Pressed{get=>(bool)GetValue(PressedProperty);set=>SetValue(PressedProperty,value);}
 public static readonly Thickness FrameInsets=new(6,5,6,9);
 static PixelFrame(){BorderThicknessProperty.OverrideMetadata(typeof(PixelFrame),new FrameworkPropertyMetadata(FrameInsets,FrameworkPropertyMetadataOptions.AffectsMeasure|FrameworkPropertyMetadataOptions.AffectsRender));}
 public PixelFrame(){SnapsToDevicePixels=true;}
 static Geometry Shape(double x,double y,double w,double h,double notch){
  var path=new StreamGeometry();using(var c=path.Open()){c.BeginFigure(new Point(x+notch,y),true,true);c.PolyLineTo(new[]{new Point(x+w-notch,y),new Point(x+w-notch,y+notch),new Point(x+w,y+notch),new Point(x+w,y+h-notch),new Point(x+w-notch,y+h-notch),new Point(x+w-notch,y+h),new Point(x+notch,y+h),new Point(x+notch,y+h-notch),new Point(x,y+h-notch),new Point(x,y+notch),new Point(x+notch,y+notch)},true,false);}path.Freeze();return path;
 }
 protected override void OnRender(DrawingContext dc){
  if(!Stepped){base.OnRender(dc);return;}
  double w=ActualWidth,h=ActualHeight;if(w<16||h<16)return;
  dc.DrawGeometry(new SolidColorBrush(Color.FromRgb(17,21,24)),null,Shape(0,0,w,h,5));
  dc.DrawGeometry(new SolidColorBrush(Color.FromRgb(71,77,76)),null,Shape(2,2,w-4,h-6,3));
  dc.DrawGeometry(Background??Brushes.DimGray,null,Shape(5,5,w-10,h-13,2));
  dc.DrawGeometry(new LinearGradientBrush(Color.FromArgb(Pressed?(byte)8:(byte)35,255,255,255),Color.FromArgb(Pressed?(byte)60:(byte)10,0,0,0),90),null,Shape(5,5,w-10,h-13,2));
  dc.DrawLine(new Pen(new SolidColorBrush(Color.FromArgb(Highlight?(byte)210:(byte)95,255,255,255)),2),new Point(6,5),new Point(w-6,5));
  dc.DrawLine(new Pen(new SolidColorBrush(Color.FromArgb(75,0,0,0)),3),new Point(5,h-9),new Point(w-5,h-9));
  if(Highlight)dc.DrawGeometry(null,new Pen(new SolidColorBrush(Color.FromArgb(130,255,255,255)),1),Shape(4,4,w-8,h-11,2));
 }
}

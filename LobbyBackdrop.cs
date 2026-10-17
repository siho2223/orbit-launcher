using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
namespace OrbitLauncher;
public sealed class LobbyBackdrop:FrameworkElement {
 DrawingGroup? cachedBackdrop;Size cachedSize;
 public static readonly DependencyProperty DriftProperty=DependencyProperty.Register("Drift",typeof(double),typeof(LobbyBackdrop),new FrameworkPropertyMetadata(0d,FrameworkPropertyMetadataOptions.AffectsRender));
 public void SetMotion(bool enabled){
  if(enabled==HasAnimatedProperties)return;
  BeginAnimation(DriftProperty,null);SetValue(DriftProperty,0d);
  if(enabled){var motion=new DoubleAnimation(0,1,TimeSpan.FromSeconds(14)){RepeatBehavior=RepeatBehavior.Forever};Timeline.SetDesiredFrameRate(motion,24);BeginAnimation(DriftProperty,motion);}
 }
 string palette="purple";
 public string Palette{get=>palette;set{if(palette==value)return;palette=value;cachedBackdrop=null;InvalidateVisual();}}
 protected override void OnRender(DrawingContext dc){
  double w=ActualWidth,h=ActualHeight;if(w<=0||h<=0)return;
  if(cachedBackdrop==null||cachedSize!=RenderSize){cachedBackdrop=new DrawingGroup();using(var canvas=cachedBackdrop.Open())DrawBackdrop(canvas,w,h);cachedBackdrop.Freeze();cachedSize=RenderSize;}
  dc.DrawDrawing(cachedBackdrop);
  double phase=(double)GetValue(DriftProperty);var random=new Random(822);
  for(int i=0;i<22;i++){double x=random.NextDouble()*w,y=(random.NextDouble()*h-phase*(26+i%5*9)+h)%h,size=i%4==0?3:2;byte alpha=(byte)(30+25*(1+Math.Sin(phase*Math.PI*2+i)));dc.DrawRectangle(new SolidColorBrush(Color.FromArgb(alpha,255,245,210)),null,new Rect(x,y,size,size));}
 }
 void DrawBackdrop(DrawingContext dc,double w,double h){
  Color C(string value)=>(Color)ColorConverter.ConvertFromString(value);
  var colors=palette switch{"green"=>new[]{"#D7E58E","#78972A","#243A10"},"orange"=>new[]{"#F6D68E","#B97C30","#513110"},"blue"=>new[]{"#A3DCE8","#4B82B5","#192E55"},"pink"=>new[]{"#F6B1D2","#B44E82","#431F3B"},_=>new[]{"#CFB0F0","#8352B4","#302044"}};
  var center=new Point(.52,.48);
  dc.DrawRectangle(new RadialGradientBrush(new GradientStopCollection{new(C(colors[0]),0),new(C(colors[1]),.48),new(C(colors[2]),1)}){Center=center,GradientOrigin=center,RadiusX=.71,RadiusY=.83},null,new Rect(0,0,w,h));
  var origin=new Point(w*center.X,h*center.Y);double extent=Math.Sqrt(w*w+h*h);
  for(int i=0;i<26;i++){
   double a=i*Math.PI/13+.3,b=a+.055;var ray=new StreamGeometry();using(var p=ray.Open()){p.BeginFigure(origin,true,true);p.LineTo(new Point(origin.X+Math.Cos(a)*extent,origin.Y+Math.Sin(a)*extent),true,false);p.LineTo(new Point(origin.X+Math.Cos(b)*extent,origin.Y+Math.Sin(b)*extent),true,false);}
   dc.DrawGeometry(new SolidColorBrush(Color.FromArgb((byte)(i%3==0?7:3),255,255,255)),null,ray);
  }
  var random=new Random(470);for(int i=0;i<160;i++){double x=random.NextDouble()*w,y=random.NextDouble()*h,size=1+random.NextDouble()*2;byte alpha=(byte)random.Next(6,32);dc.DrawRectangle(new SolidColorBrush(Color.FromArgb(alpha,255,242,224)),null,new Rect(x,y,size,size));}
  for(int i=0;i<25;i++){double x=origin.X+(random.NextDouble()-.5)*w*.48,y=origin.Y+(random.NextDouble()-.5)*h*.65,size=1+random.NextDouble()*3;dc.DrawRectangle(new SolidColorBrush(Color.FromArgb((byte)random.Next(65,160),255,239,184)),null,new Rect(x,y,size,size));}
 }
}

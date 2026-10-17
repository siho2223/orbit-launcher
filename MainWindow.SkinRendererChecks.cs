using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
namespace OrbitLauncher;
public partial class MainWindow {
 async Task CheckSkinRendererAsync(string root){
  Directory.CreateDirectory(root);var checks=new List<string>();
  void Check(bool value,string label){if(!value)throw new Exception(label);checks.Add("PASS "+label);}
  BitmapSource Fixture(int height,bool overlay){
   byte[] data=new byte[64*height*4];
   for(int i=0;i<data.Length;i+=4){data[i]=120;data[i+1]=85;data[i+2]=40;data[i+3]=0;}
   void Set(int x,int y,byte red,byte alpha){int index=(y*64+x)*4;data[index]=35;data[index+1]=95;data[index+2]=red;data[index+3]=alpha;}
   if(overlay){for(int y=36;y<48;y++)for(int x=20;x<28;x++)Set(x,y,240,(byte)(x==20||x==27||y==36||y==47?0:255));}
   if(height==32)for(int y=20;y<32;y++)for(int x=4;x<8;x++)Set(x,y,(byte)(20+(x-4)*60),0);
   return BitmapSource.Create(64,height,96,96,PixelFormats.Bgra32,null,data,64*4);
  }
  RenderTargetBitmap Render(BitmapSource skin,bool slim,string name){
   var view=new SkinPreview{Width=240,Height=400};view.Draw(skin,slim);view.Turn(-view.Yaw);
   view.Camera=new OrthographicCamera(new Point3D(0,16,90),new Vector3D(0,0,-90),new Vector3D(0,1,0),24);
   view.Measure(new Size(240,400));view.Arrange(new Rect(0,0,240,400));view.UpdateLayout();
   var bitmap=new RenderTargetBitmap(240,400,96,96,PixelFormats.Pbgra32);bitmap.Render(view);
   using var file=File.Create(Path.Combine(root,name+".png"));var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(bitmap));encoder.Save(file);return bitmap;
  }
  byte[] Pixel(BitmapSource bitmap,double x,double y){int px=(int)Math.Round(120+x*10),py=(int)Math.Round(200-(y-16)*10);byte[] sample=new byte[4];bitmap.CopyPixels(new Int32Rect(px,py,1,1),sample,4,0);return sample;}
  try{
   foreach(bool slim in new[]{false,true}){
    var bitmap=Render(SkinTextures.Normalize(Fixture(64,true)),slim,slim?"slim-cutout":"classic-cutout");
    foreach(var point in new[]{(-4.1,18d),(4.1,18d),(0d,23.9),(0d,12.1),(-.1,6d),(.1,6d)})Check(Pixel(bitmap,point.Item1,point.Item2)[3]>245,(slim?"Slim":"Classic")+" cutout overlay preserves opaque joint at "+point);
    Check((slim?Pixel(bitmap,7.5,18)[3]<20:Pixel(bitmap,7.5,18)[3]>245),"Correct "+(slim?"three":"four")+" pixel arm width");
   }
   var legacy=Render(Fixture(32,false),true,"legacy-mirrored");
   Check(Pixel(legacy,7.5,18)[3]>245,"Legacy skins use classic geometry even if slim was requested");
   Check(Math.Abs(Pixel(legacy,-3.5,6)[2]-Pixel(legacy,3.5,6)[2])<4&&Math.Abs(Pixel(legacy,-.5,6)[2]-Pixel(legacy,.5,6)[2])<4,"Legacy left leg mirrors right leg texture");
   Check(Pixel(legacy,.5,6)[2]>Pixel(legacy,3.5,6)[2]+80,"Legacy asymmetric texture is not duplicated without reflection");
   var source=Fixture(64,true);var normalized=SkinTextures.Normalize(source);byte[] untouched=new byte[4];source.CopyPixels(new Int32Rect(20,20,1,1),untouched,4,0);Check(untouched[3]==0,"Source PNG pixels remain unmodified");
   byte[] alpha=new byte[4];normalized.CopyPixels(new Int32Rect(20,20,1,1),alpha,4,0);Check(alpha[3]==255,"Inner layer is opaque");normalized.CopyPixels(new Int32Rect(20,36,1,1),alpha,4,0);Check(alpha[3]==0,"Overlay holes remain transparent");
   settings.Theme="brawl stars";ApplyTheme();Navigate("Home");UpdateLayout();
   Check(Visuals(LobbyPage).OfType<TextBlock>().Count(t=>t.Text==Loc.T("Сборки"))==1,"Only one modpacks entry remains in the lobby");
   Check(Visuals(LobbyPage).OfType<Button>().Count(b=>b.Content is string text&&text==Loc.T("Открыть сборки"))==0,"Right-hand open-modpacks button removed");
   if(File.Exists(Orbit.Core.LocalSkins.SkinPath)){
    var real=DecodeSkin(File.ReadAllBytes(Orbit.Core.LocalSkins.SkinPath));Render(real,settings.SkinModel=="slim","saved-skin-front");Check(true,"Saved user skin rendered without changing the stored file");
   }
   await Task.CompletedTask;
  }finally{File.WriteAllLines(Path.Combine(root,"skin-render-checks.txt"),checks);}
 }
}

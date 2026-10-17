using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace OrbitLauncher;
// Match Java's opaque inner skin layer without changing the saved PNG file.
internal static class SkinTextures {
 public static BitmapSource Normalize(BitmapSource input){
  if(input.PixelWidth!=64||input.PixelHeight is not(32 or 64))throw new ArgumentException("Expected a 64x64 or 64x32 skin.");
  var format=new FormatConvertedBitmap(input,PixelFormats.Bgra32,null,0);byte[] pixels=new byte[64*input.PixelHeight*4];format.CopyPixels(pixels,64*4,0);
  if(input.PixelHeight==32){bool opaqueHat=true;for(int y=0;y<16;y++)for(int x=32;x<64;x++)opaqueHat&=pixels[(y*64+x)*4+3]==255;if(opaqueHat)for(int y=0;y<16;y++)for(int x=32;x<64;x++)pixels[(y*64+x)*4+3]=0;}
  void Opaque(int x,int y,int w,int h){for(int row=y;row<y+h;row++)for(int col=x;col<x+w;col++)pixels[(row*64+col)*4+3]=255;}
  Opaque(0,0,32,16);Opaque(0,16,56,16);if(input.PixelHeight==64)Opaque(16,48,32,16);
  var normalized=BitmapSource.Create(64,input.PixelHeight,96,96,PixelFormats.Bgra32,null,pixels,64*4);normalized.Freeze();return normalized;
 }
}

using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace OrbitLauncher;
public partial class MainWindow {
 static BitmapSource? lobbyAvatar;
 // Original built-in preview texture; a saved PNG always takes precedence.
 static BitmapSource LobbyAvatar(){
  if(lobbyAvatar!=null)return lobbyAvatar;
  byte[] pixels=new byte[64*64*4];
  void Paint(int x,int y,int w,int h,string hex){var c=(Color)ColorConverter.ConvertFromString(hex);for(int row=y;row<y+h;row++)for(int col=x;col<x+w;col++){int i=(row*64+col)*4;pixels[i]=c.B;pixels[i+1]=c.G;pixels[i+2]=c.R;pixels[i+3]=255;}}
  Paint(0,0,32,16,"#42352F");Paint(8,10,8,6,"#E8B88B");Paint(8,9,2,3,"#E8B88B");Paint(14,9,2,3,"#E8B88B");Paint(9,11,2,1,"#F9E6C8");Paint(13,11,2,1,"#F9E6C8");Paint(10,11,1,1,"#374C62");Paint(13,11,1,1,"#374C62");Paint(11,14,2,1,"#A16F55");
  Paint(16,16,24,16,"#D77F36");Paint(20,20,8,12,"#E89746");Paint(23,20,2,12,"#283A4A");Paint(20,20,2,5,"#F2B65F");Paint(26,20,2,5,"#F2B65F");Paint(20,27,2,2,"#B8682F");Paint(26,27,2,2,"#B8682F");
  Paint(40,16,16,16,"#D77F36");Paint(40,28,16,4,"#E8B88B");Paint(32,48,16,16,"#D77F36");Paint(32,60,16,4,"#E8B88B");
  Paint(0,16,16,16,"#283A4A");Paint(0,29,16,3,"#182433");Paint(16,48,16,16,"#283A4A");Paint(16,61,16,3,"#182433");
  lobbyAvatar=BitmapSource.Create(64,64,96,96,PixelFormats.Bgra32,null,pixels,64*4);lobbyAvatar.Freeze();return lobbyAvatar;
 }
}

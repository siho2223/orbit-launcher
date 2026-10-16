using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Orbit.Core;
namespace OrbitLauncher;
public partial class MainWindow {
 // Only the offscreen preview harness uses an unsaved image.
 byte[]? pendingSkin;
 string skinModel="classic";
 bool skinLegacy;
 BitmapSource DecodeSkin(byte[] data) {
  LocalSkins.Validate(data);
  using var stream=new MemoryStream(data);
  var decoder=new PngBitmapDecoder(stream,BitmapCreateOptions.PreservePixelFormat,BitmapCacheOption.OnLoad);
  var image=decoder.Frames[0];image.Freeze();return image;
 }
 void RefreshSkin() {
  try {
   bool stored=File.Exists(LocalSkins.SkinPath);
   byte[]? data=pendingSkin??(stored?File.ReadAllBytes(LocalSkins.SkinPath):null);
   BitmapSource? image=data==null?null:DecodeSkin(data);
   skinLegacy=image?.PixelHeight==32;
   if(skinLegacy)skinModel="classic";
   SkinViewport.Draw(image,skinModel=="slim");
   SkinInfo.Text=image==null?Loc.T("PNG 64×64 или 64×32"):$"{image.PixelWidth}×{image.PixelHeight} · "+(skinModel=="slim"?Loc.T("Alex · узкие руки"):Loc.T("Steve · обычные руки"));
   SkinStatus.Text=pendingSkin!=null?Loc.T("Предпросмотр скина."):settings.SkinEnabled
    ?(launching?Loc.T("Скин сохранён и включён. Для применения изменений закрой Minecraft и запусти игру снова через ORBIT."):Loc.T("Скин сохранён и включён. Запусти Minecraft через ORBIT — он подключится автоматически."))
    :(stored?Loc.T("Скин сохранён, но выключен. Нажми «Включить», чтобы использовать его при следующем запуске."):Loc.T("Выбери PNG — скин сразу сохранится и включится."));
   EnableSkinButton.Content=settings.SkinEnabled?Loc.T("Скин включён"):Loc.T("Включить");
   EnableSkinButton.IsEnabled=stored&&!settings.SkinEnabled;
   DisableSkinButton.IsEnabled=settings.SkinEnabled;
   ClassicSkinButton.Background=new SolidColorBrush((Color)ColorConverter.ConvertFromString(skinModel=="classic"?"#20564C":"#1B2E3D"));
   SlimSkinButton.Background=new SolidColorBrush((Color)ColorConverter.ConvertFromString(skinModel=="slim"?"#20564C":"#1B2E3D"));
   SlimSkinButton.IsEnabled=!skinLegacy;
  } catch(Exception e) { SkinStatus.Text=e.Message; }
 }
 void SaveSkin(byte[]? data=null) {
  if(data==null&&!File.Exists(LocalSkins.SkinPath))throw new IOException(Loc.T("Сначала выбери PNG-скин."));
  if(data!=null) {
   var image=DecodeSkin(data);
   if(image.PixelHeight==32)skinModel="classic";
   Directory.CreateDirectory(Path.GetDirectoryName(LocalSkins.SkinPath)!);
   File.WriteAllBytes(LocalSkins.SkinPath+".new",data);
   File.Move(LocalSkins.SkinPath+".new",LocalSkins.SkinPath,true);
  }
  settings.SkinEnabled=true;settings.SkinModel=skinModel;settings.Save();
  pendingSkin=null;RefreshSkin();
  Status(launching?Loc.T("Скин сохранён. Перезапусти Minecraft, чтобы увидеть его."):Loc.T("Скин сохранён и включён. Можно запускать Minecraft."));
 }
 void ChooseSkin_Click(object sender,RoutedEventArgs e) {
  var dialog=new OpenFileDialog{Title=Loc.T("Выбери Minecraft-скин"),Filter="Minecraft skin (*.png)|*.png"};
  if(dialog.ShowDialog()!=true)return;
  try {
   var frame=DecodeSkin(File.ReadAllBytes(dialog.FileName));
   var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(frame));
   using var stream=new MemoryStream();encoder.Save(stream);
   SaveSkin(stream.ToArray());
  } catch(Exception ex) { SkinStatus.Text=Loc.T("Не удалось сохранить скин: ")+ex.Message; }
 }
 void ApplySkin_Click(object sender,RoutedEventArgs e) {
  try { SaveSkin(); } catch(Exception ex) { SkinStatus.Text=ex.Message; }
 }
 void ResetSkin_Click(object sender,RoutedEventArgs e) {
  try {
   settings.SkinEnabled=false;settings.Save();pendingSkin=null;RefreshSkin();
   Status(Loc.T("Скин выключен. Перезапусти Minecraft, чтобы убрать его в игре."));
  } catch(Exception ex) { SkinStatus.Text=Loc.T("Не удалось сохранить настройку: ")+ex.Message; }
 }
 void SkinModel_Click(object sender,RoutedEventArgs e) {
  skinModel=(string)((Button)sender).Tag;
  try {
   if(pendingSkin==null&&settings.SkinEnabled)SaveSkin();
   else {settings.SkinModel=skinModel;settings.Save();RefreshSkin();}
  } catch(Exception ex) { SkinStatus.Text=Loc.T("Не удалось сохранить модель: ")+ex.Message; }
 }
 void SkinTurn_Click(object sender,RoutedEventArgs e)=>SkinViewport.Turn(double.Parse((string)((Button)sender).Tag,System.Globalization.CultureInfo.InvariantCulture));
}

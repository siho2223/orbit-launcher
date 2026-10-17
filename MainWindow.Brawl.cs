using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
namespace OrbitLauncher;
public partial class MainWindow {
 Point avatarPointer;bool avatarDragging;
 Style? originalButtonStyle;
 void InitializeBrawlInteractions(){
  originalButtonStyle=(Style)Application.Current.FindResource(typeof(Button));
  VersionPopup.StaysOpen=true;
  PreviewMouseLeftButtonDown+=(s,e)=>{
   if(!VersionPopup.IsOpen)return;
   if(IsWithin(e.OriginalSource as DependencyObject,VersionPopup.Child)||IsWithin(e.OriginalSource as DependencyObject,VersionPopup.PlacementTarget))return;
   VersionPopup.IsOpen=false;
  };
  PreviewKeyDown+=(s,e)=>{if(e.Key==Key.Escape&&VersionPopup.IsOpen){VersionPopup.IsOpen=false;e.Handled=true;}};
  Deactivated+=(s,e)=>VersionPopup.IsOpen=false;
 }
 static bool IsWithin(DependencyObject? target,DependencyObject? ancestor){
  if(ancestor==null)return false;
  for(var item=target;item!=null;){if(ReferenceEquals(item,ancestor))return true;item=item is Visual or System.Windows.Media.Media3D.Visual3D?VisualTreeHelper.GetParent(item):LogicalTreeHelper.GetParent(item);}return false;
 }
 void VersionPopup_Opened(object sender,EventArgs e){ApplyPickerAppearance();}
 void VersionPopup_Closed(object sender,EventArgs e){}
 void ApplyPickerAppearance(){
  if(VersionPopup.Child is not Border panel)return;
  bool brawl=settings.Theme=="brawl stars";
  panel.CornerRadius=new CornerRadius(brawl?3:16);panel.BorderThickness=brawl?new Thickness(6,5,6,9):new Thickness(1);
  panel.SetResourceReference(Border.BorderBrushProperty,brawl?"LobbyAccent":"Brush405348");
  panel.SetResourceReference(Border.BackgroundProperty,"Brush141E25");
 }
 void AvatarDrag_Start(object sender,MouseButtonEventArgs e){
  avatarPointer=e.GetPosition(AvatarDragSurface);avatarDragging=AvatarDragSurface.CaptureMouse();e.Handled=true;
 }
 void AvatarDrag_Move(object sender,MouseEventArgs e){
  if(!avatarDragging)return;
  if(e.LeftButton!=MouseButtonState.Pressed){AvatarDragSurface.ReleaseMouseCapture();avatarDragging=false;return;}
  var p=e.GetPosition(AvatarDragSurface);LobbySkinViewport.DragBy(p.X-avatarPointer.X);avatarPointer=p;e.Handled=true;
 }
 void AvatarDrag_End(object sender,MouseButtonEventArgs e){avatarDragging=false;AvatarDragSurface.ReleaseMouseCapture();e.Handled=true;}
 void AvatarDrag_Lost(object sender,MouseEventArgs e)=>avatarDragging=false;
 void Emote_Click(object sender,RoutedEventArgs e){if(((Button)sender).Tag is string name)LobbySkinViewport.PlayEmote(name,settings.ReduceMotion);}
 void UpdateBrawlResources(bool brawl){
  Resources[typeof(Button)]=brawl?(Style)FindResource("LobbyButton"):originalButtonStyle??(Style)Application.Current.FindResource(typeof(Button));
  ApplyPickerAppearance();
  if(!brawl)LobbySkinViewport.StopEmote();
 }
}

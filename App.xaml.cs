using System.IO;using System.Windows;using System.Windows.Media;using System.Windows.Media.Imaging;
namespace OrbitLauncher;
public partial class App:Application {
 void ButtonEnter(object sender,System.Windows.Input.MouseEventArgs e)=>AnimateButton(sender,1.018,.75);
 void ButtonLeave(object sender,System.Windows.Input.MouseEventArgs e)=>AnimateButton(sender,1,0);
 void ButtonPressed(object sender,System.Windows.Input.MouseButtonEventArgs e)=>AnimateButton(sender,.975,1);
 void ButtonReleased(object sender,System.Windows.Input.MouseButtonEventArgs e)=>AnimateButton(sender,1,sender is System.Windows.Controls.Button {IsMouseOver:true}?.75:0);
 void AnimateButton(object sender,double scale,double hover){if(sender is not System.Windows.Controls.Button button||!button.IsEnabled)return;bool reduced=Resources["ReduceMotion"] is true;
  if(button.Style==Resources["Nav"]||button.Style==Resources["CardButton"])scale=1;else scale=Math.Min(1,scale);
  if(button.RenderTransform is not ScaleTransform){button.RenderTransform=new ScaleTransform(1,1);button.RenderTransformOrigin=new Point(.5,.5);}var transform=(ScaleTransform)button.RenderTransform;
  if(reduced){transform.BeginAnimation(ScaleTransform.ScaleXProperty,null);transform.BeginAnimation(ScaleTransform.ScaleYProperty,null);transform.ScaleX=transform.ScaleY=1;if(button.Template.FindName("HoverStroke",button) is FrameworkElement staticStroke){staticStroke.BeginAnimation(UIElement.OpacityProperty,null);staticStroke.Opacity=hover;}return;}
  var duration=TimeSpan.FromMilliseconds(hover==1?80:160);var ease=new System.Windows.Media.Animation.CubicEase{EasingMode=System.Windows.Media.Animation.EasingMode.EaseOut};
  transform.BeginAnimation(ScaleTransform.ScaleXProperty,new System.Windows.Media.Animation.DoubleAnimation(reduced?1:scale,duration){EasingFunction=ease});transform.BeginAnimation(ScaleTransform.ScaleYProperty,new System.Windows.Media.Animation.DoubleAnimation(reduced?1:scale,duration){EasingFunction=ease});
  if(button.Template.FindName("HoverStroke",button) is FrameworkElement stroke)stroke.BeginAnimation(UIElement.OpacityProperty,new System.Windows.Media.Animation.DoubleAnimation(hover,duration){EasingFunction=ease});
 }
 void RevealItem(object sender,RoutedEventArgs e){if(sender is System.Windows.Controls.ContentPresenter item){var owner=System.Windows.Controls.ItemsControl.ItemsControlFromItemContainer(item);RevealContainer(item,owner?.ItemContainerGenerator.IndexFromContainer(item)??0);}}
 internal static readonly DependencyProperty RevealIdProperty=DependencyProperty.RegisterAttached("RevealId",typeof(int),typeof(App),new PropertyMetadata(0));
 internal static async void FinishReveal(FrameworkElement element,TranslateTransform? move,int milliseconds){
  int id=(int)element.GetValue(RevealIdProperty)+1;element.SetValue(RevealIdProperty,id);await Task.Delay(milliseconds);
  if((int)element.GetValue(RevealIdProperty)!=id)return;
  element.BeginAnimation(UIElement.OpacityProperty,null);element.Opacity=1;if(move!=null){move.BeginAnimation(TranslateTransform.YProperty,null);move.Y=0;}
 }
 internal void RevealContainer(System.Windows.Controls.ContentPresenter item,int index){
  item.BeginAnimation(UIElement.OpacityProperty,null);item.Opacity=1;var move=item.RenderTransform as TranslateTransform??new TranslateTransform();item.RenderTransform=move;move.BeginAnimation(TranslateTransform.YProperty,null);move.Y=0;
  if(Resources["ReduceMotion"] is true)return;
  int delay=Math.Clamp(index,0,8)*38;
  item.BeginAnimation(UIElement.OpacityProperty,OrbitLauncher.MainWindow.RevealAnimation(.05,1,delay,280));
  move.BeginAnimation(TranslateTransform.YProperty,OrbitLauncher.MainWindow.RevealAnimation(22,0,delay,360));FinishReveal(item,move,delay+400);
 }
 internal void StopControlMotion(DependencyObject root){
  if(root is System.Windows.Controls.Button button&&button.RenderTransform is ScaleTransform scale){scale.BeginAnimation(ScaleTransform.ScaleXProperty,null);scale.BeginAnimation(ScaleTransform.ScaleYProperty,null);scale.ScaleX=scale.ScaleY=1;}
  if(root is System.Windows.Controls.ContentPresenter item&&item.RenderTransform is TranslateTransform move){item.BeginAnimation(UIElement.OpacityProperty,null);item.Opacity=1;move.BeginAnimation(TranslateTransform.YProperty,null);move.Y=0;}
  for(int i=0;i<VisualTreeHelper.GetChildrenCount(root);i++)StopControlMotion(VisualTreeHelper.GetChild(root,i));
 }
 async Task RunSkinHost(string dir){await Orbit.Core.SkinServer.HostAsync(dir);Shutdown();}
 protected override void OnStartup(StartupEventArgs e){base.OnStartup(e);if(e.Args.Length==2&&e.Args[0]=="--skin-host"){ShutdownMode=ShutdownMode.OnExplicitShutdown;_ = RunSkinHost(e.Args[1]);return;}bool preview=e.Args.Length>=2&&e.Args[0]=="--render-preview";
 DispatcherUnhandledException+=(s,a)=>{if(preview){File.WriteAllText(e.Args[1]+".error.txt",a.Exception.ToString());Shutdown(1);}else MessageBox.Show(a.Exception.Message,"ORBIT",MessageBoxButton.OK,MessageBoxImage.Error);a.Handled=true;};
 Resources["PopupMotion"]=System.Windows.Controls.Primitives.PopupAnimation.Fade;var window=new MainWindow(preview);if(preview){window.Left=-10000;window.Top=-10000;window.ShowActivated=false;window.ShowInTaskbar=false;}MainWindow=window;window.Show();
 if(preview)Dispatcher.BeginInvoke(new Action(async()=>{try{await window.Ready.Task.WaitAsync(TimeSpan.FromSeconds(20));if(e.Args.Length>2)await window.PreviewPageAsync(e.Args[2],Path.GetDirectoryName(Path.GetFullPath(e.Args[1])));await Task.Delay(1000);window.UpdateLayout();if(await window.CaptureMillidaPreview(e.Args[1]))return;FrameworkElement surface=e.Args.Length>2?window.PreviewSurface(e.Args[2]):window;surface.UpdateLayout();var bitmap=new RenderTargetBitmap((int)surface.ActualWidth,(int)surface.ActualHeight,96,96,PixelFormats.Pbgra32);bitmap.Render(surface);Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(e.Args[1]))!);using(var file=File.Create(e.Args[1])){var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(bitmap));png.Save(file);}}catch(Exception error){File.WriteAllText(e.Args[1]+".error.txt",error.ToString());}finally{Shutdown();}}));
 }
}

using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Orbit.Core;
namespace OrbitLauncher;
public sealed class InstanceSelectionConverter:IMultiValueConverter {
 public object Convert(object[] values,Type t,object p,CultureInfo c)=>values.Length==2&&values[0] is VersionEntry a&&values[1] is VersionEntry b&&Same(a,b)?Visibility.Visible:Visibility.Collapsed;
 internal static bool Same(VersionEntry a,VersionEntry b)=>a.Id==b.Id&&string.Equals(a.RootOverride,b.RootOverride,StringComparison.OrdinalIgnoreCase);
 public object[] ConvertBack(object value,Type[] types,object parameter,CultureInfo culture)=>throw new NotSupportedException();
}
public partial class MainWindow {
 public static readonly DependencyProperty CurrentInstanceProperty=DependencyProperty.Register(nameof(CurrentInstance),typeof(VersionEntry),typeof(MainWindow));
 public VersionEntry? CurrentInstance{get=>(VersionEntry?)GetValue(CurrentInstanceProperty);set=>SetValue(CurrentInstanceProperty,value);}
 void UpdateHero(){
  CurrentInstance=SelectedChoice;bool ru=Loc.Language=="ru";var v=SelectedChoice;
  HeroInstanceName.Text=v?.Title??(ru?"Выбери свою сборку":"Choose your instance");
  HeroInstanceInfo.Text=v==null?(ru?"Minecraft Java Edition":"Minecraft Java Edition"):v.Installed?$"Minecraft {v.BaseVersion}  ·  {v.Loader}  ·  Java {v.JavaMajor}":(ru?"Версия доступна для установки":"Available to install");
  HeroReadyLabel.Text=v?.Installed==true?(ru?"ТВОЯ СЛЕДУЮЩАЯ ИГРА":"YOUR NEXT SESSION"):(ru?"НАЧНИ НОВОЕ ПРИКЛЮЧЕНИЕ":"START YOUR NEXT ADVENTURE");
 }
 int revealGeneration;
 void RevealPageContent(FrameworkElement pageElement){
  int stamp=++revealGeneration;
  Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded,new Action(()=>{
   if(stamp!=revealGeneration||!pageElement.IsVisible)return;
   if(pageElement is ScrollViewer {Content:Panel content}){int index=0;foreach(var item in content.Children.OfType<FrameworkElement>())if(item is Border){RevealPanel(item,index++);}}
   foreach(var control in new[]{HomeVersions,AllVersions,ModsResults})if(control.IsVisible){for(int i=0;i<Math.Min(control.Items.Count,16);i++)if(control.ItemContainerGenerator.ContainerFromIndex(i) is ContentPresenter item)((App)Application.Current).RevealContainer(item,i);}
  }));
 }
 internal static DoubleAnimationUsingKeyFrames RevealAnimation(double start,double finish,int delay,int duration)=>new(){Duration=TimeSpan.FromMilliseconds(delay+duration),FillBehavior=FillBehavior.Stop,KeyFrames=new DoubleKeyFrameCollection{new DiscreteDoubleKeyFrame(start,KeyTime.FromTimeSpan(TimeSpan.Zero)),new DiscreteDoubleKeyFrame(start,KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(delay))),new EasingDoubleKeyFrame(finish,KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(delay+duration)),new CubicEase{EasingMode=EasingMode.EaseOut})}};
 void RevealPanel(FrameworkElement element,int index){
  element.BeginAnimation(OpacityProperty,null);element.Opacity=1;
  if(element.RenderTransform is not TranslateTransform)element.RenderTransform=new TranslateTransform();var move=(TranslateTransform)element.RenderTransform;move.BeginAnimation(TranslateTransform.YProperty,null);move.Y=0;
  if(settings.ReduceMotion)return;
  int delay=Math.Min(index,5)*45;
  element.BeginAnimation(OpacityProperty,RevealAnimation(.15,1,delay,300));
  move.BeginAnimation(TranslateTransform.YProperty,RevealAnimation(20,0,delay,390));App.FinishReveal(element,move,delay+430);
 }
}

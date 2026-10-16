using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Media;
using Orbit.Core;
namespace OrbitLauncher;
public partial class MainWindow {
 static IEnumerable<DependencyObject> Visuals(DependencyObject parent){for(int i=0;i<VisualTreeHelper.GetChildrenCount(parent);i++){var child=VisualTreeHelper.GetChild(parent,i);yield return child;foreach(var sub in Visuals(child))yield return sub;}}
 async Task CheckUpgradeUiAsync(string root){var report=new List<string>();void Check(bool condition,string title){if(!condition)throw new Exception("Upgrade UI: "+title);report.Add("PASS "+title);}
 Loc.Language="en";Loc.ApplyResources();Navigate("Settings");Check(PageTitle.Text=="Settings","English page title");Check(!Application.Current.Resources.Keys.OfType<string>().Where(k=>Regex.IsMatch(k,"^L[0-9]+$")).Any(k=>Regex.IsMatch((string)Application.Current.Resources[k],"[А-Яа-яёЁ]")),"All static UI labels have English translations");Loc.Language="ru";Loc.ApplyResources();Navigate("Settings");Check(PageTitle.Text=="Настройки","Russian switch updates reactive labels");Loc.Language="en";Loc.ApplyResources();Navigate("Library");UpdateLayout();await Task.Delay(400);
 var folder=Visuals(AllVersions).OfType<Button>().First(b=>b.Tag is VersionEntry&&b.ToolTip?.ToString()=="Open instance folder");Check(folder.Content is System.Windows.Controls.Viewbox,"Real vector folder icons on instance cards");
 var card=Visuals(AllVersions).OfType<Button>().First(b=>b.Tag is VersionEntry&&b.Style==FindResource("CardButton"));typeof(App).GetMethod("ButtonEnter",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(Application.Current,new object[]{card,new System.Windows.Input.MouseEventArgs(System.Windows.Input.Mouse.PrimaryDevice,0)});await Task.Delay(220);Check(card.RenderTransform is ScaleTransform scale&&Math.Abs(scale.ScaleX-1)<.0001,"Hovered cards do not expand outside layout");
 await LoadCatalogAsync();VersionPopup.IsOpen=true;await Task.Delay(200);var content=(FrameworkElement)VersionPopup.Child;content.Measure(new Size(580,520));content.Arrange(new Rect(content.DesiredSize));content.UpdateLayout();var thumb=Visuals(content).OfType<Thumb>().First(x=>x.ActualHeight>0);Check(thumb.ActualHeight>=30,"Picker scrollbar thumb remains usable");VersionPopup.IsOpen=false;
 var monitor=MonitorFromWindow(new WindowInteropHelper(this).Handle,2);var info=new MonitorInfo{Size=Marshal.SizeOf<MonitorInfo>()};GetMonitorInfo(monitor,ref info);var ptr=Marshal.AllocHGlobal(Marshal.SizeOf<MinMaxInfo>());try{Marshal.StructureToPtr(new MinMaxInfo(),ptr,false);bool handled=false;WindowBoundsHook(new WindowInteropHelper(this).Handle,0x0024,IntPtr.Zero,ptr,ref handled);var mm=Marshal.PtrToStructure<MinMaxInfo>(ptr);Check(handled&&mm.MaxSize.Y==info.Work.Bottom-info.Work.Top,"Maximize uses monitor work area and respects taskbar");}finally{Marshal.FreeHGlobal(ptr);}
 catalogType="modpack";Navigate("Mods");await modsLoadTask;Check(ModsResults.Items.Count>0,"Live modpack catalog populated");Check(ModCanInstall,"Modpacks install as independent instances");Check(ModsCategory.Items.Count>1,"Modpack categories populated");catalogType="mod";VersionPopup.IsOpen=false;Navigate("Home");File.WriteAllLines(Path.Combine(root,"upgrade-ui-checks.txt"),report);
 }
}

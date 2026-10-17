using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Orbit.Core;
namespace OrbitLauncher;
public partial class MainWindow {
 CancellationTokenSource? exportCts;
 async internal void ExportPack_Click(object sender,RoutedEventArgs e){
  e.Handled=true;if(exportCts!=null)return;
  var v=(sender as Button)?.Tag as VersionEntry??SelectedChoice;if(v==null||!v.Installed)return;
  if(installing||modBusy||launching||game is {HasExited:false}){Status(Loc.Language=="ru"?"Заверши установку и закрой игру перед экспортом.":"Finish installation and close the game before exporting.",true);return;}
  var name=string.Concat(v.Title.Select(c=>Path.GetInvalidFileNameChars().Contains(c)?'_':c));
  var dialog=new SaveFileDialog{Title=Loc.Language=="ru"?"Экспорт сборки":"Export modpack",Filter="ZIP (*.zip)|*.zip",FileName=name+".zip",AddExtension=true,DefaultExt=".zip"};if(dialog.ShowDialog(this)!=true)return;
  using var cts=new CancellationTokenSource();exportCts=cts;Navigate("Library");ExportPanel.Visibility=Visibility.Visible;ExportCancel.IsEnabled=true;ExportBar.Value=0;
  string source=LaunchEngine.InstanceDirectory(SettingsFor(v),v.Id);
  var progress=new Progress<ExportProgress>(p=>{ExportBar.Value=p.TotalBytes==0?0:100d*p.Bytes/p.TotalBytes;ExportStatus.Text=(Loc.Language=="ru"?"Упаковка ":"Packing ")+ $"{p.Files}/{p.TotalFiles} · {p.Bytes/1048576d:0.0}/{p.TotalBytes/1048576d:0.0} MB";});
  try{var result=await Task.Run(()=>PackExporter.ExportAsync(source,dialog.FileName,v,progress,cts.Token));Status((Loc.Language=="ru"?"Сборка экспортирована: ":"Modpack exported: ")+dialog.FileName);MessageBox.Show(this,(Loc.Language=="ru"?"Готово! ZIP содержит файлы сборки и инструкцию для друга.\n\n":"Done! The ZIP contains the pack files and instructions for your friend.\n\n")+dialog.FileName,"ORBIT",MessageBoxButton.OK,MessageBoxImage.Information);}
  catch(OperationCanceledException){Status(Loc.Language=="ru"?"Экспорт отменён.":"Export cancelled.");}catch(Exception ex){Status(ex.Message,true);MessageBox.Show(this,ex.Message,"ORBIT",MessageBoxButton.OK,MessageBoxImage.Error);}
  finally{exportCts=null;ExportPanel.Visibility=Visibility.Collapsed;}
 }
 void ExportCancel_Click(object sender,RoutedEventArgs e){exportCts?.Cancel();ExportCancel.IsEnabled=false;}
}

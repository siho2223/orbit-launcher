using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Orbit.Core;
namespace OrbitLauncher;
public partial class MainWindow {
 ModrinthClient modrinth=new();
 CancellationTokenSource? modsSearchCts,modInstallCts;
 readonly DispatcherTimer modsDebounce=new(){Interval=TimeSpan.FromMilliseconds(450)};
 Task modsLoadTask=Task.CompletedTask;
 bool modsReady,modsUpdating,modBusy;
 string catalogType="mod";readonly Dictionary<string,IReadOnlyList<ModCategory>> categoryCache=new();
 int modsOffset,modsTotal,modsGeneration,modsPageGeneration;
 public static readonly DependencyProperty ModCanInstallProperty=DependencyProperty.Register(nameof(ModCanInstall),typeof(bool),typeof(MainWindow),new PropertyMetadata(false));
 public bool ModCanInstall {get=>(bool)GetValue(ModCanInstallProperty);set=>SetValue(ModCanInstallProperty,value);}
 static string ModGameVersion(VersionEntry v){if(Regex.IsMatch(v.BaseVersion,@"^\d+\.\d+(\.\d+)?$|^\d\dw\d\d[a-z]$"))return v.BaseVersion;var match=Regex.Match(v.BaseVersion,@"(?<!\d)\d+\.\d+(?:\.\d+)?");return match.Success?match.Value:v.BaseVersion;}
 void RefreshModInstallState(){
  ModCanInstall=!modBusy&&!installing&&!launching&&(catalogType=="modpack"||SelectedChoice is {Installed:true} v&&(catalogType!="mod"||v.Loader is "Fabric" or "Forge" or "NeoForge" or "Quilt"));
  if(ModsTypeButton==null)return;
  foreach(var button in new[]{ModsTypeButton,PacksTypeButton,ModpacksTypeButton,ShadersTypeButton}){
   bool active=(string)button.Tag==catalogType;button.IsEnabled=!modBusy;
   button.SetResourceReference(Button.BackgroundProperty,active?"Brush173F38":"Brush1C2D3B");
   button.SetResourceReference(Button.ForegroundProperty,active?"Brush00E5A0":"BrushE2EDF1");
  }
 }
 async Task LoadContentCategoriesAsync(){
  string type=catalogType;int stamp=modsPageGeneration;string selected=(ModsCategory.SelectedItem as ModCategory)?.Id??"";
  try{
   if(!categoryCache.TryGetValue(type,out var categories)){categories=await modrinth.CategoriesAsync(catalogCts.Token,type);categoryCache[type]=categories;}
   if(type!=catalogType||stamp!=modsPageGeneration)return;
   modsUpdating=true;try{var items=new[]{new ModCategory("",Loc.T("Все категории"))}.Concat(categories).ToArray();ModsCategory.ItemsSource=items;ModsCategory.SelectedItem=items.FirstOrDefault(x=>x.Id==selected)??items[0];}finally{modsUpdating=false;}
  }catch(OperationCanceledException){}catch(Exception ex){Log("Modrinth categories: "+ex.Message);/* Search is independent of category metadata. */}
 }
 async void ModsRetry_Click(object sender,RoutedEventArgs e){modsLoadTask=LoadModsPageAsync();await modsLoadTask;}
 async void CatalogType_Click(object sender,RoutedEventArgs e){if(modBusy||((Button)sender).Tag is not string type||type==catalogType)return;catalogType=type;++modsGeneration;modsSearchCts?.Cancel();modsDebounce.Stop();modsUpdating=true;ModsQuery.Text="";ModsResults.ItemsSource=null;modsUpdating=false;modsLoadTask=LoadModsPageAsync();await modsLoadTask;}
 Task OpenModsAsync(){modsLoadTask=LoadModsPageAsync();return modsLoadTask;}
 async Task LoadModsPageAsync(){
  int stamp=++modsPageGeneration;modsSearchCts?.Cancel();++modsGeneration;
  try{
   if(!modsReady){modsReady=true;modsUpdating=true;ModsCategory.ItemsSource=new[]{new ModCategory("",Loc.T("Все категории"))};ModsCategory.SelectedIndex=0;ModsSort.ItemsSource=new[]{new ModCategory("relevance",Loc.T("По релевантности")),new ModCategory("downloads",Loc.T("Популярные")),new ModCategory("updated",Loc.T("Обновлённые")),new ModCategory("newest",Loc.T("Новые"))};ModsSort.SelectedIndex=1;modsUpdating=false;modsDebounce.Tick+=async(s,e)=>{modsDebounce.Stop();modsLoadTask=SearchModsAsync(true);await modsLoadTask;};
   }
   RefreshModInstallState();
   var categories=LoadContentCategoriesAsync();
   if(stamp==modsPageGeneration)await SearchModsAsync(true);
   await categories;
  }catch(OperationCanceledException){}catch(Exception e){ModsStatus.Text=e.Message;}
 }
 async Task SearchModsAsync(bool reset){if(modsUpdating)return;var v=SelectedChoice??new VersionEntry("","Vanilla","",0);modsDebounce.Stop();modsSearchCts?.Cancel();var cts=CancellationTokenSource.CreateLinkedTokenSource(catalogCts.Token);modsSearchCts=cts;int stamp=++modsGeneration;try{if(reset)modsOffset=0;RefreshModInstallState();string type=catalogType;bool pack=type=="resourcepack",shader=type=="shader";ModsQueryHint.Text=type=="modpack"?Loc.T(Loc.T("Поиск сборок на Modrinth…")):shader?Loc.T("Поиск шейдеров на Modrinth…"):pack?Loc.T("Поиск ресурспаков на Modrinth…"):Loc.T("Поиск модов на Modrinth…");string gameVersion=type=="modpack"?"":ModGameVersion(v);string? loader=type!="mod"||v.Loader=="Vanilla"?null:v.Loader.ToLowerInvariant();ModsTarget.Text=gameVersion+" · "+(shader?Loc.T("Шейдеры"):pack?Loc.T("Ресурспаки"):v.Loader)+" · "+(v.Installed?Loc.T("выбранная сборка"):Loc.T("версия ещё не установлена"));ModsStatus.Text=Loc.T("Поиск на Modrinth…");ModsPrevious.IsEnabled=false;ModsNext.IsEnabled=false;var results=await modrinth.SearchAsync(ModsQuery.Text.Trim(),gameVersion,loader,(ModsCategory.SelectedItem as ModCategory)?.Id??"",modsOffset,(ModsSort.SelectedItem as ModCategory)?.Id??"downloads",cts.Token,type);if(stamp!=modsGeneration||type!=catalogType||cts.IsCancellationRequested)return;ModsResults.ItemsSource=results.Projects;modsTotal=results.Total;if(type=="modpack")ModsTarget.Text=Loc.T(Loc.T("Сборки Modrinth · отдельные игровые профили"));ModsScroll.ScrollToTop();ModsCounter.Text=results.Total==0?Loc.T("Ничего не найдено"):Loc.T($"{modsOffset+1}–{modsOffset+results.Projects.Count} из {results.Total:N0}");ModsPrevious.IsEnabled=modsOffset>0;ModsNext.IsEnabled=modsOffset+20<modsTotal;ModsStatus.Text=type=="modpack"?Loc.T(Loc.T("Сборки устанавливаются отдельно и появляются в библиотеке. Устанавливается последняя опубликованная версия сборки.")):shader?Loc.T($"Шейдеры для Minecraft {gameVersion}. Для работы нужен Iris или OptiFine. Установка ZIP не устанавливает загрузчик шейдеров."):pack?Loc.T($"Ресурспаки для Minecraft {gameVersion}. После установки включи пак в настройках игры → Ресурспаки."):v.Loader=="Vanilla"?Loc.T("Для установки выбери сборку Fabric, Forge, NeoForge или Quilt в нижней панели. Сейчас показан каталог для этой версии Minecraft."):Loc.T($"Совместимые моды для {gameVersion} / {v.Loader}. Обязательные зависимости устанавливаются автоматически.");if(gameVersion.Length==0&&type!="modpack"){ModsTarget.Text=Loc.T("Все версии Minecraft · просмотр каталога");ModsStatus.Text=Loc.T("Выбери версию игры в нижней панели для совместимости и установки дополнений.");}if(results.Total==0)ModsStatus.Text+=Loc.T(" Попробуй другую категорию или запрос.");}catch(OperationCanceledException){}catch(Exception e){if(stamp==modsGeneration){ModsStatus.Text=Loc.T("Не удалось загрузить каталог: ")+e.Message;ModsResults.ItemsSource=null;ModsCounter.Text="";}}finally{if(ReferenceEquals(modsSearchCts,cts))modsSearchCts=null;cts.Dispose();}}
 void ModsQuery_Changed(object sender,TextChangedEventArgs e){if(!initialized||!modsReady||modsUpdating)return;++modsGeneration;modsSearchCts?.Cancel();modsDebounce.Stop();modsDebounce.Start();}
 async void ModsFilter_Changed(object sender,SelectionChangedEventArgs e){if(initialized&&modsReady&&!modsUpdating){modsLoadTask=SearchModsAsync(true);await modsLoadTask;}}
 async void ModsPrevious_Click(object sender,RoutedEventArgs e){modsOffset=Math.Max(0,modsOffset-20);await SearchModsAsync(false);}
 async void ModsNext_Click(object sender,RoutedEventArgs e){modsOffset+=20;await SearchModsAsync(false);}
 void ModsInstalled_Click(object sender,RoutedEventArgs e){ModsPage.Visibility=Visibility.Collapsed;FilesPage.Visibility=Visibility.Visible;ShowFiles();}
 void ModsCancel_Click(object sender,RoutedEventArgs e)=>modInstallCts?.Cancel();
 void ModWebsite_Click(object sender,RoutedEventArgs e){if(((Button)sender).Tag is ModProject project){var url="https://modrinth.com/"+catalogType+"/"+Uri.EscapeDataString(project.Id);try{Process.Start(new ProcessStartInfo(url){UseShellExecute=true});}catch(Exception ex){ModsStatus.Text=ex.Message;}}}
 async void ModInstall_Click(object sender,RoutedEventArgs e){if(exportCts!=null){Status(Loc.Language=="ru"?"Дождись завершения экспорта.":"Wait for the export to finish.");return;}if(!ModCanInstall||((Button)sender).Tag is not ModProject project)return;if(catalogType=="modpack"){await InstallModpackAsync(project);return;}if(SelectedChoice is not {} v)return;string type=catalogType;bool pack=type=="resourcepack",shader=type=="shader";modBusy=true;installing=true;modInstallCts=CancellationTokenSource.CreateLinkedTokenSource(catalogCts.Token);var ct=modInstallCts.Token;string target=LaunchEngine.InstanceDirectory(SettingsFor(v),v.Id);RefreshModInstallState();UpdateLaunchButton();VersionCombo.IsEnabled=false;ModsCancel.Visibility=Visibility.Visible;DownloadPanel.Visibility=Visibility.Visible;DownloadTitle.Text=project.Title;DownloadDetail.Text=Loc.T("Подбор версии и обязательных зависимостей…");DownloadBar.Value=0;DownloadPercent.Text="";ModsStatus.Text=Loc.T("Подбор совместимых файлов: ")+project.Title;
  try{var plan=await modrinth.ResolveAsync(project.Id,ModGameVersion(v),shader?"iris":pack?"minecraft":v.Loader,ct,type);ModsStatus.Text=Loc.T($"Установка {project.Title} · файлов: {plan.Files.Count}");await modrinth.InstallAsync(plan,target,new Progress<ModProgress>(p=>{DownloadTitle.Text=p.Stage;DownloadDetail.Text=$"Modrinth · {Bytes(p.Done)} / {Bytes(p.Total)}";DownloadBar.Value=p.Total>0?100d*p.Done/p.Total:0;DownloadPercent.Text=$"{DownloadBar.Value:0}%";}),ct);DownloadBar.Value=100;DownloadPercent.Text="100%";DownloadTitle.Text=Loc.T("Установлено · ")+project.Title;DownloadDetail.Text=Loc.T($"Файлов: {plan.Files.Count} · {v.Loader} / {ModGameVersion(v)}");ModsStatus.Text=type=="modpack"?Loc.T(Loc.T("Сборки устанавливаются отдельно и появляются в библиотеке. Устанавливается последняя опубликованная версия сборки.")):shader?project.Title+Loc.T(" установлен в shaderpacks. Включи его в меню шейдеров Iris или OptiFine."):pack?project.Title+Loc.T(" установлен. Включи его в Minecraft: Настройки → Ресурспаки."):project.Title+Loc.T(" установлен вместе с обязательными зависимостями. Можно запускать сборку.");Status((shader?Loc.T("Шейдер установлен в "):pack?Loc.T("Ресурспак установлен в "):Loc.T("Мод установлен в "))+v.Id);}
  catch(OperationCanceledException){ModsStatus.Text=Loc.T("Установка отменена. Незавершённые файлы не добавлены в сборку.");DownloadTitle.Text=Loc.T("Загрузка модов отменена");DownloadDetail.Text="";}
  catch(Exception ex){ModsStatus.Text=Loc.T("Не удалось установить: ")+ex.Message;DownloadTitle.Text=Loc.T("Ошибка установки дополнения");DownloadDetail.Text=ex.Message;Status(Loc.T("Дополнение не установлено. Подробности во вкладке «Моды»."),true);}
  finally{modInstallCts.Dispose();modInstallCts=null;modBusy=false;installing=false;VersionCombo.IsEnabled=true;ModsCancel.Visibility=Visibility.Collapsed;CancelDownloadButton.Content=Loc.T("Скрыть");RefreshModInstallState();UpdateLaunchButton();}
 }
}

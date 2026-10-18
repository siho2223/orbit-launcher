using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
namespace OrbitLauncher;
public partial class MainWindow {
 static readonly string[] ThemeOriginalColors={"#0000E5A0","#00E5A0","#062C23","#08FFFFFF","#092A28","#0A1018","#0B111B","#0BFFFFFF","#0CFFFFFF","#0E1B26","#101416","#101925","#102532","#102E31","#111A14","#112630","#12202E","#122432","#122B29","#12322B","#141E25","#142332","#142A34","#152331","#152332","#152510","#15252B","#152832","#163D35","#166B70","#172027","#172C40","#173F38","#18232E","#192A23","#192A2A","#1B2D3A","#1C2428","#1C2D3B","#20282C","#223341","#22A6C2D6","#244653","#263644","#263C6154","#263D4A","#294635","#2A3436","#2A534B","#2B3B3C","#2B3F4B","#2C4353","#2D3834","#2F4540","#3000E5A0","#314B35","#344139","#354A31","#355E66","#3B4A3F","#3C5D44","#3D494F","#405348","#407168","#41564A","#426C7C67","#506B53","#50717B","#56705F","#567D9C98","#62756D","#6400E5A0","#6D8174","#6F897B","#70887A","#708A7B","#708F87","#70ACBC","#71877A","#718F80","#71BACA","#77877E","#798681","#8298A8","#83968A","#839C91","#85988A","#85988B","#889A8D","#8DA592","#8DA699","#8DA69A","#8E9E9B","#8F9F96","#91F4CC","#94ABA0","#97ACB9","#99B0A4","#9AAFA6","#9BE6CA","#9C8BF5D1","#9EB3A8","#A3C398","#A4B7D4","#A5B9AA","#A7C1AF","#A9C5C8","#AB8BF5D1","#B8FFE1","#D3F9B4","#DCE5DF","#E2EDF1","#E4EAEB","#E8F0EA","#EDF5FA","#EEF3F0","#F2F4F7"};
 bool changingTheme;
 record ThemeChoice(string Id,string Title);
 void InitializeThemes(){
  InitializeBrawlInteractions();InitializeXmcl();InitializeVersionArtwork();
  if(settings.ThemeRevision<1){settings.Theme="brawl stars";settings.ThemeRevision=1;if(!suppressProfileSave)settings.Save();}
  changingTheme=true;
  ThemeBox.ItemsSource=new[]{new ThemeChoice("brawl stars","Default"),new ThemeChoice("default","Old theme")};
  if(settings.Theme is not("default" or "brawl stars"))settings.Theme="default";
  if(settings.ThemePalette is not("purple" or "green" or "orange" or "blue" or "pink"))settings.ThemePalette="purple";
  ThemeBox.SelectedIndex=settings.Theme=="brawl stars"?0:1;
  UpdateThemeLabels();changingTheme=false;ApplyTheme();
 }
 void UpdateThemeLabels(){
  UpdateXmclLabels();var r=Application.Current.Resources;bool ru=Loc.Language=="ru";
  r["ExportLabel"]=ru?"Экспорт ZIP":"Export ZIP";r["ExportTip"]=ru?"Упаковать моды, конфиги, шейдеры и ресурспаки":"Pack mods, configs, shaders and resource packs";r["ExportCancelLabel"]=ru?"Отмена":"Cancel";
  r["AllVersionsLabel"]=ru?"Все версии":"All versions";
  r["ThemeLabel"]=ru?"Оформление":"Appearance";
  r["ThemeHelp"]=ru?"Default — игровое лобби; Old theme — классический интерфейс.":"Default — game lobby; Old theme — classic interface.";
  r["PaletteLabel"]=ru?"Палитра Default":"Default palette";
  r["ThemeRetry"]=ru?"Повторить поиск":"Retry search";
  r["ThemeExplore"]=ru?"Открыть сборки":"Browse packs";
  r["LobbyPacks"]=ru?"Новая сборка?":"Your next modpack?";
  r["LobbyToday"]=ru?"СЕГОДНЯ ИГРАЕМ":"PLAYING TODAY";
  r["DragHint"]=ru?"Зажми персонажа и двигай мышью":"Drag the character to rotate";
  r["EmoteWave"]=ru?"Приветствие":"Wave";r["EmoteDance"]=ru?"Танец":"Dance";r["EmoteCheer"]=ru?"Радость":"Cheer";
  r["ThemeLibrary"]=ru?"УСТАНОВЛЕННЫЕ ВЕРСИИ":"INSTALLED VERSIONS";
  if(PaletteBox==null)return;bool was=changingTheme;changingTheme=true;
  PaletteBox.ItemsSource=new[]{new ThemeChoice("purple",ru?"Фиолетовая":"Purple"),new ThemeChoice("green",ru?"Зелёная":"Green"),new ThemeChoice("orange",ru?"Оранжевая":"Orange"),new ThemeChoice("blue",ru?"Синяя":"Blue"),new ThemeChoice("pink",ru?"Розовая":"Pink")};
  PaletteBox.SelectedItem=((ThemeChoice[])PaletteBox.ItemsSource).FirstOrDefault(x=>x.Id==settings.ThemePalette)??((ThemeChoice[])PaletteBox.ItemsSource)[0];changingTheme=was;
 }
 void Theme_Changed(object sender,SelectionChangedEventArgs e){
  if(changingTheme||ThemeBox?.SelectedItem is not ThemeChoice theme||PaletteBox?.SelectedItem is not ThemeChoice palette)return;
  settings.Theme=theme.Id;settings.ThemePalette=palette.Id;ApplyTheme();if(!suppressProfileSave)settings.Save();
 }
 static Color ThemeColor(string hex)=>(Color)ColorConverter.ConvertFromString(hex);
 void ApplyTheme(){
  if(settings.Theme=="default 2")settings.Theme="default";
  bool brawl=settings.Theme=="brawl stars";
  bool wasChanging=changingTheme;changingTheme=true;ThemeBox.SelectedIndex=brawl?0:1;changingTheme=wasChanging;
  string accent=settings.ThemePalette switch{"green"=>"#B9ED32","orange"=>"#FFB84D","blue"=>"#65CFFF","pink"=>"#FF69AD",_=>"#B38AFF"};
  var resources=Application.Current.Resources;
  resources["CardFrameThickness"]=brawl?new Thickness(6,5,6,9):new Thickness(1);resources["BrawlEnabled"]=brawl;resources["ThemePanelThickness"]=brawl?new Thickness(6,5,6,9):new Thickness(0);
  resources["LobbyAccent"]=new SolidColorBrush(ThemeColor(accent));
  foreach(var hex in ThemeOriginalColors){
   var c=ThemeColor(hex);var mapped=c;
   if(brawl){
    var hue=ThemeColor(settings.ThemePalette switch{"green"=>"#1C3428","orange"=>"#38291F","blue"=>"#1A2944","pink"=>"#382238",_=>"#29223F"});
    if(hex=="#00E5A0")mapped=ThemeColor(accent);
    else if(hex=="#062C23")mapped=ThemeColor("#1D1829");
    else if(c.A==255&&Math.Max(c.R,Math.Max(c.G,c.B))<105){double light=.6+(c.R+c.G+c.B)/200d;mapped=Color.FromArgb(242,(byte)Math.Min(105,hue.R*light),(byte)Math.Min(105,hue.G*light),(byte)Math.Min(125,hue.B*light));}
   }
   if(!brawl){
    if(hex=="#00E5A0")mapped=ThemeColor("#4CAF50");
    else if(hex=="#062C23")mapped=Colors.White;
    else if(c.A==255&&Math.Max(c.R,Math.Max(c.G,c.B))<105){byte level=(byte)Math.Clamp((c.R+c.G+c.B)/3,20,53);mapped=Color.FromArgb(232,level,(byte)(level+2),level);}
    else if(hex=="#9BE6CA"||hex=="#91F4CC"||hex=="#B8FFE1")mapped=ThemeColor("#A5D6A7");
   }
   resources["Brush"+hex[1..]]=new SolidColorBrush(mapped);resources["Color"+hex[1..]]=mapped;
  }
  foreach(int radius in new[]{2,3,4,5,8,9,10,11,12,13,14,16,18,22})resources["Radius"+radius]=new CornerRadius(brawl?3:Math.Min(radius,8));
  PlayButton.Effect=new System.Windows.Media.Effects.DropShadowEffect{Color=ThemeColor("#4CAF50"),Opacity=.12,BlurRadius=12,ShadowDepth=0};
  resources["ThemeButtonBorder"]=new Thickness(1);resources["ThemeButtonWeight"]=FontWeights.Normal;
  resources["ReduceMotion"]=settings.ReduceMotion;
  LobbyBackground.Palette=settings.ThemePalette;
  PalettePanel.Visibility=brawl?Visibility.Visible:Visibility.Collapsed;
  UpdateBrawlResources(brawl);ThemeBackdrop.Palette=settings.ThemePalette;RefreshMinecraftIcons();ApplyThemeLayout();RefreshModInstallState();
 }
 void ApplyThemeLayout(){
  if(LobbyPage==null)return;
  bool home=page=="Home",brawl=settings.Theme=="brawl stars",lobby=home&&brawl;
  LobbyPage.Visibility=lobby?Visibility.Visible:Visibility.Collapsed;
  HomePage.Visibility=home&&!lobby?Visibility.Visible:Visibility.Collapsed;
  SidebarColumn.Width=new GridLength(brawl?0:204);
  SidebarShell.Visibility=brawl?Visibility.Collapsed:Visibility.Visible;
  ThemeBackdrop.Visibility=brawl&&!home?Visibility.Visible:Visibility.Collapsed;
  BrawlHeading.Visibility=brawl&&!home?Visibility.Visible:Visibility.Collapsed;
  BrawlProfilePanel.Visibility=brawl?Visibility.Visible:Visibility.Collapsed;
  SettingsContent.HorizontalAlignment=brawl?HorizontalAlignment.Center:HorizontalAlignment.Left;
  PageHeading.Visibility=brawl?Visibility.Collapsed:Visibility.Visible;
  PageHost.RowDefinitions[0].Height=new GridLength(lobby?0:70);
  PageHost.Margin=lobby?new Thickness(0):brawl?new Thickness(24,12,24,14):new Thickness(34,23,30,15);
  LaunchBar.Visibility=brawl?Visibility.Collapsed:Visibility.Visible;
  BrawlFooter.Visibility=brawl&&!home?Visibility.Visible:Visibility.Collapsed;
  LaunchRow.Height=lobby?new GridLength(0):brawl?new GridLength(88):new GridLength(135);
  VersionPopup.PlacementTarget=lobby?LobbyVersionButton:brawl?BrawlFooterVersion:VersionPickerButton;
  if(brawl){WindowBrand.Text="ORBIT LAUNCHER";WindowBrand.HorizontalAlignment=HorizontalAlignment.Center;}else{WindowBrand.SetResourceReference(TextBlock.TextProperty,"L2");WindowBrand.HorizontalAlignment=HorizontalAlignment.Left;}
  LaunchBar.BorderThickness=new Thickness(0,lobby?3:1,0,0);
  PlayButton.FontSize=18;
  UpdateLobbyIdle();
  if(!lobby)LobbySkinViewport.StopEmote();
  if(lobby)RefreshSkin();
  ApplyXmclLayout();UpdateMillidaVisibility();
 }
 void LobbyTurn_Click(object sender,RoutedEventArgs e)=>LobbySkinViewport.Turn(double.Parse((string)((Button)sender).Tag,System.Globalization.CultureInfo.InvariantCulture));
 void LobbyPacks_Click(object sender,RoutedEventArgs e){catalogType="modpack";Navigate("Mods");}
}

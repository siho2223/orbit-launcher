using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
namespace OrbitLauncher;
public partial class MainWindow {
 async Task CheckPresetDropdownsAsync(string root){var report=new List<string>();void Check(bool value,string label){if(!value)throw new Exception(label);report.Add("PASS "+label);}
 Navigate("Settings");LanguageBox.IsDropDownOpen=true;UpdateLayout();await Task.Delay(200);for(int i=0;i<LanguageBox.Items.Count;i++){var item=(ComboBoxItem)LanguageBox.ItemContainerGenerator.ContainerFromIndex(i);item.ApplyTemplate();item.UpdateLayout();var texts=Visuals(item).OfType<TextBlock>().Select(t=>t.Text).ToArray();Check(texts.Any(t=>t is "English" or "Русский")&&!texts.Any(t=>t.Contains("LanguageChoice")),"Language option renders its title, not record data");}LanguageBox.IsDropDownOpen=false;
 Navigate("Skins");RefreshPresets();UpdateLayout();SkinsPage.ScrollToBottom();if(PresetBox.Items.Count>0){PresetBox.IsDropDownOpen=true;await Task.Delay(200);UpdateLayout();var item=(ComboBoxItem)PresetBox.ItemContainerGenerator.ContainerFromIndex(0);item.ApplyTemplate();item.UpdateLayout();Check(!Visuals(item).OfType<TextBlock>().Any(t=>t.Text.Contains("SkinPreset {")),"Preset dropdown hides internal ID/model record");PresetBox.IsDropDownOpen=false;Check(Visuals(PresetCards).OfType<SkinPresetThumbnail>().Any(),"Saved presets render front-facing character thumbnails");Check(Visuals(PresetCards).OfType<Button>().Any(b=>b.Tag is Orbit.Core.SkinPreset),"Preset cards carry their skin selection");}
 File.WriteAllLines(Path.Combine(root,"preset-ui-checks.txt"),report);
 }
}

using System.Text.Json;
namespace Orbit.Core;
public static class Localization {
 public static string Language {get;set;}="en";
 static readonly Dictionary<string,string> map=JsonSerializer.Deserialize<Dictionary<string,string>>(typeof(Localization).Assembly.GetManifestResourceStream("Orbit.Core.Translations.json")!)!;
 static readonly (string From,string To)[] fragments=map.Select(p=>(p.Key,p.Value)).OrderByDescending(p=>p.Key.Length).ToArray();
 public static string T(string text){if(Language=="ru")return text;if(map.TryGetValue(text,out var translated))return translated;foreach(var (from,to) in fragments)text=text.Replace(from,to,StringComparison.Ordinal);return text;}
}

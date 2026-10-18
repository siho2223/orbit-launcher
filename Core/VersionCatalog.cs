namespace Orbit.Core;
public static class VersionCatalog {
 public static List<VersionEntry> ForLoader(IEnumerable<RemoteVersion> catalog,IEnumerable<VersionEntry> installed,IEnumerable<string> supported,string loader){if(loader is not("Forge" or "Fabric"))throw new ArgumentException("Unsupported loader",nameof(loader));var allowed=supported.ToHashSet(StringComparer.Ordinal);var profiles=installed.Where(v=>v.Installed&&v.Loader==loader).ToArray();var result=new List<VersionEntry>();foreach(var remote in catalog.Where(v=>v.Type=="release"&&allowed.Contains(v.Id))){var matches=profiles.Where(v=>v.BaseVersion==remote.Id).ToArray();if(matches.Length>0)result.AddRange(matches);else result.Add(new VersionEntry(remote.Id,loader,remote.Id,0){Installed=false,Remote=remote});}return result;}
}

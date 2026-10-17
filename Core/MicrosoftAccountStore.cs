using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text.Json;
namespace Orbit.Core;
[SupportedOSPlatform("windows")]
public static class MicrosoftAccountStore {
 static byte[] Entropy=>System.Text.Encoding.UTF8.GetBytes("ORBIT.MicrosoftAccount.v1");
 static string FilePath(string? root)=>Path.Combine(root??Path.Combine(LauncherSettings.DataRoot,"auth"),"microsoft.bin");
 public static void Save(MicrosoftSession session,string? root=null){string path=FilePath(root);Directory.CreateDirectory(Path.GetDirectoryName(path)!);byte[] clear=JsonSerializer.SerializeToUtf8Bytes(session);byte[] encrypted;try{encrypted=ProtectedData.Protect(clear,Entropy,DataProtectionScope.CurrentUser);}finally{CryptographicOperations.ZeroMemory(clear);}string temporary=path+"."+Guid.NewGuid().ToString("N")+".tmp";try{File.WriteAllBytes(temporary,encrypted);File.Move(temporary,path,true);}finally{if(File.Exists(temporary))File.Delete(temporary);}}
 public static MicrosoftSession? Load(string? root=null){string path=FilePath(root);if(!File.Exists(path))return null;if(new FileInfo(path).Length>1024*1024)throw new IOException("Invalid encrypted account store.");byte[] clear=ProtectedData.Unprotect(File.ReadAllBytes(path),Entropy,DataProtectionScope.CurrentUser);try{return JsonSerializer.Deserialize<MicrosoftSession>(clear);}finally{CryptographicOperations.ZeroMemory(clear);}}
 public static void Delete(string? root=null){string path=FilePath(root);if(File.Exists(path))File.Delete(path);}
}

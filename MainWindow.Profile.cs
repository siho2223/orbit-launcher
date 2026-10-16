using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Threading;
using Orbit.Core;
namespace OrbitLauncher;
public partial class MainWindow {
 bool suppressProfileSave;string? lastSavedNickname;
 string? profileSettingsRoot;
 readonly DispatcherTimer nicknameSaveTimer=new(){Interval=TimeSpan.FromMilliseconds(400)};
 bool nicknameTimerReady;
 void Nickname_Changed(object sender,TextChangedEventArgs e){if(!initialized||suppressProfileSave)return;if(!nicknameTimerReady){nicknameTimerReady=true;nicknameSaveTimer.Tick+=(s,a)=>{nicknameSaveTimer.Stop();PersistNickname();};}nicknameSaveTimer.Stop();nicknameSaveTimer.Start();}
 void PersistNickname(){if(suppressProfileSave||NicknameBox==null)return;string nickname=NicknameBox.Text.Trim();if(!Regex.IsMatch(nickname,"^[A-Za-z0-9_]{3,16}$")){NicknameBox.ToolTip="Ник: 3–16 латинских букв, цифр или подчёркиваний. Предыдущий ник сохранён.";return;}if(nickname==lastSavedNickname)return;try{var stored=LauncherSettings.Load(profileSettingsRoot);if(stored.Nickname!=nickname){stored.Nickname=nickname;stored.Save(profileSettingsRoot);}settings.Nickname=nickname;lastSavedNickname=nickname;NicknameBox.ToolTip="Ник сохраняется автоматически.";}catch(Exception e)when(e is IOException or UnauthorizedAccessException){Status("Не удалось сохранить ник: "+e.Message,true);}}
}

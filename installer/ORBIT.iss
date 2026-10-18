#ifndef AppVersion
  #define AppVersion "0.12.3"
#endif
#ifndef PayloadDir
  #error PayloadDir must point to the staged runtime files
#endif
#ifndef InstallerOutput
  #define InstallerOutput "..\artifacts\installer"
#endif
[Setup]
AppId={{65E4F092-5A64-4DF1-9A37-740283C6A29A}
AppName=ORBIT Launcher
AppVersion={#AppVersion}
AppPublisher=siho2223
AppPublisherURL=https://github.com/siho2223/orbit-launcher
AppSupportURL=https://github.com/siho2223/orbit-launcher/issues
AppUpdatesURL=https://github.com/siho2223/orbit-launcher/releases
VersionInfoVersion={#AppVersion}.0
DefaultDirName={localappdata}\Programs\ORBIT Launcher
DefaultGroupName=ORBIT Launcher
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0
DisableDirPage=no
DisableProgramGroupPage=yes
AllowNoIcons=yes
WizardStyle=modern dark
LanguageDetectionMethod=none
ShowLanguageDialog=yes
SetupIconFile=..\orbit.ico
UninstallDisplayIcon={app}\OrbitLauncher.exe
LicenseFile=..\LICENSE
OutputDir={#InstallerOutput}
OutputBaseFilename=ORBIT-Setup-{#AppVersion}-Windows-x64
Compression=lzma2
SolidCompression=yes
CloseApplications=no
RestartApplications=no
Uninstallable=yes
[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"
[CustomMessages]
russian.DesktopIcon=Создать ярлык на рабочем столе
english.DesktopIcon=Create a desktop shortcut
russian.LaunchOrbit=Запустить ORBIT Launcher
english.LaunchOrbit=Launch ORBIT Launcher
[Tasks]
Name: "desktopicon"; Description: "{cm:DesktopIcon}"; Flags: unchecked
[Files]
Source: "{#PayloadDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Excludes: "*.pdb"
[Icons]
Name: "{group}\ORBIT Launcher"; Filename: "{app}\OrbitLauncher.exe"; WorkingDir: "{app}"
Name: "{autodesktop}\ORBIT Launcher"; Filename: "{app}\OrbitLauncher.exe"; WorkingDir: "{app}"; Tasks: desktopicon
[Run]
Filename: "{app}\OrbitLauncher.exe"; Description: "{cm:LaunchOrbit}"; Flags: nowait postinstall skipifsilent

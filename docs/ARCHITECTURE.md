# Архитектура

- `App.xaml` / `App.xaml.cs`: общие WPF-стили, реакции элементов, запуск окна и вспомогательного сервиса скинов.
- `MainWindow.xaml`: экраны и навигация.
- `MainWindow.*.cs`: запуск, загрузки, каталог дополнений, профиль, скины и анимации по отдельным областям.
- `Core/Versions.cs`: профили Minecraft, наследование и библиотеки с правилами платформы.
- `Core/LaunchEngine.cs`: локальный профиль, Java, classpath, natives и план запуска.
- `Core/MojangInstaller.cs`: официальный каталог, загрузки клиента/библиотек/ресурсов/Java.
- `Core/ModrinthClient.cs`: поиск и совместимые версии дополнений, зависимости, проверенные загрузки и установка.
- `Core/Settings.cs`: локальные настройки и атомарное сохранение файла.
- `Core/LocalSkins.cs`: подписанные локальные косметические профили и временный loopback-сервис.
- `SkinPreview.cs`: 3D-предпросмотр PNG-скина.
- `ModIcons.cs`: ограниченная асинхронная загрузка PNG/WebP и кэш.
- `Tests`: консольные тесты и отдельные Java-пробы.

WPF-проект ссылается на Core; Core не зависит от WPF. Не следует включать игровые данные или пользовательские профили в исходники. Скины, аккаунты и загрузки относятся к данным конкретного пользователя и хранятся вне репозитория.


## 0.7.0 additions

`Core/Localization.cs` loads the EN/RU catalog embedded in the core assembly. `Loc.cs` exposes reactive WPF resources; user-provided names are preserved. `MainWindow.Upgrade.cs` contains instance navigation, window work-area handling, per-process game shutdown, extra options and preset controls. `SkinPresets` stores named local skins independently from the active skin.

`Modpacks` resolves MRPACK metadata, verifies hashes, provisions the declared Minecraft and loader, installs client files and overrides into a private staging directory, validates the runtime, and publishes with a directory move. A `.orbit-pack.json` marker stores pack identity; library entries carry a root override. Failed/cancelled installs remove only their own staging directory. Fabric/Quilt profiles and Maven libraries come from official metadata; Forge/NeoForge use checksum-verified official installers. No Minecraft process is started during installation.

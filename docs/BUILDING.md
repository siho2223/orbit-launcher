# Сборка и проверки

## Требования

- Windows 10/11 x64.
- .NET 8 SDK с поддержкой Windows Desktop (устанавливается вместе с SDK на Windows).
- Интернет для NuGet, Mojang и Modrinth при соответствующих операциях.

Сборка из корня репозитория:

```powershell
dotnet restore .\OrbitLauncher.csproj
dotnet build .\OrbitLauncher.csproj -c Release
```

Запуск интерфейса вручную: `bin/Release/net8.0-windows/OrbitLauncher.exe`.

## Автономная сборка

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts/build.ps1 -Publish
```

Выход: `artifacts/publish/win-x64`. Публикация использует win-x64, self-contained и SingleFile с включёнными native-компонентами. Скрипт копирует README, лицензии и уведомления зависимостей. Для распространения архивируй всю папку результата.

## Проверка чистоты исходников

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts/verify-source.ps1
```

Скрипт проверяет обязательные файлы, хеш authlib-injector и отсутствие файлов игр/настроек/ключей в предназначенном для публикации дереве. Это локальная проверка состава, не полноценный аудит безопасности программы.

## Тесты без Minecraft

```powershell
dotnet run --project .\Tests -c Release
```

Это консольный проект с собственной точкой входа, а не dotnet test. Базовые проверки работают без установленного Minecraft и не запускают игру.

## Проверки с установленной игрой

```powershell
dotnet run --project .\Tests -c Release -- "$env:APPDATA\.minecraft"
```

Используются локальные библиотеки и Java; формируются планы запуска и выполняются отдельные Java-пробы. Полный набор зависит от установленных версий; некоторые проверки ожидают Vanilla 1.21.1. Minecraft main при проверках не запускается.

Сетевые тесты скачивают настоящие файлы. Используй отдельные пустые тестовые папки, а не свою игровую папку:

```powershell
dotnet run --project .\Tests -c Release -- --modrinth "$PWD\artifacts\mod-test"
dotnet run --project .\Tests -c Release -- --resourcepacks "$PWD\artifacts\pack-test"
dotnet run --project .\Tests -c Release -- --shaders "$PWD\artifacts\shader-test"
```

Дополнительные offscreen WPF/Java-пробы остаются в исходниках для разработки. Для части из них нужны конкретные локальные версии игры и личные тестовые fixtures; игровые текстуры в репозиторий не включены. Финальную проверку запуска игры, графики и соединения с сервером нужно выполнять отдельно.


## Установщик Windows

Установите Inno Setup 6.7.3 или новее с https://jrsoftware.org/isdl.php.
Сборка приложения, базовые тесты и установщик:

```powershell
.\scripts\build-installer.ps1 -Iscc 'C:\Program Files (x86)\Inno Setup 6\ISCC.exe'
```

Готовые EXE и SHA-256: `artifacts/installer`. Установка для текущего пользователя, Windows 10/11 x64; ярлык рабочего стола необязателен. Удаление программы сохраняет игры и пользовательские данные. Бинарный файл не подписан сертификатом издателя.


## Архивы исходников и публикация

`python scripts/package-source.py` создаёт проверенные ZIP исходников приложения и установщика в `artifacts/delivery`, вместе с SHA-256. Оба архива содержат полный проект; исходники установщика описаны в INSTALLER-SOURCE.md.

Workflow `.github/workflows/release.yml` собирает и проверяет приложение и установщик из коммита main, затем публикует preview-релиз с установщиком, обоими исходными архивами и контрольными суммами. Используется стандартный GitHub Actions token; секреты аккаунтов не помещаются в код.

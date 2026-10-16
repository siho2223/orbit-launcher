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
powershell -ExecutionPolicy Bypass -File .\scriptsuild.ps1 -Publish
```

Выход: `artifacts/publish/win-x64`. Публикация использует win-x64, self-contained и SingleFile с включёнными native-компонентами. Скрипт копирует README, лицензии и уведомления зависимостей. Для распространения архивируй всю папку результата.

## Проверка чистоты исходников

```powershell
powershell -ExecutionPolicy Bypass -File .\scriptserify-source.ps1
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

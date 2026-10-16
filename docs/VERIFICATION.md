# 0.7.0 validation

- Release build: no compiler errors or warnings.
- Basic core suite: 14 checks.
- Installed-profile launch-plan / argument / Java probe regression: 52 checks. Java metadata probes ran; Minecraft did not run.
- Upgrade tests: 35 checks covering language/settings persistence, MRPACK path and URL validation, loader/env parsing, client override precedence, archive traversal, preset isolation, rollback on archive hash mismatch, clean-machine scan.
- Live MRPACK: Simply Optimized Reloaded for Minecraft 26.1.2 with NeoForge installed in a private workspace test root. Pack, Minecraft, Java, mod files and loader completed; launch plan and asset checks found no missing files. 39 checks in that live test run.
- Fabric 0.20.0 and Quilt 0.20.0-beta.9 loader profiles for Minecraft 1.21.1 provisioned from official metadata with verified libraries.
- Offscreen WPF: 10 checks for reactive EN/RU labels, complete static English labels, vector folder icons, hover scale, minimum scrollbar thumb, taskbar work area, live modpack results/categories/install availability.
- Resource pack and shader installation regression: live search, installation and repeated-install checks passed.
- Installer installation/uninstallation and payload hashes are checked separately before distributing a release.

Minecraft was not launched. Actual gameplay, unsaved-world shutdown behavior and mixed-DPI multi-monitor interaction still require user testing. Loader installation does not guarantee compatibility of arbitrary third-party modpacks or individual mods. Forge-specific live installer verification was not performed in this session; NeoForge, Fabric and Quilt were checked as listed above.

Commands:

```powershell
dotnet run --project Tests -c Release
dotnet run --project Tests -c Release -- --upgrade artifacts/upgrade-tests
dotnet run --project Tests -c Release -- --upgrade artifacts/upgrade-live --live
```

Live tests download pack/game/Java files into their explicit test destination and do not start the game. No test output is shipped with the source.

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


## 0.8.0 appearance and catalog regression checks

Offscreen WPF harness (`--render-preview <output.png> AppearanceChecks`) checks all three layouts, accessible game controls and return navigation, five distinct lobby palette accents, restoration of the original theme, settings round trip, English/Russian labels, removal of the apply-preset button and suppression of preset BringIntoView scrolling. Mock HTTP responses verify browsing without a game selection, category-endpoint failure, all four project tabs, out-of-order requests and recovery from transient HTTP 503 responses. Fixtures use isolated settings; no game is launched and no saved skin file is modified.

Theme preview names: ThemeDefault, ThemeDefault2, ThemeBrawlPurple, ThemeBrawlGreen, ThemeBrawlOrange, ThemeBrawlBlue, ThemeBrawlPink, ThemeBrawlSettings, ThemeBrawlCompact.


0.8.1 also checks the restored rounded mint controls in every theme, compact lobby launch panel, separate stepped button templates, popup placement at the visible version button and actual popup visibility while the normal toolbar is hidden.


## 0.8.2 Brawl interaction checks

The appearance harness additionally verifies the shared themed settings shell and card frames, embedded ready-made PNG resources, repeating the same version-button Click to open/close the popup, rotation deltas and full turns, animated arm movement for all three emotes, neutral pose reset, reduced motion, navigation cleanup and restoration of the standard theme. Offscreen rendering is used for the home, settings and wave-pose visual checks. Physical pointer dragging is implemented with WPF mouse capture; games are not started.


## 0.8.3 skin renderer checks

`--render-preview <output.png> SkinRendererChecks` renders classic, slim and legacy fixtures with an orthographic front camera. Pixel-alpha probes check shoulder, neck, waist and leg seams under cutout jacket overlays; arm-width probes distinguish the two models. An asymmetric legacy leg texture verifies reflection, and alpha probes verify an opaque base while preserving overlay transparency and source pixels. If a selected skin exists, it is read and rendered locally without modifying the PNG. Personal preview output stays under artifacts and is excluded from source delivery.


## 0.9.1 interface and export regressions

Release build: zero warnings and errors. Automated checks: 14 basic core, 35 upgrade/core, 16 ZIP export, 5 instance metadata, 58 appearance/catalog, 6 motion, 23 skin renderer, 34 refinement and 10 upgrade UI checks (201 total).

`RefinementChecks` validates visible staggered entrances and fully opaque completion in all themes; unchanged hover bounds; correct selected-instance outline/home summary; reduced-motion reset; rapid page switching; retained library search after refresh; distinct roots for equal profile IDs; lobby/background animation cleanup. Theme geometry checks cover returning from Brawl stepped frames to normal 1px card borders.

`dotnet run --project Tests -c Release -- --export <absolute-fixture-directory>` verifies archive contents, exact bytes, excluded private/game folders, version metadata, cancellation before and during export, locked source files, destination validation and preservation of an existing ZIP on error. `--instances <absolute-fixture-directory>` verifies separately installed packs sharing a loader profile and the actual Minecraft version from pack metadata.

All UI tests use offscreen windows. Screenshots for default, default 2, Brawl home/library/settings and animation entrance/settled states were inspected. The delivered portable EXE was rendered independently and its SHA-256 matched the published build. No Minecraft gameplay or GTNH launch was tested.


## 0.10.0 XMCL default theme

XMCL adaptation checks validate legacy theme migration, exactly two offered themes, default settings, 80px rail/player panel layout, compact home templates, live skin rendering, accessible PLAY/version controls, actual resource-dictionary click routing, full-window profile rendering and focus, close/skins actions, version-picker toggling, settings navigation, Brawl/default round trips and minimum window fit. 19 XMCL-specific checks pass.

The appearance/catalog (58), refinement (34), motion (6), skin rendering (23), upgrade UI (10), basic core (14), export (16) and instance metadata (5) checks also passed during this update. Native-popup tests run sequentially to avoid cross-process focus interference. Release build has zero errors/warnings. Published default home and profile dialog were rendered separately. No game was launched.

Upstream XMCL commit: b7d6ddb2225aabb34debfa4eaf5321413f479ed5. MIT license is retained in ThirdParty/XMCL-MIT.txt and embedded in the executable. Copied reference Vue files are retained in ThirdParty/XMCL-reference; runtime code is a WPF adaptation. The theme exposes ORBIT's existing offline profile, not XMCL online authentication providers.


## 0.11.0 Millida / WebView2 integration

- Release build: zero errors or warnings.
- Basic core suite: 14 checks.
- Actual React/WebView2 UI: 16 checks, including copied tile DOM, Mine3D initialization, no JavaScript errors after page changes/resizing, real profile/settings/library/skin navigation, palette round trip, version-button toggle and minimum-window PLAY bounds.
- Existing default/XMCL interface: 19 checks, including profile, version picker, settings, legacy-theme migration and switching between themes.
- Microsoft: 15 mock-service / local-fixture checks cover Xbox/XSTS/Minecraft exchange, entitlements, refresh rotation, device-code validation/cancellation, allowed verification hosts, DPAPI round trip/deletion and absence of access tokens in Java argument files. No real account sign-in occurred.
- Source inventory and basic credential/path scans pass; pinned authlib-injector hash retained.

Commands:
```
dotnet run --project Tests -c Release -- --microsoft artifacts/millida-work/auth-tests
OrbitLauncher.exe --render-preview artifacts/millida-work/checks.png MillidaChecks
OrbitLauncher.exe --render-preview artifacts/millida-work/default.png XmclChecks
```

The ORBIT Microsoft application is not registered. This build has no Microsoft client ID, so sign-in is disabled pending developer registration, Minecraft Services access and live validation. Minecraft gameplay was not launched in these checks. A separately installed WebView2 Runtime is required for the new Brawl interface; screenshots were captured from actual WebView2 rendering.


## 0.12.0 validation

- Basic core suite: 14 checks; ZIP export regression: 16 checks.
- Quick-loader suite: 19 checks covering stable/recommended build selection, legacy Maven suffix, XML entity rejection, loader/installed identity priority, base-version extraction, file-conflict preservation, cancellation and fresh theme defaults.
- Live private fixture: Fabric 0.19.5 and Forge 52.1.0 installed for Minecraft 1.21.1. Both inherited the correct base profile and had their proper entry points. Existing launcher_profiles.json sentinel survived unchanged; staging directories were cleaned. All 8 Fabric and 49 Forge own library files exist. Java installer ran; Minecraft did not run.
- Default WebView2: 20 checks including actual grey/brown/green rendered colours, Fabric card selection, quick-install dialog, no Microsoft/Discord entry points, palette round trip, resize and no JavaScript errors.
- Old theme: 21 checks including public theme names, all-version rail, profile, picker, settings visibility and switching between themes.
- Source inventory/credential scan and pinned skin-agent hash checked before packaging.

Live loader installation was tested for Minecraft 1.21.1, not every historical Forge installer. Gameplay was not launched. Discord authentication remains outside this release by explicit user direction.

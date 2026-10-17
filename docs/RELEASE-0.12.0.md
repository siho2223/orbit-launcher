# ORBIT 0.12.0 — Default, Forge & Fabric

Download **ORBIT-Setup-0.12.0-Windows-x64.exe** from Assets and follow the installer. Windows 10/11 x64; .NET is included. The installer supports English/Russian and an optional desktop shortcut. Existing Minecraft files and user settings are preserved.

## What changed

- The game lobby is now **Default**, with five palettes, a Mine3D character preview, drag rotation and emotes. The previous interface is **Old theme**.
- Quick Forge/Fabric installation: choose Minecraft and install a separate loader profile. Vanilla/Java are provisioned as needed, and loader staging protects existing profiles.
- Forge profiles have an anvil and light-grey cards; Fabric has its official logo and brown cards. Other installed versions are green.
- Old theme's sidebar shows every installed instance with visible names and Minecraft/loader labels.
- Export instance mods, configs, shaders, resource packs and scripts to ZIP for a friend.
- Improved skin previews, preset selection, version picker behaviour and catalog loading.
- The update card in Old theme follows the current executable version automatically.

## Source and installer source

Both source archives include the complete launcher project and installer sources, so the installer can be rebuilt independently. See **INSTALLER-SOURCE.md**, **installer/ORBIT.iss** and **scripts/build-installer.ps1**. SHA-256 files are attached for each downloadable asset.

## Checks and limits

This is an **unsigned preview**. Microsoft and Discord sign-in are not offered; profiles are local/offline. Minecraft servers with online-mode authentication require official account authentication.

Default uses Microsoft Edge WebView2 Runtime; the native interface is used as fallback when that runtime is unavailable. Millida's protected artwork/services are excluded; attribution and dependency licenses are shipped.

Release/core, UI, source-inventory, export and loader checks passed locally. Fabric 0.19.5 and Forge 52.1.0 were installed for Minecraft 1.21.1 in a private test root; their own libraries were present and existing launcher profiles were preserved. **Minecraft gameplay was not tested**, and arbitrary historical Forge installers are not guaranteed to support unattended installation. See docs/VERIFICATION.md.

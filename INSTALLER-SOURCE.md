# Building the ORBIT installer

The installer-source archive includes the complete launcher source because its payload is built from this project. It contains no prebuilt game files, user settings, accounts or compiler binaries.

Requirements: Windows 10/11 x64, .NET 8 SDK, Inno Setup 6.7.3 or newer. Node.js 22 is needed only when rebuilding BrawlUI; the generated dist is included in source.

1. Extract this archive.
2. Optional frontend rebuild: `npm ci --prefix BrawlUI --ignore-scripts` then `npm --prefix BrawlUI run build`.
3. Run `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/build-installer.ps1 -Iscc "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"` from the extracted project root.
4. The installer and SHA-256 are written to `artifacts/installer`.

Installer definition: installer/ORBIT.iss. Build scripts: scripts/build.ps1 and scripts/build-installer.ps1. Source packaging: scripts/package-source.py. The public release workflow reproduces the installer from the same tagged commit.

The installer is per-user, supports English/Russian and an optional desktop shortcut, bundles .NET, and preserves Minecraft/user data on uninstall. Default uses Microsoft Edge WebView2 Runtime; the launcher falls back to the native theme if it is unavailable. The installer is unsigned.

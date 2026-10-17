# ORBIT 0.12.1 — quick loader compatibility fix

- Quick Forge/Fabric setup now offers only Minecraft releases supported by the selected loader, using official loader metadata. This applies to Default and Old theme.
- Opening Fabric setup from an unsupported selected version (for example Minecraft 1.12.2) selects a supported replacement instead of sending an invalid request that returned HTTP 400.
- Installation stays disabled while compatibility data is loading. Default includes a retry action if this request fails.
- Unsupported direct installation requests are rejected before any game files are downloaded, with a clear loader/version message.
- Metadata connection errors identify the server and HTTP status.

Validation: 22 quick-loader checks, 11 real WebView2 UI checks; real isolated installs of Forge 14.23.5.2859 for Minecraft 1.12.2, Forge 52.1.0 and Fabric 0.19.5 for Minecraft 1.21.1. Forge 1.12.2 client, libraries and assets passed the launch-file check. Minecraft gameplay was not tested.

Download `ORBIT-Setup-0.12.1-Windows-x64.exe` for Windows 10/11 x64. Full launcher and self-contained installer source archives, plus SHA256 checksums, are attached. Installer is an unsigned preview; Default uses Microsoft Edge WebView2 Runtime. Profiles remain offline. Quick installation installs the selected loader; OptiFine is not bundled.

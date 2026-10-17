# Third-party notices

ORBIT application source is provided under GPL-3.0-only; see LICENSE. Copyright (C) 2026 ORBIT contributors.

## authlib-injector

Unmodified authlib-injector **1.2.8**, copyright its contributors.

- Source: https://github.com/yushijinhun/authlib-injector/tree/v1.2.8
- License: AGPLv3 or later with the AUTHLIB-INJECTOR exception.
- Binary: ThirdParty/authlib-injector-1.2.8.jar
- SHA-256: 9c7f4343e6c82034958ffb48c14a2cb0c85928be7283103ce17da00c6d5a7b10
- Exact upstream source archive: ThirdParty/authlib-source-1.2.8.zip
- Full license and exception: ThirdParty/AGPLv3-with-authlib-injector-exception.txt

Used as a Java agent for local cosmetic skin textures. The companion loopback service does not authenticate Microsoft accounts or authorize access to authenticated servers. Upstream code and notices are retained unchanged.

## SkiaSharp

SkiaSharp and SkiaSharp.NativeAssets.Win32 **4.153.1**, MIT license; restored through NuGet for raster/WebP icon decoding.

https://github.com/mono/SkiaSharp

Complete supplied licenses and native dependency notices are retained in ThirdParty/SkiaSharp-*.txt. See those files for the notices of Skia and its dependencies.

## .NET and WPF

Self-contained builds include the .NET 8 and WPF runtimes. Their license texts are retained in ThirdParty/DOTNET-LICENSE.txt and ThirdParty/WPF-LICENSE.txt; .NET dependency notices are in ThirdParty/DOTNET-THIRD-PARTY-NOTICES.txt. Packaging scripts retain these notices.

## Modrinth and Minecraft

Modrinth API v2 and its official CDN are used to search public projects and download user-selected files. Individual projects retain their own licenses and authorship. Their icons are retrieved at runtime and are not shipped as source assets.

Minecraft game files, Java distributions, mods, resource packs, shader packs, player skins and saves are not included in this repository. Minecraft is a trademark of Mojang/Microsoft. ORBIT is an independent application and is not affiliated with Mojang, Microsoft or Modrinth.

## Artwork and development

Assets/Orbit.png is the OR logo generated with an AI image tool at the project owner's request; orbit.ico is its multi-resolution conversion. Other interface illustrations are implemented in XAML. Code development was assisted by AI. Third-party UI source adaptations are documented below; ORBIT retains its own branding.


## Interface artwork (0.8.2)

Ready-made PNG graphics by Kenney Vleugels / Kenney.nl, CC0 1.0. Sources:
- https://kenney.nl/assets/game-icons
- https://kenney.nl/assets/isometric-blocks
- https://kenney.nl/assets/voxel-pack

Selected original PNG files are bundled under Assets/Brawl. License texts are retained in ThirdParty/Kenney-*.txt. No generated illustration atlas is used.


## Minecraft icons at runtime (0.8.4)

The preferred Brawl item images are read directly from an installed Minecraft client JAR, owned by Mojang / Microsoft. No Minecraft textures or client files are included in the executable or source archive. Kenney images remain bundled solely as the no-client fallback.


## XMCL UI adaptation (0.10.0)

The default WPF interface adapts the classic 80px rail, layered surfaces, instance header and profile dialog from Voxelum/x-minecraft-launcher, upstream commit b7d6ddb2225aabb34debfa4eaf5321413f479ed5. Copyright (c) 2023 ci010. Licensed under MIT; full notice retained in ThirdParty/XMCL-MIT.txt. Referenced original Vue component source is retained in ThirdParty/XMCL-reference. WPF adaptations: XmclTheme.xaml, MainWindow.Xmcl.cs, MainWindow.xaml. ORBIT is independently branded and does not include XMCL authentication implementations or brand artwork.

The optional blurred panorama is read at runtime from the installed Minecraft client JAR; no panorama texture is bundled.


## Millida UI adaptation (0.11.0)

BrawlUI/src/upstream contains runtime CSS and stageArt.ts copied from https://github.com/millida/launcher at commit 38ffacfb141a295d1858f2a1e2a08148129f9fdf, GPL-3.0-only. BrawlUI/src/main.tsx adapts its Play/HubTile structure to ORBIT's native actions. MainWindow.Microsoft.cs and Core/MicrosoftSignIn.cs adapt the Microsoft authentication protocol implementation. Complete GPL license, upstream notice, trademark exclusions and third-party notices are retained in ThirdParty/Millida-*.txt and Millida-TRADEMARK.md. No Millida client ID, backend, store, friends, hosting or brand artwork is included.

BrawlUI/src/vendor/mine3d is the copied Mine3D engine, Copyright (c) 2026 Undefined Studio, MIT; see ThirdParty/Mine3D-MIT.txt. Runtime dependencies React, ReactDOM, scheduler, Three.js, skin3d and skinview-utils retain their license texts in ThirdParty. Geist and Unbounded fonts are OFL-licensed; see ThirdParty/Millida-fonts-OFL.txt. BrawlUI/dist/app.js.LEGAL.txt retains bundled notices. Microsoft WebView2 SDK is used under its NuGet package license; the separately installed Microsoft Edge WebView2 Runtime is required.


## Loader identification artwork (0.12.0)

Assets/Loaders/fabric.png is the official Fabric logo from FabricMC/fabricmc.net, assets/logo.png (MIT, Copyright 2016–2022 FabricMC). Full license: ThirdParty/Fabric-Website-LICENSE.txt.

Assets/Loaders/forge.png is the original Forge Installer wordmark from MinecraftForge/Installer, src/main/resources/big_logo.png (LGPL-2.1; ThirdParty/Forge-Installer-LICENSE.txt). ORBIT crops the anvil region at runtime for loader identification. The original, unmodified image is included in the source archive. Source: https://github.com/MinecraftForge/Installer/tree/2.0 . ORBIT is independently branded and is not endorsed by Fabric or Forge.

Left lobby tiles use the player's own skin preview plus Minecraft icons loaded at runtime, with Kenney fallbacks. Millida's excluded proprietary lobby illustrations are not redistributed.

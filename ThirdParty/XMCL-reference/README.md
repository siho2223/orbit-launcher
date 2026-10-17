# XMCL UI adaptation reference

Upstream: https://github.com/voxelum/x-minecraft-launcher
Commit: b7d6ddb2225aabb34debfa4eaf5321413f479ed5
License: MIT, copyright (c) 2023 ci010. Full license: ../XMCL-MIT.txt.

The retained upstream Vue components document the code used to adapt the classic 80px navigation rail, layered background, instance header and profile dialog to WPF. Runtime adapters live in XmclTheme.xaml and MainWindow.Xmcl.cs. ORBIT remains a WPF application; these Vue files are reference source, not an Electron runtime. Authentication providers and online features are not represented as supported unless implemented in ORBIT. No XMCL logo or game image assets are copied.

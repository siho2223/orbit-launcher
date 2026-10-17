# Millida UI source integration

Upstream: https://github.com/millida/launcher
Commit: 38ffacfb141a295d1858f2a1e2a08148129f9fdf
Copyright (C) 2024-2026 Millida. GPL-3.0-only. License and trademark notices: ../ThirdParty/Millida-*.

Copied runtime files: src/upstream/*.css, src/upstream/stageArt.ts, src/vendor/mine3d. The Play/HubTile markup in main.tsx is adapted from Play.tsx and HubTile.tsx, with unsupported Millida services removed and native ORBIT bridge actions added. Mine3D retains its own MIT notice. Fonts retain OFL notices. Brand-only assets, logos, capes, backgrounds, service URLs, telemetry, store, friends and hosting are not included. The scene uses the copied paintScene code with no proprietary backdrop image. Runtime item images and skin are supplied by ORBIT from local game/user files.

Build: npm ci --ignore-scripts; npm run build. Commit package-lock.json and dist for reproducibility. No frontend token storage: Microsoft tokens stay in the native encrypted account store.

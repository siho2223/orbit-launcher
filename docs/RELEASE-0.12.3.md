# ORBIT 0.12.3 — existing vanilla installation fix

Quick Forge and Fabric installation could both fail with "A version with this ID already exists and differs from Mojang" when an installed vanilla profile used an earlier official asset index. The old installer required a byte-identical match to the latest Mojang metadata, even when the client and every other field still matched.

The installer now reuses that existing vanilla metadata and its verified asset index. The original JSON is preserved byte for byte. Changes to the client, libraries, launch arguments, entry point or other profile fields remain protected and are not silently overwritten. Formatting changes and UTF-8 BOM do not invalidate an otherwise matching profile.

Loader stages and complete exceptions are saved to `loader-install.log` in the ORBIT application data directory. Failed installations open the launcher log. Network timeouts are reported separately from a user cancellation, and queued progress updates cannot replace the final error or success message.

The All versions, Forge and Fabric tabs from 0.12.2 are included. Full regression and UI installation checks use an isolated copy of installed files and never launch Minecraft.

This Windows x64 preview includes the installer and complete source archives. OptiFine is not bundled.

Validation: reproduced the original failure for both loaders through the real Default install buttons on a copy of an existing profile, then completed Fabric 0.19.5 and Forge 52.1.0 installation for Minecraft 1.21.1 through the same UI after the fix. Client, libraries, assets and Java passed the launch-file check; the existing vanilla JSON and launcher profiles stayed unchanged. Minecraft itself was not launched. Includes 11 installed-metadata regression checks.

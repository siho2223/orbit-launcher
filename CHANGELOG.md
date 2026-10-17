# 0.12.0 (preview)

- Game lobby renamed Default and selected on upgrade/first launch; previous interface renamed Old theme. Choices persist after migration.
- Microsoft sign-in removed from both UIs; Discord deferred. Local profiles retained.
- Forge: light grey cards and anvil; Fabric: brown cards and official logo; other installed versions: green. Colours and icons apply to both themes and version lists.
- Old theme rail now has visible instance names, Minecraft/loader labels and all installed versions.
- Quick Forge/Fabric installation: official stable/recommended metadata, Vanilla/Java provisioning, isolated loader staging, verified libraries, conflict preservation and cancellation.
- Legacy standalone ForgeOptiFine profiles now resolve their actual Minecraft base version.
- Lobby tiles include a preview of the player's own skin and Minecraft imagery; protected Millida artwork remains excluded.

# 0.11.0 (preview)

- Brawl now uses actual Millida GPL-licensed pixel CSS, adapted HubTile/Play markup and Mine3D via local React/WebView2.
- Themed library, settings, skins, worlds, Modrinth catalog and logs call existing native ORBIT actions. Original Minecraft icons remain runtime-only.
- Character drag rotation and emotes; five palettes; reduced-motion support. The version picker opens and closes from the same button.
- Microsoft device authorization, Xbox/Minecraft exchange, ownership validation, refresh and DPAPI token storage implemented. The preview has no ORBIT client ID, so live sign-in remains disabled and unverified.
- Source licenses and attribution retained; Millida branding, private artwork and unsupported services excluded.

# 0.10.0 (preview)

- Replaced the old default/default 2 layouts with a single default interface adapted from XMCL's MIT-licensed classic UI.
- Added an 80px navigation rail, persistent player panel, compact two-column instance lists, layered dark surfaces, and a profile dialog for the existing offline nickname.
- Legacy default 2 selections migrate to default. Brawl Stars remains a separate theme.
- Optional blurred background reads the installed client's panorama without bundling game textures.
- Retained launch, skins, Modrinth catalog, instance folders, ZIP export and reduced-motion controls.

# 0.9.1 (preview)

- Home now shows the selected instance, its Minecraft/loader/Java details and a Minecraft block illustration.
- Connected staggered card reveals, added staged settings section transitions and selected-instance outlines in all themes.
- Added drifting background particles with cached scenery and a 24 FPS limit; idle motion pauses when minimized.
- Fixed hover expansion outside card bounds, stale animation state and animation reset when reduced motion is enabled.
- Preserve the selected instance root when two modpacks share the same version ID. Library refresh preserves the search filter.
- Corrected the Minecraft version shown for installed modpacks.
- Includes cancellable ZIP export introduced in 0.9.0.

# 0.9.0 (preview)

- Added cancellable ZIP export on each installed version card: mods, configs, shader/resource packs and pack scripts, plus version metadata and friend instructions.
- Exports use the actual selected instance; saves, accounts, logs, game binaries and Java are excluded. Atomic output prevents partial archives from replacing existing files.
- Refined version cards, hover/press feedback in all themes and shorter page transitions; reduced motion remains supported.

# 0.8.6 (preview)

- Removed the character-drag hint and bottom status caption from the Brawl home page.
- Removed redundant loader/Java text beneath the selected game version in the lobby launch panel.

# 0.8.5 (preview)

- Restored the cog, back arrow and library grid icons.
- Replaced the cropped resource-pack thumbnail with the full grass-block window icon from installed Minecraft assets.
- Added padding and reduced block-icon sizes in Worlds and the version / Play panel.

# 0.8.4 (preview)

- Replaced the Brawl menu graphics with original Minecraft item icons, loaded read-only from an installed client.
- Used diamond pickaxe / armor stand / grass block for modpacks, skins and worlds; book / comparator / ender pearl for navigation; totem / music disc / firework for emotes.
- Icons refresh after version discovery and installation, keep sharp pixel scaling and fall back safely if no suitable client JAR is present. No game assets are redistributed.

# 0.8.3 (preview)

- Removed the duplicate right-hand Modrinth modpacks card from the Brawl lobby.
- Fixed skin overlay transparency causing visible gaps at shoulders, waist and between legs in the WPF 3D preview.
- Rendered all opaque body parts before outer layers; zero-alpha overlay pixels no longer produce depth-writing geometry.
- Matched opaque inner skin layers and mirrored legacy left limbs in both 3D previews and saved preset thumbnails.
- Removed the drag-surface tooltip that obscured the character; the hint below the preview remains.

# 0.8.2 (preview)

- Extended brawl stars palette, navigation and inset controls to settings, version picker and all standard pages.
- Replaced procedurally drawn menu illustrations with ready-made Kenney CC0 PNG assets; sources and notices are bundled.
- Fixed the version selector reopening when its trigger was clicked again. Click outside, Escape and window deactivation close it.
- Removed lobby rotation arrows; left-button drag rotates the character with mouse capture.
- Added animated wave, dance and cheer poses with arm/leg articulation, automatic reset and reduced-motion support.

# 0.8.1 (preview)

- Restored original ORBIT rounded controls, mint colors and normal button weights in every standard page, including Settings and default 2.
- Rebuilt the brawl stars lobby with illustrated navigation cards, stepped inset button frames, compact top profile controls and a centered blue version / colored Play panel.
- Added pink palette and softened the lobby background. Palettes now affect only the lobby.
- Preserved shared version selection and launch state, including progress, install and stop-game actions, across both layouts.

# 0.8.0 (preview)

- Added default, default 2 and brawl stars themes with immediate switching and persistent selection.
- Added a character lobby with original vector background, saved PNG preview and four palettes.
- Added a restrained home layout, neutral colors and reduced decoration in default 2.
- Fixed stale catalog responses after switching tabs; failed category metadata no longer prevents searching.
- Browse the catalog without a selected game; install eligibility is still checked separately.
- Added bounded retries for transient metadata API errors and an explicit search retry action.
- Fixed skin preset selection scrolling the page, and removed the redundant apply-preset button.

# 0.7.1

- Fixed language and skin-preset dropdowns showing record IDs instead of titles.
- Saved skins now have front-facing character cards. Click a card to apply the skin.
- Named preset dropdown remains below the gallery; existing saved skins are retained.

# 0.7.0

- Default English UI with live Russian switching.
- MRPACK installation and separate profiles, automatic loader provisioning.
- Library and selected-version folder icons; direct mods and root-folder buttons.
- Named local skin presets with arm model retention.
- Main action closes the game launched by ORBIT, with save/force-quit confirmations.
- Resolution, fullscreen, minimize-on-launch, remember-window and log-scrolling settings.
- Hover clipping, small scrollbar thumbs, initial window bounds and maximized taskbar overlap fixed.
- New installer and reproducible installer source; English default and Russian option.

# Changelog

## 0.6.1

- Search, categories and installation for Modrinth shaders.
- Dedicated shaderpacks directory and manifest; ZIP and hash validation.

## 0.6

- Nickname autosave on editing and window close.
- Modrinth resource packs and separate resourcepacks directory.

## 0.5

- Updated interface, mint accent, page transitions and button/card animations.
- Reduced-motion mode and navigation shortcuts.

## 0.4

- OR application icon and Modrinth mod catalog with required dependencies.
- Original project icons with PNG/WebP decoding and local cache.

## 0.3

- Local custom skins, classic/slim models and 3D preview.
- Fix for the Minecraft 1.17 offline social-service startup failure.

## 0.2

- Official Vanilla version and Java downloads.
- Fix for Windows LWJGL library selection in Minecraft 1.12.2.

## 0.1

- Initial WPF launcher and local game profiles.

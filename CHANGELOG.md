## Changelog:

### 1.0.5
- Bug fixes for gamepads
- Updated Jotunn reference

### 1.0.4
- Multiplayer bug fixes
- Added server-enforced config syncing
- Reorganized the config (please delete your old config to avoid confusion)
- Embedded assets into the binary
- Added incompatibility checks for blacks7ar.VikingsDoSwim, projjm.improvedswimming, and ch.easy.develope.vh.diving.mod
- Updated Jotunn and BepInEx references
- Removed unused and deprecated JotunnLib reference

### v1.0.3
- Added feature that allows the player to swim faster by holding the Run button
- Oxygen is consumed faster rather than stamina when fast swimming while underwater
- Resting is now triggered based on players velocity
- Large waves/thunderstorms now hinder ability to rest in water
- Fixed bug that allowed stamina regen while autoswimming
- Changed debug value has_ping_sent to is_underwater and reversed the logic
- Changed debug value reder_settings_updated_camera to render_settings_updated_camera
- Removed unused debug values, variables, and references
- Added new debug values: last_dive_cancel, fastSwimSpeed, and fastSwimStamDrain
- Added new config setting allowFastSwimming

### v1.0.2
- WaterWalking in Epic Loot was deprecated in version 0.9.0. Removed unneeded compatability fix.
- Removed the EpicLoot reference
- Updated Jotunn and JotunnLib references
- Updated Unity and Valheim references
- Removed unused debug value restor_timer_is_running
- Increased the oxygen bar removal delay to better match the stamina bar

### v1.0.1
- Fixed a bug that was preventing the oxygen bar art from loading
- Fixed grammatical errors in the config file

### v1.0.0
- Initial release
- Removed easytranslate references
- Removed take rest in water keybind from default config
- Added allow rest in water to default config
- Added logic to disable breath bar when player can breath or when dead
- Added logic to prevent You Can Breathe message when dead
- Added logic to toggle diving on key-press and automatically toggling it off when surfacing
- Added logic for resting in the water to regen stamina when player is not moving or diving
- Added configurable diving and surface messages to the config
- Fixed miscategorized settings in the config
- Fixed negative stamina while drowning
- Changed the breath bar art and behavior to match the Valheim theme
- Added a Valheim themed overlay over the breath bar
- Added logic to change sprite color for oxygen bar when it's at 25% or less
- Added logic to hide the full breath bar after a delay to match the Valheim stamina bar
- Added logic to fix a swimming on land glitch when diving close to shore
- Changed all inputs to reference Valheim keybinds to add support for custom keybinds and controllers
- Removed divetrigger from the config and set the dive trigger key to the Valheim "Crouch" binding
- Changed the default position of the oxygen bar so that it doesn't overlap the crosshair
- Set the breath bar to not immediately disappear when on land
- Fixed stamina briefly goes negative when out of oxygen
- Fixed negative stamina bug when dead
- Added logic for cancelling diving mode if still near the surface
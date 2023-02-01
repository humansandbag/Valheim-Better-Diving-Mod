# Better Diving by Main Street Gaming
This is a fork of the Valheim Diving Mod by MLIMG/Easy_Develope. Special thanks go to MLIMG for creating the original mod. Since development has ceased on the original mod, I have created a fork to continue development and implement bug fixes.

## How It Works
- Uses built-in Valheim keybinds, so this mod should work with custom keybinds and controllers as well
- While swimming, press the crouch button to enable diving mode
- Diving mode can be cancelled by pressing the crouch button again if near the surface
- While diving is enabled, look in the direction you want to dive and hold your forward movement button (W on keyboard)
- While diving, the Left, Right, Up, and Down movement buttons (WASD on keyboard) can be used to swim in each direction as well
- To return to the surface, look up toward the surface of the water and hold your forward movement button (W on keyboard)
- Diving mode will be automatically cancelled when the player returns to the surface of the water
- While on the surface with diving mode disabled, the player will automatically rest and slowly regain stamina when not in motion
- The player will gain a Diving skill while diving
- Many options can be customized via the config file

## Installation

### r2Modman (recommended)
1. Install r2modman
2. Create a new profile
3. Click the 'Online' button
4. Search for 'BetterDiving' and download
5. Click 'Start Modded'

### Manual
Copy the `MainStreetGaming-BetterDiving` folder to `<GameDirectory>/BepInEx/plugins`.

## Changelog:

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

# Changing Keybinds

Randomizer specific keybinds can be changed by editing the `Keybinds.txt` file in the `ArchipelagoData` folder

Note: this folder only appears after launching the game at least once

## Available Keybinds

**OpenTeleport**
- Default: `LeftAlt+T`, `RightAlt+T`
- Opens the teleport screen from anywhere in the game

**DoubleBash**
- Default: `Grenade`
- Used to perform a double bash trick

**GrenadeJump**
- Default: `Grenade+Jump`
- Used to perform a grenade jump trick

**ListStones**
- Default: `LeftAlt+S`, `RightAlt+S`
- Lists all keystones and mapstones the player has

**GoalProgress**
- Default: `LeftAlt+G`, `RightAlt+G`
- Lists the current goal mode and the progress towards this goal

## Adding Valid Key Codes

Each Keybind has a comma-separated list of all possible key combinations that will trigger it.

Each item in this list is a list of keys separated by plus signs. These are the keys that need to be pressed at the same time to trigger the keybind

Valid Keys
- Any Unity Keycode: This includes all (or at least most) of the symbols on the keyboard. Check [Unity documentation](https://docs.unity3d.com/ScriptReference/KeyCode.html) for a full list
- Ori controls: This links to Ori specific controls and a few controller buttons
  - Jump, SpiritFlame, Bash, SoulFlame, ChargeJump, Glide, Dash, Grenade, Left, Right, Up, Down, Start, Select
- Controller buttons: This links to generic controller buttons (including Triggers, Bumbers, and Sticks). These must be prepended with the underscore as listed
  - _A, _B, _X, _Y, _LT, _RT, _LB, _RB, _LS, _RS

## Resetting Keybinds

To reset keybinds back to the default, delete `Keybinds.txt` and launch the game.
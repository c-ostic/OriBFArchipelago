*Note: If you are updating the mod, you can skip to step 2*

# Step 0: Install Ori
1. Make sure Ori and the Blind Forest: Definitive Edition is installed

   - Note: this mod does not work with the standalone Ori randomizer. If are using the standalone randomizer, you will need to switch back to vanilla

# Step 1: Install BepInEx
1. Download and unzip BepInEx_x86_5.4.23.2.zip located [here](https://github.com/BepInEx/BepInEx/releases)
   - Note: Make sure you download the x86 version specifically even if your machine is x64
 
2. Navigate to the Ori and the Blind Forest files
   - This can be done through steam using `Manage -> Browse local files` in the game settings for Ori
 
3. Move the contents of the BepInEx_x86_5.4.23.2 folder into the Ori DE folder

4. Run Ori and the Blind Forest once for BepInEx to set up additional folders
   - Note: the game will be stuck on a black screen, this is expected and you will have to close the game manually
 
5. Navigate into BepInEx\config and open BepInEx.cfg

6. Change `Type = Application` to `Type = Camera`
   - This option is the second to last option in the cfg
 
7. Optionally, set `Enabled = true` under [Logging.Console]
   - This option enables a terminal window to see additional log messages

# Step 2: Install OriBFArchipelago
1. Download and unzip the latest release zip file from [here](https://github.com/c-ostic/OriBFArchipelago/releases)

2. Navigate to BepInEx\plugins

3. Move the entire OriBFArchipelago folder into the plugins folder

4. If you are updating the mod it may ask you to replace files. If so, click yes

# Step 3: Run the game!
1. Start Ori and the Blind Forest

2. Upon startup, you should see a set of text boxes in the upper left corner

3. If you enabled the console, you should also get a message there

4. Highlight the save profile you want to use

5. Fill out the server name, port, slot name, and (optional) password in the upper left text boxes

6. If you are having problems with the game's UI moving as you type, press the "Edit" button. 
This prevents all other input into the game. Press "Done" when complete.

7. Start the profile and begin playing!

# Additional Notes
- You can access the APWorld that goes along with this mod [here](https://github.com/c-ostic/Archipelago/releases)

- You can teleport from anywhere in the map to any activated/unlocked Teleporter by pressing `Alt-T`
  - Make sure you acquire Sein and activate the first Teleporter before teleporting for the first time
  - You can freely teleport out of the Ginso escape sequence and come back to it later
  - You can freely teleport out of the Forlorn escape sequence. You won't be able to complete it later, but there aren't any checks besides starting the escape
  - *Don't* teleport about of the Horu escape sequence if you can avoid it. This is a known issue. If you need to, try to do so at the beginning of the sequence. Otherwise, you may softlock yourself

- Keybinds (including `Alt-T` to teleport) can be changed in the Keybinds.txt file. More information about this can be found [here](Keybinds.md)

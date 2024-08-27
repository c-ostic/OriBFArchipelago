# Step 0: Install Ori
1. Make sure Ori and the Blind Forest: Definitive Edition is installed

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

# Step 3: Run the game!
1. Start Ori and the Blind Forest

2. Upon startup, you should see a set of text boxes in the upper left corner

3. If you enabled the console, you should also get a message there

4. Highlight the save profile you want to use

5. Fill out the server name, port, slot name, and (optional) password in the upper left text boxes

6. Start the profile and begin playing!

# Additional Notes
- You can access the APWorld that goes along with this mod [here](https://github.com/c-ostic/Archipelago/releases)

- You can teleport from anywhere in the map to any activated/unlocked Teleporter by pressing `Alt-T`
  - Make sure you acquire Sein and activate the first Teleporter before teleporting for the first time
  - Don't teleport during any of the escape sequences (Ginso, Forlorn, and Horu) or you may softlock yourself from completing them

- The current goal of the randomizer requires you to activate all 9 Spirit Trees and then complete the Horu Escape
  - You will not be able to enter the final door until all 9 Spirit Trees have been activated
  - More goal modes will be coming later

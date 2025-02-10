using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using HarmonyLib;

namespace OriBFArchipelago.Core
{
    /**
     * Struct to represent archipelago data associated with a single save slot
     */
    internal struct SlotData
    {
        public string serverName;
        public string slotName;
        public int port;
        public string password;

        public SlotData()
        {
            serverName = "";
            slotName = "";
            port = 0;
            password = "";
        }
    }

    /**
     * Class used for reading from and writing to files 
     */
    internal class RandomizerIO
    {
        public const string SAVE_FILE_PATH = "ArchipelagoData";
        public const int NUM_SLOTS = 10;

        /**
         * Reads the save file associated with the given slot
         */
        public static bool ReadSaveFile(int saveSlot, out RandomizerInventory inventory, out List<string> locations)
        {
            string inventoryFileName = $"Slot{saveSlot}.txt";
            string inventoryFullPath = $"{SAVE_FILE_PATH}\\{inventoryFileName}";

            string locationFileName = $"Slot{saveSlot}Locations.txt";
            string locationFullPath = $"{SAVE_FILE_PATH}\\{locationFileName}";

            try
            {
                // Read inventory first
                StreamReader sr = new StreamReader(inventoryFullPath);

                // Get the version and slotname from file first
                string version = sr.ReadLine().Split('=')[1].Trim();
                string slotName = sr.ReadLine().Split('=')[1].Trim();

                // Create the inventory
                inventory = new RandomizerInventory(version, slotName);

                // Go through rest of data to add to inventory
                string[] data = sr.ReadToEnd().Split('\n');
                sr.Close();

                foreach (string line in data)
                {
                    if (string.IsNullOrEmpty(line.Trim())) continue;

                    string[] pair = line.Trim().Split('=');

                    if (pair.Length != 2)
                    {
                        Console.WriteLine($"Incorrect format for inventory data: {line}");
                        continue;
                    }

                    try
                    {
                        InventoryItem itemName = (InventoryItem)Enum.Parse(typeof(InventoryItem), pair[0].Trim());
                        int count = int.Parse(pair[1].Trim());
                        inventory.Add(itemName, count);
                    }
                    catch (ArgumentException)
                    {
                        Console.WriteLine($"Invalid inventory data: {pair[0].Trim()}={pair[1].Trim()}");
                    }
                }

                // Read locations
                sr = new StreamReader(locationFullPath);

                data = sr.ReadToEnd().Split('\n');
                sr.Close();

                locations = new List<string>();

                foreach (string line in data)
                {
                    if (string.IsNullOrEmpty(line.Trim())) continue;

                    locations.Add(line.Trim());
                }

                return true;
            }
            catch (IOException e)
            {
                Console.WriteLine($"Could not read save file: {e}");
                inventory = new RandomizerInventory("", "");
                locations = new List<string>();
                return false;
            }
        }

        /** 
         * Saves the given inventory to a file
         */
        public static bool WriteSaveFile(int saveSlot, RandomizerInventory inventory, List<string> locations)
        {
            string inventoryFileName = $"Slot{saveSlot}.txt";
            string inventoryFullPath = $"{SAVE_FILE_PATH}\\{inventoryFileName}";

            string locationFileName = $"Slot{saveSlot}Locations.txt";
            string locationFullPath = $"{SAVE_FILE_PATH}\\{locationFileName}";

            // Write inventory
            try
            {
                // Save inventory
                StringBuilder sb = new StringBuilder();

                // Save the version and slotname first
                sb.AppendLine($"Version={inventory.Version}");
                sb.AppendLine($"SlotName={inventory.SlotName}");

                // Go through rest of inventory to save to file
                foreach (InventoryItem itemType in Enum.GetValues(typeof(InventoryItem)))
                {
                    sb.AppendLine($"{itemType}={inventory.Get(itemType)}");
                }

                // remove last new line character
                sb.Remove(sb.Length - 1, 1);

                StreamWriter sw = new StreamWriter(inventoryFullPath);

                sw.Write(sb.ToString());

                sw.Close();

                // Save locations
                sb = new StringBuilder();

                foreach (string line in locations)
                {
                    sb.AppendLine(line);
                }

                sw = new StreamWriter(locationFullPath);

                sw.Write(sb.ToString());

                sw.Close();

                return true;
            }
            catch (IOException e)
            {
                Console.WriteLine($"Could not write to save file: {e}");
                return false;
            }
        }

        /**
         * Copy files of a saved game into another slot
         */
        public static bool CopySaveFile(int originalSaveSlot, int copySaveSlot)
        {
            // Original file paths
            string originalInventoryFileName = $"Slot{originalSaveSlot}.txt";
            string originalInventoryFullPath = $"{SAVE_FILE_PATH}\\{originalInventoryFileName}";

            string originalLocationFileName = $"Slot{originalSaveSlot}Locations.txt";
            string originalLocationFullPath = $"{SAVE_FILE_PATH}\\{originalLocationFileName}";
            
            // New file paths
            string newInventoryFileName = $"Slot{copySaveSlot}.txt";
            string newInventoryFullPath = $"{SAVE_FILE_PATH}\\{newInventoryFileName}";

            string newLocationFileName = $"Slot{copySaveSlot}Locations.txt";
            string newLocationFullPath = $"{SAVE_FILE_PATH}\\{newLocationFileName}";

            try
            {
                File.Copy(originalInventoryFullPath, newInventoryFullPath);
                File.Copy(originalLocationFullPath, newLocationFullPath);
            }
            catch (IOException e)
            {
                Console.WriteLine($"Could not copy the file: {e}");
                return false;
            }

            return true;
        }
        /**
         * Deletes the specified save file
         */
        public static bool DeleteSaveFile(int saveSlot)
        {
            string inventoryFileName = $"Slot{saveSlot}.txt";
            string inventoryFullPath = $"{SAVE_FILE_PATH}\\{inventoryFileName}";

            string locationFileName = $"Slot{saveSlot}Locations.txt";
            string locationFullPath = $"{SAVE_FILE_PATH}\\{locationFileName}";

            try
            {
                File.Delete(inventoryFullPath);
                File.Delete(locationFullPath);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Could not delete file: {e}");
                return false;
            }
        }

        /**
         * Read the dictionary of archipelago slot data associated with all the game's save slots
         * If the file is not found, it creates a file
         */
        public static bool ReadSlotData(out Dictionary<int, SlotData> data)
        {
            try
            {
                string fileName = $"SlotData.txt";
                string fullPath = $"{SAVE_FILE_PATH}\\{fileName}";

                // Special case if file does not exist, create a new file
                if (!File.Exists(fullPath))
                {
                    data = CreateSlotData();
                    return WriteSlotData(data);
                }

                // Otherwise, continue and read the file
                data = new Dictionary<int, SlotData>();

                StreamReader sr = new StreamReader(fullPath);

                // Read each line which has separate slot data
                string[] lines = sr.ReadToEnd().Split('\n');

                sr.Close();

                foreach (string line in lines)
                {
                    if (string.IsNullOrEmpty(line)) continue;

                    string[] parts = line.Split(',');

                    if (parts.Length != 5)
                    {
                        Console.WriteLine($"Incorrect format for slot data: {line}");
                        continue;
                    }

                    SlotData slotData = new SlotData();
                    slotData.serverName = parts[1];
                    slotData.slotName = parts[2];
                    slotData.port = int.Parse(parts[3]);
                    slotData.password = parts[4];

                    data.Add(int.Parse(parts[0]), slotData);
                }

                return true;
            }
            catch (IOException e)
            {
                Console.WriteLine($"Could not read slot data file: {e}");
                data = CreateSlotData();
                return false;
            }
        }

        /**
         * Create slot data file if it doesn't exist
         */
        private static Dictionary<int, SlotData> CreateSlotData()
        {
            Dictionary<int, SlotData> data = new Dictionary<int, SlotData>();

            for (int i = 0; i < NUM_SLOTS; i++)
            {
                SlotData slotData = new SlotData();
                data.Add(i, slotData);
            }

            return data;
        }

        /**
         * Write slot data to file
         */
        public static bool WriteSlotData(Dictionary<int, SlotData> data)
        {
            try
            {
                string fileName = $"SlotData.txt";
                string fullPath = $"{SAVE_FILE_PATH}\\{fileName}";

                if (!Directory.Exists(SAVE_FILE_PATH))
                {
                    Directory.CreateDirectory(SAVE_FILE_PATH);
                }

                StringBuilder sb = new StringBuilder();
                

                foreach (KeyValuePair<int, SlotData> pair in data)
                {
                    sb.Append($"{pair.Key},");
                    sb.Append($"{pair.Value.serverName},");
                    sb.Append($"{pair.Value.slotName},");
                    sb.Append($"{pair.Value.port},");
                    sb.Append($"{pair.Value.password}\n");
                }

                // remove the last new line character
                sb.Remove(sb.Length - 1, 1);

                StreamWriter sw = new StreamWriter(fullPath);

                sw.Write(sb.ToString());

                sw.Close();

                return true;
            }
            catch (IOException e)
            {
                Console.WriteLine($"Could not write to slot data file: {e}");
                return false;
            }
        }
        
        /**
         * Read settings from file
         * If the file is not found, an empty dictionary is returned and the function returns false
         */
        public static bool ReadSettings(out Dictionary<RandomizerSetting, int> settings)
        {
            try
            {
                string fileName = $"Settings.txt";
                string fullPath = $"{SAVE_FILE_PATH}\\{fileName}";

                settings = new Dictionary<RandomizerSetting, int>();

                // If file does not exist, return false without error
                if (!File.Exists(fullPath))
                {
                    return false;
                }

                // otherwise, continue to read the file
                StreamReader sr = new StreamReader(fullPath);

                // Read each line which has separate slot data
                string[] lines = sr.ReadToEnd().Split('\n');

                sr.Close();

                foreach (string line in lines)
                {
                    if (string.IsNullOrEmpty(line)) continue;

                    string[] pair = line.Split('=');

                    if (pair.Length != 2)
                    {
                        Console.WriteLine($"Incorrect format for setting data: {line}");
                        continue;
                    }

                    try
                    {
                        RandomizerSetting setting = (RandomizerSetting)Enum.Parse(typeof(RandomizerSetting), pair[0].Trim());
                        int value = int.Parse(pair[1].Trim());
                        settings.Add(setting, value);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine($"Invalid settings data: {pair[0].Trim()}={pair[1].Trim()}");
                    }
                }

                return true;
            }
            catch (IOException e)
            {
                Console.WriteLine($"Could not read settings file: {e}");
                settings = new Dictionary<RandomizerSetting, int>();
                return false;
            }
        }

        /**
         * Write setting to file
         */
        public static bool WriteSettings(Dictionary<RandomizerSetting, int> settings)
        {
            try
            {
                string fileName = $"Settings.txt";
                string fullPath = $"{SAVE_FILE_PATH}\\{fileName}";

                StringBuilder sb = new StringBuilder();

                // Go through rest of inventory to save to file
                foreach (RandomizerSetting setting in settings.Keys)
                {
                    sb.AppendLine($"{setting}={settings[setting]}");
                }

                // remove last new line character
                sb.Remove(sb.Length - 1, 1);

                StreamWriter sw = new StreamWriter(fullPath);

                sw.Write(sb.ToString());

                sw.Close();

                return true;
            }
            catch (IOException e)
            {
                Console.WriteLine($"Could not write to settings file: {e}");
                return false;
            }
        }

        /**
         * Read keybinds from file
         * If the file is not found, an empty dictionary is returned and the function returns false
         */
        public static bool ReadKeybinds(out Dictionary<KeybindAction, string> keybinds)
        {
            try
            {
                string fileName = $"Keybinds.txt";
                string fullPath = $"{SAVE_FILE_PATH}\\{fileName}";

                keybinds = new Dictionary<KeybindAction, string>();

                // If file does not exist, return false without error
                if (!File.Exists(fullPath))
                {
                    return false;
                }

                // otherwise, continue to read the file
                StreamReader sr = new StreamReader(fullPath);

                // Read each line which has separate slot data
                string[] lines = sr.ReadToEnd().Split('\n');

                sr.Close();

                foreach (string line in lines)
                {
                    if (string.IsNullOrEmpty(line)) continue;

                    string[] pair = line.Split('=');

                    if (pair.Length != 2)
                    {
                        Console.WriteLine($"Incorrect format for keybind data: {line}");
                        continue;
                    }

                    try
                    {
                        KeybindAction keybind = (KeybindAction)Enum.Parse(typeof(KeybindAction), pair[0].Trim());
                        keybinds.Add(keybind, pair[1].Trim());
                    }
                    catch (Exception)
                    {
                        Console.WriteLine($"Invalid keybind data: {pair[0].Trim()}={pair[1].Trim()}");
                    }
                }

                return true;
            }
            catch (IOException e)
            {
                Console.WriteLine($"Could not read keybinds file: {e}");
                keybinds = new Dictionary<KeybindAction, string>();
                return false;
            }
        }

        /**
         * Write setting to file
         */
        public static bool WriteKeybinds(Dictionary<KeybindAction, string> keybinds)
        {
            try
            {
                string fileName = $"Keybinds.txt";
                string fullPath = $"{SAVE_FILE_PATH}\\{fileName}";

                StringBuilder sb = new StringBuilder();

                // Go through rest of inventory to save to file
                foreach (KeybindAction action in keybinds.Keys)
                {
                    sb.AppendLine($"{action}={keybinds[action]}");
                }

                // remove last new line character
                sb.Remove(sb.Length - 1, 1);

                StreamWriter sw = new StreamWriter(fullPath);

                sw.Write(sb.ToString());

                sw.Close();

                return true;
            }
            catch (IOException e)
            {
                Console.WriteLine($"Could not write to keybinds file: {e}");
                return false;
            }
        }
    } // End RandomizerIO class
}

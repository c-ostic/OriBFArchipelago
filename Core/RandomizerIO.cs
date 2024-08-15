using System;
using System.Collections.Generic;
using System.IO;

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
        public static bool ReadSaveFile(int saveSlot, out RandomizerInventory inventory)
        {
            try
            {
                string fileName = $"Slot{saveSlot}.txt";
                string fullPath = $"{SAVE_FILE_PATH}\\{fileName}";

                StreamReader sr = new StreamReader(fullPath);

                // Get the version and slotname from file first
                string version = sr.ReadLine().Split('=')[1].Trim();
                string slotName = sr.ReadLine().Split('=')[1].Trim();

                // Create the inventory
                inventory = new RandomizerInventory(version, slotName);

                // Go through rest of data to add to inventory
                string[] data = sr.ReadToEnd().Split('\n');
                foreach (string line in data)
                {
                    string[] pair = line.Split('=');

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

                sr.Close();

                return true;
            }
            catch (IOException)
            {
                Console.WriteLine("Could not read save file");
                inventory = new RandomizerInventory("", "");
                return false;
            }
        }

        /** 
         * Saves the given inventory to a file
         */
        public static bool WriteSaveFile(int saveSlot, RandomizerInventory inventory)
        {
            try
            {
                string fileName = $"Slot{saveSlot}.txt";
                string fullPath = $"{SAVE_FILE_PATH}\\{fileName}";

                StreamWriter sw = new StreamWriter(fullPath);

                // Save the version and slotname first
                sw.WriteLine($"Version={inventory.Version}");
                sw.WriteLine($"SlotName={inventory.SlotName}");

                // Go through rest of inventory to save to file
                foreach (InventoryItem itemType in Enum.GetValues(typeof(InventoryItem)))
                {
                    sw.WriteLine($"{itemType}={inventory.Get(itemType)}");
                }

                sw.Close();

                return true;
            }
            catch (IOException)
            {
                Console.WriteLine("Could not write to save file");
                return false;
            }
        }

        /**
         * Deletes the specified save file
         */
        public static bool DeleteSaveFile(int saveSlot)
        {
            string fileName = $"Slot{saveSlot}.txt";
            string fullPath = $"{SAVE_FILE_PATH}\\{fileName}";

            try
            {
                File.Delete(fullPath);
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
         */
        public static bool ReadSlotData(out Dictionary<int, SlotData> data)
        {
            try
            {
                string fileName = $"SlotData.txt";
                string fullPath = $"{SAVE_FILE_PATH}\\{fileName}";

                StreamReader sr = new StreamReader(fullPath);

                // Special case if file does not exist, create a new file
                if (!File.Exists(fullPath))
                {
                    data = CreateSlotData();
                    WriteSlotData(data);
                    return true;
                }

                // Otherwise, continue and read the file
                data = new Dictionary<int, SlotData>();

                // Read each line which has separate slot data
                string[] lines = sr.ReadToEnd().Split('\n');

                foreach (string line in lines)
                {
                    string[] parts = line.Split(',');
                    SlotData slotData = new SlotData();
                    slotData.serverName = parts[1];
                    slotData.slotName = parts[2];
                    slotData.port = int.Parse(parts[3]);
                    slotData.password = parts[4];

                    data.Add(int.Parse(parts[0]), slotData);
                }

                return true;
            }
            catch (IOException)
            {
                Console.WriteLine("Could not read slot data file");
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

                StreamWriter sw = new StreamWriter(fullPath);

                foreach (KeyValuePair<int, SlotData> pair in data)
                {
                    sw.Write($"{pair.Key},");
                    sw.Write($"{pair.Value.serverName},");
                    sw.Write($"{pair.Value.slotName},");
                    sw.Write($"{pair.Value.port},");
                    sw.WriteLine($"{pair.Value.password}");
                }

                return true;
            }
            catch (IOException)
            {
                Console.WriteLine("Could not write to slot data file");
                return false;
            }
        }
    } // End RandomizerIO class
}

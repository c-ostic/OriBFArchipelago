using System;
using System.Collections.Generic;

namespace OriBFArchipelago.Core
{
    /**
     * Represents all items that could be received by the inventory
     */
    internal enum InventoryItem
    {
        AbilityCell,
        AbilityCellUsed,
        HealthCell,
        EnergyCell,

        MapStone,
        MapStoneUsed,
        GladesMapStone,
        GladesMapStoneUsed,
        GroveMapStone,
        GroveMapStoneUsed,
        GrottoMapStone,
        GrottoMapStoneUsed,
        SwampMapStone,
        SwampMapStoneUsed,
        ValleyMapStone,
        ValleyMapStoneUsed,
        ForlornMapStone,
        ForlornMapStoneUsed,
        SorrowMapStone,
        SorrowMapStoneUsed,
        HoruMapStone,
        HoruMapStoneUsed,
        BlackrootMapStone,
        BlackrootMapStoneUsed,

        KeyStone,
        KeyStoneUsed,
        GladesKeyStone,
        GladesKeyStoneUsed,
        GrottoKeyStone,
        GrottoKeyStoneUsed,
        GinsoKeyStone,
        GinsoKeyStoneUsed,
        SwampKeyStone,
        SwampKeyStoneUsed,
        MistyKeyStone,
        MistyKeyStoneUsed,
        ForlornKeyStone,
        ForlornKeyStoneUsed,
        SorrowKeyStone,
        SorrowKeyStoneUsed,

        GinsoKey,
        ForlornKey,
        HoruKey,
        CleanWater,
        Wind,

        WallJump,
        ChargeFlame,
        DoubleJump,
        Bash,
        Stomp,
        Glide,
        Climb,
        ChargeJump,
        Dash,
        Grenade,

        TPGlades,
        TPGrove,
        TPSwamp,
        TPGrotto,
        TPGinso,
        TPValley,
        TPForlorn,
        TPSorrow,
        TPHoru,
        TPBlackroot,

        EX15,
        EX50,
        EX100,
        EX200,
        EnemyEX,

        GinsoEscapeExit,
        GinsoEscapeComplete,
    }

    /**
     * Represents the inventory of currently connected randomizer
     */
    internal class RandomizerInventory
    {
        // useful list for skills
        public static List<InventoryItem> skills = new List<InventoryItem>()
        {
            InventoryItem.WallJump,
            InventoryItem.ChargeFlame,
            InventoryItem.DoubleJump,
            InventoryItem.Bash,
            InventoryItem.Stomp,
            InventoryItem.Glide,
            InventoryItem.Climb,
            InventoryItem.ChargeJump,
            InventoryItem.Dash,
            InventoryItem.Grenade
        };

        // useful list for teleporters
        public static List<InventoryItem> teleporters = new List<InventoryItem>()
        {
            InventoryItem.TPGlades,
            InventoryItem.TPGrove,
            InventoryItem.TPSwamp,
            InventoryItem.TPGrotto,
            InventoryItem.TPGinso,
            InventoryItem.TPValley,
            InventoryItem.TPForlorn,
            InventoryItem.TPSorrow,
            InventoryItem.TPHoru,
            InventoryItem.TPBlackroot
        };

        public string Version { get; private set; }
        public string SlotName { get; private set; }

        private Dictionary<InventoryItem, int> inventory;

        public RandomizerInventory(string version, string slotName)
        {
            Version = version;
            SlotName = slotName;
            Reset();
        }

        /**
         * Reset the inventory
         */
        public void Reset()
        {
            inventory = new Dictionary<InventoryItem, int>();
            foreach (InventoryItem itemType in Enum.GetValues(typeof(InventoryItem)))
            {
                inventory.Add(itemType, 0);
            }
        }

        /**
         * Add an item to the inventory. Optional count of items
         */
        public void Add(InventoryItem item, int count = 1)
        {
            inventory[item] += count;
        }

        /**
         * Add all items from the given inventory into this one
         */
        public void AddAll(RandomizerInventory other)
        {
            foreach (InventoryItem itemType in Enum.GetValues(typeof(InventoryItem)))
            {
                inventory[itemType] += other.Get(itemType);
            }
        }

        /**
         * Get the count of the specified item
         */
        public int Get(InventoryItem item)
        {
            return inventory[item];
        }

        /**
         * Get the total amount of experience gained (both from in-game and from archipelago)
         */
        public int GetTotalExperience()
        {
            int total = 0;
            total += inventory[InventoryItem.EX15] * 15;
            total += inventory[InventoryItem.EX50] * 50;
            total += inventory[InventoryItem.EX100] * 100;
            total += inventory[InventoryItem.EX200] * 200;
            total += inventory[InventoryItem.EnemyEX];
            return total;
        }

        /**
         * Compare the count of a specified item to another inventory 
         */
        public int CompareOn(RandomizerInventory other, InventoryItem item)
        {
            if (other == null) return 1;

            return inventory[item].CompareTo(other.inventory[item]);
        }
    }
}

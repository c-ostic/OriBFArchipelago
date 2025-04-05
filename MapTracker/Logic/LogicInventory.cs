using Game;
using OriBFArchipelago.Core;
using OriBFArchipelago.MapTracker.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OriBFArchipelago.MapTracker.Logic
{
    internal class LogicInventory
    {
        private static List<Teleporter> _teleporters;
        public static List<Teleporter> Teleporters
        {
            get
            {
                if (_teleporters != null)
                    return _teleporters;

                _teleporters = new List<Teleporter>
                {
                    //new Teleporter( new MoonGuid("535515012 1334527363 694602174 1078580914"),"horuFields",""), //Excluded due to enclosed area.
                    new Teleporter( new MoonGuid("-116578275 1111087997 412427670 -1249908721"), "forlorn", "TPForlorn"),
                    new Teleporter( new MoonGuid("1192718876 1302593798 1929767334 1228332312"),"ginsoTree","TPGinso"),
                    new Teleporter( new MoonGuid("1392867786 1221127759 -1187823465 2065923254"),"mangroveFalls","TPBlackroot"),
                    new Teleporter( new MoonGuid("1752643371 1284208868 -838119773 -772240063"),"moonGrotto","TPGrotto"),
                    new Teleporter( new MoonGuid("-222749108 1226712869 1550796190 1752513159"),"mountHoru","TPHoru"),
                    new Teleporter( new MoonGuid("-526679870 1154615959 -822258040 -1157635306"),"sorrowPass","TPValley"),
                    new Teleporter( new MoonGuid("1728896576 1241211625 810412199 -1853282216"),"spiritTree","TPGrove"),
                    new Teleporter( new MoonGuid("-426388372 1251161513 2131007642 -178890301"),"sunkenGlades","TPGlades"),
                    new Teleporter( new MoonGuid("1413930166 1176009348 411655079 -1337676822"),"swamp","TPSwamp"),
                    new Teleporter( new MoonGuid("290349702 1160050707 663397788 1544872441"),"valleyOfTheWind","TPSorrow") //Yes, valleyOfTheWind teleporter goes to sorrow peak and visa versa. This is a "bug" in the original game.
                };

                return _teleporters;
            }
        }

        public static Dictionary<string, int> Inventory { get; set; }

        public static void Clear()
        {
            if (Inventory != null)
                Inventory.Clear();

            Teleporters.ForEach(d => d.IsActivaded = false);
        }
        public static Dictionary<string, int> GetInventory()
        {
            InitializeInventory();
            return Inventory;
        }

        public static void UpdateInventory()
        {
            UpdateHealth();
            UpdateEnergy();
            UpdateTeleporters();
            UpdateKeys();
            UpdateKeystones();
            UpdateMapStones();
        }

        private static void UpdateMapStones()
        {
            //todo: implement mapstone area logic
            if (RandomizerManager.Options.MapStoneLogic == MapStoneOptions.AreaSpecific)
                UpdateAreaSpecificMapStones(); //todo implement
            else if (RandomizerManager.Options.MapStoneLogic == MapStoneOptions.Progressive)
                UpdateProgressiveMapStones(); //todo implement
            else
                UpdateGenericMapStones();

        }

        private static void UpdateAreaSpecificMapStones()
        {
            foreach (var item in EnumParser.GetEnumNames(typeof(InventoryItem)).Where(d => d.EndsWith("MapStone")))
            {
                var inventoryItem = EnumParser.GetEnumValue<InventoryItem>(item);
                SetInventoryValue(item, RandomizerManager.Receiver.GetItemCount(inventoryItem));
            }
        }
        private static void UpdateProgressiveMapStones()
        {
            //todo implement
        }

        private static void UpdateGenericMapStones()
        {
            SetInventoryValue("MapStone", RandomizerManager.Receiver.GetItemCount(InventoryItem.MapStone));
        }


        private static void UpdateKeystones()
        {
            //todo: implement keystone area logic
            if (RandomizerManager.Options.KeyStoneLogic == KeyStoneOptions.AreaSpecific)
                UpdateAreaSpecificKeystones(); //todo implement
            else
                UpdateGenericKeystones();
        }

        private static void UpdateAreaSpecificKeystones()
        {
            foreach (var item in EnumParser.GetEnumNames(typeof(InventoryItem)).Where(d => d.EndsWith("KeyStone")))
            {
                var inventoryItem = EnumParser.GetEnumValue<InventoryItem>(item);
                SetInventoryValue(item, RandomizerManager.Receiver.GetItemCount(inventoryItem));
            }
        }

        private static void UpdateGenericKeystones()
        {
            SetInventoryValue("KeyStone", RandomizerManager.Receiver.GetItemCount(InventoryItem.KeyStone));
        }

        private static void UpdateKeys()
        {
            if (Sein.World.Keys.GinsoTree)
                AddInventoryItem("GinsoKey");
            if (Sein.World.Events.WaterPurified)
                AddInventoryItem("CleanWater");
            if (Sein.World.Events.WindRestored)
                AddInventoryItem("Wind");
            if (Sein.World.Keys.MountHoru)
                AddInventoryItem("HoruKey");
        }

        private static void AddInventoryItem(string entryName, int value = 1)
        {
            InitializeInventory();
            if (!Inventory.ContainsKey(entryName))
            {
                ModLogger.Debug($"Added inventory entry {entryName}{(value > 1 ? $" with value {value}" : "")}");
                Inventory.Add(entryName, value);
            }
        }

        private static void SetInventoryValue(string entryName, int value)
        {
            InitializeInventory();
            if (value == 0)
                return;

            if (!Inventory.ContainsKey(entryName))
                AddInventoryItem(entryName, value);

            if (Inventory[entryName] != value)
            {
                Inventory[entryName] = value;
                ModLogger.Debug($"Setting inventory value {entryName} to {value}");
            }
        }

        public static void AddAbility(AbilityType ability)
        {
            AddInventoryItem(ability.ToString());
        }


        private static void UpdateTeleporters()
        {
            foreach (var teleporter in TeleporterController.Instance.Teleporters)
            {
                if (!teleporter.Activated)
                    continue;

                var match = Teleporters.FirstOrDefault(d => d.GameIdentifier.Equals(teleporter.Identifier));
                if (match == null || string.IsNullOrEmpty(match.LogicIdentifier))
                    continue;

                if (match.IsActivaded)
                    continue;

                match.IsActivaded = true;
                AddInventoryItem(match.LogicIdentifier);
            }
        }
        private static void UpdateHealth()
        {
            SetInventoryValue("HealthCell", (Characters.Sein?.Mortality?.Health?.MaxHealth ?? 12) / 4);
        }
        private static void UpdateEnergy()
        {
            SetInventoryValue("EnergyCell", (int)(Characters.Sein?.Energy?.Max ?? 0));
        }
        private static void InitializeInventory()
        {
            if (Inventory == null)
                Inventory = new Dictionary<string, int>();
        }




    }

    internal class Teleporter
    {
        public MoonGuid Guid { get; set; }
        public string GameIdentifier { get; set; }
        public string LogicIdentifier { get; set; }
        public bool IsActivaded { get; set; }
        public Teleporter(MoonGuid guid, string gameIdentifier, string logicIdentifier, bool isActivated = false)
        {
            Guid = guid;
            GameIdentifier = gameIdentifier;
            LogicIdentifier = logicIdentifier;
            IsActivaded = isActivated;
        }

    }
}

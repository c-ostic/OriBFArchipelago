using Game;
using System;
using System.Collections.Generic;

namespace OriBFArchipelago.Core
{
    /**
     * Manages the randomizer inventory and receiving of new items
     */
    internal class RandomizerReceiver : ISuspendable
    {
        // Version of this randomizer (probably move this to an outer class)
        private const string VERSION = "0.1.0";

        // queue to make sure items are only granted to the player when the player is active and has control
        private Queue<InventoryItem> itemQueue;

        // the inventory loaded from file
        private RandomizerInventory savedInventory;

        // the inventory used for when the user reopens the game and archipelago resends previously granted items
        // compares against savedInventory to make sure items aren't duplicated
        private RandomizerInventory onLoadInventory;

        // the number of the associated save slot in Ori
        private int saveSlot;

        // resync once upon loading into the game
        private bool resyncedOnLoad;

        // the name of the slot associated with this instance of archipelago
        public string SlotName { get { return savedInventory.SlotName; } }

        public bool IsSuspended { get; set; }

        /**
         * Initialize the randomizer reciever
         * Returns false if the given slotname does not match the slotname in the loaded save file
         */
        public bool Init(bool isNew, int saveSlot, string apSlotName)
        {
            SuspensionManager.Register(this);

            itemQueue = new Queue<InventoryItem>();
            resyncedOnLoad = false;

            onLoadInventory = new RandomizerInventory(VERSION, apSlotName);

            if (isNew)
            {
                // If this is a new slot, create a new inventory
                savedInventory = new RandomizerInventory(VERSION, apSlotName);
                if (RandomizerIO.WriteSaveFile(saveSlot, savedInventory))
                {
                    Console.Write($"Successfully created new inventory for slot {saveSlot}");
                }
                else
                {
                    Console.Write($"Could not create inventory for slot {saveSlot}");
                }
            }
            else
            {
                // Otherwise, load the inventory from file
                if (RandomizerIO.ReadSaveFile(saveSlot, out savedInventory))
                {
                    Console.Write($"Successfully loaded inventory from slot {saveSlot}");
                }
                else
                {
                    Console.Write($"Could not load inventory from slot {saveSlot}");
                }
            }

            return apSlotName == SlotName;
        }

        /**
         * Called every unity update frame (from parent class). Used to process items received from archipelago
         */
        public void Update()
        {
            if (Characters.Sein is null || IsSuspended)
                return;

            // The first time the player loads, resync
            if (!resyncedOnLoad)
            {
                Resync();
                resyncedOnLoad = true;
            }

            // If there are any items in the queue, process them one by one
            while (itemQueue.Count > 0)
            {
                InventoryItem itemName = itemQueue.Peek();

                switch (itemName)
                {
                    case InventoryItem.AbilityCell:
                        ReceiveAbilityCell(); break;
                    case InventoryItem.HealthCell:
                        ReceiveHealthCell(); break;
                    case InventoryItem.EnergyCell:
                        ReceiveEnergyCell(); break;
                    case InventoryItem.KeyStone:
                        ReceiveKeyStones(); break;
                    case InventoryItem.MapStone:
                        ReceiveMapStones(); break;
                    case InventoryItem.GinsoKey:
                    case InventoryItem.ForlornKey:
                    case InventoryItem.HoruKey:
                    case InventoryItem.CleanWater:
                    case InventoryItem.Wind:
                        ReceiveWorldEvent(itemName); break;
                    case InventoryItem.WallJump:
                    case InventoryItem.ChargeFlame:
                    case InventoryItem.DoubleJump:
                    case InventoryItem.Bash:
                    case InventoryItem.Stomp:
                    case InventoryItem.Glide:
                    case InventoryItem.Climb:
                    case InventoryItem.ChargeJump:
                    case InventoryItem.Dash:
                    case InventoryItem.Grenade:
                        ReceiveSkill(itemName); break;
                    case InventoryItem.TPGlades:
                    case InventoryItem.TPGrove:
                    case InventoryItem.TPSwamp:
                    case InventoryItem.TPGrotto:
                    case InventoryItem.TPGinso:
                    case InventoryItem.TPValley:
                    case InventoryItem.TPForlorn:
                    case InventoryItem.TPSorrow:
                    case InventoryItem.TPHoru:
                    case InventoryItem.TPBlackroot:
                        ReceiveTeleporter(itemName); break;
                    case InventoryItem.EX15:
                    case InventoryItem.EX50:
                    case InventoryItem.EX100:
                    case InventoryItem.EX200:
                        ReceiveSpiritLight(itemName); break;
                }

                Console.WriteLine("Received " + itemName);

                itemQueue.Dequeue();
            }
        }

        /** 
         * Called when receiving an item from archipelago
         */
        public void ReceiveItem(InventoryItem item)
        {
            if (onLoadInventory.CompareOn(savedInventory, item) < 0)
            {
                // if onLoadInventory isn't caught up to savedInventory, this is a previously received item
                onLoadInventory.Add(item);
            }
            else
            {
                // Otherwise, add the item to the queue
                itemQueue.Enqueue(item);
            }
        }

        /**
         * Called when the player dies
         */
        public void OnDeath()
        {
            // Resync items to bypass the death rollback
            Console.WriteLine("Resyncing items...");
            Resync();
        }

        /**
         * Called when the player reaches/creates a checkpoint and saves the game
         */
        public void OnSave()
        {
            Console.WriteLine("Saving...");
            RandomizerIO.WriteSaveFile(saveSlot, savedInventory);
            Resync();
        }

        /**
         * Resync the player's in-game inventory with the randomizer inventory
         */
        private void Resync()
        {
            while (savedInventory.Get(InventoryItem.AbilityCell) > Characters.Sein.Inventory.SkillPointsCollected)
                ReceiveAbilityCell();

            while (savedInventory.Get(InventoryItem.EnergyCell) > Characters.Sein.Energy.Max)
                ReceiveEnergyCell();

            while (savedInventory.Get(InventoryItem.HealthCell) > Characters.Sein.Mortality.Health.HealthUpgradesCollected)
                ReceiveHealthCell();

            int keyStonesRemaining = savedInventory.Get(InventoryItem.KeyStone) - savedInventory.Get(InventoryItem.KeyStoneUsed);
            while (keyStonesRemaining > Characters.Sein.Inventory.Keystones)
                ReceiveKeyStones();

            int mapStonesRemaining = savedInventory.Get(InventoryItem.MapStone) - savedInventory.Get(InventoryItem.MapStoneUsed);
            while (mapStonesRemaining > Characters.Sein.Inventory.MapStones)
                ReceiveMapStones();

            // using >= here instead of == just in case player somehow receives more than one (due to bugs or future changes)
            Sein.World.Keys.GinsoTree = savedInventory.Get(InventoryItem.GinsoKey) >= 1;
            Sein.World.Keys.ForlornRuins = savedInventory.Get(InventoryItem.ForlornKey) >= 1;
            Sein.World.Keys.MountHoru = savedInventory.Get(InventoryItem.HoruKey) >= 1;
            Sein.World.Events.WaterPurified = savedInventory.Get(InventoryItem.CleanWater) >= 1;
            Sein.World.Events.WindRestored = savedInventory.Get(InventoryItem.Wind) >= 1;

            foreach (InventoryItem skillName in RandomizerInventory.skills)
            {
                AbilityType skill = (AbilityType)Enum.Parse(typeof(AbilityType), skillName.ToString());
                Characters.Sein.PlayerAbilities.SetAbility(skill, savedInventory.Get(skillName) >= 1);
            }

            foreach (InventoryItem teleporterName in RandomizerInventory.teleporters)
            {
                if (savedInventory.Get(teleporterName) >= 1)
                {
                    TeleporterController.Activate(tpMap[teleporterName]);
                }
            }

            int expDiff = savedInventory.GetTotalExperience() - Characters.Sein.Level.TotalExperience;
            Characters.Sein.Level.GainExperience(expDiff);
        }

        /**
         * All of the functions to receive items
         */
        #region Receive Functions

        private void ReceiveSkill(InventoryItem skillName)
        {
            var skill = (AbilityType)Enum.Parse(typeof(AbilityType), skillName.ToString());
            Characters.Sein.PlayerAbilities.SetAbility(skill, true);
        }

        private void ReceiveEnergyCell()
        {
            var sein = Characters.Sein;
            if (sein.Energy.Max == 0f)
                sein.SoulFlame.FillSoulFlameBar();

            sein.Energy.Max += 1f;
            if (Characters.Sein.Energy.Current < Characters.Sein.Energy.Max)
                sein.Energy.Current = sein.Energy.Max;

            UI.SeinUI.ShakeEnergyOrbBar();
        }

        private void ReceiveSpiritLight(InventoryItem exp)
        {
            int amount = int.Parse(exp.ToString().Substring(2));
            Characters.Sein.Level.GainExperience(amount);
        }

        private void ReceiveHealthCell()
        {
            Characters.Sein.Mortality.Health.GainMaxHeartContainer();
            UI.SeinUI.ShakeHealthbar();
        }

        private void ReceiveAbilityCell()
        {
            Characters.Sein.Level.GainSkillPoint();
            Characters.Sein.Inventory.SkillPointsCollected++;
            UI.SeinUI.ShakeExperienceBar();
        }

        private void ReceiveKeyStones()
        {
            Characters.Sein.Inventory.CollectKeystones(1);
            UI.SeinUI.ShakeKeystones();
        }

        private void ReceiveMapStones()
        {
            Characters.Sein.Inventory.MapStones++;
            UI.SeinUI.ShakeMapstones();
        }

        private void ReceiveWorldEvent(InventoryItem eventName)
        {
            switch (eventName)
            {
                case InventoryItem.GinsoKey:
                    Sein.World.Keys.GinsoTree = true;
                    break;
                case InventoryItem.CleanWater:
                    Sein.World.Events.WaterPurified = true;
                    break;

                case InventoryItem.ForlornKey:
                    Sein.World.Keys.ForlornRuins = true;
                    break;
                case InventoryItem.Wind:
                    Sein.World.Events.WindRestored = true;
                    break;

                case InventoryItem.HoruKey:
                    Sein.World.Keys.MountHoru = true;
                    break;
            }
        }

        private static readonly Dictionary<InventoryItem, string> tpMap = new Dictionary<InventoryItem, string>
        {
            [InventoryItem.TPForlorn] = "forlorn",
            [InventoryItem.TPGrotto] = "moonGrotto",
            [InventoryItem.TPSorrow] = "valleyOfTheWind",
            [InventoryItem.TPGrove] = "spiritTree",
            [InventoryItem.TPSwamp] = "swamp",
            [InventoryItem.TPValley] = "sorrowPass",
            [InventoryItem.TPGinso] = "ginsoTree",
            [InventoryItem.TPHoru] = "mountHoru",
            [InventoryItem.TPGlades] = "sunkenGlades",
            [InventoryItem.TPBlackroot] = "mangroveFalls"
        };
        private void ReceiveTeleporter(InventoryItem teleporterName)
        {
            TeleporterController.Activate(tpMap[teleporterName]);
        }

        #endregion
    }
}

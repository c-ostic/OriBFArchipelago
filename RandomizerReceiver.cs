using Game;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OriBFArchipelago
{
    internal class RandomizerReceiver : MonoBehaviour, ISuspendable
    {
        private static RandomizerReceiver instance;

        // queue to make sure items are only granted to the player when the player is active and has control
        private Queue<string> itemQueue;

        // queue of any items received since the last checkpoint
        // regranted to the player upon death; emptied into inventories on save
        private Queue<string> unsavedItems;

        // the inventory loaded from file
        private RandomizerInventory savedInventory;

        // the inventory used for when the user reopens the game and archipelago resends previously granted items
        // compares against savedInventory to make sure items aren't duplicated
        private RandomizerInventory onLoadInventory;

        // the name of the slot associated with this instance of archipelago
        private string slot;

        private bool resyncedOnLoad;

        public bool IsSuspended { get; set; }

        private void Awake() 
        { 
            itemQueue = new Queue<string>();
            unsavedItems = new Queue<string>();
            SuspensionManager.Register(this);
            instance = this;
            resyncedOnLoad = false;
        }

        private void Update()
        {
            if (Characters.Sein is null || IsSuspended) 
                return;

            if (!resyncedOnLoad)
            {
                Resync();
                resyncedOnLoad = true;
            }

            while (itemQueue.Count > 0)
            {
                string itemName = itemQueue.Peek();

                switch (itemName)
                {
                    case "AbilityCell":
                        ReceiveAbilityCell(); break;
                    case "HealthCell":
                        ReceiveHealthCell(); break;
                    case "EnergyCell":
                        ReceiveEnergyCell(); break;
                    case "KeyStone":
                        ReceiveKeyStones(); break;
                    case "Plant":
                        ReceiveSpiritLight("EX50"); break;
                    case "MapStone":
                        ReceiveMapStones(); break;
                    case "GinsoKey":
                    case "ForlornKey":
                    case "HoruKey":
                    case "CleanWater":
                    case "Wind":
                        ReceiveWorldEvent(itemName); break;
                    case "WallJump":
                    case "ChargeFlame":
                    case "DoubleJump":
                    case "Bash":
                    case "Stomp":
                    case "Glide":
                    case "Climb":
                    case "ChargeJump":
                    case "Dash":
                    case "Grenade":
                        ReceiveSkill(itemName); break;
                    case "TPGlades":
                    case "TPGrove":
                    case "TPSwamp":
                    case "TPGrotto":
                    case "TPGinso":
                    case "TPValley":
                    case "TPForlorn":
                    case "TPSorrow":
                    case "TPHoru":
                    case "TPBlackroot":
                        ReceiveTeleporter(itemName); break;
                    case "EX15":
                    case "EX100":
                    case "EX200":
                    case "EX50":
                        ReceiveSpiritLight(itemName); break;
                }

                Console.WriteLine("Received " + itemName);

                itemQueue.Dequeue();
            }
        }

        private void OnDestroy()
        {
            // empty remaining contents of unsaved inventory into saved inventory
            OnSave();
            savedInventory.Save(slot);
        }

        public void LoadFileInventory(string slot)
        {
            this.slot = slot;
            savedInventory = new RandomizerInventory(slot);
            onLoadInventory = new RandomizerInventory();
        }

        // Called when receiving an item from archipelago
        public void ReceiveItem(string item)
        {
            if (onLoadInventory.CompareOn(savedInventory, item) < 0)
            {
                onLoadInventory.Add(item);
            }
            else
            {
                unsavedItems.Enqueue(item);
                itemQueue.Enqueue(item);
            }
        }

        // Called when the player dies
        public static void OnDeath()
        {
            Console.WriteLine("Regranting items...");
            foreach (string item in instance.unsavedItems)
            {
                instance.itemQueue.Enqueue(item);
            }
        }

        // Called when the player reaches/creates a checkpoint and saves the game
        public static void OnSave()
        {
            Console.WriteLine("Saving...");
            foreach (string item in instance.unsavedItems)
            {
                instance.savedInventory.Add(item);
                instance.onLoadInventory.Add(item);
            }
            instance.savedInventory.Save(instance.slot);
            instance.unsavedItems.Clear();
            instance.Resync();
        }

        private void Resync()
        {
            while (savedInventory.abilityCells > Characters.Sein.Inventory.SkillPointsCollected)
                ReceiveAbilityCell();

            while (savedInventory.energyCells > Characters.Sein.Energy.Max)
                ReceiveEnergyCell();

            while (savedInventory.healthCells > Characters.Sein.Mortality.Health.HealthUpgradesCollected)
                ReceiveHealthCell();

            while (savedInventory.keyStones > Characters.Sein.Inventory.Keystones)
                ReceiveKeyStones();

            while (savedInventory.mapStones > Characters.Sein.Inventory.MapStones)
                ReceiveMapStones();

            Sein.World.Keys.GinsoTree = savedInventory.ginsoKey;
            Sein.World.Keys.ForlornRuins = savedInventory.forlornKey;
            Sein.World.Keys.MountHoru = savedInventory.horuKey;
            Sein.World.Events.WaterPurified = savedInventory.cleanWater;
            Sein.World.Events.WindRestored = savedInventory.wind;

            foreach (AbilityType skill in savedInventory.abilities.Keys)
            {
                Characters.Sein.PlayerAbilities.SetAbility(skill, savedInventory.abilities[skill]);
            }

            foreach (string teleporter in savedInventory.teleporters.Keys)
            {
                if (savedInventory.teleporters[teleporter])
                {
                    TeleporterController.Activate(tpMap[teleporter]);
                }
            }
        }

        private void ReceiveSkill(string skillName)
        {
            var skill = (AbilityType)Enum.Parse(typeof(AbilityType), skillName);
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

        private void ReceiveSpiritLight(string exp)
        {
            int amount = Int32.Parse(exp.Substring(2));
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

        private void ReceiveWorldEvent(string eventName)
        {
            switch (eventName)
            {
                case "GinsoKey":
                    Sein.World.Keys.GinsoTree = true;
                    break;
                case "CleanWater":
                    Sein.World.Events.WaterPurified = true;
                    break;

                case "ForlornKey":
                    Sein.World.Keys.ForlornRuins = true;
                    break;
                case "Wind":
                    Sein.World.Events.WindRestored = true;
                    break;

                case "HoruKey":
                    Sein.World.Keys.MountHoru = true;
                    break;
            }
        }

        private static readonly Dictionary<string, string> tpMap = new Dictionary<string, string>
        {
            ["TPForlorn"] = "forlorn",
            ["TPGrotto"] = "moonGrotto",
            ["TPSorrow"] = "valleyOfTheWind",
            ["TPGrove"] = "spiritTree",
            ["TPSwamp"] = "swamp",
            ["TPValley"] = "sorrowPass",
            ["TPGinso"] = "ginsoTree",
            ["TPHoru"] = "mountHoru",
            ["TPGlades"] = "sunkenGlades",
            ["TPBlackroot"] = "mangroveFalls"
        };
        private void ReceiveTeleporter(string teleporterName)
        {
            TeleporterController.Activate(tpMap[teleporterName]);
        }
    }
}

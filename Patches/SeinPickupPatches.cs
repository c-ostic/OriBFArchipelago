using Game;
using HarmonyLib;
using OriBFArchipelago.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OriBFArchipelago.Patches
{
    [HarmonyPatch(typeof(SeinPickupProcessor), nameof(SeinPickupProcessor.OnCollectSkillPointPickup))]
    internal class AbilityCellPatch
    {
        static bool Prefix(SkillPointPickup skillPointPickup)
        {
            skillPointPickup.Collected();
            RandomizerManager.Connection.CheckLocationByGameObject(skillPointPickup.gameObject);
            return false;
        }
    }

    [HarmonyPatch(typeof(SkillTreeManager), nameof(SkillTreeManager.OnMenuItemPressed))]
    internal class SpendAbilityCellPatch
    {
        static bool Prefix()
        {
            if (SkillTreeManager.Instance.CurrentSkillItem.CanEarnSkill)
            {
                int cost = SkillTreeManager.Instance.CurrentSkillItem.ActualRequiredSkillPoints;
                RandomizerManager.Receiver.ReceiveItem(InventoryItem.AbilityCellUsed, cost);
                Console.WriteLine("Ability point used: " + cost);
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(SeinPickupProcessor), nameof(SeinPickupProcessor.OnCollectMaxEnergyContainerPickup))]
    internal class EnergyCellPatch
    {
        static bool Prefix(MaxEnergyContainerPickup energyContainerPickup)
        {
            energyContainerPickup.Collected();
            RandomizerManager.Connection.CheckLocationByGameObject(energyContainerPickup.gameObject);
            return false;
        }
    }

    [HarmonyPatch(typeof(SeinPickupProcessor), nameof(SeinPickupProcessor.OnCollectMaxHealthContainerPickup))]
    internal class HealthCellPatch
    {
        static bool Prefix(MaxHealthContainerPickup maxHealthContainerPickup)
        {
            maxHealthContainerPickup.Collected();
            RandomizerManager.Connection.CheckLocationByGameObject(maxHealthContainerPickup.gameObject);
            return false;
        }
    }

    [HarmonyPatch(typeof(SeinPickupProcessor), nameof(SeinPickupProcessor.OnCollectKeystonePickup))]
    internal class KeyStonePatch
    {
        static bool Prefix(KeystonePickup keystonePickup)
        {
            keystonePickup.Collected();
            RandomizerManager.Connection.CheckLocationByGameObject(keystonePickup.gameObject);
            return false;
        }
    }

    [HarmonyPatch(typeof(SeinInventory), nameof(SeinInventory.SpendKeystones))]
    internal class SpendKeyStonePatch
    {
        static bool Prefix(int cost)
        {
            RandomizerManager.Receiver.ReceiveItem(InventoryItem.KeyStoneUsed, cost);
            return true;
        }
    }

    [HarmonyPatch(typeof(DoorWithSlots), nameof(DoorWithSlots.RestoreOrbs))]
    internal class RestoreKeyStonePatch
    {
        static bool Prefix(DoorWithSlots __instance)
        {
            // sending a negative number to receive will effectively decrement the item count
            int amount = __instance.NumberOfOrbsUsed;
            RandomizerManager.Receiver.ReceiveItem(InventoryItem.KeyStoneUsed, amount * -1);
            return true;
        }
    }

    [HarmonyPatch(typeof(SeinPickupProcessor), nameof(SeinPickupProcessor.OnCollectMapStonePickup))]
    internal class MapStoneFragmentPatch
    {
        static bool Prefix(MapStonePickup mapStonePickup)
        {
            mapStonePickup.Collected();
            RandomizerManager.Connection.CheckLocationByGameObject(mapStonePickup.gameObject);
            return false;
        }
    }

    [HarmonyPatch(typeof(AchievementsLogic), nameof(AchievementsLogic.OnMapStoneActivated))]
    internal class SpendMapStonePatch
    {
        static bool Prefix()
        {
            RandomizerManager.Receiver.ReceiveItem(InventoryItem.MapStoneUsed);
            Console.WriteLine("Mapstone used");
            return true;
        }
    }

    [HarmonyPatch(typeof(SeinPickupProcessor), nameof(SeinPickupProcessor.OnCollectExpOrbPickup))]
    internal class ExpOrbPatch
    {
        static bool Prefix(ExpOrbPickup expOrbPickup)
        {
            if (expOrbPickup.MessageType != ExpOrbPickup.ExpOrbMessageType.None)
            {
                // these are spirit light containers
                expOrbPickup.Collected();
                RandomizerManager.Connection.CheckLocationByGameObject(expOrbPickup.gameObject);
                return false;
            }
            else
            {
                // these are exp orbs dropped by enemies, so normal execution should continue to postfix
                return true;
            }
        }

        static void Postfix(ExpOrbPickup expOrbPickup)
        {
            if (expOrbPickup.MessageType == ExpOrbPickup.ExpOrbMessageType.None)
            {
                // these are exp orbs dropped by enemies, so add to inventory to track when gaining enemy exp
                int num = expOrbPickup.Amount * ((!Characters.Sein.PlayerAbilities.SoulEfficiency.HasAbility) ? 1 : 2);
                RandomizerManager.Receiver.ReceiveItem(InventoryItem.EnemyEX, num);
            }
        }
    }
}

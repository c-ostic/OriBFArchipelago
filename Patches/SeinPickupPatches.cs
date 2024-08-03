using Game;
using HarmonyLib;
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
            ArchipelagoManager.CheckLocationByGameObject(skillPointPickup.gameObject);
            return false;
        }
    }

    [HarmonyPatch(typeof(SeinPickupProcessor), nameof(SeinPickupProcessor.OnCollectMaxEnergyContainerPickup))]
    internal class EnergyCellPatch
    {
        static bool Prefix(MaxEnergyContainerPickup energyContainerPickup)
        {
            energyContainerPickup.Collected();
            ArchipelagoManager.CheckLocationByGameObject(energyContainerPickup.gameObject);
            return false;
        }
    }

    [HarmonyPatch(typeof(SeinPickupProcessor), nameof(SeinPickupProcessor.OnCollectMaxHealthContainerPickup))]
    internal class HealthCellPatch
    {
        static bool Prefix(MaxHealthContainerPickup maxHealthContainerPickup)
        {
            maxHealthContainerPickup.Collected();
            ArchipelagoManager.CheckLocationByGameObject(maxHealthContainerPickup.gameObject);
            return false;
        }
    }

    [HarmonyPatch(typeof(SeinPickupProcessor), nameof(SeinPickupProcessor.OnCollectKeystonePickup))]
    internal class KeyStonePatch
    {
        static bool Prefix(KeystonePickup keystonePickup)
        {
            keystonePickup.Collected();
            ArchipelagoManager.CheckLocationByGameObject(keystonePickup.gameObject);
            return false;
        }
    }

    [HarmonyPatch(typeof(SeinPickupProcessor), nameof(SeinPickupProcessor.OnCollectMapStonePickup))]
    internal class MapStoneFragmentPatch
    {
        static bool Prefix(MapStonePickup mapStonePickup)
        {
            mapStonePickup.Collected();
            ArchipelagoManager.CheckLocationByGameObject(mapStonePickup.gameObject);
            return false;
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
                ArchipelagoManager.CheckLocationByGameObject(expOrbPickup.gameObject);
                return false;
            }
            else
            {
                // these are exp orbs dropped by enemies, so normal execution should continue
                return true;
            }
        }
    }
}

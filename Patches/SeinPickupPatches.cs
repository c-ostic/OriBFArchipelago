using Game;
using HarmonyLib;
using OriBFArchipelago.Core;
using OriBFArchipelago.Extensions;
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
            RandomizerManager.Connection.CheckLocation(skillPointPickup.GetComponent<VisibleOnWorldMap>().MoonGuid);
            return false;
        }
    }

    [HarmonyPatch(typeof(SkillTreeManager), nameof(SkillTreeManager.OnMenuItemPressed))]
    internal class SpendAbilityCellPatch
    {
        static bool Prefix()
        {
            if (SkillTreeManager.Instance.CurrentSkillItem != null &&
                !SkillTreeManager.Instance.CurrentSkillItem.HasSkillItem &&
                SkillTreeManager.Instance.CurrentSkillItem.CanEarnSkill)
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
            RandomizerManager.Connection.CheckLocation(energyContainerPickup.GetComponent<VisibleOnWorldMap>().MoonGuid);
            return false;
        }
    }

    [HarmonyPatch(typeof(SeinPickupProcessor), nameof(SeinPickupProcessor.OnCollectMaxHealthContainerPickup))]
    internal class HealthCellPatch
    {
        static bool Prefix(MaxHealthContainerPickup maxHealthContainerPickup)
        {
            maxHealthContainerPickup.Collected();
            RandomizerManager.Connection.CheckLocation(maxHealthContainerPickup.GetComponent<VisibleOnWorldMap>().MoonGuid);
            return false;
        }
    }

    [HarmonyPatch(typeof(SeinPickupProcessor), nameof(SeinPickupProcessor.OnCollectKeystonePickup))]
    internal class KeyStonePatch
    {
        static bool Prefix(KeystonePickup keystonePickup)
        {
            keystonePickup.Collected();
            RandomizerManager.Connection.CheckLocation(keystonePickup.GetComponent<VisibleOnWorldMap>().MoonGuid);
            return false;
        }
    }

    [HarmonyPatch(typeof(SeinInventory), nameof(SeinInventory.SpendKeystones))]
    internal class SpendKeyStonePatch
    {
        static bool Prefix(int cost)
        {
            InventoryItem keystoneUsed = InventoryItem.KeyStoneUsed;
            if (RandomizerManager.Options.KeyStoneLogic == KeyStoneOptions.AreaSpecific)
            {
                WorldArea currentWorldArea = Characters.Sein.CurrentWorldArea();
                switch (currentWorldArea)
                {
                    case WorldArea.Glades:
                        keystoneUsed = InventoryItem.GladesKeyStoneUsed;
                        break;
                    case WorldArea.Grotto:
                        keystoneUsed = InventoryItem.GrottoKeyStoneUsed;
                        break;
                    case WorldArea.Ginso:
                        keystoneUsed = InventoryItem.GinsoKeyStoneUsed;
                        break;
                    case WorldArea.Swamp:
                        keystoneUsed = InventoryItem.SwampKeyStoneUsed;
                        break;
                    case WorldArea.Misty:
                        keystoneUsed = InventoryItem.MistyKeyStoneUsed;
                        break;
                    case WorldArea.Forlorn:
                        keystoneUsed = InventoryItem.ForlornKeyStoneUsed;
                        break;
                    case WorldArea.Sorrow:
                        keystoneUsed = InventoryItem.SorrowKeyStoneUsed;
                        break;
                    default:
                        // If we somehow are in an invalid area, then we should not do anything.
                        return false;
                }
            }
            RandomizerManager.Receiver.ReceiveItem(keystoneUsed, cost);
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
            InventoryItem keystoneUsed = InventoryItem.KeyStoneUsed;

            if (RandomizerManager.Options.KeyStoneLogic == KeyStoneOptions.AreaSpecific)
            {
                WorldArea currentWorldArea = Characters.Sein.CurrentWorldArea();
                switch (currentWorldArea)
                {
                    case WorldArea.Glades:
                        keystoneUsed = InventoryItem.GladesKeyStoneUsed;
                        break;
                    case WorldArea.Grotto:
                        keystoneUsed = InventoryItem.GrottoKeyStoneUsed;
                        break;
                    case WorldArea.Ginso:
                        keystoneUsed = InventoryItem.GinsoKeyStoneUsed;
                        break;
                    case WorldArea.Swamp:
                        keystoneUsed = InventoryItem.SwampKeyStoneUsed;
                        break;
                    case WorldArea.Misty:
                        keystoneUsed = InventoryItem.MistyKeyStoneUsed;
                        break;
                    case WorldArea.Forlorn:
                        keystoneUsed = InventoryItem.ForlornKeyStoneUsed;
                        break;
                    case WorldArea.Sorrow:
                        keystoneUsed = InventoryItem.SorrowKeyStoneUsed;
                        break;
                    default:
                        // If we somehow are in an invalid area, then we should not do anything.
                        return false;
                }
            }
            RandomizerManager.Receiver.ReceiveItem(keystoneUsed, amount * -1);
            return true;
        }
    }

    [HarmonyPatch(typeof(SeinPickupProcessor), nameof(SeinPickupProcessor.OnCollectMapStonePickup))]
    internal class MapStoneFragmentPatch
    {
        static bool Prefix(MapStonePickup mapStonePickup)
        {
            mapStonePickup.Collected();
            RandomizerManager.Connection.CheckLocation(mapStonePickup.GetComponent<VisibleOnWorldMap>().MoonGuid);
            return false;
        }
    }

    [HarmonyPatch(typeof(AchievementsLogic), nameof(AchievementsLogic.OnMapStoneActivated))]
    internal class SpendMapStonePatch
    {
        static bool Prefix()
        {
            InventoryItem mapstoneUsed = InventoryItem.MapStoneUsed;
            if (RandomizerManager.Options.MapStoneLogic == MapStoneOptions.AreaSpecific)
            {
                WorldArea currentWorldArea = Characters.Sein.CurrentWorldArea();
                switch (currentWorldArea)
                {
                    case WorldArea.Glades:
                        mapstoneUsed = InventoryItem.GladesMapStoneUsed;
                        break;
                    case WorldArea.Grove:
                        mapstoneUsed = InventoryItem.GroveMapStoneUsed;
                        break;
                    case WorldArea.Grotto:
                        mapstoneUsed = InventoryItem.GrottoMapStoneUsed;
                        break;
                    case WorldArea.Swamp:
                        mapstoneUsed = InventoryItem.SwampMapStoneUsed;
                        break;
                    case WorldArea.Valley:
                        mapstoneUsed = InventoryItem.ValleyMapStoneUsed;
                        break;
                    case WorldArea.Forlorn:
                        mapstoneUsed = InventoryItem.ForlornMapStoneUsed;
                        break;
                    case WorldArea.Sorrow:
                        mapstoneUsed = InventoryItem.SorrowMapStoneUsed;
                        break;
                    case WorldArea.Horu:
                        mapstoneUsed = InventoryItem.HoruMapStoneUsed;
                        break;
                    case WorldArea.Blackroot:
                        mapstoneUsed = InventoryItem.BlackrootMapStoneUsed;
                        break;
                    default:
                        // If we somehow are in an invalid area, then we should not do anything.
                        return false;
                }
            }
            RandomizerManager.Receiver.ReceiveItem(mapstoneUsed);
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
                RandomizerManager.Connection.CheckLocation(expOrbPickup.GetComponent<VisibleOnWorldMap>().MoonGuid);
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

    [HarmonyPatch(typeof(DoorWithSlots), nameof(DoorWithSlots.FixedUpdate))]
    internal class DoorWithSlotsKeystonesPatch
    {
        private static int GetCurrentKeystonesCount() => RandomizerManager.Receiver.GetCurrentKeystonesCount();

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = instructions.ToList();

            var seinField = AccessTools.Field(typeof(Game.Characters), nameof(Game.Characters.Sein));
            var field = AccessTools.Field(typeof(SeinInventory), nameof(SeinInventory.Keystones));
            for (int i = 0; i < codes.Count; i++)
            {

                if (codes[i].LoadsField(seinField) && codes[i + 2].LoadsField(field))
                {
                    CodeInstruction keystonesInstruction = CodeInstruction.Call(typeof(DoorWithSlotsKeystonesPatch), nameof(DoorWithSlotsKeystonesPatch.GetCurrentKeystonesCount));
                    keystonesInstruction.MoveLabelsFrom(codes[i]);
                    i += 2;
                    yield return keystonesInstruction;
                }
                else
                {
                    yield return codes[i];
                }
            }
        }
    }

    [HarmonyPatch(typeof(SeinKeystonesFloatProvider), nameof(SeinKeystonesFloatProvider.GetFloatValue))]
    internal class SeinKeystonesFloatProviderPatch
    {
        private static bool Prefix(ref float __result)
        {
            __result = (float) RandomizerManager.Receiver.GetCurrentKeystonesCount();
            return false;
        }
    }

    [HarmonyPatch(typeof(SeinMapstonesFloatProvider), nameof(SeinMapstonesFloatProvider.GetFloatValue))]
    internal class SeinMapstonesFloatProviderPatch
    {
        private static bool Prefix(ref float __result)
        {
            __result = (float)RandomizerManager.Receiver.GetCurrentMapstonesCount();
            return false;
        }
    }
}

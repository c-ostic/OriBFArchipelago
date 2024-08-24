using HarmonyLib;
using OriBFArchipelago.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OriBFArchipelago.Patches
{
    [HarmonyPatch(typeof(SetSeinWorldStateAction), nameof(SetSeinWorldStateAction.Perform))]
    internal class WorldEventsPatch
    {
        private static bool Prefix(SetSeinWorldStateAction __instance)
        {
            switch (__instance.State)
            { 
                case WorldState.WaterPurified:
                    RandomizerManager.Connection.CheckLocation("GinsoEscapeExit");
                    return false;
                case WorldState.GinsoTreeKey:
                    RandomizerManager.Connection.CheckLocation("WaterVein");
                    return false;
                case WorldState.WindRestored:
                    RandomizerManager.Connection.CheckLocation("ForlornEscape");
                    return false;
                case WorldState.ForlornRuinsKey:
                    RandomizerManager.Connection.CheckLocation("GumonSeal");
                    return false;
                case WorldState.MountHoruKey:
                    RandomizerManager.Connection.CheckLocation("Sunstone");
                    return false;
                default:
                    return true;
            }
        }
    }

    [HarmonyPatch(typeof(AchievementsLogic), nameof(AchievementsLogic.OnAct3End))]
    internal class GameCompletePatch
    {
        private static bool Prefix()
        {
            RandomizerManager.Connection.SendCompletion();
            return true;
        }
    }
}

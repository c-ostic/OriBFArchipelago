using HarmonyLib;
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
                    ArchipelagoManager.CheckLocationString("GinsoEscapeExit");
                    return false;
                case WorldState.GinsoTreeKey:
                    ArchipelagoManager.CheckLocationString("WaterVein");
                    return false;
                case WorldState.WindRestored:
                    ArchipelagoManager.CheckLocationString("ForlornEscape");
                    return false;
                case WorldState.ForlornRuinsKey:
                    ArchipelagoManager.CheckLocationString("GumonSeal");
                    return false;
                case WorldState.MountHoruKey:
                    ArchipelagoManager.CheckLocationString("Sunstone");
                    return false;
                case WorldState.WarmthReturned:
                    ArchipelagoManager.CompleteGame();
                    return true;
                default:
                    return true;
            }
        }
    }
}

using HarmonyLib;
using OriBFArchipelago.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OriBFArchipelago.Patches
{
    [HarmonyPatch(typeof(RuntimeSceneMetaData), nameof(RuntimeSceneMetaData.IsInsideScenePaddingBounds), new Type[] { typeof(Rect) })]
    internal class RuntimeSceneMetaDataPatches
    {
        private static bool Prefix(RuntimeSceneMetaData __instance, ref bool __result)
        {
            // We would like to keep these scenes loaded for clean up.
            if (__instance.Scene == "ginsoTreeResurrection" || __instance.Scene == "ginsoTreeWaterRisingEnd")
            {
                int ginsoEventsValue = WorldEventsHelper.GinsoWorldEvents?.Value ?? 23;
                if ((ginsoEventsValue == 25 || ginsoEventsValue == 21) && !LocalGameState.IsGinsoExit && !(RandomizerManager.Receiver?.HasItem(InventoryItem.GinsoEscapeComplete) ?? true))
                {
                    __result = true;
                    return false;
                }
            }
            return true;
        }
    }
}

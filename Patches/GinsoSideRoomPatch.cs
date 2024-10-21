using HarmonyLib;
using OriBFArchipelago.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OriBFArchipelago.Patches
{
    // Copied from https://github.com/ori-community/bf-rando/blob/12b424b24b141034f192fe2f6d7f7e6e2c2b0987/Randomiser/World%20Changes/PatchGetWorldEventCondition.cs
    [HarmonyPatch(typeof(GetWorldEventCondition), nameof(GetWorldEventCondition.Validate))]
    internal class GinsoSideRoomPatch
    {
        private static bool Prefix(GetWorldEventCondition __instance, ref bool __result)
        {
            // I'll be honest I don't know how but this fixes a bug at the side rooms next to the ginso core
            //  where the areas don't load if you finish the escape, come back and don't have clean water
            if (__instance.WorldEvents.UniqueID == 26 && RandomizerManager.Connection.IsGinsoEscapeComplete())
            {
                __result = __instance.State != 21;
                return false;
            }

            return true;
        }
    }
}

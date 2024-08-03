using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OriBFArchipelago.Patches
{
    [HarmonyPatch(typeof(TransparentWallB), nameof(TransparentWallB.HasSense), MethodType.Getter)]
    internal class EnableSenseAlways
    {
        private static bool Prefix(ref bool __result)
        {
            __result = true;
            return false;
        }
    }

    [HarmonyPatch(typeof(RuntimeGameWorldArea), "HasSenseAbility", MethodType.Getter)]
    internal class RuntimeGameWorldAreaHasSenseAbility
    {
        private static bool Prefix(ref bool __result)
        {
            __result = true;
            return false;
        }
    }
}

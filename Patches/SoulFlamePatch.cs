using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OriBFArchipelago.Patches
{
    /**
     * Allows the player to access the skill tree before any experience is gained
     */
    [HarmonyPatch(typeof(SeinSoulFlame), nameof(SeinSoulFlame.AllowedToAccessSkillTree), MethodType.Getter)]
    internal class SoulFlamePatch
    {
        static void Postfix(SeinSoulFlame __instance, ref bool __result)
        {
            __result = __instance.IsSafeToCastSoulFlame == SeinSoulFlame.SoulFlamePlacementSafety.Safe;
        }
    }
}

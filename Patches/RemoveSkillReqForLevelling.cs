using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OriBFArchipelago.Patches
{
    [HarmonyPatch(typeof(SkillItem), nameof(SkillItem.Awake))]
    internal static class RemoveSkillReqForLevelling
    {
        private static void Postfix(SkillItem __instance)
        {
            __instance.RequiredAbilities.Clear();
        }
    }
}

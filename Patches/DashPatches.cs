using Core;
using HarmonyLib;
using OriBFArchipelago.Core;
using System.Reflection;
using UnityEngine;
using Input = Core.Input;

namespace OriBFArchipelago.Patches
{
    [HarmonyPatch(typeof(SeinDashAttack), nameof(SeinDashAttack.PerformDash), [])]
    internal class DashAttackPerformDashPatch
    {
        private static bool Prefix(SeinDashAttack __instance, ref bool ___RainbowDashActivated)
        {
            ___RainbowDashActivated = RandomizerSettings.RainbowDashTrail;
                
            return true;
        }
    }

    [HarmonyPatch(typeof(SeinDashAttack), nameof(SeinDashAttack.PerformWallDash))]
    internal class DashAttackPerformWallDashPatch
    {
        private static bool Prefix(SeinDashAttack __instance, ref bool ___RainbowDashActivated)
        {
            ___RainbowDashActivated = RandomizerSettings.RainbowDashTrail;

            return true;
        }
    }

    [HarmonyPatch(typeof(SeinDashAttack), nameof(SeinDashAttack.PerformChargeDash))]
    internal class DashAttackPerformChargeDashPatch
    {
        private static bool Prefix(SeinDashAttack __instance, ref bool ___RainbowDashActivated)
        {
            ___RainbowDashActivated = RandomizerSettings.RainbowDashTrail;

            return true;
        }
    }
}

using HarmonyLib;
using OriBFArchipelago.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OriBFArchipelago.Patches
{
    [HarmonyPatch(typeof(SeinDamageReciever), nameof(SeinDamageReciever.OnKillFadeInComplete))]
    internal class DeathPatch
    {
        private static void Postfix(SeinDamageReciever __instance)
        {
            RandomizerManager.Receiver.OnDeath();
        }
    }

    [HarmonyPatch(typeof(SaveGameController), nameof(SaveGameController.PerformSave))]
    internal class SavePatch
    {
        private static void Postfix(SaveGameController __instance)
        {
            RandomizerManager.Receiver.OnSave();
        }
    }
}

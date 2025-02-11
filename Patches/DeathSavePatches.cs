using HarmonyLib;
using OriBFArchipelago.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OriBFArchipelago.Patches
{
    [HarmonyPatch(typeof(SeinDamageReciever), nameof(SeinDamageReciever.OnKill))]
    internal class DeathPatch
    {
        private static void Postfix(Damage damage)
        {
            RandomizerManager.Receiver.OnDeath();

            // assume damage that is over 100 is meant to be an insta kill
            RandomizerManager.Connection.OnDeath(damage.Amount > 100);
        }
    }

    [HarmonyPatch(typeof(SaveGameController), nameof(SaveGameController.PerformSave))]
    internal class SavePatch
    {
        private static void Postfix(SaveGameController __instance)
        {
            RandomizerManager.Receiver.OnSave();
            RandomizerSettings.Save();
        }
    }
}

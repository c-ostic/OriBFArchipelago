using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OriBFArchipelago.Patches
{
    [HarmonyPatch(typeof(EntityDamageReciever), nameof(EntityDamageReciever.OnRecieveDamage))]
    internal class PetrifiedPlantPatch
    {
        private static void Postfix(EntityDamageReciever __instance)
        {
            if (__instance.NoHealthLeft && __instance.Entity is PetrifiedPlant)
            {
                ArchipelagoManager.CheckLocationByGameObject(__instance.Entity.gameObject);
            }
        }
    }
}

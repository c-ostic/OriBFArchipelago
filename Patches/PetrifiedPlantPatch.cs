using HarmonyLib;
using OriBFArchipelago.Core;
using OriBFArchipelago.MapTracker.Core;

namespace OriBFArchipelago.Patches
{
    [HarmonyPatch(typeof(EntityDamageReciever), nameof(EntityDamageReciever.OnRecieveDamage))]
    internal class PetrifiedPlantPatch
    {
        private static void Postfix(EntityDamageReciever __instance)
        {
            if (__instance.NoHealthLeft && __instance.Entity is PetrifiedPlant)
            {
                RandomizerManager.Connection.CheckLocation(__instance.Entity.MoonGuid);               
            }
        }
    }
}

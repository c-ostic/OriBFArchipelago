using HarmonyLib;
using OriBFArchipelago.Core;

namespace OriBFArchipelago.Patches
{
    internal class TitleScreenManagerPatches
    {
        [HarmonyPatch(typeof(TitleScreenManager), nameof(EntityDamageReciever.Awake))]
        internal class TitleScreenManagerPatch
        {
            private static void Postfix(EntityDamageReciever __instance)
            {
                RandomizerMessager.instance.AddMessage($"Tip: To use the ingame tracker, go to 'Help & Options' > 'Tracker Options' and turn on map visibility and icon visibility.");
            }
        }
    }
}

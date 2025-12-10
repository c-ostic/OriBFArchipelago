using HarmonyLib;
using OriBFArchipelago.ArchipelagoUI;
using OriBFArchipelago.Core;
using OriBFArchipelago.Helper;

namespace OriBFArchipelago.Patches
{
    internal class TitleScreenManagerPatches
    {
        [HarmonyPatch(typeof(TitleScreenManager), nameof(EntityDamageReciever.Awake))]
        internal class TitleScreenManagerPatch
        {
            private static void Postfix(EntityDamageReciever __instance)
            {
                if (!RandomizerSettings.SeenInfoMessage)
                    RandomizerInformationManager.ShowWelcomeMessage();
            }           
        }
    }
}

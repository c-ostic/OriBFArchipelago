using HarmonyLib;
using OriBFArchipelago.Core;

namespace OriBFArchipelago.Patches
{
    [HarmonyPatch(typeof(MenuScreenManager), nameof(MenuScreenManager.ShowMenuScreen), 
        [typeof(MenuScreenManager.Screens), typeof(bool)])]
    internal class ShowMenuScreenPatch
    {
        static bool Prefix()
        {
            RandomizerSettings.ShowSettings = true;
            return true;
        }
    }

    [HarmonyPatch(typeof(MenuScreenManager), nameof(MenuScreenManager.HideMenuScreen))]
    internal class HideMenuScreenPatch
    {
        static bool Prefix()
        {
            RandomizerSettings.ShowSettings = false;
            return true;
        }
    }
}

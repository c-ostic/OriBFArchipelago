using Game;
using HarmonyLib;
using OriBFArchipelago.Core;
using OriBFArchipelago.MapTracker.Core;

namespace OriBFArchipelago.Patches
{
    [HarmonyPatch(typeof(MenuScreenManager), nameof(MenuScreenManager.ShowMenuScreen), [typeof(MenuScreenManager.Screens), typeof(bool)])]
    internal class ShowMenuScreenPatch
    {
        static bool Prefix()
        {
            RandomizerSettings.ShowSettings = true;
            RuntimeGameWorldAreaPatch.ToggleDiscoveredAreas(MaptrackerSettings.MapVisibility);
            MaptrackerSettings.ResetCheckCount();
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

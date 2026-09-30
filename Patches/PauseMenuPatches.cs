using HarmonyLib;
using OriBFArchipelago.ArchipelagoUI;
using OriBFArchipelago.Core;
using OriBFArchipelago.MapTracker.Core;
using OriBFArchipelago.MapTracker.Logic;

namespace OriBFArchipelago.Patches
{
    [HarmonyPatch(typeof(MenuScreenManager), nameof(MenuScreenManager.ShowMenuScreen), [typeof(MenuScreenManager.Screens), typeof(bool)])]
    internal class ShowMenuScreenPatch
    {
        static bool Prefix(MenuScreenManager.Screens screen)
        {
            try
            {
                RandomizerSettings.ShowSettings = true;
                RuntimeGameWorldAreaPatch.ToggleDiscoveredAreas(MaptrackerSettings.MapVisibility);
                IconHoverUI.InvalidateCache();

                // Recompute the reachable/remaining check totals up front so the
                // "X out of Y are reachable" readout is correct as soon as the map opens.
                if (screen == MenuScreenManager.Screens.WorldMap)
                    LogicManager.RecalculateCheckCounts();
            }
            catch
            {

            }
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

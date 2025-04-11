using HarmonyLib;
using OriBFArchipelago.Core;
using OriBFArchipelago.MapTracker.Core;
using OriBFArchipelago.MapTracker.Logic;
using System;
using System.Collections.Generic;

namespace OriBFArchipelago.Patches
{
    [HarmonyPatch(typeof(MenuScreenManager), nameof(MenuScreenManager.ShowMenuScreen), [typeof(MenuScreenManager.Screens), typeof(bool)])]
    internal class ShowMenuScreenPatch
    {
        static bool Prefix()
        {
            RandomizerSettings.ShowSettings = true;
            RuntimeGameWorldAreaPatch.ToggleDiscoveredAreas(MaptrackerSettings.MapVisibility);
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

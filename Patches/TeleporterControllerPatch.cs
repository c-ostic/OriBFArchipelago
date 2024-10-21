using Game;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OriBFArchipelago.Patches
{
    // Copied from https://github.com/ori-community/bf-rando/blob/12b424b24b141034f192fe2f6d7f7e6e2c2b0987/Randomiser/TeleporterControllerPatches.cs
    [HarmonyPatch(typeof(TeleporterController))]
    internal static class TeleporterControllerPatches
    {
        [HarmonyPostfix, HarmonyPatch(nameof(TeleporterController.OnFadedToBlack))]
        private static void OnFadedToBlackPostfix()
        {
            // Reset misty woods
            var mistySim = new WorldEvents();
            mistySim.MoonGuid = new MoonGuid(1061758509, 1206015992, 824243626, -2026069462);
            int value = World.Events.Find(mistySim).Value;
            if (value != 1 && value != 8)
            {
                World.Events.Find(mistySim).Value = 10;
            }
        }
    }
}

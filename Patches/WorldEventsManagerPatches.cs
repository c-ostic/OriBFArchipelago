using HarmonyLib;
using OriBFArchipelago.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OriBFArchipelago.Patches
{

    [HarmonyPatch(typeof(WorldEventsManager), nameof(WorldEventsManager.Find))]
    internal class WorldEventsManagerPatches
    {
        private static bool Prefix(ref Dictionary<MoonGuid, WorldEventsRuntime> ___m_worldEvents)
        {
            if (WorldEventsHelper.GameEventsRuntime == null)
            {
                WorldEventsHelper.GameEventsRuntime = ___m_worldEvents;
            }
            return true;
        }
    }
}

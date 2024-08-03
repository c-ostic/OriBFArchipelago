using Game;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OriBFArchipelago.Patches
{
    [HarmonyPatch(typeof(SeinDoorHandler), nameof(SeinDoorHandler.EnterIntoDoor))]
    internal class ForbidDoorPatch
    {
        // Returning false = cannot travel through door
        private static bool Prefix(Door door)
        {
            // Don't let anyone through to the element of warmth
            if (door.name == "mountHoruExitDoor")
            {
                // TODO: show ui hint to say door is blocked until all trees are activated
                return ArchipelagoManager.IsGoalComplete();
            }

            return true;
        }
    }
}

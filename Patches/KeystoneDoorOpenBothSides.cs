using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OriBFArchipelago.Patches
{
    [HarmonyPatch(typeof(DoorWithSlots), nameof(DoorWithSlots.SeinInRange), MethodType.Getter)]
    internal class KeystoneDoorOpenBothSides
    {
        private static bool Prefix(DoorWithSlots __instance, ref bool __result)
        {
            __result = !__instance.OriHasTargets && __instance.DistanceToSein <= __instance.Radius;
            return false;
        }
    }
}

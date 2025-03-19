using HarmonyLib;
using OriBFArchipelago.MapTracker.Core;
using UnityEngine;

namespace OriBFArchipelago.Patches

{
    [HarmonyPatch(typeof(AreaMapNavigation))]

    //This function removes the random wandering the map does when having the area map open.
    internal class AreaMapNavigationPatch
    {
        static AreaMapNavigationPatch()
        {
            ModLogger.Debug($"Patching {nameof(AreaMapNavigationPatch)}");
        }

        [HarmonyPatch("UpdatePlane")]
        [HarmonyPrefix]
        static bool Prefix(AreaMapNavigation __instance)
        {
            // Get the private MapPivot field using reflection
            var mapPivot = AccessTools.Field(typeof(AreaMapNavigation), "MapPivot").GetValue(__instance) as Transform;

            // Get the Zoom property
            float zoom = __instance.Zoom;

            // Update without the sway
            __instance.MapPlaneSize = Vector2.one * zoom;
            mapPivot.position = -__instance.ScrollPosition * zoom;

            // Skip the original method
            return false;
        }
    }
}
using HarmonyLib;
using OriBFArchipelago.MapTracker.Core;
using System;
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
            //Disables the sway in the map
            if (!MaptrackerSettings.DisableMapSway)
                return true;        
                try {
            var mapPivot = AccessTools.Field(typeof(AreaMapNavigation), "MapPivot").GetValue(__instance) as Transform;
            float zoom = __instance.Zoom;
            __instance.MapPlaneSize = Vector2.one * zoom;
            mapPivot.position = -__instance.ScrollPosition * zoom;

            }
            catch (Exception ex)
            {
                ModLogger.Error($"{ex}");
            }
            // Skip the original method
            return false;
        }
    }
}
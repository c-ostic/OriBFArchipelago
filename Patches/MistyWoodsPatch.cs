using HarmonyLib;
using OriBFArchipelago.MapTracker.Core;
using UnityEngine;

namespace OriBFArchipelago.Patches;

/// <summary>
/// Patchs the Misty Woods Area of the GameMapUI
/// We go through a routine that is active when we need to patch the map
/// </summary>
[HarmonyPatch(typeof(GameMapUI), nameof(GameMapUI.UpdateAreaText))]
internal class UpdateAreaTextPath
{
    /// <summary>
    /// Unlock the Misty Woods Area on the GameMapUI if the map is visible
    /// </summary>
    private static void Prefix()
    {
        Transform mapPivot = AreaMapUI.Instance.transform.Find("mapPivot");
        if (MaptrackerSettings.MapVisibility == MapVisibilityEnum.Visible)
        {
            mapPivot.FindChild("mistyWoodsFog").gameObject.SetActive(false);
            mapPivot.FindChild("mistyWoods").gameObject.SetActive(true);
        } 
        else if (MaptrackerSettings.MapVisibility == MapVisibilityEnum.Not_Visible)
        {
            mapPivot.FindChild("mistyWoodsFog").gameObject.SetActive(true);
            mapPivot.FindChild("mistyWoods").gameObject.SetActive(false);
        }
    }
}
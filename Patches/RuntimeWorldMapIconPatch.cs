using HarmonyLib;
using OriBFArchipelago.MapTracker.Core;
using OriBFArchipelago.MapTracker.Logic;
using OriBFArchipelago.MapTracker.UI;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace OriBFArchipelago.Patches
{
    [HarmonyPatch(typeof(RuntimeWorldMapIcon))]
    internal class RuntimeWorldMapIconPatch
    {

        // Static constructor will run when the class is first accessed
        static RuntimeWorldMapIconPatch()
        {
            ModLogger.Debug($"Patching {nameof(RuntimeWorldMapIconPatch)}");
        }


        [HarmonyPatch("IsVisible")]
        [HarmonyPrefix]
        internal static bool IsVisiblePrefix(RuntimeWorldMapIcon __instance, ref bool __result)
        {
            try
            {
                if (IsDuplicateIcon(__instance))
                    return false;

                switch (MaptrackerSettings.IconVisibility)
                {
                    case IconVisibilityEnum.All:
                        __result = true;
                        return false;
                    case IconVisibilityEnum.In_Logic:
                        __result = LogicManager.IsInLogic(__instance);
                        return false;
                    case IconVisibilityEnum.Original:
                        return true;
                    case IconVisibilityEnum.None:
                        __result = false;
                        return false;
                }
                return false;
            }
            catch (System.Exception ex)
            {
                ModLogger.Error($"Error at isvisible: {ex}");
                return true;
            }
        }
        private static bool IsDuplicateIcon(RuntimeWorldMapIcon icon)
        {
            List<MoonGuid> duplicateIcons = new List<MoonGuid>{
                 new MoonGuid("1607939702 1149860266 185564807 -1906561306"), //duplicate icon on bash
                 new MoonGuid("1725611206 1201986298 -435475044 -1944513031"), //duplicate icon on ability point in burrows
            };


            if (duplicateIcons.Contains(icon.Guid))
                return true;
            return false;
        }

        [HarmonyPatch("Show")]
        [HarmonyPostfix]
        internal static void Postfix(RuntimeWorldMapIcon __instance)
        {
            try
            {

                if (__instance.Icon == WorldMapIconType.SavePedestal)
                    return;

                if (__instance.Icon != WorldMapIconType.Invisible && __instance.IsVisible(AreaMapUI.Instance))
                {
                    // Get the private m_iconGameObject field using reflection
                    var gameObject = (GameObject)typeof(RuntimeWorldMapIcon).GetField("m_iconGameObject", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
                    if (gameObject != null)
                    {
                        IconHoverEffectUI existingHoverEffect = gameObject.GetComponent<IconHoverEffectUI>();

                        if (existingHoverEffect != null)
                            return;

                        var area = (RuntimeGameWorldArea)typeof(RuntimeWorldMapIcon).GetField("Area", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
                        IconHoverEffectUI hoverEffect = gameObject.AddComponent<IconHoverEffectUI>();
                        hoverEffect.IconType = __instance;
                        hoverEffect.MapUI = AreaMapUI.Instance;
                        hoverEffect.Area = area;
                    }
                }
            }
            catch (System.Exception ex)
            {
                ModLogger.Error($"{ex}");
            }
        }
    }
}
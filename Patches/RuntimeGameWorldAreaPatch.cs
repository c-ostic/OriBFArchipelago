﻿using HarmonyLib;
using OriBFArchipelago.Core;
using OriBFArchipelago.MapTracker.Core;
using OriModding.BF.UiLib.Map;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace OriBFArchipelago.Patches
{
    [HarmonyPatch(typeof(RuntimeGameWorldArea))]
    internal class RuntimeGameWorldAreaPatch
    {
        public static bool AddedCustomIcons = false;
        public static Condition StoredMistyWoodsCondition { get; set; }
        
        [HarmonyPatch("Initialize")]
        [HarmonyPostfix]
        internal static void Initialize_Postfix(RuntimeGameWorldArea __instance)
        {
            try
            {   //todo: Check if this works as intended
                if (AddedCustomIcons)
                    return;
                var locations = LocationLookup.GetLocations().Where(d => d.CustomIconType != CustomWorldMapIconType.None);
                foreach (var location in locations)
                {
                    
                    var customIcon = new CustomWorldMapIcon(location.CustomIconType, location.WorldPosition, location.MoonGuid);
                    //todo: Make this work with locationlookup

                    if (location.Name == "Sunstone")
                        customIcon.Type = CustomWorldMapIconType.Sunstone;
                    else if (location.Name == "GumonSeal")
                        customIcon.Type = CustomWorldMapIconType.WaterVein;
                    else if (location.Name == "GinsoEscapeExit")
                        customIcon.Type = CustomWorldMapIconType.CleanWater;


                    CustomWorldMapIconManager.Register(customIcon);
                }
                AddedCustomIcons = true;
            }
            catch (System.Exception ex)
            {
                ModLogger.Error($"Error adding custom icons: {ex}");
            }
        }

        public static List<string> DiscoveredAreas { get; set; }
        static RuntimeGameWorldAreaPatch()
        {
            DiscoveredAreas = new List<string>();
            ModLogger.Debug($"Patching {nameof(RuntimeGameWorldAreaPatch)}");
        }

        public static void ToggleDiscoveredAreas(MapVisibilityEnum mapVisibility)
        {
            if (GameWorld.Instance?.RuntimeAreas == null)
            {
                ModLogger.Debug($"{nameof(GameWorld.Instance.RuntimeAreas)} is empty");
                return;
            }
            else if (mapVisibility == MapVisibilityEnum.Visible && DiscoveredAreas.Count == 0)
            {
                ModLogger.Debug("Discovering all areas");
                var face = GameWorld.Instance.RuntimeAreas.FirstOrDefault().Area.CageStructureTool.Faces.OrderBy(d => d.ID).FirstOrDefault();
                var id = face.ID.ToString();
                GameWorld.Instance.RuntimeAreas.ForEach(area =>
                {
                    area.DiscoverAllAreas();
                    if (area.Area.AreaName.ToString() == "MISTY WOODS")
                    {
                        if (area.Area.VisitableCondition != null)
                        {
                            StoredMistyWoodsCondition = area.Area.VisitableCondition;
                        }
                        area.Area.VisitableCondition = null;
                        SetMistyWoodsVisibility(true);
                    }
                });
                DiscoveredAreas.Add(id);

            }
            else if (mapVisibility == MapVisibilityEnum.Not_Visible && DiscoveredAreas.Count > 0)
            {
                ModLogger.Debug("Undiscovering all areas");
                GameWorld.Instance.RuntimeAreas.ForEach(area =>
                {
                    area.UnDiscoverAllAreas();
                    if (area.Area.AreaName.ToString() == "MISTY WOODS")
                    {
                        area.Area.VisitableCondition = StoredMistyWoodsCondition;
                        SetMistyWoodsVisibility(false);
                    }
                });
                DiscoveredAreas.Clear();
            }
        }

        /**
         * Set the visibility of Misty Woods with `isMistyWoodsVisible`. Remove the Misty Woods fog
         * if `isMistyWoodsVisible` is true, otherwise the fog is active and Misty Woods is hidden.
         */
        private static void SetMistyWoodsVisibility(bool isMistyWoodsVisible)
        {
            Transform mapPivot = AreaMapUI.Instance.transform.Find("mapPivot");
            mapPivot.FindChild("mistyWoodsFog").gameObject.SetActive(!isMistyWoodsVisible);
            mapPivot.FindChild("mistyWoods").gameObject.SetActive(isMistyWoodsVisible);
        }
    }

    public static class RuntimeGameWorldAreaExtensions
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void UnDiscoverAllAreas(this RuntimeGameWorldArea instance)
        {
            if (instance == null)
                return;
            try
            {
                // Get the private field using reflection
                FieldInfo statesField = typeof(RuntimeGameWorldArea).GetField("m_worldAreaStates", BindingFlags.NonPublic | BindingFlags.Instance);

                var worldAreaStates = (Dictionary<int, WorldMapAreaState>)statesField.GetValue(instance);
                CageStructureTool cageStructureTool = instance.Area.CageStructureTool;

                // Create a list to store keys to remove
                List<int> keysToRemove = new List<int>();

                foreach (CageStructureTool.Face face in cageStructureTool.Faces)
                {
                    if (worldAreaStates.TryGetValue(face.ID, out var state) && state == WorldMapAreaState.Discovered)
                    {
                        keysToRemove.Add(face.ID);
                    }
                }

                // Remove the discovered (but not visited) areas
                foreach (int key in keysToRemove)
                {
                    worldAreaStates.Remove(key);
                }
            }
            catch (System.Exception ex)
            {
                ModLogger.Error($"{ex}");
            }
        }
    }
}

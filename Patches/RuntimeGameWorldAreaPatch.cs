using HarmonyLib;
using OriBFArchipelago.Core;
using OriBFArchipelago.MapTracker.Core;
using OriBFArchipelago.MapTracker.Logic;
using OriBFArchipelago.MapTracker.Model;
using OriModding.BF.UiLib.Map;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace OriBFArchipelago.Patches
{
    [HarmonyPatch(typeof(RuntimeGameWorldArea))]
    internal class RuntimeGameWorldAreaPatch
    {

        

        [HarmonyPatch("Initialize")]
        [HarmonyPostfix]
        internal static void Initialize_Postfix(RuntimeGameWorldArea __instance)
        {

            try
            {//todo: Check if this works as intended
                var locations = LocationLookup.GetLocations().Where(d => d.CustomIconType != CustomWorldMapIconType.None);
                foreach (var location in locations)
                {
                    ModLogger.Debug($"Adding icon for {location.Name} - {location.CustomIconType}");
                    var customIcon = new CustomWorldMapIcon(location.CustomIconType, location.WorldPosition, location.MoonGuid);
                    CustomWorldMapIconManager.Register(customIcon);
                }
            }
            catch (System.Exception ex)
            {
                ModLogger.Error($"Error adding custom icons: {ex}");
            }
        }

        private static List<string> DiscoveredAreas { get; set; }
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
                GameWorld.Instance.RuntimeAreas.ForEach(c => c.DiscoverAllAreas());
                DiscoveredAreas.Add(id);

            }
            else if (mapVisibility == MapVisibilityEnum.Not_Visible && DiscoveredAreas.Count > 0)
            {
                ModLogger.Debug("Undiscovering all areas");
                GameWorld.Instance.RuntimeAreas.ForEach(c => c.UnDiscoverAllAreas());
                DiscoveredAreas.Clear();
            }
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

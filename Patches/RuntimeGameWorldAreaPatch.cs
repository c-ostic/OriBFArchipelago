using HarmonyLib;
using OriBFArchipelago.MapTracker.Core;
using OriBFArchipelago.MapTracker.Logic;
using OriBFArchipelago.MapTracker.Model;
using OriModding.BF.UiLib.Map;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace OriBFArchipelago.Patches
{
    /*


               1938935733 1113883664 315507611 -1453775037
               -1062105612 1120930976 -179497838 -662336101
               -1219150589 1211475862 412676004 474288113
               996812703 1304625676 850149517 -1070265226
               -1687547388 1177774465 1131249040 -1920973108
               1794137324 1290555749 -474798429 873483678
               -1718288593 1233280399 -713278054 -1447006297
               759945948 1198006022 -1894807636 860641409
               1302457559 1133323409 154604163 -888175303
               -1069953246 1269463446 -819894877 -2009365983
               524981524 1177548378 -1596446582 231191171
               -1715107533 1140498873 -1374404172 1244026798
               1310836902 1169764107 951946675 -922855353
               -153230875 1205417878 193577345 1692889776
               -2131077341 1279229552 515428512 1593442546
               578554584 1225422486 -1564184182 1449825939
               822610300 1079173238 -1748833393 2100112619
               250304250 1104287048 1213970857 -1324568316
               -659755354 1242124298 1268032145 -203084651
               478957642 1086332882 -2081113681 -1889550710
               -1986105471 1185691069 -47194738 -1250051667
               1255433986 1308678331 453657770 507189175
               1971286891 1340902128 -1136687483 786625687
       */
    [HarmonyPatch(typeof(RuntimeGameWorldArea))]
    internal class RuntimeGameWorldAreaPatch
    {
        private static List<ExtendedCustomWorldMapIcon> CustomXpIcons = new List<ExtendedCustomWorldMapIcon>
        {
            new ExtendedCustomWorldMapIcon(WorldMapIconType.Experience, new MoonGuid("1357098119 1185246384 -60723813 -1846269103"), new Vector3(43.9f, -156.1f, 0), "Hollow Grove Flower #1", "Hollow Grove"),
            new ExtendedCustomWorldMapIcon(WorldMapIconType.Experience, new MoonGuid("1515223554 1193340384 -1596868467 697952739"), new Vector3(330.5f, -77.0f, 0), "Hollow Grove Flower #1", "Hollow Grove"),
            new ExtendedCustomWorldMapIcon(WorldMapIconType.Experience, new MoonGuid("560810798 1177388095 1561676448 -1886145880"), new Vector3(628.4f, -119.5f,0), "Hollow Grove Flower #1", "Hollow Grove"),
            new ExtendedCustomWorldMapIcon(WorldMapIconType.Experience, new MoonGuid("-227868995 1177742190 -644734542 1909369139"), new Vector3(447.7f, -367.7f, 0), "Hollow Grove Flower #1", "Hollow Grove"),
            new ExtendedCustomWorldMapIcon(WorldMapIconType.Experience, new MoonGuid("509607018 1143884117 -863808114 -1366570643"), new Vector3(439.6f, -344.9f, 0), "Hollow Grove Flower #1", "Hollow Grove"),
            new ExtendedCustomWorldMapIcon(WorldMapIconType.Experience, new MoonGuid("-1427834574 1228643039 258065063 192959857"), new Vector3(493.0f, -400.8f, 0), "Hollow Grove Flower #1", "Hollow Grove"),
            new ExtendedCustomWorldMapIcon(WorldMapIconType.Experience, new MoonGuid("-782504693 1227994259 -1940970872 -746412143"), new Vector3(540.7f, 101.1f, 0), "Hollow Grove Flower #1", "Hollow Grove")
        };

        [HarmonyPatch("Initialize")]
        [HarmonyPostfix]
        internal static void Initialize_Postfix(RuntimeGameWorldArea __instance)
        {

            try
            {
                // Add custom icons to the area
                foreach (var iconInfo in CustomXpIcons.Where(c => c.AreaName.Equals(__instance.Area.AreaName.ToString(), System.StringComparison.CurrentCultureIgnoreCase)))
                {
                    ModLogger.Debug($"Adding icon {iconInfo.Name} for area {__instance.Area.AreaName}");
                    CustomXpIcons.ForEach(x => CustomWorldMapIconManager.Register(iconInfo.Icon));
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
                FieldInfo statesField = typeof(RuntimeGameWorldArea).GetField("m_worldAreaStates",
                    BindingFlags.NonPublic | BindingFlags.Instance);

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

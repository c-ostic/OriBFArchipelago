using BepInEx;
using BepInEx.Configuration;
using OriBFArchipelago.ArchipelagoUI;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OriBFArchipelago.MapTracker.Core
{
    internal class MaptrackerSettings
    {
        private static string OldSaveSlotFilePath { get { return Paths.ConfigPath + $"/MapTrackerSlot{SaveSlotsUI.Instance.CurrentSaveSlot.SaveSlotIndex}.cfg"; } }
        private static Dictionary<MoonGuid, bool> Checks { get; set; }



        public static bool EnableIconInfocUI { get { return MapTrackerOptionsScreen.EnableIconInfocUI; } }
        public static MapVisibilityEnum MapVisibility { get { return MapTrackerOptionsScreen.MapVisibility; } }
        public static IconVisibilityEnum IconVisibility { get { return MapTrackerOptionsScreen.IconVisibility; } }
        public static IconVisibilityLogicEnum IconVisibilityLogic { get { return MapTrackerOptionsScreen.IconVisibilityLogic; } }
        public static bool DisableMapSway { get { return MapTrackerOptionsScreen.DisableMapSway; } }


        public static int ChecksInLogic { get { return Checks.Select(d => d.Value).Count(d => d); } }
        public static int ChecksLeft { get { return Checks.Count; } }
        public static bool AllAreasDiscovered { get; set; }
       
        internal static void AddCheck(MoonGuid guid, bool isInLogic = false)
        {
            if (!Checks.ContainsKey(guid))
                Checks.Add(guid, isInLogic);
            else if (!Checks[guid] && isInLogic)
                Checks[guid] = isInLogic;
        }
        internal static void ResetCheckCount()
        {
            Checks = [];
        }
        internal static void Delete()
        {
            if (File.Exists(OldSaveSlotFilePath))
                File.Delete(OldSaveSlotFilePath);
        }
    }
}


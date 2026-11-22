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

        public static bool HideNonCollectableIcons { get { return MapTrackerOptionsScreen.HideNonCollectableIcons; } }
        public static bool EnableIconInfocUI { get { return MapTrackerOptionsScreen.EnableIconInfocUI; } }
        public static MapVisibilityEnum MapVisibility { get { return MapTrackerOptionsScreen.MapVisibility; } }
        public static IconVisibilityEnum IconVisibility { get { return MapTrackerOptionsScreen.IconVisibility; } }
        public static IconVisibilityLogicEnum IconVisibilityLogic { get { return MapTrackerOptionsScreen.IconVisibilityLogic; } }
        public static bool DisableMapSway { get { return MapTrackerOptionsScreen.DisableMapSway; } }


        // Computed in one pass when the world map opens (see LogicManager.RecalculateCheckCounts).
        public static int ChecksInLogic { get; private set; }
        public static int ChecksLeft { get; private set; }
        public static bool AllAreasDiscovered { get; set; }

        /// <summary>Stores the reachable / remaining check totals for the world-map readout.</summary>
        internal static void SetCheckCounts(int inLogic, int left)
        {
            ChecksInLogic = inLogic;
            ChecksLeft = left;
        }
        internal static void Delete()
        {
            if (File.Exists(OldSaveSlotFilePath))
                File.Delete(OldSaveSlotFilePath);
        }
    }
}


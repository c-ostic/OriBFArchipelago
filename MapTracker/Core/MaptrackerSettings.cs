﻿using BepInEx;
using BepInEx.Configuration;
using OriBFArchipelago.MapTracker.UI;
using System.Collections.Generic;
using System.IO;

namespace OriBFArchipelago.MapTracker.Core
{
    internal class MaptrackerSettings
    {
        private static string OldSaveSlotFilePath { get { return Paths.ConfigPath + $"/MapTrackerSlot{SaveSlotsUI.Instance.CurrentSaveSlot.SaveSlotIndex}.cfg"; } }
        public static bool EnableIconInfocUI { get { return MapTrackerOptionsScreen.GetEnableIconInfocUI(); } }
        public static MapVisibilityEnum MapVisibility { get { return MapTrackerOptionsScreen.GetMapVisibility(); } }
        public static IconVisibilityEnum IconVisibility { get { return MapTrackerOptionsScreen.GetIconVisibility(); } }
        public static bool DisableMapSway { get { return MapTrackerOptionsScreen.GetDisableMapSway(); } }

        internal static void Delete()
        { //Keep this to cleanup old config files
            if (File.Exists(OldSaveSlotFilePath))
                File.Delete(OldSaveSlotFilePath);
        }
    }
}


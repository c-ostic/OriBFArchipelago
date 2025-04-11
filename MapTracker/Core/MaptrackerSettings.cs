﻿using BepInEx;
using BepInEx.Configuration;
using OriBFArchipelago.MapTracker.Logic;
using OriBFArchipelago.MapTracker.UI;
using OriBFArchipelago.Patches;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OriBFArchipelago.MapTracker.Core
{
    internal class MaptrackerSettings
    {

        private const string CONFIGSECTION = "MapTracker";
        private static string SaveSlotFilePath { get { return Paths.ConfigPath + $"/MapTracker/Slot{SaveSlotsUI.Instance.CurrentSaveSlot.SaveSlotIndex}.cfg"; } }

        private static ConfigFile _config;
        private static ConfigEntry<string> _collectedCustomItems { get; set; }
        public static List<MoonGuid> CollectedCustomIcons { get; set; }


        public static bool EnableIconInfocUI { get { return MapTrackerOptionsScreen.GetEnableIconInfocUI(); } }
        public static MapVisibilityEnum MapVisibility { get { return MapTrackerOptionsScreen.GetMapVisibility(); } }
        public static IconVisibilityEnum IconVisibility { get { return MapTrackerOptionsScreen.GetIconVisibility(); } }
        public static bool DisableMapSway { get { return MapTrackerOptionsScreen.GetDisableMapSway(); } }
        public static void LoadSettings()
        {
            try
            {
                Reset();

                _config = new ConfigFile(SaveSlotFilePath, true);
                InitializeSettings();
                ModLogger.Debug($"Loading settings: {_config.ConfigFilePath}");
            }
            catch (Exception ex)
            {
                ModLogger.Error($"Found error: {ex}");
            }
        }

        private static void InitializeSettings()
        {
            _collectedCustomItems = _config.Bind(CONFIGSECTION, "CustomItems", string.Empty, "Holds all custom icons that have been found by their moonguid");
            CollectedCustomIcons = new List<MoonGuid>();
            if (!string.IsNullOrEmpty(_collectedCustomItems.Value))
            {
                CollectedCustomIcons = _collectedCustomItems.Value.Split(',').Select(c => new MoonGuid(c)).ToList();
            }
        }

        public static void Save()
        {
            _collectedCustomItems.SetSerializedValue(string.Join(",", CollectedCustomIcons.Select(c => c.ToString()).ToArray()));
        }
        public static void AddCustomIconCheck(MoonGuid moonGuid)
        {
            if (!CollectedCustomIcons.Contains(moonGuid))
            {
                var collectedCustomIcons = new List<MoonGuid>(CollectedCustomIcons);
                collectedCustomIcons.Add(moonGuid);
                CollectedCustomIcons = collectedCustomIcons;                
            }
        }

        internal static void Delete()
        {
            if (File.Exists(SaveSlotFilePath))
                File.Delete(SaveSlotFilePath);
        }

        internal static void Reset()
        {
            //todo: This is a "quick fix". All settings should be in a seperate class and handled there
            ModLogger.Info("Resetting maptracker settings");
            RuntimeGameWorldAreaPatch.DiscoveredAreas = new List<string>();
            RuntimeGameWorldAreaPatch.AddedCustomIcons = false;
            CollectedCustomIcons = new List<MoonGuid>();
        }
    }
}


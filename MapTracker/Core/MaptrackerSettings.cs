using BepInEx;
using BepInEx.Configuration;
using OriBFArchipelago.MapTracker.UI;
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


        public static bool EnableLogicUI { get { return ModOptionsScreen.GetEnableLogicUI(); } }
        public static MapVisibilityEnum MapVisibility { get { return ModOptionsScreen.GetMapVisibility(); } }
        public static IconVisibilityEnum IconVisibility { get { return ModOptionsScreen.GetIconVisibility(); } }
        public static void LoadSettings()
        {
            try
            {
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
        public static void AddPetrifiedPlant(MoonGuid moonGuid)
        {
            if (!CollectedCustomIcons.Contains(moonGuid))
            {
                var plants = new List<MoonGuid>(CollectedCustomIcons);
                plants.Add(moonGuid);
                CollectedCustomIcons = plants;
                ModLogger.Debug($"Added petrified plant: {moonGuid}");
            }
        }

        internal static void Delete()
        {
            if (File.Exists(SaveSlotFilePath))
                File.Delete(SaveSlotFilePath);
        }
    }
}


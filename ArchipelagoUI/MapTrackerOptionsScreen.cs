using BepInEx;
using BepInEx.Configuration;
using OriBFArchipelago.Core;
using OriBFArchipelago.MapTracker.Core;
using System;
using System.IO;

namespace OriBFArchipelago.ArchipelagoUI
{
    internal class MapTrackerOptionsScreen : BaseModOptionsScreen
    {
        private const string CONFIGSECTION = "MapTracker";
        private static ConfigFile _config;

        private static ConfigEntry<string> _mapVisibility { get; set; }
        private static ConfigEntry<string> _iconVisibility { get; set; }
        private static ConfigEntry<string> _iconVisibilityLogic { get; set; }
        private static ConfigEntry<bool> _enableIconInfocUI { get; set; }
        private static ConfigEntry<bool> _disableMapSway { get; set; }

        private static string ConfigSavePath { get { return RandomizerIO.GetFilePath("MapTracker.cfg"); } }
        private static string OldConfigSavePath { get { return Paths.ConfigPath + $"/OriBFMapTracker/Tracker.cfg"; } }
        public MapTrackerOptionsScreen()
        {
            ModLogger.Debug("Loaded MaptrackerSettingsScreen");
        }

        public override void InitScreen()
        {
            try
            {
                if (File.Exists(OldConfigSavePath)) //Keep this to accomodate older versions. Removing in 2to4 patches
                    CopyOldConfigToNew();

                _config = new ConfigFile(ConfigSavePath, true);

                ModLogger.Debug($"Loading settings: {_config.ConfigFilePath}");
                InitializeSettings(_config);
                SetComponents();
            }
            catch (Exception ex)
            {
                ModLogger.Error($"Error in InitScreen: {ex.ToString()}");
            }
        }

        private void CopyOldConfigToNew()
        {
            if (File.Exists(OldConfigSavePath))
            {
                File.WriteAllText(ConfigSavePath, File.ReadAllText(OldConfigSavePath));
                File.Delete(OldConfigSavePath);
            }
        }

        private void InitializeSettings(ConfigFile config)
        {
            ModLogger.Debug("Initializing settings");
            _mapVisibility = config.Bind(CONFIGSECTION, "MapVisibility", $"{MapVisibilityEnum.Visible}", "Sets map visibility");
            _iconVisibility = config.Bind(CONFIGSECTION, "IconVisibility", $"{IconVisibilityEnum.In_Logic}", "Sets icon visibility");
            _iconVisibilityLogic = config.Bind(CONFIGSECTION, "IconVisiblityLogic", $"{IconVisibilityLogicEnum.Game}", "Sets icon visibility logic");
            _enableIconInfocUI = config.Bind(CONFIGSECTION, "EnableItemUI", false, "Sets enableitemui");
            _disableMapSway = config.Bind(CONFIGSECTION, "DisableMapSway", false, "Sets disablemapsway");
            ModLogger.Debug("Settings initialized successfully");
        }

        private void SetComponents()
        {
            ModLogger.Debug("Setting up UI components");
            AddMultiToggle(_mapVisibility, "Map visiblity", "Not Visibile: Normal game logic for showing maps\nVisible: Shows all maps always", EnumParser.GetEnumNames<MapVisibilityEnum>());
            AddMultiToggle(_iconVisibility, "Icon visibility", "None: Never show icons\nOriginal: Show icons based on normal game logic.\nIn Logic: Shows icons in logic and icon logic (Setting below)\nAll: Show all uncollected icons in game", EnumParser.GetEnumNames<IconVisibilityEnum>());
            AddMultiToggle(_iconVisibilityLogic, "Icon logic", "Game: Shows items collected in game, dying without saving will reshow the icon.\nArchipelago: Shows icons based on archipelago. Dying will keep icons hidden except for goal required items.", EnumParser.GetEnumNames<IconVisibilityLogicEnum>());
            AddToggle(_enableIconInfocUI, "Icon info", "Enables a small window on the top right that information about the item location. This can be triggered with the mouse, or the dot that appears in the middle of the map when using controller");
            AddToggle(_disableMapSway, "Disable map sway", "Disables the swap in the map. Usefull when enabling Item UI for better pointing at icons.");
            ModLogger.Debug("UI components set up successfully");
        }

        internal static MapVisibilityEnum MapVisibility => EnumParser.GetEnumValue<MapVisibilityEnum>(_mapVisibility.Value);
        internal static IconVisibilityEnum IconVisibility => EnumParser.GetEnumValue<IconVisibilityEnum>(_iconVisibility.Value);
        internal static IconVisibilityLogicEnum IconVisibilityLogic => EnumParser.GetEnumValue<IconVisibilityLogicEnum>(_iconVisibilityLogic.Value);
        internal static bool EnableIconInfocUI => _enableIconInfocUI?.Value ?? false;
        internal static bool DisableMapSway => _disableMapSway?.Value ?? false;
    }
}


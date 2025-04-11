using BepInEx;
using BepInEx.Configuration;
using OriBFArchipelago.MapTracker.Core;

namespace OriBFArchipelago.MapTracker.UI
{
    internal class MapTrackerOptionsScreen : BaseModOptionsScreen
    {
        private const string CONFIGSECTION = "MapTracker";


        private static ConfigFile _config;

        private static ConfigEntry<string> MapVisibility { get; set; }
        private static ConfigEntry<string> IconVisibility { get; set; }
        private static ConfigEntry<bool> EnableIconInfocUI { get; set; }
        private static ConfigEntry<bool> DisableMapSway { get; set; }


        public MapTrackerOptionsScreen()
        {
            ModLogger.Debug("Loaded MaptrackerSettingsScreen");
        }

        // Override the InitScreen method to add your custom settings
        public override void InitScreen()
        {
            try
            {
                _config = new ConfigFile(Paths.ConfigPath + $"/OriBFMapTracker/Tracker.cfg", true);
                ModLogger.Debug($"Loading settings: {_config.ConfigFilePath}");
                InitializeSettings(_config);
                SetComponents();
            }
            catch (System.Exception ex)
            {
                ModLogger.Error($"Error in InitScreen: {ex.ToString()}");
            }
        }

        private void InitializeSettings(ConfigFile config)
        {
            ModLogger.Debug("Initializing settings");
            MapVisibility = config.Bind(CONFIGSECTION, "MapVisibility", $"{MapVisibilityEnum.Visible}", "Sets map visibility");
            IconVisibility = config.Bind(CONFIGSECTION, "IconVisibility", $"{IconVisibilityEnum.In_Logic}", "Sets icon visibility");
            EnableIconInfocUI = config.Bind(CONFIGSECTION, "EnableItemUI", false, "Sets enableitemui");
            DisableMapSway = config.Bind(CONFIGSECTION, "DisableMapSway", false, "Sets disablemapsway");
            ModLogger.Debug("Settings initialized successfully");
        }

        private void SetComponents()
        {
            ModLogger.Debug("Setting up UI components");
            AddButton("Teleport to start", "Teleports Ori to the starting area.", TeleporterManager.TeleportToStart);
            AddMultiToggle(setting: MapVisibility, label: "Map visiblity", tooltip: "Options: " + string.Join(", ", EnumParser.GetEnumNames(typeof(MapVisibilityEnum))), options: EnumParser.GetEnumNames(typeof(MapVisibilityEnum)));
            AddMultiToggle(setting: IconVisibility, label: "Icon visibility", tooltip: "Options: " + string.Join(", ", EnumParser.GetEnumNames(typeof(IconVisibilityEnum))), options: EnumParser.GetEnumNames(typeof(IconVisibilityEnum)));
            AddToggle(setting: EnableIconInfocUI, label: "Icon info", tooltip: "Enables a small window on the top right that information about the item location. This can be triggered with the mouse, or the dot that appears in the middle of the map when using controller");
            AddToggle(setting: DisableMapSway, label: "Disable map sway", tooltip: "Disables the swap in the map. Usefull when enabling Item UI for better pointing at icons.");
            ModLogger.Debug("UI components set up successfully");
        }

        public static MapVisibilityEnum GetMapVisibility()
        {
            return EnumParser.GetEnumValue<MapVisibilityEnum>(MapVisibility.Value);
        }
        public static IconVisibilityEnum GetIconVisibility()
        {
            return EnumParser.GetEnumValue<IconVisibilityEnum>(IconVisibility.Value);
        }
        public static bool GetEnableIconInfocUI()
        {
            return EnableIconInfocUI?.Value ?? false;
        }

        internal static bool GetDisableMapSway()
        {
            return DisableMapSway?.Value ?? false;
        }
    }
}


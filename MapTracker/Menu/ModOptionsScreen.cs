using BepInEx;
using BepInEx.Configuration;
using Game;
using OriBFArchipelago.MapTracker.Core;
using System.Linq;
using UnityEngine;

namespace OriBFArchipelago.MapTracker.Menu
{
    internal class ModOptionsScreen : BaseModOptionsScreen
    {
        private static ConfigFile _config;
        private static bool _isInitialized;
        public ModOptionsScreen()
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

        private void OnEnable()
        {
            // If we couldn't initialize in InitScreen, try again when the component is enabled
            if (!_isInitialized && SaveSlotsUI.Instance != null && SaveSlotsUI.Instance.CurrentSaveSlot != null)
            {
                _isInitialized = true;
                AddButton("Teleport to start", "Teleports Ori to the starting area.", TeleportToStart);
            }
        }

        private void InitializeSettings(ConfigFile config)
        {
            ModLogger.Debug("Initializing settings");
            MaptrackerSettings.MapVisibility = config.Bind("MapTracker", "MapVisibility", $"{MapVisibilityEnum.Visible}", "Sets map visibility");
            MaptrackerSettings.IconVisibility = config.Bind("MapTracker", "IconVisibility", $"{IconVisibilityEnum.In_Logic}", "Sets icon visibility");
            ModLogger.Debug("Settings initialized successfully");
        }

        private void SetComponents()
        {
            ModLogger.Debug("Setting up UI components");
            AddMultiToggle(setting: MaptrackerSettings.MapVisibility, label: "Map visiblity", tooltip: "Options: " + string.Join(", ", EnumParser.GetEnumNames(typeof(MapVisibilityEnum))), options: EnumParser.GetEnumNames(typeof(MapVisibilityEnum)));
            AddMultiToggle(setting: MaptrackerSettings.IconVisibility, label: "Icon visibility", tooltip: "Options: " + string.Join(", ", EnumParser.GetEnumNames(typeof(IconVisibilityEnum))), options: EnumParser.GetEnumNames(typeof(IconVisibilityEnum)));
            ModLogger.Debug("UI components set up successfully");
        }

        private void TeleportToStart()
        {
            try
            {
                UI.Menu.HideMenuScreen(true);
                var original = TeleporterController.Instance.Teleporters.FirstOrDefault();
                var originalPos = original.WorldPosition;
                original.WorldPosition = new Vector3(189, -219, 0);
                TeleporterController.BeginTeleportation(original);
                ModLogger.Debug("Teleport to start");
                original.WorldPosition = originalPos;
                
            }
            catch (System.Exception ex)
            {
                ModLogger.Error(ex.ToString());
            }
        }
    }
}


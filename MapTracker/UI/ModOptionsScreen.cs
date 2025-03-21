using BepInEx;
using BepInEx.Configuration;
using Game;
using OriBFArchipelago.Core;
using OriBFArchipelago.MapTracker.Core;
using System;
using System.Linq;
using UnityEngine;

namespace OriBFArchipelago.MapTracker.UI
{
    internal class ModOptionsScreen : BaseModOptionsScreen
    {
        private const string CONFIGSECTION = "MapTracker";


        private static ConfigFile _config;
        private static bool _isInitialized;

        private static ConfigEntry<string> MapVisibility { get; set; }
        private static ConfigEntry<string> IconVisibility { get; set; }
        private static ConfigEntry<bool> EnableLogicUI { get; set; }
        private static ConfigEntry<bool> DisableMapSway { get; set; }


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
            if (!_isInitialized && SaveSlotsUI.Instance != null && SaveSlotsUI.Instance.CurrentSaveSlot != null)
            {
                _isInitialized = true;
                AddButton("Teleport to start", "Teleports Ori to the starting area.", TeleportToStart);
            }
        }

        private void InitializeSettings(ConfigFile config)
        {
            ModLogger.Debug("Initializing settings");
            MapVisibility = config.Bind(CONFIGSECTION, "MapVisibility", $"{MapVisibilityEnum.Visible}", "Sets map visibility");
            IconVisibility = config.Bind(CONFIGSECTION, "IconVisibility", $"{IconVisibilityEnum.In_Logic}", "Sets icon visibility");
            EnableLogicUI = config.Bind(CONFIGSECTION, "EnableItemUI", false, "Sets enableitemui");
            DisableMapSway = config.Bind(CONFIGSECTION, "DisableMapSway", false, "Sets disablemapsway");
            ModLogger.Debug("Settings initialized successfully");
        }

        private void SetComponents()
        {
            ModLogger.Debug("Setting up UI components");
            AddMultiToggle(setting: MapVisibility, label: "Map visiblity", tooltip: "Options: " + string.Join(", ", EnumParser.GetEnumNames(typeof(MapVisibilityEnum))), options: EnumParser.GetEnumNames(typeof(MapVisibilityEnum)));
            AddMultiToggle(setting: IconVisibility, label: "Icon visibility", tooltip: "Options: " + string.Join(", ", EnumParser.GetEnumNames(typeof(IconVisibilityEnum))), options: EnumParser.GetEnumNames(typeof(IconVisibilityEnum)));
            AddToggle(setting: EnableLogicUI, label: "Icon info", tooltip: "Enables a small window on the top right that information about the item location. This can be triggered with the mouse, or the dot that appears in the middle of the map when using controller");
            AddToggle(setting: DisableMapSway, label: "Disable map sway", tooltip: "Disables the swap in the map. Usefull when enabling Item UI for better pointing at icons.");
            ModLogger.Debug("UI components set up successfully");
        }

        private void TeleportToStart()
        {
            try
            {
                if (Characters.Sein.Active && !Characters.Sein.IsSuspended && !Characters.Sein.Controller.IsSwimming && Characters.Sein.Controller.CanMove)
                {
                    Game.UI.Menu.HideMenuScreen(true);
                    var original = TeleporterController.Instance.Teleporters.FirstOrDefault();
                    var originalPos = original.WorldPosition; //Save original position - fornlorn cavern
                    original.WorldPosition = new Vector3(189, -219, 0); //Set position to starting location
                    TeleporterController.BeginTeleportation(original);
                    ModLogger.Debug("Teleport to start");
                    original.WorldPosition = originalPos; //Return position to original otherwise forlorn will always teleport to starting location
                }
                else
                    RandomizerMessager.instance.AddMessage("You can not teleport from here. Get to a save place where you can freely stand.");
            }
            catch (System.Exception ex)
            {
                ModLogger.Error(ex.ToString());
            }
        }


        public static MapVisibilityEnum GetMapVisibility()
        {
            return EnumParser.GetEnumValue<MapVisibilityEnum>(MapVisibility.Value);
        }
        public static IconVisibilityEnum GetIconVisibility()
        {
            return EnumParser.GetEnumValue<IconVisibilityEnum>(IconVisibility.Value);
        }
        public static bool GetEnableLogicUI()
        {
            return EnableLogicUI?.Value ?? false;
        }

        internal static bool GetDisableMapSway()
        {
            return DisableMapSway?.Value ?? false;
        }
    }
}


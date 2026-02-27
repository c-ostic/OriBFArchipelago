using BepInEx.Configuration;
using OriBFArchipelago.Core;
using OriBFArchipelago.MapTracker.Core;
using System;

namespace OriBFArchipelago.ArchipelagoUI
{
    internal class ArchipelagoOptionsScreen : BaseModOptionsScreen
    {
        private const string CONFIGSECTION = "Archipelago";
        private static ConfigFile _config;

        private static string ConfigSavePath { get { return RandomizerIO.GetFilePath("Archipelago.cfg"); } }
        private static ConfigEntry<bool> _skipCutscenes { get; set; }
        private static ConfigEntry<string> _lastUsedTeleporter { get; set; }
        private static ConfigEntry<bool> _doubleBashAssist {  get; set; }
        private static ConfigEntry<bool> _doubleBashTap { get; set; }
        private static ConfigEntry<bool> _grenadeJumpAssist { get; set; }
        private static ConfigEntry<string> _messagerState { get; set; }
        private static ConfigEntry<float> _messageDuration { get; set; }

        public ArchipelagoOptionsScreen()
        {
            ModLogger.Debug("Loaded ArchipelagoOptionsScreen");
        }
        public override void InitScreen()
        {
            _config = new ConfigFile(ConfigSavePath, true);
            InitializeSettings();
            SetComponents();
            LoadSettings();
        }
        private void InitializeSettings()
        {
            ModLogger.Debug("Initializing settings");
            _skipCutscenes = _config.Bind(CONFIGSECTION, "SkipCutscenes", false, "Sets skip cutscenes");
            _lastUsedTeleporter = _config.Bind(CONFIGSECTION, "LastTeleporterUsed", "none", "Sets last teleporter used");
            _doubleBashAssist = _config.Bind(CONFIGSECTION, "DoubleBashAssist", true, "Enables double bash");
            _doubleBashTap = _config.Bind(CONFIGSECTION, "DoubleBashTap", false, "Enables double bash tap");
            _grenadeJumpAssist = _config.Bind(CONFIGSECTION, "GrenadeJumpAssist", true, "Enables grenade jump");
            _messagerState = _config.Bind(CONFIGSECTION, "MessagerState", RandomizerMessager.MessagerState.All.ToString(), "Sets message state");
            _messageDuration = _config.Bind(CONFIGSECTION, "MessageDuration", 6f, "Sets message duration");
            ModLogger.Debug("Settings initialized successfully");
        }

        private void SetComponents()
        {
            try
            {
                ModLogger.Debug("Setting up UI components");
                AddToggle(_skipCutscenes, "Skip cutscenes", "Will skip nearly all cutscenes and remove the forced slow walk towards cutscenes.");
                AddToggle(_doubleBashAssist, "Double Bash Assist", "Enables a keybind to help perform a double bash trick.");
                AddToggle(_doubleBashTap, "Double Bash Tap", "When enabled, <need to review>");
                AddToggle(_grenadeJumpAssist, "Grenade Jump Assist", "Enables a keybind to help perform a grenade jump trick.");
                AddMultiToggle(_messagerState, "Messager State", "All: Show all AP messages.\nLocal: Only show messages that relate to this game.\nNone: Show no messages.", Enum.GetNames(typeof(RandomizerMessager.MessagerState)));
                AddSlider(_messageDuration, "Message Duration", 2f, 10f, 1f, "How long messages appear on screen.\nMin: 2 seconds\nMax: 10 seconds");
                ModLogger.Debug("UI components set up successfully");
            }
            catch (System.Exception ex)
            {
                ModLogger.Error(ex.ToString());
            }
        }

        private void LoadSettings()
        {
            TeleporterManager.SetLastTeleporter(LastUsedTeleporter);
        }

        internal static bool SkipCutscenes => _skipCutscenes?.Value ?? false;
        internal static string LastUsedTeleporter
        {
            get
            {
                return _lastUsedTeleporter?.Value ?? null;
            }
            set
            {
                _lastUsedTeleporter.Value = value;
            }
        }
        internal static bool DoubleBashAssist => _doubleBashAssist?.Value ?? true;
        internal static bool DoubleBashTap => _doubleBashTap?.Value ?? false;
        internal static bool GrenadeJumpAssist => _grenadeJumpAssist?.Value ?? true;
        internal static RandomizerMessager.MessagerState MessageState => (RandomizerMessager.MessagerState)Enum.Parse(typeof(RandomizerMessager.MessagerState), _messagerState.Value);
        internal static float MessageDuration => _messageDuration?.Value ?? 6f;
    }
}


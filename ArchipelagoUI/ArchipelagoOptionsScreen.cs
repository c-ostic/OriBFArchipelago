using BepInEx.Configuration;
using OriBFArchipelago.Core;
using OriBFArchipelago.MapTracker.Core;

namespace OriBFArchipelago.ArchipelagoUI
{
    internal class ArchipelagoOptionsScreen : BaseModOptionsScreen
    {
        private const string CONFIGSECTION = "Archipelago";
        private static ConfigFile _config;

        private static string ConfigSavePath { get { return RandomizerIO.GetFilePath("Archipelago.cfg"); } }
        private static ConfigEntry<bool> _skipCutscenes { get; set; }
        private static ConfigEntry<string> _lastUsedTeleporter { get; set; }
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
            ModLogger.Debug("Settings initialized successfully");
        }

        private void SetComponents()
        {
            try
            {
                ModLogger.Debug("Setting up UI components");
                AddToggle(_skipCutscenes, "Skip cutscenes", "Will skip nearly all cutscenes and remove the forced slow walk towards cutscenes.");
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
    }
}


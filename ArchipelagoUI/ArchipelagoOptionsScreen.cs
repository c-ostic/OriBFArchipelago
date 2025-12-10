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
        private static ConfigEntry<bool> _skipFinalEscape { get; set; }
        public ArchipelagoOptionsScreen()
        {
            ModLogger.Debug("Loaded ArchipelagoOptionsScreen");
        }
        public override void InitScreen()
        {
            _config = new ConfigFile(ConfigSavePath, true);
            InitializeSettings();
            SetComponents();
        }
        private void InitializeSettings()
        {
            ModLogger.Debug("Initializing settings");
            _skipCutscenes = _config.Bind(CONFIGSECTION, "SkipCutscenes", false, "Sets skip cutscenes");
            _skipFinalEscape = _config.Bind(CONFIGSECTION, "Skip final escape", false, "Skips final escape");
            ModLogger.Debug("Settings initialized successfully");
        }

        private void SetComponents()
        {
            try
            {
                ModLogger.Debug("Setting up UI components");
                    AddToggle(_skipCutscenes, "Skip cutscenes", "Will skip nearly all cutscenes and remove the forced slow walk towards cutscenes.");
                AddToggle(_skipFinalEscape, "Skip final escape", "Will skip the final escape sequence and finish the game on entering horu's door");
                ModLogger.Debug("UI components set up successfully");
            }
            catch (System.Exception ex)
            {
                ModLogger.Error(ex.ToString());
            }
        }

        internal static bool SkipCutscenes => _skipCutscenes?.Value ?? false;
        internal static bool SkipFinalEscape => _skipFinalEscape?.Value ?? false;
    }
}


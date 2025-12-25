using BepInEx;
using HarmonyLib;
using OriBFArchipelago.ArchipelagoUI;
using OriBFArchipelago.Core;
using OriBFArchipelago.MapTracker.Core;
using OriModding.BF.Core;
using System;
using System.Reflection;

namespace OriBFArchipelago
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class OriBFArchipelago : BaseUnityPlugin
    {
        private void Awake()
        {
            // Plugin startup logic
            ModLogger.Initialize(PluginInfo.PLUGIN_NAME, Logger);
            ModLogger.Info($"Plugin {PluginInfo.PLUGIN_GUID} v{GetAssemblyVersion()} is starting...");
            ModLogger.Info($"Plugin {PluginInfo.PLUGIN_GUID} has loaded successfully!");

            var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.PatchAll();

            SceneBootstrap.RegisterHandler(RandomiserBootstrap.SetupBootstrap, "Randomizer");

            Controllers.Add<RandomizerController>(null, "Randomizer");
            Controllers.Add<RandomizerMessager>(null, "Randomizer");
            Controllers.Add<RandomizerManager>(null, "Randomizer");
            Controllers.Add<Keybinder>(null, "Randomizer");
            Controllers.Add<RandomizerSettings>(null, "Randomizer");
            Controllers.Add<IconHoverUI>(null, "MapTracker");
        }

        public string GetAssemblyVersion()
        {
            var assembly = Assembly.GetExecutingAssembly();
            Version version = assembly.GetName().Version;
            return version.ToString();
        }

    }
}
using BepInEx;
using HarmonyLib;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using System;
using Game;
using System.Reflection;
using BepInEx.Logging;
using OriModding.BF.Core;
using System.IO;
using OriBFArchipelago.Core;

namespace OriBFArchipelago
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class OriBFArchipelago : BaseUnityPlugin
    {
        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.PatchAll();

            SceneBootstrap.RegisterHandler(RandomiserBootstrap.SetupBootstrap, "Randomizer");

            Controllers.Add<RandomizerController>(null, "Randomizer");
            //Controllers.Add<RandomizerMessager>(null, "Randomizer");
            Controllers.Add<RandomizerManager>(null, "Randomizer");
        }
    }
}

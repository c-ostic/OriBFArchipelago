using MonoMod.Utils;
using OriBFArchipelago.ArchipelagoUI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OriBFArchipelago.Core
{
    internal enum RandomizerSetting
    {
        DoubleBashAssist, // 0 is off, 1 in on
        BashTap, // 0 is off, 1 is on
        GrenadeJumpAssist, // 0 is off, 1 in on
        MessagerState, // 0 is none, 1 is local, 2 is all
        MessageDuration // number of seconds messages stay
    }

    internal class RandomizerSettings : MonoBehaviour
    {
        private static RandomizerSettings instance;

        // Since message duration will be a slider, it needs a min and max
        private const float MIN_MESSAGE_DURATION = 2f;
        private const float MAX_MESSAGE_DURATION = 10f;

        public static bool EnableDebug => false; //Enables debug settings; use only for programmers

        private static Dictionary<RandomizerSetting, int> defaultSettings = new Dictionary<RandomizerSetting, int>
        {
            { RandomizerSetting.DoubleBashAssist, 1 },
            { RandomizerSetting.BashTap, 0 },
            { RandomizerSetting.GrenadeJumpAssist, 1 },
            { RandomizerSetting.MessagerState, 2 },
            { RandomizerSetting.MessageDuration, 6 }
        };

        // saves the currently used settings
        private Dictionary<RandomizerSetting, int> settings = new Dictionary<RandomizerSetting, int>();

        // used as a basis to dynamically change the font size
        private Vector2 baseScreenSize = new Vector2(1920, 1080);

        private GUIStyle textStyle, buttonStyle;

        private void Awake()
        {
            instance = this;
            ShowSettings = false;

            textStyle = new GUIStyle();
            textStyle.wordWrap = false;
            textStyle.fontStyle = FontStyle.Bold;
            textStyle.normal.textColor = new Color(1f, 1f, 1f);

            // read settings from file, or (if it doesn't exist) write the defaults to a file
            Dictionary<RandomizerSetting, int> fileSettings;
            if (RandomizerIO.ReadSettings(out fileSettings))
            {
                foreach (RandomizerSetting setting in Enum.GetValues(typeof(RandomizerSetting)))
                {
                    if (fileSettings.ContainsKey(setting))
                    {
                        settings.Add(setting, fileSettings[setting]);
                    }
                    else
                    {
                        settings.Add(setting, defaultSettings[setting]);
                    }
                }
            }
            else
            {
                RandomizerIO.WriteSettings(defaultSettings);
                settings.AddRange(defaultSettings);
            }
        }

        private void OnGUI()
        {
            // Show options
            if (ShowSettings && Game.UI.Menu.CurrentScreen == MenuScreenManager.Screens.WorldMap)
            {
                textStyle.fontSize = (int)(25 * (Screen.width / baseScreenSize.x));

                // GetStyle can only be called from within OnGUI, so this needs to be here instead of Awake()
                buttonStyle = GUI.skin.GetStyle("button");
                buttonStyle.fontStyle = FontStyle.Bold;
                buttonStyle.normal.textColor = new Color(1f, 1f, 1f);

                // display in the bottom left corner
                GUILayout.BeginArea(new Rect(5, Screen.height * 4 / 5, Screen.width / 3, Screen.height / 5));

                GUILayout.BeginVertical();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Double Bash Assist: ", textStyle, GUILayout.ExpandWidth(false));
                settings[RandomizerSetting.DoubleBashAssist] = GUILayout.Toggle(settings[RandomizerSetting.DoubleBashAssist] == 1, "") ? 1 : 0;
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Bash Tap: ", textStyle, GUILayout.ExpandWidth(false));
                settings[RandomizerSetting.BashTap] = GUILayout.Toggle(settings[RandomizerSetting.BashTap] == 1, "") ? 1 : 0;
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Grenade Jump Assist: ", textStyle, GUILayout.ExpandWidth(false));
                settings[RandomizerSetting.GrenadeJumpAssist] = GUILayout.Toggle(settings[RandomizerSetting.GrenadeJumpAssist] == 1, "") ? 1 : 0;
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Show messages: ", textStyle, GUILayout.ExpandWidth(false));
                string[] stateOptions = { "None", "Minimal", "Verbose" };
                settings[RandomizerSetting.MessagerState] = GUILayout.Toolbar(settings[RandomizerSetting.MessagerState], stateOptions, buttonStyle);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Message Duration: ", textStyle, GUILayout.ExpandWidth(false));
                settings[RandomizerSetting.MessageDuration] = (int)GUILayout.HorizontalSlider(settings[RandomizerSetting.MessageDuration], MIN_MESSAGE_DURATION, MAX_MESSAGE_DURATION);
                GUILayout.Label(settings[RandomizerSetting.MessageDuration] + "", textStyle);
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();

                GUILayout.EndArea();
            }
        }

        public static bool SkipFinalEscape => ArchipelagoOptionsScreen.SkipFinalEscape;
        public static bool SkipCutscenes => ArchipelagoOptionsScreen.SkipCutscenes;
        public static bool ShowSettings { get; set; }
        public static bool InSaveSelect { get; set; }
        public static bool InGame { get; set; }
        public static int ActiveSaveSlot { get; set; }

        public static int Get(RandomizerSetting setting)
        {
            return instance.settings[setting];
        }

        public static void Save()
        {
            RandomizerIO.WriteSettings(instance.settings);
        }
    }
}

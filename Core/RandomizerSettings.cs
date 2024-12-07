using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OriBFArchipelago.Core
{
    internal class RandomizerSettings : MonoBehaviour
    {
        internal enum Setting
        {
            DoubleBashAssist, // 0 is off, 1 in on
            GrenadeJumpAssist, // 0 is off, 1 in on
            MessagerState, // 0 is none, 1 is local, 2 is all
            MessageDuration // number of seconds messages stay
        }

        private static RandomizerSettings instance;

        // Since message duration will be a slider, it needs a min and max
        private const float MIN_MESSAGE_DURATION = 2f;
        private const float MAX_MESSAGE_DURATION = 10f;

        private static Dictionary<Setting, int> defaultSettings = new Dictionary<Setting, int>
        {
            { Setting.DoubleBashAssist, 1 },
            { Setting.GrenadeJumpAssist, 1 },
            { Setting.MessagerState, 2 },
            { Setting.MessageDuration, 6 }
        };

        // saves the currently used settings
        private Dictionary<Setting, int> settings = new Dictionary<Setting, int>();

        // used as a basis to dynamically change the font size
        private Vector2 baseScreenSize = new Vector2(1920, 1080);

        private GUIStyle textStyle, buttonStyle;

        private void Awake()
        {
            settings = defaultSettings;
            instance = this;
            ShowSettings = false;

            textStyle = new GUIStyle();
            textStyle.wordWrap = false;
            textStyle.fontStyle = FontStyle.Bold;
            textStyle.normal.textColor = new Color(1f, 1f, 1f);

            // TODO: call RandomizerIO to get settings from file
            // TODO: write file if file does not exist
        }

        private void OnGUI()
        {
            // Show options
            if (ShowSettings)
            {
                textStyle.fontSize = (int)(25 * (Screen.width / baseScreenSize.x));

                // GetStyle can only be called from within OnGUI, so this needs to be here instead of Awake()
                buttonStyle = GUI.skin.GetStyle("button");
                buttonStyle.fontStyle = FontStyle.Bold;
                buttonStyle.normal.textColor = new Color(1f, 1f, 1f);

                // display in the top right corner
                GUILayout.BeginArea(new Rect(5, Screen.height * 3 / 4, Screen.width / 3, Screen.height / 4));

                GUILayout.BeginVertical();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Double Bash Assist: ", textStyle, GUILayout.ExpandWidth(false));
                settings[Setting.DoubleBashAssist] = GUILayout.Toggle(settings[Setting.DoubleBashAssist] == 1, "") ? 1 : 0;
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Grenade Jump Assist: ", textStyle, GUILayout.ExpandWidth(false));
                settings[Setting.GrenadeJumpAssist] = GUILayout.Toggle(settings[Setting.GrenadeJumpAssist] == 1, "") ? 1 : 0;
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Message Duration: ", textStyle, GUILayout.ExpandWidth(false));
                settings[Setting.MessageDuration] = (int)GUILayout.HorizontalSlider(settings[Setting.MessageDuration], MIN_MESSAGE_DURATION, MAX_MESSAGE_DURATION);
                GUILayout.Label(settings[Setting.MessageDuration] + "", textStyle);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Show messages: ", textStyle, GUILayout.ExpandWidth(false));
                string[] stateOptions = { "None", "Minimal", "Verbose" };
                settings[Setting.MessagerState] = GUILayout.Toolbar(settings[Setting.MessagerState], stateOptions, buttonStyle);
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();

                GUILayout.EndArea();
            }
        }

        public static bool ShowSettings { get; set; }

        public static int Get(Setting setting)
        {
            return instance.settings[setting];
        }

        public static void Save()
        {
            // TODO: save settings to file
            Console.WriteLine("Saving settings...");
        }
    }
}

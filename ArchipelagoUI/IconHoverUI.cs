using OriBFArchipelago.Core;
using OriBFArchipelago.MapTracker.Core;
using OriBFArchipelago.MapTracker.Logic;
using UnityEngine;

namespace OriBFArchipelago.ArchipelagoUI
{
    internal class IconHoverUI : MonoBehaviour
    {
        public IconHoverUI()
        {
            ModLogger.Info("Loaded logic screen");
        }
        public static bool ShowMapTrackerSettings { get; set; }
        public static RuntimeWorldMapIcon Icon { get; set; }
        public static RuntimeGameWorldArea Area { get; set; }

        private Vector2 baseScreenSize = new Vector2(1920, 1080);



        private GUIStyle textStyle;

        private void Awake()
        {
            textStyle = new GUIStyle();
            textStyle.wordWrap = false;
            textStyle.fontStyle = FontStyle.Bold;
            textStyle.normal.textColor = new Color(1f, 1f, 1f);
            textStyle.fontSize = (int)(10 * (Screen.width / baseScreenSize.x));
        }
        private void OnGUI()
        {

            try
            {
                if (RandomizerSettings.ShowSettings && ShowMapTrackerSettings && Icon != null && MaptrackerSettings.EnableIconInfocUI)
                {
                    // Set the depth before any GUI calls
                    GUI.depth = 0;

                    textStyle.fontSize = (int)(10 * (Screen.width / baseScreenSize.x));
                    GUI.backgroundColor = Color.black;

                    GUILayout.BeginArea(new Rect(Screen.width * 4 / 5, 5, Screen.width / 3, Screen.height / 5));
                    GUILayout.BeginVertical();

                    var trackerItem = LogicManager.Get(Icon);


                    if (trackerItem != null)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label($"Name: {trackerItem.Name}", textStyle, GUILayout.ExpandWidth(false));
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        GUILayout.Label($"Item: {trackerItem.Type}", textStyle, GUILayout.ExpandWidth(false));
                        GUILayout.EndHorizontal();
                    }

                    if (RandomizerSettings.EnableDebug)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label($"Guid: {new System.Guid(Icon.Guid.ToByteArray())}", textStyle, GUILayout.ExpandWidth(false));
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        GUILayout.Label($"MoonGuid: {Icon.Guid}", textStyle, GUILayout.ExpandWidth(false));
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        GUILayout.Label($"Position: {Icon.Position}", textStyle, GUILayout.ExpandWidth(false));
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        GUILayout.Label($"Area: {Area?.Area?.name}", textStyle, GUILayout.ExpandWidth(false));
                        GUILayout.EndHorizontal();
                    }

                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"In Logic: {(LogicManager.IsInLogic(Icon) ? "yes" : "no")}", textStyle, GUILayout.ExpandWidth(false));
                    GUILayout.EndHorizontal();

                    if (trackerItem != null)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label($"Status: {RandomizerManager.Receiver.GetLocationStatus(trackerItem.Name)}", textStyle, GUILayout.ExpandWidth(false));
                        GUILayout.EndHorizontal();
                    }

                    //todo: Add logic information - rotate through multiple every x-seconds?

                    GUILayout.EndVertical();
                    GUILayout.EndArea();
                }
            }
            catch (System.Exception ex)
            {
                ModLogger.Error($"{ex}");
            }
        }
    }
}

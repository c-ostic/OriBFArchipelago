using Game;
using HarmonyLib;
using OriBFArchipelago.Core;
using System.Collections.Generic;
using UnityEngine;

namespace OriBFArchipelago.ArchipelagoUI
{
    [HarmonyPatch]
    internal class VersionUI : MonoBehaviour
    {
        private static GUIStyle textStyle;
        private static readonly float textPadding = 10f; // Padding from the top in pixels
        private static readonly Color textColor = new Color(1f, 1f, 1f, 0.6f); // White with slight transparency

        [HarmonyPatch(typeof(GameController), "Start")]
        [HarmonyPostfix]
        private static void AddLocationText(GameController __instance)
        {
            GameObject guiObject = new GameObject("Version");
            guiObject.AddComponent<VersionUI>();
            DontDestroyOnLoad(guiObject);
        }

        private void Start()
        {
            // Create the text style only once when the component starts
            if (textStyle == null)
            {
                CreateTextStyle();
            }
        }

        private void OnGUI()
        {
            if (!RandomizerSettings.ShowSettings || UI.Menu.CurrentScreen != MenuScreenManager.Screens.WorldMap)
                return;

            if (textStyle == null)
                return;

            string checksText = $"v{PluginInfo.PLUGIN_VERSION}";

            Vector2 textSize = textStyle.CalcSize(new GUIContent(checksText));
            Rect textRect = new Rect(Screen.width - textSize.x - textPadding, Screen.height - textSize.y - textPadding, textSize.x, textSize.y);
            Color prevColor = GUI.color;
            GUI.color = textColor;
            GUI.Label(textRect, checksText, textStyle);
            GUI.color = prevColor;
        }

        private void CreateTextStyle()
        {
            textStyle = new GUIStyle();
            textStyle.fontSize = 10;
            textStyle.fontStyle = FontStyle.Bold;
            textStyle.normal.textColor = Color.white;
            textStyle.richText = true;
        }

        private void OnDestroy()
        {
            // Clean up resources if needed
            textStyle = null;
        }
        public List<GameObject> FindObjectsNearPosition(Vector3 position, float radius)
        {
            List<GameObject> nearbyObjects = new List<GameObject>();

            // Find all colliders within the specified radius
            Collider[] hitColliders = Physics.OverlapSphere(position, radius);

            // Add the corresponding GameObjects to the list
            foreach (Collider collider in hitColliders)
            {
                nearbyObjects.Add(collider.gameObject);
            }

            return nearbyObjects;
        }
    }
}

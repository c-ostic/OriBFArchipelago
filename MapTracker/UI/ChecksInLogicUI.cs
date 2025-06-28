using Game;
using HarmonyLib;
using OriBFArchipelago.Core;
using OriBFArchipelago.MapTracker.Core;
using System.Collections.Generic;
using UnityEngine;

namespace OriBFArchipelago.MapTracker.UI
{
    [HarmonyPatch]
    internal class ChecksInLogicUI : MonoBehaviour
    {
        private static GUIStyle textStyle;
        private static readonly float textPadding = 10f; // Padding from the top in pixels
        private static readonly Color textColor = new Color(1f, 1f, 1f, 0.6f); // White with slight transparency

        [HarmonyPatch(typeof(GameController), "Start")]
        [HarmonyPostfix]
        private static void AddLocationText(GameController __instance)
        {
            GameObject guiObject = new GameObject("ChecksInLogicUI");
            guiObject.AddComponent<ChecksInLogicUI>();
            Object.DontDestroyOnLoad(guiObject);
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
            if (!RandomizerSettings.ShowSettings || Game.UI.Menu.CurrentScreen != MenuScreenManager.Screens.WorldMap)
                return;

            if (textStyle == null)
                return;

            if (MaptrackerSettings.ChecksLeft == 0)
                return;

            float centerX = Screen.width / 2f;

            int checksInLogic = MaptrackerSettings.ChecksInLogic;
            int checksLeft = MaptrackerSettings.ChecksLeft;
            string checksText = $"{checksInLogic} out of {checksLeft} are reachable";

            Vector2 textSize = textStyle.CalcSize(new GUIContent(checksText));
            Rect textRect = new Rect(centerX - textSize.x / 2f, textPadding, textSize.x, textSize.y);
            Color prevColor = GUI.color;
            GUI.color = textColor;
            GUI.Label(textRect, checksText, textStyle);
            GUI.color = prevColor;
        }

        private void CreateTextStyle()
        {
            textStyle = new GUIStyle();
            textStyle.fontSize = 18;
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
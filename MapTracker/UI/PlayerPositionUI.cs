using Game;
using HarmonyLib;
using OriBFArchipelago.Core;
using System.Collections.Generic;
using UnityEngine;

namespace OriBFArchipelago.MapTracker.UI
{
    [HarmonyPatch]
    internal class PlayerPositionUI : MonoBehaviour
    {
        private static GUIStyle textStyle;
        private static readonly float textPadding = 10f; // Padding from the top in pixels
        private static readonly Color textColor = new Color(1f, 1f, 1f, 0.8f); // White with slight transparency        

        [HarmonyPatch(typeof(GameController), "Start")]
        [HarmonyPostfix]
        private static void AddLocationText(GameController __instance)
        {
            if (RandomizerSettings.EnableDebug)
            {
                GameObject guiObject = new GameObject("LocationTextGUI");
                guiObject.AddComponent<PlayerPositionUI>();
                Object.DontDestroyOnLoad(guiObject);
            }
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
            // Check control scheme directly in OnGUI
            if (textStyle == null || Characters.Sein?.Position == null)
                return;

            // Get screen center for X position
            float centerX = Screen.width / 2f;

            var position = Characters.Sein.Position;
            var positionText = $"{(int)position.x} {(int)position.y} {(int)position.z}";
            // Calculate text size to center it properly
            Vector2 textSize = textStyle.CalcSize(new GUIContent(positionText));

            // Draw text at the top middle of the screen
            Rect textRect = new Rect(
                centerX - textSize.x / 2f,
                textPadding,
                textSize.x,
                textSize.y
            );

            Color prevColor = GUI.color;
            GUI.color = textColor;
            GUI.Label(textRect, positionText, textStyle);
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
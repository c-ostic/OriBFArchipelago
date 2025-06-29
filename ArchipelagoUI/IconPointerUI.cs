using HarmonyLib;
using OriBFArchipelago.Core;
using OriBFArchipelago.MapTracker.Core;
using UnityEngine;

namespace OriBFArchipelago.ArchipelagoUI
{

    [HarmonyPatch]
    internal class IconPointerUI : MonoBehaviour
    {
        private static Texture2D circleTexture;
        private static readonly float circleSize = 10f; // Size in pixels
        private static readonly Color circleColor = new Color(1f, 1f, 1f, 0.8f); // White with slight transparency

        [HarmonyPatch(typeof(GameController), "Start")]  // Or another appropriate base game class/method
        [HarmonyPostfix]
        private static void AddCenterPointer(GameController __instance)
        {
            GameObject guiObject = new GameObject("CenterPointerGUI");
            guiObject.AddComponent<IconPointerUI>();
            DontDestroyOnLoad(guiObject);
        }

        private void Start()
        {
            // Create the circle texture only once when the component starts
            if (circleTexture == null)
            {
                CreateCircleTexture();
            }
        }

        private void OnGUI()
        {
            // Check control scheme directly in OnGUI
            if (circleTexture == null || GameSettings.Instance.CurrentControlScheme != ControlScheme.Controller || !RandomizerSettings.ShowSettings || !MaptrackerSettings.EnableIconInfocUI || Game.UI.Menu.CurrentScreen != MenuScreenManager.Screens.WorldMap )
                return;

            // Get screen center
            float centerX = Screen.width / 2f;
            float centerY = Screen.height / 2f;

            // Draw circle using GUI.DrawTexture
            Rect circleRect = new Rect(
                centerX - circleSize / 2f,
                centerY - circleSize / 2f,
                circleSize,
                circleSize
            );

            Color prevColor = GUI.color;
            GUI.color = circleColor;
            GUI.DrawTexture(circleRect, circleTexture);
            GUI.color = prevColor;
        }

        private void CreateCircleTexture()
        {
            circleTexture = new Texture2D(32, 32);
            float radius = 16f;
            Vector2 center = new Vector2(16, 16);

            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    Color pixelColor = distance <= radius ? Color.white : Color.clear;
                    circleTexture.SetPixel(x, y, pixelColor);
                }
            }

            circleTexture.Apply();
        }

        private void OnDestroy()
        {
            // Only destroy the texture if this is the last instance being destroyed
            if (circleTexture != null && !gameObject.scene.isLoaded)
            {
                Destroy(circleTexture);
                circleTexture = null;
            }
        }
    }
}

using HarmonyLib;
using OriBFArchipelago.Core;
using OriBFArchipelago.MapTracker.Core;
using System.Collections.Generic;
using UnityEngine;
using CoreInput = Core.Input;

namespace OriBFArchipelago.ArchipelagoUI
{
    /// <summary>
    /// Adds a native "Goal progress" hint button to the world map's bottom legend
    /// (mirroring the teleport hint buttons) plus a toggleable panel that lists
    /// progress toward each active goal. The GoalProgress keybind (Alt+G) still
    /// shows the transient message during gameplay as before.
    /// </summary>
    internal class GoalProgressUI
    {
        [HarmonyPatch(typeof(GameMapUI), nameof(GameMapUI.Awake))]
        public static class GameMapUI_Awake_Patch
        {
            [HarmonyPostfix]
            static void Awake_Postfix(GameMapUI __instance)
            {
                if (__instance.GetComponent<GoalProgressHintDrawer>() == null)
                    __instance.gameObject.AddComponent<GoalProgressHintDrawer>();
            }
        }

        public class GoalProgressHintDrawer : MonoBehaviour
        {
            // ---- Tweakables --------------------------------------------------
            // Keyboard fallback. The controller button is the left bumper (free on
            // the map's normal state; the teleport bumpers are on the inventory screen).
            private static readonly KeyCode ToggleKey = KeyCode.F5;
            private const string ControllerIcon = "<icon>R</>"; // Left Shoulder glyph
            // Local-space gap placing the new hint just right of the "back" entry.
            private const float RightGap = 0.8f;
            // ------------------------------------------------------------------

            private static bool showPanel = false;

            private GameMapUI gameMapUI;
            private GameObject hint;
            private bool hintCreated;
            private bool wasKeyboardUsedLast;

            private GUIStyle panelStyle;
            private Texture2D backgroundTexture;
            private const float PanelContentWidth = 360f;
            private const float Padding = 12f;
            private static readonly Color TextColor = new Color(1f, 1f, 1f, 0.9f);
            private static readonly Color BackgroundColor = new Color(0f, 0f, 0f, 0.6f);

            private void Awake()
            {
                gameMapUI = GetComponent<GameMapUI>();
            }

            private void Start()
            {
                CreateHint();
                CreateStyles();
            }

            /// <summary>True while the player is looking at the normal (non-objective,
            /// non-teleporter) world map, which is when the legend and panel belong.</summary>
            private bool OnNormalMap()
            {
                return gameMapUI != null
                    && gameMapUI.IsVisible
                    && !gameMapUI.ShowingObjective
                    && !gameMapUI.ShowingTeleporters;
            }

            private void Update()
            {
                if (!OnNormalMap())
                    return;

                bool toggle = UnityEngine.Input.GetKeyDown(ToggleKey);

                if (CoreInput.LeftShoulder.OnPressed && !CoreInput.LeftShoulder.Used)
                {
                    CoreInput.LeftShoulder.Used = true;
                    toggle = true;
                }

                if (toggle)
                    showPanel = !showPanel;

                // Swap the button glyph when the player switches keyboard <-> controller.
                bool keyboard = PlayerInput.Instance != null && PlayerInput.Instance.WasKeyboardUsedLast;
                if (hintCreated && keyboard != wasKeyboardUsedLast)
                {
                    UpdateHintText();
                    wasKeyboardUsedLast = keyboard;
                }
            }

            private void CreateHint()
            {
                if (gameMapUI == null || gameMapUI.BottomLegend == null)
                {
                    ModLogger.Debug("GoalProgressUI: BottomLegend not found");
                    return;
                }

                Transform legend = gameMapUI.BottomLegend.transform;
                if (legend.childCount == 0)
                {
                    ModLogger.Debug("GoalProgressUI: BottomLegend has no hint buttons to clone");
                    return;
                }

                // Log the legend layout so the placement can be tuned if needed.
                for (int i = 0; i < legend.childCount; i++)
                {
                    Transform c = legend.GetChild(i);
                    ModLogger.Debug($"GoalProgressUI: BottomLegend child[{i}] '{c.name}' localPos={c.localPosition}");
                }

                // The bottom legend (zoom, navigate, back) is full, but there is clear
                // space to the right of "back". Clone it and place the new entry there,
                // the same way the teleport RB hint sits to the right of its template.
                Transform backButton = legend.Find("back");
                Transform template = backButton != null ? backButton : legend.GetChild(legend.childCount - 1);

                hint = Instantiate(template.gameObject);
                hint.transform.SetParent(legend);
                hint.name = "goal_progress_hint";

                Vector3 pos = template.localPosition;
                pos.x += RightGap;
                hint.transform.localPosition = pos;
                hint.transform.localRotation = template.localRotation;
                hint.transform.localScale = template.localScale;

                hintCreated = true;
                wasKeyboardUsedLast = PlayerInput.Instance != null && PlayerInput.Instance.WasKeyboardUsedLast;
                UpdateHintText();
            }

            private void UpdateHintText()
            {
                if (hint == null)
                    return;

                MessageBox messageBox = hint.GetComponent<MessageBox>();
                if (messageBox == null)
                    return;

                string icon = (PlayerInput.Instance != null && PlayerInput.Instance.WasKeyboardUsedLast)
                    ? ToggleKey.ToString()
                    : ControllerIcon;

                // Clear the provider so OverrideText takes precedence.
                messageBox.MessageProvider = null;
                messageBox.OverrideText = $"{icon}  Goal Legend";
                messageBox.RefreshText();
            }

            private void OnGUI()
            {
                if (!showPanel || !OnNormalMap() || panelStyle == null)
                    return;

                List<string> lines;
                try
                {
                    lines = RandomizerManager.Connection?.GetGoalProgressLines();
                }
                catch
                {
                    return;
                }

                if (lines == null || lines.Count == 0)
                    return;

                List<string> content = new List<string> { "Goal Progress" };
                content.AddRange(lines);

                const float innerPad = 12f;
                const float lineSpacing = 4f;

                float totalHeight = 0f;
                foreach (string line in content)
                    totalHeight += panelStyle.CalcHeight(new GUIContent(line), PanelContentWidth) + lineSpacing;

                float boxWidth = PanelContentWidth + innerPad * 2;
                Rect box = new Rect(Screen.width - boxWidth - Padding, Padding, boxWidth, totalHeight + innerPad * 2);

                Color prev = GUI.color;

                GUI.color = BackgroundColor;
                GUI.DrawTexture(box, backgroundTexture);

                float y = box.y + innerPad;
                foreach (string line in content)
                {
                    float h = panelStyle.CalcHeight(new GUIContent(line), PanelContentWidth);
                    GUI.color = TextColor;
                    GUI.Label(new Rect(box.x + innerPad, y, PanelContentWidth, h), line, panelStyle);
                    y += h + lineSpacing;
                }

                GUI.color = prev;
            }

            private void CreateStyles()
            {
                panelStyle = new GUIStyle
                {
                    fontSize = 16,
                    fontStyle = FontStyle.Bold,
                    richText = true,
                    wordWrap = true
                };
                panelStyle.normal.textColor = Color.white;

                backgroundTexture = new Texture2D(1, 1);
                backgroundTexture.SetPixel(0, 0, Color.white);
                backgroundTexture.Apply();
            }

            private void OnDestroy()
            {
                panelStyle = null;
                if (backgroundTexture != null)
                {
                    Destroy(backgroundTexture);
                    backgroundTexture = null;
                }
            }
        }
    }
}

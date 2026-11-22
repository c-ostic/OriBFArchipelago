using HarmonyLib;
using OriBFArchipelago.MapTracker.Core;
using SmartInput;
using System.Collections.Generic;
using UnityEngine;
using CoreInput = Core.Input;

namespace OriBFArchipelago.ArchipelagoUI
{
    /// <summary>
    /// A toggleable info panel shown on the world map, driven by a hint button in the
    /// map's bottom legend. Each concrete panel (goal progress, AP settings, ...) lives
    /// in its own file; <see cref="MapPanelController"/> hosts and lays them all out.
    /// </summary>
    internal abstract class MapPanel
    {
        /// <summary>Whether the panel is currently shown. Persisted between map opens.</summary>
        public bool Visible;

        /// <summary>Keyboard key that toggles this panel individually.</summary>
        public abstract KeyCode ToggleKey { get; }

        /// <summary>Anchor the panel to the bottom-right (else top-right) of the screen.</summary>
        public abstract bool AnchorBottomRight { get; }

        /// <summary>Heading rendered at the top of the panel.</summary>
        public abstract string Title { get; }

        /// <summary>The lines of content to render. Called every frame while visible.</summary>
        public abstract List<string> GetLines();
    }

    /// <summary>
    /// Attaches to <see cref="GameMapUI"/> and manages every <see cref="MapPanel"/>:
    /// clones a legend hint per panel, measures and re-packs the whole bottom legend so
    /// nothing overlaps, reads the toggle inputs, and draws the visible panels. The
    /// GoalProgress keybind (Alt+G) still shows its transient message as before.
    /// </summary>
    internal class MapPanelController : MonoBehaviour
    {
        [HarmonyPatch(typeof(GameMapUI), nameof(GameMapUI.Awake))]
        public static class GameMapUI_Awake_Patch
        {
            [HarmonyPostfix]
            static void Awake_Postfix(GameMapUI __instance)
            {
                if (__instance.GetComponent<MapPanelController>() == null)
                    __instance.gameObject.AddComponent<MapPanelController>();
                if (__instance.GetComponent<MapCheckNavigator>() == null)
                    __instance.gameObject.AddComponent<MapCheckNavigator>();
            }
        }

        /// <summary>
        /// A is the panels toggle, but vanilla also reads it in
        /// <see cref="GameMapTransitionManager.Advance"/> to zoom from the world map into
        /// the area map. Mark the press used before that runs so it doesn't do both.
        /// This has to be a patch rather than Update(): Advance is driven from
        /// GameMapUI.FixedUpdate, which runs before Update within a frame.
        /// </summary>
        [HarmonyPatch(typeof(GameMapTransitionManager), nameof(GameMapTransitionManager.Advance))]
        public static class GameMapTransitionManager_Advance_Patch
        {
            [HarmonyPrefix]
            static void Advance_Prefix()
            {
                if (IsOnNormalMap() && new ControllerButtonInput(PanelsToggleButton).GetButton())
                    CoreInput.ActionButtonA.Used = true;
            }
        }

        // ---- Legend layout (legend-local units) ------------------------------
        // Entries are packed left-to-right across [zoomX - LeftExtend, RightLimit]
        // with DesiredGap between them, shrinking uniformly if the row would overflow.
        // Widths are measured at runtime; FallbackWidth is only used if measuring fails.
        private const float LeftExtend = 0.4f;
        private const float RightLimit = 4.3f;
        private const float DesiredGap = 0.18f;
        private const float FallbackWidth = 0.9f;
        // ----------------------------------------------------------------------

        // Static so panel visibility survives the map being closed/reopened.
        private static readonly MapPanel[] Panels = { new GoalProgressPanel(), new ApSettingsPanel() };

        // A single controller button shows/hides both info panels together.
        // (The D-pad isn't exposed as a distinct button, and the bumpers now flip
        // through checks - see MapCheckNavigator.)
        //
        // Vanilla binds A on the world map to "zoom into the area map"; we swallow that
        // press below so A only toggles the panels. The right trigger and the mouse
        // wheel still zoom in.
        private const XboxControllerInput.Button PanelsToggleButton = XboxControllerInput.Button.ButtonA;

        // Glyph for the button above, from the game's ButtonIconUtility table.
        private const string PanelsToggleIcon = "<icon>e</>";

        private GameMapUI gameMapUI;
        private Transform legendRoot;
        private GameObject[] panelHints;
        private Transform[] layoutItems;
        private float layoutZoomX;
        private bool hintsCreated;
        private bool layoutDone;
        private bool wasKeyboardUsedLast;

        private readonly List<Transform> heldItems = new List<Transform>();
        private readonly List<float> heldX = new List<float>();
        private readonly List<Vector3> heldScale = new List<Vector3>();

        // Previous-frame held state of the shared panels toggle button, for edge detection
        // (ControllerButtonInput.GetButton reports held-state, not a one-shot press).
        private bool panelsButtonHeldLast;

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
            CreateHints();
            CreateStyles();
        }

        /// <summary>True while the player is on the normal (non-objective,
        /// non-teleporter) world map, where the legend and panels belong.</summary>
        private bool OnNormalMap()
        {
            return IsOnNormalMap(gameMapUI);
        }

        private static bool IsOnNormalMap()
        {
            return IsOnNormalMap(GameMapUI.Instance);
        }

        private static bool IsOnNormalMap(GameMapUI map)
        {
            return map != null
                && map.IsVisible
                && !map.ShowingObjective
                && !map.ShowingTeleporters;
        }

        private void Update()
        {
            if (!OnNormalMap())
                return;

            // Lay out once, now that the entries are visible and measurable.
            if (hintsCreated && !layoutDone)
            {
                LayoutLegend();
                layoutDone = true;
            }

            // Hold the packed entries in place in case the game re-lays them out.
            for (int i = 0; i < heldItems.Count; i++)
            {
                if (heldItems[i] != null)
                    SetHold(heldItems[i], heldX[i], heldScale[i]);
            }

            HandleToggleInput();

            bool keyboard = PlayerInput.Instance != null && PlayerInput.Instance.WasKeyboardUsedLast;
            if (hintsCreated && keyboard != wasKeyboardUsedLast)
            {
                UpdateHintText();
                wasKeyboardUsedLast = keyboard;
            }
        }

        /// <summary>
        /// Keyboard keys (F5/F6) toggle each panel individually; the shared controller
        /// button (X) shows or hides both panels together.
        /// </summary>
        private void HandleToggleInput()
        {
            // Keyboard: per-panel individual toggle.
            foreach (MapPanel panel in Panels)
            {
                if (UnityEngine.Input.GetKeyDown(panel.ToggleKey))
                    panel.Visible = !panel.Visible;
            }

            // Controller: one button toggles both panels together (edge-detected).
            bool now = new ControllerButtonInput(PanelsToggleButton).GetButton();
            if (now && !panelsButtonHeldLast)
            {
                bool anyVisible = false;
                foreach (MapPanel panel in Panels)
                    anyVisible |= panel.Visible;

                bool show = !anyVisible;
                foreach (MapPanel panel in Panels)
                    panel.Visible = show;
            }
            panelsButtonHeldLast = now;
        }

        private void CreateHints()
        {
            if (gameMapUI == null || gameMapUI.BottomLegend == null)
            {
                ModLogger.Debug("MapPanelController: BottomLegend not found");
                return;
            }

            legendRoot = gameMapUI.BottomLegend.transform;

            Transform zoomEntry = legendRoot.Find("zoom");
            Transform navigateEntry = legendRoot.Find("navigate");
            Transform backEntry = legendRoot.Find("back");

            Transform template = backEntry != null
                ? backEntry
                : (legendRoot.childCount > 0 ? legendRoot.GetChild(legendRoot.childCount - 1) : null);
            if (template == null)
            {
                ModLogger.Debug("MapPanelController: no legend button to clone");
                return;
            }

            panelHints = new GameObject[Panels.Length];
            for (int i = 0; i < Panels.Length; i++)
                panelHints[i] = CloneHint(template, legendRoot, $"appanel_hint_{i}");

            List<Transform> items = new List<Transform> { zoomEntry, navigateEntry, backEntry };
            foreach (GameObject h in panelHints)
                items.Add(h.transform);
            layoutItems = items.ToArray();
            layoutZoomX = zoomEntry != null ? zoomEntry.localPosition.x : 0f;

            hintsCreated = true;
            wasKeyboardUsedLast = PlayerInput.Instance != null && PlayerInput.Instance.WasKeyboardUsedLast;
            UpdateHintText();
        }

        /// <summary>
        /// Measures each entry's rendered width (in legend-local units) and packs the
        /// row across [zoomX - LeftExtend, RightLimit] with DesiredGap between entries,
        /// shrinking everything uniformly if it would overflow.
        /// </summary>
        private void LayoutLegend()
        {
            if (layoutItems == null || legendRoot == null)
                return;

            float lossyX = Mathf.Abs(legendRoot.lossyScale.x);

            float[] widths = new float[layoutItems.Length];
            float total = 0f;
            for (int i = 0; i < layoutItems.Length; i++)
            {
                widths[i] = MeasureLocalWidth(layoutItems[i], lossyX, FallbackWidth);
                total += widths[i];
            }

            float leftStart = layoutZoomX - LeftExtend;
            float available = RightLimit - leftStart;
            int gaps = layoutItems.Length - 1;

            float gap = DesiredGap;
            float scale = total > 0f ? (available - gap * gaps) / total : 1f;
            if (scale > 1f)
            {
                scale = 1f;
                gap = gaps > 0 ? Mathf.Max(0f, (available - total) / gaps) : 0f;
            }
            if (scale < 0.1f)
                scale = 0.1f;

            heldItems.Clear();
            heldX.Clear();
            heldScale.Clear();

            float x = leftStart;
            for (int i = 0; i < layoutItems.Length; i++)
            {
                if (layoutItems[i] != null)
                {
                    Vector3 targetScale = layoutItems[i].localScale * scale;
                    heldItems.Add(layoutItems[i]);
                    heldX.Add(x);
                    heldScale.Add(targetScale);
                    SetHold(layoutItems[i], x, targetScale);
                }
                x += widths[i] * scale + gap;
            }
        }

        private static float MeasureLocalWidth(Transform entry, float legendLossyX, float fallback)
        {
            if (entry == null || legendLossyX <= 0f)
                return fallback;

            Renderer[] renderers = entry.GetComponentsInChildren<Renderer>(true);
            bool any = false;
            Bounds bounds = new Bounds();
            foreach (Renderer r in renderers)
            {
                if (!any)
                {
                    bounds = r.bounds;
                    any = true;
                }
                else
                {
                    bounds.Encapsulate(r.bounds);
                }
            }

            if (!any || bounds.size.x <= 0f)
                return fallback;

            return bounds.size.x / legendLossyX;
        }

        private GameObject CloneHint(Transform template, Transform legend, string name)
        {
            GameObject clone = Instantiate(template.gameObject);
            clone.transform.SetParent(legend);
            clone.name = name;
            clone.transform.localRotation = template.localRotation;
            clone.transform.localScale = template.localScale;
            clone.transform.localPosition = template.localPosition;
            return clone;
        }

        private static void SetHold(Transform t, float x, Vector3 scale)
        {
            Vector3 p = t.localPosition;
            p.x = x;
            t.localPosition = p;
            t.localScale = scale;
        }

        private void UpdateHintText()
        {
            if (panelHints == null || panelHints.Length < 2)
                return;

            bool keyboard = PlayerInput.Instance != null && PlayerInput.Instance.WasKeyboardUsedLast;

            // Hint 0: flip through in-logic checks with the bumpers (controller-only feature).
            SetHintText(panelHints[0], "<icon>R</> <icon>S</>", "Checks");

            // Hint 1: show/hide both info panels. One controller button (A) for both;
            // the keyboard keeps the individual F5/F6 keys.
            string panelsIcon = keyboard ? "F5/F6" : PanelsToggleIcon;
            SetHintText(panelHints[1], panelsIcon, "Goals / Settings");
        }

        private void SetHintText(GameObject hintObj, string icon, string label)
        {
            if (hintObj == null)
                return;

            MessageBox messageBox = hintObj.GetComponent<MessageBox>();
            if (messageBox == null)
                return;

            messageBox.MessageProvider = null;
            messageBox.OverrideText = $"{icon}  {label}";
            messageBox.RefreshText();
        }

        private void OnGUI()
        {
            if (!OnNormalMap() || panelStyle == null)
                return;

            foreach (MapPanel panel in Panels)
            {
                if (!panel.Visible)
                    continue;

                List<string> lines = panel.GetLines();
                if (lines != null && lines.Count > 0)
                    DrawPanel(panel.Title, lines, panel.AnchorBottomRight);
            }
        }

        private void DrawPanel(string title, List<string> lines, bool bottomRight)
        {
            List<string> content = new List<string> { title };
            content.AddRange(lines);

            const float innerPad = 12f;
            const float lineSpacing = 4f;

            float totalHeight = 0f;
            foreach (string line in content)
                totalHeight += panelStyle.CalcHeight(new GUIContent(line), PanelContentWidth) + lineSpacing;

            float boxWidth = PanelContentWidth + innerPad * 2;
            float boxHeight = totalHeight + innerPad * 2;
            float boxX = Screen.width - boxWidth - Padding;
            float boxY = bottomRight ? Screen.height - boxHeight - Padding : Padding;
            Rect box = new Rect(boxX, boxY, boxWidth, boxHeight);

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

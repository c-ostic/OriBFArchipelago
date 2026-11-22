using OriBFArchipelago.MapTracker.Core;
using OriBFArchipelago.MapTracker.Logic;
using UnityEngine;

namespace OriBFArchipelago.ArchipelagoUI
{
    /// <summary>
    /// Draws a colored ring around each check icon on the map to communicate its logic state:
    ///   green  = in logic at the current difficulty
    ///   yellow = reachable only at a harder difficulty
    ///   red    = not reachable even at the hardest difficulty
    ///   grey   = already collected
    /// Non-check icons (doors, walls, keystones, save pedestals) get no ring.
    ///
    /// The ring re-evaluates its color every time the map opens (via <see cref="LogicRingUI"/>'s
    /// OnEnable), so colors update as the player unlocks new access.
    /// </summary>
    internal static class LogicIconRings
    {
        public static bool Enabled = true;

        private const string RingObjectName = "APLogicRing";

        // Ring size relative to the icon transform. Tuned so the ring hugs the icon art.
        private const float RingScale = 0.6f;

        private static Sprite _ringSprite;

        public static void Apply(RuntimeWorldMapIcon icon, GameObject go)
        {
            if (!Enabled || go == null)
                return;

            try
            {
                // Only real check locations get a ring.
                if (LogicManager.GetLogicState(icon) == IconLogicState.NotACheck)
                    return;

                // Never add a second ring to the same icon GameObject.
                if (go.transform.Find(RingObjectName) != null)
                    return;

                var ring = new GameObject(RingObjectName);
                ring.transform.SetParent(go.transform, false);
                ring.transform.localPosition = Vector3.zero;
                ring.transform.localRotation = Quaternion.identity;
                ring.transform.localScale = Vector3.one * RingScale;

                // The map renders on a specific Unity layer/camera; a fresh GameObject
                // defaults to layer 0 and would be culled. Copy the icon's layer + sorting.
                var iconRenderer = go.GetComponentInChildren<Renderer>();
                ring.layer = iconRenderer != null ? iconRenderer.gameObject.layer : go.layer;

                var sr = ring.AddComponent<SpriteRenderer>();
                sr.sprite = GetRingSprite();
                if (iconRenderer != null)
                {
                    sr.sortingLayerID = iconRenderer.sortingLayerID;
                    sr.sortingOrder = iconRenderer.sortingOrder - 1;
                }

                var ringUI = ring.AddComponent<LogicRingUI>();
                ringUI.Icon = icon;
                ringUI.Renderer = sr;
                ringUI.Refresh();
            }
            catch (System.Exception ex)
            {
                ModLogger.Error($"[LogicRing] {ex}");
            }
        }

        /// <summary>
        /// Builds (once) a soft anti-aliased white ring sprite at runtime so we don't ship an
        /// asset. White so it can be tinted to any state color.
        /// </summary>
        private static Sprite GetRingSprite()
        {
            if (_ringSprite != null)
                return _ringSprite;

            const int size = 256;
            var tex = new Texture2D(size, size, TextureFormat.ARGB32, true);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Trilinear;
            tex.anisoLevel = 2;

            var pixels = new Color[size * size];
            float center = (size - 1) / 2f;
            float outer = center - 2f;     // leave a little padding so the outer edge stays smooth
            float thickness = 16f;
            float inner = outer - thickness;
            float aa = 2.0f;               // antialiasing width in pixels

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);

                    // Smoothstep in on the inner edge, out on the outer edge.
                    float outerA = Mathf.Clamp01((outer - d) / aa);
                    float innerA = Mathf.Clamp01((d - inner) / aa);
                    float a = Mathf.Min(outerA, innerA);

                    pixels[y * size + x] = new Color(1f, 1f, 1f, a);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true);

            _ringSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            return _ringSprite;
        }
    }

    /// <summary>
    /// Keeps a ring's color in sync with its icon's logic state. Re-evaluates whenever the
    /// GameObject is enabled (i.e. each time the map is shown), so rings recolor after the
    /// player gains new abilities/items.
    /// </summary>
    internal class LogicRingUI : MonoBehaviour
    {
        public RuntimeWorldMapIcon Icon;
        public SpriteRenderer Renderer;

        private static readonly Color Green = new Color(0.30f, 1.00f, 0.40f, 0.50f);
        private static readonly Color Yellow = new Color(1.00f, 0.85f, 0.15f, 0.50f);
        private static readonly Color Red = new Color(1.00f, 0.30f, 0.26f, 0.50f);
        private static readonly Color Grey = new Color(0.60f, 0.60f, 0.60f, 0.45f);

        void OnEnable()
        {
            Refresh();
        }

        public void Refresh()
        {
            if (Icon == null || Renderer == null)
                return;

            // Rings only appear in the dedicated Rings display mode. In every other icon-visibility
            // mode we keep the classic look (no rings), even if a ring object already exists from a
            // previous mode. Re-evaluated here because Refresh runs each time the map opens.
            if (MaptrackerSettings.IconVisibility != IconVisibilityEnum.Rings)
            {
                Renderer.enabled = false;
                return;
            }

            switch (LogicManager.GetLogicState(Icon))
            {
                case IconLogicState.InLogic:
                    Renderer.enabled = true;
                    Renderer.color = Green;
                    break;
                case IconLogicState.PossibleAtHarder:
                    Renderer.enabled = true;
                    Renderer.color = Yellow;
                    break;
                case IconLogicState.OutOfLogic:
                    Renderer.enabled = true;
                    Renderer.color = Red;
                    break;
                case IconLogicState.Collected:
                    Renderer.enabled = true;
                    Renderer.color = Grey;
                    break;
                default: // NotACheck
                    Renderer.enabled = false;
                    break;
            }
        }
    }
}

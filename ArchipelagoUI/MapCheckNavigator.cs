using Game;
using HarmonyLib;
using OriBFArchipelago.Core;
using OriBFArchipelago.MapTracker.Logic;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using CoreInput = Core.Input;

namespace OriBFArchipelago.ArchipelagoUI
{
    /// <summary>
    /// Lets the player flip through the in-logic checks on the world map with the
    /// bumpers: right bumper = next, left bumper = previous. Each step centers the map
    /// on that check (which, on a controller, also pops up its hover info panel because
    /// the cursor is pinned to screen-center).
    ///
    /// The ordered list is built once when the map is opened so that stepping forward
    /// then backward always lands on the same icon. Order: nearest area to the player
    /// first, and within each area the checks nearest the player first.
    /// </summary>
    internal class MapCheckNavigator : MonoBehaviour
    {
        private GameMapUI gameMapUI;
        private readonly List<RuntimeWorldMapIcon> orderedIcons = new List<RuntimeWorldMapIcon>();
        private int index = -1;
        private bool wasOnNormalMap;

        // Private fields on AreaMapNavigation that drive the vanilla objective-focus
        // animation (HandleObjectiveFocus lerps ScrollPosition from -> to while
        // m_focusTime counts down from 1 to 0).
        private static readonly FieldInfo FocusTimeField = AccessTools.Field(typeof(AreaMapNavigation), "m_focusTime");
        private static readonly FieldInfo FromPositionField = AccessTools.Field(typeof(AreaMapNavigation), "m_fromPosition");
        private static readonly FieldInfo ToPositionField = AccessTools.Field(typeof(AreaMapNavigation), "m_toPosition");

        private void Awake()
        {
            gameMapUI = GetComponent<GameMapUI>();
        }

        /// <summary>True while on the normal (non-objective, non-teleporter) world map.</summary>
        private bool OnNormalMap()
        {
            return gameMapUI != null
                && gameMapUI.IsVisible
                && !gameMapUI.ShowingObjective
                && !gameMapUI.ShowingTeleporters;
        }

        private void Update()
        {
            bool onMap = OnNormalMap();

            // Rebuild the list the next time it is needed whenever the map is (re)opened,
            // so the order is stable for the duration it stays open.
            if (onMap && !wasOnNormalMap)
            {
                orderedIcons.Clear();
                index = -1;
            }
            wasOnNormalMap = onMap;

            if (!onMap)
                return;

            bool next = CoreInput.RightShoulder.OnPressed && !CoreInput.RightShoulder.Used;
            bool prev = CoreInput.LeftShoulder.OnPressed && !CoreInput.LeftShoulder.Used;
            if (!next && !prev)
                return;

            // Consume the press so the base game doesn't also react to it.
            if (next) CoreInput.RightShoulder.Used = true;
            if (prev) CoreInput.LeftShoulder.Used = true;

            EnsureList();
            if (orderedIcons.Count == 0)
                return;

            int n = orderedIcons.Count;
            int delta = next ? 1 : -1;
            index = ((index + delta) % n + n) % n;

            RuntimeWorldMapIcon icon = orderedIcons[index];
            if (icon != null && AreaMapUI.Instance != null && AreaMapUI.Instance.Navigation != null)
                FocusOn(AreaMapUI.Instance.Navigation, icon.Position);
        }

        /// <summary>
        /// Smoothly pans the map to <paramref name="target"/> the same way the vanilla
        /// game moves between objectives: prime the from/to positions and reset the
        /// focus timer, then the game's per-frame HandleObjectiveFocus eases us there.
        /// </summary>
        private static void FocusOn(AreaMapNavigation nav, Vector2 target)
        {
            if (FocusTimeField == null || FromPositionField == null || ToPositionField == null)
            {
                nav.CenterMapOnWorldPosition(target); // fallback: instant snap
                return;
            }

            FromPositionField.SetValue(nav, nav.ScrollPosition);
            ToPositionField.SetValue(nav, target);
            FocusTimeField.SetValue(nav, 1f);

            if (nav.FocusSound != null)
                nav.FocusSound.Play();
        }

        private void EnsureList()
        {
            if (orderedIcons.Count == 0)
                BuildOrderedList();
        }

        /// <summary>
        /// Collects every in-logic check icon currently on the map and orders them:
        /// areas nearest the player first, and within each area the nearest checks first.
        /// </summary>
        private void BuildOrderedList()
        {
            orderedIcons.Clear();
            index = -1;

            Vector2 player = Characters.Sein != null ? (Vector2)Characters.Sein.Position : Vector2.zero;

            Dictionary<WorldArea, List<RuntimeWorldMapIcon>> byArea = new Dictionary<WorldArea, List<RuntimeWorldMapIcon>>();
            List<RuntimeWorldMapIcon> unknownArea = new List<RuntimeWorldMapIcon>();

            foreach (IconHoverEffectUI hover in FindObjectsOfType<IconHoverEffectUI>())
            {
                RuntimeWorldMapIcon icon = hover != null ? hover.IconType : null;
                if (icon == null)
                    continue;

                if (LogicManager.GetLogicState(icon) != IconLogicState.InLogic)
                    continue;

                Location location = LogicManager.Get(icon);
                if (location != null)
                {
                    if (!byArea.TryGetValue(location.Area, out List<RuntimeWorldMapIcon> list))
                    {
                        list = new List<RuntimeWorldMapIcon>();
                        byArea[location.Area] = list;
                    }
                    list.Add(icon);
                }
                else
                {
                    unknownArea.Add(icon);
                }
            }

            // Areas ordered by the distance from the player to their nearest check.
            List<WorldArea> areas = new List<WorldArea>(byArea.Keys);
            areas.Sort((a, b) =>
            {
                int cmp = NearestDistance(byArea[a], player).CompareTo(NearestDistance(byArea[b], player));
                return cmp != 0 ? cmp : ((int)a).CompareTo((int)b);
            });

            foreach (WorldArea area in areas)
            {
                List<RuntimeWorldMapIcon> list = byArea[area];
                SortByDistance(list, player);
                orderedIcons.AddRange(list);
            }

            // Icons with no known area (should be rare) go last, still distance-ordered.
            SortByDistance(unknownArea, player);
            orderedIcons.AddRange(unknownArea);
        }

        private static float NearestDistance(List<RuntimeWorldMapIcon> icons, Vector2 player)
        {
            float best = float.MaxValue;
            foreach (RuntimeWorldMapIcon icon in icons)
            {
                float d = Vector2.Distance(player, icon.Position);
                if (d < best)
                    best = d;
            }
            return best;
        }

        private static void SortByDistance(List<RuntimeWorldMapIcon> icons, Vector2 player)
        {
            icons.Sort((a, b) =>
            {
                int cmp = Vector2.Distance(player, a.Position).CompareTo(Vector2.Distance(player, b.Position));
                if (cmp != 0)
                    return cmp;

                // Stable tie-break on the icon guid so the order is fully deterministic.
                return string.CompareOrdinal(a.Guid?.ToString(), b.Guid?.ToString());
            });
        }
    }
}

using OriBFArchipelago.Core;
using OriBFArchipelago.MapTracker.Core;
using OriBFArchipelago.MapTracker.Logic;
using System;
using System.Collections.Generic;
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

        // Colors for per-requirement highlighting in the out-of-logic list
        private const string MetColor = "#7CFC00";   // have it
        private const string UnmetColor = "#FF5555"; // missing it
        private const int TokensPerLine = 5;         // wrap the collective list to bound width

        // Hover-change cache: the requirement display is computed once per hovered icon and
        // rendered from here every frame, so OnGUI (which runs ~twice per frame) never re-runs
        // the logic solver. Invalidated when a menu screen (re)opens - see ShowMenuScreenPatch.
        // Note: a bool flag is used instead of comparing _cachedGuid against null, because
        // MoonGuid's overloaded == throws on a null operand.
        private static bool _hasCache;
        private static MoonGuid _cachedGuid;
        private static bool _cachedInLogic;
        private static List<string> _cachedLines = new List<string>();

        // When the item is out of logic at the current difficulty but reachable at a harder one,
        // this holds that difficulty's name and its (colored) requirement lines.
        private static string _cachedHarderDifficulty;
        private static List<string> _cachedHarderLines = new List<string>();

        private GUIStyle textStyle;

        /// <summary>
        /// Drop the cached logic display so it is recomputed on the next hover. Called when a
        /// menu screen opens (inventory/options may have changed since the last map view).
        /// </summary>
        public static void InvalidateCache()
        {
            _hasCache = false;
        }

        private void Awake()
        {
            InitStyle();
        }

        private void InitStyle()
        {
            textStyle = new GUIStyle();
            textStyle.wordWrap = false;
            textStyle.richText = true;
            textStyle.fontStyle = FontStyle.Bold;
            textStyle.normal.textColor = new Color(1f, 1f, 1f);
            textStyle.fontSize = (int)(10 * (Screen.width / baseScreenSize.x));
        }

        private void OnGUI()
        {
            try
            {
                if (!(RandomizerSettings.ShowSettings && ShowMapTrackerSettings && Icon != null && MaptrackerSettings.EnableIconInfocUI))
                    return;

                if (textStyle == null)
                    InitStyle();

                // Gather all data BEFORE any GUILayout call, so a thrown exception can never
                // leave the IMGUI layout with an unbalanced Begin/End and break the panel.
                var trackerItem = LogicManager.Get(Icon);
                if (!_hasCache || !_cachedGuid.Equals(Icon.Guid))
                    RecomputeLogicCache(trackerItem);

                // A collected location is never "in logic" (it drops out once checked), so show
                // that reason explicitly instead of a bare "no" next to satisfied requirements.
                LocationStatus? status = trackerItem != null ? RandomizerManager.Receiver?.GetLocationStatus(trackerItem.Name) : null;
                bool collected = status == LocationStatus.Checked || status == LocationStatus.CheckedNotSaved;

                // Set the depth before any GUI calls
                GUI.depth = 0;
                textStyle.fontSize = (int)(10 * (Screen.width / baseScreenSize.x));
                GUI.backgroundColor = Color.black;

                GUILayout.BeginArea(new Rect(Screen.width * 4 / 5, 5, Screen.width / 3, Screen.height / 5));
                GUILayout.BeginVertical();

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
                GUILayout.Label(collected ? "In Logic: collected" : $"In Logic: {(_cachedInLogic ? "yes" : "no")}", textStyle, GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();

                if (trackerItem != null)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"Status: {status}", textStyle, GUILayout.ExpandWidth(false));
                    GUILayout.EndHorizontal();
                }

                if (trackerItem != null && _cachedLines.Count > 0)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Logic:", textStyle, GUILayout.ExpandWidth(false));
                    GUILayout.EndHorizontal();

                    foreach (var line in _cachedLines)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(line, textStyle, GUILayout.ExpandWidth(false));
                        GUILayout.EndHorizontal();
                    }
                }

                // If it's not reachable now but is on a harder difficulty, show that ruleset's
                // requirements so the player knows what would unlock it and at what difficulty.
                if (trackerItem != null && !collected && !_cachedInLogic && _cachedHarderDifficulty != null && _cachedHarderLines.Count > 0)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"Possible on {_cachedHarderDifficulty}:", textStyle, GUILayout.ExpandWidth(false));
                    GUILayout.EndHorizontal();

                    foreach (var line in _cachedHarderLines)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(line, textStyle, GUILayout.ExpandWidth(false));
                        GUILayout.EndHorizontal();
                    }
                }

                GUILayout.EndVertical();
                GUILayout.EndArea();
            }
            catch (System.Exception ex)
            {
                ModLogger.Error($"{ex}");
            }
        }

        /// <summary>
        /// Builds the cached in-logic flag and the formatted requirement line(s) for the hovered
        /// icon. Shows the collective requirements gathered along the least-blocked route from the
        /// logical start to the item (plus its own access edge), each token colored met (green) /
        /// unmet (red) - so the red tokens are exactly what is blocking access. Self-contained:
        /// any failure logs and leaves safe defaults so the panel still renders.
        /// </summary>
        private void RecomputeLogicCache(Location trackerItem)
        {
            _hasCache = true;
            _cachedGuid = Icon.Guid;
            _cachedInLogic = false;
            _cachedLines = new List<string>();
            _cachedHarderDifficulty = null;
            _cachedHarderLines = new List<string>();

            try
            {
                _cachedInLogic = LogicManager.IsInLogic(Icon);

                if (trackerItem == null)
                    return;

                var options = RandomizerManager.Options;
                var inventory = RandomizerManager.Receiver?.GetAllItems();
                if (options == null || inventory == null)
                    return;

                var checker = LogicManager.LogicChecker;
                var tokens = checker.GetCollectiveRequirements(trackerItem.Name, options.LogicDifficulty, inventory, options);

                if (tokens.Count == 0)
                    _cachedLines.Add("Free");
                else
                    _cachedLines = FormatTokenLines(tokens, checker, inventory, options);

                // Out of logic now: find the easiest harder difficulty that makes it reachable and
                // show what it would take there.
                if (!_cachedInLogic)
                    ComputeHarderDifficulty(trackerItem, checker, inventory, options);
            }
            catch (Exception ex)
            {
                ModLogger.Error($"RecomputeLogicCache: {ex}");
            }
        }

        /// <summary>
        /// Finds the lowest difficulty above the current one at which the pickup becomes accessible,
        /// and caches that difficulty's name and (colored) collective requirements.
        /// </summary>
        private void ComputeHarderDifficulty(Location trackerItem, LogicChecker checker, Dictionary<string, int> inventory, RandomizerOptions options)
        {
            foreach (DifficultyOptions diff in Enum.GetValues(typeof(DifficultyOptions)))
            {
                if (diff <= options.LogicDifficulty)
                    continue;

                if (!checker.IsPickupAccessible(trackerItem.Name, diff, inventory, options))
                    continue;

                _cachedHarderDifficulty = diff.ToString();
                var harderTokens = checker.GetCollectiveRequirements(trackerItem.Name, diff, inventory, options);
                _cachedHarderLines = harderTokens.Count == 0
                    ? new List<string> { "Free" }
                    : FormatTokenLines(harderTokens, checker, inventory, options);
                return;
            }
        }

        /// <summary>
        /// Colors each requirement token met (green) / unmet (red) and wraps the list across lines
        /// so a long collective requirement list stays inside the panel.
        /// </summary>
        private List<string> FormatTokenLines(List<string> tokens, LogicChecker checker, Dictionary<string, int> inventory, RandomizerOptions options)
        {
            var lines = new List<string>();
            var parts = new List<string>();
            foreach (var token in tokens)
            {
                bool met = checker.IsRequirementSatisfied(token, inventory, options);
                string color = met ? MetColor : UnmetColor;
                parts.Add($"<color={color}>{LogicRequirementFormatter.FormatToken(token)}</color>");

                if (parts.Count == TokensPerLine)
                {
                    lines.Add(string.Join(", ", parts.ToArray()));
                    parts.Clear();
                }
            }

            if (parts.Count > 0)
                lines.Add(string.Join(", ", parts.ToArray()));

            return lines;
        }
    }
}

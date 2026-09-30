using OriBFArchipelago.Core;
using OriBFArchipelago.MapTracker.Core;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace OriBFArchipelago.ArchipelagoUI
{
    /// <summary>
    /// World-map panel that lists every Archipelago setting selected in the yaml
    /// (the parsed <see cref="RandomizerOptions"/>). Toggled with the X button (both
    /// info panels together) or F6; shown in the bottom-right.
    /// </summary>
    internal class ApSettingsPanel : MapPanel
    {
        public override KeyCode ToggleKey => KeyCode.F6;
        public override bool AnchorBottomRight => true;
        public override string Title => "AP Settings";

        public override List<string> GetLines()
        {
            List<string> lines = new List<string>();

            object options = RandomizerManager.Options;
            if (options == null)
            {
                lines.Add("No settings loaded.");
                return lines;
            }

            try
            {
                foreach (PropertyInfo prop in options.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    // Skip the derived goal-location list; it isn't a yaml setting.
                    if (prop.Name == "GoalLocations" || !prop.CanRead)
                        continue;

                    object value = prop.GetValue(options, null);
                    lines.Add($"{prop.Name}: {FormatValue(value)}");
                }
            }
            catch (System.Exception ex)
            {
                ModLogger.Error(ex.ToString());
            }

            return lines;
        }

        private static string FormatValue(object value)
        {
            if (value == null)
                return "-";

            if (value is System.Array array)
            {
                List<string> parts = new List<string>();
                foreach (object item in array)
                    parts.Add(item?.ToString() ?? "-");
                return parts.Count > 0 ? string.Join(", ", parts.ToArray()) : "(none)";
            }

            return value.ToString();
        }
    }
}

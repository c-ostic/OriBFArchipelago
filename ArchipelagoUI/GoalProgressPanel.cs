using OriBFArchipelago.Core;
using System.Collections.Generic;
using UnityEngine;

namespace OriBFArchipelago.ArchipelagoUI
{
    /// <summary>
    /// World-map panel that lists progress toward each active goal. Toggled with the
    /// X button (both info panels together) or F5; shown in the top-right. A more
    /// discoverable alternative to the GoalProgress keybind (Alt+G), which still works.
    /// </summary>
    internal class GoalProgressPanel : MapPanel
    {
        public override KeyCode ToggleKey => KeyCode.F5;
        public override bool AnchorBottomRight => false;
        public override string Title => "Goal Progress";

        public override List<string> GetLines()
        {
            try
            {
                return RandomizerManager.Connection?.GetGoalProgressLines() ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }
    }
}

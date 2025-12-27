using HarmonyLib;
using OriBFArchipelago.Core;
using OriBFArchipelago.MapTracker.Core;
using System.Collections.Generic;
using System.Linq;

namespace OriBFArchipelago.Patches.RemoveAnimations
{
    [HarmonyPatch(typeof(MapStone), nameof(MapStone.Start))]
    public class MapStoneStartPatch
    {
        [HarmonyPostfix]
        public static void Postfix(MapStone __instance)
        {
            if (MaptrackerSettings.MapVisibility == MapVisibilityEnum.Visible && RandomizerSettings.SkipCutscenes)
            {
                if (__instance.OnOpenedAction == null)
                    return;
                ActionSequence actionSequence = __instance.OnOpenedAction as ActionSequence;
                if (actionSequence != null)
                {
                    // Filter to keep only InstantiateAction actions
                    List<ActionMethod> keptActions = new List<ActionMethod>();
                    string[] actionsToKeep = ["UnhighlightMapStoneAction", "InstantiateAction", "LetterboxAction", "BaseAnimatorAction"];

                    foreach (ActionMethod action in actionSequence.Actions)
                    {
                        if (actionsToKeep.Contains(action.GetType().Name))
                            keptActions.Add(action);
                    }

                    // Clear and replace the actions list
                    actionSequence.Actions.Clear();
                    foreach (var action in keptActions)
                        actionSequence.Actions.Add(action);

                }
            }
        }
    }
}
using HarmonyLib;
using OriBFArchipelago.Core;

namespace OriBFArchipelago.Patches
{
    [HarmonyPatch(typeof(SeinDoorHandler), nameof(SeinDoorHandler.EnterIntoDoor))]
    internal class ForbidDoorPatch
    {        
        // Returning false = cannot travel through door
        private static bool Prefix(Door door)
        {
            // Don't let anyone through to the element of warmth
            if (door.name == "mountHoruExitDoor")
            {
                var isGoalCompleted = RandomizerManager.Connection.IsGoalComplete();
                if (isGoalCompleted && RandomizerSettings.SkipFinalEscape)
                {
                    RandomizerMessager.instance.AddMessage("Goal completed");
                    RandomizerManager.Connection.SendCompletion();
                    return false;
                }
                return isGoalCompleted;
            }
            return true;
        }
    }
}

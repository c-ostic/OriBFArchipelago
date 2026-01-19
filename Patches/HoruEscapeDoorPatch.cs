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
            if (door.name == "mountHoruExitDoor")
            {
                var isGoalCompleted = RandomizerManager.Connection.IsGoalComplete();
                if (isGoalCompleted && !RandomizerManager.Options.RequireFinalEscape)
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

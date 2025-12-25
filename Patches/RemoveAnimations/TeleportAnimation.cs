using HarmonyLib;

namespace OriBFArchipelago.Patches.RemoveAnimations
{
    [HarmonyPatch(typeof(TeleporterController), nameof(TeleporterController.BeginTeleportation))]
    internal class TeleportAnimation
    {
        public static void Postfix()
        {
            var startTimeField = AccessTools.Field(typeof(TeleporterController), "m_startTime");

            if (startTimeField == null)
                return;

            //Update startTime to mimic time passing
            var startTime = (float)startTimeField.GetValue(TeleporterController.Instance);
            startTimeField.SetValue(TeleporterController.Instance, startTime - 12f);
        }
    }
}
using HarmonyLib;
using OriBFArchipelago.Core;
using OriBFArchipelago.MapTracker.Core;

namespace OriBFArchipelago.Patches
{
    [HarmonyPatch(typeof(SaveSlotsUI))]
    public class SaveSlotUIPatch
    {
        public SaveSlotUIPatch()
        {
            ModLogger.Debug("Patching saveslotsuit");
        }

        [HarmonyPatch(nameof(SaveSlotsUI.SetVisible))]
        [HarmonyPrefix]
        static bool SetVisiblePrefix(bool visible)
        {
            if (!RandomizerSettings.InGame)
            {
                RandomizerSettings.InSaveSelect = visible;
                RandomizerManager.instance.InspectSaveSlot(0);
            }
            return true; // Allow the original method to run
        }
    }
}

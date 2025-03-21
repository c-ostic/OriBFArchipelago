using HarmonyLib;
using OriBFArchipelago.MapTracker.Core;
using OriBFArchipelago.MapTracker.UI;
using OriModding.BF.UiLib.Menu;

namespace OriBFArchipelago.Patches
{
    [HarmonyPatch(typeof(OptionsScreen), "Awake")]
    public class OptionsScreenPatch
    {
        static OptionsScreenPatch()
        {
            ModLogger.Debug($"Patching {nameof(OptionsScreenPatch)}");
        }
        static void Postfix(OptionsScreen __instance)
        {
            CustomMenuManager.RegisterOptionsScreen<ModOptionsScreen>("Tracker options", 100);
        }
    }
}

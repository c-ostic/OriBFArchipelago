using HarmonyLib;
using OriBFArchipelago.ArchipelagoUI;
using OriBFArchipelago.MapTracker.Core;
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
            CustomMenuManager.RegisterOptionsScreen<MapTrackerOptionsScreen>("Tracker options", 100);
            CustomMenuManager.RegisterOptionsScreen<ArchipelagoOptionsScreen>("Archipelago options", 150);
            
        }
    }
}

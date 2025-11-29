using Core;
using HarmonyLib;
using OriBFArchipelago.MapTracker.Core;

namespace OriBFArchipelago.Patches
{
    [HarmonyPatch(typeof(CleverMenuItemSelectionManager), nameof(CleverMenuItemSelectionManager.FixedUpdate))]
    public static class CleverMenuItemSelectionManagerPatches
    {
        [HarmonyPostfix]
        static void FixedUpdate_Postfix(CleverMenuItemSelectionManager __instance)
        {
            // Only run if the menu is visible and active
            if (!__instance.IsVisible || __instance.IsSuspended || !GameController.IsFocused)
            {
                return;
            }

            // Don't interfere if something is already being performed or locked
            if (__instance.IsLocked)
            {
                return;
            }

            if (__instance.CurrentMenuItem != null && __instance.CurrentMenuItem.IsPerforming())
            {
                return;
            }

            // YOUR CUSTOM BUTTON HANDLING HERE
            HandleCustomButtons(__instance);
        }

        private static void HandleCustomButtons(CleverMenuItemSelectionManager manager)
        {
            if (Game.UI.Menu.CurrentScreen != MenuScreenManager.Screens.Inventory)
                return;

            if (Input.LeftShoulder.OnPressed && !Input.LeftShoulder.Used)
            {
                Input.LeftShoulder.Used = true;
                OnCustomAction1(manager);
            }

            if (Input.RightShoulder.OnPressed && !Input.RightShoulder.Used)
            {
                Input.RightShoulder.Used = true;
                OnCustomAction2(manager);
            }
        }

        private static void OnCustomAction1(CleverMenuItemSelectionManager manager)
        {
            TeleporterManager.TeleportToStart();
        }

        private static void OnCustomAction2(CleverMenuItemSelectionManager manager)
        {
            TeleporterManager.TeleportToLastTeleporter();
        }
    }
}
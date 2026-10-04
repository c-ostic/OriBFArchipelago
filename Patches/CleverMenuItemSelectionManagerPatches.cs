using HarmonyLib;
using OriBFArchipelago.Core;
using OriBFArchipelago.Helper;
using OriBFArchipelago.MapTracker.Core;
using UnityEngine;
using CoreInput = Core.Input;

namespace OriBFArchipelago.Patches
{
    [HarmonyPatch(typeof(CleverMenuItemSelectionManager), nameof(CleverMenuItemSelectionManager.FixedUpdate))]
    public static class CleverMenuItemSelectionManagerPatches
    {
        private static RandomizerMessageBox _confirmationBox = null;

        [HarmonyPostfix]
        static void FixedUpdate_Postfix(CleverMenuItemSelectionManager __instance)
        {
            if (_confirmationBox != null && _confirmationBox.IsActive && !__instance.IsVisible)
            {
                CancelTeleport(__instance);
                return;
            }

            if (!__instance.IsVisible || __instance.IsSuspended || !GameController.IsFocused)
            {
                return;
            }

            if (_confirmationBox != null && _confirmationBox.IsActive)
            {
                if (CoreInput.Start.OnPressed)
                {
                    CancelTeleport(__instance);
                }

                if (CoreInput.Cancel.OnPressed)
                {
                    CoreInput.Cancel.Used = true;
                }

                if (CoreInput.ActionButtonA.OnPressed)
                {
                    CoreInput.ActionButtonA.Used = true;
                }

                return;
            }

            if (__instance.IsLocked)
            {
                return;
            }

            if (__instance.CurrentMenuItem != null && __instance.CurrentMenuItem.IsPerforming())
            {
                return;
            }

            HandleCustomButtons(__instance);
        }

        private static void HandleCustomButtons(CleverMenuItemSelectionManager manager)
        {
            if (Game.UI.Menu.CurrentScreen != MenuScreenManager.Screens.Inventory)
                return;

            if (CoreInput.LeftShoulder.OnPressed && !CoreInput.LeftShoulder.Used)
            {
                CoreInput.LeftShoulder.Used = true;
                ShowTeleportConfirmation(manager);
            }

            if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.F3))
            {
                ShowTeleportConfirmation(manager);
            }

            if (CoreInput.RightShoulder.OnPressed && !CoreInput.RightShoulder.Used)
            {
                CoreInput.RightShoulder.Used = true;
                OpenTeleportMap();
            }

            if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.F4))
            {
                OpenTeleportMap();
            }
        }

        private static void ShowTeleportConfirmation(CleverMenuItemSelectionManager manager)
        {
            string message = "Teleport to start?";

            Vector3 position = manager.transform.position;
            position.y += 2.0f;

            _confirmationBox = new RandomizerMessageBox(
                message,
                onConfirm: () => ExecuteTeleport(manager),
                onCancel: () => CancelTeleport(manager),
                position: position
            );
            _confirmationBox.Show();

            manager.IsSuspended = true;
        }

        private static void ExecuteTeleport(CleverMenuItemSelectionManager manager)
        {
            manager.IsSuspended = false;
            TeleporterManager.TeleportToStart();
        }

        /**
         * Closes the inventory screen and opens the teleporter map
         * (the same map shown by the OpenTeleport keybind).
         */
        private static void OpenTeleportMap()
        {
            Game.UI.Menu.HideMenuScreen(true);
            RandomizerController.Instance?.ShowTeleportMenu();
        }

        private static void CancelTeleport(CleverMenuItemSelectionManager manager)
        {
            manager.IsSuspended = false;
            _confirmationBox?.Destroy();
            _confirmationBox = null;
        }
    }
}
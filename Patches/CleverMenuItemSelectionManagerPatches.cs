using HarmonyLib;
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
                ShowTeleportConfirmation(TeleportAction.ToStart, manager);
            }

            if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.F3))
            {
                ShowTeleportConfirmation(TeleportAction.ToStart, manager);
            }

            if (TeleporterManager.GetLastTeleporter() != null)
            {
                if (CoreInput.RightShoulder.OnPressed && !CoreInput.RightShoulder.Used)
                {
                    CoreInput.RightShoulder.Used = true;
                    ShowTeleportConfirmation(TeleportAction.ToLastTeleporter, manager);
                }

                if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.F4))
                {
                    ShowTeleportConfirmation(TeleportAction.ToLastTeleporter, manager);
                }
            }
        }

        private static void ShowTeleportConfirmation(TeleportAction action, CleverMenuItemSelectionManager manager)
        {
            string message = action == TeleportAction.ToStart
                ? "{confirm}Teleport to start?\n{cancel}Cancel?"
                : $"{{confirm}}Teleport to {TeleporterManager.GetLastTeleporter().FriendlyName}?\n{{cancel}}Cancel?";

            Vector3 position = manager.transform.position;
            position.y += 2.0f;

            _confirmationBox = new RandomizerMessageBox(
                message,
                onConfirm: () => ExecuteTeleport(action, manager),
                onCancel: () => CancelTeleport(manager),
                position: position
            );
            _confirmationBox.Show();

            manager.IsSuspended = true;
        }

        private static void ExecuteTeleport(TeleportAction action, CleverMenuItemSelectionManager manager)
        {
            manager.IsSuspended = false;

            switch (action)
            {
                case TeleportAction.ToStart:
                    TeleporterManager.TeleportToStart();
                    break;
                case TeleportAction.ToLastTeleporter:
                    TeleporterManager.TeleportToLastTeleporter();
                    break;
            }
        }

        private static void CancelTeleport(CleverMenuItemSelectionManager manager)
        {
            manager.IsSuspended = false;
            _confirmationBox?.Destroy();
            _confirmationBox = null;
        }

        private enum TeleportAction
        {
            None = 0,
            ToStart = 1,
            ToLastTeleporter = 2
        }
    }
}
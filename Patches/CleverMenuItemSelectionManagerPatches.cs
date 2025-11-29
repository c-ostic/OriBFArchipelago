using Game;
using HarmonyLib;
using OriBFArchipelago.MapTracker.Core;
using UnityEngine;
using CoreInput = Core.Input;

namespace OriBFArchipelago.Patches
{
    [HarmonyPatch(typeof(CleverMenuItemSelectionManager), nameof(CleverMenuItemSelectionManager.FixedUpdate))]
    public static class CleverMenuItemSelectionManagerPatches
    {
        private static ConfirmOrCancel _confirmComponent = null;
        private const string keyboardConfirmButton = "<icon>D</>";
        private const string xboxConfirmButton = "<icon>e</>";
        private const string keyboardCancelButton = "<icon>y</>";
        private const string xboxCancelButton = "<icon>f</>";
        [HarmonyPostfix]
        static void FixedUpdate_Postfix(CleverMenuItemSelectionManager __instance)
        {
            if (_confirmComponent != null && _confirmComponent.enabled && !__instance.IsVisible)
            {
                CancelTeleport(__instance);
                return;
            }

            if (!__instance.IsVisible || __instance.IsSuspended || !GameController.IsFocused)
            {
                return;
            }

            if (_confirmComponent != null && _confirmComponent.enabled)
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

        private static void ShowTeleportConfirmation(TeleportAction action, CleverMenuItemSelectionManager manager)
        {

            if (_confirmComponent != null)
            {
                Object.Destroy(_confirmComponent.gameObject);
                _confirmComponent = null;
            }

            ConfirmOrCancel prefab = FindConfirmationPrefab();

            if (prefab == null)
            {
                ExecuteTeleport(action, manager);
                return;
            }

            _confirmComponent = Object.Instantiate(prefab);

            Vector3 position = manager.transform.position;
            position.y += 2.0f;
            _confirmComponent.transform.position = position;

            string message = action == TeleportAction.ToStart
                ? $@"{GetConfirmIcon()}Teleport to start?
{GetCancelIcon()}Cancel?"
                : $@"{GetConfirmIcon()}Teleport to {TeleporterManager.GetLastTeleporter().FriendlyName}?
{GetCancelIcon()}Cancel? ";
            UpdatePromptText(_confirmComponent.gameObject, message);

            _confirmComponent.OnConfirm += () => ExecuteTeleport(action, manager);
            _confirmComponent.OnCancel = () => CancelTeleport(manager);
            _confirmComponent.enabled = true;
            manager.IsSuspended = true;
        }

        private static string GetConfirmIcon()
        {
            return PlayerInput.Instance.WasKeyboardUsedLast ? keyboardConfirmButton : xboxConfirmButton;
        }
        private static string GetCancelIcon()
        {
            return PlayerInput.Instance.WasKeyboardUsedLast ? keyboardCancelButton : xboxCancelButton;
        }

        private static void UpdatePromptText(GameObject promptObject, string newText)
        {
            MessageBox[] messageBoxes = promptObject.GetComponentsInChildren<MessageBox>();

            foreach (MessageBox messageBox in messageBoxes)
            {
                messageBox.OverrideText = newText;
                messageBox.SetMessage(new MessageDescriptor(newText));
            }
        }

        private static ConfirmOrCancel FindConfirmationPrefab()
        {
            SaveSlotsUI saveSlotsUI = SaveSlotsUI.Instance;
            if (saveSlotsUI != null && saveSlotsUI.DeleteQuestion != null)
            {
                return saveSlotsUI.DeleteQuestion;
            }

            return null;
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

            if (_confirmComponent != null)
            {
                Object.Destroy(_confirmComponent.gameObject);
                _confirmComponent = null;
            }
        }

        private static void CancelTeleport(CleverMenuItemSelectionManager manager)
        {
            manager.IsSuspended = false;
            if (_confirmComponent != null)
            {
                Object.Destroy(_confirmComponent.gameObject);
                _confirmComponent = null;

            }
        }

        private enum TeleportAction
        {
            None = 0,
            ToStart = 1,
            ToLastTeleporter = 2
        }
    }
}
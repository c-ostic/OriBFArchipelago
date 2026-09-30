using System;
using UnityEngine;

namespace OriBFArchipelago.Helper
{
    public class RandomizerMessageBox
    {
        private ConfirmOrCancel _popupComponent;
        private const string KeyboardConfirmButton = "<icon>D</>";
        private const string XboxConfirmButton = "<icon>e</>";
        private const string KeyboardCancelButton = "<icon>y</>";
        private const string XboxCancelButton = "<icon>f</>";
        private const int MaximumTextLength = 400;
        public bool IsActive => _popupComponent != null && _popupComponent.enabled;

        private string _message;
        private Action _onConfirm;
        private Action _onCancel;
        private Vector3? _position;
        private string _confirmText;
        private string _cancelText;

        public RandomizerMessageBox(string message, Action onConfirm = null, string confirmText = null, Action onCancel = null, string cancelText = null, Vector3? position = null)
        {
            _message = message;
            _onConfirm = onConfirm;
            _onCancel = onCancel;
            _position = position;
            _confirmText = confirmText;
            _cancelText = cancelText;
        }

        public void Show()
        {
            Show(_message, _onConfirm, _onCancel, _position, _confirmText, _cancelText);
        }

        /**
         * Shows the native popup as a non-dismissable info box: just the message, no button
         * prompts, and confirm/cancel are swallowed so it stays up until Destroy() is called.
         * Used for the "connecting" overlay.
         */
        public void ShowInfo()
        {
            Destroy();

            ConfirmOrCancel prefab = FindPopupPrefab();
            if (prefab == null)
                return;

            _popupComponent = UnityEngine.Object.Instantiate(prefab);

            if (_position.HasValue)
                _popupComponent.transform.position = _position.Value;

            UpdatePromptText(_popupComponent.gameObject, _message);

            // Mark it so ConfirmOrCancelPatch blocks the A/Cancel buttons from closing it.
            // (ConfirmOrCancel.FixedUpdate disables itself on button press regardless of the
            // OnConfirm/OnCancel handlers, so swallowing the events alone isn't enough.)
            _popupComponent.gameObject.AddComponent<NonDismissablePopup>();

            _popupComponent.enabled = true;
        }

        /**
         * Updates the message text of an already shown popup.
         */
        public void SetMessage(string message)
        {
            _message = message;
            if (_popupComponent != null)
                UpdatePromptText(_popupComponent.gameObject, message);
        }

        private void Show(string message, Action onConfirm = null, Action onCancel = null, Vector3? position = null, string confirmText = null, string cancelText = null)
        {
            Destroy();

            ConfirmOrCancel prefab = FindPopupPrefab();
            if (prefab == null)
            {
                onConfirm?.Invoke();
                return;
            }

            _popupComponent = UnityEngine.Object.Instantiate(prefab);

            if (position.HasValue)
                _popupComponent.transform.position = position.Value;

            string formattedMessage = FormatMessage(message, onConfirm != null, onCancel != null, confirmText, cancelText);
            UpdatePromptText(_popupComponent.gameObject, formattedMessage);

            if (onConfirm == null && onCancel == null)
            {
                _popupComponent.OnConfirm += Destroy;
                _popupComponent.OnCancel = Destroy;
            }
            else
            {
                if (onConfirm != null)
                {
                    _popupComponent.OnConfirm += () =>
                    {
                        onConfirm.Invoke();
                        Destroy();
                    };
                }
                else
                    _popupComponent.OnConfirm += Destroy;

                if (onCancel != null)
                {
                    _popupComponent.OnCancel = () =>
                    {
                        onCancel.Invoke();
                        Destroy();
                    };
                }
                else
                    _popupComponent.OnCancel = Destroy;
            }

            _popupComponent.enabled = true;
        }

        public void Destroy()
        {
            if (_popupComponent != null)
            {
                UnityEngine.Object.Destroy(_popupComponent.gameObject);
                _popupComponent = null;
            }
        }

        private string FormatMessage(string message, bool hasConfirm, bool hasCancel, string confirmText, string cancelText)
        {
            if (string.IsNullOrEmpty(confirmText))
                confirmText = "Ok";

            if (string.IsNullOrEmpty(cancelText))
                cancelText = "Cancel";

            string formattedMessage = message;

            if (hasConfirm && hasCancel)
                formattedMessage = $"{message}\n{GetConfirmIcon()}{confirmText}\n{GetCancelIcon()}{cancelText}";
            else if (hasConfirm)
                formattedMessage = $"{message}\n{GetConfirmIcon()}{confirmText}";
            else if (hasCancel)
                formattedMessage = $"{message}\n{GetCancelIcon()}{cancelText}";
            else
                formattedMessage = $"{message}\n{GetConfirmIcon()}Close";

            var cleanedMessage = CleanupEmptyLines(formattedMessage);

            if (cleanedMessage.Length > MaximumTextLength)
                return $"This text is to long - {cleanedMessage}";

            return cleanedMessage;
        }

        private string CleanupEmptyLines(string text)
        {
            string[] lines = text.Split('\n');
            var nonEmptyLines = new System.Collections.Generic.List<string>();

            foreach (string line in lines)
            {
                if (!string.IsNullOrEmpty(line))
                {
                    nonEmptyLines.Add(line);
                }
            }

            return string.Join("\n", nonEmptyLines.ToArray());
        }

        private static string GetConfirmIcon()
        {
            return PlayerInput.Instance.WasKeyboardUsedLast ? KeyboardConfirmButton : XboxConfirmButton;
        }

        private static string GetCancelIcon()
        {
            return PlayerInput.Instance.WasKeyboardUsedLast ? KeyboardCancelButton : XboxCancelButton;
        }

        private static void UpdatePromptText(GameObject promptObject, string newText)
        {
            global::MessageBox[] messageBoxes = promptObject.GetComponentsInChildren<global::MessageBox>();

            foreach (global::MessageBox messageBox in messageBoxes)
            {
                messageBox.OverrideText = newText;
                messageBox.SetMessage(new MessageDescriptor(newText));
            }
        }

        private static ConfirmOrCancel FindPopupPrefab()
        {
            SaveSlotsUI saveSlotsUI = SaveSlotsUI.Instance;
            if (saveSlotsUI != null && saveSlotsUI.DeleteQuestion != null)
            {
                return saveSlotsUI.DeleteQuestion;
            }

            return null;
        }
    }

    /// <summary>
    /// Marker for popups that must not be closable by the player (e.g. the "connecting" overlay).
    /// <c>ConfirmOrCancelPatch</c> checks for this and blocks the confirm/cancel buttons.
    /// </summary>
    internal class NonDismissablePopup : MonoBehaviour { }
}
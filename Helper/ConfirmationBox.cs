using System;
using Game;
using UnityEngine;

namespace OriBFArchipelago.Helper
{
    public class ConfirmationBox
    {
        private ConfirmOrCancel _confirmComponent;
        private const string KeyboardConfirmButton = "<icon>D</>";
        private const string XboxConfirmButton = "<icon>e</>";
        private const string KeyboardCancelButton = "<icon>y</>";
        private const string XboxCancelButton = "<icon>f</>";

        public bool IsActive => _confirmComponent != null && _confirmComponent.enabled;

        public ConfirmationBox(string message, Action onConfirm, Action onCancel = null, Vector3? position = null)
        {
            Show(message, onConfirm, onCancel, position);
        }

        public void Show(string message, Action onConfirm, Action onCancel = null, Vector3? position = null)
        {
            // Clean up existing confirmation if any
            Destroy();

            ConfirmOrCancel prefab = FindConfirmationPrefab();
            if (prefab == null)
            {
                // No prefab available, execute immediately
                onConfirm?.Invoke();
                return;
            }

            _confirmComponent = UnityEngine.Object.Instantiate(prefab);

            // Set position
            if (position.HasValue)
            {
                _confirmComponent.transform.position = position.Value;
            }

            // Format message with appropriate button icons
            string formattedMessage = FormatMessage(message);
            UpdatePromptText(_confirmComponent.gameObject, formattedMessage);

            // Setup callbacks
            _confirmComponent.OnConfirm += () =>
            {
                onConfirm?.Invoke();
                Destroy();
            };

            _confirmComponent.OnCancel = () =>
            {
                onCancel?.Invoke();
                Destroy();
            };

            _confirmComponent.enabled = true;
        }

        public void Destroy()
        {
            if (_confirmComponent != null)
            {
                UnityEngine.Object.Destroy(_confirmComponent.gameObject);
                _confirmComponent = null;
            }
        }

        private string FormatMessage(string message)
        {
            string confirmIcon = GetConfirmIcon();
            string cancelIcon = GetCancelIcon();

            // If message doesn't contain placeholders, add default format
            if (!message.Contains("{confirm}") && !message.Contains("{cancel}"))
            {
                return $"{confirmIcon}{message}\n{cancelIcon}Cancel?";
            }

            // Replace placeholders
            return message
                .Replace("{confirm}", confirmIcon)
                .Replace("{cancel}", cancelIcon);
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
    }
}
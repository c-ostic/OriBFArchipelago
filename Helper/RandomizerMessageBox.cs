using BepInEx;
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

        public bool IsActive => _popupComponent != null && _popupComponent.enabled;

        private string _message;
        private Action _onConfirm;
        private Action _onCancel;
        private Vector3? _position;
        private string _confirmText;
        private string _cancelText;
        private float _scale;

        public RandomizerMessageBox(
            string message,
            Action onConfirm = null,
            string confirmText = null,
            Action onCancel = null,
            string cancelText = null,
            Vector3? position = null,                       
            float scale = 1.0f)
        {
            _message = message;
            _onConfirm = onConfirm;
            _onCancel = onCancel;
            _position = position;
            _confirmText = confirmText;
            _cancelText = cancelText;
            _scale = scale;
        }

        public void Show()
        {
            Show(_message, _onConfirm, _onCancel, _position, _confirmText, _cancelText, _scale);
        }

        private void Show(
            string message,
            Action onConfirm = null,
            Action onCancel = null,
            Vector3? position = null,
            string confirmText = null,
            string cancelText = null,
            float scale = 1.5f)
        {
            // Clean up existing popup if any
            Destroy();

            ConfirmOrCancel prefab = FindPopupPrefab();
            if (prefab == null)
            {
                // No prefab available, execute confirm action if provided, otherwise just return
                onConfirm?.Invoke();
                return;
            }

            _popupComponent = UnityEngine.Object.Instantiate(prefab);

            // Set position
            if (position.HasValue)
            {
                _popupComponent.transform.position = position.Value;
            }

            // Apply scale to the popup - scale box but keep text same size
            if (scale != 1.0f)
            {
                ScalePopupKeepTextSize(_popupComponent.gameObject, scale);
            }

            // Format message with appropriate button icons and texts
            string formattedMessage = FormatMessage(
                message,
                onConfirm != null,
                onCancel != null,
                confirmText,
                cancelText);
            UpdatePromptText(_popupComponent.gameObject, formattedMessage);

            // If both actions are null, any button press closes the popup
            if (onConfirm == null && onCancel == null)
            {
                _popupComponent.OnConfirm += Destroy;
                _popupComponent.OnCancel = Destroy;
            }
            else
            {
                // Setup callbacks
                if (onConfirm != null)
                {
                    _popupComponent.OnConfirm += () =>
                    {
                        onConfirm.Invoke();
                        Destroy();
                    };
                }
                else
                {
                    // If no confirm action, just close on confirm press
                    _popupComponent.OnConfirm += Destroy;
                }

                if (onCancel != null)
                {
                    _popupComponent.OnCancel = () =>
                    {
                        onCancel.Invoke();
                        Destroy();
                    };
                }
                else
                {
                    // If no cancel action, just close on cancel press
                    _popupComponent.OnCancel = Destroy;
                }
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

        private string FormatMessage(
            string message,
            bool hasConfirm,
            bool hasCancel,
            string confirmText,
            string cancelText)
        {
            // Set default texts if not provided
            if (string.IsNullOrEmpty(confirmText))
            {
                confirmText = "Confirm";
            }

            if (string.IsNullOrEmpty(cancelText))
            {
                cancelText = "Cancel";
            }

            // If message contains placeholders, use them
            if (message.Contains("{confirm}") || message.Contains("{cancel}"))
            {
                string result = message;

                if (hasConfirm)
                {
                    result = result.Replace("{confirm}", $"{GetConfirmIcon()}{confirmText}");
                }
                else
                {
                    // Remove confirm placeholder and its line if no confirm action
                    result = result.Replace("{confirm}", "");
                }

                if (hasCancel)
                {
                    result = result.Replace("{cancel}", $"{GetCancelIcon()}{cancelText}");
                }
                else
                {
                    // Remove cancel placeholder and its line if no cancel action
                    result = result.Replace("{cancel}", "");
                }

                return CleanupEmptyLines(result);
            }

            // If no placeholders, add buttons based on what's available
            string formattedMessage = message;

            if (hasConfirm && hasCancel)
            {
                formattedMessage = $"{message}\n{GetConfirmIcon()}{confirmText}\n{GetCancelIcon()}{cancelText}";
            }
            else if (hasConfirm)
            {
                formattedMessage = $"{message}\n{GetConfirmIcon()}{confirmText}";
            }
            else if (hasCancel)
            {
                formattedMessage = $"{message}\n{GetCancelIcon()}{cancelText}";
            }
            else
            {
                // Information only - show any button to close
                formattedMessage = $"{message}\n{GetConfirmIcon()}Close";
            }

            return formattedMessage;
        }

        private string CleanupEmptyLines(string text)
        {
            // Remove lines that are empty or only contain whitespace
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

        private static void ScalePopupKeepTextSize(GameObject promptObject, float scale)
        {
            // First, collect all MessageBox components before scaling
            global::MessageBox[] messageBoxes = promptObject.GetComponentsInChildren<global::MessageBox>();

            // Store original scales of text-related components
            var textScales = new System.Collections.Generic.Dictionary<Transform, Vector3>();

            foreach (global::MessageBox messageBox in messageBoxes)
            {
                if (messageBox.transform != null)
                {
                    textScales[messageBox.transform] = messageBox.transform.localScale;
                }
            }

            // Also check for TextMesh components (common for text rendering in Unity)
            TextMesh[] textMeshes = promptObject.GetComponentsInChildren<TextMesh>();
            foreach (TextMesh textMesh in textMeshes)
            {
                if (textMesh.transform != null)
                {
                    textScales[textMesh.transform] = textMesh.transform.localScale;
                }
            }

            // Check for any component with "Text" in the name
            Transform[] allTransforms = promptObject.GetComponentsInChildren<Transform>();
            foreach (Transform t in allTransforms)
            {
                if (t.name.ToLower().Contains("text") && !textScales.ContainsKey(t))
                {
                    textScales[t] = t.localScale;
                }
            }

            // Now scale the root object
            promptObject.transform.localScale = new Vector3(scale, scale, scale);

            // Counter-scale all text elements to keep them at original size
            float inverseScale = 1.0f / scale;
            foreach (var kvp in textScales)
            {
                kvp.Key.localScale = new Vector3(
                    kvp.Value.x * inverseScale,
                    kvp.Value.y * inverseScale,
                    kvp.Value.z * inverseScale
                );
            }

            // Log what we found for debugging
            UnityEngine.Debug.Log($"[RandomizerMessageBox] Scaled popup to {scale}x. Found {messageBoxes.Length} MessageBox, {textMeshes.Length} TextMesh, and {textScales.Count} total text components to counter-scale");
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
}
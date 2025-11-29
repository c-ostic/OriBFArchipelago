using HarmonyLib;
using OriBFArchipelago.MapTracker.Core;
using UnityEngine;

namespace OriBFArchipelago.Patches
{
    internal class InventoryManagerPatches
    {
        [HarmonyPatch(typeof(InventoryManager), nameof(InventoryManager.Awake))]
        public static class InventoryManager_Awake_Patch
        {
            [HarmonyPostfix]
            static void Awake_Postfix(InventoryManager __instance)
            {
                // Add our GUI drawer component
                __instance.gameObject.AddComponent<InventoryButtonHintDrawer>();
            }
        }

        public class InventoryButtonHintDrawer : MonoBehaviour
        {
            private InventoryManager inventoryManager;
            private GameObject lbButtonHint;
            private GameObject rbButtonHint;
            private bool hintsCreated = false;
            private bool wasVisible = false;
            private bool wasKeyboardUsedLast = false;


            void Awake()
            {
                inventoryManager = GetComponent<InventoryManager>();
            }

            void Start()
            {
                CreateButtonHints();
            }

            void CreateButtonHints()
            {
                ModLogger.Debug("Creating custom button hints");

                Transform legendTransform = inventoryManager.transform.Find("legend");
                if (legendTransform == null)
                {
                    ModLogger.Debug("Legend object not found!");
                    return;
                }

                Transform backButton = legendTransform.Find("back");
                Transform selectButton = legendTransform.Find("select");
                Transform navigateButton = legendTransform.Find("navigate");

                if (backButton == null)
                {
                    ModLogger.Debug("Back button template not found!");
                    return;
                }

                // Calculate spacing between existing buttons
                float spacing = 0f;
                if (navigateButton != null && selectButton != null)
                {
                    spacing = selectButton.localPosition.x - navigateButton.localPosition.x;
                }

                // Create LB button hint
                lbButtonHint = UnityEngine.Object.Instantiate(backButton.gameObject);
                lbButtonHint.transform.SetParent(legendTransform);
                lbButtonHint.name = "lb_custom";

                Vector3 lbPos = navigateButton != null ? navigateButton.localPosition : backButton.localPosition;
                lbPos.x -= (spacing > 0 ? spacing * 3 : 4f);
                lbButtonHint.transform.localPosition = lbPos;
                lbButtonHint.transform.localRotation = backButton.localRotation;
                lbButtonHint.transform.localScale = backButton.localScale;

                // Create RB button hint
                rbButtonHint = UnityEngine.Object.Instantiate(backButton.gameObject);
                rbButtonHint.transform.SetParent(legendTransform);
                rbButtonHint.name = "rb_custom";

                Vector3 rbPos = backButton.localPosition;
                rbPos.x += (spacing > 0 ? spacing : 2f);
                rbButtonHint.transform.localPosition = rbPos;
                rbButtonHint.transform.localRotation = backButton.localRotation;
                rbButtonHint.transform.localScale = backButton.localScale;

                hintsCreated = true;
                ModLogger.Debug("Button hints created successfully");
            }

            private void UpdateLBButtonText()
            {
                if (lbButtonHint == null) return;

                string buttonIcon = GetLBButtonIcon();
                SetButtonHintText(lbButtonHint, $"{buttonIcon}  Teleport to start");
            }

            private void UpdateRBButtonText()
            {
                if (rbButtonHint == null) return;

                if (TeleporterManager.GetLastTeleporter() != null)
                {
                    string buttonIcon = GetRBButtonIcon();
                    SetButtonHintText(rbButtonHint, $"{buttonIcon}  Teleport to {TeleporterManager.GetLastTeleporter().FriendlyName}");
                    rbButtonHint.SetActive(true);
                }
                else
                {
                    rbButtonHint.SetActive(false);
                }
            }

            private string GetLBButtonIcon()
            {
                if (PlayerInput.Instance.WasKeyboardUsedLast)
                {
                    return "F3";
                }
                else
                {
                    return "<icon>R</>"; // Left Shoulder
                }
            }

            private string GetRBButtonIcon()
            {
                if (PlayerInput.Instance.WasKeyboardUsedLast)
                {
                    return "F4";
                }
                else
                {
                    return "<icon>S</>"; // Right Shoulder
                }
            }

            private void SetButtonHintText(GameObject buttonHint, string newText)
            {
                Component messageBoxComponent = buttonHint.GetComponent("MessageBox");
                if (messageBoxComponent == null)
                {
                    return;
                }

                var messageBoxType = messageBoxComponent.GetType();

                // Clear MessageProvider so OverrideText takes precedence
                var messageProviderField = messageBoxType.GetField("MessageProvider",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                if (messageProviderField != null)
                {
                    messageProviderField.SetValue(messageBoxComponent, null);
                }

                var overrideTextField = messageBoxType.GetField("OverrideText", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                if (overrideTextField != null)
                {
                    overrideTextField.SetValue(messageBoxComponent, newText);

                    var refreshMethod = messageBoxType.GetMethod("RefreshText", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                    if (refreshMethod != null)
                    {
                        refreshMethod.Invoke(messageBoxComponent, null);
                    }
                }
            }

            void OnGUI()
            {
                if (!hintsCreated) return;

                bool isVisible = inventoryManager.NavigationManager.IsVisible;
                bool currentKeyboardState = PlayerInput.Instance.WasKeyboardUsedLast;

                // Update when menu opens OR when input method changes
                if ((isVisible && !wasVisible) || (isVisible && currentKeyboardState != wasKeyboardUsedLast))
                {
                    UpdateLBButtonText();
                    UpdateRBButtonText();
                    wasKeyboardUsedLast = currentKeyboardState;
                }

                wasVisible = isVisible;

                if (lbButtonHint != null)
                {
                    lbButtonHint.SetActive(isVisible);
                }

                // Hide RB when inventory is closed
                if (!isVisible && rbButtonHint != null)
                {
                    rbButtonHint.SetActive(false);
                }
            }
        }
    }
}
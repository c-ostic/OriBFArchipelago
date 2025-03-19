using BepInEx.Configuration;
using Core;
using OriModding.BF.Core;
using OriModding.BF.UiLib.Menu;
using System;
using System.Linq;
using UnityEngine;

namespace OriBFArchipelago.MapTracker.Menu
{
    internal class BaseModOptionsScreen : CustomOptionsScreen
    {
        internal void AddMultiToggle(ConfigEntry<string> setting, string label, string tooltip, string[] options, Func<string, string> displayFormatter = null)
        {
            // Create the base menu item
            CleverMenuItem cleverMenuItem = AddItem(label);
            cleverMenuItem.name = label;

            // Create a custom component to manage multi-state toggling
            var multiToggleAction = cleverMenuItem.gameObject.AddComponent<MultiToggleCustomSettingsAction>();

            // Store the necessary information in the component
            multiToggleAction.Setting = setting;
            multiToggleAction.Options = options.Select(x => x.ToString()).ToArray();
            multiToggleAction.DisplayFormatter = displayFormatter ?? (x => x);

            // Initialize the component
            multiToggleAction.Init();

            // Add press callback to cycle through options
            cleverMenuItem.PressedCallback += delegate
            {
                multiToggleAction.Cycle();
            };

            // Configure tooltip
            ConfigureTooltip(cleverMenuItem.GetComponent<CleverMenuItemTooltip>(), tooltip);
        }

        private void ConfigureTooltip(CleverMenuItemTooltip tooltipComponent, string tooltip)
        {
            BasicMessageProvider basicMessageProvider = ScriptableObject.CreateInstance<BasicMessageProvider>();
            basicMessageProvider.SetMessage(tooltip);
            tooltipComponent.Tooltip = basicMessageProvider;
        }
    }

    public class MultiToggleCustomSettingsAction : MonoBehaviour
    {
        public ConfigEntry<string> Setting;
        public string[] Options;
        public Func<string, string> DisplayFormatter;
        public MessageBox MessageBox;
        private int currentIndex;

        public void Awake()
        {
            // Try to find an existing ToggleSettingsAction to reuse sound effects
            ToggleSettingsAction componentInChildren = GetComponentInChildren<ToggleSettingsAction>();
            if (componentInChildren != null)
            {
                OnSound = componentInChildren.OnSound;
                OffSound = componentInChildren.OffSound;
                UnityEngine.Object.Destroy(componentInChildren);
            }
        }

        public SoundProvider OnSound;
        public SoundProvider OffSound;

        public void Cycle()
        {
            // Move to the next option
            currentIndex = (currentIndex + 1) % Options.Length;

            // Set the new value
            Setting.Value = Options[currentIndex];

            // Update display
            UpdateDisplay();

            // Play sound
            PlaySound();
        }

        private void PlaySound()
        {
            // You might want to adjust sound logic for multi-state toggle
            if ((bool)OnSound)
            {
                Sound.Play(OnSound.GetSound(null), transform.position, null);
            }
        }

        public void Init()
        {
            // Find the message box for displaying current state
            MessageBox = transform.FindChild("text/stateText").GetComponent<MessageBox>();

            // Find the current index of the initial setting
            currentIndex = Array.IndexOf(Options, Setting.Value);
            if (currentIndex == -1)
            {
                // If current value is not in options, default to first option
                currentIndex = 0;
                Setting.Value = Options[0];
            }

            // Initial display update
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            // Use the provided formatter or default to ToString
            string displayText = DisplayFormatter(Options[currentIndex]);
            MessageBox.SetMessage(new MessageDescriptor(displayText));
        }
    }
}
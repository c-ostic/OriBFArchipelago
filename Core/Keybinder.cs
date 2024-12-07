using Core;
using HarmonyLib;
using OriModding.BF.InputLib;
using SmartInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Input = Core.Input;

namespace OriBFArchipelago.Core
{
    [HarmonyPatch(typeof(PlayerInput), nameof(PlayerInput.RefreshControls))]
    internal class PlayerInputPatch
    {
        private static void Postfix()
        {
            Keybinder.Instance.UpdateState();
        }
    }


    /**
     * Links custom randomizer actions to keybinds
     */
    internal class Keybinder : MonoBehaviour
    {
        /**
         * Map core input names to their corresponding name in the Input enum
         * (Mostly the same except for Dash and Grenade)
         */
        private static Dictionary<string, Input.InputButtonProcessor> coreInputMap = new Dictionary<string, Input.InputButtonProcessor>
        {
            {"Jump", Input.Jump},
            {"SpiritFlame", Input.SpiritFlame},
            {"Bash", Input.Bash},
            {"SoulFlame", Input.SoulFlame},
            {"ChargeJump", Input.ChargeJump},
            {"Glide", Input.Glide},
            {"Dash", Input.RightShoulder},
            {"Grenade", Input.LeftShoulder},
            {"Left", Input.Left},
            {"Right", Input.Right},
            {"Up", Input.Up},
            {"Down", Input.Down},
            {"LeftStick", Input.LeftStick},
            {"RightStick", Input.RightStick},
            {"Start", Input.Start},
            {"Select", Input.Select}
        };

        /**
         * Define the custom randomizer actions
         */
        internal enum Action
        {
            OpenTeleport,
            DoubleBash,
            GrenadeJump
        }

        /**
         * Default keybinds for the above actions
         */ 
        private static Dictionary<Action, string> defaultKeybinds = new Dictionary<Action, string>
        {
            { Action.OpenTeleport, "LeftAlt+T, RightAlt+T" },
            { Action.DoubleBash, "Grenade" },
            { Action.GrenadeJump, "Grenade+Jump" }
        };

        /**
         * Used with dictionary below for mapping controller button names to XboxControllerInput
         */
        internal enum ControllerButton
        {
            A,
            B,
            X,
            Y,
            LT,
            RT,
            LB,
            RB,
            LS,
            RS
        }

        /**
         * Used with enum above for mapping controller button names to XboxControllerInput
         */
        private static Dictionary<ControllerButton, XboxControllerInput.Button> controllerMap = new Dictionary<ControllerButton, XboxControllerInput.Button>
        {
            { ControllerButton.A, XboxControllerInput.Button.ButtonA },
            { ControllerButton.B, XboxControllerInput.Button.ButtonB },
            { ControllerButton.X, XboxControllerInput.Button.ButtonX },
            { ControllerButton.Y, XboxControllerInput.Button.ButtonY },
            { ControllerButton.LT, XboxControllerInput.Button.LeftTrigger },
            { ControllerButton.RT, XboxControllerInput.Button.RightTrigger },
            { ControllerButton.LB, XboxControllerInput.Button.LeftShoulder },
            { ControllerButton.RB, XboxControllerInput.Button.RightShoulder },
            { ControllerButton.LS, XboxControllerInput.Button.LeftStick },
            { ControllerButton.RS, XboxControllerInput.Button.RightStick }
        };

        /**
         * Represents a single key or button press
         */
        public class SingleInput: Input.InputButtonProcessor
        {
            public enum InputType
            {
                CoreInput,
                ControllerButton,
                KeyCode
            };

            private string raw;

            private KeyCode key;

            private Input.InputButtonProcessor coreInput;

            private ControllerButton button;

            private InputType type;

            public SingleInput(string input)
            {
                input = input.Trim();

                raw = input;
                if (input.StartsWith("_"))
                {
                    type = InputType.ControllerButton;
                    button = (ControllerButton)Enum.Parse(typeof(ControllerButton), input.Substring(1), true);
                }
                else if (coreInputMap.ContainsKey(input))
                {
                    type = InputType.CoreInput;
                    coreInput = coreInputMap[input];
                }
                else
                {
                    type = InputType.KeyCode;
                    key = input != "" ? (KeyCode)Enum.Parse(typeof(KeyCode), input, true) : KeyCode.None;
                }
            }

            public override string ToString()
            {
                return raw;
            }

            /**
             * Intended to be called every frame to update the state of this input
             */
            public void UpdateState()
            {
                switch (type)
                {
                    case InputType.CoreInput:
                        this.Update(coreInput.Pressed);
                        break;
                    case InputType.ControllerButton:
                        this.Update(new ControllerButtonInput(controllerMap[button]).GetButton());
                        break;
                    case InputType.KeyCode:
                        this.Update(MoonInput.GetKey(key));
                        break;
                }
            }
        }

        /**
         * Represents a chord of keys (could still be one key)
         */
        private class SingleBind
        {
            List<SingleInput> inputs = new List<SingleInput>();

            public SingleBind(string data)
            {
                string[] inputStrings = data.Trim().Split('+');

                foreach (string inputString in inputStrings)
                {
                    try
                    {
                        inputs.Add(new SingleInput(inputString));
                    }
                    catch (ArgumentException)
                    { 
                        // Log the specific failing key, but then pass on the exception since this single bind isn't complete
                        Console.WriteLine("Keybinder: Unrecognized key \"" + inputString + "\" found in file");
                        inputs.Clear();
                        throw new ArgumentException();
                    }
                }
            }

            public SingleBind(List<SingleInput> inputs)
            {
                this.inputs = inputs;
            }

            /**
             * Intended to be called every frame to update the state of this keybind
             */
            public void UpdateState()
            {
                foreach (SingleInput input in inputs)
                {
                    input.UpdateState();
                }
            }

            public override string ToString()
            {
                return string.Join("+", inputs.Select(i => i.ToString()).ToArray());
            }

            /**
             * Returns true while all inputs are pressed down
             */
            public bool IsPressed
            {
                get
                {
                    return inputs.All(i => i.IsPressed);
                }
            }

            /**
             * Is true only the moment the full keybind is pressed down
             * Expects there to be an activator key that is pressed last
             */
            public bool OnPressed
            {
                get
                {
                    // At least one of the inputs was pressed this frame
                    // and the others were also just pressed or were already pressed
                    return inputs.Any(j => j.OnPressed) && inputs.All(i => i.OnPressed || i.IsPressed);
                }
            }
        }

        /**
         * Represents a set of keybinds that are all linked to the same action
         */
        private class BindSet
        {
            List<SingleBind> binds = new List<SingleBind>();

            public BindSet(string data)
            {
                string[] bindStrings = data.Trim().Split(',');

                foreach (string bindString in bindStrings)
                {
                    try
                    {
                        binds.Add(new SingleBind(bindString));
                    }
                    catch (ArgumentException)
                    {
                        // Can safely log error and move on to the next keybind
                        Console.WriteLine("Keybinder: Unable to parse keybind \"" + bindString.Trim() + "\"");
                    }
                }
            }

            public BindSet(List<SingleBind> binds)
            {
                this.binds = binds;
            }

            /**
             * Intended to be called every frame to update the state of each keybind
             */
            public void UpdateState()
            {
                foreach (SingleBind bind in binds)
                {
                    bind.UpdateState();
                }
            }

            public override string ToString()
            {
                return string.Join(", ", binds.Select(i => i.ToString()).ToArray());
            }

            /**
             * Returns true if any of the binds are pressed
             */
            public bool IsPressed
            {
                get
                {
                    return binds.Any(i => i.IsPressed);
                }
            }

            /**
             * Returns true when one of the binds OnPressed fires
             */
            public bool OnPressed
            {
                get
                {
                    return binds.Any(i => i.OnPressed);
                }
            }
        }

        /**
         * Creates a dictionary of bindsets using a provided dictionary of keybind strings
         * If any actions are not set, they are filled with the defaults
         */
        private static Dictionary<Action, BindSet> parseBindSets(Dictionary<Action, string> keybindStrings)
        {
            List<Action> unusedActions = Enum.GetValues(typeof(Action)).Cast<Action>().ToList();

            Dictionary<Action, BindSet> keybinds = new Dictionary<Action, BindSet>();

            // Try to add all from the provided dictionary
            if (keybindStrings != null)
            {
                foreach(Action action in keybindStrings.Keys)
                {
                    unusedActions.Remove(action);

                    keybinds.Add(action, new BindSet(keybindStrings[action]));
                }
            }

            // Fill any remaining actions from the default
            foreach (Action action in unusedActions)
            {
                keybinds.Add(action, new BindSet(defaultKeybinds[action]));
            }

            Console.WriteLine("Keybinder: Loaded the following keybinds");
            foreach (Action action in keybinds.Keys)
            {
                Console.WriteLine(action.ToString() + ": " + keybinds[action]);
            }

            return keybinds;
        }

        private Dictionary<Action, BindSet> keybinds = new Dictionary<Action, BindSet>();

        public static Keybinder Instance { get; private set; }

        private void Awake()
        {
            Instance = this;

            // TODO: add a call to RandomizerIO to get a dictionary read from file
            // TODO: add a call to RandomizerIO to write file if it didn't exist

            keybinds = parseBindSets(null);
        }

        /**
         * Called every frame to update the state of each keybind. 
         * Linked to PlayerInput update instead of using its own so controls are updated at the same time as the base game
         */
        public void UpdateState()
        {
            foreach (BindSet set in keybinds.Values)
            {
                set.UpdateState();
            }
        }

        /**
         * Checks if one of the bindsets is pressed
         */
        public static bool IsPressed(Action action)
        {
            return Instance.keybinds[action].IsPressed;
        }

        /**
         * Checks if one of the bindsets was pressed this frame
         */
        public static bool OnPressed(Action action)
        {
            return Instance.keybinds[action].OnPressed;
        }
    }
}

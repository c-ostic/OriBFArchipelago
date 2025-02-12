using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using CatlikeCoding.TextBox;
using HarmonyLib;

namespace OriBFArchipelago.Core
{
    /**
     * Creates and manages both the archipelago connection and the randomizer instance when starting a save slot
     */
    internal class RandomizerManager : MonoBehaviour
    {
        public static RandomizerReceiver Receiver { get { return instance.receiver; } }
        public static ArchipelagoConnection Connection { get { return instance.connection; } }

        public static RandomizerOptions Options { get { return instance.options; } }

        public static bool IsEditing {  get { return instance.isEditing; } }

        public static RandomizerManager instance;

        // references to both the receiver and the connection
        private RandomizerReceiver receiver;
        private ArchipelagoConnection connection;
        private RandomizerOptions options;

        private bool inGame, inSaveSelect, failedToStart, isEditing;
        private Dictionary<int, SlotData> saveSlots;

        // strings associated with the gui buttons in OnGUI
        private string slotName = "", server = "", port = "", password = "";

        /**
         * Called at game launch
         */

        private void Awake()
        {
            instance = this;
            if (RandomizerIO.ReadSlotData(out saveSlots))
            {
                Console.WriteLine("Successfully read slot data");
            }
            else
            {
                Console.WriteLine("Could not read slot data");
            }

            inGame = false;
            inSaveSelect = true; // TODO: change this to false when on start screen vs save select menu
            failedToStart = false;
            isEditing = false;
        }

        /**
         * Called every frame
         */

        private void Update()
        {
            // Call the update method on the receiver while in game
            if (inGame)
            {
                receiver.Update();
                connection.Update();
            }

            // If loading into a level failed to start, re-enable the save slots ui
            if (failedToStart)
            {
                FindObjectOfType<SaveSlotsUI>().Active = true;
                failedToStart = false;
            }
        }

        /**
         * Create a UI to allow the user to input archipelago data
         */
        private void OnGUI()
        {
            // Only display this UI when on the save select screen
            if (inSaveSelect)
            {
                GUILayout.BeginArea(new Rect(5, 5, 300, 200));

                GUILayout.BeginVertical();

                // Create an area for slot name
                GUILayout.BeginHorizontal();
                GUILayout.Label("Slot Name");
                slotName = GUILayout.TextField(slotName, 50);
                GUILayout.EndHorizontal();

                // Create an area for server name
                GUILayout.BeginHorizontal();
                GUILayout.Label("Server");
                server = GUILayout.TextField(server, 50);
                GUILayout.EndHorizontal();

                // Create an area for port number
                GUILayout.BeginHorizontal();
                GUILayout.Label("Port");
                port = GUILayout.TextField(port, 50);
                GUILayout.EndHorizontal();

                // Create an area for password
                GUILayout.BeginHorizontal();
                GUILayout.Label("Password");
                password = GUILayout.TextField(password, 50);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if (isEditing)
                {
                    // Create a button to stop editing
                    if (GUILayout.Button("Done"))
                    {
                        isEditing = false;
                    }
                }
                else
                {
                    // Create a button to start editing
                    if (GUILayout.Button("Edit"))
                    {
                        isEditing = true;
                    }
                }
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();
                GUILayout.EndArea();
            }
        }

        /**
         * Called when selecting a save slot, but not starting it yet
         */
        public void InspectSaveSlot(int index)
        {
            Console.WriteLine($"Inspecting save slot {index}");
            if (SaveSlotsUI.Instance is not null && index >= 0 && index < saveSlots.Count)
            {
                SlotData data = saveSlots[index];
                slotName = data.slotName;
                server = data.serverName;
                port = data.port + "";
                password = data.password;
            }
        }

        /**
         * Called when attempting to start a save slot
         * Returns false if there is a problem with the save slot data or archipelago connection
         */
        public bool StartSaveSlot(bool isNew)
        {
            int saveSlot = SaveSlotsUI.Instance.CurrentSlotIndex;
            Console.WriteLine($"Starting save slot {saveSlot}");

            bool canStart = true;

            // Attempt to load the this slots data first
            receiver = new RandomizerReceiver();
            if (!receiver.Init(isNew, saveSlot, slotName))
            {
                canStart = false;
                Console.WriteLine("Slot name provided does not match save file");
            }

            // Attempt to connect to archipelago only if the save slot was loaded correctly
            connection = new ArchipelagoConnection();
            int.TryParse(port, out int parsedPort);
            if (!canStart || !connection.Init(server, parsedPort, slotName, password))
            {
                canStart = false;
                Console.WriteLine("Could not connect to archipelago server");
            }

            // Check if the game can start
            if (canStart)
            {
                // If so, set necessary flags and update the slot data
                inGame = true;
                inSaveSelect = false;

                SlotData updatedData = new SlotData();
                updatedData.slotName = slotName;
                updatedData.serverName = server;
                updatedData.port = parsedPort;
                updatedData.password = password;

                saveSlots[saveSlot] = updatedData;
                RandomizerIO.WriteSlotData(saveSlots);

                options = new RandomizerOptions(connection.SlotData);

                if (options.DeathLinkLogic != DeathLinkOptions.Disabled)
                {
                    connection.EnableDeathLink(true);
                }
            }
            else
            {
                // Otherwise, trip failedToStart flag so UI can be re-enabled
                failedToStart = true;
            }

            return canStart;
        }

        /**
         * Called when returning to the main menu from a save
         */
        public void QuitSaveSlot()
        {
            Console.WriteLine($"Quitting save slot {SaveSlotsManager.CurrentSlotIndex}");
            inGame = false;
            inSaveSelect = true;
            connection.Disconnect();
            connection = null;
            receiver.OnSave(true);
            receiver = null;
            LocalGameState.Reset();
        }

        /**
         * Called when deleting a save slot
         */
        public void DeleteSaveSlot(int index)
        {
            Console.WriteLine($"Deleting save slot {index}");

            // Delete connection data
            saveSlots[index] = new SlotData();
            RandomizerIO.WriteSlotData(saveSlots);

            // Reinspect the save slot to clear out the previous data
            InspectSaveSlot(SaveSlotsUI.Instance.CurrentSlotIndex);

            // Delete slot data
            RandomizerIO.DeleteSaveFile(index);
        }
    }

    /**
     * Patch into the function that loads a pre-existing save file
     */
    [HarmonyPatch(typeof(SaveSlotsUI), nameof(SaveSlotsUI.UsedSaveSlotSelected))]
    internal class LoadGamePatch
    {
        private static bool Prefix()
        {
            return RandomizerManager.instance.StartSaveSlot(false);
        }
    }

    /**
     * Patch into the function that creates a new save file
     */
    [HarmonyPatch(typeof(SaveSlotsUI), nameof(SaveSlotsUI.SetDifficulty))]
    internal class NewGamePatch
    {
        private static bool Prefix()
        {
            return RandomizerManager.instance.StartSaveSlot(true);
        }
    }

    /**
     * Patch into the function that determines which save is currently selected
     */
    [HarmonyPatch(typeof(SaveSlotsUI), nameof(SaveSlotsUI.SetCurrentItem), typeof(int))]
    internal class InspectSavePatch
    {
        private static bool Prefix(int index)
        {
            RandomizerManager.instance.InspectSaveSlot(index);
            return true;
        }
    }

    /**
     * Patch into the function that is called when returning to main menu
     */
    [HarmonyPatch(typeof(ReturnToTitleScreenAction), nameof(ReturnToTitleScreenAction.Perform))]
    internal class ReturnToTitleScreenPatch
    {
        private static bool Prefix()
        {
            RandomizerManager.instance.QuitSaveSlot();
            return true;
        }
    }

    /**
     * Patch into the function called when deleting a save slot
     */
    [HarmonyPatch(typeof(SaveSlotsManager), nameof(SaveSlotsManager.DeleteSlot))]
    internal class DeleteSavePatch
    {
        private static bool Prefix(int index)
        {
            RandomizerManager.instance.DeleteSaveSlot(index);
            return true;
        }
    }
    
    /**
     * Patch into the function called when copying a save slot
     */
    [HarmonyPatch(typeof(SaveSlotsManager), nameof(SaveSlotsManager.CopySlot))]
    internal class CopySaveFilePatch
    {
        private static void Prefix(int from, int to)
        {
            RandomizerIO.CopySaveFile(from, to);
        }
    }

    /**
     * Prevent the refreshing of controls while editing
     */
    [HarmonyPatch(typeof(PlayerInput), nameof(PlayerInput.FixedUpdate))]
    internal class PreventPlayerInputPatch
    {
        private static bool Prefix()
        {
            return !RandomizerManager.IsEditing;
        }
    }
}

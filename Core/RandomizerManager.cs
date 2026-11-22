using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using CatlikeCoding.TextBox;
using HarmonyLib;
using OriBFArchipelago.MapTracker.Core;
using OriBFArchipelago.Helper;
using System.Collections;
using UnityEngine.Networking.Match;

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

        public static bool ArchipelagoIOFocussed { get { return instance.archipelagoIOFocussed; } }

        public static RandomizerManager instance;

        // references to both the receiver and the connection
        private RandomizerReceiver receiver;
        private ArchipelagoConnection connection;
        private RandomizerOptions options;

        private bool failedToStart, archipelagoIOFocussed;
        private Dictionary<int, SlotData> saveSlots;

        // Tracks an in-progress asynchronous connection attempt started from the save select screen
        private bool connecting;
        // Set true right before we re-run the original save slot action so the level actually loads
        private bool connectionApproved;
        // Native Ori popup shown while connecting
        private RandomizerMessageBox connectingBox;
        // Menu selection managers suspended while connecting, restored once the attempt finishes
        private readonly List<CleverMenuItemSelectionManager> suspendedMenus = new List<CleverMenuItemSelectionManager>();
        // Details of the save slot start we're resuming once the connection succeeds
        private bool pendingIsNew;
        private DifficultyMode pendingDifficulty;
        private int pendingSaveSlot;
        private int pendingParsedPort;

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

            RandomizerSettings.InGame = false;
            failedToStart = false;
        }

        /**
         * Called every frame
         */

        private void Update()
        {
            // Drive the asynchronous connection attempt started from the save select screen
            if (connecting)
            {
                ProcessPendingConnection();
            }

            // Call the update method on the receiver while in game
            if (RandomizerSettings.InGame)
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
         * Polls the background connection attempt and, once it finishes, either
         * resumes loading the save slot (success) or re-enables the save select ui (failure).
         */
        private void ProcessPendingConnection()
        {
            // still waiting on the background thread
            if (connection.Status == ArchipelagoConnection.ConnectionStatus.Connecting)
                return;

            connecting = false;
            RestoreMenus();
            connectingBox?.Destroy();
            connectingBox = null;
            RandomizerMessager.instance.Clear();
            RandomizerMessager.instance.AddMessage(connection.StatusMessage);

            if (connection.Status == ArchipelagoConnection.ConnectionStatus.Connected)
            {
                FinishSuccessfulStart();
            }
            else // Failed / None
            {
                Console.WriteLine("Could not connect to archipelago server");
                connection = null;
                receiver = null;
                failedToStart = true;
            }
        }

        /**
         * Suspends every visible menu selection manager so the player can't interact with the
         * menu behind the connecting overlay. The game's CleverMenuItemSelectionManager.FixedUpdate
         * early-returns while IsSuspended is set, which blocks all menu input.
         */
        private void SuspendMenus()
        {
            suspendedMenus.Clear();
            foreach (CleverMenuItemSelectionManager manager in FindObjectsOfType<CleverMenuItemSelectionManager>())
            {
                if (manager.IsVisible && !manager.IsSuspended)
                {
                    manager.IsSuspended = true;
                    suspendedMenus.Add(manager);
                }
            }
        }

        /**
         * Restores the menu selection managers suspended by SuspendMenus.
         */
        private void RestoreMenus()
        {
            foreach (CleverMenuItemSelectionManager manager in suspendedMenus)
            {
                if (manager != null)
                    manager.IsSuspended = false;
            }
            suspendedMenus.Clear();
        }

        /**
         * Create a UI to allow the user to input archipelago data
         */
        private void OnGUI()
        {
            // Only display this UI when on the save select screen
            if (RandomizerSettings.InSaveSelect)
            {

                GUILayout.BeginArea(new Rect(5, 5, 300, 200));

                GUILayout.BeginVertical();

                Dictionary<string, string> fields = new Dictionary<string, string>
                {
                    {"Slot Name", slotName},
                    {"Server", server},
                    {"Port", port},
                    {"Password", password}
                };

                // Create an area for slot name
                GUILayout.BeginHorizontal();
                GUILayout.Label("Slot Name");
                GUI.SetNextControlName("slotname");
                slotName = GUILayout.TextField(slotName, 50, GUILayout.Width(200));
                GUILayout.EndHorizontal();

                // Create an area for server name
                GUILayout.BeginHorizontal();
                GUILayout.Label("Server");
                GUI.SetNextControlName("server");
                server = GUILayout.TextField(server, 50, GUILayout.Width(200));
                GUILayout.EndHorizontal();

                // Create an area for port number
                GUILayout.BeginHorizontal();
                GUILayout.Label("Port");
                GUI.SetNextControlName("port");
                port = GUILayout.TextField(port, 50, GUILayout.Width(200));
                GUILayout.EndHorizontal();

                // Create an area for password
                GUILayout.BeginHorizontal();
                GUILayout.Label("Password");
                GUI.SetNextControlName("password");
                password = GUILayout.TextField(password, 50, GUILayout.Width(200));
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();
                GUILayout.EndArea();

                archipelagoIOFocussed = new string[] { "slotname", "server", "port", "password" }.Contains(GUI.GetNameOfFocusedControl());
            }
        }

        /**
         * Called when selecting a save slot, but not starting it yet
         */
        public void InspectSaveSlot(int index)
        {
            if (SaveSlotsUI.Instance is not null && index >= 0 && index < saveSlots.Count)
            {
                SlotData data = saveSlots[index];
                slotName = data.slotName;
                server = data.serverName;
                port = data.port + "";
                password = data.password;
            }
        }

        public bool CopySaveSlot(int from, int to)
        {
            saveSlots[to] = saveSlots[from];
            RandomizerIO.WriteSlotData(saveSlots);
            InspectSaveSlot(to);
            return true;
        }
        /**
         * Called when attempting to start a save slot.
         *
         * Connecting to the archipelago server can take a while (or hang), so it is done
         * asynchronously on a background thread. This method returns false to keep the player
         * on the save select screen while connecting - so messages such as "Attempting to
         * connect" and "Failed to connect" actually show up instead of the client hanging.
         * Once the background attempt succeeds, ProcessPendingConnection re-runs the original
         * save slot action (with connectionApproved set) to actually load the level.
         *
         * Returns true only to let the original game method run: either because the save slot
         * data is invalid (nothing for us to do) or because the connection already succeeded.
         */
        public bool BeginStartSaveSlot(bool isNew, DifficultyMode difficulty)
        {
            // Second pass: the connection already succeeded and we're re-running the original
            // action so the game loads the level. Let it through.
            if (connectionApproved)
            {
                connectionApproved = false;
                return true;
            }

            // Ignore repeated presses while a connection attempt is already in progress
            if (connecting)
            {
                return false;
            }

            string missingFields = string.Join(", ", new[] { string.IsNullOrEmpty(slotName) ? "slotname" : null, string.IsNullOrEmpty(server) ? "server" : null, string.IsNullOrEmpty(port) ? "port" : null }.Where(f => f != null).ToArray());

            if (!string.IsNullOrEmpty(missingFields))
            {
                RandomizerMessager.instance.AddMessage($"Required fields are empty: {missingFields}");
                failedToStart = true;
                return false;
            }

            int saveSlot = SaveSlotsUI.Instance.CurrentSlotIndex;
            Console.WriteLine($"Starting save slot {saveSlot}");

            // Attempt to load the this slots data first
            receiver = new RandomizerReceiver();
            if (!receiver.Init(isNew, saveSlot, slotName))
            {
                Console.WriteLine("Slot name provided does not match save file");
                failedToStart = true;
                return false;
            }

            RandomizerMessager.instance.AddMessage($"Attempting to connect to {server}:{port} {slotName}");

            // Start connecting to archipelago in the background so the game keeps rendering
            int.TryParse(port, out int parsedPort);
            connection = new ArchipelagoConnection();
            connection.Init(server, parsedPort, slotName, password);

            // Remember what we need to resume the load once the connection succeeds
            pendingIsNew = isNew;
            pendingDifficulty = difficulty;
            pendingSaveSlot = saveSlot;
            pendingParsedPort = parsedPort;
            connecting = true;

            // Block menu input while connecting, then show the native Ori popup with the status
            SuspendMenus();
            connectingBox = new RandomizerMessageBox($"Attempting to connect to\n{server}:{port}\nas {slotName}");
            connectingBox.ShowInfo();

            connection.BeginConnect();

            // Don't load the level yet; ProcessPendingConnection resumes once connected
            return false;
        }

        /**
         * Called from ProcessPendingConnection once the background connection has succeeded.
         * Runs the game-state setup that must happen on the main thread and then re-runs the
         * original save slot action so the level loads.
         */
        private void FinishSuccessfulStart()
        {
            // Post-login setup that reads the receiver state - safe here on the main thread
            connection.FinalizeConnection();

            receiver.SyncArchipelagoCheckedLocations(connection.GetArchipelagoCheckedLocations());

            RandomizerSettings.InGame = true;
            RandomizerSettings.InSaveSelect = false;

            SlotData updatedData = new SlotData();
            updatedData.slotName = slotName;
            updatedData.serverName = server;
            updatedData.port = pendingParsedPort;
            updatedData.password = password;

            saveSlots[pendingSaveSlot] = updatedData;
            RandomizerIO.WriteSlotData(saveSlots);

            options = new RandomizerOptions(connection.SlotData);
            if (options.Goal == GoalOptions.WarmthFragments || options.Goal == GoalOptions.WorldTour)
            {
                ModLogger.Debug("Checking goal locations");

                if (options.GoalLocations == null)
                    connection.SetGoalLocationsInOptions();
            }
            if (options.DeathLinkLogic != DeathLinkOptions.Disabled)
            {
                connection.EnableDeathLink(true);
            }

            // Re-run the original save slot action, now connected, so the level actually loads
            connectionApproved = true;
            if (pendingIsNew)
            {
                SaveSlotsUI.Instance.SetDifficulty(pendingDifficulty);
            }
            else
            {
                SaveSlotsUI.Instance.UsedSaveSlotSelected();
            }
        }

        /**
         * Called when returning to the main menu from a save
         */
        public void QuitSaveSlot()
        {
            Console.WriteLine($"Quitting save slot {SaveSlotsManager.CurrentSlotIndex}");
            RandomizerSettings.InGame = false;
            receiver.OnSave(true);
            connection.Disconnect();
            connection = null;
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
            return RandomizerManager.instance.BeginStartSaveSlot(false, DifficultyMode.Normal);
        }
    }

    /**
     * Patch into the function that creates a new save file
     */
    [HarmonyPatch(typeof(SaveSlotsUI), nameof(SaveSlotsUI.SetDifficulty))]
    internal class NewGamePatch
    {
        private static bool Prefix(DifficultyMode difficulty)
        {
            return RandomizerManager.instance.BeginStartSaveSlot(true, difficulty);
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
            MaptrackerSettings.Delete();
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
            RandomizerManager.instance.CopySaveSlot(from, to);
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
            return !RandomizerManager.ArchipelagoIOFocussed;
        }
    }
}

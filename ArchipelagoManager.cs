using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using Archipelago.MultiClient.Net.Packets;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Text;
using UnityEngine;
using static System.Collections.Specialized.BitVector32;

namespace OriBFArchipelago
{
    internal class ArchipelagoManager : MonoBehaviour
    {
        public const string GAME_NAME = "Ori and the Blind Forest";

        private static ArchipelagoManager instance;

        public bool Connected { get; private set; }

        private ArchipelagoSession session;
        private RandomizerReceiver receiver;
        private RandomizerMessager messager;

        public void Start()
        {
            instance = this;
            receiver = FindObjectOfType<RandomizerReceiver>();
            messager = FindObjectOfType<RandomizerMessager>();

            string hostname = "", slot = "", password = "";
            int port = 0;

            try
            {
                StreamReader sr = new StreamReader("BepInEx\\plugins\\OriBFArchipelago\\Files\\connection.config");

                hostname = sr.ReadLine().Split('=')[1];
                port = Int32.Parse(sr.ReadLine().Split('=')[1]);
                slot = sr.ReadLine().Split('=')[1];
                password = sr.ReadLine().Split('=')[1];

                //Close the file
                sr.Close();

                Console.WriteLine("Read connection file");

            }
            catch (Exception e)
            {
                Console.WriteLine("Could not find connection file: " + e.Message);
            }

            receiver.LoadFileInventory(slot);
            Init(hostname, port, slot, password);
        }

        /**
         * Creates an Archipelago session and sets up events to listen to
         */
        public void Init(string hostname, int port, string user, string password) 
        {
            instance = this;
            session = ArchipelagoSessionFactory.CreateSession(hostname, port);

            session.MessageLog.OnMessageReceived += OnMessageReceived;
            session.Items.ItemReceived += OnItemReceived;

            Connect(hostname, user, password);
        }

        /**
         * Tries to connect to a slot on the Archipelago server
         */
        private void Connect(string server, string user, string password)
        {
            LoginResult result;

            try
            {
                // handle TryConnectAndLogin attempt here and save the returned object to `result`
                result = session.TryConnectAndLogin(GAME_NAME, user, ItemsHandlingFlags.AllItems, 
                    null, null, null, password);
            }
            catch (Exception e)
            {
                result = new LoginFailure(e.GetBaseException().Message);
            }

            if (!result.Successful)
            {
                LoginFailure failure = (LoginFailure)result;
                string errorMessage = $"Failed to Connect to {server} as {user}:";
                foreach (string error in failure.Errors)
                {
                    errorMessage += $"\n    {error}";
                }
                foreach (ConnectionRefusedError error in failure.ErrorCodes)
                {
                    errorMessage += $"\n    {error}";
                }
                Console.WriteLine(errorMessage);
                //ReceivedMessage.Invoke(errorMessage);
            }
            else
            {
                // Successfully connected, `ArchipelagoSession` (assume statically defined as `session` from now on) can now be used to interact with the server and the returned `LoginSuccessful` contains some useful information about the initial connection (e.g. a copy of the slot data as `loginSuccess.SlotData`)
                var loginSuccess = (LoginSuccessful)result;
                Console.WriteLine($"Successfully connected to {server} as {user}");
                //ReceivedMessage.Invoke("Successfully connected to slot " + loginSuccess.Slot);
            }

            Connected = result.Successful;
        }

        /**
         * Upon receiving a message from the server, forward to this class's message event
         */
        private void OnMessageReceived(LogMessage message)
        {
            messager.AddMessage(message.ToString());
            Console.WriteLine(message.ToString());
        }

        /**
         * Upon receiving an item from the server, forward to this class's item event
         */
        private void OnItemReceived(ReceivedItemsHelper helper)
        {
            string itemName = helper.PeekItem().ItemName;

            receiver.ReceiveItem(itemName);

            helper.DequeueItem();
        }

        /**
         * Send a message to the server that a location has been checked
         */
        private void CheckLocation(string location)
        {
            if (Connected)
            {
                if (location is not null)
                {
                    long locationId = session.Locations.GetLocationIdFromName(GAME_NAME, location);
                    session.Locations.CompleteLocationChecks(locationId);
                    Console.WriteLine("Checked " + location);
                }
                else
                {
                    Console.WriteLine("Invalid location: " + location);
                }
            }
            else
            {
                Console.WriteLine("Checked " + location + " but not connected");
            }
        }

        private void SendCompletion()
        {
            if (Connected)
            {
                StatusUpdatePacket statusUpdatePacket = new StatusUpdatePacket();
                statusUpdatePacket.Status = ArchipelagoClientState.ClientGoal;
                session.Socket.SendPacket(statusUpdatePacket);
            }
        }

        private readonly List<string> goalLocations = new List<string>
        {
            "BashSkillTree",
            "ChargeFlameSkillTree",
            "ChargeJumpSkillTree",
            "ClimbSkillTree",
            "DashSkillTree",
            "DoubleJumpSkillTree",
            "GrenadeSkillTree",
            "StompSkillTree",
            "WallJumpSkillTree"
        };

        private bool CheckGoalCompletion()
        {
            if (Connected)
            {
                bool hasMetGoal = true;
                ReadOnlyCollection<long> checkedLocations = session.Locations.AllLocationsChecked;
                foreach (string goalLocation in goalLocations)
                {
                    long id = session.Locations.GetLocationIdFromName(GAME_NAME, goalLocation);
                    if (!checkedLocations.Contains(id))
                    {
                        hasMetGoal = false;
                    }
                }
                return hasMetGoal;
            }
            else
            {
                return false;
            }
        }

        private bool CheckGinsoEscapeCompletion()
        {
            if (Connected)
            {
                ReadOnlyCollection<long> checkedLocations = session.Locations.AllLocationsChecked;
                long id = session.Locations.GetLocationIdFromName(GAME_NAME, "GinsoEscapeExit");
                if (checkedLocations.Contains(id))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static void CheckLocationByGameObject(GameObject g)
        {            
            instance.CheckLocation(LocationLookup.GetLocationName(g));
        }

        public static void CheckLocationString(string location)
        {
            instance.CheckLocation(location);
        }

        public static void CompleteGame()
        {
            instance.SendCompletion();
        }

        public static bool IsGoalComplete()
        {
            return instance.CheckGoalCompletion();
        }

        public static bool IsGinsoEscapeComplete()
        {
            return instance.CheckGinsoEscapeCompletion();
        }
    }
}

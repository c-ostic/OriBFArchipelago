using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using Archipelago.MultiClient.Net.Packets;
using Game;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using UnityEngine;

namespace OriBFArchipelago.Core
{
    /**
     * Manages the archipelago connection
     */
    internal class ArchipelagoConnection
    {
        // The name of the game used to connect to arhcipelago
        public const string GAME_NAME = "Ori and the Blind Forest";

        // The archipelago session
        private ArchipelagoSession session;
        private DeathLinkService deathLinkService;
        private string slotName;

        // boolean used to ignore the death caused by deathlink as to not send another
        private bool ignoreNextDeath;
        // boolean used to queue a death from death link if player is in menu or cutscene
        private bool queueDeath;

        public bool Connected { get; private set; }

        public Dictionary<string, object> SlotData { get; private set; }

        /**
         * Creates an Archipelago session and sets up events to listen to
         */
        public bool Init(string hostname, int port, string user, string password)
        {
            slotName = user;

            session = ArchipelagoSessionFactory.CreateSession(hostname, port);

            session.MessageLog.OnMessageReceived += OnMessageReceived;
            session.Items.ItemReceived += OnItemReceived;

            deathLinkService = session.CreateDeathLinkService();
            deathLinkService.OnDeathLinkReceived += OnDeathLinkRecieved;

            return Connect(hostname, user, password);
        }

        /**
         * Disconnects from the archipelago server
         */
        public void Disconnect()
        {
            if (Connected)
            {
                session.Socket.Disconnect();
            }
        }

        /**
         * Tries to connect to a slot on the Archipelago server
         */
        private bool Connect(string server, string user, string password)
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
                RandomizerMessager.instance.AddMessage($"Failed to connect to {server} as {user}");
            }
            else
            {
                // Successfully connected, `ArchipelagoSession` (assume statically defined as `session` from now on) can now be used to interact with the server and the returned `LoginSuccessful` contains some useful information about the initial connection (e.g. a copy of the slot data as `loginSuccess.SlotData`)
                var loginSuccess = (LoginSuccessful)result;
                Console.WriteLine($"Successfully connected to {server} as {user}");
                RandomizerMessager.instance.AddMessage($"Successfully connected to {server} as {user}");
                SlotData = loginSuccess.SlotData;
            }

            Connected = result.Successful;
            return Connected;
        }

        public void Update()
        {
            if (queueDeath &&
                Characters.Sein.Active && 
                !Characters.Sein.IsSuspended && 
                Characters.Sein.Controller.CanMove && 
                !UI.MainMenuVisible)
            {
                queueDeath = false;
                Damage damage = new Damage(10000f, Vector2.zero, Vector2.zero, DamageType.Lava, Characters.Sein.gameObject);
                Characters.Sein.Controller.OnRecieveDamage(damage);
            }
        }

        /**
         * Upon receiving a message from the server, forward to this class's message event
         */
        private void OnMessageReceived(LogMessage message)
        {
            Console.WriteLine(message.ToString());
            RandomizerMessager.instance.AddMessage(message);
        }

        /**
         * Upon receiving an item from the server, forward to this class's item event
         */
        private void OnItemReceived(ReceivedItemsHelper helper)
        {
            string itemName = helper.PeekItem().ItemName;

            RandomizerManager.Receiver.ReceiveItem((InventoryItem) Enum.Parse(typeof(InventoryItem), itemName));

            helper.DequeueItem();
        }

        /**
         * Upon receiving a death link from the server, kill the player
         */
        private void OnDeathLinkRecieved(DeathLink deathLink)
        {
            // Send a message about the death
            if (deathLink.Cause != null)
            {
                RandomizerMessager.instance.AddMessage(deathLink.Cause);
            }
            else
            {
                RandomizerMessager.instance.AddMessage(deathLink.Source + " died");
            }

            // A death caused by death link on full will trigger another death link; this is to prevent that
            if (RandomizerManager.Options.DeathLinkLogic == DeathLinkOptions.Full)
            {
                ignoreNextDeath = true;
            }

            queueDeath = true;
        }

        /**
         * Send a message to the server that a location has been checked
         */
        public async void CheckLocation(string location)
        {
            if (Connected)
            {
                if (location is not null)
                {
                    long locationId = session.Locations.GetLocationIdFromName(GAME_NAME, location);
                    await Task.Factory.StartNew(() => session.Locations.CompleteLocationChecks(locationId));
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

        /**
         * Check location using gameobject location lookup
         */
        public void CheckLocationByGameObject(GameObject g)
        {
            CheckLocation(LocationLookup.GetLocationName(g));
        }

        /**
         * Sends the complete goal message to the archipelago server
         */
        public void SendCompletion()
        {
            if (Connected)
            {
                StatusUpdatePacket statusUpdatePacket = new StatusUpdatePacket();
                statusUpdatePacket.Status = ArchipelagoClientState.ClientGoal;
                session.Socket.SendPacket(statusUpdatePacket);
                Console.WriteLine("Complete Goal");
            }
        }

        /**
         * Enabled or disabled deathlink
         */
        public void EnableDeathLink(bool enable)
        {
            if (enable)
            {
                deathLinkService.EnableDeathLink();
            }
            else
            {
                deathLinkService.DisableDeathLink();
            }
        }

        /**
         * Sends a death link to the archipelago server
         */
        public void SendDeathLink()
        {
            if (!ignoreNextDeath)
            {
                deathLinkService.SendDeathLink(new DeathLink(slotName, slotName + " perished in the Blind Forest"));
            }

            ignoreNextDeath = false;
        }

        // List of required locations for the All Trees goal
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

        /**
         * Check if the goal condition has been met
         */
        public bool IsGoalComplete()
        {
            if (Connected)
            {
                bool hasMetGoal = true;
                ReadOnlyCollection<long> checkedLocations = session.Locations.AllLocationsChecked;
                int countTrees = 0;
                foreach (string goalLocation in goalLocations)
                {
                    long id = session.Locations.GetLocationIdFromName(GAME_NAME, goalLocation);
                    if (!checkedLocations.Contains(id))
                    {
                        hasMetGoal = false;
                        Console.WriteLine("Missing tree: " + goalLocation);
                    }
                    else
                    {
                        countTrees++;
                    }
                }
                RandomizerMessager.instance.AddMessage($"{countTrees} of out 9 trees checked");
                return hasMetGoal;
            }
            else
            {
                return false;
            }
        }

        /**
         * Check if the ginso escape has been complete
         */
        public bool IsGinsoEscapeComplete()
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

        public bool IsForlornEscapeComplete()
        {
            if (Connected)
            {
                ReadOnlyCollection<long> checkedLocations = session.Locations.AllLocationsChecked;
                long id = session.Locations.GetLocationIdFromName(GAME_NAME, "ForlornEscape");
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
    }
}

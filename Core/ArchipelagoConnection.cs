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
using System.Text;
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
            if (location is null)
            {
                Console.WriteLine("Invalid location: " + location);
                return;
            }

            if (Connected)
            {
                if (RandomizerManager.Options.MapStoneLogic == MapStoneOptions.Progressive &&
                    mapLocations.Contains(location))
                {
                    int nextMap = RandomizerManager.Receiver.GetItemCount(InventoryItem.MapStoneUsed);
                    location = "ProgressiveMap" + nextMap;
                }

                long locationId = session.Locations.GetLocationIdFromName(GAME_NAME, location);
                await Task.Factory.StartNew(() => session.Locations.CompleteLocationChecks(locationId));
                Console.WriteLine("Checked " + location);
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
        private readonly List<string> skillTreeLocations = new List<string>
        {
            "BashSkillTree",
            "ChargeFlameSkillTree",
            "ChargeJumpSkillTree",
            "ClimbSkillTree",
            "DashSkillTree",
            "DoubleJumpSkillTree",
            "GrenadeSkillTree",
            "StompSkillTree",
            "WallJumpSkillTree",
            "GlideSkillFeather"
        };

        // List of required locations for the All Maps goal
        private readonly List<string> mapLocations = new List<string>
        {
            "GladesMap",
            "BlackrootMap",
            "HollowGroveMap",
            "GumoHideoutMap",
            "SwampMap",
            "HoruMap",
            "ValleyMap",
            "ForlornMap",
            "SorrowMap"
        };

        /**
         * Check if the goal condition has been met
         */
        public bool IsGoalComplete()
        {
            if (Connected)
            {
                bool hasMetGoal = true;
                StringBuilder message = new StringBuilder();
                ReadOnlyCollection<long> checkedLocations = session.Locations.AllLocationsChecked;

                if (RandomizerManager.Options.Goal == GoalOptions.AllSkillTrees)
                {
                    int countTrees = 0;
                    List<string> uncheckedTrees = new List<string>();
                    foreach (string goalLocation in skillTreeLocations)
                    {
                        long id = session.Locations.GetLocationIdFromName(GAME_NAME, goalLocation);
                        if (!checkedLocations.Contains(id))
                        {
                            hasMetGoal = false;
                            uncheckedTrees.Add(goalLocation);
                        }
                        else
                        {
                            countTrees++;
                        }
                    }
                    message.Append($"{countTrees} of out 10 trees checked. \n");
                    if (!hasMetGoal)
                    {
                        message.Append("Missing ");
                        foreach (string tree in uncheckedTrees)
                        {
                            message.Append(tree).Append(", ");
                        }
                        message.Remove(message.Length - 2, 2); // remove last comma
                    }
                }
                else if (RandomizerManager.Options.Goal == GoalOptions.AllMaps &&
                    RandomizerManager.Options.MapStoneLogic != MapStoneOptions.Progressive)
                {
                    int countMaps = 0;
                    List<string> uncheckedMaps = new List<string>();
                    foreach (string goalLocation in mapLocations)
                    {
                        long id = session.Locations.GetLocationIdFromName(GAME_NAME, goalLocation);
                        if (!checkedLocations.Contains(id))
                        {
                            hasMetGoal = false;
                            uncheckedMaps.Add(goalLocation);
                        }
                        else
                        {
                            countMaps++;
                        }
                    }
                    message.Append($"{countMaps} of out 9 maps checked. \n");
                    if (!hasMetGoal)
                    {
                        message.Append("Missing ");
                        foreach (string map in uncheckedMaps)
                        {
                            message.Append(map).Append(", ");
                        }
                        message.Remove(message.Length - 2, 2); // remove last comma
                    }
                }
                else if (RandomizerManager.Options.Goal == GoalOptions.AllMaps &&
                    RandomizerManager.Options.MapStoneLogic == MapStoneOptions.Progressive)
                {
                    int mapstoneUsed = RandomizerManager.Receiver.GetItemCount(InventoryItem.MapStoneUsed);
                    hasMetGoal = mapstoneUsed == 9;
                    message.Append($"{mapstoneUsed} of out 9 maps checked. ");
                }
                else if (RandomizerManager.Options.Goal == GoalOptions.WarmthFragments)
                {
                    int collectedWarmthFragments = RandomizerManager.Receiver.GetItemCount(InventoryItem.WarmthFragment);
                    int requiredWarmthFragments = RandomizerManager.Options.WarmthFragmentsRequired;
                    int availableWarmthFragments = RandomizerManager.Options.WarmthFragmentsAvailable;
                    hasMetGoal = collectedWarmthFragments >= requiredWarmthFragments;
                    message.Append($"Collected {collectedWarmthFragments} out of {requiredWarmthFragments} warmth fragments needed. \n");
                    if (!hasMetGoal)
                        message.Append($"{availableWarmthFragments - collectedWarmthFragments} remain in multiworld");
                }
                else if (RandomizerManager.Options.Goal == GoalOptions.WorldTour)
                {
                    int collectedRelics = RandomizerManager.Receiver.GetItemCount(InventoryItem.Relic);
                    int requiredRelics = RandomizerManager.Options.RelicCount;
                    string[] relicAreas = RandomizerManager.Options.WorldTourAreas;
                    hasMetGoal = collectedRelics >= requiredRelics;
                    message.Append($"Collected {collectedRelics} out of {requiredRelics} relics. \n");
                    if (!hasMetGoal)
                    {
                        message.Append($"Relics can be found in ");
                        foreach (string relic in relicAreas)
                        {
                            message.Append(relic).Append(", ");
                        }
                        message.Remove(message.Length - 2, 2); // remove last comma
                    }
                }
                else
                    hasMetGoal = true;

                RandomizerMessager.instance.AddMessage(message.ToString());

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

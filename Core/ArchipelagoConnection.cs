using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using Archipelago.MultiClient.Net.Packets;
using Game;
using OriBFArchipelago.MapTracker.Core;
using OriModding.BF.UiLib.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
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
        public const string MAP_LOCATION_DATA_KEY = "MapLocations";
        public const string FOUND_RELICS_DATA_KEY = "FoundRelics";
        public const string POSITION_DATA_KEY = "Position";
        public const string SEIN = "SeinCollected";
        public const string GOAL_LOCATION_DATA_KEY = "GoalLocations";

        // The archipelago session
        private ArchipelagoSession session;
        private DeathLinkService deathLinkService;
        private string hostname;
        private int port;
        private string slotName;
        private string password;

        // Position update variables
        private float positionTimer = 0;
        private const float POSITION_UPDATE_RATE = 5; // every 5 seconds

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
            this.hostname = hostname;
            this.port = port;
            slotName = user;
            this.password = password;

            session = ArchipelagoSessionFactory.CreateSession(hostname, port);

            session.MessageLog.OnMessageReceived += OnMessageReceived;
            session.Items.ItemReceived += OnItemReceived;

            deathLinkService = session.CreateDeathLinkService();
            deathLinkService.OnDeathLinkReceived += OnDeathLinkRecieved;

            return Connect();
        }

        /**
         * Tries to reconnect to the archipelago server
         */
        public void Reconnect()
        {
            if (session is not null)
            {
                Disconnect();
                Connect();
            }
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
        private bool Connect()
        {
            LoginResult result;

            try
            {
                // handle TryConnectAndLogin attempt here and save the returned object to `result`
                result = session.TryConnectAndLogin(GAME_NAME, slotName, ItemsHandlingFlags.AllItems,
                    null, null, null, password);
            }
            catch (Exception e)
            {
                result = new LoginFailure(e.GetBaseException().Message);
            }

            RandomizerMessager.instance.Clear();
            if (!result.Successful)
            {
                LoginFailure failure = (LoginFailure)result;
                string errorMessage = $"Failed to Connect to {hostname} as {slotName}:";
                foreach (string error in failure.Errors)
                {
                    errorMessage += $"\n    {error}";
                }
                foreach (ConnectionRefusedError error in failure.ErrorCodes)
                {
                    errorMessage += $"\n    {error}";
                }
                Console.WriteLine(errorMessage);
                RandomizerMessager.instance.AddMessage($"Failed to connect to {hostname} as {slotName}");
            }
            else
            {
                // Successfully connected, `ArchipelagoSession` (assume statically defined as `session` from now on) can now be used to interact with the server and the returned `LoginSuccessful` contains some useful information about the initial connection (e.g. a copy of the slot data as `loginSuccess.SlotData`)
                var loginSuccess = (LoginSuccessful)result;
                Console.WriteLine($"Successfully connected to {hostname} as {slotName}");
                RandomizerMessager.instance.AddMessage($"Successfully connected to {hostname} as {slotName}");
                SlotData = loginSuccess.SlotData;

                session.DataStorage[Scope.Slot, MAP_LOCATION_DATA_KEY].Initialize(new string[0]);
                session.DataStorage[Scope.Slot, FOUND_RELICS_DATA_KEY].Initialize(new string[0]);
                session.DataStorage[Scope.Slot, POSITION_DATA_KEY].Initialize(new float[0]);
                session.DataStorage[Scope.Slot, SEIN].Initialize(false);
                session.DataStorage[Scope.Slot, GOAL_LOCATION_DATA_KEY].Initialize(new string[0]);


                RecheckLocations();
                UpdateMapLocations();
            }

            Connected = result.Successful;
            return Connected;
        }

        /**
         * Called every frame
         */
        public void Update()
        {
            // Kills the player if a death link is queued
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

            // Update position every POSITION_UPDATE_RATE seconds
            positionTimer += Time.deltaTime;
            if (positionTimer >= POSITION_UPDATE_RATE)
            {
                positionTimer = 0;
                UpdatePositionTracking();
            }
        }

        /**
         * Updates the player's current player position to the AP server
         */
        private void UpdatePositionTracking()
        {
            if (Characters.Sein is not null)
            {
                float x = Characters.Sein.transform.position.x;
                float y = Characters.Sein.transform.position.y;
                float[] position = [x, y];

                Task.Factory.StartNew(() => session.DataStorage[Scope.Slot, POSITION_DATA_KEY] = position);
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

            var inventoryItem = EnumParser.GetEnumValue<InventoryItem>(itemName);
            RandomizerManager.Receiver.ReceiveItem(inventoryItem);

            if (IsGoalItem(inventoryItem))
                RandomizerManager.Receiver.UpdateGoal();
            //If goal item, update goal
            if (inventoryItem == InventoryItem.Relic)
                ReceiveRelic(helper);

            helper.DequeueItem();
        }

        private bool IsGoalItem(InventoryItem item)
        {
            switch (item)
            {
                case InventoryItem.Relic:
                    return true;
                case InventoryItem.WarmthFragment:
                    return true;
                default:
                    return false;
            }
        }

        private void ReceiveRelic(ReceivedItemsHelper helper)
        {
            Location location = LocationLookup.Get(helper.PeekItem().LocationName);
            if (location != null)
            {
                session.DataStorage[Scope.Slot, FOUND_RELICS_DATA_KEY].GetAsync<string[]>(x =>
                {
                    string[] areas = x;
                    if (!areas.Contains(location.Area.ToString()))
                    {
                        session.DataStorage[Scope.Slot, FOUND_RELICS_DATA_KEY] += new string[] { location.Area.ToString() };
                    }
                });
            }
            else
            {
                Console.WriteLine("Found Relic from unknown location");
            }
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
        public void CheckLocation(Location location)
        {
            if (location is null)
                return;

            if (RandomizerManager.Options.MapStoneLogic == MapStoneOptions.Progressive && mapLocations.Contains(location.Name))
            {
                // track the original location in addition to the progressive location
                RandomizerManager.Receiver.CheckLocation(location.Name);

                int nextMap = RandomizerManager.Receiver.GetItemCount(InventoryItem.MapStoneUsed);
                location = LocationLookup.Get("ProgressiveMap" + nextMap);
            }

            if (location.Type == LocationType.Skill || location.Type == LocationType.Map)
                RandomizerManager.Receiver.UpdateGoal();

            if (Connected)
            {
                long locationId = session.Locations.GetLocationIdFromName(GAME_NAME, location.Name);
                Task.Factory.StartNew(() => session.Locations.CompleteLocationChecks(locationId));
                Console.WriteLine("Checked " + location);
            }
            else
            {
                Console.WriteLine("Checked " + location + " but not connected");
            }

            RandomizerManager.Receiver.CheckLocation(location.Name);
            UpdateMapLocations();
        }

        public IEnumerable<string> GetArchipelagoCheckedLocations()
        {
            return session.Locations.AllLocationsChecked.Select(d => session.Locations.GetLocationNameFromId(d));
        }
        /**
         * Check location using MoonGuid
         */
        public void CheckLocation(MoonGuid guid)
        {
            CheckLocation(LocationLookup.Get(guid));
        }

        /**
         * Check location using name
         */
        public void CheckLocation(string name)
        {
            CheckLocation(LocationLookup.Get(name));
        }

        /**
         * Sends all locally checked locations to the archipelago server in case any were missed
         */
        private void RecheckLocations()
        {
            IEnumerable<long> ids = RandomizerManager.Receiver.GetAllLocations()
                .Select(x => session.Locations.GetLocationIdFromName(GAME_NAME, x));
            session.Locations.CompleteLocationChecks(ids.ToArray());
        }


        /**
         * Updates the datastorage value with map locations checked while using progressive mapstones
         * This way, trackers can still see which map locations have been checked
         */
        private void UpdateMapLocations()
        {
            List<string> foundMaps = new List<string>();

            foreach (string mapLocation in mapLocations)
            {
                if (RandomizerManager.Receiver.IsLocationChecked(mapLocation))
                {
                    foundMaps.Add(mapLocation);
                }
            }

            // Stores mapLocations array to DataStorage key "Slot:<slot_number>:MapLocations"
            Task.Factory.StartNew(() => session.DataStorage[Scope.Slot, MAP_LOCATION_DATA_KEY] = foundMaps.ToArray());
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
         * Called when the player dies
         */
        public void OnDeath(bool instakill = false)
        {
            UpdateMapLocations();

            // assume damage that is over 100 is meant to be an insta kill
            if (RandomizerManager.Options.DeathLinkLogic == DeathLinkOptions.Full ||
                RandomizerManager.Options.DeathLinkLogic == DeathLinkOptions.Partial && !instakill)
            {
                SendDeathLink();
            }
        }

        /**
         * Sends a death link to the archipelago server
         */
        private void SendDeathLink()
        {
            if (!ignoreNextDeath)
            {
                Task.Factory.StartNew(() => deathLinkService.SendDeathLink(
                    new DeathLink(slotName, slotName + " perished in the Blind Forest")));
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
        public bool IsGoalComplete(bool showCompletionMessage = true)
        {
            if (!Connected)
                return false;

            bool hasMetGoal = CheckGoalCompletion(out StringBuilder message);
            if (showCompletionMessage)
                RandomizerMessager.instance.AddMessage(message.ToString());
            return hasMetGoal;
        }

        private bool CheckGoalCompletion(out StringBuilder message)
        {
            message = new StringBuilder();

            switch (RandomizerManager.Options.Goal)
            {
                case GoalOptions.AllSkillTrees:
                    return CheckAllSkillTreesGoal(message);
                case GoalOptions.AllMaps:
                    return CheckAllMapsGoal(message);
                case GoalOptions.WarmthFragments:
                    return CheckWarmthFragmentsGoal(message);
                case GoalOptions.WorldTour:
                    return CheckWorldTourGoal(message);
                default:
                    return true;
            }
        }

        private bool CheckAllSkillTreesGoal(StringBuilder message)
        {
            int countTrees = 0;
            List<string> uncheckedTrees = new List<string>();

            foreach (string goalLocation in skillTreeLocations)
            {
                if (!RandomizerManager.Receiver.IsLocationChecked(goalLocation))
                {
                    uncheckedTrees.Add(goalLocation);
                }
                else
                {
                    countTrees++;
                }
            }

            AppendSkillTreesMessage(message, countTrees, uncheckedTrees);
            return uncheckedTrees.Count == 0;
        }

        private void AppendSkillTreesMessage(StringBuilder message, int countTrees, List<string> uncheckedTrees)
        {
            message.Append($"{countTrees} out of 10 trees checked. \n");
            if (uncheckedTrees.Count > 0)
            {
                message.Append("Missing ");
                foreach (string tree in uncheckedTrees)
                {
                    message.Append(tree).Append(", ");
                }
                message.Remove(message.Length - 2, 2); // remove last comma
            }
        }

        private bool CheckAllMapsGoal(StringBuilder message)
        {
            int countMaps = 0;
            List<string> uncheckedMaps = new List<string>();

            foreach (string goalLocation in mapLocations)
            {
                if (!RandomizerManager.Receiver.IsLocationChecked(goalLocation))
                {
                    uncheckedMaps.Add(goalLocation);
                }
                else
                {
                    countMaps++;
                }
            }

            AppendMapsMessage(message, countMaps, uncheckedMaps);
            return uncheckedMaps.Count == 0;
        }

        private void AppendMapsMessage(StringBuilder message, int countMaps, List<string> uncheckedMaps)
        {
            message.Append($"{countMaps} out of 9 maps checked. \n");
            if (uncheckedMaps.Count > 0)
            {
                message.Append("Missing ");
                foreach (string map in uncheckedMaps)
                {
                    message.Append(map).Append(", ");
                }
                message.Remove(message.Length - 2, 2); // remove last comma
            }
        }

        private bool CheckWarmthFragmentsGoal(StringBuilder message)
        {
            int collectedWarmthFragments = RandomizerManager.Receiver.GetItemCount(InventoryItem.WarmthFragment);
            int requiredWarmthFragments = RandomizerManager.Options.WarmthFragmentsRequired;
            int availableWarmthFragments = RandomizerManager.Options.WarmthFragmentsAvailable;

            bool goalComplete = collectedWarmthFragments >= requiredWarmthFragments;

            AppendWarmthFragmentsMessage(message, collectedWarmthFragments, requiredWarmthFragments, availableWarmthFragments, goalComplete);
            return goalComplete;
        }

        private void AppendWarmthFragmentsMessage(StringBuilder message, int collected, int required, int available, bool goalComplete)
        {
            message.Append($"Collected {collected} out of {required} warmth fragments needed. \n");
            if (!goalComplete)
            {
                message.Append($"{available - collected} remain in multiworld");
            }
        }

        private bool CheckWorldTourGoal(StringBuilder message)
        {
            int collectedRelics = RandomizerManager.Receiver.GetItemCount(InventoryItem.Relic);
            int requiredRelics = RandomizerManager.Options.RelicCount;
            WorldArea[] relicAreas = RandomizerManager.Options.WorldTourAreas;

            bool goalComplete = collectedRelics >= requiredRelics;

            AppendWorldTourMessage(message, collectedRelics, requiredRelics, relicAreas, goalComplete);
            return goalComplete;
        }

        private void AppendWorldTourMessage(StringBuilder message, int collected, int required, WorldArea[] relicAreas, bool goalComplete)
        {
            message.Append($"Collected {collected} out of {required} relics.");

            if (!goalComplete)
            {
                Task.Factory.StartNew(() =>
                session.DataStorage[Scope.Slot, FOUND_RELICS_DATA_KEY].GetAsync<string[]>(x =>
                {
                    StringBuilder relicMessage = new StringBuilder();

                    relicMessage.Append($"Remaining relics can be found in ");
                    foreach (WorldArea area in relicAreas)
                    {
                        string[] areas = x;
                        if (!areas.Contains(area.ToString()))
                        {
                            relicMessage.Append(area).Append(", ");
                        }
                    }
                    relicMessage.Remove(relicMessage.Length - 2, 2); // remove last comma

                    RandomizerMessager.instance.AddMessage(relicMessage.ToString());
                }));
            }
        }

        internal void StoreSeinInArchipelagoSession()
        {
            Task.Factory.StartNew(() => session.DataStorage[Scope.Slot, SEIN] = true);
        }

        internal bool IsSeinCollected()
        {            
            var sein = session.DataStorage[Scope.Slot, SEIN].To<bool>();
            return sein;
        }

        /// <summary>
        /// This function sets all important goal locations into the RandomizerOptions class
        /// </summary>
        internal void SetGoalLocationsInOptions()
        {
            session.DataStorage[Scope.Slot, GOAL_LOCATION_DATA_KEY].GetAsync<string[]>(x =>
            {
                if (x != null && x.Any())
                {
                    ModLogger.Debug("Collected goal locations from session");
                    RandomizerManager.Options.GoalLocations = x;
                }
                else
                {
                    session.Locations.ScoutLocationsAsync((results) =>
                    {
                        var locations = new string[0];
                        foreach (var result in results)
                        {
                            if (result.Value.ItemName.Equals(InventoryItem.WarmthFragment.ToString(), StringComparison.CurrentCultureIgnoreCase) || result.Value.ItemName.Equals(InventoryItem.Relic.ToString(), StringComparison.CurrentCultureIgnoreCase) && !locations.Any(d => d == result.Value.LocationName))
                                locations = [.. locations, result.Value.LocationName];
                        }

                        ModLogger.Debug("Collected locations from archipelago");
                        Task.Factory.StartNew(() => session.DataStorage[Scope.Slot, GOAL_LOCATION_DATA_KEY] = locations);
                        RandomizerManager.Options.GoalLocations = locations;


                    }, HintCreationPolicy.None, session.Locations.AllLocations.ToArray());


                }
            });
        }
    }
}

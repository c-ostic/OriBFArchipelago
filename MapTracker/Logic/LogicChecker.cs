using OriBFArchipelago.Core;
using OriBFArchipelago.MapTracker.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OriBFArchipelago.MapTracker.Logic
{
    internal class LogicChecker
    {
        private Dictionary<string, Dictionary<string, Dictionary<string, List<List<string>>>>> _logic;
        //private OriOptions _options;

        public LogicChecker()
        {
            _logic = new RulesData().GetFullLogic();
            //_options = options ?? new OriOptions();
        }

        /// <summary>
        /// Find the container location for a specific pickup
        /// </summary>
        public string FindPickupLocation(string pickupName)
        {
            foreach (var location in _logic)
            {
                if (location.Value.ContainsKey(pickupName))
                {
                    return location.Key;
                }
            }
            return null;
        }

        /// <summary>
        /// Check if a pickup is accessible with the given inventory at a specific difficulty level
        /// </summary>
        public bool IsPickupAccessible(string pickupName, Dictionary<string, int> inventory, DifficultyOptions difficultyLevel)
        {
            string location = FindPickupLocation(pickupName);
            if (location == null)
            {
                ModLogger.Debug($"Pickup {pickupName} not found in any location");
                return false;
            }

            // First check if we can reach the location containing the pickup
            if (!IsLocationReachable(location, inventory, "SunkenGladesRunaway", difficultyLevel))
                return false;

            // Now check if we can access the pickup itself
            if (!CanAccess(location, pickupName, inventory, difficultyLevel))
                return false;

            return true;
        }
        /// <summary>
        /// Check if a location is reachable with the given inventory at a specific difficulty level
        /// </summary>
        public bool IsLocationReachable(string targetLocation, Dictionary<string, int> inventory, string startLocation, DifficultyOptions difficultyLevel)
        {
            if (targetLocation == startLocation)
                return true;

            HashSet<string> visited = new HashSet<string>();
            Queue<string> queue = new Queue<string>();

            visited.Add(startLocation);
            queue.Enqueue(startLocation);

            while (queue.Count > 0)
            {
                string current = queue.Dequeue();

                if (_logic.ContainsKey(current))
                {
                    foreach (var connection in _logic[current])
                    {
                        string destination = connection.Key;

                        // Only consider connections to other locations
                        if (_logic.ContainsKey(destination) && !visited.Contains(destination))
                        {
                            if (CanAccess(current, destination, inventory, difficultyLevel))
                            {
                                if (destination == targetLocation)
                                    return true;

                                visited.Add(destination);
                                queue.Enqueue(destination);
                            }
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Check if a destination can be accessed from a location with the given inventory at a specific difficulty level
        /// </summary>
        private bool CanAccess(string fromLocation, string toDestination, Dictionary<string, int> inventory, DifficultyOptions difficultyLevel)
        {
            //            ModLogger.Debug($"Checking {fromLocation} - {toDestination} - {difficultyLevel}");
            if (!_logic.ContainsKey(fromLocation) || !_logic[fromLocation].ContainsKey(toDestination))
                return false;

            var requirementSets = GetDifficultyRequirements(fromLocation, toDestination, difficultyLevel);

            foreach (var reqSet in requirementSets)
            {
                //ModLogger.Debug(string.Join(", ", reqSet.ToArray()));
                bool canSatisfySet = true;
                foreach (var req in reqSet)
                {
                    if (!CanSatisfyRequirement(req, inventory))
                    {
                        canSatisfySet = false;
                        break;
                    }
                }

                if (canSatisfySet)
                    return true;
            }

            return false;
        }

        private List<List<string>> GetDifficultyRequirements(string fromLocation, string toDestination, DifficultyOptions maxDifficulty)
        {
            var requirements = new List<List<string>>();
            var locationRequirements = _logic[fromLocation][toDestination];
            var allDifficulties = Enum.GetValues(typeof(DifficultyOptions)).Cast<DifficultyOptions>().ToArray();

            // Filter to include only values less than or equal to the starting difficulty
            var filteredDifficulties = allDifficulties.Where(d => d <= maxDifficulty).ToArray();

            // Loop through the filtered values

            foreach (var difficulty in filteredDifficulties)
            {
                var loweredDifficulty = difficulty.ToString().ToLower();
                if (!locationRequirements.ContainsKey(loweredDifficulty))
                {
                    //ModLogger.Debug($"{fromLocation} does not contain {difficulty}");
                    continue;
                }

                var difficultiyRequirements = locationRequirements[loweredDifficulty];
                if (difficultiyRequirements != null && difficultiyRequirements.Any())
                    requirements.AddRange(difficultiyRequirements);
            }

            return requirements;
        }


        /// <summary>
        /// Check if a specific requirement can be satisfied with the given inventory
        /// </summary>
        private bool CanSatisfyRequirement(string requirement, Dictionary<string, int> inventory)
        {
            // Special cases
            if (requirement == "None")
                return false;

            if (requirement == "Free" || requirement == "Open")
                return true;

            if (requirement == "OpenWorld")
                return false; // Not implemented

            // Handle numeric requirements (e.g., HealthCell:3)
            if (requirement.Contains(":"))
            {
                var parts = requirement.Split(':');
                var itemName = parts[0];
                var count = int.Parse(parts[1]);

                // Special case for HealthCell - only count if damage boost is enabled
                if (itemName == "HealthCell" && !RandomizerManager.Options.EnableDamageBoost)
                    return false;

                return inventory.ContainsKey(itemName) && inventory[itemName] >= count;
            }

            // Handle special abilities
            switch (requirement)
            {
                case "Lure":
                    return RandomizerManager.Options.EnableLure;

                case "DoubleBash":
                    return RandomizerManager.Options.EnableDoubleBash && HasItem("Bash", inventory);

                case "GrenadeJump":
                    return RandomizerManager.Options.EnableGrenadeJump &&
                           HasItem("Climb", inventory) &&
                           HasItem("ChargeJump", inventory) &&
                           HasItem("Grenade", inventory);

                case "ChargeFlameBurn":
                    return RandomizerManager.Options.EnableChargeFlame &&
                           HasItem("ChargeFlame", inventory) &&
                           HasItem("AbilityCell", inventory, 3);

                case "ChargeDash":
                case "RocketJump":
                    return RandomizerManager.Options.EnableChargeDash &&
                           HasItem("Dash", inventory) &&
                           HasItem("AbilityCell", inventory, 6);

                case "AirDash":
                    return RandomizerManager.Options.EnableAirDash &&
                           HasItem("Dash", inventory) &&
                           HasItem("AbilityCell", inventory, 3);

                case "TripleJump":
                    return RandomizerManager.Options.EnableTripleJump &&
                           HasItem("DoubleJump", inventory) &&
                           HasItem("AbilityCell", inventory, 12);

                case "UltraDefense":
                    return RandomizerManager.Options.EnableDamageBoost &&
                           HasItem("AbilityCell", inventory, 12);

                case "BashGrenade":
                    return HasItem("Bash", inventory) && HasItem("Grenade", inventory);

                case "Rekindle":
                    return RandomizerManager.Options.EnableRekindle;

                default:
                    // Normal abilities like Dash, Climb, etc.
                    return HasItem(requirement, inventory);
            }
        }

        private bool HasItem(string itemName, Dictionary<string, int> inventory, int count = 1)
        {
            return inventory.ContainsKey(itemName) && inventory[itemName] >= count;
        }

        /// <summary>
        /// Gets a list of all collectable items/pickups in the logic
        /// </summary>
        public List<string> GetAllCollectableItems()
        {
            var collectables = new HashSet<string>();

            foreach (var location in _logic)
            {
                foreach (var connection in location.Value)
                {
                    string destination = connection.Key;

                    // If this destination is not a location itself, it's likely a pickup
                    if (!_logic.ContainsKey(destination))
                    {
                        collectables.Add(destination);
                    }
                }
            }

            var result = collectables.ToList();
            result.Sort();
            return result;
        }
    }
}

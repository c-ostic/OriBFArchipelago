using OriBFArchipelago.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OriBFArchipelago.MapTracker.Logic
{
    internal class LogicChecker
    {
        private Dictionary<string, Dictionary<string, Dictionary<string, List<List<string>>>>> _logic;
        //private OriOptions _options;

        // The logical starting area for all reachability checks.
        private const string StartLocation = "SunkenGladesRunaway";

        public LogicChecker()
        {
            _logic = new RulesDataReader().GetFullLogic();

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
        /// Gathers the collective requirements to reach AND collect a pickup: it traces the
        /// least-blocked route from the logical start to the pickup's container area (preferring
        /// edges you can already satisfy, only crossing blocked ones when unavoidable), then
        /// unions the requirements chosen along that route with the pickup's own access edge.
        /// Returns the deduped list of raw requirement tokens (e.g. "Climb", "GinsoKey",
        /// "HealthCell:4"); the caller formats/colors them. Empty if the pickup has no rules or
        /// no route exists.
        /// </summary>
        public List<string> GetCollectiveRequirements(string pickupName, DifficultyOptions difficultyLevel, Dictionary<string, int> inventory, RandomizerOptions options)
        {
            string container = FindPickupLocation(pickupName);
            if (container == null)
                return new List<string>();

            var collected = new List<string>();

            // Requirements along the route from the start area to the container area.
            foreach (var edge in FindLeastBlockedPath(StartLocation, container, difficultyLevel, inventory, options))
            {
                EdgeUnmetCost(edge.Key, edge.Value, difficultyLevel, inventory, options, out var bestSet);
                if (bestSet != null)
                    collected.AddRange(bestSet);
            }

            // The pickup's own access edge.
            EdgeUnmetCost(container, pickupName, difficultyLevel, inventory, options, out var finalSet);
            if (finalSet != null)
                collected.AddRange(finalSet);

            return Consolidate(collected);
        }

        /// <summary>
        /// Dijkstra from start to target over area-to-area edges, where an edge's cost is the
        /// smallest number of currently-unsatisfied requirements among its alternatives (0 if
        /// already satisfiable). Returns the chosen route as an ordered list of (from, to) hops,
        /// or an empty list if start == target or no route exists.
        /// </summary>
        private List<KeyValuePair<string, string>> FindLeastBlockedPath(string start, string target, DifficultyOptions difficulty, Dictionary<string, int> inventory, RandomizerOptions options)
        {
            var path = new List<KeyValuePair<string, string>>();
            if (target == start || !_logic.ContainsKey(start))
                return path;

            var dist = new Dictionary<string, int>();
            var prev = new Dictionary<string, string>();
            var visited = new HashSet<string>();
            dist[start] = 0;

            while (true)
            {
                // Pick the unvisited node with the smallest known distance.
                string current = null;
                int best = int.MaxValue;
                foreach (var kv in dist)
                {
                    if (visited.Contains(kv.Key) || kv.Value >= best)
                        continue;
                    best = kv.Value;
                    current = kv.Key;
                }

                if (current == null)
                    break;
                if (current == target)
                    break;
                visited.Add(current);

                if (!_logic.ContainsKey(current))
                    continue;

                foreach (var connection in _logic[current])
                {
                    string dest = connection.Key;
                    // Only traverse edges to other areas (not pickups).
                    if (!_logic.ContainsKey(dest) || visited.Contains(dest))
                        continue;

                    int weight = EdgeUnmetCost(current, dest, difficulty, inventory, options, out _);
                    if (weight == int.MaxValue)
                        continue;

                    int candidate = dist[current] + weight;
                    if (!dist.ContainsKey(dest) || candidate < dist[dest])
                    {
                        dist[dest] = candidate;
                        prev[dest] = current;
                    }
                }
            }

            if (!prev.ContainsKey(target))
                return path; // no route found

            // Reconstruct start -> target and return as ordered hops.
            var chain = new List<string>();
            string node = target;
            while (node != start && prev.ContainsKey(node))
            {
                chain.Add(node);
                node = prev[node];
            }
            chain.Add(start);
            chain.Reverse();

            for (int i = 0; i + 1 < chain.Count; i++)
                path.Add(new KeyValuePair<string, string>(chain[i], chain[i + 1]));

            return path;
        }

        /// <summary>
        /// Cost of an edge = the fewest currently-unsatisfied requirements among its alternative
        /// sets (0 if satisfiable now). Impossible sets (containing "None"/"OpenWorld") are
        /// skipped. Outputs the chosen (lowest-cost) requirement set. Returns int.MaxValue if
        /// there is no viable set.
        /// </summary>
        private int EdgeUnmetCost(string from, string to, DifficultyOptions difficulty, Dictionary<string, int> inventory, RandomizerOptions options, out List<string> bestSet)
        {
            bestSet = null;
            if (!_logic.ContainsKey(from) || !_logic[from].ContainsKey(to))
                return int.MaxValue;

            int best = int.MaxValue;
            foreach (var set in GetDifficultyRequirements(from, to, difficulty))
            {
                int unmet = 0;
                bool impossible = false;
                foreach (var req in set)
                {
                    if (req == "None" || req == "OpenWorld" || IsIrrelevantStone(req, options))
                    {
                        // Not applicable under the current settings (e.g. a generic MapStone
                        // requirement while area-specific mapstone logic is active). Skip the set
                        // so the display picks the variant that actually matches the settings.
                        impossible = true;
                        break;
                    }
                    if (LogicRequirementFormatter.IsMeta(req))
                        continue; // Free/Open - always satisfied, no cost
                    if (!CanSatisfyRequirement(req, inventory, options))
                        unmet++;
                }

                if (impossible)
                    continue;
                if (unmet < best)
                {
                    best = unmet;
                    bestSet = set;
                }
                if (best == 0)
                    break;
            }

            return best;
        }

        /// <summary>
        /// True when a keystone/mapstone requirement variant does not match the active settings:
        /// the generic "KeyStone"/"MapStone" tokens only apply under Anywhere logic, and the
        /// area-specific variants (e.g. "ValleyMapStone", "GladesKeyStone") only apply otherwise.
        /// Used to pick the requirement variant the player actually plays with.
        /// </summary>
        private static bool IsIrrelevantStone(string requirement, RandomizerOptions options)
        {
            int colon = requirement.IndexOf(':');
            string name = colon >= 0 ? requirement.Substring(0, colon) : requirement;

            if (name == "MapStone")
                return options.MapStoneLogic != MapStoneOptions.Anywhere;
            if (name.EndsWith("MapStone"))
                return options.MapStoneLogic == MapStoneOptions.Anywhere;

            if (name == "KeyStone")
                return options.KeyStoneLogic != KeyStoneOptions.Anywhere;
            if (name.EndsWith("KeyStone"))
                return options.KeyStoneLogic == KeyStoneOptions.Anywhere;

            return false;
        }

        /// <summary>
        /// Dedupes a flat list of requirement tokens, dropping meta tokens and, for counted
        /// resources with the same name (e.g. "HealthCell:3" and "HealthCell:4"), keeping the
        /// highest count.
        /// </summary>
        private List<string> Consolidate(List<string> tokens)
        {
            var plain = new List<string>();
            var counted = new Dictionary<string, int>();
            var countedOrder = new List<string>();

            foreach (var token in tokens)
            {
                if (LogicRequirementFormatter.IsMeta(token))
                    continue;

                int colon = token.IndexOf(':');
                if (colon >= 0)
                {
                    string name = token.Substring(0, colon);
                    int value;
                    if (!int.TryParse(token.Substring(colon + 1), out value))
                        value = 0;

                    if (!counted.ContainsKey(name))
                    {
                        counted[name] = value;
                        countedOrder.Add(name);
                    }
                    else if (value > counted[name])
                    {
                        counted[name] = value;
                    }
                }
                else if (!plain.Contains(token))
                {
                    plain.Add(token);
                }
            }

            var result = new List<string>(plain);
            foreach (var name in countedOrder)
                result.Add($"{name}:{counted[name]}");
            return result;
        }

        /// <summary>
        /// Check whether a single requirement token is satisfied by the given inventory/options.
        /// Exposed so the UI can color each requirement met/unmet.
        /// </summary>
        public bool IsRequirementSatisfied(string requirement, Dictionary<string, int> inventory, RandomizerOptions options)
        {
            return CanSatisfyRequirement(requirement, inventory, options);
        }

        /// <summary>
        /// Check if a pickup is accessible with the given inventory at a specific difficulty level
        /// </summary>
        public bool IsPickupAccessible(string pickupName, DifficultyOptions difficultyLevel, Dictionary<string, int> inventory, RandomizerOptions options)
        {
            string location = FindPickupLocation(pickupName);
            if (location == null)
                return false;

            // First check if we can reach the location containing the pickup
            if (!IsLocationReachable(location, StartLocation, difficultyLevel, inventory, options))
                return false;

            // Now check if we can access the pickup itself
            if (!CanAccess(location, pickupName, difficultyLevel, inventory, options))
                return false;

            return true;
        }
        /// <summary>
        /// Check if a location is reachable with the given inventory at a specific difficulty level
        /// </summary>
        public bool IsLocationReachable(string targetLocation, string startLocation, DifficultyOptions difficultyLevel, Dictionary<string, int> inventory, RandomizerOptions options)
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
                            if (CanAccess(current, destination, difficultyLevel, inventory, options))
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
        private bool CanAccess(string fromLocation, string toDestination, DifficultyOptions difficultyLevel, Dictionary<string, int> inventory, RandomizerOptions options)
        {
            if (!_logic.ContainsKey(fromLocation) || !_logic[fromLocation].ContainsKey(toDestination))
                return false;

            var requirementSets = GetDifficultyRequirements(fromLocation, toDestination, difficultyLevel);

            foreach (var reqSet in requirementSets)
            {
                bool canSatisfySet = true;
                foreach (var req in reqSet)
                {
                    if (!CanSatisfyRequirement(req, inventory, options))
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
                    continue;

                var difficultiyRequirements = locationRequirements[loweredDifficulty];
                if (difficultiyRequirements != null && difficultiyRequirements.Any())
                    requirements.AddRange(difficultiyRequirements);
            }

            return requirements;
        }


        /// <summary>
        /// Check if a specific requirement can be satisfied with the given inventory
        /// </summary>
        private bool CanSatisfyRequirement(string requirement, Dictionary<string, int> inventory, RandomizerOptions options)
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

                if (itemName == "HealthCell")
                {
                    // Special case for HealthCell - only count if damage boost is enabled
                    if (!options.EnableDamageBoost)
                        return false;

                }
                return inventory.ContainsKey(itemName) && inventory[itemName] >= count;
            }

            // Handle special abilities
            switch (requirement)
            {
                case "Lure":
                    return options.EnableLure;

                case "DoubleBash":
                    return options.EnableDoubleBash && HasItem("Bash", inventory);

                case "GrenadeJump":
                    return options.EnableGrenadeJump &&
                           HasItem("Climb", inventory) &&
                           HasItem("ChargeJump", inventory) &&
                           HasItem("Grenade", inventory);

                case "ChargeFlameBurn":
                    return options.EnableChargeFlame &&
                           HasItem("ChargeFlame", inventory) &&
                           HasItem("AbilityCell", inventory, 3);

                case "ChargeDash":
                case "RocketJump":
                    return options.EnableChargeDash &&
                           HasItem("Dash", inventory) &&
                           HasItem("AbilityCell", inventory, 6);

                case "AirDash":
                    return options.EnableAirDash &&
                           HasItem("Dash", inventory) &&
                           HasItem("AbilityCell", inventory, 3);

                case "TripleJump":
                    return options.EnableTripleJump &&
                           HasItem("DoubleJump", inventory) &&
                           HasItem("AbilityCell", inventory, 12);

                case "UltraDefense":
                    return options.EnableDamageBoost &&
                           HasItem("AbilityCell", inventory, 12);

                case "BashGrenade":
                    return HasItem("Bash", inventory) && HasItem("Grenade", inventory);

                case "Rekindle":
                    return options.EnableRekindle;

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

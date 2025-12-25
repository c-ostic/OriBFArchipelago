using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System;

namespace OriBFArchipelago.MapTracker.Logic
{
    public class RulesDataReader
    {
        
        public Dictionary<string, Dictionary<string, Dictionary<string, List<List<string>>>>> GetFullLogic()
        {
            var rulesData = new RulesData();

            var connections = ParseJsonFile(rulesData.GetConnectionRules());
            var pickups = ParseJsonFile(rulesData.GetLocationRules());
            return MergeLogicFiles(connections, pickups);
        }

        private static Dictionary<string, Dictionary<string, Dictionary<string, List<List<string>>>>> ParseJsonFile(string jsonContent)
        {
            try
            {
                // Parse the JSON as a JObject
                JObject rootObject = JObject.Parse(jsonContent);
                var result = new Dictionary<string, Dictionary<string, Dictionary<string, List<List<string>>>>>();

                // Iterate through each location
                foreach (var locationProperty in rootObject.Properties())
                {
                    string locationName = locationProperty.Name;
                    JToken locationValue = locationProperty.Value;

                    // Create entry for this location
                    result[locationName] = new Dictionary<string, Dictionary<string, List<List<string>>>>();

                    // Skip if null or not an object
                    if (locationValue == null || locationValue.Type != JTokenType.Object)
                        continue;

                    // Process each destination/pickup in this location
                    foreach (var destinationProperty in ((JObject)locationValue).Properties())
                    {
                        string destinationName = destinationProperty.Name;
                        JToken destinationValue = destinationProperty.Value;

                        // Initialize the dictionary for this destination
                        result[locationName][destinationName] = new Dictionary<string, List<List<string>>>();

                        // Process based on the structure
                        if (destinationValue.Type == JTokenType.Object)
                        {
                            // This contains the difficulty levels (casual, expert, etc.)
                            var difficultyObject = (JObject)destinationValue;

                            foreach (var difficultyProperty in difficultyObject.Properties())
                            {
                                string difficultyName = difficultyProperty.Name;

                                // Initialize the requirement sets for this difficulty
                                result[locationName][destinationName][difficultyName] = new List<List<string>>();

                                // Each difficulty has an array of requirement sets
                                if (difficultyProperty.Value.Type == JTokenType.Array)
                                {
                                    var difficultyArray = (JArray)difficultyProperty.Value;

                                    foreach (var requirementSet in difficultyArray)
                                    {
                                        // Each requirement set is an array of requirements
                                        if (requirementSet.Type == JTokenType.Array)
                                        {
                                            var requirements = new List<string>();
                                            foreach (var requirement in requirementSet)
                                            {
                                                requirements.Add(requirement.ToString());
                                            }
                                            result[locationName][destinationName][difficultyName].Add(requirements);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing JSON with difficulty: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                return new Dictionary<string, Dictionary<string, Dictionary<string, List<List<string>>>>>();
            }
        }

        public static Dictionary<string, Dictionary<string, Dictionary<string, List<List<string>>>>> MergeLogicFiles(
            Dictionary<string, Dictionary<string, Dictionary<string, List<List<string>>>>> connections,
            Dictionary<string, Dictionary<string, Dictionary<string, List<List<string>>>>> pickups)
        {
            // Create a new dictionary for the merged content
            var merged = new Dictionary<string, Dictionary<string, Dictionary<string, List<List<string>>>>>();

            // Copy all connections
            foreach (var location in connections)
            {
                string locationName = location.Key;
                merged[locationName] = new Dictionary<string, Dictionary<string, List<List<string>>>>();

                foreach (var destination in location.Value)
                {
                    string destinationName = destination.Key;
                    merged[locationName][destinationName] = new Dictionary<string, List<List<string>>>();

                    foreach (var difficulty in destination.Value)
                    {
                        string difficultyName = difficulty.Key;
                        merged[locationName][destinationName][difficultyName] = new List<List<string>>(difficulty.Value);
                    }
                }
            }

            // Add all pickups
            foreach (var location in pickups)
            {
                string locationName = location.Key;

                // If the location doesn't exist in the merged dictionary, add it
                if (!merged.ContainsKey(locationName))
                {
                    merged[locationName] = new Dictionary<string, Dictionary<string, List<List<string>>>>();
                }

                // Add all pickups for this location
                foreach (var pickup in location.Value)
                {
                    string pickupName = pickup.Key;
                    merged[locationName][pickupName] = new Dictionary<string, List<List<string>>>();

                    foreach (var difficulty in pickup.Value)
                    {
                        string difficultyName = difficulty.Key;
                        merged[locationName][pickupName][difficultyName] = new List<List<string>>(difficulty.Value);
                    }
                }
            }

            return merged;
        }
    }
}
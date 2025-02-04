using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace OriBFArchipelago.Core
{
    internal enum GoalOptions
    {
        AllSkillTrees = 0,
        AllMaps = 1,
        WarmthFragments = 2,
        WorldTour = 3,
        None = 4
    }

    internal enum DifficultyOptions
    {
        Casual = 0,
        Standard = 1,
        Expert = 2,
        Master = 3,
        Glitched = 4
    }

    internal enum KeyStoneOptions
    {
        Anywhere = 0,
        AreaSpecific = 1
    }

    internal enum MapStoneOptions
    {
        Anywhere = 0,
        AreaSpecific = 1,
        Progressive = 2
    }

    internal enum DeathLinkOptions
    {
        Disabled = 0,
        Partial = 1,
        Full = 2
    }

    internal class RandomizerOptions
    {
        public GoalOptions Goal { get; }
        public int WarmthFragmentsAvailable { get; }
        public int WarmthFragmentsRequired { get; }
        public int RelicCount { get; }
        public WorldArea[] WorldTourAreas { get; }
        public DifficultyOptions LogicDifficulty { get; }
        public KeyStoneOptions KeyStoneLogic { get; }
        public MapStoneOptions MapStoneLogic { get; }
        public DeathLinkOptions DeathLinkLogic { get; }

        public RandomizerOptions(Dictionary<string, object> apSlotData) {
            if (apSlotData != null)
            {
                Goal = apSlotData.TryGetValue("goal", out object goalOption) ? (GoalOptions) Enum.ToObject(typeof(GoalOptions), goalOption) : GoalOptions.AllSkillTrees;
                WarmthFragmentsAvailable = apSlotData.TryGetValue("warmth_fragments_available", out object warmthFragmentsAvailable) ? Convert.ToInt32(warmthFragmentsAvailable) : 0;
                WarmthFragmentsRequired = apSlotData.TryGetValue("warmth_fragments_required", out object warmthFragmentsRequired) ? Convert.ToInt32(warmthFragmentsRequired) : 0;
                RelicCount = apSlotData.TryGetValue("relic_count", out object relicCount) ? Convert.ToInt32(relicCount) : 0;
                WorldTourAreas = apSlotData.TryGetValue("world_tour_areas", out object worldTourAreas) ? JsonConvert.DeserializeObject<WorldArea[]>(worldTourAreas.ToString()) : [];
                LogicDifficulty = apSlotData.TryGetValue("logic_difficulty", out object difficultyOption) ? (DifficultyOptions) Enum.ToObject(typeof(DifficultyOptions), difficultyOption) : DifficultyOptions.Casual;
                KeyStoneLogic = apSlotData.TryGetValue("keystone_logic", out object keystoneOption) ? (KeyStoneOptions) Enum.ToObject(typeof(KeyStoneOptions), keystoneOption) : KeyStoneOptions.Anywhere;
                MapStoneLogic = apSlotData.TryGetValue("mapstone_logic", out object mapstoneOption) ? (MapStoneOptions) Enum.ToObject(typeof(MapStoneOptions), mapstoneOption) : MapStoneOptions.Anywhere;
                DeathLinkLogic = apSlotData.TryGetValue("deathlink_logic", out object deathlinkOption) ? (DeathLinkOptions)Enum.ToObject(typeof(DeathLinkOptions), deathlinkOption) : DeathLinkOptions.Disabled;
            }
            else
            {
                Goal = GoalOptions.AllSkillTrees;
                WarmthFragmentsAvailable = 0;
                WarmthFragmentsRequired = 0;
                RelicCount = 0;
                WorldTourAreas = [];
                LogicDifficulty = DifficultyOptions.Casual;
                KeyStoneLogic = KeyStoneOptions.Anywhere;
                MapStoneLogic = MapStoneOptions.Anywhere;
                DeathLinkLogic = DeathLinkOptions.Disabled;
            }
        }
    }
}

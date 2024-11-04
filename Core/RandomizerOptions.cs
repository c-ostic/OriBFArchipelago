using System;
using System.Collections.Generic;

namespace OriBFArchipelago.Core
{
    internal enum GoalOptions
    {
        AllSkillTrees = 0
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
        AreaSpecific = 1
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
        public DifficultyOptions LogicDifficulty { get; }
        public KeyStoneOptions KeyStoneLogic { get; }
        public MapStoneOptions MapStoneLogic { get; }
        public DeathLinkOptions DeathLinkLogic { get; }

        public RandomizerOptions(Dictionary<string, object> apSlotData) {
            if (apSlotData != null)
            {
                Goal = apSlotData.TryGetValue("goal", out object goalOption) ? (GoalOptions) Enum.ToObject(typeof(GoalOptions), goalOption) : GoalOptions.AllSkillTrees;
                LogicDifficulty = apSlotData.TryGetValue("logic_difficulty", out object difficultyOption) ? (DifficultyOptions) Enum.ToObject(typeof(DifficultyOptions), difficultyOption) : DifficultyOptions.Casual;
                KeyStoneLogic = apSlotData.TryGetValue("keystone_logic", out object keystoneOption) ? (KeyStoneOptions) Enum.ToObject(typeof(KeyStoneOptions), keystoneOption) : KeyStoneOptions.Anywhere;
                MapStoneLogic = apSlotData.TryGetValue("mapstone_logic", out object mapstoneOption) ? (MapStoneOptions) Enum.ToObject(typeof(MapStoneOptions), mapstoneOption) : MapStoneOptions.Anywhere;
                DeathLinkLogic = apSlotData.TryGetValue("deathlink_logic", out object deathlinkOption) ? (DeathLinkOptions)Enum.ToObject(typeof(DeathLinkOptions), deathlinkOption) : DeathLinkOptions.Disabled;
            }
            else
            {
                Goal = GoalOptions.AllSkillTrees;
                LogicDifficulty = DifficultyOptions.Casual;
                KeyStoneLogic = KeyStoneOptions.Anywhere;
                MapStoneLogic = MapStoneOptions.Anywhere;
                DeathLinkLogic = DeathLinkOptions.Disabled;
            }
        }
    }
}

using Newtonsoft.Json;
using OriBFArchipelago.MapTracker.Core;
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
        public bool EnableDamageBoost { get; internal set; }
        public bool EnableLure { get; internal set; }
        public bool EnableDoubleBash { get; internal set; }
        public bool EnableGrenadeJump { get; internal set; }
        public bool EnableChargeFlame { get; internal set; }
        public bool EnableChargeDash { get; internal set; }
        public bool EnableAirDash { get; internal set; }
        public bool EnableTripleJump { get; internal set; }
        public bool EnableRekindle { get; internal set; }
        public string[] GoalLocations { get; set; }

        public RandomizerOptions(Dictionary<string, object> apSlotData)
        {
            try
            {
                if (apSlotData != null)
                {
                    Goal = apSlotData.TryGetValue("goal", out object goalOption) ? (GoalOptions)Enum.ToObject(typeof(GoalOptions), goalOption) : GoalOptions.AllSkillTrees;
                    WarmthFragmentsAvailable = apSlotData.TryGetValue("warmth_fragments_available", out object warmthFragmentsAvailable) ? Convert.ToInt32(warmthFragmentsAvailable) : 0;
                    WarmthFragmentsRequired = apSlotData.TryGetValue("warmth_fragments_required", out object warmthFragmentsRequired) ? Convert.ToInt32(warmthFragmentsRequired) : 0;
                    RelicCount = apSlotData.TryGetValue("relic_count", out object relicCount) ? Convert.ToInt32(relicCount) : 0;
                    WorldTourAreas = apSlotData.TryGetValue("world_tour_areas", out object worldTourAreas) ? JsonConvert.DeserializeObject<WorldArea[]>(worldTourAreas.ToString()) : [];
                    LogicDifficulty = apSlotData.TryGetValue("logic_difficulty", out object difficultyOption) ? (DifficultyOptions)Enum.ToObject(typeof(DifficultyOptions), difficultyOption) : DifficultyOptions.Casual;
                    KeyStoneLogic = apSlotData.TryGetValue("keystone_logic", out object keystoneOption) ? (KeyStoneOptions)Enum.ToObject(typeof(KeyStoneOptions), keystoneOption) : KeyStoneOptions.Anywhere;
                    MapStoneLogic = apSlotData.TryGetValue("mapstone_logic", out object mapstoneOption) ? (MapStoneOptions)Enum.ToObject(typeof(MapStoneOptions), mapstoneOption) : MapStoneOptions.Anywhere;
                    DeathLinkLogic = apSlotData.TryGetValue("deathlink_logic", out object deathlinkOption) ? (DeathLinkOptions)Enum.ToObject(typeof(DeathLinkOptions), deathlinkOption) : DeathLinkOptions.Disabled;

                    EnableLure = apSlotData.TryGetValue("enable_lure", out object enable_lure) ? Convert.ToBoolean(enable_lure) : false;
                    EnableDamageBoost = apSlotData.TryGetValue("enable_damage_boost", out object enable_damage_boost) ? Convert.ToBoolean(enable_damage_boost) : false;

                    EnableDoubleBash = apSlotData.TryGetValue("enable_double_bash", out object enable_double_bash) ? Convert.ToBoolean(enable_double_bash) : false;
                    EnableGrenadeJump = apSlotData.TryGetValue("enable_grenade_jump", out object enable_grenade_jump) ? Convert.ToBoolean(enable_grenade_jump) : false;
                    EnableChargeFlame = apSlotData.TryGetValue("enable_charge_flame_burn", out object enable_charge_flame_burn) ? Convert.ToBoolean(enable_charge_flame_burn) : false;
                    EnableChargeDash = apSlotData.TryGetValue("enable_charge_dash", out object enable_charge_dash) ? Convert.ToBoolean(enable_charge_dash) : false;
                    EnableAirDash = apSlotData.TryGetValue("enable_air_dash", out object enable_air_dash) ? Convert.ToBoolean(enable_air_dash) : false;
                    EnableTripleJump = apSlotData.TryGetValue("enable_triple_jump", out object enable_triple_jump) ? Convert.ToBoolean(enable_triple_jump) : false;
                    EnableRekindle = apSlotData.TryGetValue("enable_rekindle", out object enable_rekindle) ? Convert.ToBoolean(enable_rekindle) : false;
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
            catch(System.Exception ex)
            {
                ModLogger.Error($"{ex}");
            }
        }
    }
}

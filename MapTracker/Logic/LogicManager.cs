using OriBFArchipelago.Core;
using OriBFArchipelago.MapTracker.Core;
using System;

namespace OriBFArchipelago.MapTracker.Logic
{
    /// <summary>
    /// High-level logic status of a check icon, used to color its map ring.
    /// </summary>
    internal enum IconLogicState
    {
        NotACheck,          // door / wall / keystone / non-location icon -> no ring
        Collected,          // location already checked -> grey ring
        InLogic,            // uncollected and reachable at current difficulty -> green ring
        PossibleAtHarder,   // reachable only at a harder difficulty than the current one -> yellow ring
        OutOfLogic          // not reachable even at the hardest difficulty -> red ring
    }

    internal class LogicManager
    {
        private static LogicChecker _logicChecker;
        public static LogicChecker LogicChecker { get { return _logicChecker ?? (_logicChecker = new LogicChecker()); } }

        internal static Location Get(RuntimeWorldMapIcon icon)
        {
            return LocationLookup.Get(icon.Guid);
        }
        internal static bool IsInLogic(RuntimeWorldMapIcon icon)
        {
            try
            {
                if (IsIgnoredIconType(icon.Icon))
                    return !MaptrackerSettings.HideNonCollectableIcons;

                var trackerItem = LocationLookup.Get(icon.Guid);
                if (trackerItem == null)
                    return false;

                if (RandomizerManager.Receiver.IsLocationChecked(trackerItem.Name, MaptrackerSettings.IconVisibilityLogic == IconVisibilityLogicEnum.Game, trackerItem.IsGoalRequiredItem()))
                    return false;

                MaptrackerSettings.AddCheck(icon.Guid);

                var checkIsInLogic = LogicChecker.IsPickupAccessible(trackerItem.Name, RandomizerManager.Options.LogicDifficulty, RandomizerManager.Receiver.GetAllItems(), RandomizerManager.Options);
                if (checkIsInLogic)
                    MaptrackerSettings.AddCheck(icon.Guid, checkIsInLogic);
                return checkIsInLogic;

            }
            catch (Exception ex)
            {
                ModLogger.Error($"Error at IsInLogic: {ex}");
                return false;
            }
        }

        /// <summary>
        /// Classifies a check icon for map-ring coloring: collected (grey), in logic at the
        /// current difficulty (green), reachable only at a harder difficulty (yellow), or
        /// not reachable at all (red). Non-location icons return <see cref="IconLogicState.NotACheck"/>.
        /// </summary>
        internal static IconLogicState GetLogicState(RuntimeWorldMapIcon icon)
        {
            try
            {
                if (IsIgnoredIconType(icon.Icon))
                    return IconLogicState.NotACheck;

                var trackerItem = LocationLookup.Get(icon.Guid);
                if (trackerItem == null)
                    return IconLogicState.NotACheck;

                if (RandomizerManager.Receiver.IsLocationChecked(trackerItem.Name, MaptrackerSettings.IconVisibilityLogic == IconVisibilityLogicEnum.Game, trackerItem.IsGoalRequiredItem()))
                    return IconLogicState.Collected;

                var items = RandomizerManager.Receiver.GetAllItems();
                var options = RandomizerManager.Options;

                if (LogicChecker.IsPickupAccessible(trackerItem.Name, options.LogicDifficulty, items, options))
                    return IconLogicState.InLogic;

                // Not in logic at the current difficulty: is it reachable at all (hardest ruleset)?
                if (options.LogicDifficulty != DifficultyOptions.Glitched &&
                    LogicChecker.IsPickupAccessible(trackerItem.Name, DifficultyOptions.Glitched, items, options))
                    return IconLogicState.PossibleAtHarder;

                return IconLogicState.OutOfLogic;
            }
            catch (Exception ex)
            {
                ModLogger.Error($"Error at GetLogicState: {ex}");
                return IconLogicState.NotACheck;
            }
        }

        internal static bool IsUncollected(RuntimeWorldMapIcon icon)
        {
            try
            {
                if (IsIgnoredIconType(icon.Icon))
                    return !MaptrackerSettings.HideNonCollectableIcons;

                var trackerItem = LocationLookup.Get(icon.Guid);
                if (trackerItem == null)
                    return false;

                return !RandomizerManager.Receiver.IsLocationChecked(trackerItem.Name, MaptrackerSettings.IconVisibilityLogic == IconVisibilityLogicEnum.Game, trackerItem.IsGoalRequiredItem());
            }
            catch (Exception ex)
            {
                ModLogger.Error($"Error at IsUncollected: {ex}");
                return false;
            }
        }

        private static bool IsIgnoredIconType(WorldMapIconType iconType)
        {

            switch (iconType)
            {
                case WorldMapIconType.KeystoneDoorTwo:
                case WorldMapIconType.BreakableWall:
                case WorldMapIconType.BreakableWallBroken:
                case WorldMapIconType.StompableFloor:
                case WorldMapIconType.StompableFloorBroken:
                case WorldMapIconType.EnergyGateTwo:
                case WorldMapIconType.EnergyGateOpen:
                case WorldMapIconType.KeystoneDoorFour:
                case WorldMapIconType.KeystoneDoorOpen:
                case WorldMapIconType.EnergyGateFour:
                case WorldMapIconType.SavePedestal:
                    return true;
                default:
                    return false;
            }
        }
    }
}

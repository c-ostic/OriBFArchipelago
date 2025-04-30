using JetBrains.Annotations;
using OriBFArchipelago.Core;
using OriBFArchipelago.MapTracker.Core;
using System;
using System.Collections.Generic;

namespace OriBFArchipelago.MapTracker.Logic
{
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
                    return false;

                var trackerItem = LocationLookup.Get(icon.Guid);
                if (trackerItem == null)
                    return false;
                
                if (MaptrackerSettings.IconVisibilityLogic == IconVisibilityLogicEnum.Archipelago && RandomizerManager.Receiver.IsLocationChecked(trackerItem.Name, trackerItem.IsGoalRequiredItem()))
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
                    return true;
                default:
                    return false;
            }
        }
    }
}

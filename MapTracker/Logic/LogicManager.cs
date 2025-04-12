using OriBFArchipelago.Core;
using OriBFArchipelago.MapTracker.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace OriBFArchipelago.MapTracker.Logic
{
    internal class LogicManager
    {
        private static LogicChecker _logicChecker;

        internal static Location Get(RuntimeWorldMapIcon icon)
        {
            return LocationLookup.Get(icon.Guid);
        }
        internal static bool IsInLogic(RuntimeWorldMapIcon icon)
        {
            try
            {
                if (_logicChecker == null)
                    _logicChecker = new LogicChecker();

                if (IsIgnoredIconType(icon.Icon))
                    return false;

                if (IsDuplicateIcon(icon))
                    return false;

                var trackerItem = LocationLookup.Get(icon.Guid);
                if (trackerItem == null)
                    return false;

                if (MaptrackerSettings.IconVisibilityLogic == IconVisibilityLogicEnum.Archipelago && RandomizerManager.Receiver.IsLocationChecked(trackerItem.Name))
                    return false;

                return _logicChecker.IsPickupAccessible(trackerItem.Name, RandomizerManager.Receiver.GetAllItems(), RandomizerManager.Options.LogicDifficulty);
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

        private static bool IsDuplicateIcon(RuntimeWorldMapIcon icon)
        {
            List<MoonGuid> duplicateIcons = new List<MoonGuid>{
                 new MoonGuid("1607939702 1149860266 185564807 -1906561306"), //duplicate icon on bash
                 new MoonGuid("1725611206 1201986298 -435475044 -1944513031"), //duplicate icon on ability point in burrows
            };


            if (duplicateIcons.Contains(icon.Guid))
                return true;
            return false;
        }
    }
}

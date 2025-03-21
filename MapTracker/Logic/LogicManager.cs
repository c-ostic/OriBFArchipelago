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

                if (icon.Icon == WorldMapIconType.SavePedestal)
                {
                    var tp = LogicInventory.Teleporters.FirstOrDefault(d => d.Guid == icon.Guid);
                    return tp?.IsActivaded ?? false;
                }

                //if (IsCollectedPetrifiedPlant(icon))
                //return false;






                var trackerItem = LocationLookup.Get(icon.Guid);
                if (icon.Icon == WorldMapIconType.AbilityPedestal && trackerItem == null)
                {
                    ModLogger.Debug($"Failed to find ability pedestal: {icon.Guid}");
                    return false;
                }

                if (trackerItem == null)
                    return false;

                return _logicChecker.IsPickupAccessible(trackerItem.Name, LogicInventory.GetInventory());
            }
            catch (Exception ex)
            {
                ModLogger.Error($"Error at IsInLogic: {ex}");
                return false;
            }

            //check petrifiedplants
            //get item name by locations.
            //check item against logic
            //build inventory
            //more?
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
                case WorldMapIconType.EnergyGateTwelve:
                case WorldMapIconType.EnergyGateTen:
                case WorldMapIconType.EnergyGateEight:
                case WorldMapIconType.EnergyGateSix:
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

using OriModding.BF.UiLib.Map;
using UnityEngine;

namespace OriBFArchipelago.MapTracker.Model
{
    internal class ExtendedCustomWorldMapIcon
    {
        public string Name { get; set; }
        public string AreaName { get; set; }
        public CustomWorldMapIcon Icon { get; set; }



        public ExtendedCustomWorldMapIcon(WorldMapIconType iconType, MoonGuid guid, Vector3 position, string name, string areaName)
        {
            Name = name;
            AreaName = areaName;
            Icon = new CustomWorldMapIcon(iconType, position, guid);
        }
    }
}

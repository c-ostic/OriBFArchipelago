using System;
using System.Collections.Generic;
using System.Linq;
using OriBFArchipelago.Core;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace OriBFArchipelago.Extensions
{
    public static class MoonExtensions
    {
        //Taken from original rando: https://github.com/ori-community/bf-rando/blob/main/Randomiser/Extensions/MoonExtensions.cs
        public static WorldArea CurrentWorldArea(this SeinCharacter sein)
        {
            return GameWorld.Instance.WorldAreaAtPosition(sein.Position)?.AsWorldArea() ?? WorldArea.Void;
        }

        public static WorldArea AsWorldArea(this GameWorldArea area)
        {
            switch (area.AreaIdentifier)
            {
                case "sunkenGlades": return WorldArea.Glades;
                case "hollowGrove": return WorldArea.Grove;
                case "moonGrotto": return WorldArea.Grotto;
                case "ginsoTree": return WorldArea.Ginso;
                case "thornfeltSwamp": return WorldArea.Swamp;
                case "mistyWoods": return WorldArea.Misty;
                case "valleyOfTheWind": return WorldArea.Valley;
                case "sorrowPass": return WorldArea.Sorrow;
                case "forlornRuins": return WorldArea.Forlorn;
                case "mangrove": return WorldArea.Blackroot;
                case "mountHoru": return WorldArea.Horu;
                default: return WorldArea.Void;
            }
        }
    }
}

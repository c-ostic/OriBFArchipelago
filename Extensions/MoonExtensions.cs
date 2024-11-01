using Core;
using OriBFArchipelago.Core;

namespace OriBFArchipelago.Extensions
{
    public static class MoonExtensions
    {
        //Taken from original rando: https://github.com/ori-community/bf-rando/blob/main/Randomiser/Extensions/MoonExtensions.cs
        public static WorldArea CurrentWorldArea(this SeinCharacter sein)
        {
            return GameWorld.Instance.WorldAreaAtPosition(sein.Position)?.AsWorldArea() ?? GetAreaByScenes();
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
                default:
                    return WorldArea.Void;
            }
        }

        private static WorldArea GetAreaByScenes()
        {
            // Annoyingly, there is no area identifier at Horu Mapstone. Instead, look up by scene at that mapstone.
            if (Scenes.Manager.CurrentScene?.Scene == "mountHoruHubTop")
            {
                return WorldArea.Horu;
            }
            return WorldArea.Void;
        }
    }
}

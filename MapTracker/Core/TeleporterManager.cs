using Game;
using OriBFArchipelago.Core;
using OriBFArchipelago.MapTracker.Model;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OriBFArchipelago.MapTracker.Core
{
    internal static class TeleporterManager
    {
        private static List<Teleporter> _teleporters;
        public static List<Teleporter> Teleporters
        {
            get
            {
                if (_teleporters != null)
                    return _teleporters;

                _teleporters = new List<Teleporter>
                {
                    //new Teleporter( new MoonGuid("535515012 1334527363 694602174 1078580914"),"horuFields",""), //Excluded due to enclosed area.
                    new Teleporter( new MoonGuid("-116578275 1111087997 412427670 -1249908721"), "forlorn", "TPForlorn"),
                    new Teleporter( new MoonGuid("1192718876 1302593798 1929767334 1228332312"),"ginsoTree","TPGinso"),
                    new Teleporter( new MoonGuid("1392867786 1221127759 -1187823465 2065923254"),"mangroveFalls","TPBlackroot"),
                    new Teleporter( new MoonGuid("1752643371 1284208868 -838119773 -772240063"),"moonGrotto","TPGrotto"),
                    new Teleporter( new MoonGuid("-222749108 1226712869 1550796190 1752513159"),"mountHoru","TPHoru"),
                    new Teleporter( new MoonGuid("-526679870 1154615959 -822258040 -1157635306"),"sorrowPass","TPValley"),
                    new Teleporter( new MoonGuid("1728896576 1241211625 810412199 -1853282216"),"spiritTree","TPGrove"),
                    new Teleporter( new MoonGuid("-426388372 1251161513 2131007642 -178890301"),"sunkenGlades","TPGlades"),
                    new Teleporter( new MoonGuid("1413930166 1176009348 411655079 -1337676822"),"swamp","TPSwamp"),
                    new Teleporter( new MoonGuid("290349702 1160050707 663397788 1544872441"),"valleyOfTheWind","TPSorrow") //Yes, valleyOfTheWind teleporter goes to sorrow peak and visa versa. This is a "bug" in the original game.
                };

                return _teleporters;
            }
        }
        public static void TeleportToStart()
        {
            try
            {
                if (Characters.Sein == null)
                {
                    RandomizerMessager.instance.AddMessage("You have to start a game before you can use this ability.");
                    return;
                }

                Game.UI.Menu.HideMenuScreen(true);
                if (!Characters.Sein.Active || Characters.Sein.IsSuspended || Characters.Sein.Controller.IsSwimming || !Characters.Sein.Controller.CanMove)
                {
                    RandomizerMessager.instance.AddMessage("You can not teleport from here. Get to a save place where you can freely stand.");
                    return;
                }

                var original = TeleporterController.Instance.Teleporters.FirstOrDefault();
                var originalPos = original.WorldPosition; //Save original position - fornlorn cavern
                original.WorldPosition = new Vector3(189, -219, 0); //Set position to starting location
                TeleporterController.BeginTeleportation(original);
                ModLogger.Debug("Teleport to start");
                original.WorldPosition = originalPos; //Return position to original otherwise forlorn will always teleport to starting location
            }
            catch (System.Exception ex)
            {
                ModLogger.Error(ex.ToString());
            }
        }
    }
}
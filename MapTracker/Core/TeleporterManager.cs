using Game;
using OriBFArchipelago.Core;
using OriBFArchipelago.Helper;
using OriBFArchipelago.MapTracker.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OriBFArchipelago.MapTracker.Core
{
    internal static class TeleporterManager
    {
        private static List<Teleporter> _teleporters;
        private static Teleporter _lastUsedTeleporter;

        private static Vector3 _startingLocation = new Vector3(189, -219, 0);

        public static Vector3 StartingLocationCoordinates
        {
            get { return _startingLocation; }
        }
        public static List<Teleporter> Teleporters
        {
            get
            {
                if (_teleporters != null)
                    return _teleporters;

                _teleporters = new List<Teleporter>
                {
                    //new Teleporter( new MoonGuid("535515012 1334527363 694602174 1078580914"),"horuFields",""), //Excluded due to enclosed area.
                    new Teleporter( new MoonGuid("-116578275 1111087997 412427670 -1249908721"), "forlorn", "TPForlorn", "Forlorn Ruins"),
                    new Teleporter( new MoonGuid("1192718876 1302593798 1929767334 1228332312"),"ginsoTree","TPGinso", "Ginso Tree"),
                    new Teleporter( new MoonGuid("1392867786 1221127759 -1187823465 2065923254"),"mangroveFalls","TPBlackroot", "Blackroot Cavern"),
                    new Teleporter( new MoonGuid("1752643371 1284208868 -838119773 -772240063"),"moonGrotto","TPGrotto", "Moon Grotto"),
                    new Teleporter( new MoonGuid("-222749108 1226712869 1550796190 1752513159"),"mountHoru","TPHoru", "Horu"),
                    new Teleporter( new MoonGuid("-526679870 1154615959 -822258040 -1157635306"),"sorrowPass","TPValley", "Valley of the Wind"),
                    new Teleporter( new MoonGuid("1728896576 1241211625 810412199 -1853282216"),"spiritTree","TPGrove", "Grove"),
                    new Teleporter( new MoonGuid("-426388372 1251161513 2131007642 -178890301"),"sunkenGlades","TPGlades", "Sunken Glades"),
                    new Teleporter( new MoonGuid("1413930166 1176009348 411655079 -1337676822"),"swamp","TPSwamp", "Thornfelt Swamp"),
                    new Teleporter( new MoonGuid("290349702 1160050707 663397788 1544872441"),"valleyOfTheWind","TPSorrow","Sorrow Pass") //Yes, valleyOfTheWind teleporter goes to sorrow peak and visa versa. This is a "bug" in the original game.
                };

                return _teleporters;
            }
        }
        public static void TeleportToStart()
        {
            try
            {

                if (!CanTeleport())
                    return;

                var original = TeleporterController.Instance.Teleporters.FirstOrDefault();
                var originalPos = original.WorldPosition; //Save original position - fornlorn cavern
                original.WorldPosition = _startingLocation; //Set position to starting location
                StartTeleport(original);
                original.WorldPosition = originalPos; //Return position to original otherwise forlorn will always teleport to starting location
            }
            catch (System.Exception ex)
            {
                ModLogger.Error(ex.ToString());
            }
        }

        public static Teleporter GetLastTeleporter()
        {
            return _lastUsedTeleporter;
        }
        public static void SetLastTeleporter(GameMapTeleporter gameMapTeleporter)
        {
            try
            {
                if (RandoGuard.IsNull(gameMapTeleporter, "gameMapTeleporter can not be null"))
                    return;

                var teleporter = Teleporters.FirstOrDefault(d => d.GameIdentifier == gameMapTeleporter.Identifier);
                if (RandoGuard.IsNull(teleporter, $"Failed to find teleporter with identifier:{gameMapTeleporter.Identifier}"))
                    return;

                _lastUsedTeleporter = teleporter;
            }
            catch (Exception ex)
            {
                ModLogger.Error(ex.ToString());
            }
        }

        internal static void TeleportToLastTeleporter()
        {
            if (!CanTeleport())
                return;

            if (RandoGuard.IsNullWithMessage(_lastUsedTeleporter, "You haven't used a teleporter yet in this seession."))
                return;

            var teleporter = TeleporterController.Instance.Teleporters.FirstOrDefault(d => d.Identifier == _lastUsedTeleporter.GameIdentifier);
            StartTeleport(teleporter);
        }


        private static bool CanTeleport()
        {
            if (RandoGuard.IsNullWithMessage(Characters.Sein, "You have to start a game before you can use this ability."))
                return false;

            if (!Characters.Sein.Active || Characters.Sein.Controller.IsSwimming || !Characters.Sein.Controller.CanMove)
            {
                RandomizerMessager.instance.AddMessage("You can not teleport from here. Get to a save place where you can freely stand.");
                ModLogger.Debug($"Sein active: {Characters.Sein.Active}");
                ModLogger.Debug($"Sein swimming: {Characters.Sein.Controller.IsSwimming}");
                ModLogger.Debug($"Sein can move: {Characters.Sein.Controller.CanMove}");
                return false;
            }

            return true;
        }

        private static void StartTeleport(GameMapTeleporter teleporter)
        {
            if (teleporter == null)
                return;

            TeleporterController.BeginTeleportation(teleporter);
            Game.UI.Menu.HideMenuScreen(true);
        }
    }
}
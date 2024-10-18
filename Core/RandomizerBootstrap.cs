using Game;
using OriModding.BF.Core;
using OriModding.BF.l10n;
using OriModding.BF.UiLib.Menu;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace OriBFArchipelago.Core
{
    internal static class RandomiserBootstrap
    {
        public static void SetupBootstrap(SceneBootstrap sceneBootstrap)
        {
            sceneBootstrap.BootstrapActions = new Dictionary<string, Action<SceneRoot>>
            {
                // Horu
                ["mountHoruHubMid"] = BootstrapMountHoruHubMid,

                // Death plane
                ["valleyOfTheWindBackground"] = BootstrapValleyOfTheWindBackground,

                // Ginso Fixes
                ["ginsoEntranceSketch"] = BootstrapGinsoEntry,
                ["ginsoTreeWaterRisingEnd"] = BootstrapGinsoEnd,
                ["ginsoTreeWaterRisingMid"] = BootstrapGinsoEscapeMid,
                ["ginsoTreeWaterRisingBtm"] = BootstrapGinsoEscapeStart,
                ["ginsoTreeResurrection"] = BootstrapGinsoTreeResurrection,
                ["thornfeltSwampActTwoStart"] = BootstrapThornfeltSwampActTwoStart,

                // Forlorn Fixes
                ["forlornRuinsResurrection"] = BootstrapForlornRuinsResurrection,

            };
        }

        #region Horu
        private static void BootstrapMountHoruHubMid(SceneRoot sceneRoot)
        {
            // Open Dungeons (remove all lava)

            string[] lavaStreamNames = { "lavaStreamA", "lavaStreamB", "lavaStreamC", "lavaStreamD", "lavaStreamE", "lavaStreamF", "lavaStreamECausticOn", "LavaFGElements", "uberLavaBottom", "lavaStreamECausticOff", "lavaStreamFCausticOff" };
            foreach (string lavaStreamName in lavaStreamNames)
            {
                GameObject lavaStream = sceneRoot.transform.Find(lavaStreamName).gameObject;
                lavaStream.SetActive(false);
            }
        }
        #endregion

        #region Ginso Fixes
        private static void BootstrapThornfeltSwampActTwoStart(SceneRoot sceneRoot)
        {
            // Make swamp cutscene play based on "finished ginso escape" instead of "clean water"
            ReplaceCondition(sceneRoot.transform.Find("*setups").GetComponent<OnSceneStartRunAction>());
            ReplaceCondition(sceneRoot.transform.Find("*objectiveSetup/objectiveSetupTrigger").GetComponent<OnSceneStartRunAction>());

            // Hide gumo until you do the escape
            var gumoSavesSein = sceneRoot.transform.Find("*gumoSavesSein");
            var condition = gumoSavesSein.gameObject.AddComponent<FinishedGinsoEscapeCondition>();
            AddActivator(gumoSavesSein, gumoSavesSein.Find("group").gameObject, condition);

            // patch the post-Ginso cutscene to fix softlock when Sein's dialogue is auto-skipped
            ActionSequence seinAnimationSequence = sceneRoot.transform.FindChild("*objectiveSetup/objectiveSetupTrigger/seinSpriteAction").GetComponent<ActionSequence>();
            WaitAction waitAction = seinAnimationSequence.Actions[1] as WaitAction;
            waitAction.Duration = 5.0f;

            // force the music to start up, dang it
            var musicZones = sceneRoot.transform.Find("musicZones").GetComponentsInChildren<MusicZone>(includeInactive: true);
            foreach (var zone in musicZones)
                zone.gameObject.SetActive(!zone.gameObject.activeInHierarchy);
        }

        private static void ReplaceCondition(OnSceneStartRunAction action)
        {
            var condition = action.gameObject.AddComponent<FinishedGinsoEscapeCondition>();
            UnityEngine.Object.Destroy(action.Condition);
            action.Condition = condition;
        }

        private static void BootstrapGinsoEntry(SceneRoot sceneRoot)
        {
            // Allow the water vein to be inserted even when clean water is owned
            var activator = sceneRoot.transform.Find("*setups/openingGinsoTree").GetComponent<ActivateBasedOnCondition>();
            var condition = activator.gameObject.AddComponent<ConstantCondition>();
            condition.IsTrue = true;
            activator.Condition = condition;
        }

        private static void BootstrapGinsoEnd(SceneRoot sceneRoot)
        {
            // Remove branches that block the exit when clean water is owned
            sceneRoot.transform.Find("artAfter/artAfter/blockingWall").gameObject.SetActive(false);
            sceneRoot.transform.Find("artAfter/artAfter/surfaceColliders").gameObject.SetActive(false);

            PatchMusicZones(sceneRoot.transform.Find("musiczones"));
        }

        private static void BootstrapGinsoEscapeMid(SceneRoot sceneRoot)
        {
            PatchMusicZones(sceneRoot.transform.Find("artBefore/musiczones"));
        }

        private static void BootstrapGinsoEscapeStart(SceneRoot sceneRoot)
        {
            PatchMusicZones(sceneRoot.transform.Find("artBefore/musiczones"));
        }

        private static void BootstrapGinsoTreeResurrection(SceneRoot sceneRoot)
        {
            {
                PatchMusicZones(sceneRoot.transform.Find("musicZones").GetChild(0));
                PatchMusicZones(sceneRoot.transform.Find("musicZones").GetChild(1));

                // Change the walls blocking access to the side rooms so they will be disabled if you have finished the escape
                var activator = sceneRoot.transform.Find("*heartResurrection/restoringHeartWaterRising/activator").GetComponent<ActivateBasedOnCondition>();
                var newCondition = activator.gameObject.AddComponent<FinishedGinsoEscapeCondition>();
                activator.Condition = newCondition;

                // Fix the "double heart" effect of both the active and inactive hearts being visible at once if you have clean water
                var artAfter = sceneRoot.transform.Find("artAfter/artAfter");
                var condition = artAfter.gameObject.AddComponent<FinishedGinsoEscapeCondition>();
                AddActivator(artAfter, artAfter.Find("heartClean").gameObject, condition);
                AddActivator(artAfter, artAfter.Find("rotatingLightraysA").gameObject, condition);
                AddActivator(artAfter, artAfter.Find("rotatingLightraysB").gameObject, condition);
            }

            {
                // Elemental kill door blocking spirit well
                var door = sceneRoot.transform.Find("*turretEnemyPuzzle/*enemyPuzzle/doorSetup").gameObject;
                var doorActivator = door.AddComponent<ActivateBasedOnCondition>();
                var doorCondition = door.AddComponent<ConstantCondition>();
                doorActivator.Condition = doorCondition;
                doorActivator.Target = door.transform.Find("sidewaysDoor").gameObject;
                doorCondition.IsTrue = false;
            }
        }

        private static void AddActivator(Transform root, GameObject target, Condition condition)
        {
            var activator1 = root.gameObject.AddComponent<ActivateBasedOnCondition>();
            activator1.Target = target;
            activator1.Condition = condition;
        }

        private static void PatchMusicZones(Transform musicZones)
        {
            // Also includes soul link zones
            var musicZoneActivators = musicZones.GetComponents<ActivateBasedOnCondition>();
            var newCondition = musicZoneActivators.First().gameObject.AddComponent<FinishedGinsoEscapeCondition>();
            foreach (var activator in musicZoneActivators)
                activator.Condition = newCondition;

            UnityEngine.Object.Destroy(musicZones.GetComponent<SeinWorldStateCondition>());
        }
        #endregion

        #region Forlorn Fixes
        private static void BootstrapForlornRuinsResurrection(SceneRoot sceneRoot)
        {
            // Make the rocks blocking access to Folorn final room dependent on ForlornEscape location instead 
            var backtrackingBlockOffTrigger = sceneRoot.transform.Find("*backtrackingBlockOff").GetComponent<ActivationBasedOnCondition>();
            UnityEngine.Object.Destroy(backtrackingBlockOffTrigger.Condition);
            var backtrackingBlockOffCondition = backtrackingBlockOffTrigger.gameObject.AddComponent<FinishedForlornEscapeCondition>();
            backtrackingBlockOffTrigger.Condition = backtrackingBlockOffCondition;

            // For wind inside the final room
            bool hasEscaped = RandomizerManager.Connection.IsForlornEscapeComplete();
            sceneRoot.transform.Find("floatZone").gameObject.SetActive(hasEscaped);
        }
        #endregion

        #region Stomp Triggers
        private static void BootstrapValleyOfTheWindBackground(SceneRoot sceneRoot)
        {
            var deathZoneTrigger = sceneRoot.transform.Find("*getFeatherSetupContainer/*kuroHideSetup/kuroDeathZones").GetComponent<ActivateBasedOnCondition>();
            UnityEngine.Object.Destroy(deathZoneTrigger.Condition);
            var deathZoneCondition = deathZoneTrigger.gameObject.AddComponent<StompTriggerCondition>();
            deathZoneTrigger.Condition = deathZoneCondition;

            var kuroCliffTriggerCollider = sceneRoot.transform.Find("*getFeatherSetupContainer/*kuroCliffLowerHint/triggerCollider").GetComponent<PlayerCollisionTrigger>();
            UnityEngine.Object.Destroy(kuroCliffTriggerCollider.Condition);
            var kuroCliffCondition = kuroCliffTriggerCollider.gameObject.AddComponent<StompTriggerCondition>();
            kuroCliffTriggerCollider.Condition = kuroCliffCondition;
        }
        #endregion
    }

    public class StompTriggerCondition : Condition
    {
        public override bool Validate(IContext context)
        {
            return Characters.Sein != null && Characters.Sein.PlayerAbilities.HasAbility(AbilityType.Stomp);
        }
    }

    public class FinishedGinsoEscapeCondition : Condition
    {
        public bool IsTrue = true;

        public override bool Validate(IContext context)
        {
            return RandomizerManager.Connection.IsGinsoEscapeComplete() == IsTrue;
        }
    }

    public class FinishedForlornEscapeCondition : Condition
    {
        public bool IsTrue = true;

        public override bool Validate(IContext context)
        {
            return RandomizerManager.Connection.IsForlornEscapeComplete() == IsTrue;
        }
    }

    public class ConstantCondition : Condition
    {
        public bool IsTrue { get; set; }

        public override bool Validate(IContext context)
        {
            return IsTrue;
        }
    }
}

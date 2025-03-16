using Game;
using OriModding.BF.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OriBFArchipelago.Core
{
    /**
     * Bootstraps specific scenes to fix bugs and make randomizer changes
     * Many changes are modified from https://github.com/ori-community/bf-rando/blob/main/Randomiser/World%20Changes/RandomiserBootstrap.cs
     */
    internal static class RandomiserBootstrap
    {
        public static void SetupBootstrap(SceneBootstrap sceneBootstrap)
        {
            sceneBootstrap.BootstrapActions = new Dictionary<string, Action<SceneRoot>>
            {
                // Horu Fixes
                ["mountHoruHubMid"] = BootstrapMountHoruHubMid,

                // Valley Fixes
                ["valleyOfTheWindBackground"] = BootstrapValleyOfTheWindBackground,

                // Ginso Fixes
                ["ginsoEntranceSketch"] = BootstrapGinsoEntry,
                ["ginsoTreeWaterRisingEnd"] = BootstrapGinsoEnd,
                ["ginsoTreeWaterRisingMid"] = BootstrapGinsoEscapeMid,
                ["ginsoTreeWaterRisingBtm"] = BootstrapGinsoEscapeStart,
                ["ginsoTreeResurrection"] = BootstrapGinsoTreeResurrection,
                ["thornfeltSwampActTwoStart"] = BootstrapThornfeltSwampActTwoStart,
                ["kuroMomentCutscene"] = BootstrapKuroMomentCutscene,

                // Forlorn Fixes
                ["forlornRuinsResurrection"] = BootstrapForlornRuinsResurrection,
                ["forlornRuinsC"] = BootstrapForlornRuinsC,

                // Grotto Fixes
                ["moonGrottoRopeBridge"] = BootstrapGrottoRopeBridge,
                ["moonGrottoGumosHideoutB"] = BootstrapGumosHideoutB,

                // Grove Fixes
                ["spiritTreeRefined"] = BootstrapSpiritTreeRefined
            };
        }

        private static T InsertAction<T>(ActionSequence sequence, int index, MoonGuid guid, SceneRoot sceneRoot) where T : ActionMethod
        {
            var go = new GameObject();
            go.transform.SetParent(sequence.transform);
            var action = go.AddComponent<T>();
            action.MoonGuid = guid;
            action.RegisterToSaveSceneManager(sceneRoot.SaveSceneManager);
            sequence.Actions.Insert(index, action);
            ActionSequence.Rename(sequence.Actions);
            return action;
        }

        #region Horu Fixes
        private static void BootstrapMountHoruHubMid(SceneRoot sceneRoot)
        {
            // Open Dungeons (remove all lava)

            string[] lavaStreamNames = { "lavaStreamA", "lavaStreamB", "lavaStreamC", "lavaStreamD", "lavaStreamE", "lavaStreamF", "lavaStreamECausticOn", "LavaFGElements", "uberLavaBottom", "lavaStreamECausticOff", "lavaStreamFCausticOff" };
            foreach (string lavaStreamName in lavaStreamNames)
            {
                GameObject lavaStream = sceneRoot.transform.Find(lavaStreamName).gameObject;
                lavaStream.SetActive(false);
            }

            // modified from https://github.com/ori-community/bf-rando/blob/main/Randomiser/World%20Changes/RandomiserBootstrap.cs#L380
            // credit to Vulajin for the bootstrap
            // add randomized pickup actions for each end of room cutscene
            Transform lavaDrainParent = sceneRoot.transform.FindChild("*doorSetups/lavaDrainSetups");

            CheckLocationAction action;

            // door1LavaDrain - (L3) mountHoruBreakyPathTop
            action = InsertAction<CheckLocationAction>(lavaDrainParent.FindChild("*door1LavaDrains/*door1LavaDrain").GetComponent<ActionSequence>(), 3,
                new MoonGuid(-300318401, 1327879929, 1546957364, -1505614911), sceneRoot);
            action.Location = "HoruL3";

            // door2LavaDrain - (R1) mountHoruStomperSystemsR
            action = InsertAction<CheckLocationAction>(lavaDrainParent.FindChild("*door2LavaDrains/*door2LavaDrain").GetComponent<ActionSequence>(), 3,
                new MoonGuid(-300318401, 1327879929, 1546957364, -1505614912), sceneRoot);
            action.Location = "HoruR1";

            // door3LavaDrain - (R2) mountHoruProjectileCorridor
            action = InsertAction<CheckLocationAction>(lavaDrainParent.FindChild("*door3LavaDrains/*door3LavaDrain").GetComponent<ActionSequence>(), 3,
                new MoonGuid(-300318401, 1327879929, 1546957364, -1505614913), sceneRoot);
            action.Location = "HoruR2";

            // door5LavaDrain - (R3) mountHoruMovingPlatform
            action = InsertAction<CheckLocationAction>(lavaDrainParent.FindChild("*door5LavaDrains/*door5LavaDrain").GetComponent<ActionSequence>(), 3,
                new MoonGuid(-300318401, 1327879929, 1546957364, -1505614914), sceneRoot);
            action.Location = "HoruR3";

            // door7LavaDrain - (L2) mountHoruBigPushBlock
            action = InsertAction<CheckLocationAction>(lavaDrainParent.FindChild("*door7LavaDrains/*door7LavaDrain").GetComponent<ActionSequence>(), 3,
                new MoonGuid(-300318401, 1327879929, 1546957364, -1505614915), sceneRoot);
            action.Location = "HoruL2";

            // door8LavaDrain - (L1) mountHoruBlockableLasers
            action = InsertAction<CheckLocationAction>(lavaDrainParent.FindChild("*door8LavaDrains/*door8LavaDrain").GetComponent<ActionSequence>(), 3,
                new MoonGuid(-300318401, 1327879929, 1546957364, -1505614916), sceneRoot);
            action.Location = "HoruL1";

            // special cases for L4/R4
            CheckLocationAction leftPickupAction = lavaDrainParent.gameObject.AddComponent<CheckLocationAction>();
            leftPickupAction.MoonGuid = new MoonGuid(-300318401, 1327879929, 1546957364, -1505614917);
            leftPickupAction.Location = "HoruL4";
            leftPickupAction.RegisterToSaveSceneManager(sceneRoot.SaveSceneManager);
            CheckLocationAction rightPickupAction = lavaDrainParent.gameObject.AddComponent<CheckLocationAction>();
            rightPickupAction.MoonGuid = new MoonGuid(-300318401, 1327879929, 1546957364, -1505614918);
            rightPickupAction.Location = "HoruR4";
            rightPickupAction.RegisterToSaveSceneManager(sceneRoot.SaveSceneManager);

            // door4LavaDrain - L4/R4, whichever comes first
            ActionSequence doorSequence = lavaDrainParent.FindChild("*door4LavaDrains/*door4LavaDrain").GetComponent<ActionSequence>();
            GameObject obj = new GameObject("pickupAction");
            obj.transform.parent = doorSequence.transform;

            RunActionCondition conditionPickupAction = obj.AddComponent<RunActionCondition>();
            conditionPickupAction.MoonGuid = new MoonGuid(-1261986975, 1336041250, 1663544246, -817715174);
            conditionPickupAction.RegisterToSaveSceneManager(sceneRoot.SaveSceneManager);
            conditionPickupAction.Action = leftPickupAction;
            conditionPickupAction.ElseAction = rightPickupAction;
            conditionPickupAction.Condition = (doorSequence.Actions[2] as RunActionCondition).Condition;

            doorSequence.Actions.Insert(3, conditionPickupAction);
            ActionSequence.Rename(doorSequence.Actions);

            // door6LavaDrain - L4/R4, whichever comes second
            doorSequence = lavaDrainParent.FindChild("*door6LavaDrains/*door6LavaDrain").GetComponent<ActionSequence>();
            obj = new GameObject("pickupAction");
            obj.transform.parent = doorSequence.transform;

            conditionPickupAction = obj.AddComponent<RunActionCondition>();
            conditionPickupAction.MoonGuid = new MoonGuid(-300318401, 1327879929, 1536957364, -1500614911);
            conditionPickupAction.RegisterToSaveSceneManager(sceneRoot.SaveSceneManager);
            conditionPickupAction.Action = rightPickupAction;
            conditionPickupAction.ElseAction = leftPickupAction;
            conditionPickupAction.Condition = (doorSequence.Actions[2] as RunActionCondition).Condition;

            doorSequence.Actions.Insert(3, conditionPickupAction);
            ActionSequence.Rename(doorSequence.Actions);
        }
        #endregion

        #region Ginso Fixes
        private static void BootstrapThornfeltSwampActTwoStart(SceneRoot sceneRoot)
        {
            // Make swamp cutscene play based on "finished ginso escape" instead of "clean water"
            ReplaceCondition(sceneRoot.transform.Find("*setups").GetComponent<OnSceneStartRunAction>());
            ReplaceCondition(sceneRoot.transform.Find("*objectiveSetup/objectiveSetupTrigger").GetComponent<OnSceneStartRunAction>());

            ActionSequence gumoCutsceneActionSequence = sceneRoot.transform.Find("*objectiveSetup/objectiveSetupTrigger/objectiveSetupAction").GetComponent<ActionSequence>();

            var ginsoCompleteAction = InsertAction<SendLocalAPItemsAction>(gumoCutsceneActionSequence, 45, new MoonGuid(-1289149174, 680822595, 558787450, 1729667918), sceneRoot);
            ginsoCompleteAction.Item = InventoryItem.GinsoEscapeComplete;

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
            var condition = action.gameObject.AddComponent<LeftGinsoCondition>();
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

            // Prevent teleporting animation near the exit from triggering the exit and avoid potential softlock. Ask me how I know.
            var triggerTransform = sceneRoot.transform.Find("*exit/trigger");
            var condition = triggerTransform.gameObject.AddComponent<IsTeleportingCondition>();
            var triggerAction = triggerTransform.GetComponent<PlayerCollisionTrigger>();
            triggerAction.Condition = condition;

            var artBeforeTransform = sceneRoot.transform.Find("artBefore");
            var artBeforeNewCondition = artBeforeTransform.gameObject.AddComponent<FinishedGinsoEscapeCondition>();
            var artBeforeAction = artBeforeTransform.GetComponent<ActivateBasedOnCondition>();
            artBeforeAction.Condition = artBeforeNewCondition;

            var artAfterTransform = sceneRoot.transform.Find("artAfter");
            var artAfterNewCondition = artAfterTransform.gameObject.AddComponent<FinishedGinsoEscapeCondition>();
            var artAfterAction = artAfterTransform.GetComponent<ActivateBasedOnCondition>();
            artAfterAction.Condition = artAfterNewCondition;
        }

        private static void BootstrapGinsoEscapeMid(SceneRoot sceneRoot)
        {
            PatchMusicZones(sceneRoot.transform.Find("artBefore/musiczones"));
        }

        private static void BootstrapGinsoEscapeStart(SceneRoot sceneRoot)
        {
            PatchMusicZones(sceneRoot.transform.Find("artBefore/musiczones"));

            // Reset enemies needed for bash
            var enemiesTransform = sceneRoot.transform.Find("enemies");
            var jumperEnemy1 = enemiesTransform.GetChild(0).GetComponent<JumperEnemyPlaceholder>();
            var jumperEnemy2 = enemiesTransform.GetChild(1).GetComponent<JumperEnemyPlaceholder>();

            jumperEnemy1.RespawnTime = 15;
            jumperEnemy2.RespawnTime = 15;

            var waterRisingSpeedTrigger = sceneRoot.transform.Find("waterChangePropertiesTriggers/waterChangePropertiesTrigger").GetComponent<PlayerCollisionTrigger>();
            waterRisingSpeedTrigger.TriggerOnce = false;
            waterRisingSpeedTrigger.Active = true;

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

                // Make it so we can retrigger the escape sequence
                var interestTrigger = sceneRoot.transform.Find("*heartResurrection/restoringHeartWaterRising/triggerWaterSequence").GetComponent<OriInterestTriggerB>();
                interestTrigger.RunOnce = false;

                // Walls should not be disabled based on clean water
                var blockingWallsTransform = sceneRoot.transform.Find("*heartResurrection/restoringHeartWaterRising/blockingWalls");

                var newWallCondition = blockingWallsTransform.gameObject.AddComponent<FinishedGinsoEscapeCondition>();

                foreach (ActivateBasedOnCondition wallActivator in blockingWallsTransform.GetComponents<ActivateBasedOnCondition>())
                {
                    wallActivator.Condition = newWallCondition;
                }
            }

            {
                // Elemental kill doors blocking spirit well and entry
                var door1 = sceneRoot.transform.Find("*turretEnemyPuzzle/*doorASetup").gameObject;
                var door1Activator = door1.AddComponent<ActivateBasedOnCondition>();
                var door1Condition = door1.AddComponent<ConstantCondition>();
                door1Activator.Condition = door1Condition;
                door1Activator.Target = door1.transform.Find("triggerCollider").gameObject;
                door1Condition.IsTrue = false;

                var door2 = sceneRoot.transform.Find("*turretEnemyPuzzle/*enemyPuzzle/doorSetup").gameObject;
                var door2Activator = door2.AddComponent<ActivateBasedOnCondition>();
                var door2Condition = door2.AddComponent<ConstantCondition>();
                door2Activator.Condition = door2Condition;
                door2Activator.Target = door2.transform.Find("sidewaysDoor").gameObject;
                door2Condition.IsTrue = false;
            }
        }

        private static void BootstrapKuroMomentCutscene(SceneRoot sceneRoot)
        {
            var action = InsertAction<SetGinsoExitAction>(sceneRoot.transform.Find("masterTimelineSequence/actionSequence").GetComponent<ActionSequence>(), 0, new MoonGuid(307071171, -850108097, -1715582487, 2063130120), sceneRoot);
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
            bool hasEscaped = RandomizerManager.Receiver.IsLocationChecked("ForlornEscape");
            sceneRoot.transform.Find("floatZone").gameObject.SetActive(hasEscaped);
        }

        private static void BootstrapForlornRuinsC(SceneRoot sceneRoot)
        {
            // force the animation of the bridge coming down to play upon entering the area
            ActionSequence entranceSequence = sceneRoot.transform.Find("*forlornEntranceLoad").GetComponent<ActionSequence>();
            GameObject bridgeGravity = sceneRoot.transform.Find("*setupGravity/timelineSequence").gameObject;
            BaseAnimatorAction bridgeGravityAction = InsertAction<BaseAnimatorAction>(entranceSequence, 8, new MoonGuid(-1289139173, 680722594, 558787458, 1729657920), sceneRoot);
            bridgeGravityAction.Target = bridgeGravity;
            bridgeGravityAction.Animators = [bridgeGravity.GetComponent<BaseAnimator>()];
            bridgeGravityAction.AnimatorsMode = BaseAnimatorAction.FindAnimatorsMode.GameObject;
            bridgeGravityAction.Command = BaseAnimatorAction.PlayMode.Restart;

            // remove the cutscene to place the nightberry
            GameObject nightberryCutscene = sceneRoot.transform.Find("*setupGravity/pedestalAction/*setups/triggers/cutsceneCollisionTrigger").gameObject;
            ConstantCondition cutsceneCondition = nightberryCutscene.AddComponent<ConstantCondition>();
            nightberryCutscene.GetComponent<PlayerCollisionStayTrigger>().Condition = cutsceneCondition;
            cutsceneCondition.IsTrue = false;

            // activate the bridge colliders
            GameObject roomSetup = sceneRoot.transform.Find("*setupGravity").gameObject;
            GameObject bridgeColliders = sceneRoot.transform.Find("*setupGravity/timelineSequence/bridgeColliders").gameObject;
            ActivateBasedOnCondition bridgeActivator = roomSetup.AddComponent<ActivateBasedOnCondition>();
            ConstantCondition bridgeCondition = roomSetup.AddComponent<ConstantCondition>();
            bridgeActivator.Condition = bridgeCondition;
            bridgeActivator.Target = bridgeColliders;
            bridgeCondition.IsTrue = true;

            // deactivate the door to the laser room
            GameObject laserDoor = sceneRoot.transform.Find("*setupGravity/solidWallSetup").gameObject;
            ActivateBasedOnCondition doorActivator = roomSetup.AddComponent<ActivateBasedOnCondition>();
            ConstantCondition doorCondition = roomSetup.AddComponent<ConstantCondition>();
            doorActivator.Condition = doorCondition;
            doorActivator.Target = laserDoor;
            doorCondition.IsTrue = false;
        }
        #endregion

        #region Valley Fixes
        private static void BootstrapValleyOfTheWindBackground(SceneRoot sceneRoot)
        {
            var deathZoneTrigger = sceneRoot.transform.Find("*getFeatherSetupContainer/*kuroHideSetup/kuroDeathZones").GetComponent<ActivateBasedOnCondition>();
            UnityEngine.Object.Destroy(deathZoneTrigger.Condition);
            var deathZoneCondition = deathZoneTrigger.gameObject.AddComponent<ConstantCondition>();
            deathZoneTrigger.Condition = deathZoneCondition;
            deathZoneCondition.IsTrue = true;

            var kuroCliffTriggerCollider = sceneRoot.transform.Find("*getFeatherSetupContainer/*kuroCliffLowerHint/triggerCollider").GetComponent<PlayerCollisionTrigger>();
            UnityEngine.Object.Destroy(kuroCliffTriggerCollider.Condition);
            var kuroCliffCondition = kuroCliffTriggerCollider.gameObject.AddComponent<ConstantCondition>();
            kuroCliffTriggerCollider.Condition = kuroCliffCondition;
            kuroCliffCondition.IsTrue = true;
        }
        #endregion

        #region Grotto Fixes
        private static void BootstrapGrottoRopeBridge(SceneRoot sceneRoot)
        {
            ActionSequence fallingSequence = sceneRoot.transform.Find("*gumoBridgeSetup/group/action").GetComponent<ActionSequence>();
            SetGrottoBridgeFallingAction fallingAction = InsertAction<SetGrottoBridgeFallingAction>(fallingSequence, 8, new MoonGuid(-1289139173, 680722594, 558787458, 1729657921), sceneRoot);
            fallingAction.IsTrue = true;
        }

        private static void BootstrapGumosHideoutB(SceneRoot sceneRoot)
        {
            PlayerCollisionTrigger landSetupTrigger = sceneRoot.transform.Find("*landSetup/trigger").GetComponent<PlayerCollisionTrigger>();
            IsGrottoBridgeFallingCondition fallingCondition = landSetupTrigger.gameObject.AddComponent<IsGrottoBridgeFallingCondition>();
            landSetupTrigger.Condition = fallingCondition;

            ActionSequence landSequence = sceneRoot.transform.Find("*landSetup/sequence").GetComponent<ActionSequence>();
            SetGrottoBridgeFallingAction fallingAction = InsertAction<SetGrottoBridgeFallingAction>(landSequence, 0, new MoonGuid(-1289139173, 680722594, 558787458, 1729657922), sceneRoot);
            fallingAction.IsTrue = false;
        }
        #endregion

        #region Grove Fixes
        private static void BootstrapSpiritTreeRefined(SceneRoot sceneRoot)
        {
            // Unlike most other pickups, which are permanent placeholders that spawn an object with a DestroyOnRestoreCheckpoint component,
            // this one is *just* an object with a DestroyOnRestoreCheckpoint component. Disable that to prevent its untimely demise.
            sceneRoot.transform.FindChild("mediumExpOrb").GetComponent<DestroyOnRestoreCheckpoint>().enabled = false;
        }
        #endregion
    }

    public class FinishedGinsoEscapeCondition : Condition
    {
        public bool IsTrue = true;

        public override bool Validate(IContext context)
        {
            return (LocalGameState.IsGinsoExit || (RandomizerManager.Receiver?.HasItem(InventoryItem.GinsoEscapeComplete) ?? false)) == IsTrue;
        }
    }

    public class FinishedForlornEscapeCondition : Condition
    {
        public bool IsTrue = true;

        public override bool Validate(IContext context)
        {
            return (RandomizerManager.Receiver?.IsLocationChecked("ForlornEscape") ?? false) == IsTrue;
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

    internal class SendLocalAPItemsAction : ActionMethod
    {
        internal InventoryItem Item { get; set; }

        public override void Perform(IContext context)
        {
            RandomizerManager.Receiver.ReceiveItem(Item);   
        }
    }

    internal class SetGinsoExitAction : ActionMethod
    {
        public override void Perform(IContext context)
        {
            LocalGameState.IsGinsoExit = true;
        }
    }

    // Used purely for debugging purposes
    internal class LogMessageAction : ActionMethod
    {
        public string message = "";

        public override void Perform(IContext context)
        {
            Console.WriteLine(message);
        }
    }

    internal class SetGrottoBridgeFallingAction : ActionMethod
    {
        public bool IsTrue = true;

        public override void Perform(IContext context)
        {
            LocalGameState.IsGrottoBridgeFalling = IsTrue;
        }
    }

    internal class IsGrottoBridgeFallingCondition: Condition
    {
        public override bool Validate(IContext context)
        {
            return LocalGameState.IsGrottoBridgeFalling;
        }
    }

    public class LeftGinsoCondition : Condition
    {
        public bool IsTrue = true;

        public override bool Validate(IContext context)
        {
            return LocalGameState.IsGinsoExit == IsTrue;
        }
    }

    public class IsTeleportingCondition : Condition
    {
        public override bool Validate(IContext context) 
        {
            return !LocalGameState.IsTeleporting;
        }
    }

    public class CheckLocationAction : ActionMethod
    {
        public string Location = "";

        public override void Perform(IContext context)
        {
            RandomizerManager.Connection.CheckLocation(Location);
        }
    }
}

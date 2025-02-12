using Game;
using Core;
using HarmonyLib;
using OriBFArchipelago.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OriBFArchipelago.Patches
{
    // Copied from https://github.com/ori-community/bf-rando/blob/12b424b24b141034f192fe2f6d7f7e6e2c2b0987/Randomiser/TeleporterControllerPatches.cs
    [HarmonyPatch(typeof(TeleporterController))]
    internal static class TeleporterControllerPatches
    {
        [HarmonyPostfix, HarmonyPatch(nameof(TeleporterController.OnFadedToBlack))]
        private static void OnFadedToBlackPostfix()
        {
            // save the game on teleporting
            GameController.Instance.SaveGameController.PerformSave();

            // Reset misty woods
            var mistyEvents = WorldEventsHelper.MistyWorldEvents;
            int value = mistyEvents?.Value ?? 10;
            if (value != 1 && value != 8)
            {
                mistyEvents.Value = 10;
            }

            // Reset ginso tree
            WorldEventsRuntime ginsoEvents = WorldEventsHelper.GinsoWorldEvents;
            int ginsoEventsValue = ginsoEvents?.Value ?? 23;

            if ((ginsoEventsValue == 25 || ginsoEventsValue == 21) && !(LocalGameState.IsGinsoExit) && !(RandomizerManager.Receiver?.HasItem(InventoryItem.GinsoEscapeComplete) ?? true))
            {
                Sein.World.Events.WaterPurified = false;

                var ginsoTreeResurrectionSceneManagerScene = Scenes.Manager.GetSceneManagerScene("ginsoTreeResurrection");
                var ginsoTreeResurrectionScene = ginsoTreeResurrectionSceneManagerScene.SceneRoot;

                // Revert the sequence of events when restoring Ginso tree

                ginsoTreeResurrectionScene.transform.Find("musicZones/musicZoneHeartBefore").gameObject.SetActive(true);
                ginsoTreeResurrectionScene.transform.Find("musicZones").GetChild(1).GetChild(0).GetChild(0).gameObject.SetActive(false);
                ginsoTreeResurrectionScene.transform.Find("*heartResurrection/restoringHeartWaterRising/activator/group/scrollLockWaterRising").gameObject.SetActive(false);
                ginsoTreeResurrectionScene.transform.Find("*heartResurrection/restoringHeartWaterRising/purpleGoopFallSounds").gameObject.SetActive(true);
                ginsoTreeResurrectionScene.transform.Find("*heartResurrection/blockingCreep/blockingCreep").gameObject.SetActive(true);
                ginsoTreeResurrectionScene.transform.Find("*heartResurrection/restoringHeartWaterRising/triggerWaterSequence").gameObject.SetActive(true);
                ginsoTreeResurrectionScene.transform.Find("surfaceColliders/surfaceColliderAfterResurrection").gameObject.SetActive(false);
                ginsoTreeResurrectionScene.transform.Find("*heartResurrection/chainReactionSetup/spiritLanternPlaceholders/middle").GetChild(0).Find("spiritLantern").gameObject.SetActive(false);
                ginsoTreeResurrectionScene.transform.Find("*heartResurrection/chainReactionSetup/spiritLanternPlaceholders/middle").GetChild(0).Find("lock").gameObject.SetActive(true);
                ginsoTreeResurrectionScene.transform.Find("*heartResurrection/restoringHeartWaterRising/triggerWaterSequence").gameObject.SetActive(true);

                BaseAnimatorAction reverseTimelineSequenceAction = new BaseAnimatorAction();
                var timelineSequenceObject = ginsoTreeResurrectionScene.transform.Find("*heartResurrection/restoringHeartWaterRising/timelineSequence").gameObject;

                reverseTimelineSequenceAction.Target = timelineSequenceObject;
                reverseTimelineSequenceAction.Command = BaseAnimatorAction.PlayMode.StopAtStart;
                reverseTimelineSequenceAction.AnimatorsMode = BaseAnimatorAction.FindAnimatorsMode.GameObject;
                reverseTimelineSequenceAction.Start();
                reverseTimelineSequenceAction.Perform(null);

                var ginsoTreeWaterRisingBackgroundSceneManagerScene = Scenes.Manager.GetSceneManagerScene("ginsoTreeWaterRisingBackground");
                var ginsoTreeWaterRisingBackgroundScene = ginsoTreeWaterRisingBackgroundSceneManagerScene.SceneRoot;

                // Reset water level

                AnimatorAction reverseRisingWaterSequenceAction = new AnimatorAction();
                var risingWaterGameObject = ginsoTreeWaterRisingBackgroundScene.transform.Find("*risingWaterGroup/*risingWater").gameObject;

                foreach (LegacyTranslateAnimator animator in risingWaterGameObject.GetComponents<LegacyTranslateAnimator>())
                {
                    animator.Restart();
                    animator.StopAndSampleAtStart();
                    animator.AnimateX = true;
                    animator.AnimateY = true;
                    animator.AnimateZ = true;
                    animator.RestoreToOriginalState();
                    animator.AnimateX = false;
                    animator.AnimateY = false;
                    animator.AnimateZ = false;
                }

                ginsoTreeWaterRisingBackgroundScene.transform.Find("particles").gameObject.SetActive(false);

                // Resetting upper doors

                var blockingWallsAnimator = ginsoTreeResurrectionScene.transform.Find("*heartResurrection/restoringHeartWaterRising/blockingWalls").GetComponentsInChildren<LegacyAnimator>();

                foreach (LegacyAnimator animator in blockingWallsAnimator)
                {
                    animator.Restart();
                    animator.StopAndSampleAtStart();
                }

                var risingWater = risingWaterGameObject.GetComponent<RisingWater>();
                risingWater.Speed = 5;

                // Resetting "vents" (as the game called them) in ginso escape end section
                var ginsoTreeWaterRisingEndSceneManagerScene = Scenes.Manager.GetSceneManagerScene("ginsoTreeWaterRisingEnd");

                if (ginsoTreeWaterRisingEndSceneManagerScene != null)
                {
                    var ginsoTreeWaterRisingEndSceneroot = ginsoTreeWaterRisingEndSceneManagerScene.SceneRoot;
                    var ginsoTreeWaterRisingEndParticleSteamVentTransform = ginsoTreeWaterRisingEndSceneroot.transform.Find("artBefore/artBefore/*particleSteamVent");
                    for (int i = 0; i < ginsoTreeWaterRisingEndParticleSteamVentTransform.childCount; i++)
                    {
                        var ventTransform = ginsoTreeWaterRisingEndParticleSteamVentTransform.GetChild(i);
                        var setupTransform = ventTransform.Find("setup");
                        if (setupTransform.gameObject.activeSelf)
                        {
                            setupTransform.Find("explosion").gameObject.SetActive(true);
                            setupTransform.gameObject.SetActive(false);
                        }
                    };
                    Scenes.Manager.UnloadScene(ginsoTreeWaterRisingEndSceneManagerScene, false, true);
                }

                // Set this flag back to pre trigger
                ginsoEvents.Value = 23;

                // Unloaing scenes seems to cleared up random debris on second trigger after TP
                Scenes.Manager.UnloadScene(ginsoTreeResurrectionSceneManagerScene, false, true);
                Scenes.Manager.UnloadScene(ginsoTreeWaterRisingBackgroundSceneManagerScene, false, true);

                // Need to create checkpoint on TP keep surrounding area loaded
                LocalGameState.IsPendingCheckpoint = true;
            }
        }

        [HarmonyPostfix, HarmonyPatch(nameof(TeleporterController.OnFinishedTeleporting))]
        private static void OnFinishedTeleportingPostfix()
        {
            if (LocalGameState.IsPendingCheckpoint)
            {
                GameController.Instance.CreateCheckpoint();

                LocalGameState.IsPendingCheckpoint = false;
            }

            RandomizerController.Instance.ShowRandomTip();
        }

        [HarmonyPostfix, HarmonyPatch(nameof(TeleporterController.BeginTeleportation))]
        private static void BeginTeleportationPostfix(GameMapTeleporter selectedTeleporter)
        {
            if (selectedTeleporter.Area.Area.AreaNameString == "Forlorn Ruins")
            {
                LocalGameState.TeleportNightberry = true;
            }
        }
    }
}

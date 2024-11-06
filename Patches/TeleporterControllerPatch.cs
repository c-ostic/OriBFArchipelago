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
        [HarmonyPrefix, HarmonyPatch(nameof(TeleporterController.OnFadedToBlack))]
        private static bool OnFadedToBlackPrefix()
        {

            // Reset ginso tree
            var ginsoSim = new WorldEvents();
            ginsoSim.MoonGuid = new MoonGuid(687998245, 1199897005, -1787166542, 576748618);
            int ginsoEventsValue = World.Events.Find(ginsoSim).Value;
            Console.WriteLine(ginsoEventsValue);
            if (ginsoEventsValue == 21 && !(LocalGameState.IsGinsoExit) && !(RandomizerManager.Receiver?.HasItem(InventoryItem.GinsoEscapeComplete) ?? true))
            {

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

                var blockingWallsAnimator = ginsoTreeResurrectionScene.transform.Find("*heartResurrection/restoringHeartWaterRising/blockingWalls").GetComponentsInChildren<LegacyAnimator>();

                foreach (LegacyAnimator animator in blockingWallsAnimator)
                {
                    animator.Restart();
                    animator.StopAndSampleAtStart();
                }

                World.Events.Find(ginsoSim).Value = 23;

                // Unloaing scenes seems to cleared up random debris on second trigger after TP
                // OriCore.Scenes.Manager.UnloadScene(ginsoTreeResurrectionSceneManagerScene, false, true);
                Scenes.Manager.UnloadAllScenes();
            }
            return true;
        }

        [HarmonyPostfix, HarmonyPatch(nameof(TeleporterController.OnFadedToBlack))]
        private static void OnFadedToBlackPostfix()
        {
            // Reset misty woods
            var mistySim = new WorldEvents();
            mistySim.MoonGuid = new MoonGuid(1061758509, 1206015992, 824243626, -2026069462);
            int value = World.Events.Find(mistySim).Value;
            if (value != 1 && value != 8)
            {
                World.Events.Find(mistySim).Value = 10;
            }
        }
    }
}

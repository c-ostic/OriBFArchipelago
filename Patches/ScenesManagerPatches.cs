using Core;
using HarmonyLib;
using OriBFArchipelago.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OriBFArchipelago.Patches
{
    [HarmonyPatch(typeof(ScenesManager), nameof(ScenesManager.Awake))]
    internal class ScenesManagerPatches
    {
        private static void PatchKuroMomentTreeFallCutsceneDuplicate(RuntimeSceneMetaData sceneMetaData)
        {
            CompoundCondition compCondition = (CompoundCondition) sceneMetaData.LoadingCondition;

            NotCondition previousCondition = (NotCondition) compCondition.Tests[0].Conditions[1];

            var innerCondition = previousCondition.Condition;

            compCondition.Tests[0].Conditions[1] = new IsInGinsoKuroCutsceneCondition();
            UnityEngine.Object.Destroy(innerCondition);
            UnityEngine.Object.Destroy(previousCondition);
        }

        private static void Postfix()
        {
            MoonGuid guid = new MoonGuid(-172843866, 436504499, -1307662934, 1026713766);
            RuntimeSceneMetaData kuroMomentTreeFallCutsceneDuplicateMetaData = Scenes.Manager.FindRuntimeSceneMetaData(guid);
            PatchKuroMomentTreeFallCutsceneDuplicate(kuroMomentTreeFallCutsceneDuplicateMetaData);
        }
    }

    public class IsInGinsoKuroCutsceneCondition : Condition
    {
        public bool IsTrue = true;

        public override bool Validate(IContext context)
        {
            return ((RandomizerManager.Receiver?.HasItem(InventoryItem.GinsoEscapeExit) ?? false) && !(RandomizerManager.Receiver?.HasItem(InventoryItem.GinsoEscapeComplete) ?? false)) == IsTrue;
        }
    }
}

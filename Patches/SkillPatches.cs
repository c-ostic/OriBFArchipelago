using Game;
using HarmonyLib;
using OriBFArchipelago.Core;
using OriBFArchipelago.MapTracker.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OriBFArchipelago.Patches
{
    [HarmonyPatch(typeof(GetAbilityPedestal), nameof(GetAbilityPedestal.ActivatePedestal))]
    internal class SkillPatch
    {
        private static bool Prefix(GetAbilityPedestal __instance)
        {
            try
            {
                //SKIP: StartCoroutine(MoveSeinToCenterSmoothly());
                DropCarryingItem();
                RestoreHealthAndEnergy();
                // SKIP: Characters.Sein.Controller.PlayAnimation(GetAbilityAnimation);
                // SKIP: Characters.Sein.PlayerAbilities.SetAbility(Ability, value: true);
                SetPedestalToCollected(__instance);
                PlayActivatePedestalAnimation(__instance);

                RandomizerManager.Connection.CheckLocation(__instance.MoonGuid);
                return false;
            }
            catch (Exception ex)
            {
                ModLogger.Error($"Error in SkillPatch prefix: {ex.Message}");
                ModLogger.Error(ex.StackTrace);
                return true; // Run original method as fallback if our patch fails
            }
        }
        private static void DropCarryingItem()
        {
            if ((bool)Characters.Sein.Abilities.Carry && Characters.Sein.Abilities.Carry.CurrentCarryable != null)
            {
                Characters.Sein.Abilities.Carry.CurrentCarryable.Drop();
            }
        }

        private static void RestoreHealthAndEnergy()
        {
            Characters.Sein.Mortality.Health.RestoreAllHealth();
            Characters.Sein.Energy.RestoreAllEnergy();
        }

        private static void SetPedestalToCollected(GetAbilityPedestal __instance)
        {
            var changeStateMethod = AccessTools.Method(typeof(GetAbilityPedestal), "ChangeState");
            var statesCompletedField = AccessTools.Field(typeof(GetAbilityPedestal).GetNestedType("States"), "Completed");
            object completedState = statesCompletedField.GetValue(null);
            changeStateMethod.Invoke(__instance, [completedState]);
        }

        private static void PlayActivatePedestalAnimation(GetAbilityPedestal __instance)
        {
            var activatePedestalSequenceField = AccessTools.Field(typeof(GetAbilityPedestal), "ActivatePedestalSequence");

            var sequence = activatePedestalSequenceField.GetValue(__instance);
            __instance.StartCoroutine(QuicklyFinishSequence(sequence, __instance.MoonGuid));

            //Show atleast some visuals that the tree is collected
            RunActivateTreeAnimation(__instance);
        }

        private static void RunActivateTreeAnimation(GetAbilityPedestal __instance)
        {
            SeinLevel seinLevelInstance = Characters.Sein?.Level;
            GameObject OnLevelUpGameObject = AccessTools.Field(typeof(SeinLevel), "OnLevelUpGameObject").GetValue(seinLevelInstance) as GameObject;

            GameObject gameObject = (GameObject)InstantiateUtility.Instantiate(OnLevelUpGameObject, Characters.Sein.Position, Quaternion.identity);
            TargetPositionFollower component = gameObject.GetComponent<TargetPositionFollower>();
            component.Target = __instance.transform;
            component.Target.position = new Vector3(component.Target.position.x, component.Target.position.y + 9, component.Target.position.z);
        }

        private static IEnumerator QuicklyFinishSequence(object sequence, MoonGuid moonGuid)
        {
            yield return new WaitForSeconds(0.1f);

            var actionsField = AccessTools.Field(sequence.GetType(), "Actions");
            var actions = actionsField.GetValue(sequence) as System.Collections.IList;

            List<object> filteredActions;
            // Filter in place - keep only actions containing specified descriptions            
            if (moonGuid == new MoonGuid("-1569470439 1154065095 1369872519 224313871"))
                filteredActions = actions.Cast<object>().Where(a => a.ToString().Contains("Create Checkpoint Action") || a.ToString().Contains("Activate trigger") || a.ToString().Contains("Set World Event Action")).ToList();
            else
                filteredActions = actions.Cast<object>().Where(a => a.ToString().Contains("Create Checkpoint Action")).ToList();

            // Replace original actions with filtered ones
            actions.Clear();
            foreach (var action in filteredActions)
                actions.Add(action);

            // Execute the sequence
            AccessTools.Method(sequence.GetType(), "Perform").Invoke(sequence, [null]);
        }

        private static void Postfix(GetAbilityPedestal __instance)
        {
            RandomizerManager.Connection.CheckLocation(__instance.MoonGuid);
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            var seinField = AccessTools.Field(typeof(Game.Characters), nameof(Game.Characters.Sein));
            var setAbilityMethod = AccessTools.Method(typeof(PlayerAbilities), nameof(PlayerAbilities.SetAbility));

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].LoadsField(seinField) && codes[i + 5].Calls(setAbilityMethod))
                    i += 6; // skips enabling the ability

                yield return codes[i];
            }
        }
    }

    [HarmonyPatch(typeof(GetAbilityAction), nameof(GetAbilityAction.Perform))]
    internal class FeatherSeinPatch
    {
        // This is called only when collecting sein or feather - no other skills
        private static bool Prefix(GetAbilityAction __instance)
        {
            if (LocationLookup.Get(__instance.MoonGuid)?.Name == "GlideSkillFeather")
            {
                RandomizerManager.Connection.CheckLocation(__instance.MoonGuid);
                return false;
            }
            else if (LocationLookup.Get(__instance.MoonGuid)?.Name == "Sein")
            {
                RandomizerManager.CollectSein();
                RandomizerMessager.instance.AddMessage($"Good luck with the randomizer! \nTip: " +
                    $"Press {Keybinder.ToString(KeybindAction.OpenTeleport)} to teleport. \n" +
                    $"Press {Keybinder.ToString(KeybindAction.Help)} to see other keybinds.");
            }
            return true;
        }
    }
}

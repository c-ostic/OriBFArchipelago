using Core;
using HarmonyLib;
using OriBFArchipelago.Core;
using System.Reflection;
using UnityEngine;
using Input = Core.Input;

namespace OriBFArchipelago.Patches
{
    /**
     * Modifed from https://github.com/sparkle-preference/OriDERandomizer/blob/master/modified_classes/SeinBashAttack.cs
     */
    [HarmonyPatch(typeof(SeinBashAttack), nameof(SeinBashAttack.UpdateNormalState))]
    internal class BashAttackUpdateStatePatch
    {
        private static bool Prefix(SeinBashAttack __instance)
        {
            // Get the private field from __instance required to recreate this function
            float m_timeRemainingOfBashButtonPress = (float)AccessTools.Field(typeof(SeinBashAttack), "m_timeRemainingOfBashButtonPress").GetValue(__instance);

            // Save whether a double bash was queued
            LocalGameState.WasDoubleBashQueued = LocalGameState.QueueDoubleBash;

            // Play the sound normally and when an extra is queued
            if (Input.Bash.OnPressed || LocalGameState.QueueDoubleBash)
            {
                LocalGameState.QueueDoubleBash = false;
                m_timeRemainingOfBashButtonPress = 0.5f;
                if (__instance.Sein.IsOnGround && 
                    __instance.Sein.Speed.x == 0f && 
                    !SeinAbilityRestrictZone.IsInside(SeinAbilityRestrictZoneMode.AllAbilities) && 
                    !__instance.Sein.Abilities.Carry.IsCarrying)
                {
                    __instance.Sein.Animation.Play(__instance.BackFlipAnimation, 10, null);
                    __instance.Sein.PlatformBehaviour.PlatformMovement.LocalSpeedY = __instance.BackFlipSpeed;
                    if ((!__instance.Sein.PlayerAbilities.BashBuff.HasAbility) ? __instance.StationaryBashSound : __instance.UpgradedStationaryBashSound)
                    {
                        Sound.Play((!__instance.Sein.PlayerAbilities.BashBuff.HasAbility) ? __instance.StationaryBashSound.GetSound(null) : __instance.UpgradedStationaryBashSound.GetSound(null), __instance.transform.position, null);
                    }
                }
            }

            if (m_timeRemainingOfBashButtonPress > 0f)
            {
                m_timeRemainingOfBashButtonPress -= Time.deltaTime;
                if ((Input.Bash.OnReleased || 
                    ((double)m_timeRemainingOfBashButtonPress <= 0.4 && 
                    (double)m_timeRemainingOfBashButtonPress >= 0.4 - (double)Time.deltaTime)) && 
                    !SeinAbilityRestrictZone.IsInside(SeinAbilityRestrictZoneMode.AllAbilities) && 
                    !__instance.Sein.Abilities.Carry.IsCarrying)
                {
                    __instance.BashFailed();
                }
                if (Input.Bash.Released || m_timeRemainingOfBashButtonPress <= 0f)
                {
                    m_timeRemainingOfBashButtonPress = 0f;
                }
            }

            // Begin bash normally or if a bash was queued
            if ((m_timeRemainingOfBashButtonPress > 0f || LocalGameState.WasDoubleBashQueued) && __instance.CanBash)
            {
                __instance.BeginBash();
            }

            // Invoke private methods of SeinBashAttack
            AccessTools.Method(typeof(SeinBashAttack), "HandleFindingTarget").Invoke(__instance, []);
            AccessTools.Method(typeof(SeinBashAttack), "UpdateTargetHighlight").Invoke(__instance, [__instance.Target]);

            return false;
        }
    }

    /**
     * Modifed from https://github.com/sparkle-preference/OriDERandomizer/blob/master/modified_classes/BashAttackGame.cs
     */
    [HarmonyPatch]
    internal class BashAttackGameFinishedPatch
    {
        // Need to use this method because BashAttackGame is an internal class
        static MethodBase TargetMethod()
        {
            return AccessTools.TypeByName("BashAttackGame").GetMethod("GameFinished", AccessTools.allDeclared);
        }

        private static void Postfix()
        {
            // Queue a double bash if the button is pressed and a double bash wasn't previously queued
            if (Keybinder.IsPressed(Keybinder.Action.DoubleBash) && !LocalGameState.WasDoubleBashQueued)
            {
                LocalGameState.QueueDoubleBash = true;
            }
            LocalGameState.WasDoubleBashQueued = false;
        }
    }
}

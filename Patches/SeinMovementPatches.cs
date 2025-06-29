using Game;
using HarmonyLib;
using OriBFArchipelago.Core;
using OriBFArchipelago.MapTracker.Core;
using System.Linq;
using UnityEngine;

namespace OriBFArchipelago.Patches
{
    internal class SeinMovementPatches
    {
        [HarmonyPatch(typeof(SeinMaxSpeedBasedOnDistance), "OnHorizontalInputCalculate")]
        [HarmonyPrefix]
        public static bool Prefix(SeinMaxSpeedBasedOnDistance __instance)
        {
            try
            {
                if (!RandomizerSettings.SkipCutscenes)
                    return RunOriginalMethod();

                if (IsSkipableLocation(__instance))
                    return false;

                LogPosition(__instance);
                return true;
            }
            catch (System.Exception ex)
            {
                ModLogger.Error($"Error in HorizontalInput patch: {ex.Message}");
            }

            return true; // Continue with original method execution
        }

        private static bool IsSkipableLocation(SeinMaxSpeedBasedOnDistance __instance)
        {
            var positions = new Vector3[] {
                    new Vector3(-164.6f, -261.7f, -3.1f) //Slow down near sein
                };
            var target = __instance.Target.position;
            float tolerance = 0.5f; // Adjust this value as needed

            if (positions.Any(skipPosition => Vector3.Distance(skipPosition, target) < tolerance))
            {
                Characters.Sein.PlatformBehaviour.LeftRightMovement.HorizontalInput *= 20 / 20;
                return true;
            }
            return false;
        }

        private static void LogPosition(SeinMaxSpeedBasedOnDistance __instance)
        {
            // Get the original horizontal input before modification
            float originalHorizontalInput = Characters.Sein.PlatformBehaviour.LeftRightMovement.HorizontalInput;

            // Calculate the distance
            float distance = Vector3.Distance(Characters.Sein.Position, __instance.Target.position);

            // Calculate the normalized distance
            float normalizedDistance = distance / __instance.Distance;

            // Get the speed multiplier from the curve
            float speedMultiplier = __instance.SpeedOverDistance.Evaluate(normalizedDistance);

            // Calculate what the new horizontal input will be
            float newHorizontalInput = originalHorizontalInput * speedMultiplier;

            ModLogger.Info($"HorizontalInput Debug - " +
                          $"Original Input: {originalHorizontalInput:F3}, " +
                          $"Distance: {distance:F2}, " +
                          $"Max Distance: {__instance.Distance:F2}, " +
                          $"Normalized Distance: {normalizedDistance:F3}, " +
                          $"Speed Multiplier: {speedMultiplier:F3}, " +
                          $"New Input: {newHorizontalInput:F3}, " +
                          $"Sein Position: {Characters.Sein.Position}, " +
                          $"Target Position: {__instance.Target.position}");
        }
        private static bool RunOriginalMethod()
        {
            return true;
        }

        private static bool SkipOriginalMethod()
        {
            return false;
        }
    }
}

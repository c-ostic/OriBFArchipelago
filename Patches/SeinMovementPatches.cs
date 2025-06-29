using Game;
using HarmonyLib;
using OriBFArchipelago.Core;
using OriBFArchipelago.MapTracker.Core;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OriBFArchipelago.Patches
{
    [HarmonyPatch]
    internal class SeinMovementPatches
    {
        private static List<Vector3> LoggedLocations { get; set; }
        [HarmonyPatch(typeof(SeinMaxSpeedBasedOnDistance), "OnHorizontalInputCalculate")]
        [HarmonyPrefix]
        public static bool Prefix(SeinMaxSpeedBasedOnDistance __instance)
        {
            try
            {
                if (!RandomizerSettings.SkipCutscenes)
                    return RunOriginalMethod();

                if (IsSkipableLocation(__instance))
                    return SkipOriginalMethod();

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
                    new Vector3(-164.6f, -261.7f, -3.1f), //Near sein collection
                    new Vector3(131.2f, -247.5f, 0.0f),  //Above sunken glades pedestal
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
            float tolerance = 0.5f; // Adjust this value as needed
            if (LoggedLocations == null)
                LoggedLocations = new List<Vector3>();

            if (LoggedLocations.Any(pos => Vector3.Distance(pos, __instance.Target.position) < tolerance))
            {
                return;
            }

            LoggedLocations.Add(__instance.Target.position);
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

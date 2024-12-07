using Game;
using HarmonyLib;
using OriBFArchipelago.Core;
using System;
using Input = Core.Input;

namespace OriBFArchipelago.Patches
{
    /**
     * Modified from https://github.com/sparkle-preference/OriDERandomizer/blob/master/modified_classes/SeinController.cs
     */
    [HarmonyPatch(typeof(SeinController), nameof(SeinController.HandleJumping))]
    internal class GrenadeJumpPatch
    {
        private static void Prefix(SeinController __instance)
        {
            if (__instance.IgnoreControllerInput || __instance.LockMovementInput || !__instance.CanMove)
            {
                return;
            }

            bool grenadeJumpPressed = false;
            bool grenadeJumpHeld = false;
            // only enable grenade jump if the assist is enabled
            if (RandomizerSettings.Get(RandomizerSettings.Setting.GrenadeJumpAssist) == 1)
            {
                grenadeJumpPressed = Keybinder.OnPressed(Keybinder.Action.GrenadeJump);
                grenadeJumpHeld = Keybinder.IsPressed(Keybinder.Action.GrenadeJump);
            }

            // If grenade jump is queued, activate the correct controls to initiate the jump
            if (LocalGameState.QueueGrenadeJump)
            {
                LocalGameState.QueueGrenadeJump = false;
                if (grenadeJumpHeld && 
                    CharacterState.IsActive(__instance.Sein.Abilities.WallChargeJump) && 
                    __instance.Sein.Abilities.GrabWall && 
                    __instance.Sein.Abilities.WallChargeJump.CanChargeJump && 
                    __instance.IsAimingGrenade)
                {
                    Input.LeftShoulder.IsPressed = true;
                    Input.Jump.IsPressed = true;
                }
            }

            // If grenade jump is pressed, queue grenade jump for the next frame
            if (grenadeJumpPressed && 
                CharacterState.IsActive(Characters.Sein.Abilities.WallChargeJump) &&
                Characters.Sein.Abilities.GrabWall &&
                Characters.Sein.Abilities.WallChargeJump.CanChargeJump &&
                Characters.Sein.Abilities.Grenade &&
                Characters.Sein.Abilities.Grenade.CanAim && 
                !__instance.IsAimingGrenade)
            {
                LocalGameState.QueueGrenadeJump = true;
                Input.LeftShoulder.IsPressed = true;
                Input.Jump.IsPressed = false;
            }
        }
    }
}

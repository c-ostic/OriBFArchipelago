using HarmonyLib;
using OriBFArchipelago.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OriBFArchipelago.Patches
{
    [HarmonyPatch(typeof(GetAbilityPedestal), nameof(GetAbilityPedestal.ActivatePedestal))]
    internal class SkillPatch
    {
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
                RandomizerMessager.instance.AddMessage($"Good luck with the randomizer! \nTip: " +
                    $"Press {Keybinder.ToString(KeybindAction.OpenTeleport)} to teleport. \n" +
                    $"Press {Keybinder.ToString(KeybindAction.Help)} to see other keybinds.");
            }
            return true;
        }
    }
}

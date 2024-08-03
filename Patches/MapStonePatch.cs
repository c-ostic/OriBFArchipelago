using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

namespace OriBFArchipelago.Patches
{
    [HarmonyPatch(typeof(MapStone), nameof(MapStone.FixedUpdate))]
    internal class MapStonePatch
    {
        private static void Grant(MapStone mapstone)
        {
            ArchipelagoManager.CheckLocationByGameObject(mapstone.gameObject);
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = instructions.ToList();

            var field = AccessTools.Field(typeof(MapStone), nameof(MapStone.CurrentState));

            for (int i = 0; i < codes.Count; i++)
            {
                yield return codes[i];

                if (codes[i].StoresField(field) && codes[i - 1].LoadsConstant((int)MapStone.State.Activated)) // this.CurrentState = State.Activated
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return CodeInstruction.Call(typeof(MapStonePatch), nameof(MapStonePatch.Grant)); // MapstonePatch.Grant(this)
                }
            }
        }
    }
}

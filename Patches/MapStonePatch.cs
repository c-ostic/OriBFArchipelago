using HarmonyLib;
using OriBFArchipelago.Core;
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
            RandomizerManager.Connection.CheckLocation(mapstone.GetComponent<VisibleOnWorldMap>().MoonGuid);
        }

        private static int GetCurrentMapstonesCount() => RandomizerManager.Receiver.GetCurrentMapstonesCount();

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = instructions.ToList();

            var field = AccessTools.Field(typeof(MapStone), nameof(MapStone.CurrentState));

            var seinField = AccessTools.Field(typeof(Game.Characters), nameof(Game.Characters.Sein));
            var mapstonesField = AccessTools.Field(typeof(SeinInventory), nameof(SeinInventory.MapStones));

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].LoadsField(seinField) && codes[i + 2].LoadsField(mapstonesField) && codes[i + 3].LoadsConstant(0))
                {
                    yield return CodeInstruction.Call(typeof(MapStonePatch), nameof(MapStonePatch.GetCurrentMapstonesCount));
                    i += 2;
                    continue;
                }

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

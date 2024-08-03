using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OriBFArchipelago.Patches
{
    [HarmonyPatch(typeof(SeinGrenadeAttack), "EnergyCostFinal", MethodType.Getter)]
    internal class FreeGrenadePatch
    {
        private static void Postfix(ref float __result)
        {
            __result = 0;
        }
    }
}

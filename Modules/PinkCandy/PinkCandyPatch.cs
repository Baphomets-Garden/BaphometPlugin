using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using InventorySystem.Items.Usables.Scp330;
using UniverseModule.Generic.Core;
using UniverseModule.Generic.Patching;

namespace BaphometPlugin.Modules.PinkCandy;

[AutomaticExecution]
[UniversePatch("Serpents Garden - Pink Candy", PatchType.Misc)]
public static class PinkCandyPatch
{
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(CandyPink), nameof(CandyPink.SpawnChanceWeight), MethodType.Getter)]
    public static IEnumerable<CodeInstruction> OnCandyPinkChance(IEnumerable<CodeInstruction> instructions)
    {
        yield return new CodeInstruction(OpCodes.Ldc_R4, 0.25f);
        yield return new CodeInstruction(OpCodes.Ret);
    }
}
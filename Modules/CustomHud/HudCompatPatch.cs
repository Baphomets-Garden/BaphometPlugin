using HarmonyLib;
using Hints;
using UniverseModule;
using UniverseModule.Generic.Core;
using UniverseModule.Generic.Patching;

namespace BaphometPlugin.Modules.CustomHud;

[AutomaticExecution]
[UniversePatch("SerpentsGarden - Hud", PatchType.Misc)]
public static class HudCompatPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(HintDisplay), nameof(HintDisplay.Show))]
    public static bool OnSendingHint(HintDisplay __instance, Hint hint)
    {
        var type = hint.GetType();
            
        if (type == typeof(TranslationHint))
            return false;

        if (type != typeof(TextHint)) return true;
        
        var t = hint as TextHint;
        __instance.gameObject.GetUniversePlayer().SendHudHint(ScreenZone.CenterBottom, t.Text, t.DurationScalar);
        return false;
    }
}
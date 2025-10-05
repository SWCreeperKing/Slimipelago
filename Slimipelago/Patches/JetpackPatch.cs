using HarmonyLib;

namespace Slimipelago.Patches;

[PatchAll]
public static class JetpackPatch
{
    public static EnergyJetpack Jetpack;

    [HarmonyPatch(typeof(EnergyJetpack), "Start"), HarmonyPostfix]
    public static void JetpackInit(EnergyJetpack __instance)
    {
        Jetpack = __instance;
    }
}
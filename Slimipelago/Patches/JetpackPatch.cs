using HarmonyLib;

namespace Slimipelago.Patches;

[PatchAll]
public static class JetpackPatch
{
    public static EnergyJetpack Jetpack;
    public static bool EnableJetpack = false;

    [HarmonyPatch(typeof(EnergyJetpack), "Start"), HarmonyPostfix]
    public static void JetpackInit(EnergyJetpack __instance)
    {
        Jetpack = __instance;
    }
    
    [HarmonyPatch(typeof(EnergyJetpack), "CanStart_Jetpack"), HarmonyPrefix]
    public static void HasJetpack(EnergyJetpack __instance)
    {
        PlayerModelPatch.Model.hasJetpack = EnableJetpack;
    }
}
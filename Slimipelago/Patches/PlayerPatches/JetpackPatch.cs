using HarmonyLib;
using Slimipelago.Patches.UiPatches;

namespace Slimipelago.Patches.PlayerPatches;

[PatchAll]
public static class JetpackPatch
{
    public static EnergyJetpack Jetpack;
    public static bool EnableJetpack = false;

    [HarmonyPatch(typeof(EnergyJetpack), "Start"), HarmonyPostfix]
    public static void JetpackInit(EnergyJetpack __instance)
    {
        Jetpack = __instance;
        MainMenuPatch.OnGamePotentialExit += () => Jetpack = null;
    }
    
    [HarmonyPatch(typeof(EnergyJetpack), "CanStart_Jetpack"), HarmonyPrefix]
    public static void HasJetpack(EnergyJetpack __instance)
    {
        PlayerModelPatch.Model.hasJetpack = EnableJetpack;
    }
}
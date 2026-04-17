using HarmonyLib;
using MonomiPark.SlimeRancher.DataModel;
using Slimipelago.Archipelago;
using Slimipelago.Patches.UiPatches;

namespace Slimipelago.Patches.PlayerPatches;

[PatchAll]
public static class JetpackPatch
{
    // public static EnergyJetpack Jetpack;

    // [HarmonyPatch(typeof(EnergyJetpack), "Start"), HarmonyPostfix]
    // public static void JetpackInit(EnergyJetpack __instance)
    // {
    //     try
    //     {
    //         Jetpack = __instance;
    //         MainMenuPatch.OnGamePotentialExit += () => Jetpack = null;
    //     }
    //     catch (Exception e) { Core.Log.Error(e); }
    // }

    [HarmonyPatch(typeof(EnergyJetpack), "CanStart_Jetpack"), HarmonyPrefix]
    public static bool HasJetpack(EnergyJetpack __instance, ref bool __result, PlayerModel ___model,
        PlayerState ___playerState, float ___jetpackEnergyThreshold)
    {
        __result = ApSlimeClient.EnableJetpack;
        if (!__result) return false;

        if (___playerState.GetCurrEnergy() >= (double)___jetpackEnergyThreshold * ___model.jetpackEfficiency)
        {
            __result = true;
        }
        else
        {
            __instance.jetpackAudio.Cue = __instance.jetpackNoEnergyCue;
            __instance.jetpackAudio.Play();
        }
        return false;
    }
}
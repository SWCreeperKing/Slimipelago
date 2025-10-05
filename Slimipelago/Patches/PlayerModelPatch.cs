using HarmonyLib;
using MonomiPark.SlimeRancher.DataModel;

namespace Slimipelago.Patches;

[PatchAll]
public static class PlayerModelPatch
{
    public static PlayerModel Model;
    public static bool EnableRecovery = true;
    
    [HarmonyPatch(typeof(PlayerModel), "Init"), HarmonyPostfix]
    public static void PlayerInit(PlayerModel __instance)
    {
        Model = __instance;
        
        if (Model.upgrades.Contains(PlayerState.Upgrade.JETPACK))
        {
            Model.upgrades.Remove(PlayerState.Upgrade.JETPACK);
        }

        Model.hasJetpack = false;
        
        Core.Log.Msg("PlayerInit");
    }

    [HarmonyPatch(typeof(PlayerModel), "Recover"), HarmonyPrefix]
    public static bool Regen(PlayerModel __instance) => EnableRecovery;
}
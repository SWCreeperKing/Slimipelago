using HarmonyLib;
using MonomiPark.SlimeRancher.DataModel;
using Slimipelago.Patches.UiPatches;

namespace Slimipelago.Patches.PlayerPatches;

[PatchAll]
public static class PlayerModelPatch
{
    public static PlayerModel Model;
    public static WorldModel WorldModel;
    public static bool EnableRecovery = true;
    
    [HarmonyPatch(typeof(PlayerState), "SetModel"), HarmonyPostfix]
    public static void SetModel(PlayerModel model)
    {
        try
        {
            Model = model;
            MainMenuPatch.OnGamePotentialExit += () => Model = null;
            Core.Log.Msg("PlayerInit");
        }
        catch (Exception e)
        {
            Core.Log.Error(e);
        }
    }
    
    [HarmonyPatch(typeof(PlayerModel), "Recover"), HarmonyPrefix]
    public static bool Regen(PlayerModel __instance) => EnableRecovery;

    [HarmonyPatch(typeof(PlayerModel), "SetWorldModel"), HarmonyPrefix]
    public static void SetWorldModel(PlayerModel __instance, WorldModel worldModel)
    {
        Model ??= __instance;
        WorldModel = worldModel;
        MainMenuPatch.OnGamePotentialExit += () =>
        {
            WorldModel = null;
            Model = null;
        };
    }
}
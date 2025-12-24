using HarmonyLib;

namespace Slimipelago.Patches.PlayerPatches;

[PatchAll]
public static class IntroPatch
{
    [HarmonyPatch(typeof(IntroUI), "Awake"), HarmonyPrefix]
    public static bool Awake(IntroUI __instance)
    {
        Core.Log.Msg("New Save");
        Destroyer.Destroy(__instance.gameObject, "IntroPatch.Awake");
        return false;
    }
}
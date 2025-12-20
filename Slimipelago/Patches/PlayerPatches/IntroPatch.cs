using HarmonyLib;
using Object = UnityEngine.Object;

namespace Slimipelago.Patches.PlayerPatches;

[PatchAll]
public static class IntroPatch
{
    [HarmonyPatch(typeof(IntroUI), "Awake"), HarmonyPrefix]
    public static bool Awake(IntroUI __instance)
    {
        Core.Log.Msg("New Save");
        Object.Destroy(__instance.gameObject);
        return false;
    }
}
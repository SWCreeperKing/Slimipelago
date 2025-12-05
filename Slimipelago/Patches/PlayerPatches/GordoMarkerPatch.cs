using HarmonyLib;

namespace Slimipelago.Patches.PlayerPatches;

[PatchAll]
public static class GordoMarkerPatch
{
    [HarmonyPatch(typeof(GordoDisplayOnMap), "ShowOnMap"), HarmonyPrefix]
    public static bool DisplayMarker(GordoDisplayOnMap __instance, ref bool __result)
    {
        if (__instance.name.StartsWith("gordoGold") || __instance.GetPrivateField<GordoEat>("gordoEat").HasPopped())
            return __result = false;
        var cell = __instance.GetComponentInParent<CellDirector>();
        if (cell is null) return false;

        __result = true;
        return false;
    }
}
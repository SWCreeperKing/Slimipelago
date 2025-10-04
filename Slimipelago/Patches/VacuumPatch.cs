using HarmonyLib;

namespace Slimipelago.Patches;

[HarmonyPatchAll]
public static class VacuumPatch
{
    [HarmonyPatch(typeof(Vacuumable), "TryConsume"), HarmonyPrefix]
    public static bool CanVacuum(Vacuumable __instance, ref bool __result)
    {
        var name = __instance.name.Replace("(Clone)", "").ToLower();
        if (!name.Contains("slime")) return true;
        name = name.Replace("slime", "");
        Core.Log.Msg($"Try vacuum slime: [{name}]");
        __result = false;
        return false;
    }
}
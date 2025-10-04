using HarmonyLib;

namespace Slimipelago.Patches;

[HarmonyPatchAll]
public static class PlayerTrackerPatch
{
    [HarmonyPatch(typeof(PlayerZoneTracker), "OnEntered"), HarmonyPrefix]
    public static void AreaEntered(PlayerZoneTracker __instance, ZoneDirector.Zone zone)
    {
        Core.Log.Msg($"Zone entered: [{zone}]");
    }

    // [HarmonyPatch(typeof(DisplayOnMap), "ShowOnMap"), HarmonyPrefix]
    // public static bool ShowOnMap(ref bool __result)
    // {
    //     __result = true;
    //     return false;
    // }
}
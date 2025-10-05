using HarmonyLib;

namespace Slimipelago.Patches;

[HarmonyPatchAll]
public static class PlayerTrackerPatch
{
    public static HashSet<ZoneDirector.Zone> AllowedZones = [ZoneDirector.Zone.REEF];
    
    [HarmonyPatch(typeof(PlayerZoneTracker), "OnEntered"), HarmonyPrefix]
    public static void AreaEntered(PlayerZoneTracker __instance, ZoneDirector.Zone zone)
    {
        if (zone is ZoneDirector.Zone.RANCH) return;
        if (AllowedZones.Contains(zone)) return;
        Core.BanishPlayer();
        // Core.Log.Msg($"Zone entered: [{zone}]");
    }

    // [HarmonyPatch(typeof(DisplayOnMap), "ShowOnMap"), HarmonyPrefix]
    // public static bool ShowOnMap(ref bool __result)
    // {
    //     __result = true;
    //     return false;
    // }
}
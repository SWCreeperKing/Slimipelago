using HarmonyLib;

namespace Slimipelago.Patches;

[PatchAll]
public static class PlayerTrackerPatch
{
    public static ZoneDirector.Zone CurrentPlayerZone;
    public static HashSet<ZoneDirector.Zone> AllowedZones = [ZoneDirector.Zone.REEF];
    
    [HarmonyPatch(typeof(PlayerZoneTracker), "OnEntered"), HarmonyPrefix]
    public static void AreaEntered(PlayerZoneTracker __instance, ZoneDirector.Zone zone)
    {
        CurrentPlayerZone = zone;
        Core.Log.Msg($"Zone entered: [{zone}]");
        if (zone is ZoneDirector.Zone.RANCH) return;
        if (AllowedZones.Contains(zone)) return;
        // Core.BanishPlayer();
    }

    // [HarmonyPatch(typeof(DisplayOnMap), "ShowOnMap"), HarmonyPrefix]
    // public static bool ShowOnMap(ref bool __result)
    // {
    //     __result = true;
    //     return false;
    // }
}
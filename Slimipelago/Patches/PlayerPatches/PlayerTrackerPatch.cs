using HarmonyLib;
using Slimipelago.Added;

namespace Slimipelago.Patches.PlayerPatches;

[PatchAll]
public static class PlayerTrackerPatch
{
    public static HashSet<string> AllowedZones = [];

    public static Dictionary<ZoneDirector.Zone, string> ZoneTypeToName = new()
    {
        [ZoneDirector.Zone.NONE] = "None", 
        [ZoneDirector.Zone.RANCH] = "Ranch", 
        [ZoneDirector.Zone.REEF] = "Dry Reef", 
        [ZoneDirector.Zone.QUARRY] = "Indigo Quarry", 
        [ZoneDirector.Zone.MOSS] = "Moss Blanket", 
        [ZoneDirector.Zone.DESERT] = "Glass Desert", 
        // [ZoneDirector.Zone.SEA] = "Slime Sea", 
        [ZoneDirector.Zone.RUINS] = "Ruins", 
        [ZoneDirector.Zone.RUINS_TRANSITION] = "Ruins Transition", 
    };
    
    [HarmonyPatch(typeof(PlayerZoneTracker), "OnEntered"), HarmonyPrefix]
    public static void AreaEntered(PlayerZoneTracker __instance, ZoneDirector.Zone zone)
    {
        // Core.Log.Msg($"Zone entered: [{zone}]");
        if (!PlayerStatePatch.FirstUpdate) return;
        if (zone is ZoneDirector.Zone.NONE or ZoneDirector.Zone.RANCH) return;
        if (ZoneTypeToName.ContainsKey(zone) && AllowedZones.Contains(ZoneTypeToName[zone])) return;
        Playground.BanishPlayer();
    }
}
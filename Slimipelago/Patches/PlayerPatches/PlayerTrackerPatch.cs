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
        [ZoneDirector.Zone.RUINS] = "Ancient Ruins",
        [ZoneDirector.Zone.RUINS_TRANSITION] = "Ancient Ruins Transition",
    };

    [HarmonyPatch(typeof(PlayerZoneTracker), "OnEntered"), HarmonyPrefix]
    public static void AreaEntered(PlayerZoneTracker __instance, ZoneDirector.Zone zone)
    {
        if (!PlayerStatePatch.FirstUpdate) return;
        if (zone is ZoneDirector.Zone.NONE or ZoneDirector.Zone.RANCH or ZoneDirector.Zone.SEA
            or ZoneDirector.Zone.SLIMULATIONS or ZoneDirector.Zone.WILDS or ZoneDirector.Zone.VALLEY
            or ZoneDirector.Zone.VIKTOR_LAB) return;
        if (ZoneTypeToName.ContainsKey(zone) && AllowedZones.Contains(ZoneTypeToName[zone])) return;
        Core.Log.Msg($"Player Entered Restricted Area: [{zone}]");
        Playground.BanishPlayer();
    }
}
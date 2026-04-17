using HarmonyLib;
using Slimipelago.Added;
using Slimipelago.Patches.UiPatches;
using static Slimipelago.GameLoader;

namespace Slimipelago.Patches.PlayerPatches;

[PatchAll]
public static class PlayerTrackerPatch
{
    public static HashSet<string> AllowedZones = [];

    public static Dictionary<ZoneDirector.Zone, string> ZoneTypeToName;

    [HarmonyPatch(typeof(PlayerZoneTracker), "OnEntered"), HarmonyPrefix]
    public static void AreaEntered(PlayerZoneTracker __instance, ZoneDirector.Zone zone)
    {
        try
        {
            if (!PlayerStatePatch.FirstUpdate) return;
            if (!ZoneTypeToName.TryGetValue(zone, out var zoneString)) return;
            if (AllowedZones.Contains(zoneString)) return;
            Core.Log.Msg($"Player Entered Restricted Area: [{zone}]");
            PopupPatch.AddItemToQueue(
                new ApPopup(Spritemap["got_trap"], "Restricted", $"Player Entered Restricted Area: [{zone}]", "BEGONE")
            );
            Playground.BanishPlayer();
        }
        catch (Exception e)
        {
            Core.Log.Error(e);
        }
    }
}
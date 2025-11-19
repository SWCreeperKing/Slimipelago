using HarmonyLib;
using MonomiPark.SlimeRancher.Regions;

namespace Slimipelago.Patches.Interactables;

[PatchAll]
public static class MapFragmentPatch
{
    [HarmonyPatch(typeof(MapDataEntry), "Start"), HarmonyPrefix]
    public static bool MapMarker(MapDataEntry __instance)
    {
        var region = __instance.GetComponentInParent<Region>();
        GameLoader.MakeMarker("map", __instance.transform.position, null, region.setId);
        return false;
    }
    
    [HarmonyPatch(typeof(MapDataEntry), "Activate"), HarmonyPrefix]
    public static void ActivateMapDataEntry(MapDataEntry __instance)
    {
        Core.Log.Msg($"Map activated: [{__instance.zone}]");
        var hash = __instance.transform.position.HashPos();

        var color = GameLoader.MarkerDictionary[hash].Image.color;
        color.a /= 4;
        GameLoader.MarkerDictionary[hash].Image.color = color;
    }
}
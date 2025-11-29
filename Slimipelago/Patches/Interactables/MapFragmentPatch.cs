using HarmonyLib;

namespace Slimipelago.Patches.Interactables;

[PatchAll]
public static class MapFragmentPatch
{
    [HarmonyPatch(typeof(MapDataEntry), "Start"), HarmonyPrefix]
    public static bool MapMarker(MapDataEntry __instance)
    {
        __instance.InteractableInstanced("map");
        return false;
    }
    
    [HarmonyPatch(typeof(MapDataEntry), "Activate"), HarmonyPrefix]
    public static bool ActivateMapDataEntry(MapDataEntry __instance)
    {
        __instance.InteractableInteracted("Map");
        return false;
    }
}
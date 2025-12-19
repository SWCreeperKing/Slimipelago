using HarmonyLib;

namespace Slimipelago.Patches.Interactables;

[PatchAll]
public static class MapFragmentPatch
{
    [HarmonyPatch(typeof(MapDataEntry), "Start"), HarmonyPrefix]
    public static void MapMarker(MapDataEntry __instance)
    {
        __instance.InteractableInstanced("map");
    }

    [HarmonyPatch(typeof(MapDataEntry), "Activate"), HarmonyPrefix]
    public static void ActivateMapDataEntry(MapDataEntry __instance) => __instance.InteractableInteracted("Map");
}
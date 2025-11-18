using HarmonyLib;
using MonomiPark.SlimeRancher.Regions;

namespace Slimipelago.Patches;

// [PatchAll]
public static class GordoPatch
{
    [HarmonyPatch(typeof(GordoEat), "Start"), HarmonyPostfix]
    public static void MapGordos(GordoEat __instance)
    {
        var region = __instance.GetComponentInParent<Region>();
        GameLoader.MakeMarker("gordo", __instance.transform.position, null, region.setId);
    }

    [HarmonyPatch(typeof(GordoEat), "GetTargetCount"), HarmonyPatch]
    public static bool GordoEatLess(ref int count)
    {
        count = 5;
        return false;
    }
}
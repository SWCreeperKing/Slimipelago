using HarmonyLib;
using MonomiPark.SlimeRancher.Regions;

namespace Slimipelago.Patches;

[PatchAll]
public static class PodPatch
{
    private static readonly string[] PodType = ["mk1", "mk2", "mk3", "cosmetic"];

    [HarmonyPatch(typeof(TreasurePod), "Awake"), HarmonyPostfix]
    public static void MarkPod(TreasurePod __instance)
    {
        var region = __instance.GetComponentInParent<Region>();
        GameLoader.MakeMarker(PodType[GetType(__instance)], __instance.transform.position, null, region.setId);
    }

    public static int GetType(TreasurePod pod)
    {
        var name = pod.name.ToLower();
        if (name.Contains("rank1")) return 1;
        if (name.Contains("rank2")) return 2;
        if (name.Contains("rank3")) return 3;
        if (name.Contains("cosmetic")) return 4;
        throw new ArgumentException($"Pod: [{name}] has no valid type");
    }
}
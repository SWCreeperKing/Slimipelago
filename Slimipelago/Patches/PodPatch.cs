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
        var type = GetType(__instance);
        GameLoader.MakeMarker(PodType[type], __instance.transform.position, null, region.setId);
        // Core.Log.Msg($"Required: [{__instance.requiredUpgrade}]");
    }

    public static int GetType(TreasurePod pod)
    {
        var name = pod.name.ToLower();
        if (name.Contains("rank1")) return 0;
        if (name.Contains("rank2")) return 1;
        if (name.Contains("rank3")) return 2;
        if (name.Contains("cosmetic")) return 3;
        throw new ArgumentException($"Pod: [{name}] has no valid type");
    }

    // [HarmonyPatch(typeof(TreasurePod), "Activate"), HarmonyPrefix]
    // public static void OpenPod(TreasurePod __instance)
    // {
    //     Core.Log.Msg(
    //         $"pod [type: {GetType(__instance) switch { 0 => "I", 1 => "II", 2 => "III", 3 => "Cosmetic" }}] id: {__instance.transform.position.HashPos()}");
    // }
}
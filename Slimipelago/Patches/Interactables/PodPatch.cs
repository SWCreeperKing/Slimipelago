using HarmonyLib;

namespace Slimipelago.Patches.Interactables;

[PatchAll]
public static class PodPatch
{
    private static readonly string[] PodType = ["mk1", "mk2", "mk3", "cosmetic"];

    [HarmonyPatch(typeof(TreasurePod), "Awake"), HarmonyPostfix]
    public static void MarkPod(TreasurePod __instance)
    {
        __instance.InteractableInstanced(PodType[GetType(__instance)]);
    }

    public static int GetType(TreasurePod pod)
    {
        var name = pod.name.ToLower();
        if (name.Contains("rank1")) return 0;
        if (name.Contains("rank2")) return 1;
        if (name.Contains("rank3")) return 2;
        return name.Contains("cosmetic") ? 3 : throw new ArgumentException($"Pod: [{name}] has no valid type");
    }

    [HarmonyPatch(typeof(TreasurePod), "Activate"), HarmonyPrefix]
    public static void OpenPod(TreasurePod __instance)
    {
        __instance.InteractableInteracted("Pod");
    }
}
using HarmonyLib;
using Slimipzelago.Archipelago;

namespace Slimipelago.Patches.Interactables;

[PatchAll]
public static class PodPatch
{
    private static readonly string[] PodType = ["mk1", "mk2", "mk3", "cosmetic"];

    [HarmonyPatch(typeof(TreasurePod), "Awake"), HarmonyPostfix]
    public static void MarkPod(TreasurePod __instance)
    {
        __instance.InteractableInstanced(PodType[GetType(__instance)]);
        
        if (!ApSlimeClient.LocationDictionary.TryGetValue(__instance.transform.position.HashPos(), out var itemName))
            return;
        if (!ApSlimeClient.Client.MissingLocations.Contains(itemName) || __instance.CurrState is TreasurePod.State.LOCKED) return;
        __instance.CurrState = TreasurePod.State.LOCKED;
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
    public static void OpenPod(TreasurePod __instance) => __instance.InteractableInteracted("Pod");

    [HarmonyPatch(typeof(TreasurePod), "HasKey"), HarmonyPrefix]
    public static bool HasKey(TreasurePod __instance, ref bool __result)
    {
        __result = !__instance.needsUpgrade ||
                   (ApSlimeClient.ItemCache.TryGetValue("Progressive Treasure Cracker", out var value) &&
                    value >= (int)__instance.requiredUpgrade - 99);
        return false;
    }

    [HarmonyPatch(typeof(TreasurePod), "HasAnyKey"), HarmonyPrefix]
    public static bool HasAnyKey(TreasurePod __instance, ref bool __result)
    {
        __result = ApSlimeClient.ItemCache.TryGetValue("Progressive Treasure Cracker", out var value) && value > 0;
        return false;
    }
}
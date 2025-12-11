using HarmonyLib;

namespace Slimipelago.Patches.GadgetPatches;

[PatchAll]
public static class ExtractorPatch
{
    [HarmonyPatch(typeof(Extractor), "StartNewCycleOrDestroy"), HarmonyPrefix]
    public static void Start(Extractor __instance) => __instance.infiniteCycles = true;
}
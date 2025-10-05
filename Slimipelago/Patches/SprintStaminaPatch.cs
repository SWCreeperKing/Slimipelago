using HarmonyLib;

namespace Slimipelago.Patches;

[PatchAll]
public static class SprintStaminaPatch
{
    [HarmonyPatch(typeof(StaminaRun), "Update"), HarmonyPrefix]
    public static bool StopStaminaRunDrain() => false;
}
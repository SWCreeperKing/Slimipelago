using HarmonyLib;

namespace Slimipelago.Patches.PlayerPatches;

[PatchAll]
public static class SprintStaminaPatch
{
    public static bool StopStaminaRunUsage = false; 
    
    [HarmonyPatch(typeof(StaminaRun), "Update"), HarmonyPrefix]
    public static bool StopStaminaRunDrain() => !StopStaminaRunUsage;
}
using HarmonyLib;
using Slimipelago.Patches.PlayerPatches;

namespace Slimipelago.Patches.Interactables;

[PatchAll]
public static class PuzzleSlotPatch
{
    [HarmonyPatch(typeof(PuzzleSlot), "OnTriggerEnter"), HarmonyPrefix]
    public static bool PreventTeleport(PuzzleSlot __instance)
    {
        try
        {
            return __instance.catchId is not Identifiable.Id.QUANTUM_PLORT
                   || PlayerTrackerPatch.AllowedZones.Contains("Glass Desert");
        }
        catch (Exception e)
        {
            Core.Log.Error(e);
        }
        return true;
    }
}
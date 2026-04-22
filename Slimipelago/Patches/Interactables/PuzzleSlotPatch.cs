using HarmonyLib;
using Slimipelago.Archipelago;
using Slimipelago.Patches.PlayerPatches;
using static Slimipelago.Archipelago.ItemConstants;

namespace Slimipelago.Patches.Interactables;

[PatchAll]
public static class PuzzleSlotPatch
{
    [HarmonyPatch(typeof(PuzzleSlot), "OnTriggerEnter"), HarmonyPrefix]
    public static bool PreventTeleport(PuzzleSlot __instance)
    {
        var hasJetpack = ApSlimeClient.EnableJetpack;
        var energyLevel = ApSlimeClient.ItemCache.TryGetValue(MaxEnergy, out var val1) ? val1 : 0;
        
        try
        {
            switch (__instance.catchId)
            {
                case Identifiable.Id.HONEY_PLORT:
                    LogicHandler.PreviousLines.Clear();
                    return LogicHandler.CheckPlorts(["Honey Plort"], hasJetpack, energyLevel);
                case Identifiable.Id.RAD_PLORT:
                    LogicHandler.PreviousLines.Clear();
                    return LogicHandler.CheckPlorts(["Rad Plort"], hasJetpack, energyLevel);
                case Identifiable.Id.QUANTUM_PLORT:
                    return PlayerTrackerPatch.AllowedZones.Contains("Glass Desert");
            }
        }
        catch (Exception e)
        {
            Core.Log.Error(e);
        }
        return true;
    }
}
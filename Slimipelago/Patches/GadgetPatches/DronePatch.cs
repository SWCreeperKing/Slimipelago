using HarmonyLib;

namespace Slimipelago.Patches.GadgetPatches;

[PatchAll]
public static class DronePatch
{
    [HarmonyPatch(typeof(Drone), "LateUpdate"), HarmonyPostfix]
    public static void Update(Drone __instance)
    {
        var battery = __instance.station.battery;
        if (battery.meter.localScale.y > .9f) return;
        battery.AddLiquid(Identifiable.Id.WATER_LIQUID, 1);
    }
}
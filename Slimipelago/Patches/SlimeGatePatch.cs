using HarmonyLib;
using MonomiPark.SlimeRancher.Regions;

namespace Slimipelago.Patches;

[PatchAll]
public static class SlimeGatePatch
{
    [HarmonyPatch(typeof(AccessDoor), "Awake"), HarmonyPostfix]
    public static void MapGate(AccessDoor __instance)
    {
        var slimeGate = __instance.GetComponentInChildren<SlimeGateActivator>();
        if (slimeGate is null) return;
        
        var region = slimeGate.GetComponentInParent<Region>();
        GameLoader.MakeMarker("gate", slimeGate.transform.position, null, region.setId);
    }
    
    // [HarmonyPatch(typeof(SlimeGateActivator), )]
    // public static void MapGate(SlimeGateActivator __instance)
    // {
    //     
    // }
}
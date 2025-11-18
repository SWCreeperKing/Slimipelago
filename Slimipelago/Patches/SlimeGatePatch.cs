using HarmonyLib;
using MonomiPark.SlimeRancher.Regions;

namespace Slimipelago.Patches;

[PatchAll]
public static class SlimeGatePatch
{
    public static int Count = 0;
    
    [HarmonyPatch(typeof(AccessDoor), "Awake"), HarmonyPostfix]
    public static void MapGate(AccessDoor __instance)
    {
        var slimeGate = __instance.GetComponentInChildren<SlimeGateActivator>();
        if (slimeGate is null) return;

        var region = slimeGate.GetComponentInParent<Region>();
        // var hash = slimeGate.transform.position.HashPos();
        // Core.Log.Msg($"({(ApSlimeClient.LocationDictionary.ContainsKey(hash) ? 'X' : ' ')}) Gate: [{++Count}] in [{region}]");
        GameLoader.MakeMarker("gate", slimeGate.transform.position, null, region.setId);
    }
    
    // [HarmonyPatch(typeof(SlimeGateActivator), )]
    // public static void MapGate(SlimeGateActivator __instance)
    // {
    //     
    // }
}
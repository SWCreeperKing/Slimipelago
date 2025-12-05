using HarmonyLib;
using Slimipelago.Added;
using Slimipzelago.Archipelago;
using UnityEngine;

namespace Slimipelago.Patches.Interactables;

[PatchAll]
public static class AccessDoorPatch
{
    public static Dictionary<Vector3, ItemDisplayOnMap> TeleportMarkers = [];

    public static Dictionary<string, Vector3> HashToTeleportLocation = new()
    {
        ["[x:-252|y:10|z:14]"] = GameLoader.ReefBeach, //	Gate - Dry Reef to Ring Island
        ["[x:-174|y:2|z:331]"] = GameLoader.Moss, //	Gate - Dry Reef to Moss Blanket
        ["[x:25|y:13|z:181]"] = GameLoader.Quarry, //	Gate - Dry Reef to Indigo Quarry
        ["[x:-29|y:13|z:437]"] = GameLoader.RuinsTransition, //	Gate - Moss Blanket to Ancient Ruins Transition
        ["[x:35|y:6|z:394]"] = GameLoader.RuinsTransition, //	Gate - Indigo Quarry to Ancient Ruins Transition
        ["[x:36|y:13|z:526]"] = GameLoader.Ruins, //	Gate - Ancient Ruins Transition to Ancient Ruins
        // ["[x:57|y:-4|z:980]"] = , //	Gate - Ancient Ruins to Glass Desert
        ["[x:39|y:1030|z:396]"] = GameLoader.Glass, //	Gate - Glass Desert Eastern 
        ["[x:-177|y:1025|z:451]"] = GameLoader.Glass, //	Gate - Glass Desert Western
    };

    public static AccessDoor LabDoor;
    public static AccessDoor GrottoDoor;
    public static AccessDoor OvergrowthDoor;
    public static AccessDoor DocksDoor;

    [HarmonyPatch(typeof(AccessDoor), "Awake"), HarmonyPostfix]
    public static void Init(AccessDoor __instance)
    {
        var id = __instance.lockedRegionId;

        switch (id)
        {
            case PediaDirector.Id.LAB:
                LabDoor = __instance;
                break;
            case PediaDirector.Id.GROTTO:
                GrottoDoor = __instance;
                break;
            case PediaDirector.Id.OVERGROWTH:
                OvergrowthDoor = __instance;
                break;
            case PediaDirector.Id.DOCKS:
                DocksDoor = __instance;
                break;
            default:
                return;
        }

        foreach (var child in __instance.gameObject.GetChildren().Where(obj => obj.name.ToLower() != "barrier"))
        {
            child.SetActive(false);
        }
    }

    [HarmonyPatch(typeof(AccessDoor), "Update"), HarmonyPrefix]
    public static void Update(AccessDoor __instance)
    {
        RunDoorCheck(__instance);
    }

    public static void RunDoorCheck(AccessDoor door)
    {
        if (door.CurrState is not AccessDoor.State.OPEN) return;
        var hash = door.transform.position.HashPos();
        
        if (!HashToTeleportLocation.TryGetValue(hash, out var destPos)) return;
        if (TeleportMarkers.ContainsKey(destPos)) return;
        TeleportMarkers[destPos] = GameLoader.MakeTeleporterMarker(destPos);
    }
}
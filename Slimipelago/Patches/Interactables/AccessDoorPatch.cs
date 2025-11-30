using HarmonyLib;

namespace Slimipelago.Patches.Interactables;

[PatchAll]
public static class AccessDoorPatch
{
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
}
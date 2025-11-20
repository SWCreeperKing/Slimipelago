using HarmonyLib;
using Slimipelago.Patches.UiPatches;

namespace Slimipelago.Patches.Expansions;

[PatchAll]
public static class AccessDoorPatch
{
    public static AccessDoor LabDoor;
    public static AccessDoor GrottoDoor;
    public static AccessDoor OvergrowthDoor;

    [HarmonyPatch(typeof(AccessDoor), "Awake"), HarmonyPostfix]
    public static void Init(AccessDoor __instance)
    {
        var id = __instance.lockedRegionId;
        __instance.gameObject.SetActive(false);
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
        }
    }
}
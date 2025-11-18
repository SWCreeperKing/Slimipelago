using HarmonyLib;

namespace Slimipelago.Patches.Expansions;

[PatchAll]
public static class LabAccessPatch
{
    public static LabAccessDoor Door;

    [HarmonyPatch(typeof(LabAccessDoor), "Awake")]
    public static void Init(LabAccessDoor __instance)
    {
        Door = __instance;
        MainMenuPatch.OnGamePotentialExit += () => Door = null;
    }
}
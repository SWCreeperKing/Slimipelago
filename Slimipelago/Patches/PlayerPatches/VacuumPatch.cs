using HarmonyLib;

namespace Slimipelago.Patches.PlayerPatches;

[PatchAll]
public static class VacuumPatch
{
    public static WeaponVacuum PlayerVacuum;
    
    [HarmonyPatch(typeof(WeaponVacuum), "Awake"), HarmonyPostfix]
    public static void Awake(WeaponVacuum __instance)
    {
        PlayerVacuum = __instance;
    }
}
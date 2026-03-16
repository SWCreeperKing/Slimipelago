using HarmonyLib;
using Slimipelago.Archipelago;

namespace Slimipelago.Patches.UiPatches;

[PatchAll]
public static class SaveErrorCheck
{
    [HarmonyPatch(typeof(SaveErrorUI), "SetException"), HarmonyPrefix]
    public static void FindFullError(Exception e, string path)
    {
        Core.Log.Error("FAILED TO SAVE", e);
    }
    
    [HarmonyPatch(typeof(AutoSaveDirector), "SaveGame"), HarmonyPrefix]
    public static void SaveStopTrapsError()
    {
        TrapLoader.EMERGENCY_END_TRAP();
    }
}
using HarmonyLib;

namespace Slimipelago.Patches;

[HarmonyPatchAll]
public static class TranslationPatch
{
    [HarmonyPatch(typeof(MessageBundle), "Xlate"), HarmonyPrefix]
    public static bool AddApNames(string compoundKey, ref string __result)
    {
        if (!compoundKey.StartsWith("archi_")) return true;
        __result = compoundKey.Substring(6);
        return false;
    }
}
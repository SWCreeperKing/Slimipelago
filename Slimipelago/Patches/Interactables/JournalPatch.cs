using HarmonyLib;

namespace Slimipelago.Patches.Interactables;

[PatchAll]
public static class JournalPatch
{
    [HarmonyPatch(typeof(JournalEntry), "Start"), HarmonyPrefix]
    public static bool Start(JournalEntry __instance)
    {
        __instance.InteractableInstanced("log");
        return false;
    }

    [HarmonyPatch(typeof(JournalEntry), "Activate"), HarmonyPostfix]
    public static void ActivateJournal(JournalEntry __instance)
    {
        __instance.InteractableInteracted("Entry");
    }
}
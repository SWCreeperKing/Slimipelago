using HarmonyLib;
using UnityEngine;

namespace Slimipelago.Patches.Interactables;

[PatchAll]
public static class JournalPatch
{
    public static GameObject NotePrefab = null;
    
    [HarmonyPatch(typeof(JournalEntry), "Start"), HarmonyPrefix]
    public static bool Start(JournalEntry __instance)
    {
        if (NotePrefab is null) NotePrefab = __instance.uiPrefab;
        
        __instance.InteractableInstanced("log");
        return false;
    }

    [HarmonyPatch(typeof(JournalEntry), "Activate"), HarmonyPostfix]
    public static void ActivateJournal(JournalEntry __instance)
    {
        __instance.InteractableInteracted("Entry");
    }
}
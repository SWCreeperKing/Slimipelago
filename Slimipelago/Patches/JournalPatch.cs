using HarmonyLib;

namespace Slimipelago.Patches;

[PatchAll]
public static class JournalPatch
{
    public static bool EnableJournals = true;
    public static HashSet<string> Entries = [];
    
    [HarmonyPatch(typeof(JournalEntry), "Start"), HarmonyPrefix]
    public static bool Start(JournalEntry __instance)
    {
        __instance.gameObject.SetActive(EnableJournals);
        return false;
    }

    [HarmonyPatch(typeof(JournalEntry), "Activate"), HarmonyPrefix]
    public static bool ActivateJournal(JournalEntry __instance)
    {
        if (!Entries.Add(__instance.entryKey)) return false;
        Core.Log.Msg($"new log this session: [{__instance.entryKey}]");
        return false;
    }

    [HarmonyPatch(typeof(MapDataEntry), "Activate"), HarmonyPrefix]
    public static bool ActivateMapDataEntry(MapDataEntry __instance)
    {
        Core.Log.Msg($"Map activated: [{__instance.zone}]");
        return false;
    }
}
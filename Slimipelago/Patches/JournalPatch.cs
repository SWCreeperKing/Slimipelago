using HarmonyLib;

namespace Slimipelago.Patches;

[HarmonyPatchAll]
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
    public static void Activate(JournalEntry __instance)
    {
        if (!Entries.Add(__instance.entryKey)) return;
        Core.Log.Msg($"new log this session: [{__instance.entryKey}]");
    }
}
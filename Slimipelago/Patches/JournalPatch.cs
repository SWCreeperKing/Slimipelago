using HarmonyLib;
using MonomiPark.SlimeRancher.Regions;

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
        var region = __instance.GetComponentInParent<Region>();
        GameLoader.MakeMarker("log", __instance.transform.position, null, region.setId);
        return false;
    }

    [HarmonyPatch(typeof(JournalEntry), "Activate"), HarmonyPrefix]
    public static bool ActivateJournal(JournalEntry __instance)
    {
        var hash = __instance.transform.position.HashPos();
        if (!ApSlimeClient.LocationsFound.Add(hash)) return false;
        // if (!Entries.Add(__instance.entryKey)) return false;
        // Core.Log.Msg($"new log this session: [{__instance.entryKey}]");
        var color = GameLoader.MarkerDictionary[hash].Image.color;
        color.a /= 4;
        GameLoader.MarkerDictionary[hash].Image.color = color;
        
        PopupPatch.AddItemToQueue(new ApPopupData(GameLoader.Spritemap["normal"], "Log Found", ApSlimeClient.LocationDictionary[hash]));
        // return false;
        return true;
    }

    [HarmonyPatch(typeof(MapDataEntry), "Activate"), HarmonyPrefix]
    public static void ActivateMapDataEntry(MapDataEntry __instance)
    {
        Core.Log.Msg($"Map activated: [{__instance.zone}]");
        var hash = __instance.transform.position.HashPos();

        var color = GameLoader.MarkerDictionary[hash].Image.color;
        color.a /= 4;
        GameLoader.MarkerDictionary[hash].Image.color = color;
    }
    
    [HarmonyPatch(typeof(MapDataEntry), "Start"), HarmonyPrefix]
    public static bool MapMarker(MapDataEntry __instance)
    {
        var region = __instance.GetComponentInParent<Region>();
        GameLoader.MakeMarker("map", __instance.transform.position, null, region.setId);
        return false;
    }
}
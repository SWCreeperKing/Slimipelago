using HarmonyLib;
using MonomiPark.SlimeRancher.Regions;
using Slimipelago.Patches.UiPatches;

namespace Slimipelago.Patches.Interactables;

[PatchAll]
public static class JournalPatch
{
    [HarmonyPatch(typeof(JournalEntry), "Start"), HarmonyPrefix]
    public static bool Start(JournalEntry __instance)
    {
        __instance.gameObject.SetActive(true);
        var hash = __instance.transform.position.HashPos();
        if (ApSlimeClient.LocationsFound.Contains(hash)) return false;
        var region = __instance.GetComponentInParent<Region>();
        GameLoader.MakeMarker("log", __instance.transform.position, null, region.setId);
        return false;
    }

    [HarmonyPatch(typeof(JournalEntry), "Activate"), HarmonyPrefix]
    public static bool ActivateJournal(JournalEntry __instance)
    {
        var hash = __instance.transform.position.HashPos();
        if (!ApSlimeClient.LocationsFound.Add(hash)) return false;
        GameLoader.ChangeMarkerColor(hash, color =>
        {
            color.a = 0;
            return color;
        });

        PopupPatch.AddItemToQueue(new ApPopupData(GameLoader.Spritemap["normal"], "Log Found",
            ApSlimeClient.LocationDictionary[hash]));
        return true;
    }
}
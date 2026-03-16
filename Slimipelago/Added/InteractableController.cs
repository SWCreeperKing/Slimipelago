using MonomiPark.SlimeRancher.Regions;
using Slimipelago.Archipelago;
using Slimipelago.Patches.PlayerPatches;
using UnityEngine;

namespace Slimipelago.Patches.Interactables;

public static class InteractableController
{
    public static void InteractableInstanced<T>(this T __instance, string markerName) where T : MonoBehaviour
    {
        try
        {
            var hash = __instance.transform.position.HashPos();
            Region region;
            if (!ApSlimeClient.LocationDictionary.TryGetValue(hash, out var itemName))
            {
                if (Core.DebugLevel <= 0) return;
                Core.Log.Msg($"Location Hash not found: [{hash}] for [{__instance.gameObject.name}]");
                region = __instance.GetComponentInParent<Region>();
                GameLoader.MakeMarker(
                    markerName, __instance.transform.position,
                    () => PlayerStatePatch.TeleportPlayer(__instance.transform.position, region.setId),
                    region.setId
                );

                // __instance.gameObject.SetActive(false);
                return;
            }

            var found = !ApSlimeClient.Client.MissingLocations.Contains(itemName);
            __instance.gameObject.SetActive(true);

            if (markerName is "log") { found = found && ApSlimeClient.NoteLocations.HasFlag(hash); }

            if (found) return;

            region = __instance.GetComponentInParent<Region>();
            GameLoader.MakeMarker(markerName, __instance.transform.position, null, region.setId);
            ApSlimeClient.QueueReLogic = true;
        }
        catch (Exception e) { Core.Log.Error(e); }
    }

    public static void InteractableInteracted<T>(this T __instance, string itemFound) where T : MonoBehaviour
    {
        var hash = __instance.transform.position.HashPos();

        if (itemFound is "Entry")
        {
            ApSlimeClient.NoteLocations += hash;
            ApSlimeClient.Client.SendToStorage("note_locations", (ulong)ApSlimeClient.NoteLocations);

            if (Core.DebugLevel > 1)
            {
                var flag = Convert.ToString((long)ApSlimeClient.NoteLocations.GetFlag(hash), 2);
                var currentFlag = Convert.ToString((long)(ulong)ApSlimeClient.NoteLocations, 2);
                var maxFlag = Convert.ToString((long)ApSlimeClient.NoteLocations.MaxFlag, 2);
                Core.Log.Msg(
                    $"Note location checked [{hash}], flag: [{flag}], total: [{currentFlag}], max: [{maxFlag}]"
                );
            }
        }

        if (!ApSlimeClient.LocationDictionary.TryGetValue(hash, out var itemName))
        {
            if (Core.DebugLevel > 0)
            {
                Core.Log.Msg($"Location Hash not found: [{itemFound}]:[{hash}] for [{__instance.gameObject.name}]");
            }

            return;
        }

        if (!ApSlimeClient.Client.MissingLocations.Contains(itemName)) return;
        ApSlimeClient.SendItem($"{itemFound} Found", itemName);
        // __instance.gameObject.SetActive(false);
    }
}
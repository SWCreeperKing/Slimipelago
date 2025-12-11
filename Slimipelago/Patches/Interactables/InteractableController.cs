using MonomiPark.SlimeRancher.Regions;
using Slimipelago.Archipelago;
using UnityEngine;

namespace Slimipelago.Patches.Interactables;

public static class InteractableController
{
    extension<T>(T __instance) where T : MonoBehaviour
    {
        public void InteractableInstanced(string markerName)
        {
            var hash = __instance.transform.position.HashPos();
            if (!ApSlimeClient.LocationDictionary.TryGetValue(hash, out var itemName))
            {
                // Core.Log.Msg($"Location Hash not found: [{hash}] for [{__instance.gameObject.name}]");
                // __instance.gameObject.SetActive(false);
                // return;
            }

            var found = !ApSlimeClient.Client.MissingLocations.Contains(itemName);
            __instance.gameObject.SetActive(true);
            if (found) return;

            var region = __instance.GetComponentInParent<Region>();
            GameLoader.MakeMarker(markerName, __instance.transform.position, null, region.setId);
            ApSlimeClient.QueueReLogic = true;
        }

        public void InteractableInteracted(string itemFound)
        {
            var hash = __instance.transform.position.HashPos();
            ApSlimeClient.SendItem($"{itemFound} Found", ApSlimeClient.LocationDictionary[hash]);
            if (!ApSlimeClient.Client.MissingLocations.Contains(ApSlimeClient.LocationDictionary[hash])) return;

            // __instance.gameObject.SetActive(false);
        }
    }
}
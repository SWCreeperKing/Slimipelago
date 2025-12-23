using MonomiPark.SlimeRancher.Regions;
using Slimipelago.Archipelago;
using Slimipelago.Patches.PlayerPatches;
using UnityEngine;

namespace Slimipelago.Patches.Interactables;

public static class InteractableController
{
    extension<T>(T __instance) where T : MonoBehaviour
    {
        public void InteractableInstanced(string markerName)
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
                    GameLoader.MakeMarker(markerName, __instance.transform.position,
                        () => PlayerStatePatch.TeleportPlayer(__instance.transform.position, region.setId),
                        region.setId);

                    // __instance.gameObject.SetActive(false);
                    return;
                }

                var found = !ApSlimeClient.Client.MissingLocations.Contains(itemName);
                __instance.gameObject.SetActive(true);
                if (found) return;

                region = __instance.GetComponentInParent<Region>();
                GameLoader.MakeMarker(markerName, __instance.transform.position, null, region.setId);
                ApSlimeClient.QueueReLogic = true;
            }
            catch (Exception e)
            {
                Core.Log.Error(e);
            }
        }

        public void InteractableInteracted(string itemFound)
        {
            var hash = __instance.transform.position.HashPos();
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
}
using MonomiPark.SlimeRancher.Regions;
using Slimipelago.Patches.UiPatches;
using UnityEngine;

namespace Slimipelago.Patches.Interactables;

public static class InteractableController
{
    extension<T>(T __instance) where T: MonoBehaviour
    {
        public void InteractableInstanced(string markerName)
        {
            var hash = __instance.transform.position.HashPos();
            var found = ApSlimeClient.LocationsFound.Contains(hash);
            __instance.gameObject.SetActive(!found);
            if (found) return;
        
            var region = __instance.GetComponentInParent<Region>();
            GameLoader.MakeMarker(markerName, __instance.transform.position, null, region.setId);
        }

        public void InteractableInteracted(string itemFound)
        {
            var hash = __instance.transform.position.HashPos();
            if (!ApSlimeClient.LocationsFound.Add(hash)) return;
            GameLoader.ChangeMarkerColor(hash, color =>
            {
                color.a = 0;
                return color;
            });
        
            __instance.gameObject.SetActive(false);

            PopupPatch.AddItemToQueue(new ApPopupData(GameLoader.Spritemap["normal"], $"{itemFound} Found",
                ApSlimeClient.LocationDictionary[hash]));
        }
    }
}
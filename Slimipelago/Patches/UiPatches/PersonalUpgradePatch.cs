using Archipelago.MultiClient.Net.Models;
using HarmonyLib;
using MonomiPark.SlimeRancher.DataModel;
using Slimipelago.Archipelago;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Slimipelago.Archipelago.ApSlimeClient;

namespace Slimipelago.Patches.UiPatches;

[PatchAll]
public static class PersonalUpgradePatch
{
    public static Dictionary<string, string> PurchaseNameKeyToLocation = [];

    [HarmonyPatch(typeof(PlayerModel), "ApplyUpgrade"), HarmonyPrefix]
    public static bool ApplyUpgrade(PlayerState.Upgrade upgrade)
    {
        // Core.Log.Msg($"upgrade: [{upgrade}]");
        if (!UpgradeLocations.TryGetValue(upgrade, out var location)) return true;
        if (!Client.MissingLocations.Contains(location)) return false;
        SendItem("Upgrade Bought", location);
        return false;
    }

    [HarmonyPatch(typeof(PurchaseUI), "Select"), HarmonyPostfix]
    public static void UpgradeDescription(PurchaseUI __instance, PurchaseUI.Purchasable purchasable)
    {
        if (!PurchaseNameKeyToLocation.TryGetValue(purchasable.nameKey, out var locationName)) return;
        if (!ScoutedLocations.TryGetValue(locationName, out var itemInfo))
        {
            var loc = Client.ScoutLocation(locationName);
            if (loc is null) return;
            itemInfo = ScoutedLocations[locationName] = loc;
        }

        __instance.selectedTitle.text = locationName;
        __instance.selectedDesc.text = $"{itemInfo.ItemName}\nfor [{itemInfo.Player.Name}]";
        __instance.selectedImg.sprite = ItemHandler.ItemImage(itemInfo);
    }

    [HarmonyPatch(typeof(PersonalUpgradeUI), "CreateUpgradePurchasable"), HarmonyPostfix]
    public static void CreateUpgradePurchasable(PlayerState.Upgrade upgrade, ref PurchaseUI.Purchasable __result)
    {
        if (!UpgradeLocations.TryGetValue(upgrade, out var location)) return;
        PurchaseNameKeyToLocation[__result.nameKey] = location;
    }

    [HarmonyPatch(typeof(PurchaseUI), "UpdateButton"), HarmonyPostfix]
    public static void UpdateButton(PurchaseUI.Purchasable purchasable, GameObject buttonObj)
    {
        try
        {
            if (!PurchaseNameKeyToLocation.TryGetValue(purchasable.nameKey, out var locationName)) return;
            if (!ScoutedLocations.TryGetValue(locationName, out var itemInfo))
            {
                var loc = Client.ScoutLocation(locationName);
                if (loc is null) return;
                itemInfo = ScoutedLocations[locationName] = loc;
            }

            var component2 = buttonObj.transform.Find("Content/Name").gameObject.GetComponent<TMP_Text>();
            var component3 = buttonObj.transform.Find("Content/Icon").gameObject.GetComponent<Image>();
            var component4 = buttonObj.transform.Find("Content/Count").gameObject.GetComponent<TMP_Text>();

            component2.text = itemInfo.ItemName;
            component3.overrideSprite = ItemHandler.ItemImage(itemInfo);
            component4.enabled = false;
        }
        catch (KeyNotFoundException)
        {
            // Core.Log.Error($"Upgrade key not found: [{PurchaseNameKeyToLocation[purchasable.nameKey]}]");
        }
        catch (Exception e)
        {
            Core.Log.Error(e);
        }
    }
}
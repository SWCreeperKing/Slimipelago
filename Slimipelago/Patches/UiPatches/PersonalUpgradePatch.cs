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
        try
        {
            if (!UpgradeLocations.TryGetValue(upgrade, out var location)) return true;
            if (!Client.MissingLocations.Contains(location)) return false;
            SendItem("Upgrade Bought", location);
        }
        catch (Exception e)
        {
            Core.Log.Error(e);
        }
        return false;
    }

    [HarmonyPatch(typeof(PurchaseUI), "Select"), HarmonyPostfix]
    public static void UpgradeDescription(PurchaseUI __instance, PurchaseUI.Purchasable purchasable)
    {
        try
        {
            if (!PurchaseNameKeyToLocation.TryGetValue(purchasable.nameKey, out var locationName)) return;

            var scout = ItemHandler.ScoutLocation(locationName);
            if (scout is null) return;

            __instance.selectedTitle.text = locationName;
            __instance.selectedDesc.text = $"{scout.ItemName}\nfor [{scout.Player.Name}]";
            __instance.selectedImg.sprite = ItemHandler.ItemImage(scout);
        }
        catch (Exception e)
        {
            Core.Log.Error(e);
        }
    }

    [HarmonyPatch(typeof(PersonalUpgradeUI), "CreateUpgradePurchasable"), HarmonyPostfix]
    public static void CreateUpgradePurchasable(PlayerState.Upgrade upgrade, ref PurchaseUI.Purchasable __result)
    {
        if (!UpgradeLocations.TryGetValue(upgrade, out var location)) return;
        PurchaseNameKeyToLocation[__result.nameKey] = location;
        
        try
        {
            var scout = ItemHandler.ScoutLocation(location);
            if (scout is null) return;

            if (scout.ItemName.EndsWith(ItemConstants.NewBucksEnding)) __result.cost = 0;
        }
        catch (Exception)
        {
            if (location.Contains("Buy Personal Upgrade (Treasure Cracker lv.")) return;
            Core.Log.Warning($"something something location not found Key: [{location}], [{upgrade}] (ignore plz)");
        }
    }
    
    [HarmonyPatch(typeof(PersonalUpgradeUI), "Upgrade"), HarmonyPrefix]
    public static void RefundMoney(PlayerState.Upgrade upgrade, ref int cost)
    {
        try
        {
            if (!UpgradeLocations.TryGetValue(upgrade, out var location)) return;
            if (!Client.IsConnected) return;

            var scout = ItemHandler.ScoutLocation(location);
            if (scout is null) return;
            if (scout.ItemName.EndsWith(ItemConstants.NewBucksEnding)) cost = 0;
        }
        catch (Exception e)
        {
            if (upgrade is PlayerState.Upgrade.TREASURE_CRACKER_1 or PlayerState.Upgrade.TREASURE_CRACKER_2
                or PlayerState.Upgrade.TREASURE_CRACKER_3) return;
            Core.Log.Error($"something went wrong with Key: [{upgrade}]", e);
        }
    }

    [HarmonyPatch(typeof(PurchaseUI), "UpdateButton"), HarmonyPostfix]
    public static void UpdateButton(PurchaseUI.Purchasable purchasable, GameObject buttonObj)
    {
        try
        {
            if (!PurchaseNameKeyToLocation.TryGetValue(purchasable.nameKey, out var locationName)) return;
            
            var scout = ItemHandler.ScoutLocation(locationName);
            if (scout is null) return;

            var component2 = buttonObj.transform.Find("Content/Name").gameObject.GetComponent<TMP_Text>();
            var component3 = buttonObj.transform.Find("Content/Icon").gameObject.GetComponent<Image>();
            var component4 = buttonObj.transform.Find("Content/Count").gameObject.GetComponent<TMP_Text>();

            component2.text = scout.ItemName;
            component3.overrideSprite = ItemHandler.ItemImage(scout);
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
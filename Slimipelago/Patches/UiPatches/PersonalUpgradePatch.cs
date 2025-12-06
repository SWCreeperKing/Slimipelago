using Archipelago.MultiClient.Net.Models;
using HarmonyLib;
using MonomiPark.SlimeRancher.DataModel;
using Slimipelago.Archipelago;

namespace Slimipelago.Patches.UiPatches;

[PatchAll]
public static class PersonalUpgradePatch
{
    public static Dictionary<string, string> PurchaseNameKeyToLocation = [];
    public static Dictionary<string, ScoutedItemInfo> ScoutedUpgrades = [];

    [HarmonyPatch(typeof(PlayerModel), "ApplyUpgrade"), HarmonyPrefix]
    public static bool ApplyUpgrade(PlayerState.Upgrade upgrade)
    {
        // Core.Log.Msg($"upgrade: [{upgrade}]");
        var location = ApSlimeClient.UpgradeLocations[upgrade];
        if (!ApSlimeClient.Client.MissingLocations.Contains(location)) return false;
        ApSlimeClient.SendItem("Upgrade Bought", location);
        return false;
    }

    [HarmonyPatch(typeof(PurchaseUI), "Select"), HarmonyPostfix]
    public static void UpgradeDescription(PurchaseUI __instance, PurchaseUI.Purchasable purchasable)
    {
        if (!PurchaseNameKeyToLocation.TryGetValue(purchasable.nameKey, out var locationName)) return;
        if (!ScoutedUpgrades.TryGetValue(purchasable.nameKey, out var itemInfo))
        {
            var loc = ApSlimeClient.Client.ScoutLocation(locationName);
            if (loc is null) return;
            itemInfo = ScoutedUpgrades[purchasable.nameKey] = loc;
        }

        var item = new AssetItem(itemInfo.ItemGame, itemInfo.ItemName, itemInfo.Flags);
        __instance.selectedTitle.text = itemInfo.ItemName;
        __instance.selectedDesc.text = $"for [{itemInfo.Player.Name}]";
        __instance.selectedImg.sprite = ItemHandler.ItemImage(item);
    }

    [HarmonyPatch(typeof(PersonalUpgradeUI), "CreateUpgradePurchasable"), HarmonyPostfix]
    public static void CreateUpgradePurchasable(PlayerState.Upgrade upgrade, ref PurchaseUI.Purchasable __result)
    {
        if (!ApSlimeClient.UpgradeLocations.TryGetValue(upgrade, out var location)) return;
        PurchaseNameKeyToLocation[__result.nameKey] = location;
    }
}
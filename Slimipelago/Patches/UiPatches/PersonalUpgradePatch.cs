using HarmonyLib;
using MonomiPark.SlimeRancher.DataModel;
using Slimipelago.Archipelago;
using Slimipzelago.Archipelago;

namespace Slimipelago.Patches.UiPatches;

[PatchAll]
public static class PersonalUpgradePatch
{
    [HarmonyPatch(typeof(PlayerModel), "ApplyUpgrade"), HarmonyPrefix]
    public static bool ApplyUpgrade(PlayerState.Upgrade upgrade)
    {
        // Core.Log.Msg($"upgrade: [{upgrade}]");
        var location = ApWorldShenanigans.UpgradeLocations[upgrade];
        if (!ApSlimeClient.Client.MissingLocations.Contains(location)) return false;
        ApSlimeClient.SendItem("Upgrade Bought", location);
        return false;
    }
    
    // public static List<PurchaseUI.Purchasable> TestPurchasables = [];

     // [HarmonyPatch(typeof(PersonalUpgradeUI), "CreatePurchaseUI"), HarmonyPrefix]
//     public static bool OverridePurchasables(PersonalUpgradeUI __instance, ref GameObject __result)
//     {
//         __result = SRSingleton<GameContext>.Instance.UITemplates.CreatePurchaseUI(__instance.titleIcon,
//             MessageUtil.Qualify("ui", "t.personal_upgrades"), TestPurchasables.ToArray(), false, __instance.Close);
//         return false;
//     }
//
//     public static PurchaseUI.Purchasable CreatePurchasable(string name, Sprite icon, string description, int cost,
//         UnityAction onPurchase, Func<bool> unlocked, Func<bool> available)
//         => new($"archi_{name}", icon, icon, $"archi_{description}", cost, null, onPurchase, unlocked, available);
}
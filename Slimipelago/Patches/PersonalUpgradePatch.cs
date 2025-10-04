using HarmonyLib;
using UnityEngine;
using UnityEngine.Events;

namespace Slimipelago.Patches;

[HarmonyPatchAll]
public static class PersonalUpgradePatch
{
    public static List<PurchaseUI.Purchasable> TestPurchasables = [];

    [HarmonyPatch(typeof(PersonalUpgradeUI), "CreatePurchaseUI"), HarmonyPrefix]
    public static bool OverridePurchasables(PersonalUpgradeUI __instance, ref GameObject __result)
    {
        __result = SRSingleton<GameContext>.Instance.UITemplates.CreatePurchaseUI(__instance.titleIcon,
            MessageUtil.Qualify("ui", "t.personal_upgrades"), TestPurchasables.ToArray(), false, __instance.Close);
        return false;
    }

    public static PurchaseUI.Purchasable CreatePurchasable(string name, Sprite icon, string description, int cost,
        UnityAction onPurchase, Func<bool> unlocked, Func<bool> available)
        => new($"archi_{name}", icon, icon, $"archi_{description}", cost, null, onPurchase, unlocked, available);
}
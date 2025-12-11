using HarmonyLib;
using Slimipelago.Patches.PlayerPatches;

namespace Slimipelago.Patches.UiPatches;

[PatchAll]
public static class CorporatePatch
{
    [HarmonyPatch(typeof(CorporatePartnerUI), "BuyLevel"), HarmonyPrefix]
    public static void BuyLevel(ProgressDirector progressDir, int level, int cost)
    {
        var progress = progressDir.GetProgress(ProgressDirector.ProgressType.CORPORATE_PARTNER);
        if (progress >= level || progress < level - 1 || PlayerStatePatch.PlayerState.GetCurrency() < cost) return;
        Core.Log.Msg($"Bought 7Zee Level: [{level}] for [{cost:###,###}] Newbucks");
    }
}
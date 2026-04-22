using System.Text.RegularExpressions;
using HarmonyLib;
using Slimipelago.Archipelago;
using Slimipelago.Patches.PlayerPatches;
using static Slimipelago.Archipelago.ApSlimeClient;
using GoalType = Slimipelago.Archipelago.GoalType;

namespace Slimipelago.Patches.UiPatches;

[PatchAll]
public static class CorporatePatch
{
    [HarmonyPatch(typeof(CorporatePartnerUI), "BuyLevel"), HarmonyPrefix]
    public static void BuyLevel(ProgressDirector progressDir, int level, int cost)
    {
        try
        {
            var progress = progressDir.GetProgress(ProgressDirector.ProgressType.CORPORATE_PARTNER);
            if (progress >= level || progress < level - 1 || PlayerStatePatch.PlayerState.GetCurrency() < cost) return;

            SendItems(
                "Ranked Up!", Client.MissingLocations.Where(loc =>
                    {
                        try
                        {
                            if (!loc.Contains("7Zee")) return false;
                            var dot = loc.IndexOf('.');
                            return int.Parse(loc.Substring(dot + 1, loc.IndexOf(':') - dot - 1)) <= level;
                        }
                        catch (Exception e) { Core.Log.Error(e); }
                        return false;
                    }
                ).ToArray()
            );

            if (level < 28) return;
            Client.TryGoal(GoalType.Corporate);
        }
        catch (Exception e) { Core.Log.Error(e); }
    }

    [HarmonyPatch(typeof(CorporatePartnerUI), "EnableReward"), HarmonyPostfix]
    public static void EnableReward(CorporatePartnerUI __instance, int rank, int rewardIndex)
    {
        try
        {
            if (!CorporateLocations.TryGetValue(rank, out var locations)) return;
            var location = locations[rewardIndex];
            if (!Client.MissingLocations.Contains(location)) return;

            AssetItem item = ScoutedLocations[location];
            __instance.rewardTitles[rewardIndex].text = item.ItemName;
            __instance.rewardIcons[rewardIndex].overrideSprite = ItemHandler.ItemImage(item);
        }
        catch (Exception e) { Core.Log.Error(e); }
    }
}
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
    public static readonly Regex LevelRegex = new(@"7Zee lv.(\d+):", RegexOptions.Compiled);

    [HarmonyPatch(typeof(CorporatePartnerUI), "BuyLevel"), HarmonyPrefix]
    public static void BuyLevel(ProgressDirector progressDir, int level, int cost)
    {
        var progress = progressDir.GetProgress(ProgressDirector.ProgressType.CORPORATE_PARTNER);
        if (progress >= level || progress < level - 1 || PlayerStatePatch.PlayerState.GetCurrency() < cost) return;

        SendItems(
            "Ranked Up!", Client.MissingLocations.Where(loc =>
                {
                    if (!LevelRegex.IsMatch(loc)) return false;
                    return int.Parse(LevelRegex.Match(loc).Groups[1].Value) <= level;
                }
            ).ToArray()
        );

        if (level < 28) return;
        Client.TryGoal(GoalType.Corporate);
    }

    [HarmonyPatch(typeof(CorporatePartnerUI), "EnableReward"), HarmonyPostfix]
    public static void EnableReward(CorporatePartnerUI __instance, int rank, int rewardIndex)
    {
        if (!CorporateLocations.TryGetValue(rank, out var locations)) return;
        var location = locations[rewardIndex];
        if (!Client.MissingLocations.Contains(location)) return;


        AssetItem item = ScoutedLocations.TryGetValue(location, out var scoutedItem) ? scoutedItem
            : ScoutedLocations[location] = Client.ScoutLocation(location);
        
        __instance.rewardTitles[rewardIndex].text = item.ItemName;
        __instance.rewardIcons[rewardIndex].overrideSprite = ItemHandler.ItemImage(item);
    }
}
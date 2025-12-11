using HarmonyLib;
using MonomiPark.SlimeRancher.DataModel;
using Slimipelago.Archipelago;

namespace Slimipelago.Patches.Interactables;

[PatchAll]
public static class MarketPatch
{
    [HarmonyPatch(typeof(EconomyDirector), "GetTargetValue"), HarmonyPrefix]
    public static bool GetTargetValue(WorldModel worldModel, Identifiable.Id id, float baseValue, float fullSaturation,
        float day, ref float __result)
    {
        if (ApSlimeClient.HackTheMarket)
        {
            __result = baseValue * 1.5f;
            return false;
        }
        
        return true;
    }


    [HarmonyPatch(typeof(EconomyDirector), "GetTargetValue"), HarmonyPostfix]
    public static void AfterGetTargetValue(WorldModel worldModel, Identifiable.Id id, float baseValue, float fullSaturation,
        float day, ref float __result)
    {
        if (!ApSlimeClient.ItemCache.TryGetValue("Progressive Market Stonks", out var value)) return;
        __result *= 5 * value / 100f + 1;
    }
}
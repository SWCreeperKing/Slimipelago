using HarmonyLib;
using MonomiPark.SlimeRancher.DataModel;
using Slimipelago.Archipelago;
using UnityEngine;
using static Slimipelago.Archipelago.ItemConstants;

namespace Slimipelago.Patches.Interactables;

[PatchAll]
public static class MarketPatch
{
    public static bool Crash = false;

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
    public static void AfterGetTargetValue(WorldModel worldModel, Identifiable.Id id, float baseValue,
        float fullSaturation,
        float day, ref float __result)
    {
        if (Crash)
        {
            __result = 1;
            return;
        }

        if (!ApSlimeClient.ItemCache.TryGetValue(ProgMarket, out var value)) return;
        __result *= 20 * value / 100f + 1;
    }

    [HarmonyPatch(typeof(EconomyDirector), "ResetPrices"), HarmonyPostfix]
    public static void AfterCrash() { Crash = false; }

    [HarmonyPatch(typeof(MarketUI), "Start"), HarmonyPostfix]
    public static void MarketManip(MarketUI __instance, Dictionary<MarketUI.PlortEntry, GameObject> ___amountMap)
    {
        foreach (var entries in ___amountMap)
        {
            var plort = entries.Key;
            if (!LogicHandler.PlortLocations.TryGetValue(plort.id, out var loc)) continue;
            if (!ApSlimeClient.Client.MissingLocations.Contains(loc)) continue;

            var priceEntry = entries.Value.GetComponent<PriceEntry>();
            var marketItem = priceEntry.gameObject.AddComponent<MarketItem>();

            marketItem.Entry = priceEntry;
            marketItem.Location = loc;
            marketItem.Image = ItemHandler.ItemImage(ItemHandler.ScoutLocation(loc));
        }
    }

    [HarmonyPatch(typeof(MarketUI), "PlortCountUpdate", typeof(Identifiable.Id)), HarmonyPostfix]
    public static void PlortSold(Identifiable.Id id)
    {
        if (!LogicHandler.PlortLocations.TryGetValue(id, out var loc)) return;
        if (!ApSlimeClient.Client.MissingLocations.Contains(loc)) return;
        ApSlimeClient.SendItem("Plort Sold", loc);
    }
}

public class MarketItem : MonoBehaviour
{
    public PriceEntry Entry;
    public bool Track;
    public float SwitchTimer;
    public Sprite Image;
    public string Location;
    private bool Ended;

    private void Update()
    {
        if (Ended) return;
        if (!ApSlimeClient.Client.MissingLocations.Contains(Location))
        {
            Ended = true;
            Entry.itemIcon.overrideSprite = null;
        }

        SwitchTimer += Time.deltaTime;
        if (SwitchTimer < 3) return;
        SwitchTimer = 0;

        Entry.itemIcon.overrideSprite = Track ? null : Image;
        Track = !Track;
    }
}
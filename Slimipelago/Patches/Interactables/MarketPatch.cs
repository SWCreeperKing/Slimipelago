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
    public static void AfterGetTargetValue(WorldModel worldModel, Identifiable.Id id, float baseValue, float fullSaturation,
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
    public static void AfterCrash()
    {
        Crash = false;
    }
    
    // [HarmonyPatch(typeof(MarketUI), "Start"), HarmonyPostfix]
    // public static void MarketManip(MarketUI __instance)
    // {
    //     var amountMap = __instance.GetPrivateField<Dictionary<MarketUI.PlortEntry, GameObject>>("amountMap");
    //     
    //     foreach (var entries in amountMap)
    //     {
    //         var priceEntry = entries.Value.GetComponent<PriceEntry>();
    //         var marketItem = priceEntry.gameObject.AddComponent<MarketItem>();
    //         marketItem.Plort = entries.Key;
    //         marketItem.Entry = priceEntry;
    //     }
    // }

    [HarmonyPatch(typeof(MarketUI), "PlortCountUpdate", typeof(Identifiable.Id)), HarmonyPostfix]
    public static void PlortSold(Identifiable.Id id)
    {
        Core.Log.Msg($"Plort Sold: [{id}]");
    }
}

// public class MarketItem : MonoBehaviour
// {
//     public MarketUI.PlortEntry Plort;
//     public PriceEntry Entry;
//     public bool Track;
//     public float SwitchTimer;
//     
//     private void Update()
//     {
//         if (!Core.MarketItems.ContainsKey(Plort.id))
//         {
//             if (Entry.itemIcon.overrideSprite is not null)
//             {
//                 Entry.itemIcon.overrideSprite = null;
//             }
//             
//             return;
//         }
//         
//         SwitchTimer += Time.deltaTime;
//         if (SwitchTimer < 3) return;
//         SwitchTimer = 0;
//
//         Entry.itemIcon.overrideSprite = Track ? null : Core.Spritemap[Core.MarketItems[Plort.id]];
//         Track = !Track;
//     }
// }
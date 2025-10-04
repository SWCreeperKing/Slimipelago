using System.Reflection;
using HarmonyLib;
using MelonLoader;
using UnityEngine;

namespace Slimipelago.Patches;

[HarmonyPatchAll]
public static class MarketUIPatch
{
    [HarmonyPatch(typeof(MarketUI), "Start"), HarmonyPostfix]
    public static void MarketManip(MarketUI __instance)
    {
        var amountMap = __instance.GetPrivateField<Dictionary<MarketUI.PlortEntry, GameObject>>("amountMap");
        
        foreach (var entries in amountMap)
        {
            var priceEntry = entries.Value.GetComponent<PriceEntry>();
            var marketItem = priceEntry.gameObject.AddComponent<MarketItem>();
            marketItem.Plort = entries.Key;
            marketItem.Entry = priceEntry;
        }
    }

    [HarmonyPatch(typeof(MarketUI), "PlortCountUpdate", typeof(Identifiable.Id)), HarmonyPostfix]
    public static void PlortSold(Identifiable.Id id)
    {
        Core.Log.Msg($"Plort Sold: [{id}]");
    }
}

[RegisterTypeInIl2Cpp]
public class MarketItem : MonoBehaviour
{
    public MarketUI.PlortEntry Plort;
    public PriceEntry Entry;
    public bool Track;
    public float SwitchTimer;
    
    private void Update()
    {
        if (!Core.MarketItems.ContainsKey(Plort.id))
        {
            if (Entry.itemIcon.overrideSprite is not null)
            {
                Entry.itemIcon.overrideSprite = null;
            }
            
            return;
        }
        
        SwitchTimer += Time.deltaTime;
        if (SwitchTimer < 3) return;
        SwitchTimer = 0;

        Entry.itemIcon.overrideSprite = Track ? null : Core.Spritemap[Core.MarketItems[Plort.id]];
        Track = !Track;
    }
}
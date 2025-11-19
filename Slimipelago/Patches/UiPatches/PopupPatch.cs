using HarmonyLib;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Object;

namespace Slimipelago.Patches.UiPatches;

[PatchAll]
public static class PopupPatch
{
    private static double DeadTime;
    private static Queue<ApPopupData> PopupDatas = [];
    [CanBeNull] private static GameObject CurrentPopup;

    [HarmonyPatch(typeof(PopupDirector), "QueueForPopup"), HarmonyPrefix]
    public static bool StopPopups() => false;
    
    private static GameObject CreateItemPopup(Sprite sprite, string itemDialogue, string item, string otherPlayer, Action onPopup)
    {
        var popup = Instantiate(SRSingleton<SceneContext>.Instance.GadgetDirector.gadgetPopupPrefab);
        Destroy(popup.GetComponent<BlueprintPopupUI>());
        var container = popup.GetChild(0);
        var panel = container.GetChild(0);
        panel.GetChild(0).GetComponent<Image>().overrideSprite = sprite;
        panel.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text = item;
        panel.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Archipelago";
        panel.GetChild(3).GetChild(0).GetComponent<TextMeshProUGUI>().text = itemDialogue;
        panel.GetChild(4).GetComponent<TextMeshProUGUI>().text = otherPlayer;
        onPopup?.Invoke();
        return popup;
    }

    public static void AddItemToQueue(ApPopupData itemData)
    {
        PopupDatas.Enqueue(itemData);
        Core.Log.Msg($"Popup Queue: [{PopupDatas.Count}]");
    }

    public static void UpdateQueue()
    {
        if (CurrentPopup is null)
        {
            if (!PopupDatas.Any()) return;
            var newData = PopupDatas.Dequeue();
            Core.Log.Msg($"Popup Queue: [{PopupDatas.Count}]");
            CurrentPopup = CreateItemPopup(newData.Sprite, newData.ItemDialogue, newData.Item, newData.OtherPlayer, newData.OnPopup);
            DeadTime = 0;
        }
        
        if (CurrentPopup is null) return;
        DeadTime += Time.deltaTime;
        
        if (DeadTime <= 3) return;
        Destroy(CurrentPopup);
        CurrentPopup = null;
        DeadTime = 0;
    }
}

public readonly struct ApPopupData(Sprite sprite, string itemDialogue, string item, string otherPlayer = "", Action onPopup = null)
{
    public readonly Sprite Sprite = sprite;
    public readonly string ItemDialogue = itemDialogue;
    public readonly string Item = item;
    public readonly string OtherPlayer = otherPlayer;
    public readonly Action OnPopup = onPopup;
}
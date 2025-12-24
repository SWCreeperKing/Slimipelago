using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Slimipelago.Patches.UiPatches;

[PatchAll]
public static class PopupPatch
{
    public static PopupDirector PopupDirector;
    public static Queue<PopupDirector.PopupCreator> PopupQueue;

    [HarmonyPatch(typeof(PopupDirector), "Awake"), HarmonyPostfix]
    public static void Awake(PopupDirector __instance)
    {
        PopupDirector = __instance;
        PopupQueue = PopupDirector.GetPrivateField<Queue<PopupDirector.PopupCreator>>("popupQueue");
    }

    [HarmonyPatch(typeof(PopupDirector), "QueueForPopup"), HarmonyPrefix]
    public static bool StopPopups(PopupDirector.PopupCreator creator)
    {
        try
        {
            var director = creator.GetPrivateField<PediaDirector>("pediaDir");
            director.Unlock(creator.GetPrivateField<PediaDirector.Id>("id"));
        }
        catch
        {
            // ignored
        }

        return false;
    }

    public static void AddItemToQueue(ApPopup creator)
    {
        try
        {
            PopupQueue.Enqueue(creator);
            PopupDirector.MaybePopupNext();
        }
        catch (Exception e)
        {
            Core.Log.Error(e);
        }
    }
}

public class ApPopup(
    Sprite sprite,
    string itemDialogue,
    string item,
    string otherPlayer = "",
    Action onPopup = null,
    float timer = 4) : PopupDirector.PopupCreator
{
    public readonly Sprite Sprite = sprite;
    public readonly string ItemDialogue = itemDialogue;
    public readonly string Item = item;
    public readonly string OtherPlayer = otherPlayer;
    public readonly Action OnPopup = onPopup;
    public readonly float Timer = timer;

    public override void Create()
    {
        var popup = Object.Instantiate(SRSingleton<SceneContext>.Instance.GadgetDirector.gadgetPopupPrefab);
        var popupUI = popup.GetComponent<BlueprintPopupUI>();
        popupUI.SetPrivateField("timeOfDeath", Time.time + Timer);
            
        var container = popup.GetChild(0);
        var panel = container.GetChild(0);
        panel.GetChild(0).GetComponent<Image>().overrideSprite = Sprite;
        panel.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text = Item;
        panel.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Archipelago";
        panel.GetChild(3).GetChild(0).GetComponent<TextMeshProUGUI>().text = ItemDialogue;
        panel.GetChild(4).GetComponent<TextMeshProUGUI>().text = OtherPlayer;
        OnPopup?.Invoke();
    }

    public override bool Equals(object other) { return other is ApPopup popup && popup.GetHashCode() == GetHashCode(); }

    public override int GetHashCode()
        => $"{Sprite.name},{ItemDialogue},{Item},{OtherPlayer},{OnPopup.GetHashCode()}".GetHashCode();

    public override bool ShouldClear() => false;
}
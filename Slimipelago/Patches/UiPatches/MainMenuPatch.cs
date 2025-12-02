using HarmonyLib;
using JetBrains.Annotations;
using Slimipzelago.Archipelago;
using TMPro;
using UnityEngine;

namespace Slimipelago.Patches.UiPatches;

[PatchAll]
public static class MainMenuPatch
{
    public static GameObject ContinueButton;
    public static GameObject LoadButton;
    public static GameObject NewGameButton;
    [CanBeNull] public static event Action OnGamePotentialExit; 

    [HarmonyPatch(typeof(MainMenuUI), "Start"), HarmonyPostfix]
    public static void MenuPatch(MainMenuUI __instance)
    {
        OnGamePotentialExit?.Invoke();
        OnGamePotentialExit = null;
        
        var container = __instance.GetChild(1);
        ContinueButton = container.GetChild(0);
        LoadButton = container.GetChild(1);
        NewGameButton = container.GetChild(2);

        ContinueButton.AddComponent<Invisinator>();
        NewGameButton.gameObject.SetActive(ApSlimeClient.Client.IsConnected);
        LoadButton.gameObject.SetActive(ApSlimeClient.Client.IsConnected);
    }

    [HarmonyPatch(typeof(NewGameUI), "Start"), HarmonyPostfix]
    public static void PlayPatch(NewGameUI __instance)
    {
        var container = __instance.GetChild(0);
        container.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Create New Archipelago Game";

        var infoPanel = container.GetChild(1);
        infoPanel.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"Slot: [{ApSlimeClient.SlotName}]";

        var inputField = infoPanel.GetChild(1).GetComponent<SRInputField>();
        inputField.readOnly = true;
        inputField.text = ApSlimeClient.GameUUID;

        var modeList = infoPanel.GetChild(7).GetChild(0);
        modeList.GetChild(2).SetActive(false);
    }

    [HarmonyTranspiler, HarmonyPatch(typeof(NewGameUI), "PlayNewGame")]
    public static IEnumerable<CodeInstruction> PlayNewGameTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            if (instruction.opcode.ToString() == "ldc.i4.s" && (sbyte)instruction.operand == 24)
            {
                instruction.operand = sbyte.MaxValue;
            }

            yield return instruction;
        }
    }

    [HarmonyPatch(typeof(LoadGameUI), "ScrollToTop"), HarmonyPostfix]
    public static void LoadMenuDisplay(LoadGameUI __instance)
    {
        var hasAny = false;
        foreach (var child in __instance.loadButtonPanel.gameObject.GetChildren())
        {
            var name = child.GetChild(2).GetComponent<TextMeshProUGUI>();
            if (name.text != ApSlimeClient.GameUUID)
            {
                child.SetActive(false);
                continue;
            }

            name.text = ApSlimeClient.SlotName;
            hasAny = true;
            child.GetComponent<SRToggle>().Select();
        }

        if (hasAny) return;
        __instance.summaryPanel.gameObject.SetActive(false);
    }


    [HarmonyPatch(typeof(GameSummaryPanel), "Init"), HarmonyPostfix]
    public static void PostSummarySetData(GameSummaryPanel __instance, GameData.Summary gameSummary)
    {
        if (gameSummary.displayName != ApSlimeClient.GameUUID) return;
        __instance.gameNameText.text = ApSlimeClient.SlotName;
    }
}
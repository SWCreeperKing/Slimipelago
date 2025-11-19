using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Slimipelago.Patches.UiPatches;

[PatchAll]
public static class OptionsPatch
{
    [HarmonyPatch(typeof(OptionsUI), "Awake"), HarmonyPostfix]
    public static void ApInfo(OptionsUI __instance)
    {
        if (SceneManager.GetActiveScene().name is not "MainMenu") return;
        __instance.modsTab.SetActive(true);
        __instance.videoTab.GetComponent<SRToggle>().isOn = false;
        __instance.modsTab.GetComponent<SRToggle>().isOn = true;
        __instance.modsTab.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Archipelago";
        __instance.modsTab.transform.SetSiblingIndex(0);
        __instance.SelectModsTab();
    }
    
    [HarmonyPatch(typeof(OptionsUI), "SetupMods"), HarmonyPrefix]
    public static bool SetupAp(OptionsUI __instance)
    {
        if (SceneManager.GetActiveScene().name is not "MainMenu") return true;
        var modPanel = __instance.modsPanel.GetChild(0);
        var panel = new GameObject("Ap menu");
        panel.transform.parent = modPanel.transform;
        
        var layout = panel.AddComponent<GridLayoutGroup>();
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.startAxis = GridLayoutGroup.Axis.Horizontal;
        layout.cellSize = new Vector2(700, 50);
        layout.spacing = new Vector2(20, 40);
        layout.constraintCount = 1;

        var g1 = Helper.CreateHorizontalGroup(panel).gameObject;
        Helper.CreateText("Address:Port", Color.black, g1);
        Helper.CreateInputField("Ap Address", g1);

        var g2 = Helper.CreateHorizontalGroup(panel).gameObject;
        Helper.CreateText("Password    ", Color.black, g2);
        Helper.CreateInputField("Ap Password", g2).contentType = InputField.ContentType.Password;

        var g3 = Helper.CreateHorizontalGroup(panel).gameObject;
        Helper.CreateText("Slot name    ", Color.black, g3);
        Helper.CreateInputField("Ap Slot", g3);

        Helper.CreateButton("    Connect", panel, () => Core.Log.Msg("TRY CONNECT"));
        return false;
    }
}
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
        var address = Helper.CreateInputField("Ap Address", g1);
        address.text = ApSlimeClient.AddressPort;

        var g2 = Helper.CreateHorizontalGroup(panel).gameObject;
        Helper.CreateText("Password    ", Color.black, g2);
        var password = Helper.CreateInputField("Ap Password", g2);
        password.contentType = InputField.ContentType.Password;
        password.text = ApSlimeClient.Password;

        var g3 = Helper.CreateHorizontalGroup(panel).gameObject;
        Helper.CreateText("Slot name    ", Color.black, g3);
        var slot = Helper.CreateInputField("Ap Slot", g3);
        slot.text = ApSlimeClient.SlotName;

        var connectButton = Helper.CreateButton(ApSlimeClient.Client.IsConnected ? "Disconnect" : "Connect", panel,
            null, out var buttonText);
        var errorText = Helper.CreateText("", Color.red, panel);
        connectButton.onClick.AddListener(() =>
        {
            Core.Log.Msg("Try Connect");
            try
            {
                if (!ApSlimeClient.Client.IsConnected)
                {
                    errorText.text = "";
                    var error = ApSlimeClient.TryConnect(address.text, password.text, slot.text);

                    if (error is null)
                    {
                        buttonText.text = "Disconnect";
                    }
                    else
                    {
                        errorText.text = string.Join("\n", error);
                    }
                }
                else
                {
                    ApSlimeClient.Client.TryDisconnect();
                    buttonText.text = "Connect";
                }
            }
            catch (Exception e)
            {
                Core.Log.Error(e);
            }
        });
        return false;
    }
}
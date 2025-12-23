using CreepyUtil.Archipelago;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Slimipelago.Helper;
using static Slimipelago.Archipelago.ApSlimeClient;

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
        panel.transform.SetParent(modPanel.transform);

        var layout = panel.AddComponent<GridLayoutGroup>();
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.startAxis = GridLayoutGroup.Axis.Horizontal;
        layout.cellSize = new Vector2(700, 50);
        layout.spacing = new Vector2(20, 40);
        layout.constraintCount = 1;

        var g1 = CreateHorizontalGroup(panel).gameObject;
        CreateText("Address:Port", Color.black, g1);
        var address = CreateInputField("Ap Address", g1);
        address.text = Data.AddressPort;

        var g2 = CreateHorizontalGroup(panel).gameObject;
        CreateText("Password    ", Color.black, g2);
        var password = CreateInputField("Ap Password", g2);
        password.contentType = InputField.ContentType.Password;
        password.text = Data.Password;

        var g3 = CreateHorizontalGroup(panel).gameObject;
        CreateText("Slot name    ", Color.black, g3);
        var slot = CreateInputField("Ap Slot", g3);
        slot.text = Data.SlotName;

        var g4 = CreateHorizontalGroup(panel).gameObject;
        CreateCheckbox(__instance.modTogglePrefab, g4, Data.DeathLink, "DeathLink\n ", b =>
            {
                Data.DeathLink = b;
                if (!Client.IsConnected) return;
                if (Client.Tags[ArchipelagoTag.DeathLink]) _ = Client.Tags - ArchipelagoTag.DeathLink;
                else _ = Client.Tags + ArchipelagoTag.DeathLink;
            });
        CreateCheckbox(__instance.modTogglePrefab, g4, Data.DeathLinkTrap, "Deathlink does Trap\ninstead of dying",
                b => Data.DeathLinkTrap = b);

        var g42 = CreateHorizontalGroup(panel).gameObject;
        CreateCheckbox(__instance.modTogglePrefab, g42, Data.TrapLink, "TrapLink\n ", b =>
            {
                Data.TrapLink = b;
                if (!Client.IsConnected) return;
                if (Client.Tags[ArchipelagoTag.TrapLink]) _ = Client.Tags - ArchipelagoTag.TrapLink;
                else _ = Client.Tags + ArchipelagoTag.TrapLink;
            });
        CreateCheckbox(__instance.modTogglePrefab, g42, Data.TrapLinkRandom, "Give random traps\nfor unknown traps",
                b => Data.TrapLinkRandom = b);

        var g5 = CreateHorizontalGroup(panel).gameObject;
        CreateCheckbox(__instance.modTogglePrefab, g5, Data.MusicRando, "Music Rando\n ", b => Data.MusicRando = b);
        CreateCheckbox(__instance.modTogglePrefab, g5, Data.MusicRandoRandomizeOnce, "Music Rando:\nRandomize Once",
            b => Data.MusicRandoRandomizeOnce = b);

        var g6 = CreateHorizontalGroup(panel).gameObject;
        CreateCheckbox(__instance.modTogglePrefab, g6, Data.UseCustomAssets, "Use Archipelago Utilities\n Custom Assets",
                b => Data.UseCustomAssets = b);

        var connectButton = CreateButton(Client.IsConnected ? "Disconnect" : "Connect", g6,
            null, out var buttonText);
        var errorText = CreateText("", Color.red, panel);

        connectButton.onClick.AddListener(() =>
        {
            Core.Log.Msg("Try Connect");

            ItemsWaiting.Clear();
            Items.Clear();

            try
            {
                if (!Client.IsConnected)
                {
                    errorText.text = "";
                    var error = TryConnect(address.text, password.text, slot.text);

                    if (error is null)
                    {
                        buttonText.text = "Disconnect";
                        Data.AddressPort = address.text;
                        Data.Password = password.text;
                        Data.SlotName = slot.text;
                        SaveFile();
                    }
                    else
                    {
                        errorText.text = string.Join("\n", error);
                    }
                }
                else
                {
                    Client.TryDisconnect();
                    buttonText.text = "Connect";
                }
            }
            catch (Exception e)
            {
                Core.Log.Error(e);
            }

            MainMenuPatch.NewGameButton.gameObject.SetActive(Client.IsConnected);
            MainMenuPatch.LoadButton.gameObject.SetActive(Client.IsConnected);
        });
        return false;
    }
}
using CreepyUtil.Archipelago;
using HarmonyLib;
using Slimipelago.Added;
using Slimipelago.Archipelago;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Widgitpelago.Archipelago.CustGui;
using static Slimipelago.Helper;
using static Slimipelago.Archipelago.ApSlimeClient;
using Label = Widgitpelago.Archipelago.CustGui.Label;

namespace Slimipelago.Patches.UiPatches;

[PatchAll]
public static class OptionsPatch
{
    public static DlTlConfig ConfigWindow = new();
    public static GuiUpdaterinator ConfigUpdater;

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
        ConfigUpdater = __instance.gameObject.AddComponent<GuiUpdaterinator>();
        ConfigUpdater.GuiToRender = ConfigWindow;

        var modPanel = __instance.modsPanel.GetChild(0);
        var panel = new GameObject("Ap menu");
        panel.transform.SetParent(modPanel.transform);

        var layout = panel.AddComponent<GridLayoutGroup>();
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.startAxis = GridLayoutGroup.Axis.Horizontal;
        layout.cellSize = new Vector2(850, 50);
        layout.spacing = new Vector2(15, 45);
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

        var topRow = CreateHorizontalGroup(panel).gameObject;
        var middleRow = CreateHorizontalGroup(panel).gameObject;

        CreateCheckbox(__instance.modTogglePrefab, topRow, Data.MusicRando, "Music Rando\n ", b => Data.MusicRando = b);
        CreateCheckbox(
            __instance.modTogglePrefab, topRow, Data.MusicRandoRandomizeOnce, "Music Rando:\nRandomize Once",
            b => Data.MusicRandoRandomizeOnce = b
        );

        CreateButton("Link Configs", middleRow, () =>
        {
            ConfigUpdater.WindowOpen = !ConfigUpdater.WindowOpen;
            SaveFile();
        }, out _);

        var g6 = CreateHorizontalGroup(panel).gameObject;
        CreateCheckbox(
            __instance.modTogglePrefab, g6, Data.UseCustomAssets,
            "Use Archipelago Utilities\nCustom Assets\n(lags on connect)",
            b => Data.UseCustomAssets = b
        );

        var connectButton = CreateButton(
            Client.IsConnected ? "Disconnect" : "Connect", g6,
            null, out var buttonText
        );
        var errorText = CreateText("", Color.red, panel);

        connectButton.onClick.AddListener(() =>
            {
                Core.Log.Msg("Try Connect");

                Items.Clear();
                ItemCache.Clear();

                try
                {
                    if (!Client.IsConnected)
                    {
                        errorText.text = "";
                        Client.DeathLinkGroups.Clear();
                        Client.DeathLinkGroups.Add(ConfigWindow.DeathLinkGroup);
                        var error = TryConnect(address.text, password.text, slot.text);

                        if (error is null)
                        {
                            buttonText.text = "Disconnect";
                            Data.AddressPort = address.text;
                            Data.Password = password.text;
                            Data.SlotName = slot.text;
                            Data.DeathLinkGroup = ConfigWindow.DeathLinkGroup;
                            SaveFile();
                        }
                        else
                        {
                            errorText.text = string.Join("\n", error);
                            Core.Log.Error(string.Join("\n", error));
                        }
                    }
                    else
                    {
                        Client.TryDisconnect();
                        buttonText.text = "Connect";
                    }
                }
                catch (Exception e) { Core.Log.Error(e); }

                MainMenuPatch.NewGameButton.gameObject.SetActive(Client.IsConnected);
                MainMenuPatch.LoadButton.gameObject.SetActive(Client.IsConnected);
            }
        );
        panel.transform.localPosition = Vector3.zero;
        return false;
    }
}

public class DlTlConfig : GuiBuilder
{
    public ScrollWindow ConfigBox = new(0, 20, 20, 350, 500, "Death/Trap Link Config");
    public string DeathLinkGroup;

    public DlTlConfig() => DeathLinkGroup = Data.DeathLinkGroup;

    protected override void InitGUI()
    {
        try
        {
            ConfigBox.AddChildren(
                new Label("DeathLink", 20),
                new ButtonGroup(
                    new SetterGetter<int>(() => Data.DeathLinkTrap ? 2 : Data.DeathLink ? 1 : 0, UpdateDeathLink),
                    ["Off", "On", "Get Trapped"], 20
                ),
                new Label("DeathLink Group", 20),
                new TextField(new SetterGetter<string>(() => DeathLinkGroup, s => DeathLinkGroup = s), 20),
                new Spacer(20),
                new Label("TrapLink", 20),
                new ButtonGroup(
                    new SetterGetter<int>(() => Data.TrapLinkRandom ? 2 : Data.TrapLink ? 1 : 0, UpdateTrapLink),
                    ["Off", "On", "+Unsupported"], 20
                ),
                new Spacer(20)
            );

            foreach (var kv in TrapLoader.TrapTypeToName)
            {
                var trapName = kv.Value;
                if (!Data.TrapConfigs.ContainsKey(trapName))
                {
                    Data.TrapConfigs[trapName] = kv.Key is TrapLoader.Trap.Tarr ? 0 : 2;
                }

                ConfigBox.AddChildren(
                    new Label($"{trapName} Trap", 20),
                    new ButtonGroup(
                        new SetterGetter<int>(() => Data.TrapConfigs[trapName], i =>
                            {
                                Data.TrapConfigs[trapName] = i;
                                TrapLoader.ReLoadAvailableTraps();
                            }
                        ),
                        ["Off", "Receive Only", "On"], 20
                    )
                );
            }
        }
        catch (Exception e) { Core.Log.Error(e); }
    }

    protected override void OnGUI() => ConfigBox.Render();

    public void UpdateTrapLink(int option)
    {
        Data.TrapLinkRandom = option is 2;
        Data.TrapLink = option is not 0;
        if (!Client.IsConnected) return;
        if (Client.Tags[ArchipelagoTag.TrapLink]) _ = Client.Tags - ArchipelagoTag.TrapLink;
        else _ = Client.Tags + ArchipelagoTag.TrapLink;
    }

    public void UpdateDeathLink(int option)
    {
        Data.DeathLinkTrap = option is 2;
        Data.DeathLink = option is not 0;
        if (!Client.IsConnected) return;
        if (Client.Tags[ArchipelagoTag.DeathLink]) _ = Client.Tags - ArchipelagoTag.DeathLink;
        else
        {
            Data.DeathLinkGroup = DeathLinkGroup;
            Client.DeathLinkGroups.Clear();
            Client.DeathLinkGroups.Add(Data.DeathLinkGroup);
            _ = Client.Tags + ArchipelagoTag.DeathLink;
        }
    }
}
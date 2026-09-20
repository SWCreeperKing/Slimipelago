using HarmonyLib;
using JetBrains.Annotations;
using Slimipelago.Added;
using Slimipelago.Archipelago;
using Slimipelago.Archipelago.CustGui;
using TMPro;
using UnityEngine;
using static Slimipelago.Archipelago.ApSlimeClient;

namespace Slimipelago.Patches.UiPatches;

[PatchAll]
public static class MainMenuPatch
{
    public static GameObject ContinueButton;
    public static GameObject LoadButton;
    public static GameObject NewGameButton;
    [CanBeNull] public static event Action OnGamePotentialExit;

    public static LocationsLoader LocationsLoader = new();
    [CanBeNull] public static GuiUpdaterinator LocationsProgressViewer;

    [HarmonyPatch(typeof(MainMenuUI), "Start"), HarmonyPostfix]
    public static void MenuPatch(MainMenuUI __instance)
    {
        OnGamePotentialExit?.Invoke();
        OnGamePotentialExit = null;

        try { DisconnectAndReset(); }
        catch (Exception e) { Core.Log.Error(e); }

        var container = __instance.GetChild(1);
        ContinueButton = container.GetChild(0);
        LoadButton = container.GetChild(1);
        NewGameButton = container.GetChild(2);

        ContinueButton.AddComponent<Invisinator>();
        UpdateButtonStatuses();

        LocationsProgressViewer = __instance.gameObject.AddComponent<GuiUpdaterinator>();
        LocationsProgressViewer!.GuiToRender = LocationsLoader;
        LocationsProgressViewer.WindowOpen = LocationsLoader.LocationsLoading;
    }

    [HarmonyPatch(typeof(NewGameUI), "Start"), HarmonyPostfix]
    public static void PlayPatch(NewGameUI __instance)
    {
        var container = __instance.GetChild(0);
        container.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Create New Archipelago Game";

        var infoPanel = container.GetChild(1);
        infoPanel.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"Slot: [{Data.SlotName}]";

        var inputField = infoPanel.GetChild(1).GetComponent<SRInputField>();
        inputField.readOnly = true;
        inputField.text = GameUUID;

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
            if (name.text != GameUUID)
            {
                child.SetActive(false);
                continue;
            }

            name.text = Data.SlotName;
            hasAny = true;
            child.GetComponent<SRToggle>().Select();
        }

        if (hasAny) return;
        __instance.summaryPanel.gameObject.SetActive(false);
    }

    [HarmonyPatch(typeof(GameSummaryPanel), "Init"), HarmonyPostfix]
    public static void PostSummarySetData(GameSummaryPanel __instance, GameData.Summary gameSummary)
    {
        if (gameSummary.displayName != GameUUID) return;
        __instance.gameNameText.text = Data.SlotName;
    }

    public static void UpdateButtonStatuses()
    {
        var visible = Client.IsConnected;

        if (visible) visible = LocationsLoader.LocationsStatus;

        NewGameButton.gameObject.SetActive(visible);
        LoadButton.gameObject.SetActive(visible);
    }
}

public class LocationsLoader : GuiBuilder
{
    public bool LocationsLoading;
    public bool LocationsStatus;
    public int LocationsToScout;
    public int LocationsScouted;
    public int LastScoutedCount;
    public int NextScoutedCount;
    public string LocationsText;

    public Window ProgressWindow;

    protected override void InitGUI()
    {
        LocationsLoading = true;
        LocationsStatus = false;
        LocationsToScout = 0;
        LocationsScouted = 0;
        LastScoutedCount = 0;
        NextScoutedCount = 0;

        ProgressWindow = new Window(67, 30, 30, 300, "Locations Loader")
           .AddChild(new Label(new TextRef(() => LocationsText), 30, alignment: TextAnchor.MiddleCenter));
    }

    protected override void OnGUI()
    {
        lock (this)
        {
            NextScoutedCount = LocationsScouted; // redundancy
        }

        if (NextScoutedCount != LastScoutedCount)
        {
            LocationsText
                = $"Loaded: {LocationsScouted}/{LocationsToScout} ({(double)LocationsScouted / LocationsToScout * 100:##0.00})%";
            LastScoutedCount = NextScoutedCount;
        }

        ProgressWindow.Render();
    }

    public async void LoadScoutLocations()
    {
        MainMenuPatch.LocationsProgressViewer?.WindowOpen = true;
        lock (this)
        {
            LocationsLoading = true;
            LocationsStatus = false;
            LocationsToScout = 0;
            LocationsScouted = 0;
        }

        try
        {
            ScoutedLocations.Clear();
            ItemHandler.ItemSprites.Clear();
            var list = UpgradeLocations.Values.Concat(LocationDictionary.Values)
                                       .Concat(CorporateLocations.Values.SelectMany(s => s))
                                       .Concat(LogicHandler.PlortLocations.Values)
                                       .Where(s => Client.IsMissingLocation(s))
                                       .ToArray();

            lock (this) LocationsToScout = list.Length;
            Core.Log.Msg($"Loading [{LocationsToScout}] Locations");

            var i = 0;
            var milestone = 0;
            foreach (var loc in list)
            {
                try
                {
                    if (!ScoutedLocations.TryGetValue(loc, out var itemInfo))
                    {
                        var scoutedLoc = Client.ScoutLocation(loc);
                        if (scoutedLoc is null) continue;
                        itemInfo = ScoutedLocations[loc] = scoutedLoc;
                    }

                    if (Data.UseCustomAssets) ItemHandler.ItemImage(itemInfo);
                }
                catch { Core.Log.Error($"Could not scout location: [{loc}]"); }
                i++;
                lock (this) LocationsScouted = list.Length;
                var curMilestone = (int)(Math.Floor((double)i / LocationsToScout * 10));
                if (milestone == curMilestone) continue;
                milestone = curMilestone;
                Core.Log.Msg($"Loaded: {i}/{LocationsToScout} ({(double)i / LocationsToScout * 100:##0.00})%");
            }
        }
        catch (Exception e) { Core.Log.Error(e); }
        Core.Log.Msg("Loaded all Locations");

        MainMenuPatch.LocationsProgressViewer?.WindowOpen = false;
        lock (this)
        {
            LocationsLoading = false;
            LocationsStatus = true;
            LocationsToScout = 0;
            LocationsScouted = 0;
        }
    }
}
// using Archipelago.MultiClient.Net.Enums;
// using CreepyUtil.Archipelago;
// using CreepyUtil.Archipelago.ApClient;

using System.Collections.Concurrent;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using CreepyUtil.Archipelago;
using CreepyUtil.Archipelago.ApClient;
using JetBrains.Annotations;
using MonomiPark.SlimeRancher.DataModel;
using Newtonsoft.Json;
using Slimipelago.Patches.Interactables;
using Slimipelago.Patches.PlayerPatches;
using static Slimipelago.Patches.PlayerPatches.PlayerModelPatch;

namespace Slimipelago;

public static class ApSlimeClient
{
    public static Dictionary<string, string> LocationDictionary = [];
    public static Dictionary<string, string> LocationInfoDictionary = [];
    public static List<ItemInfo> Items = [];
    public static List<ItemInfo> ItemsWaiting = [];
    public static ConcurrentDictionary<string, int> ItemCache = [];
    public static ApClient Client = new();

    public static string AddressPort = "archipelago.gg:12345";
    public static string Password = "";
    public static string SlotName = "Rancher1";

    public static string[] Zones =
    [
        "The Ranch",
        "The Lab",
        "The Overgrowth",
        "The Grotto",
        "Dry Reef",
        "Indigo Quarry",
        "Moss Blanket",
        "Ancient Ruins Transition",
        "Ancient Ruins",
        "Glass Desert",
// "The Slime Sea",
    ];

    public static string GameUUID = "";

    public static void Init()
    {
        if (File.Exists("ApConnection.txt"))
        {
            var fileText = File.ReadAllText("ApConnection.txt").Replace("\r", "").Split('\n');
            AddressPort = fileText[0];
            Password = fileText[1];
            SlotName = fileText[2];
        }

        Client.OnConnectionLost += () =>
        {
            PlayerStatePatch.SaveAndQuitButton?.onClick?.Invoke();
            Core.Log.Error("Lost Connection to Ap");
        };

        Client.OnConnectionEvent += _ => { GameUUID = (string)Client.SlotData["uuid"]; };

        Client.OnConnectionErrorReceived += (e, s) => { Core.Log.Error(e); };
    }

    [CanBeNull]
    public static string[] TryConnect(string addressPort, string password, string slotName)
    {
        var addressSplit = addressPort.Split(':');

        if (addressSplit.Length != 2) return ["Address Field is incorrect"];
        if (!int.TryParse(addressSplit[1], out var port)) return ["Port is incorrect"];

        var login = new LoginInfo(port, slotName, addressSplit[0], password);
        return Client.TryConnect(login, "Slime Rancher", ItemsHandlingFlags.AllItems);
    }

    public static void Update()
    {
        if (Client is null) return;
        Client.UpdateConnection();

        if (!Client.IsConnected) return;
        ItemsWaiting.AddRange(Client.GetOutstandingItems()!);

        if (!PlayerStatePatch.FirstUpdate) return;
        foreach (var item in ItemsWaiting)
        {
            ProcessItem(item);
        }

        Items.AddRange(ItemsWaiting);
        ItemsWaiting.Clear();
    }

    public static void SaveFile() => File.WriteAllText("ApConnection.txt", $"{AddressPort}\n{Password}\n{SlotName}");

    public static void WorldOpened()
    {
        Core.Log.Msg("World Opened");
        PlayerTrackerPatch.AllowedZones.Clear();
        AccessDoorPatch.LabDoor.CurrState = AccessDoor.State.CLOSED;
        AccessDoorPatch.OvergrowthDoor.CurrState = AccessDoor.State.CLOSED;
        AccessDoorPatch.GrottoDoor.CurrState = AccessDoor.State.CLOSED;
        AccessDoorPatch.DocksDoor.CurrState = AccessDoor.State.OPEN;

        ItemCache.Clear();

        foreach (var item in Items)
        {
            ProcessItem(item);
        }
    }

    public static void ProcessItem(ItemInfo item)
    {
        var name = item.ItemName;
        if (name.Contains("Region Unlock: "))
        {
            RegionItem(name.Substring(15));
        }
        else
        {
            UpgradeItem(name);
        }
    }

    public static void RegionItem(string region)
    {
        if (PlayerTrackerPatch.ZoneTypeToName.ContainsValue(region))
        {
            PlayerTrackerPatch.AllowedZones.Add(region);
        }
        else
        {
            switch (region)
            {
                case "The Lab":
                    AccessDoorPatch.LabDoor.CurrState = AccessDoor.State.OPEN;
                    break;
                case "The Overgrowth":
                    AccessDoorPatch.OvergrowthDoor.CurrState = AccessDoor.State.OPEN;
                    break;
                case "The Grotto":
                    AccessDoorPatch.GrottoDoor.CurrState = AccessDoor.State.OPEN;
                    break;
            }
        }
    }

    public static void UpgradeItem(string name)
    {
        if (name.StartsWith("Progressive"))
        {
            ItemCache.TryAdd(name, 0);
        }

        try
        {
            switch (name)
            {
                case "Air Burst":
                    Model.hasAirBurst = true;
                    return;
                case "Liquid Slot":
                    Model.ammoDict[PlayerState.AmmoMode.DEFAULT].IncreaseUsableSlots(5);
                    return;
                case "Golden Sure Shot":
                    return;
                case "Progressive Max Health":
                    Model.maxHealth = 150 + 50 * ItemCache[name];
                    if ((double)Model.currHealth >= Model.maxHealth)
                        break;
                    Model.healthBurstAfter =
                        Math.Min(Model.healthBurstAfter, PlayerModelPatch.WorldModel.worldTime + 300.0);
                    break;
                case "Progressive Max Ammo":
                    Model.maxAmmo = PlayerModel.DEFAULT_MAX_AMMO[ItemCache[name] + 1];
                    break;
                case "Progressive Run Efficiency":
                    Model.runEfficiency = ItemCache[name] == 0 ? 0.667f : .5f;
                    break;
                case "Progressive Max Energy":
                    Model.maxEnergy = 150 + 50 * ItemCache[name];
                    if ((double)Model.currEnergy >= Model.maxEnergy)
                        break;
                    Model.energyRecoverAfter = Math.Min(Model.energyRecoverAfter,
                        PlayerModelPatch.WorldModel.worldTime + 300.0);
                    break;
                case "Progressive Jetpack":
                    switch (ItemCache[name])
                    {
                        case 0:
                            JetpackPatch.EnableJetpack = true;
                            Model.jetpackEfficiency = 1f;
                            break;
                        case 1:
                            Model.jetpackEfficiency = .8f;
                            break;
                    }

                    break;
                case "Progressive Treasure Cracker":
                    break;
                default:
                    return;
            }

            ItemCache[name]++;
        }
        catch (Exception e)
        {
            Core.Log.Msg(JsonConvert.SerializeObject(ItemCache, Formatting.Indented));
            Core.Log.Msg($"Error with: {name}");
            Core.Log.Error(e);
        }
    }
}
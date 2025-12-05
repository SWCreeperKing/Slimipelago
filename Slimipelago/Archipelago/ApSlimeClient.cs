using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using CreepyUtil.Archipelago;
using CreepyUtil.Archipelago.ApClient;
using JetBrains.Annotations;
using KaitoKid.ArchipelagoUtilities.AssetDownloader.ItemSprites;
using Slimipelago;
using Slimipelago.Archipelago;
using Slimipelago.Patches.Interactables;
using Slimipelago.Patches.PlayerPatches;
using Slimipelago.Patches.UiPatches;

namespace Slimipzelago.Archipelago;

public static class ApSlimeClient
{
    public static Dictionary<string, int> RandoSeeds = [];
    public static Dictionary<string, string> LocationDictionary = [];
    public static Dictionary<string, string> LocationInfoDictionary = [];
    public static List<ItemInfo> Items = [];
    public static List<ItemInfo> ItemsWaiting = [];
    public static ConcurrentDictionary<string, int> ItemCache = [];
    public static ApClient Client = new();

    public static string AddressPort = "archipelago.gg:12345";
    public static string Password = "";
    public static string SlotName = "Rancher1";
    public static bool DeathLink = false;
    public static bool DeathLinkTeleport = false;
    public static bool TrapLink = false;
    public static bool TrapLinkRandom = false;
    public static bool MusicRando = false;
    public static bool MusicRandoRandomizeOnce = false;
    public static bool UseCustomAssets = true;

    public static long CurrentItemIndex;

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

            if (fileText.Length > 3)
            {
                var boolArr = fileText[3].ToCharArray();
                DeathLink = StrBool(boolArr[0]);
                DeathLinkTeleport = StrBool(boolArr[1]);
                TrapLink = StrBool(boolArr[2]);
                TrapLinkRandom = StrBool(boolArr[3]);
                MusicRando = StrBool(boolArr[4]);
                MusicRandoRandomizeOnce = StrBool(boolArr[5]);
                if (boolArr.Length > 6) UseCustomAssets = StrBool(boolArr[6]);
            }
        }

        Client.OnConnectionLost += () =>
        {
            PlayerStatePatch.SaveAndQuitButton?.onClick?.Invoke();
            Core.Log.Error("Lost Connection to Ap");
        };

        Client.OnConnectionEvent += _ =>
        {
            GameUUID = (string)Client.SlotData["uuid"];
            CurrentItemIndex = Client.GetFromStorage("new_item_index", def: 0L);

            var seed = Client.Seed;
            if (seed is null)
            {
                Core.Log.Warning("Could not get the seed of the room");
                return;
            }

            using var sha = SHA1.Create();
            RandoSeeds[seed!] = BitConverter.ToInt32(sha.ComputeHash(Encoding.UTF8.GetBytes(seed)), 0);
        };

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
            ItemHandler.ProcessItem(item);
        }

        Items.AddRange(ItemsWaiting);
        ItemsWaiting.Clear();
    }

    public static void SaveFile()
        => File.WriteAllText("ApConnection.txt",
            $"{AddressPort}\n{Password}\n{SlotName}\n{BoolStr(DeathLink)}{BoolStr(DeathLinkTeleport)}{BoolStr(TrapLink)}{BoolStr(TrapLinkRandom)}{BoolStr(MusicRando)}{BoolStr(MusicRandoRandomizeOnce)}{BoolStr(UseCustomAssets)}");

    private static char BoolStr(bool b) => b ? '1' : '0';
    private static bool StrBool(char c) => c == '1';

    public static void WorldOpened()
    {
        try
        {
            PersonalUpgradePatch.ScoutedUpgrades = [];
            Core.Log.Msg("World Opened");
            ItemHandler.ItemNumberTracker = 0;
            PlayerTrackerPatch.AllowedZones.Clear();
            AccessDoorPatch.LabDoor.CurrState = AccessDoor.State.CLOSED;
            AccessDoorPatch.OvergrowthDoor.CurrState = AccessDoor.State.CLOSED;
            AccessDoorPatch.GrottoDoor.CurrState = AccessDoor.State.CLOSED;
            AccessDoorPatch.DocksDoor.CurrState = AccessDoor.State.OPEN;

            ItemCache.Clear();

            foreach (var item in Items)
            {
                ItemHandler.ProcessItem(item);
            }
        }
        catch (Exception e)
        {
            Core.Log.Error(e);
        }
    }

    public static void SendItem(string locationType, string location)
    {
        var item = Client.ScoutLocation(location);
        if (item?.Player.Slot != Client.PlayerSlot)
        {
            if (item is null)
            {
                PopupPatch.AddItemToQueue(new ApPopupData(GameLoader.Spritemap["normal"], locationType,
                    location));
            }
            else
            {
                var loc = new AssetItem(item.ItemGame, item.ItemName, item.Flags);
                PopupPatch.AddItemToQueue(new ApPopupData(ItemHandler.ItemImage(loc), locationType,
                    $"sent: [{item.ItemName}]", $"to: {item.Player.Name}"));
            }
        }

        Client.SendLocation(location);

        if (Client.MissingLocations.Any(loc => loc.ToLower().Contains("note"))) return;
        Client.Goal();
    }
}

public class AssetItem(string game, string item, ItemFlags flags) : IAssetLocation
{
    public int GetSeed() => 0;
    public string GameName { get; } = game;
    public string ItemName { get; } = item;
    public ItemFlags ItemFlags { get; } = flags;
}
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using CreepyUtil.Archipelago;
using CreepyUtil.Archipelago.ApClient;
using JetBrains.Annotations;
using KaitoKid.ArchipelagoUtilities.AssetDownloader.ItemSprites;
using Newtonsoft.Json;
using Slimipelago.Added;
using Slimipelago.Patches.Interactables;
using Slimipelago.Patches.PlayerPatches;
using Slimipelago.Patches.UiPatches;
using static Slimipelago.Archipelago.TrapLoader;

namespace Slimipelago.Archipelago;

public static class ApSlimeClient
{
    public static Dictionary<string, int> RandoSeeds = [];
    public static Dictionary<string, string> LocationDictionary = [];
    public static Dictionary<string, string> LocationInfoDictionary = [];
    public static Dictionary<PlayerState.Upgrade, string> UpgradeLocations;
    public static Dictionary<int, string[]> CorporateLocations;
    public static List<ItemInfo> Items = [];
    public static List<ItemInfo> ItemsWaiting = [];
    public static ConcurrentDictionary<string, int> ItemCache = [];
    public static string[] HintedItems = [];
    public static ApClient Client = new();

    public static ApData Data;
    public static bool HackTheMarket = true;
    public static bool QueueReLogic;
    public static long GoalType;

    public static long CurrentItemIndex;

    public static string GameUUID = "";

    public static void Init()
    {
        if (File.Exists("ApConnection.json"))
        {
            Data = JsonConvert.DeserializeObject<ApData>(File.ReadAllText("ApConnection.json").Replace("\r", ""));
        }

        if (File.Exists("ApConnection.txt"))
        {
            Data = new ApData();
            Data.Init();
            SaveFile();
            File.Delete("ApConnection.txt");
        }

        Client.OnConnectionLost += () =>
        {
            PlayerStatePatch.SaveAndQuitButton?.onClick?.Invoke();
            Core.Log.Error("Lost Connection to Ap");
        };

        Client.OnConnectionEvent += _ =>
        {
            HintedItems = [];
            GameUUID = (string)Client.SlotData["uuid"];
            CurrentItemIndex = Client.GetFromStorage("new_item_index", def: 0L);

            HackTheMarket = !Client.SlotData.TryGetValue("fix_market_rates", out var value) || (bool)value;
            GoalType = Client.SlotData.TryGetValue("goal_type", out var value1) ? (long)value1 : 0;

            var seed = Client.Seed;
            if (seed is null)
            {
                Core.Log.Warning("Could not get the seed of the room");
                return;
            }

            using var sha = SHA1.Create();
            RandoSeeds[seed!] = BitConverter.ToInt32(sha.ComputeHash(Encoding.UTF8.GetBytes(seed)), 0);
            LoadTrapData();
        };

        Client.OnConnectionErrorReceived += (e, s) => { Core.Log.Error(e); };

        Client.OnUnregisteredTrapLinkReceived +=
            (player, trap) => TrapLinkTraps.Enqueue(new TrapLinkTrap(trap, player));

        Client.OnDeathLinkPacketReceived += (player, message) =>
        {
            if (Data.DeathLinkTrap)
            {
                TrapLinkTraps.Enqueue(new TrapLinkTrap(BaseTrapNames[Playground.Random.Next(BaseTrapNames.Count)],
                    $"(DeathLink) {player}"));
            }
            else
            {
                PlayerDeathHandlerPatch.DeathlinkRecieved = true;
                DeathHandler.Kill(PlayerStatePatch.PlayerInWorld, DeathHandler.Source.CHICKEN_VAMPIRISM, null, "DeathLink");
            }
        };
    }

    [CanBeNull]
    public static string[] TryConnect(string addressPort, string password, string slotName)
    {
        var addressSplit = addressPort.Split(':');

        if (addressSplit.Length != 2) return ["Address Field is incorrect"];
        if (!int.TryParse(addressSplit[1], out var port)) return ["Port is incorrect"];

        var login = new LoginInfo(port, slotName, addressSplit[0], password);

        List<ArchipelagoTag> tags = [];

        if (Data.DeathLink) tags.Add(ArchipelagoTag.DeathLink);
        if (Data.TrapLink) tags.Add(ArchipelagoTag.TrapLink);

        return Client.TryConnect(login, "Slime Rancher", ItemsHandlingFlags.AllItems, tags: tags.ToArray());
    }

    public static void Update()
    {
        try
        {
            if (Client is null) return;
            Client.UpdateConnection();

            if (!Client.IsConnected) return;
            ItemsWaiting.AddRange(Client.GetOutstandingItems()!);

            if (QueueReLogic)
            {
                QueueReLogic = false;
                LogicHandler.LogicCheck();
            }

            if (Client.PushUpdatedVariables(false, out var hints))
            {
                var player = Client.PlayerSlot;
                HintedItems = hints.Where(hint => hint.Status is HintStatus.Priority && hint.FindingPlayer == player)
                                   .Select(hint => Client.LocationIdToLocationName(hint.LocationId, player))
                                   .ToArray();
                LogicHandler.LogicCheck();
                QueueReLogic = true;
            }

            if (!PlayerStatePatch.FirstUpdate || !ItemsWaiting.Any()) return;
            foreach (var item in ItemsWaiting)
            {
                ItemHandler.ProcessItem(item);
            }

            Items.AddRange(ItemsWaiting);
            ItemsWaiting.Clear();
            QueueReLogic = true;
        }
        catch (Exception e)
        {
            Core.Log.Error(e);
        }
    }

    public static void SaveFile() => File.WriteAllText("ApConnection.json", JsonConvert.SerializeObject(Data));

    public static void WorldOpened()
    {
        try
        {
            Core.Log.Msg("World Opened");
            PersonalUpgradePatch.ScoutedUpgrades.Clear();
            ItemHandler.ItemNumberTracker = 0;
            PlayerTrackerPatch.AllowedZones.Clear();

            AccessDoorPatch.LabDoor.CurrState = AccessDoor.State.CLOSED;
            AccessDoorPatch.OvergrowthDoor.CurrState = AccessDoor.State.CLOSED;
            AccessDoorPatch.GrottoDoor.CurrState = AccessDoor.State.CLOSED;
            AccessDoorPatch.DocksDoor.CurrState = AccessDoor.State.OPEN;

            Items.Clear();
            ItemCache.Clear();

            if (SRSingleton<GameContext>.Instance.AutoSaveDirector.IsNewGame())
            {
                CurrentItemIndex = 0;
            }

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

    public static void SendItems(string locationType, string[] locations)
    {
        foreach (var location in locations)
        {
            QueueItemPopup(locationType, location);
        }

        Client.SendLocations(locations);
        UpdateGoal();
    }

    public static void SendItem(string locationType, string location)
    {
        QueueItemPopup(locationType, location);
        Client.SendLocation(location);
        UpdateGoal();
    }

    public static void QueueItemPopup(string locationType, string location)
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
    }

    public static void UpdateGoal()
    {
        QueueReLogic = true;

        if (Client.MissingLocations.Any(loc => GoalType switch
            {
                0 => loc.ToLower().Contains("note"),
                1 => loc.ToLower().Contains("7zee lv."),
                _ => true
            })) return;
        Client.Goal();
    }

    public static void DisconnectAndReset()
    {
        try
        {
            if (Client.IsConnected) Client.TryDisconnect();
            GameUUID = null;
            CurrentItemIndex = 0;
            JetpackPatch.EnableJetpack = false;

            ItemsWaiting.Clear();
            AccessDoorPatch.TeleportMarkers.Clear();
            PersonalUpgradePatch.PurchaseNameKeyToLocation.Clear();
            PopupPatch.Reset();
        }
        catch (Exception e)
        {
            Core.Log.Error(e);
        }
    }
}

public class AssetItem(string game, string item, ItemFlags flags) : IAssetLocation
{
    public int GetSeed() => 0;
    public string GameName { get; } = game;
    public string ItemName { get; } = item;
    public ItemFlags ItemFlags { get; } = flags;
}
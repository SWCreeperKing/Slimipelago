using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using CreepyUtil.Archipelago;
using CreepyUtil.Archipelago.ApClient;
using JetBrains.Annotations;
using KaitoKid.Utilities.Interfaces;
using Newtonsoft.Json;
using Slimipelago.Added;
using Slimipelago.Patches.Interactables;
using Slimipelago.Patches.PlayerPatches;
using Slimipelago.Patches.UiPatches;
using UnityEngine;
using static Slimipelago.Archipelago.TrapLoader;
using static Slimipelago.Patches.UiPatches.PersonalUpgradePatch;

namespace Slimipelago.Archipelago;

public enum GoalType
{
    Notes = 0, Corporate = 1, Credits = 2,
}

public static class ApSlimeClient
{
    public static Dictionary<string, int> RandoSeeds = [];
    public static Dictionary<string, string> LocationDictionary = [];
    public static Dictionary<string, string> LocationInfoDictionary = [];
    public static Dictionary<PlayerState.Upgrade, string> UpgradeLocations;
    public static Dictionary<int, string[]> CorporateLocations;
    public static List<ItemInfo> Items = [];
    public static ConcurrentDictionary<string, int> ItemCache = [];
    public static string[] HintedItems = [];
    public static ApClient Client = new(new TimeSpan(0, 1, 0));
    public static bool QueuedDeathLink = false;
    public static LoseFlag<string> NoteLocations;
    public static Dictionary<string, ScoutedItemInfo> ScoutedLocations = [];
    public static Dictionary<string, string> GateLocks = [];
    public static bool EnableJetpack = false;
    public static int NoteCount;
    public static int CurrentNotes;

    public static ApData Data = new();
    public static bool HackTheMarket = true;
    public static bool QueueReLogic;
    public static long CurrentItemIndex;
    public static string GameUUID = "";

    public static void Init()
    {
        Core.Log.Msg($"TIMEOUT: [{ArchipelagoSession.ArchipelagoConnectionTimeoutInSeconds}]");
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
            QueuedDeathLink = false;
            PlayerDeathHandlerPatch.DeathlinkRecieved = false;
            HintedItems = [];
            GameUUID = (string)Client.SlotData["uuid"];
            CurrentItemIndex = Client.GetFromStorage("new_item_index", def: 0L);

            HackTheMarket = !Client.SlotData.TryGetValue("fix_market_rates", out var value) || (bool)value;
            Client.SetGoalType((GoalType)(Client.SlotData.TryGetValue("goal_type", out var value1) ? (long)value1 : 0));

            NoteLocations.SetFlag(Client.GetFromStorage("note_locations", def: 0ul));
            CurrentNotes = Convert.ToString((long)(ulong)NoteLocations, 2).Count(c => c is '1');

            LogicHandler.SkipLogic[SkipLogic.None] = true;
            LogicHandler.SkipLogic[SkipLogic.EasySkips] = Client.SlotData.TryGetValue("easy_skips", out var l)
                                                          && (bool)l;
            LogicHandler.SkipLogic[SkipLogic.PreciseMovement]
                = Client.SlotData.TryGetValue("precise_movement", out var l1) && (bool)l1;
            LogicHandler.SkipLogic[SkipLogic.ObscureLocations]
                = Client.SlotData.TryGetValue("obscure_locations", out var l2) && (bool)l2;
            LogicHandler.SkipLogic[SkipLogic.JetpackBoosts] = Client.SlotData.TryGetValue("jetpack_boosts", out var l3)
                                                              && (bool)l3;
            LogicHandler.SkipLogic[SkipLogic.LargoJumps] = Client.SlotData.TryGetValue("largo_jumps", out var l4)
                                                           && (bool)l4;
            LogicHandler.SkipLogic[SkipLogic.DangerousSkips]
                = Client.SlotData.TryGetValue("dangerous_skips", out var l5) && (bool)l5;

            var seed = Client.Seed;
            if (seed is null)
            {
                Core.Log.Warning("Could not get the seed of the room");
                return;
            }

            using var sha = SHA1.Create();
            RandoSeeds[seed!] = BitConverter.ToInt32(sha.ComputeHash(Encoding.UTF8.GetBytes(seed)), 0);
            LoadTrapData();

            ScoutedLocations.Clear();
            ItemHandler.ItemSprites.Clear();
            var list = UpgradeLocations.Values.Concat(LocationDictionary.Values)
                                       .Concat(CorporateLocations.Values.SelectMany(s => s))
                                       .Concat(LogicHandler.PlortLocations.Values)
                                       .Where(s => Client.MissingLocations.Contains(s))
                                       .ToArray();

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
            }
        };

        Client.OnConnectionErrorReceived += (e, s) => Core.Log.Error(e);

        Client.OnUnregisteredTrapLinkReceived +=
            (player, trap) => TrapLinkTraps.Enqueue(new TrapLinkTrap(trap, player));

        Client.OnDeathLinkPacketReceived += (group, player, message) =>
        {
            Core.Log.Msg($"DeathLink from [{player}], [{group}]: [{message}] ({Data.DeathLinkTrap})");
            if (Data.DeathLinkTrap)
            {
                TrapLinkTraps.Enqueue(
                    new TrapLinkTrap(
                        BaseTrapNames[Playground.Random.Next(BaseTrapNames.Count)],
                        $"(DeathLink) {player}"
                    )
                );
            }
            else
            {
                QueuedDeathLink = true;
                PlayerDeathHandlerPatch.DeathlinkRecieved = true;
                Core.Log.Msg($"DL queued: [{QueuedDeathLink}],[{PlayerDeathHandlerPatch.DeathlinkRecieved}]");
            }
        };

        Client.HintsTrackedEvent += hints =>
        {
            var player = Client.PlayerSlot;
            HintedItems = hints.Where(hint => hint.Status is HintStatus.Priority && hint.FindingPlayer == player)
                               .Select(hint => Client.LocationIdToLocationName(hint.LocationId, player))
                               .ToArray();
            QueueReLogic = true;
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

            if (QueueReLogic)
            {
                try
                {
                    LogicHandler.LogicCheck();

                    QueueReLogic = false;
                }
                catch (Exception e) { Core.Log.Error(e); }
            }

            if (PlayerStatePatch.FirstUpdate)
            {
                var items = Client.GetOutstandingItems();
                if (items.Length > 0)
                {
                    for (var index = 0; index < items.Length; index++)
                    {
                        var item = items[index];
                        if (Core.DebugLevel > 0)
                        {
                            Core.Log.Msg(
                                $"Handling Item: [{item.ItemName}] | [{index + 1}/{items.Length + Items.Count}]"
                            );
                        }
                        ItemHandler.ProcessItem(item);
                        if (Core.DebugLevel > 0)
                        {
                            Core.Log.Msg(
                                $"Handled Item: [{item.ItemName}] | [{index + 1}/{items.Length + Items.Count}]"
                            );
                        }
                    }
                    Items.AddRange(items);
                    QueueReLogic = true;
                    Core.Log.Msg($"Handled all [{items.Length}] items");
                }
            }
            else return;

            // Core.Log.Msg($"Update: [{QueuedDeathLink}]");
            // Core.Log.Msg(
            //     $"{SRSingleton<SceneContext>.Instance.TimeDirector.HasPauser()}, {SRSingleton<SceneContext>.Instance.TimeDirector.IsFastForwarding()}");
            if (SRSingleton<SceneContext>.Instance.TimeDirector.HasPauser()) return;
            if (SRSingleton<SceneContext>.Instance.TimeDirector.IsFastForwarding()) return;
            if (PlayerStatePatch.Disabler is null) return;
            if (PlayerStatePatch.Disabler.GetPrivateField<List<Component>>("blockers").Any()) return;
            TrapLoader.Update();
            if (!QueuedDeathLink) return;
            DeathHandler.Kill(PlayerStatePatch.PlayerInWorld, DeathHandler.Source.CHICKEN_VAMPIRISM, null, "DeathLink");
            QueuedDeathLink = false;
        }
        catch (Exception e) { Core.Log.Error(e); }
    }

    public static void SaveFile() => File.WriteAllText("ApConnection.json", JsonConvert.SerializeObject(Data));

    public static void WorldOpened()
    {
        try
        {
            Core.Log.Msg("World Opened");
            ItemHandler.ItemNumberTracker = 0;
            PlayerTrackerPatch.AllowedZones.Clear();

            AccessDoorPatch.LabDoor.CurrState = AccessDoor.State.CLOSED;
            AccessDoorPatch.OvergrowthDoor.CurrState = AccessDoor.State.CLOSED;
            AccessDoorPatch.GrottoDoor.CurrState = AccessDoor.State.CLOSED;
            AccessDoorPatch.DocksDoor.CurrState = AccessDoor.State.CLOSED;

            if (SRSingleton<GameContext>.Instance.AutoSaveDirector.IsNewGame()) CurrentItemIndex = 0;

            if (Core.DebugLevel < 1 && !Client.IsGoalType(GoalType.Notes)) return;
            GameObject.Find("HUD Root/HudUI/UIContainer").AddComponent<UITracker>();
        }
        catch (Exception e) { Core.Log.Error(e); }
    }

    public static void SendItems(string locationType, string[] locations)
    {
        foreach (var location in locations) { QueueItemPopup(locationType, location); }

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
        try
        {
            var item = Client.ScoutLocation(location);
            if (item?.Player.Slot == Client.PlayerSlot) return;

            if (item is null)
            {
                PopupPatch.AddItemToQueue(
                    new ApPopup(
                        GameLoader.Spritemap["normal"], locationType,
                        location
                    )
                );
            }
            else
            {
                PopupPatch.AddItemToQueue(
                    new ApPopup(
                        ItemHandler.ItemImage(item), locationType,
                        $"sent: [{item.ItemName}]", $"to: {item.Player.Name}"
                    )
                );
            }
        }
        catch (Exception e) { Core.Log.Error(e); }
    }

    public static void UpdateGoal()
    {
        QueueReLogic = true;

        if (!NoteLocations.IsMaxFlag()) return;
        Client.TryGoal(GoalType.Notes);
    }

    public static void DisconnectAndReset()
    {
        try
        {
            if (Client.IsConnected) Client.TryDisconnect();
            GameUUID = null;
            CurrentItemIndex = 0;
            EnableJetpack = false;
            AccessDoorPatch.TeleportMarkers.Clear();
            PurchaseNameKeyToLocation.Clear();
        }
        catch (Exception e) { Core.Log.Error(e); }
    }
}

public class AssetItem(string game, string item, ItemFlags flags) : IAssetLocation
{
    public int GetSeed() => 0;
    public string GameName { get; } = game;
    public string ItemName { get; } = item;
    public ItemFlags ItemFlags { get; } = flags;
    public string Uid = $"{game};{item};{flags}";

    public static implicit operator AssetItem(ScoutedItemInfo item) => new(item.ItemGame, item.ItemName, item.Flags);
}
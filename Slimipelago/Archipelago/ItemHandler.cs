using Archipelago.MultiClient.Net.Models;
using MonomiPark.SlimeRancher.DataModel;
using Newtonsoft.Json;
using Slimipelago.Patches.Interactables;
using Slimipelago.Patches.PlayerPatches;
using Slimipelago.Patches.UiPatches;
using UnityEngine;
using static Slimipelago.Patches.PlayerPatches.PlayerModelPatch;
using static Slimipelago.Archipelago.ApSlimeClient;
using static Slimipelago.Archipelago.ItemConstants;

namespace Slimipelago.Archipelago;

public static class ItemHandler
{
    public static Dictionary<string, Sprite> ItemSprites = [];
    public static long ItemNumberTracker;

    public static void ProcessItem(ItemInfo item)
    {
        ItemNumberTracker++;
        var firstTime = ItemNumberTracker > CurrentItemIndex;

        var name = item.ItemName;
        // Core.Log.Msg($"Handling Item: [{item.ItemName}]");
        if (name.Contains("Region Unlock: "))
        {
            RegionItem(name.Substring(15));
        }
        else
        {
            UpgradeItem(name, firstTime);
        }

        if (!firstTime) return;

        var sprite = GameLoader.GetSpriteFromItemFlag(item.Flags);
        if (sprite is "trap") sprite = "got_trap";

        PopupPatch.AddItemToQueue(new ApPopupData(GameLoader.Spritemap[sprite], "Item Received", item.ItemName,
            $"from: {item.Player.Name}", () =>
            {
                if (ItemNumberTracker <= CurrentItemIndex) return;
                Client.SendToStorage("new_item_index", ItemNumberTracker);
            }));
    }

    public static void RegionItem(string region)
    {
        if (PlayerTrackerPatch.ZoneTypeToName.ContainsValue(region))
        {
            PlayerTrackerPatch.AllowedZones.Add(region);
            if (region != "Dry Reef") return;
            GameLoader.MakeTeleporterMarker(GameLoader.Reef);
        }
        else
        {
            switch (region)
            {
                case "The Lab":
                    AccessDoorPatch.LabDoor.CurrState = AccessDoor.State.OPEN;
                    PlayerTrackerPatch.AllowedZones.Add(region);
                    GameLoader.MakeTeleporterMarker(GameLoader.Lab);
                    break;
                case "The Overgrowth":
                    AccessDoorPatch.OvergrowthDoor.CurrState = AccessDoor.State.OPEN;
                    PlayerTrackerPatch.AllowedZones.Add(region);
                    GameLoader.MakeTeleporterMarker(GameLoader.Overgrowth);
                    break;
                case "The Grotto":
                    AccessDoorPatch.GrottoDoor.CurrState = AccessDoor.State.OPEN;
                    PlayerTrackerPatch.AllowedZones.Add(region);
                    GameLoader.MakeTeleporterMarker(GameLoader.Grotto);
                    break;
            }
        }
    }

    public static void UpgradeItem(string name, bool firstReceive)
    {
        if (name.StartsWith("Progressive") || name is TrapSlime)
        {
            ItemCache.TryAdd(name, 0);
        }

        try
        {
            switch (name)
            {
                case AirBurst:
                    Model.hasAirBurst = true;
                    return;
                case LiquidSlot:
                    Model.ammoDict[PlayerState.AmmoMode.DEFAULT].IncreaseUsableSlots(5);
                    return;
                case SureShot:
                    return;
                case MaxHealth:
                    Model.maxHealth = 150 + 50 * ItemCache[name];
                    if ((double)Model.currHealth >= Model.maxHealth)
                    {
                        ItemCache[name]++;
                        return;
                    }

                    Model.healthBurstAfter =
                        Math.Min(Model.healthBurstAfter, PlayerModelPatch.WorldModel.worldTime + 300.0);
                    ItemCache[name]++;
                    return;
                case MaxAmmo:
                    Model.maxAmmo = PlayerModel.DEFAULT_MAX_AMMO[ItemCache[name] + 1];
                    ItemCache[name]++;
                    return;
                case RunEfficency:
                    Model.runEfficiency = ItemCache[name] == 0 ? 0.667f : .5f;
                    ItemCache[name]++;
                    return;
                case MaxEnergy:
                    Model.maxEnergy = 150 + 50 * ItemCache[name];
                    if ((double)Model.currEnergy >= Model.maxEnergy)
                    {
                        ItemCache[name]++;
                        return;
                    }

                    Model.energyRecoverAfter = Math.Min(Model.energyRecoverAfter,
                        PlayerModelPatch.WorldModel.worldTime + 300.0);
                    ItemCache[name]++;
                    return;
                case ProgJetpack:
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

                    ItemCache[name]++;
                    return;
                case ProgTreasure or ProgMarket or TrapSlime:
                    ItemCache[name]++;
                    break;
            }

            if (!firstReceive) return;
            if (name.EndsWith(NewBucksEnding))
            {
                int.TryParse(name.Substring(0, name.IndexOf(NewBucksEnding[0])), out var amount);
                PlayerStatePatch.PlayerState.AddCurrency(amount);
            }
            else
                switch (name)
                {
                    case ItemConstants.Drone:
                        AddGadget(Gadget.Id.DRONE);
                        break;
                    case AdvDrone:
                        AddGadget(Gadget.Id.DRONE_ADVANCED);
                        break;
                    case MarketLink:
                        AddGadget(Gadget.Id.MARKET_LINK);
                        break;
                }
        }
        catch (Exception e)
        {
            Core.Log.Msg(JsonConvert.SerializeObject(ItemCache, Formatting.Indented));
            Core.Log.Msg($"Error with: {name}");
            Core.Log.Error(e);
        }
    }

    public static void AddGadget(Gadget.Id id) => SRSingleton<SceneContext>.Instance.GadgetDirector?.AddGadget(id);

    public static Sprite ItemImage(AssetItem location)
    {
        var fallback = GameLoader.Spritemap[GameLoader.GetSpriteFromItemFlag(location.ItemFlags)];
        try
        {
            if (!Data.UseCustomAssets) return fallback;

            var res = Core.ItemSpritesManager.TryGetCustomAsset(location, "Slime Rancher", false, true,
                out var spriteData);

            if (!res || spriteData is null) return fallback;
            var file = spriteData.FilePath;

            if (ItemSprites.TryGetValue(file, out var sprite)) return sprite;

            ItemSprites[file] = sprite = GameLoader.CreateSprite(file);
            sprite.texture.filterMode = FilterMode.Point;
            return sprite;
        }
        catch (Exception e)
        {
            Core.Log.Error(e);
        }

        return fallback;
    }
}
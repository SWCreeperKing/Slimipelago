using Archipelago.MultiClient.Net.Models;
using MonomiPark.SlimeRancher.DataModel;
using Newtonsoft.Json;
using Slimipelago.Patches.Interactables;
using Slimipelago.Patches.PlayerPatches;
using Slimipelago.Patches.UiPatches;
using UnityEngine;
using static Slimipelago.Patches.PlayerPatches.PlayerModelPatch;
using static Slimipelago.Archipelago.ApSlimeClient;
using ILogger = KaitoKid.Utilities.Interfaces.ILogger;

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
                    {
                        ItemCache[name]++;
                        return;
                    }

                    Model.healthBurstAfter =
                        Math.Min(Model.healthBurstAfter, PlayerModelPatch.WorldModel.worldTime + 300.0);
                    ItemCache[name]++;
                    return;
                case "Progressive Max Ammo":
                    Model.maxAmmo = PlayerModel.DEFAULT_MAX_AMMO[ItemCache[name] + 1];
                    ItemCache[name]++;
                    return;
                case "Progressive Run Efficiency":
                    Model.runEfficiency = ItemCache[name] == 0 ? 0.667f : .5f;
                    ItemCache[name]++;
                    return;
                case "Progressive Max Energy":
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

                    ItemCache[name]++;
                    return;
                case "Progressive Treasure Cracker" or "Progressive Market Stonks":
                    ItemCache[name]++;
                    break;
            }

            if (!firstReceive) return;
            if (name.EndsWith("x Newbucks"))
            {
                int.TryParse(name.Substring(0, name.IndexOf('x')), out var amount);
                PlayerStatePatch.PlayerState.AddCurrency(amount);
            }
            else switch (name)
            {
                case "Drone":
                    SRSingleton<SceneContext>.Instance.GadgetDirector?.AddGadget(Gadget.Id.DRONE);
                    break;
                case "Advanced Drone":
                    SRSingleton<SceneContext>.Instance.GadgetDirector?.AddGadget(Gadget.Id.DRONE_ADVANCED);
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

    public static Sprite ItemImage(AssetItem location)
    {
        var fallback = GameLoader.Spritemap[GameLoader.GetSpriteFromItemFlag(location.ItemFlags)];
        try
        {
            if (!UseCustomAssets) return fallback;

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

public class Logger : ILogger
{
    public void LogError(string message) => Core.Log?.Error(message);
    public void LogError(string message, Exception e) => Core.Log?.Error(message, e);
    public void LogWarning(string message) => Core.Log?.Warning(message);
    public void LogInfo(string message) => Core.Log?.Msg(message);
    public void LogMessage(string message) => Core.Log?.Msg(message);
    public void LogDebug(string message) => Core.Log?.Msg(message);

    public void LogDebugPatchIsRunning(string patchedType, string patchedMethod, string patchType, string patchMethod,
        params object[] arguments)
        => Core.Log?.Msg($"Debug Patch: [{patchedMethod}] -> [{patchMethod}]");

    public void LogDebug(string message, params object[] arguments) => Core.Log?.Msg(message);
    public void LogErrorException(string prefixMessage, Exception ex, params object[] arguments) => Core.Log?.Error(ex);

    public void LogWarningException(string prefixMessage, Exception ex, params object[] arguments)
        => Core.Log?.Error(ex);

    public void LogErrorException(Exception ex, params object[] arguments) => Core.Log?.Error(ex);
    public void LogWarningException(Exception ex, params object[] arguments) => Core.Log?.Error(ex);
    public void LogErrorMessage(string message, params object[] arguments) => Core.Log?.Error(message);

    public void LogErrorException(string patchType, string patchMethod, Exception ex, params object[] arguments)
        => Core.Log?.Error(ex);
}
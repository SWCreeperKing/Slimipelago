using Archipelago.MultiClient.Net.Models;
using MonomiPark.SlimeRancher.DataModel;
using Newtonsoft.Json;
using Slimipelago.Patches.Interactables;
using Slimipelago.Patches.PlayerPatches;
using Slimipelago.Patches.UiPatches;
using static Slimipelago.Patches.PlayerPatches.PlayerModelPatch;
using static Slimipzelago.Archipelago.ApSlimeClient;

namespace Slimipelago.Archipelago;

public static class ItemHandler
{
    public static long ItemNumberTracker;

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

        ItemNumberTracker++;
        if (ItemNumberTracker <= CurrentItemIndex) return;
        var sprite = GameLoader.GetSpriteFromItemFlag(item.Flags);
        if (sprite is "trap") sprite = "got_trap";
        
        PopupPatch.AddItemToQueue(new ApPopupData(GameLoader.Spritemap[sprite], "Item Recieved", item.ItemName, $"from: {item.Player.Name}", () =>
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
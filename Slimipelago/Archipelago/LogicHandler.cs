using Slimipelago.Patches.PlayerPatches;
using UnityEngine;
using static Slimipelago.Archipelago.ItemConstants;
using static Slimipelago.Archipelago.ZoneConstants;

namespace Slimipelago.Archipelago;

public static class LogicHandler
{
    public static Dictionary<string, LogicLine> LogicLines = [];
    public static Dictionary<string, bool> LogicCache = [];

    public static void AddLogic(string line)
    {
        var split = line.Split(':');
        LogicLines[split[0]] = new LogicLine(split[1].Count(c => c is 'c'), split[1].Contains('j'),
            split[1].Count(c => c is 'e'), split[2]);
    }

    public static void LogicCheck()
    {
        var hasJetpack = JetpackPatch.EnableJetpack;
        var crackerLevel = ApSlimeClient.ItemCache.TryGetValue(ProgTreasure, out var val) ? val : 0;
        var energyLevel = ApSlimeClient.ItemCache.TryGetValue(MaxEnergy, out var val1) ? val1 : 0;
        foreach (var kv in LogicLines)
        {
            LogicCache[kv.Key] = kv.Value.UpdateLogic(crackerLevel, hasJetpack, energyLevel);
        }

        foreach (var kv in GameLoader.MarkerDictionary)
        {
            var marker = kv.Value;
            if (marker.LocationName == "") continue;

            GameLoader.ChangeMarkerColor(kv.Key, _ =>
            {
                var color = !ApSlimeClient.HintedItems.Contains(marker.LocationName)
                    ? Color.white
                    : Color.yellow;
                
                if (!ApSlimeClient.Client.MissingLocations.Contains(marker.LocationName))
                {
                    color.a = 0;
                }
                else if (Check(marker.LocationName))
                {
                    color.a = 1;
                }
                else
                {
                    color.a = .4f;
                }

                return color;
            });
        }
    }

    public static bool Check(string locationName) => !LogicCache.TryGetValue(locationName, out var val) || val;
}

public readonly struct LogicLine(int crackerLevel, bool needsJetpack, int energyLevel, string region)
{
    public bool UpdateLogic(int currentCrackerLevel, bool currentlyHasJetpack, int currentEnergyLevel)
    {
        if (needsJetpack && !currentlyHasJetpack) return false;
        return currentCrackerLevel >= crackerLevel && currentEnergyLevel >= energyLevel && HasRegion(region);
    }

    public bool HasRegion(string region)
    {
        return PlayerTrackerPatch.AllowedZones.Contains(region) && region switch
        {
            Glass => HasRegion(Ruins),
            Ruins => HasRegion(Transition) && HasRegion(Moss) && HasRegion(Quarry),
            Transition => HasRegion(Moss) || HasRegion(Quarry),
            Moss or Quarry => HasRegion(Reef),
            _ => true
        }; 
    }
}
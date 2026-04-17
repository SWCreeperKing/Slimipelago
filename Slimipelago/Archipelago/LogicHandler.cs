using Slimipelago.Patches.PlayerPatches;
using UnityEngine;
using static Slimipelago.Archipelago.ItemConstants;

namespace Slimipelago.Archipelago;

public static class LogicHandler
{
    public static Dictionary<string, List<LogicLine>> LogicLines = [];
    public static Dictionary<string, List<RegionLine>> RegionLines = [];
    public static Dictionary<string, bool> LogicCache = [];
    public static Dictionary<string, bool> RegionCache = [];
    public static Dictionary<string, string[]> Plorts = [];
    public static Dictionary<SkipLogic, bool> SkipLogic = [];
    public static Stack<string> PreviousLines = [];

    public static void AddPlort(string line)
    {
        var split = line.Split(':');
        Plorts[split[0]] = split[1].Split([';'], StringSplitOptions.RemoveEmptyEntries);
    }

    public static void AddRegion(string line)
    {
        var split = line.Split(':');
        if (!RegionLines.ContainsKey(split[0])) RegionLines[split[0]] = [];

        RegionLines[split[0]].Add(
            new RegionLine(
                split[1], split[2].Contains('j'), split[2].Count(c => c is 'e'),
                (SkipLogic)int.Parse(split[3]), split[4].Split([';'], StringSplitOptions.RemoveEmptyEntries),
                split[5].Split([';'], StringSplitOptions.RemoveEmptyEntries)
            )
        );
    }

    public static void AddLogic(string line)
    {
        var split = line.Split(':');
        if (!LogicLines.ContainsKey(split[0])) LogicLines[split[0]] = [];

        LogicLines[split[0]].Add(
            new LogicLine(
                split[1].Count(c => c is 'c'), split[1].Contains('j'),
                split[1].Count(c => c is 'e'), (SkipLogic)int.Parse(split[2]), split[3]
            )
        );
    }

    public static void LogicCheck()
    {
        try
        {
            RegionCache.Clear();
            var hasJetpack = ApSlimeClient.EnableJetpack;
            var crackerLevel = ApSlimeClient.ItemCache.TryGetValue(ProgTreasure, out var val) ? val : 0;
            var energyLevel = ApSlimeClient.ItemCache.TryGetValue(MaxEnergy, out var val1) ? val1 : 0;
            foreach (var line in RegionLines.Keys)
            {
                PreviousLines.Clear();
                CheckRegion(line, hasJetpack, energyLevel);
            }

            foreach (var kv in LogicLines)
            {
                RegionCache.Clear();
                try
                {
                    LogicCache[kv.Key] = kv.Value.Any(logic => logic.UpdateLogic(
                            crackerLevel, hasJetpack, energyLevel
                        )
                    );
                }
                catch (KeyNotFoundException) { Core.Log.Error($"Failed key: [{kv.Key}]"); }
                catch (Exception e) { Core.Log.Error($"Failed key: [{kv.Key}]", e); }
            }

            foreach (var kv in GameLoader.MarkerDictionary)
            {
                var marker = kv.Value;
                if (marker.LocationName == "") continue;

                GameLoader.ChangeMarkerColor(
                    kv.Key, _ =>
                    {
                        if (marker.LocationName == "Null") return Color.red;

                        var isNote = marker.IsNote;
                        var isLocGotten = !ApSlimeClient.Client.MissingLocations.Contains(marker.LocationName);

                        var color = !ApSlimeClient.HintedItems.Contains(marker.LocationName)
                            ? Color.white
                            : Color.yellow;

                        if (isLocGotten && isNote && !ApSlimeClient.NoteLocations[kv.Key]) color = Color.magenta;
                        else if (isLocGotten) color.a = 0;
                        else if (Check(marker.LocationName)) color.a = 1;
                        else color.a = .4f;

                        return color;
                    }
                );
            }
        }
        catch (Exception e) { Core.Log.Error(e); }
    }

    public static bool Check(string locationName) => !LogicCache.TryGetValue(locationName, out var val) || val;

    public static bool CheckRegion(string region, bool currentlyHasJetpack, int currentEnergyLevel)
    {
        if (RegionCache.TryGetValue(region, out var checkRegion)) return checkRegion;
        if (!RegionLines.ContainsKey(region)) return RegionCache[region] = true;
        return RegionCache[region] = RegionLines[region]
           .Any(line => line.UpdateLogic(currentlyHasJetpack, currentEnergyLevel));
    }
}

public readonly struct LogicLine(int crackerLevel, bool needsJetpack, int energyLevel, SkipLogic skipLogic,
    string region)
{
    public bool UpdateLogic(int currentCrackerLevel, bool currentlyHasJetpack, int currentEnergyLevel)
    {
        if (!skipLogic.HasSkip()) return false;
        if (needsJetpack && !currentlyHasJetpack) return false;
        return currentCrackerLevel >= crackerLevel && currentEnergyLevel >= energyLevel
                                                   && HasRegion(region, currentlyHasJetpack, currentEnergyLevel);
    }

    public bool HasRegion(string _region, bool currentlyHasJetpack, int currentEnergyLevel)
        => LogicHandler.CheckRegion(_region, currentlyHasJetpack, currentEnergyLevel);
}

public readonly struct RegionLine(string from, bool needsJetpack, int energyLevel, SkipLogic skipLogic,
    string[] plorts, string[] unlocks)
{
    public readonly string Id
        = $"[{from}];[{needsJetpack}];[{energyLevel}];[{skipLogic}];[{string.Join(", ", plorts)}];[{string.Join(",", unlocks)}]";

    public bool UpdateLogic(bool currentlyHasJetpack, int currentEnergyLevel)
    {
        if (LogicHandler.PreviousLines.Contains(Id)) return false;
        LogicHandler.PreviousLines.Push(Id);
        if (!skipLogic.HasSkip()) return false;
        if (needsJetpack && !currentlyHasJetpack) return false;
        if (currentEnergyLevel < energyLevel) return false;
        return HasUnlocks() && CanReach(currentlyHasJetpack, currentEnergyLevel);
    }

    public bool CanReach(bool jet, int energy)
        => LogicHandler.CheckRegion(from, jet, energy)
           && (plorts.Length == 0
               || plorts.All(plort => LogicHandler.Plorts[plort].Any(reg => LogicHandler.CheckRegion(reg, jet, energy))
               ));

    public bool HasUnlocks() => unlocks.Length == 0 || unlocks.All(PlayerTrackerPatch.AllowedZones.Contains);
}

[Flags]
public enum SkipLogic
{
    None = 0, EasySkips = 1, PreciseMovement = 1 << 1,
    ObscureLocations = 1 << 2, JetpackBoosts = 1 << 3, LargoJumps = 1 << 4,
    DangerousSkips = 1 << 5
}

public static class SkipLogicHelper
{
    public static bool HasSkip(this SkipLogic logic)
    {
        if (logic is SkipLogic.None) return true;
        List<bool> rules = [];

        if (logic.HasFlag(SkipLogic.EasySkips)) rules.Add(LogicHandler.SkipLogic[SkipLogic.EasySkips]);
        if (logic.HasFlag(SkipLogic.PreciseMovement)) rules.Add(LogicHandler.SkipLogic[SkipLogic.PreciseMovement]);
        if (logic.HasFlag(SkipLogic.ObscureLocations)) rules.Add(LogicHandler.SkipLogic[SkipLogic.ObscureLocations]);
        if (logic.HasFlag(SkipLogic.JetpackBoosts)) rules.Add(LogicHandler.SkipLogic[SkipLogic.JetpackBoosts]);
        if (logic.HasFlag(SkipLogic.LargoJumps)) rules.Add(LogicHandler.SkipLogic[SkipLogic.LargoJumps]);
        if (logic.HasFlag(SkipLogic.DangerousSkips)) rules.Add(LogicHandler.SkipLogic[SkipLogic.DangerousSkips]);

        return rules.All(b => b);
    }
}
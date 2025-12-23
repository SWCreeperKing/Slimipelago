using System.Collections.Concurrent;
using System.Reflection;
using JetBrains.Annotations;
using Slimipelago.Added;
using Slimipelago.Patches.Interactables;
using Slimipelago.Patches.PlayerPatches;
using Slimipelago.Patches.UiPatches;
using UnityEngine;
using static Slimipelago.GameLoader;

namespace Slimipelago.Archipelago;

public static class TrapLoader
{
    public enum Trap
    {
        Whoops, // Whoops!
        Ranch, // send him to detroit
        Text, // hobson note
        Tarr, // spawn tarr
        MarketCrash, // crash the market
        Zoom, // Zoom trap
        ScreenFlip, // Flip camera 
        Smol, // make smol
        Damage, // EMOTIONAL DAMAGE 
        Radiation, // i don't feel so good mr hobson
        EnergyDrain, // bye bye energy
        Freeze, // FREEZE!
        NoVac, // that must *SUCC*
        Wide, // https://www.youtube.com/watch?v=Wl959QnD3lM
        FrameSlime, // https://imgur.com/a/dA0afHO
        Underwater, // https://www.youtube.com/shorts/-enuIBVmKy4
    }

    public static List<string> BaseTrapNames = [];
    public static Dictionary<Trap, string> TrapTypeToName = [];
    public static Dictionary<string, Func<string, bool>> Traps = [];
    public static List<TrapAttribute> TrapAttributes = [];
    public static ConcurrentQueue<TrapLinkTrap> TrapLinkTraps = [];
    public static long TrapSlimesUsedCount;

    private static Action TrapReset = null;
    private static float ResetTimer = 0;
    private static float TrapTimer = 0;

    public static void Init()
    {
        var asm = Assembly.GetExecutingAssembly();
        var types = asm.GetTypes();
        var hints = types.Where(t => t.GetCustomAttributes<TrapHintAttribute>().Any()).ToArray();

        foreach (var hint in hints)
        {
            var methods = hint.GetMethods()
                              .Where(m => m.GetCustomAttributes<TrapAttribute>().Any())
                              .Select(m => (method: m, trap: m.GetCustomAttribute<TrapAttribute>()))
                              .ToArray();

            foreach (var method in methods)
            {
                Func<string, bool> action = s => (bool)method.method.Invoke(null, [s]);
                TrapAttributes.Add(method.trap);
                TrapTypeToName[method.trap.Trap] = method.trap.TrapNames[0];
                BaseTrapNames.Add(method.trap.TrapNames[0]);

                foreach (var trapName in method.trap.TrapNames)
                {
                    Traps[trapName] = action;
                }
            }
        }

        if (!Directory.Exists("mod dev/Slimipelago")) return;
        if (File.Exists("mod dev/Slimipelago/Traps.md")) return;

        File.WriteAllText("mod dev/Slimipelago/Traps.md",
            $"""
             | Trap | Description | Acceptable from TrapLink |
             |:-----|:-----------:|:------------------------|
             {string.Join("\n", TrapAttributes.OrderBy(att => att.TrapNames[0]).Select(att => $"|{string.Join("|", att.TrapNames[0], att.Description, string.Join("<br>", att.TrapNames.OrderBy(s => s)))}|"))}

             | TrapLink Trap | Gets Converted to |
             |:--------------|:-----------------|
             {string.Join("\n", TrapAttributes.SelectMany(att => att.TrapNames, (attribute, s) => $"|{s}|{attribute.TrapNames[0]}|").OrderBy(s => s))}
             """);
    }

    public static void Update()
    {
        if (!PlayerStatePatch.FirstUpdate) return;
        if (SRSingleton<SceneContext>.Instance.TimeDirector.HasPauser()) return;

        if (TrapTimer > 0 && TrapReset is null && (TrapLinkTraps.Any() || GetTrapAmount() > TrapSlimesUsedCount))
        {
            TrapTimer -= Time.deltaTime;
        }
        else if (TrapReset is null && TrapLinkTraps.TryPeek(out var traplink))
        {
            if (RunTrap(traplink.Trap, traplink.Player))
            {
                TrapLinkTraps.TryDequeue(out _);
            }
            else
            {
                TrapTimer += 3;
            }
        }
        else if (TrapTimer <= 0 && TrapReset is null && GetTrapAmount() > TrapSlimesUsedCount)
        {
            var trap = TrapAttributes[Playground.Random.Next(TrapAttributes.Count)];
            if (RunTrap(trap.Trap))
            {
                TrapTimer = Playground.Random.Next(30, 60);
                ApSlimeClient.Client.SendToStorage("used_traps", ++TrapSlimesUsedCount);
            }
            else
            {
                TrapTimer += 3;
            }
        }

        if (TrapReset is null) return;
        if (ResetTimer <= 0)
        {
            TrapReset();
            TrapReset = null;
        }

        ResetTimer -= Time.deltaTime;
    }

    public static void LoadTrapData()
    {
        TrapTimer = 60;
        TrapReset = null;
        TrapLinkTraps = [];
        TrapSlimesUsedCount = ApSlimeClient.Client.GetFromStorage("used_traps", def: 0L);
        Playground.WasBanished = false;
        MarketPatch.Crash = false;
    }

    public static bool RunTrap(Trap trap) => RunTrap(TrapTypeToName[trap]);

    public static bool RunTrap(string trap, [CanBeNull] string player = null)
    {
        if (!Traps.ContainsKey(trap)) return false;
        if (TrapReset is not null) return false;
        if (player is not null)
        {
            PopupPatch.AddItemToQueue(new ApPopupData(Spritemap["got_trap"], "Trapped",
                $"{trap} Trap", $"From [{player}]"));
        }
        else if (ApSlimeClient.Data.TrapLink)
        {
            ApSlimeClient.Client.SendTrapLink($"{trap} Trap");
        }

        return Traps[trap](trap);
    }

    public static bool SetTrapReSetter(Action trapReset, float resetTime)
    {
        if (TrapReset is not null) return false;
        TrapReset = trapReset;
        ResetTimer = resetTime;
        return true;
    }

    public static bool HasTrapReset() => TrapReset is not null;

    public static int GetTrapAmount()
        => ApSlimeClient.ItemCache.TryGetValue(ItemConstants.TrapSlime, out var traps) ? traps : 0;
}

[AttributeUsage(AttributeTargets.Class)]
public class TrapHintAttribute : Attribute;

[AttributeUsage(AttributeTargets.Method)]
public class TrapAttribute(TrapLoader.Trap trap, string[] trapName, string description) : Attribute
{
    public readonly TrapLoader.Trap Trap = trap;
    public readonly string[] TrapNames = trapName;
    public readonly string Description = description;
}

public readonly struct TrapLinkTrap(string trap, string player)
{
    public readonly string Trap = trap;
    public readonly string Player = player;
}
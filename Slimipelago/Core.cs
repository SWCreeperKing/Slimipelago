using System.Reflection;
using KaitoKid.ArchipelagoUtilities.AssetDownloader.ItemSprites;
using MelonLoader;
using Newtonsoft.Json;
using Slimipelago.Archipelago;
using Slimipelago.Patches.PlayerPatches;
using Slimipelago.Patches.UiPatches;
using UnityEngine;
using static Slimipelago.GameLoader;
using Logger = Slimipelago.Archipelago.Logger;

[assembly: MelonInfo(typeof(Slimipelago.Core), "Slimipelago", "1.0.0", "SW_CreeperKing", null)]
[assembly: MelonGame("Monomi Park", "Slime Rancher")]

namespace Slimipelago;

public class Core : MelonMod
{
    public static int DebugLevel;
    public static MelonLogger.Instance Log;

    private static Logger Logger;
    public static ArchipelagoItemSprites ItemSpritesManager;

    public override void OnInitializeMelon()
    {
        if (File.Exists("debug.txt"))
        {
            DebugLevel = int.TryParse(File.ReadAllText("debug.txt"), out var debugLvl) ? debugLvl : 0;
        }

        Log = LoggerInstance;
        Logger = new Logger();
        ItemSpritesManager = new ArchipelagoItemSprites(Logger, JsonConvert.DeserializeObject<ItemSpriteAliases>);

        ApWorldShenanigans.RunShenanigans();
        var locationFileData = File
                              .ReadAllText("Mods/SW_CreeperKing.Slimipelago/Data/Locations.txt")
                              .Replace("\r", "")
                              .Split('\n')
                              .Select(s => s.Split(','))
                              .ToArray();

        foreach (var data in locationFileData)
        {
            if (ApSlimeClient.LocationDictionary.ContainsKey(data[0]))
            {
                Log.Msg($"Duplicate Key: {data[0]}");
                continue;
            }

            ApSlimeClient.LocationDictionary[data[0]] = data[1];
            if (data.Length < 3) continue;
            ApSlimeClient.LocationInfoDictionary[data[0]] = data[2];
        }

        ApSlimeClient.UpgradeLocations = File
                                        .ReadAllText("Mods/SW_CreeperKing.Slimipelago/Data/Upgrades.txt")
                                        .Replace("\r", "")
                                        .Split('\n')
                                        .Select(s => s.Split(','))
                                        .ToDictionary(sArr => (PlayerState.Upgrade)int.Parse(sArr[1]), sArr => sArr[0]);

        ApSlimeClient.CorporateLocations = File
                                          .ReadAllText("Mods/SW_CreeperKing.Slimipelago/Data/7Zee.txt")
                                          .Replace("\r", "")
                                          .Split('\n')
                                          .Where(line => line.Trim() != "")
                                          .Select(s =>
                                           {
                                               var split = s.Split(',');
                                               return (int.Parse(split[1]), split[0]);
                                           })
                                          .GroupBy(t => t.Item1)
                                          .ToDictionary(g => g.Key, g => g.Select(t => t.Item2).ToArray());

        foreach (var line in File.ReadAllText("Mods/SW_CreeperKing.Slimipelago/Data/Logic.txt").Split('\n'))
        {
            LogicHandler.AddLogic(line);
        }

        ApSlimeClient.Init();

        Log.Msg("Shenanigans finished");

        LoadSprites();

        Log.Msg("Assets Loaded");

        var classesToPatch = Assembly.GetAssembly(typeof(Core))
                                     .GetTypes()
                                     .Where(t => t.GetCustomAttributes<PatchAll>().Any())
                                     .ToArray();

        Log.Msg($"Loading [{classesToPatch.Length}] Class patches");

        foreach (var patch in classesToPatch)
        {
            HarmonyInstance.PatchAll(patch);

            Log.Msg($"Loaded: [{patch.Name}]");
        }

        Log.Msg("Loading Songs");

        MusicPatch.LoadSongs();

        Log.Msg("Initialized.");

        // IntroUI
        // ExchangeFullUI
        KeyRegistry.AddKey(KeyCode.P, () => Log.Msg(PlayerStatePatch.PlayerInWorld.transform.position));
        KeyRegistry.AddKey(KeyCode.Backspace,
            () => SRSingleton<SceneContext>.Instance.GadgetDirector?.AddGadget(Gadget.Id.DRONE_ADVANCED));
    }

    public override void OnUpdate()
    {
        KeyRegistry.Update();
        PopupPatch.UpdateQueue();
        ApSlimeClient.Update();
    }
}
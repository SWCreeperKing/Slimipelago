using System.Reflection;
using CreepyUtil.Archipelago;
using KaitoKid.ArchipelagoUtilities.AssetDownloader.ItemSprites;
using MelonLoader;
using MonomiPark.SlimeRancher.Services;
using Newtonsoft.Json;
using Slimipelago;
using Slimipelago.Archipelago;
using Slimipelago.Patches.UiPatches;
using static Slimipelago.GameLoader;
using Logger = Slimipelago.Archipelago.Logger;

[assembly: MelonInfo(typeof(Core), "Slimipelago", Core.VersionNumber, "SW_CreeperKing", null)]
[assembly: MelonGame("Monomi Park", "Slime Rancher")]

namespace Slimipelago;

public class Core : MelonMod
{
    public const string VersionNumber = "0.2.3";    
    public const string DataFolder = "Mods/SW_CreeperKing.Slimipelago/Data";
    
    public static int DebugLevel;
    public static MelonLogger.Instance Log;

    public static ArchipelagoItemSprites ItemSpritesManager;
    private static Logger Logger;

    public override void OnInitializeMelon()
    {
        if (File.Exists("debug.txt"))
        {
            DebugLevel = int.TryParse(File.ReadAllText("debug.txt"), out var debugLvl) ? debugLvl : 0;
        }

        Log = LoggerInstance;
        Logger = new Logger();
        ItemSpritesManager = new ArchipelagoItemSprites(Logger, JsonConvert.DeserializeObject<ItemSpriteAliases>);

        Log.Msg("Starting Shenanigans");

        var locationFileData = File
                              .ReadAllLines($"{DataFolder}/Locations.txt")
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

        Log.Msg("Loading Shenanigans");

        ApSlimeClient.UpgradeLocations = File
                                        .ReadAllLines($"{DataFolder}/Upgrades.txt")
                                        .Select(s => s.Split(','))
                                        .ToDictionary(sArr => (PlayerState.Upgrade)int.Parse(sArr[1]), sArr => sArr[0]);

        ApSlimeClient.CorporateLocations = File
                                          .ReadAllLines($"{DataFolder}/7Zee.txt")
                                          .Where(line => line.Trim() != "")
                                          .Select(s =>
                                           {
                                               var split = s.Split(',');
                                               return (int.Parse(split[1]), split[0]);
                                           })
                                          .GroupBy(t => t.Item1)
                                          .ToDictionary(g => g.Key, g => g.Select(t => t.Item2).ToArray());

        foreach (var line in File.ReadAllLines($"{DataFolder}/Logic.txt"))
        {
            LogicHandler.AddLogic(line);
        }

        ApSlimeClient.NoteLocations = new LoseFlag<string>("None");
        ApSlimeClient.NoteLocations.AddFlags(File.ReadAllLines($"{DataFolder}/NoteLocations.txt"));

        Log.Msg("Trapping Shenanigans");

        TrapLoader.Init();

        Log.Msg("Finalizing Shenanigans");

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
    }

    public override void OnUpdate()
    {
        KeyRegistry.Update();
        ApSlimeClient.Update();
        TrapLoader.Update();
    }
}
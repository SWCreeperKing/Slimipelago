using System.Reflection;
using CreepyUtil.Archipelago;
using KaitoKid.ArchipelagoUtilities.AssetDownloader.ItemSprites;
using MelonLoader;
using Newtonsoft.Json;
using Slimipelago;
using Slimipelago.Archipelago;
using Slimipelago.Patches.PlayerPatches;
using Slimipelago.Patches.UiPatches;
using UnityEngine;
using static Slimipelago.GameLoader;
using Logger = Slimipelago.Archipelago.Logger;

[assembly: MelonInfo(typeof(Core), "Slimipelago", Core.VersionNumber, "SW_CreeperKing", null)]
[assembly: MelonGame("Monomi Park", "Slime Rancher")]

namespace Slimipelago;

public class Core : MelonMod
{
    public const string VersionNumber = "0.3.0";
    public const string DataFolder = "Mods/SW_CreeperKing.Slimipelago/Data";

    public static int DebugLevel;
    public static MelonLogger.Instance Log;

    public static ArchipelagoItemSprites ItemSpritesManager;
    private static Logger Logger;

    public override void OnInitializeMelon()
    {
        // AchievementsDirector // for achievements
        // ProgressDirector // for progression
        
        Log = LoggerInstance;
        if (File.Exists("debug.txt"))
        {
            DebugLevel = int.TryParse(File.ReadAllText("debug.txt"), out var debugLvl) ? debugLvl : 0;
        }

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

        PlayerTrackerPatch.ZoneTypeToName = File.ReadAllLines($"{DataFolder}/Zones.txt")
                                                .Select(s => s.Split(':'))
                                                .Where(arr => arr[0] is not "-")
                                                .ToDictionary(
                                                     arr => (ZoneDirector.Zone)int.Parse(arr[0]), arr => arr[1]
                                                 );

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
                                               }
                                           )
                                          .GroupBy(t => t.Item1)
                                          .ToDictionary(g => g.Key, g => g.Select(t => t.Item2).ToArray());

        ApSlimeClient.GateLocks = File.ReadAllLines($"{DataFolder}/Gates.txt")
                                      .Select(s => s.Split(';')).ToDictionary(arr => arr[0], arr => arr[1]);

        foreach (var line in File.ReadAllLines($"{DataFolder}/Logic.txt")) LogicHandler.AddLogic(line);
        Log.Msg("Main Logic Loaded");
        foreach (var line in File.ReadAllLines($"{DataFolder}/RegionLogic.txt")) LogicHandler.AddRegion(line);
        Log.Msg("Region Logic Loaded");
        foreach (var line in File.ReadAllLines($"{DataFolder}/PlortLogic.txt")) LogicHandler.AddPlort(line);
        Log.Msg("Plort Logic Loaded");

        ApSlimeClient.NoteLocations = new LoseFlag<string>("None");
        ApSlimeClient.NoteLocations.AddFlags(File.ReadAllLines($"{DataFolder}/NoteLocations.txt"));
        ApSlimeClient.NoteCount = Convert.ToString((long)ApSlimeClient.NoteLocations.MaxFlag, 2).Count(c => c is '1');

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

        if (DebugLevel <= 0) return;
        KeyRegistry.AddKey(
            KeyCode.J,
            () => Log.Msg(TrapLoader.RunRandomTrap() ? "Ran a random trap" : "Failed to run random trap")
        );
        
        KeyRegistry.AddKey(KeyCode.Backslash, () =>
        {
            var system = typeof(SECTR_AudioSystem).GetPrivateStaticField<SECTR_AudioSystem>("system");
            if (system is null)
            {
                Log.Msg("Audio System not init");
                return;
            }
            
            var globalInstances = typeof(SECTR_AudioSystem).GetPrivateStaticField<object>("activeInstances");
            var globalInstanceCount = globalInstances.CallPublicProperty<int>("Count");
            
            Log.Msg($"Global Instances Types: [{globalInstanceCount}], max: [{system.MaxInstances}]");
        });
    }

    public override void OnUpdate()
    {
        KeyRegistry.Update();
        ApSlimeClient.Update();
    }
}
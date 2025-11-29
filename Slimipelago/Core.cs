using System.Reflection;
using MelonLoader;
using Slimipelago.Patches.UiPatches;
using static Slimipelago.GameLoader;

[assembly: MelonInfo(typeof(Slimipelago.Core), "Slimipelago", "1.0.0", "SW_CreeperKing", null)]
[assembly: MelonGame("Monomi Park", "Slime Rancher")]

namespace Slimipelago;

public class Core : MelonMod
{
    public static MelonLogger.Instance Log;

    public override void OnInitializeMelon()
    {
        Log = LoggerInstance;

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

        Log.Msg("Initialized.");
    }

    public override void OnUpdate()
    {
        KeyRegistry.Update();
        PopupPatch.UpdateQueue();
        ApSlimeClient.Update();
    }
}
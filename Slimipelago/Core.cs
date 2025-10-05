using System.Reflection;
using InControl;
using MelonLoader;
using MonomiPark.SlimeRancher.Regions;
using Slimipelago.Patches;
using UnityEngine;
using UnityEngine.UI;
using static Identifiable;
using static Slimipelago.Patches.JetpackPatch;
using static Slimipelago.Patches.PlayerModelPatch;
using static Slimipelago.Patches.PlayerStatePatch;
using Object = UnityEngine.Object;

[assembly: MelonInfo(typeof(Slimipelago.Core), "Slimipelago", "1.0.0", "SW_CreeperKing", null)]
[assembly: MelonGame("Monomi Park", "Slime Rancher")]

// icon: Redriel
namespace Slimipelago;

public class Core : MelonMod
{
    public static Dictionary<string, Sprite> Spritemap = [];

    public static Dictionary<Id, string> MarketItems = new()
    {
        [Id.PINK_PLORT] = "normal",
        [Id.RAD_PLORT] = "trap",
        [Id.ROCK_PLORT] = "progressive"
    };

    public static MelonLogger.Instance Log;
    public static bool TestNotItemBought = true;
    public static Task TrapReset;

    public override void OnInitializeMelon()
    {
        Log = LoggerInstance;
        CreateSprite("normal", "APSR");
        CreateSprite("trap", "APSR_Trap");
        CreateSprite("progressive", "APSR_Progressive");
        CreateSprite("useful", "APSR_Useful");
        CreateSprite("got_trap", "APSR_Got_Trap");

        Log.Msg("Assets Loaded");

        PersonalUpgradePatch.TestPurchasables.Add(PersonalUpgradePatch.CreatePurchasable("Test Item",
            Spritemap["normal"], "This is just a simple test", 1, () => { }, () => true, () => TestNotItemBought));

        PersonalUpgradePatch.TestPurchasables.Add(PersonalUpgradePatch.CreatePurchasable("Test Trap Item",
            Spritemap["trap"], "This is just a simple test", 1, () => { }, () => true, () => TestNotItemBought));

        PersonalUpgradePatch.TestPurchasables.Add(PersonalUpgradePatch.CreatePurchasable("Test Progressive Item",
            Spritemap["progressive"], "This is just a simple test", 1, () => { }, () => true, () => TestNotItemBought));

        var classesToPatch = Assembly.GetAssembly(typeof(Core))
                                     .GetTypes()
                                     .Where(t => t.GetCustomAttributes<HarmonyPatchAll>().Any())
                                     .ToArray();

        Log.Msg($"Loading [{classesToPatch.Length}] Class patches");

        foreach (var patch in classesToPatch)
        {
            HarmonyInstance.PatchAll(patch);
            Log.Msg($"Loaded: [{patch.Name}]");
        }

        LoadDebugKeys();

        Log.Msg("Initialized.");
    }

    public void CreateSprite(string key, string file, string fileType = "png")
    {
        var path = $"Mods/SW_CreeperKing.Slimipelago/Assets/Images/{file}.{fileType}";
        Texture2D texture = new(2, 2);
        if (!texture.LoadImage(File.ReadAllBytes(path)))
            throw new ArgumentException($"Error sprite not created: [{file}]");
        Spritemap[key] = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f));
    }

    public void LoadDebugKeys()
    {
        Log.Msg("Loading Debug Keys");
        KeyRegistry.AddKey(KeyCode.J, () =>
        {
            if (Model is null || Jetpack is null) return;

            if (Model.upgrades.Contains(PlayerState.Upgrade.JETPACK))
            {
                Model.upgrades.Remove(PlayerState.Upgrade.JETPACK);
            }
            else
            {
                Model.upgrades.Add(PlayerState.Upgrade.JETPACK);
            }

            Model.hasJetpack = !Model.hasJetpack;
            Log.Msg("Toggled Jetpack");
        });

        var i = 0;
        string[] arr = ["normal", "trap", "progressive", "useful"];
        string[] arr1 = ["Item Received", "Item Sent"];
        KeyRegistry.AddKey(KeyCode.K, () =>
        {
            try
            {
                var sprite = Spritemap[arr[i % arr.Length]];
                PopupPatch.AddItemToQueue(new ApPopupData(sprite, arr1[i % arr1.Length], "[Insert random item here]",
                    "[Insert other player here]"));
                i++;
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        });

        KeyRegistry.AddKey(KeyCode.L, () => Log.Msg($"Toggle Recovery to [{EnableRecovery = !EnableRecovery}]"));

        KeyRegistry.AddKey(KeyCode.Semicolon, () => MakeMarker("normal"));
        KeyRegistry.AddKey(KeyCode.Quote, () => MakeMarker("trap"));
        KeyRegistry.AddKey(KeyCode.F9, BanishPlayer);
        KeyRegistry.AddKey(KeyCode.F8,
            () =>
            {
                PopupPatch.AddItemToQueue(new ApPopupData(Spritemap["got_trap"], "WOOPS!", "WOOPS!", "From debug keys",
                    Woops));
            });
    }

    public override void OnUpdate()
    {
        KeyRegistry.Update();
        PopupPatch.UpdateQueue();
    }

    public static void MakeMarker(string id)
    {
        GameObject gobj = new($"Archipelago Marker Display ({id})")
        {
            transform =
            {
                parent = PlayerInWorld.GetComponent<PlayerDisplayOnMap>().transform.parent,
            }
        };
        gobj.AddComponent<RegionMember>();

        var obj = gobj.AddComponent<ItemDisplayOnMap>();
        obj.Image.overrideSprite = Spritemap[id];
    }

    public static void Woops()
    {
        if (TrapReset is not null) return;
        try
        {
            var beforePos = PlayerInWorld.transform.position;
            var pos = PlayerInWorld.transform.position;
            pos.y += 300;
            PlayerInWorld.transform.position = pos;
            TrapReset = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(5000);
                    PlayerInWorld.transform.position = beforePos;
                    TrapReset = null;
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            });
        }
        catch (Exception e)
        {
            Log.Error(e);
        }
    }

    public static void BanishPlayer()
        => PlayerInWorld.transform.SetPositionAndRotation(new Vector3(90.9811f, 20, -140.8849f),
            Quaternion.Euler(0, 19.227f, 0));
}
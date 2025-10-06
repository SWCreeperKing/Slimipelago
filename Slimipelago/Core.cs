﻿using System.Reflection;
using MelonLoader;
using Slimipelago.Patches;
using UnityEngine;
using static Identifiable;
using static Slimipelago.GameLoader;
using static Slimipelago.Patches.JetpackPatch;
using static Slimipelago.Patches.PlayerModelPatch;
using static Slimipelago.Patches.PlayerStatePatch;

[assembly: MelonInfo(typeof(Slimipelago.Core), "Slimipelago", "1.0.0", "SW_CreeperKing", null)]
[assembly: MelonGame("Monomi Park", "Slime Rancher")]

namespace Slimipelago;

public class Core : MelonMod
{

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
        LoadSprites();

        Log.Msg("Assets Loaded");

        PersonalUpgradePatch.TestPurchasables.Add(PersonalUpgradePatch.CreatePurchasable("Test Item",
            Spritemap["normal"], "This is just a simple test", 1, () => { }, () => true, () => TestNotItemBought));

        PersonalUpgradePatch.TestPurchasables.Add(PersonalUpgradePatch.CreatePurchasable("Test Trap Item",
            Spritemap["trap"], "This is just a simple test", 1, () => { }, () => true, () => TestNotItemBought));

        PersonalUpgradePatch.TestPurchasables.Add(PersonalUpgradePatch.CreatePurchasable("Test Progressive Item",
            Spritemap["progressive"], "This is just a simple test", 1, () => { }, () => true, () => TestNotItemBought));

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

        LoadDebugKeys();

        Log.Msg("Initialized.");
    }

    public void LoadDebugKeys()
    {
        Log.Msg("Loading Debug Keys");
        KeyRegistry.AddKey(KeyCode.J, () =>
        {
            if (Model is null || Jetpack is null) return;
            //
            // if (Model.upgrades.Contains(PlayerState.Upgrade.JETPACK))
            // {
            //     Model.upgrades.Remove(PlayerState.Upgrade.JETPACK);
            // }
            // else
            // {
            //     Model.upgrades.Add(PlayerState.Upgrade.JETPACK);
            // }

            // Model.hasJetpack = !Model.hasJetpack;
            EnableJetpack = !EnableJetpack;
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

        KeyRegistry.AddKey(KeyCode.Semicolon, () => MakeMarker("normal", PlayerPos));
        KeyRegistry.AddKey(KeyCode.Quote, () => MakeMarker("trap", PlayerPos));
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

    public static void BanishPlayer() => TeleportPlayer(Home);
}
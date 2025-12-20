using MonomiPark.SlimeRancher.Regions;
using Slimipelago.Archipelago;
using Slimipelago.Patches.Interactables;
using Slimipelago.Patches.PlayerPatches;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace Slimipelago.Added;

[TrapHint]
public static class Playground
{
    public static Random Random = new Random();
    public static Task TrapReset;

    public const string DefMessage =
        "According to all known laws of programming, there is no way an archipelago should be able to bk, the archipelago, of course, bks anyway";

    [Trap(TrapLoader.Trap.Whoops, ["Whoops!", "Banana", "Banana Peel", "Eject Ability", "Spring", "Slip"],
        "Teleports the player into the sky")]
    public static void Whoops()
    {
        if (TrapReset is not null) return;
        try
        {
            var beforePos = PlayerStatePatch.PlayerInWorld.transform.position;
            var pos = PlayerStatePatch.PlayerInWorld.transform.position;
            pos.y += 300;
            PlayerStatePatch.PlayerInWorld.transform.position = pos;
            TrapReset = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(5000);
                    PlayerStatePatch.PlayerInWorld.transform.position = beforePos;
                    TrapReset = null;
                }
                catch (Exception e)
                {
                    Core.Log.Error(e);
                }
            });
        }
        catch (Exception e)
        {
            Core.Log.Error(e);
        }
    }

    [Trap(TrapLoader.Trap.Text, ["Text", "Phone", "Aaa", "Tip", "Omo", "Spam"], "Puts text on the screen")]
    public static void Note()
    {
        if (!Directory.Exists("Mods/SW_CreeperKing.Slimipelago/TextTrap"))
        {
            Directory.CreateDirectory("Mods/SW_CreeperKing.Slimipelago/TextTrap");
        }

        var files = Directory.GetFiles("Mods/SW_CreeperKing.Slimipelago/TextTrap");
        
        var gameObject = Object.Instantiate(JournalPatch.NotePrefab);
        gameObject.GetComponent<JournalUI>().journalText.text = files.Length == 0? DefMessage : File.ReadAllText(files[Random.Next(files.Length)]);       
    }

    [Trap(TrapLoader.Trap.Home, ["Home", "Get Out"], "Teleports the player home")]
    public static void BanishPlayer()
        => PlayerStatePatch.TeleportPlayer(GameLoader.Home, RegionRegistry.RegionSetId.HOME);

    [Trap(TrapLoader.Trap.Tarr, ["Tarr", "Bomb", "Bee", "Animal", "Bonk", "Fear", "Nut", "Pie", "Police", "Poison"], "Spawns a Tarr on the player")]
    public static void SpawnTarr()
    {
        var pos = PlayerStatePatch.PlayerInWorld.transform.position + new Vector3(0, 10, 0);
        var prefab = SRSingleton<GameContext>.Instance.LookupDirector.GetPrefab(Identifiable.Id.TARR_SLIME);
        var spawned =
            SRBehaviour.InstantiateActor(prefab, PlayerModelPatch.Model.currRegionSetId, pos, Quaternion.identity);

        foreach (var listener in spawned.GetComponentsInChildren<SpawnListener>(true))
        {
            listener.DidSpawn();
        }
    }
}
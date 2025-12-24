using DG.Tweening;
using MonomiPark.SlimeRancher.Regions;
using Slimipelago.Archipelago;
using Slimipelago.Patches.Interactables;
using Slimipelago.Patches.PlayerPatches;
using UnityEngine;
using static Slimipelago.Patches.PlayerPatches.PlayerStatePatch;
using static Slimipelago.Patches.PlayerPatches.VacuumPatch;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace Slimipelago.Added;

[TrapHint]
public static class Playground
{
    public static readonly Matrix4x4 CameraFlipMatrix4X4 = Matrix4x4.Scale(new Vector3(-1, 1, 1));
    public static readonly Random Random = new();
    public static readonly Vector3 NormalScale = new(1f, 1f, 1f);
    public static readonly Vector3 SmolScale = new(.1f, .1f, .1f);
    public static readonly Vector3 WideScale = new(3f, .1f, .1f);

    public static GameObject NoteObj;
    public static int Fps;
    public static int VSync;
    public static bool WasBanished;

    public const string DefMessage =
        "According to all known laws of programming, there is no way an archipelago should be able to bk, the archipelago, of course, bks anyway";

    [Trap(TrapLoader.Trap.Whoops,
        [
            "Whoops!", "Ice Floor", "Icy Hot Pants", "Push", "Gravity", "Banana", "Jump", "Banana Peel",
            "Eject Ability", "Spring", "Slip", "Hiccup"
        ],
        "Teleports the player into the sky")]
    public static bool Whoops(string trapName)
    {
        var beforePos = PlayerInWorld.transform.position;

        return ActivateTrapWithReset(() =>
        {
            WasBanished = false;
            var pos = PlayerInWorld.transform.position;
            pos.y += 300;
            PlayerInWorld.transform.position = pos;
        }, () =>
        {
            if (WasBanished) return;
            PlayerInWorld.transform.position = beforePos;
        }, 5);
    }

    [Trap(TrapLoader.Trap.Text,
        ["Text", "Ghost Chat", "Math Quiz", "Pinball", "Breakout", "Snake", "UNO Challenge", "PONG Challenge", "Pong", "Trivia Trap", "Laughter", "Cutscene","Literature", "Phone", "Aaa", "Tip", "Omo", "Spam", "Tutorial", "Exposition"],
        "Puts text on the screen")]
    public static bool Note(string trapName)
    {
        if (NoteObj is not null)
        {
            try
            {
                if (NoteObj.activeSelf) return false;
            }
            catch
            {
                NoteObj = null;
            }
        }

        if (!Directory.Exists("Mods/SW_CreeperKing.Slimipelago/TextTrap"))
        {
            Directory.CreateDirectory("Mods/SW_CreeperKing.Slimipelago/TextTrap");
        }

        var files = Directory.GetFiles("Mods/SW_CreeperKing.Slimipelago/TextTrap");

        NoteObj = Object.Instantiate(JournalPatch.NotePrefab);
        NoteObj.GetComponent<JournalUI>().journalText.text =
            files.Length == 0 ? DefMessage : File.ReadAllText(files[Random.Next(files.Length)]);

        return true;
    }

    [Trap(TrapLoader.Trap.Ranch, ["Ranch", "Home", "Resistance", "Sleep", "Instant Death", "Get Out"],
        "Teleports the player back to the Ranch")]
    public static bool BanishPlayer(string trapName = "")
    {
        TeleportPlayer(GameLoader.Home, RegionRegistry.RegionSetId.HOME);
        WasBanished = true;
        return true;
    }

    [Trap(TrapLoader.Trap.Tarr,
        [
            "Tarr", "Fishin' Boo", "Buyon", "Gooey", "Army", "Thwimp", "Bomb", "Ghost", "Animal", "Bonk",
            "Fear", "Nut", "Pie", "Bee", "Police", "Meteor", "Rockfall", "Spike Ball", "TNT Barrel"
        ],
        "Spawns 1 to 5 Tarrs on top of the player (unless . . . ???)")]
    public static bool SpawnTarr(string trapName)
    {
        List<GameObject> spawned = [];

        return ActivateTrapWithReset(() =>
        {
            for (var i = 0; i < (trapName == "Bee" ? 10 : Random.Next(1, 5)); i++)
            {
                spawned.Add(SpawnTarr(trapName == "Bee" ? .4f :2f/(i + 1)));
            }
        }, () => spawned.ForEach(tarr => Destroyer.Destroy(tarr, "TarrTrap.Reset")), 15);
    }

    [Trap(TrapLoader.Trap.MarketCrash, ["Market Crash", "Expensive Stocks", "Empty Item Box"],
        "Crashes the market prices")]
    public static bool MarketCrash(string trapName)
    {
        if (MarketPatch.Crash) return false;
        MarketPatch.Crash = true;
        return true;
    }

    [Trap(TrapLoader.Trap.Zoom, ["Zoom", "Deisometric", "Spooky"], "Zooms the camera in, in a strange way")]
    public static bool Zoom(string trapName)
        => ActivateTrapWithReset(() => PlayerCamera.orthographic = true, () => PlayerCamera.orthographic = false);

    [Trap(TrapLoader.Trap.ScreenFlip,
        ["Screen Flip", "Flip", "Mirror", "Monkey Mash", "Reversal", "Camera Rotate", "Confound", "Confuse", "Confusion", "Reverse"],
        "Flips the camera horizontally")]
    public static bool CameraFlip(string trapName)
        => ActivateTrapWithReset(() =>
            {
                PlayerCamera.projectionMatrix *= CameraFlipMatrix4X4;
                GL.invertCulling = true;
            },
            () =>
            {
                PlayerCamera.projectionMatrix *= CameraFlipMatrix4X4;
                GL.invertCulling = false;
            }, 20);

    [Trap(TrapLoader.Trap.Smol, ["Tiny"], "Makes the player tiny")]
    public static bool Smol(string trapName)
        => ActivateTrapWithReset(() => PlayerInWorld.transform.DOScale(SmolScale, 1),
            () => PlayerInWorld.transform.DOScale(NormalScale, 1), 20);

    [Trap(TrapLoader.Trap.Wide, ["W I D E", "Squash", "Paper"], "Makes the player W I D E")]
    public static bool Wide(string trapName)
        => ActivateTrapWithReset(() => PlayerInWorld.transform.DOScale(WideScale, 1),
            () => PlayerInWorld.transform.DOScale(NormalScale, 1));

    [Trap(TrapLoader.Trap.Damage, ["Damage", "Blue Balls Curse", "One Hit KO", "Instant Crystal"],
        "Hurts the player, lots")]
    public static bool Damage(string trapName)
    {
        PlayerStatePatch.PlayerDamageable.Damage(PlayerStatePatch.PlayerState.GetCurrHealth() - 1, null);
        return true;
    }

    [Trap(TrapLoader.Trap.Radiation,
        ["Radiation", "Fire", "Electrocution", "Double Damage", "Burn", "Poison", "Poison Mushroom"],
        "Gives the player max radiation")]
    public static bool Radiation(string trapName)
    {
        PlayerStatePatch.PlayerState.SetRad(100);
        return true;
    }

    [Trap(TrapLoader.Trap.EnergyDrain, ["Energy Drain", "No Stocks", "Depletion", "SvC Effect", "Dry"], "Sets player's energy to 0")]
    public static bool EnergyDrain(string trapName)
    {
        PlayerStatePatch.PlayerState.SetEnergy(0);
        return true;
    }

    [Trap(TrapLoader.Trap.Freeze, ["Freeze", "Input Sequence", "Paralysis", "Paralyze", "Chaos Control", "Bubble", "Stun", "Ice", "Frozen"],
        "Freezes the player in place")]
    public static bool Freeze(string trapName)
        => ActivateTrapWithReset(() => PlayerLockOnDeath.Freeze(), () => PlayerLockOnDeath.Unfreeze());

    [Trap(TrapLoader.Trap.NoVac, ["No Vac", " No Guarding", "No Petals","No Revivals", "Disable A", "Disable B", "Disable C Up", "Disable Tag", "Disable Z"],
        "Disables the vacuum")]
    public static bool NoVac(string trapName)
        => ActivateTrapWithReset(() =>
            {
                PlayerVacuum.DropAllVacced();
                PlayerVacuum.gameObject.SetActive(false);
            },
            () => PlayerVacuum.gameObject.SetActive(true));

    [Trap(TrapLoader.Trap.FrameSlime, ["Frame Slime", "PowerPoint", "Bullet Time"],
        "Forcefully feeds a frame slime some of your frames per second")]
    public static bool FrameSlime(string trapName)
        => ActivateTrapWithReset(() =>
        {
            VSync = QualitySettings.vSyncCount;
            Fps = Application.targetFrameRate;
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 15;
        }, () =>
        {
            QualitySettings.vSyncCount = VSync;
            Application.targetFrameRate = Fps;
        });

    [Trap(TrapLoader.Trap.Underwater, ["Underwater", "Sticky Floor", "Rail", "Sticky Hands", "Honey", "Iron Boots", "Slow", "Slowness", "Fishing"], "Gives you underwater effects")]
    public static bool Underwater(string trapName)
    {
        if (PlayerEffects.Underwater.Active) return false;
        return ActivateTrapWithReset(() =>
        {
            PlayerEffects.Underwater.TryStart();
            PlayerEffects.Underwater.NextAllowedStopTime = Time.time + 9;
            SRSingleton<SceneContext>.Instance.AmbianceDirector.EnterWater();
        }, () =>
        {
            PlayerEffects.Underwater.TryStop();
            SRSingleton<SceneContext>.Instance.AmbianceDirector.ExitWater();
        });
    }

    public static bool ActivateTrapWithReset(Action trapAction, Action trapResetAction, float resetTime = 10)
    {
        if (TrapLoader.HasTrapReset()) return false;
        try
        {
            trapAction();
            HouseTrigger.SetActive(false);
        }
        catch (Exception e)
        {
            Core.Log.Error("Error executing trap");
            Core.Log.Error(e);
        }

        return TrapLoader.SetTrapReSetter(() =>
        {
            try
            {
                trapResetAction();
            }
            catch (Exception e)
            {
                Core.Log.Error("Error correcting trap");
                Core.Log.Error(e);
            }
            HouseTrigger.SetActive(true);
        }, resetTime);
    }
    
    public static GameObject SpawnTarr(float scale)
    {
        var pos = PlayerInWorld.transform.position + new Vector3(Random.Next(-4, 4), Random.Next(5, 15), Random.Next(-4, 4));
        var prefab = SRSingleton<GameContext>.Instance.LookupDirector.GetPrefab(Identifiable.Id.TARR_SLIME);
        var spawned =
            SRBehaviour.InstantiateActor(prefab, PlayerModelPatch.Model.currRegionSetId, pos, Quaternion.identity);

        foreach (var listener in spawned.GetComponentsInChildren<SpawnListener>(true))
        {
            listener.DidSpawn();
        }

        spawned.transform.DOScale(new Vector3(scale, scale, scale), 1);
        return spawned;
    }
}

/* ~ is maybe in the future, X is no
~ 144p Trap
X Animal Bonus Trap
X Bald Trap
X Controller Drift Trap
X Cursed Ball Trap
~ Fake Transition
~ Fast Trap
X Frog Trap
~ Fuzzy Trap -> 144p
~ Gadget Shuffle Trap
~ Invisible Trap
~ Invisibility Trap
X Items to Bombs
~ Jumping Jacks Trap
X Light Up Path Trap
X My Turn! Trap
X Number Sequence Trap
~ Pixelate Trap -> 144p
~ Pixellation Trap -> 144p
X Pokemon Count Trap
X Pokemon Trivia Trap
X Posession Trap
~ Spotlight Trap
X Swap Trap
~ Time Limit
~ Time Warp Trap
~ Timer Trap
 */
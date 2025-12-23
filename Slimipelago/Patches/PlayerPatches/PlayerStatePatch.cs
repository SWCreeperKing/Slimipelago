using HarmonyLib;
using JetBrains.Annotations;
using MonomiPark.SlimeRancher.Regions;
using Slimipelago.Archipelago;
using Slimipelago.Patches.Interactables;
using Slimipelago.Patches.UiPatches;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Slimipelago.Patches.PlayerPatches;

[PatchAll]
public static class PlayerStatePatch
{
    public static PlayerState PlayerState;
    public static FirestormActivator FirestormActivator;
    public static GameObject PlayerInWorld = null;
    public static Rigidbody PlayerInWorldBody;
    public static vp_FPPlayerEventHandler PlayerEffects;
    public static Camera PlayerCamera;
    public static LockOnDeath PlayerLockOnDeath;
    public static PlayerDamageable PlayerDamageable;
    public static Map PlayerMap;
    public static Button SaveAndQuitButton;
    public static bool FirstUpdate { get; private set; }

    [CanBeNull] public static event Action OnFirstUpdate;

    public static Vector3 PlayerPos => PlayerInWorld.transform.position;

    [HarmonyPatch(typeof(PlayerState), "Awake"), HarmonyPostfix]
    public static void PlayerAwake(PlayerState __instance)
    {
        if (SceneManager.GetActiveScene().name is "MainMenu") return;
        if (__instance is null) return;
        try
        {
            PlayerState = __instance;
            PlayerInWorld = GameObject.Find("SimplePlayer");
            PlayerInWorldBody = PlayerInWorld.GetComponent<Rigidbody>();
            SaveAndQuitButton = GameObject.Find("HUD Root/PauseMenu/PauseUI/Buttons/QuitButton").GetComponent<Button>();
            FirestormActivator = PlayerInWorld.GetComponent<FirestormActivator>();
            PlayerLockOnDeath = PlayerInWorld.GetComponent<LockOnDeath>();
            PlayerDamageable = PlayerInWorld.GetComponent<PlayerDamageable>();
            PlayerEffects = PlayerInWorld.GetComponent<vp_FPPlayerEventHandler>();
            PlayerCamera = PlayerInWorld.GetChild(0).GetComponent<Camera>();
            PlayerCamera.orthographicSize = 1;
            
            MainMenuPatch.OnGamePotentialExit += () =>
            {
                OnFirstUpdate = null;
                PlayerState = null;
                PlayerInWorld = null;
                PlayerInWorldBody = null;
                PlayerMap = null;
                SaveAndQuitButton = null;
                PlayerLockOnDeath = null;
                PlayerDamageable = null;
                PlayerCamera = null;
                PlayerEffects = null;
                FirstUpdate = false;
            };

            Core.Log.Msg("Player Awake");
            GameLoader.ResetData();
        }
        catch (Exception e)
        {
            Core.Log.Error(e);
        }
    }

    [HarmonyPatch(typeof(PlayerState), "Update"), HarmonyPostfix]
    public static void Update()
    {
        try
        {
            if (FirstUpdate) return;
            Core.Log.Msg("First Update");
            
            foreach (var accessDoor in Resources.FindObjectsOfTypeAll<AccessDoor>())
            {
                AccessDoorPatch.RunDoorCheck(accessDoor);
            }

            ApSlimeClient.WorldOpened();
            OnFirstUpdate?.Invoke();

            FirstUpdate = true;
        }
        catch (Exception e)
        {
            Core.Log.Error(e);
        }
    }

    [HarmonyPatch(typeof(Map), "Start"), HarmonyPostfix]
    public static void MapAlive(Map __instance)
    {
        PlayerMap = __instance;
        GameLoader.LoadMapMarkers();
        Core.Log.Msg("Map Alive");
    }

    public static void TeleportPlayer(Vector3 pos, RegionRegistry.RegionSetId region)
    {
        pos.y += 5;
        PlayerModelPatch.Model.SetCurrRegionSet(region);
        PlayerInWorld.transform.position = pos;
        SRSingleton<SceneContext>.Instance.AmbianceDirector.ExitAllLiquid();
        PlayerEffects.Underwater.Stop();
        PlayerState.SetAmmoMode(PlayerState.AmmoMode.DEFAULT);
    }
}
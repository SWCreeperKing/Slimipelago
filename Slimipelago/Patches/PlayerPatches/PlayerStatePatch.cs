using HarmonyLib;
using JetBrains.Annotations;
using MonomiPark.SlimeRancher.Regions;
using Slimipelago.Patches.Interactables;
using Slimipelago.Patches.UiPatches;
using Slimipzelago.Archipelago;
using UnityEngine;
using UnityEngine.UI;

namespace Slimipelago.Patches.PlayerPatches;

[PatchAll]
public static class PlayerStatePatch
{
    public static PlayerState PlayerState;
    public static FirestormActivator FirestormActivator;
    public static GameObject PlayerInWorld = null;
    public static Rigidbody PlayerInWorldBody;
    public static Map PlayerMap;
    public static Button SaveAndQuitButton;
    public static bool FirstUpdate { get; private set; }

    [CanBeNull] public static event Action OnFirstUpdate;

    public static Vector3 PlayerPos => PlayerInWorld.transform.position;

    [HarmonyPatch(typeof(PlayerState), "Awake"), HarmonyPostfix]
    public static void PlayerAwake(PlayerState __instance)
    {
        PlayerState = __instance;
        PlayerInWorld = GameObject.Find("SimplePlayer");
        PlayerInWorldBody = PlayerInWorld.GetComponent<Rigidbody>();
        SaveAndQuitButton = GameObject.Find("HUD Root/PauseMenu/PauseUI/Buttons/QuitButton").GetComponent<Button>();
        FirestormActivator = PlayerInWorld.GetComponent<FirestormActivator>();

        MainMenuPatch.OnGamePotentialExit += () =>
        {
            OnFirstUpdate = null;
            PlayerState = null;
            PlayerInWorld = null;
            PlayerInWorldBody = null;
            PlayerMap = null;
            SaveAndQuitButton = null;
            FirstUpdate = false;
        };

        OnFirstUpdate += () => Core.Log.Msg("First Update");
        OnFirstUpdate += ApSlimeClient.WorldOpened;

        Core.Log.Msg("Player Awake");
        GameLoader.ResetData();
    }

    [HarmonyPatch(typeof(PlayerState), "Update"), HarmonyPostfix]
    public static void Update()
    {
        try
        {
            if (FirstUpdate) return;
            OnFirstUpdate?.Invoke();
            foreach (var accessDoor in Resources.FindObjectsOfTypeAll<AccessDoor>())
            {
                AccessDoorPatch.RunDoorCheck(accessDoor);
            }

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
    }
}
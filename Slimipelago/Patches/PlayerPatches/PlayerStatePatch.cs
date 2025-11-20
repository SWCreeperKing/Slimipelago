using HarmonyLib;
using JetBrains.Annotations;
using Slimipelago.Patches.UiPatches;
using UnityEngine;
using UnityEngine.UI;

namespace Slimipelago.Patches.PlayerPatches;

[PatchAll]
public static class PlayerStatePatch
{
    public static PlayerState PlayerState;
    public static GameObject PlayerInWorld = null;
    public static Rigidbody PlayerInWorldBody;
    public static Map PlayerMap;
    public static Button SaveAndQuitButton;
    private static bool _FirstUpdate;

    [CanBeNull] public static event Action OnFirstUpdate;
    
    public static Vector3 PlayerPos => PlayerInWorld.transform.position;
    
    [HarmonyPatch(typeof(PlayerState), "Awake"), HarmonyPostfix]
    public static void PlayerAwake(PlayerState __instance)
    {
        PlayerState = __instance;
        PlayerInWorld = GameObject.Find("SimplePlayer");
        PlayerInWorldBody = PlayerInWorld.GetComponent<Rigidbody>();
        SaveAndQuitButton = GameObject.Find("HUD Root/PauseMenu/PauseUI/Buttons/QuitButton").GetComponent<Button>();

        MainMenuPatch.OnGamePotentialExit += () =>
        {
            OnFirstUpdate = null;
            PlayerState = null;
            PlayerInWorld = null;
            PlayerInWorldBody = null;
            PlayerMap = null;
            SaveAndQuitButton = null;
            _FirstUpdate = false;
        };

        OnFirstUpdate += () => Core.Log.Msg("First Update");
        Core.Log.Msg("Player Awake");
        GameLoader.ResetData();
    }

    [HarmonyPatch(typeof(PlayerState), "Update"), HarmonyPostfix]
    public static void Update()
    {
        if (_FirstUpdate) return;
        OnFirstUpdate?.Invoke();
        _FirstUpdate = true;
    }

    [HarmonyPatch(typeof(Map), "Start"), HarmonyPostfix]
    public static void MapAlive(Map __instance)
    {
        PlayerMap = __instance;
        GameLoader.LoadMapMarkers();
        Core.Log.Msg("Map Alive");
    }

    public static void TeleportPlayer(Vector3 pos)
    {
        pos.y += 5;
        PlayerInWorld.transform.position = pos;
    }
}
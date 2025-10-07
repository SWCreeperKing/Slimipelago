using HarmonyLib;
using UnityEngine;

namespace Slimipelago.Patches;

[PatchAll]
public static class PlayerStatePatch
{
    public static PlayerState PlayerState;
    public static GameObject PlayerInWorld = null;
    public static Rigidbody PlayerInWorldBody;
    public static Map PlayerMap;
    
    public static Vector3 PlayerPos => PlayerInWorld.transform.position;
    
    [HarmonyPatch(typeof(PlayerState), "Awake"), HarmonyPostfix]
    public static void PlayerAwake(PlayerState __instance)
    {
        PlayerState = __instance;
        PlayerInWorld = GameObject.Find("SimplePlayer");
        PlayerInWorldBody = PlayerInWorld.GetComponent<Rigidbody>();
        Core.Log.Msg("Player Awake");
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
using HarmonyLib;
using UnityEngine;

namespace Slimipelago.Patches;

[HarmonyPatchAll]
public static class PlayerStatePatch
{
    public static PlayerState PlayerState;
    public static GameObject PlayerInWorld;
    public static Rigidbody PlayerInWorldBody;
    
    [HarmonyPatch(typeof(PlayerState), "Awake"), HarmonyPostfix]
    public static void PlayerAwake(PlayerState __instance)
    {
        PlayerState = __instance;
        PlayerInWorld = GameObject.Find("SimplePlayer");
        PlayerInWorldBody = PlayerInWorld.GetComponent<Rigidbody>();
        Core.Log.Msg($"PlayerState obj: [{PlayerState.gameObject.name}]");
    }
}
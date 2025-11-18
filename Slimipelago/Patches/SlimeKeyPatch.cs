using HarmonyLib;
using UnityEngine;

namespace Slimipelago.Patches;

// [PatchAll]
public static class SlimeKeyPatch
{
    [HarmonyPatch(typeof(SlimeKey), "Awake"), HarmonyPostfix]
    public static void Start(SlimeKey __instance)
    {
        Core.Log.Msg(__instance.name);
    }

    [HarmonyPatch(typeof(SlimeKey), "OnTriggerEnter"), HarmonyPrefix]
    public static bool OnTriggerEnter(Collider col, SlimeKey __instance)
    {
        if (col.gameObject != SRSingleton<SceneContext>.Instance.Player) return false;
        var pos = __instance.transform.position;
        Core.Log.Msg($"Touched key: {pos.HashPos()}");
        // return false;
        return true;
    }
}
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace Slimipelago.Patches.PlayerPatches;

[PatchAll]
public static class CameraDisablerPatch
{
    [CanBeNull] public static List<Component> DisablerBlockers = null;

    [HarmonyPatch(typeof(CameraDisabler), "Start"), HarmonyPostfix]
    public static void DisablerStart(List<Component> ___blockers) => DisablerBlockers = ___blockers;
}
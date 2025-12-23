using HarmonyLib;
using Slimipelago.Archipelago;
using UnityEngine.SceneManagement;

namespace Slimipelago.Patches.UiPatches;

[PatchAll]
public static class CreditsPatch
{
    [HarmonyPatch(typeof(CreditsUI), "OnEnable"), HarmonyPrefix]
    public static void OnEnable()
    {
        if (SceneManager.GetActiveScene().name is "MainMenu") return;
        if (ApSlimeClient.GoalType != 2) return;
        ApSlimeClient.Client.Goal();
    }
}
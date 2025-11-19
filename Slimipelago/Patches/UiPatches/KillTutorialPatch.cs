using HarmonyLib;

namespace Slimipelago.Patches.UiPatches;

[PatchAll]
public static class KillTutorialPatch
{
    [HarmonyPatch(typeof(TutorialDirector), "MaybeShowPopup"), HarmonyPrefix]
    public static bool PopupTutorial(TutorialDirector.Id id) => false;
}
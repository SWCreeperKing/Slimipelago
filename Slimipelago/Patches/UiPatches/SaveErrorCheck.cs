using HarmonyLib;
using MonomiPark.SlimeRancher.DataModel;

namespace Slimipelago.Patches.UiPatches;

[PatchAll]
public static class SaveErrorCheck
{
    [HarmonyPatch(typeof(SaveErrorUI), "SetException"), HarmonyPrefix]
    public static void FindFullError(Exception e, string path)
    {
        Core.Log.Error("FAILED TO SAVE", e);
    }

    [HarmonyPatch(typeof(GameModel), "AllActors"), HarmonyPostfix]
    public static void ActorFix(GameModel __instance, ref Dictionary<long, ActorModel> __result)
    {
        __result = __result.Where(allActor =>
        {
            var ident = allActor.Value.ident;
            if (!Identifiable.SCENE_OBJECTS.Contains(ident) && ident != Identifiable.Id.QUICKSILVER_SLIME)
            {
                var actor = allActor.Value;

                try
                {
                    var pos = actor.GetPos();
                }
                catch
                {
                    Core.Log.Error($"Failed to get position of an actor of [{__instance.name}]: [{actor.actorId}]:[{actor.ident}]");
                    return false;
                }
            }
            return true;
        }).ToDictionary(kv => kv.Key, kv => kv.Value);
    }
}
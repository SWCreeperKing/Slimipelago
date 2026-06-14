using HarmonyLib;
using MonomiPark.SlimeRancher;
using MonomiPark.SlimeRancher.DataModel;
using MonomiPark.SlimeRancher.Persist;
using Slimipelago.Archipelago;

namespace Slimipelago.Patches.UiPatches;

[PatchAll]
public static class SaveErrorCheck
{
    [HarmonyPatch(typeof(AutoSaveDirector), "SaveGame"), HarmonyPrefix]
    public static void EndTrapsOnSave() => TrapLoader.EMERGENCY_END_TRAP();

    [HarmonyPatch(typeof(AutoSaveDirector), "SaveAllNow"), HarmonyPrefix]
    public static void EndTrapsOnSave2() => TrapLoader.EMERGENCY_END_TRAP();

    [HarmonyPatch(typeof(SaveErrorUI), "SetException"), HarmonyPrefix]
    public static void FindFullError(Exception e, string path) => Core.Log.Error("FAILED TO SAVE", e);

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
                        Core.Log.Error(
                            $"Failed to get position of an actor of [{__instance.name}]: [{actor.actorId}]:[{actor.ident}]"
                        );
                        // return false;
                    }
                }
                return true;
            }
        ).ToDictionary(kv => kv.Key, kv => kv.Value);
    }

    [HarmonyPatch(typeof(SavedGame), "Pull", typeof(GameModel), typeof(List<ActorDataV09>), typeof(WorldV22)),
     HarmonyPrefix]
    private static bool Pull(SavedGame __instance, GameModel gameModel, List<ActorDataV09> actors, WorldV22 world)
    {
        foreach (var allActor in gameModel.AllActors())
        {
            var actor = allActor.Value;
            var ident = actor.ident;
            if (Identifiable.SCENE_OBJECTS.Contains(ident) || ident == Identifiable.Id.QUICKSILVER_SLIME) continue;

            try
            {
                var actorData = __instance.CallPrivateMethod<ActorDataV09>(
                    "BuildActorData", gameModel, (int)ident, actor.actorId, actor, world
                );
                actors.Add(actorData);
            }
            catch (Exception)
            {
                Core.Log.Msg($"Skipping Actor: [{actor.actorId}]|[{actor.currRegionSetId}]|{allActor.Value.ident}");
            }
        }
        return false;
    }
}
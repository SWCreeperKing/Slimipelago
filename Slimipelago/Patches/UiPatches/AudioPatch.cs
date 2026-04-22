using HarmonyLib;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Slimipelago.Patches.UiPatches;

// [PatchAll]
public static class AudioPatch
{
    [HarmonyPatch(typeof(SECTR_PointSource), "Play"), HarmonyPrefix]
    public static bool PlortScored(SECTR_PointSource __instance, SECTR_AudioCueInstance ___instance, float ___volume,
        float ___pitch)
    {
        // if (__instance.name.Contains("slime")) return true;

        try
        {
            if (__instance.gameObject.transform.parent?.gameObject.name is not "PlortCollector") return true;

            if (__instance.IsPlaying && ___instance.Loops) ___instance.Stop(false);
            if (__instance.Cue is null)
            {
                Core.Log.Warning("Cue is Null");
                return true;
            }

            var system = typeof(SECTR_AudioSystem).GetPrivateStaticField<SECTR_AudioSystem>("system");
            var globalInstances = typeof(SECTR_AudioSystem).GetPrivateStaticField<object>("activeInstances");
            var globalInstanceCount = globalInstances.CallPublicProperty<int>("Count");
            
            var instanceTable = typeof(SECTR_AudioSystem).GetPrivateStaticField<object>("maxInstancesTable");
            var hasMaxInstances = instanceTable.CallPublicMethod<bool>("ContainsKey", __instance.Cue);
            
            Core.Log.Msg($"Global Instances: [{globalInstanceCount}], max: [{system.MaxInstances}]");
            Core.Log.Msg($"Is max instances: [{hasMaxInstances}]");

            ___instance = __instance.Cue.Spatialization != SECTR_AudioCue.Spatializations.Infinite3D
                ? SECTR_AudioSystem.Play(__instance.Cue, __instance.transform, Vector3.zero, __instance.Loop)
                : SECTR_AudioSystem.Play(
                    __instance.Cue, SECTR_AudioSystem.Listener, Random.onUnitSphere, __instance.Loop
                );

            if (!___instance)
            {
                Core.Log.Warning("Instance is Null");
                return false;
            }
            ___instance.Volume = ___volume;
            ___instance.Pitch = ___pitch;

            Core.Log.Msg($"Play Sound: [{__instance.name}] | v:{___volume}, p:{___pitch}");
        }
        catch (Exception e) { Core.Log.Error(e); }

        return true;
    }
}
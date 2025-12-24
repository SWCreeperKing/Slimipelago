using CreepyUtil.Archipelago;
using HarmonyLib;
using Slimipelago.Archipelago;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Slimipelago.Patches.PlayerPatches;

[PatchAll]
public static class PlayerDeathHandlerPatch
{
    public static bool DeathlinkRecieved = false;

    public static Dictionary<DeathHandler.Source, string> DeathMessages = new()
    {
        [DeathHandler.Source.UNDEFINED] = "found the end of their adventure",
        [DeathHandler.Source.SLIME_ATTACK] = "got a bit too curious about slimes",
        [DeathHandler.Source.SLIME_ATTACK_PLAYER] = "became a tasty morsel of a rampaging tarr",
        [DeathHandler.Source.SLIME_CRYSTAL_SPIKES] = "didn't look where they were walking",
        [DeathHandler.Source.SLIME_DAMAGE_PLAYER_ON_TOUCH] = "got too close to the spikes of a rock slime",
        [DeathHandler.Source.SLIME_EXPLODE] = "went boom",
        [DeathHandler.Source.SLIME_IGNITE] = "burned into ashes",
        [DeathHandler.Source.SLIME_RAD] = "wanted to become Superman",
        [DeathHandler.Source.CHICKEN_VAMPIRISM] = "got to close to a... vampiric chicken??? what?",
        [DeathHandler.Source.KILL_ON_TRIGGER] = "went for a swim",
        [DeathHandler.Source.EMERGENCY_RETURN] = "was too frightened by the slimes",
        [DeathHandler.Source.FALL_DAMAGE] = "fell from a high cliff? there's no fall damage... How did you do that",
    };
    
    [HarmonyPatch(typeof(PlayerDeathHandler), "ResetPlayer"), HarmonyPrefix]
    public static void ResetPlayer(ref PlayerDeathHandler.DeathType deathType)
    {
        if (deathType is PlayerDeathHandler.DeathType.SLIMULATIONS) return;
        deathType = PlayerDeathHandler.DeathType.RESET_PLAYER_LOCATION;
    }

    [HarmonyPatch(typeof(DeathHandler), "Kill"), HarmonyPrefix]
    public static void Kill(GameObject gameObject, DeathHandler.Source source)
    {
        if (SceneManager.GetActiveScene().name is  "MainMenu") return;
        if (!ApSlimeClient.Client.IsConnected) return;
        if (!ApSlimeClient.Client.Tags[ArchipelagoTag.DeathLink]) return;
        if (gameObject.name is not "SimplePlayer") return;
        TrapLoader.EMERGENCY_END_TRAP();
        
        if (DeathlinkRecieved)
        {
            DeathlinkRecieved = false;
            return;
        }
        
        Core.Log.Msg("Sending Deathlink");
        ApSlimeClient.Client.SendDeathLink($"{ApSlimeClient.Client.PlayerName} {DeathMessages[source]}");
    }
}
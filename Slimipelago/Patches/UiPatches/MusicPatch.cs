using HarmonyLib;
using MonomiPark.SlimeRancher.Regions;
using Slimipzelago.Archipelago;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Random = System.Random;

namespace Slimipelago.Patches.UiPatches;

[PatchAll]
public static class MusicPatch
{
    public static Random Random = new();
    public static readonly string CurrentDirectory = Directory.GetCurrentDirectory();
    public static readonly Dictionary<string, List<SECTR_AudioCue.ClipData>[]> Songs = [];
    public static Dictionary<string, SECTR_AudioCue.ClipData[]> VanillaSongs = [];

    public static Dictionary<ZoneDirector.Zone, string> SongZones = new()
    {
        [ZoneDirector.Zone.RANCH] = "Ranch",
        [ZoneDirector.Zone.REEF] = "Reef",
        [ZoneDirector.Zone.QUARRY] = "Quarry",
        [ZoneDirector.Zone.MOSS] = "Moss",
        [ZoneDirector.Zone.DESERT] = "Desert",
        [ZoneDirector.Zone.SEA] = "Sea",
        [ZoneDirector.Zone.RUINS] = "Ruins",
        [ZoneDirector.Zone.RUINS_TRANSITION] = "Ruins Transition",
    };

    [HarmonyPatch(typeof(MusicDirector), "GetRegionMusic"), HarmonyPostfix]
    private static void GetRegionMusic(MusicDirector __instance, RegionMember member, ref MusicDirector.Music __result)
    {
        if (!ApSlimeClient.MusicRando) return;
        if (ApSlimeClient.MusicRandoRandomizeOnce) return;
        var source = ZoneDirector.Zones(member);
        if (__result is not MusicDirector.Music.Zone.Default zoneSong || !source.Any()) return;
        SetDayAndNight(zoneSong, SongZones[source.Max()]);
    }

    [HarmonyPatch(typeof(MusicDirector), "OnSceneLoaded"), HarmonyPostfix]
    private static void OnSceneLoaded(MusicDirector __instance, Scene scene, LoadSceneMode mode)
    {
        if (!ApSlimeClient.MusicRando) return;
        VanillaSongs = new Dictionary<string, SECTR_AudioCue.ClipData[]>
        {
            ["Ranch"] =
            [
                __instance.ranchMusic.background.AudioClips[0], __instance.ranchMusic.nightBackground.AudioClips[0]
            ],
            ["Reef"] =
            [
                __instance.reefMusic.background.AudioClips[0], __instance.reefMusic.nightBackground.AudioClips[0]
            ],
            ["Quarry"] =
                [__instance.quarryMusic.background.AudioClips[0], __instance.quarryMusic.nightBackground.AudioClips[0]],
            ["Moss"] =
            [
                __instance.mossMusic.background.AudioClips[0], __instance.mossMusic.nightBackground.AudioClips[0]
            ],
            ["Desert"] =
                [__instance.desertMusic.background.AudioClips[0], __instance.desertMusic.nightBackground.AudioClips[0]],
            ["Sea"] = [__instance.seaMusic.background.AudioClips[0], __instance.seaMusic.nightBackground.AudioClips[0]],
            ["Ruins"] =
            [
                __instance.ruinsMusic.background.AudioClips[0], __instance.ruinsMusic.nightBackground.AudioClips[0]
            ],
            ["Ruins Transition"] =
            [
                __instance.ruinsTransMusic.background.AudioClips[0],
                __instance.ruinsTransMusic.nightBackground.AudioClips[0]
            ],
        };

        if (!ApSlimeClient.MusicRandoRandomizeOnce) return;
        var seed = ApSlimeClient.Client.Seed;
        Random = seed is null ? new Random() : new Random(ApSlimeClient.RandoSeeds[seed]);
        SetDayAndNight(__instance.reefMusic, "Reef");
        SetDayAndNight(__instance.quarryMusic, "Quarry");
        SetDayAndNight(__instance.mossMusic, "Moss");
        SetDayAndNight(__instance.desertMusic, "Desert");
        SetDayAndNight(__instance.seaMusic, "Sea");
        SetDayAndNight(__instance.ruinsMusic, "Ruins");
        SetDayAndNight(__instance.ruinsTransMusic, "Ruins Transition");
    }

    public static void SetDayAndNight(MusicDirector.Music.Zone.Default music, string area)
    {
        music.background.AudioClips = GetRandomSong(area, true);
        music.nightBackground.AudioClips = GetRandomSong(area, false);
    }

    public static List<SECTR_AudioCue.ClipData> GetRandomSong(string region, bool isDay)
    {
        var possibleSongs = Songs[region][isDay ? 0 : 1]
                           .Concat(Songs[region][2])
                           .Concat(Songs["Any"][isDay ? 0 : 1])
                           .Concat(Songs["Any"][2])
                           .Concat(VanillaSongs.Select(songs => songs.Value[isDay ? 0 : 1]))
                           .ToArray();
        return [possibleSongs[Random.Next(possibleSongs.Length)]];
    }

    public static void LoadSongs()
    {
        CheckDirectory("MusicRando");

        var folders = SongZones.Values.Append("Any").ToArray();
        foreach (var folder in folders)
        {
            CheckDirectory($"MusicRando/{folder}/Day");
            CheckDirectory($"MusicRando/{folder}/Night");
            CheckDirectory($"MusicRando/{folder}/Both");
        }

        foreach (var folder in folders)
        {
            List<SECTR_AudioCue.ClipData> day = [];
            List<SECTR_AudioCue.ClipData> night = [];
            List<SECTR_AudioCue.ClipData> both = [];
            LoadSongsInDirectory(day, $"MusicRando/{folder}/Day");
            LoadSongsInDirectory(night, $"MusicRando/{folder}/Night");
            LoadSongsInDirectory(both, $"MusicRando/{folder}/Both");
            Songs[folder] = [day, night, both];
        }
    }

    public static void CheckDirectory(string dir)
    {
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
    }

    public static void LoadSongsInDirectory(List<SECTR_AudioCue.ClipData> clips, string dir)
    {
        foreach (var rawFile in Directory.GetFiles(dir))
        {
            var file = $"{CurrentDirectory}/{rawFile}";
            Task.Run(async () =>
                 {
                     clips.Add(new SECTR_AudioCue.ClipData(await LoadClip(file)));
                     Core.Log.Msg($"Song Loaded: [{rawFile}]");
                 }).GetAwaiter().GetResult();
        }
    }

    public static async Task<AudioClip> LoadClip(string path)
    {
        AudioClip clip = null;

        var type = path.Split('.').Last().ToLower() switch
        {
            "wav" => AudioType.WAV,
            "mp3" => AudioType.MPEG,
            "ogg" => AudioType.OGGVORBIS,
            _ => AudioType.UNKNOWN
        };

        using var uwr = UnityWebRequestMultimedia.GetAudioClip(path, type);
        uwr.SendWebRequest();

        // wrap tasks in try/catch, otherwise it'll fail silently
        try
        {
            while (!uwr.isDone) await Task.Delay(5);

            if (uwr.isNetworkError || uwr.isHttpError) Core.Log.Error($"{uwr.error}");
            else clip = DownloadHandlerAudioClip.GetContent(uwr);
        }
        catch (Exception e)
        {
            Core.Log.Error(e);
        }

        return clip;
    }
}
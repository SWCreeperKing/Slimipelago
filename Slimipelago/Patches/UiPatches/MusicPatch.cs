using HarmonyLib;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using static Slimipelago.Archipelago.ApSlimeClient;
using Random = System.Random;

namespace Slimipelago.Patches.UiPatches;

[PatchAll]
public static class MusicPatch
{
    public static Random Random = new();
    public static readonly string CurrentDirectory = Directory.GetCurrentDirectory();
    public static readonly Dictionary<string, List<SECTR_AudioCue.ClipData>[]> Songs = [];
    public static readonly string[] EventSongs = ["Tarr", "Firestorm"];

    public static readonly string[] RegionSongs =
        ["Ranch", "Reef", "Quarry", "Moss", "Desert", "Sea", "Ruins", "Ruins Transition"];

    [HarmonyPatch(typeof(MusicDirector), "OnSceneLoaded"), HarmonyPostfix]
    private static void OnSceneLoaded(MusicDirector __instance, Scene scene, LoadSceneMode mode)
    {
        if (!Data.MusicRando) return;
        var songs = new Dictionary<string, SECTR_AudioCue[]>
        {
            ["Ranch"] = [__instance.ranchMusic.background, __instance.ranchMusic.nightBackground],
            ["Reef"] = [__instance.reefMusic.background, __instance.reefMusic.nightBackground],
            ["Quarry"] = [__instance.quarryMusic.background, __instance.quarryMusic.nightBackground],
            ["Moss"] = [__instance.mossMusic.background, __instance.mossMusic.nightBackground],
            ["Desert"] = [__instance.desertMusic.background, __instance.desertMusic.nightBackground],
            ["Sea"] = [__instance.seaMusic.background, __instance.seaMusic.nightBackground],
            ["Ruins"] = [__instance.ruinsMusic.background, __instance.ruinsMusic.nightBackground],
            ["Ruins Transition"] =
                [__instance.ruinsTransMusic.background, __instance.ruinsTransMusic.nightBackground],
            ["Tarr"] = [__instance.tarrMusic],
            ["Firestorm"] = [__instance.firestormMusic],
        };

        foreach (var cue in songs.Values.SelectMany(cues => cues))
        {
            cue.PlaybackMode = SECTR_AudioCue.PlaybackModes.Random;
        }

        var seed = Client.Seed;
        Random = seed is null ? new Random() : new Random(RandoSeeds[seed]);

        foreach (var region in RegionSongs)
        {
            AddSongs(songs[region][0], region, 0, true);
            AddSongs(songs[region][1], region, 1, true);
        }

        foreach (var song in EventSongs)
        {
            AddSongs(songs[song][0], song, 0, false);
        }

        return;

        void AddSongs(SECTR_AudioCue cue, string region, int timeOfDay, bool any)
        {
            AppendSongs(cue.AudioClips, region, timeOfDay, any);

            if (!Data.MusicRandoRandomizeOnce) return;
            var clip = cue.AudioClips[Random.Next(cue.AudioClips.Count)];
            cue.AudioClips.Clear();
            cue.AudioClips.Add(clip);
        }

        void AppendSongs(List<SECTR_AudioCue.ClipData> data, string region, int timeOfDay, bool any)
        {
            LesserAppendSongs(data, region, timeOfDay);
            if (!any) return;
            LesserAppendSongs(data, "Any", timeOfDay);
        }

        void LesserAppendSongs(List<SECTR_AudioCue.ClipData> data, string region, int timeOfDay)
        {
            data.AddRange(Songs[region][timeOfDay]);
            if (Songs[region].Length <= 2) return;
            data.AddRange(Songs[region][2]);
        }
    }

    public static void LoadSongs()
    {
        CheckDirectory("MusicRando");

        var folders = RegionSongs.Append("Any").ToArray();
        foreach (var folder in folders)
        {
            CheckDirectory($"MusicRando/{folder}/Day");
            CheckDirectory($"MusicRando/{folder}/Night");
            CheckDirectory($"MusicRando/{folder}/Both");
        }

        foreach (var song in EventSongs)
        {
            CheckDirectory($"MusicRando/{song}");
        }

        foreach (var folder in folders)
        {
            try
            {
                Songs[folder] = [[], [], []];
                LoadSongsInDirectory(Songs[folder][0], $"MusicRando/{folder}/Day");
                LoadSongsInDirectory(Songs[folder][1], $"MusicRando/{folder}/Night");
                LoadSongsInDirectory(Songs[folder][2], $"MusicRando/{folder}/Both");
            }
            catch (Exception e)
            {
                Core.Log.Error($"Failed Loading: [{folder}]");
                Core.Log.Error(e);
            }
        }

        foreach (var song in EventSongs)
        {
            Songs[song] = [[]];
            LoadSongsInDirectory(Songs[song][0], $"MusicRando/{song}");
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
            try
            {
                var file = $"{CurrentDirectory}/{rawFile}";
                var clip = LoadClip(file);
                if (clip is null) return;
                clips.Add(new SECTR_AudioCue.ClipData(clip));
                Core.Log.Msg($"Song Loaded: [{rawFile}]");
            }
            catch (Exception e)
            {
                Core.Log.Error($"Failed Loading: [{rawFile}]");
                Core.Log.Error(e);
            }
        }
    }

    public static AudioClip LoadClip(string path)
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

        while (!uwr.isDone) Task.Delay(5).GetAwaiter().GetResult();

        if (uwr.isNetworkError || uwr.isHttpError) Core.Log.Error($"{uwr.error}");
        else clip = DownloadHandlerAudioClip.GetContent(uwr);

        return clip;
    }
}
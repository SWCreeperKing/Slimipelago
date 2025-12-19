using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using static Slimipelago.Archipelago.ApSlimeClient;
using Random = System.Random;

namespace Slimipelago.Patches.UiPatches;

[PatchAll]
public static class MusicPatch
{
    [CanBeNull] public static MethodInfo TarrMusicMethod;
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

    [HarmonyPatch(typeof(MusicDirector), "OnSceneLoaded"), HarmonyPostfix]
    private static void OnSceneLoaded(MusicDirector __instance, Scene scene, LoadSceneMode mode)
    {
        var trying = "WTF???";
        try
        {
            if (!MusicRando) return;
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
                [
                    __instance.quarryMusic.background.AudioClips[0],
                    __instance.quarryMusic.nightBackground.AudioClips[0]
                ],
                ["Moss"] =
                [
                    __instance.mossMusic.background.AudioClips[0], __instance.mossMusic.nightBackground.AudioClips[0]
                ],
                ["Desert"] =
                [
                    __instance.desertMusic.background.AudioClips[0],
                    __instance.desertMusic.nightBackground.AudioClips[0]
                ],
                ["Sea"] =
                [
                    __instance.seaMusic.background.AudioClips[0], __instance.seaMusic.nightBackground.AudioClips[0]
                ],
                ["Ruins"] =
                [
                    __instance.ruinsMusic.background.AudioClips[0], __instance.ruinsMusic.nightBackground.AudioClips[0]
                ],
                ["Ruins Transition"] =
                [
                    __instance.ruinsTransMusic.background.AudioClips[0],
                    __instance.ruinsTransMusic.nightBackground.AudioClips[0]
                ],
                ["Tarr"] =
                [
                    __instance.tarrMusic.AudioClips[0]
                ]
            };

            if (!MusicRandoRandomizeOnce) return;
            var seed = Client.Seed;
            Random = seed is null ? new Random() : new Random(RandoSeeds[seed]);
            trying = "Ranch";
            SetDayAndNight(__instance.ranchMusic, "Ranch");
            trying = "Reef";
            SetDayAndNight(__instance.reefMusic, "Reef");
            trying = "Quarry";
            SetDayAndNight(__instance.quarryMusic, "Quarry");
            trying = "Moss";
            SetDayAndNight(__instance.mossMusic, "Moss");
            trying = "Glass Desert";
            SetDayAndNight(__instance.desertMusic, "Desert");
            trying = "Sea";
            SetDayAndNight(__instance.seaMusic, "Sea");
            trying = "Ruins";
            SetDayAndNight(__instance.ruinsMusic, "Ruins");
            trying = "Ruins Transition";
            SetDayAndNight(__instance.ruinsTransMusic, "Ruins Transition");
            trying = "Tarr";
            __instance.tarrMusic.AudioClips = GetRandomSong(Songs["Tarr"][0].Concat(VanillaSongs["Tarr"]));
        }
        catch (Exception e)
        {
            Core.Log.Error($"There was an error trying to load Music Rando on [{trying}]");
            Core.Log.Error(e);
        }
    }

    [HarmonyPatch(typeof(MusicDirector), "GetRegionMusic"), HarmonyPostfix]
    private static void GetRegionMusic(MusicDirector __instance, RegionMember member, ref MusicDirector.Music __result)
    {
        try
        {
            if (!MusicRando) return;
            if (MusicRandoRandomizeOnce) return;
            var source = ZoneDirector.Zones(member);
            if (__result is not MusicDirector.Music.Zone.Default zoneSong || !source.Any()) return;
            SetDayAndNight(zoneSong, SongZones[source.Max()]);
        }
        catch (Exception e)
        {
            Core.Log.Error(e);
        }
    }

    [HarmonyPatch(typeof(MusicDirector), "SetTarrMode"), HarmonyPrefix]
    public static bool SetTarrMode(MusicDirector __instance, bool enabled)
    {
        if (!MusicRando || MusicRandoRandomizeOnce) return true;
        try
        {
            __instance.tarrMusic.AudioClips = GetRandomSong(Songs["Tarr"][0].Concat(VanillaSongs["Tarr"]));

            if (TarrMusicMethod is null)
            {
                TarrMusicMethod = __instance.GetType()
                                            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                                            .FirstOrDefault(mi =>
                                             {
                                                 var param = mi.GetParameters();
                                                 return mi.Name == "Enqueue" && param.Length == 3 &&
                                                        param[2].ParameterType == typeof(bool);
                                             });
                MainMenuPatch.OnGamePotentialExit += () => TarrMusicMethod = null;
            }
            
            TarrMusicMethod!.Invoke(__instance, [__instance.tarrMusic, MusicDirector.Priority.TARR, enabled]);
        }
        catch (Exception e)
        {
            Core.Log.Error(e);
        }

        return false;
    }

    public static void SetDayAndNight(MusicDirector.Music.Zone.Default music, string area)
    {
        music.background.AudioClips = GetRandomSong(area, true);
        music.nightBackground.AudioClips = GetRandomSong(area, false);
    }

    public static List<SECTR_AudioCue.ClipData> GetRandomSong(string region, bool isDay)
    {
        var time = isDay ? 0 : 1;
        var possibleSongs = Songs[region][time]
                           .Concat(Songs[region][2])
                           .Concat(Songs["Any"][time])
                           .Concat(Songs["Any"][2])
                           .Concat(VanillaSongs.Where(songs => songs.Key != "Tarr")
                                               .Select(songs => songs.Value[time]))
                           .ToArray();
        return GetRandomSong(possibleSongs);
    }

    public static List<SECTR_AudioCue.ClipData> GetRandomSong(IEnumerable<SECTR_AudioCue.ClipData> list)
    {
        var clipDatas = list as SECTR_AudioCue.ClipData[] ?? list.ToArray();
        if (clipDatas.Length == 0)
        {
            Core.Log.Error("NO SONGS IN LIST");
            return [];
        }
        
        if (clipDatas.Length == 1) return [clipDatas[0]];
        return [clipDatas[Random.Next(clipDatas.Length)]];
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

        CheckDirectory("MusicRando/Tarr");

        foreach (var folder in folders)
        {
            try
            {
                List<SECTR_AudioCue.ClipData> day = [];
                List<SECTR_AudioCue.ClipData> night = [];
                List<SECTR_AudioCue.ClipData> both = [];
                LoadSongsInDirectory(day, $"MusicRando/{folder}/Day");
                LoadSongsInDirectory(night, $"MusicRando/{folder}/Night");
                LoadSongsInDirectory(both, $"MusicRando/{folder}/Both");
                Songs[folder] = [day, night, both];
            }
            catch (Exception e)
            {
                Core.Log.Error($"Failed Loading: [{folder}]");
                Core.Log.Error(e);
            }
        }

        List<SECTR_AudioCue.ClipData> tarr = [];
        LoadSongsInDirectory(tarr, "MusicRando/Tarr");
        Songs["Tarr"] = [tarr];
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
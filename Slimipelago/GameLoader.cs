using Archipelago.MultiClient.Net.Enums;
using MonomiPark.SlimeRancher.DataModel;
using Slimipelago.Added;
using Slimipelago.Patches.PlayerPatches;
using Slimipelago.Patches.UiPatches;
using Slimipzelago.Archipelago;
using UnityEngine;
using UnityEngine.Events;
using static MonomiPark.SlimeRancher.Regions.RegionRegistry;
using static Slimipelago.Patches.PlayerPatches.PlayerStatePatch;

namespace Slimipelago;

public static class GameLoader
{
    public static readonly Vector3 Home = new(91, 14.9f, -141);
    public static readonly Vector3 Overgrowth = new(-44.8f, 17.1f, -158.3f);
    public static readonly Vector3 Lab = new(194.7f, 14.8f, -273.1f);
    public static readonly Vector3 Reef = new(-108.7f, .6f, 138.9f);
    public static readonly Vector3 ReefBeach = new(-236.7f, 0.9f, -120.6f);
    public static readonly Vector3 Quarry = new(258.7f, 4.5f, 189.1f);
    public static readonly Vector3 Moss = new(-307.9f, .4f, 401.5f);
    public static readonly Vector3 RuinsTransition = new(38.9f, 4.8f, 498.8f);
    public static readonly Vector3 Ruins = new(91.9f, 22.8f, 715.8f);
    public static readonly Vector3 Glass = new(-28.4f, 1033.4f, 437.1f);

    public static readonly Dictionary<string, ItemDisplayOnMap> MarkerDictionary = [];
    public static Dictionary<string, Sprite> Spritemap = [];

    private static readonly Queue<MapMarkerData> MarkerQueue = [];
    private const bool ReplaceNullOnClickWithId = true;

    public static void LoadSprites()
    {
        CreateSprite("normal", "APSR");
        CreateSprite("trap", "APSR_Trap");
        CreateSprite("progressive", "APSR_Progressive");
        CreateSprite("useful", "APSR_Useful");
        CreateSprite("got_trap", "APSR_Got_Trap");
        CreateSprite("fast_travel", "FastTravel");
        CreateSprite("mk1", "PodMk1");
        CreateSprite("mk2", "PodMk2");
        CreateSprite("mk3", "PodMk3");
        CreateSprite("cosmetic", "PodCosmetic");
        CreateSprite("gate", "Gate");
        CreateSprite("plort", "Plort");
        CreateSprite("gordo", "Gordo");
        CreateSprite("log", "Log");
        CreateSprite("map", "Map");
    }

    public static void LoadMapMarkers()
    {
        MakeTeleporterMarker(Home);

        while (MarkerQueue.Any())
        {
            var data = MarkerQueue.Dequeue();
            MakeMarker(data.Text, data.Coords, data.OnClick, data.Region);
        }
    }

    public static void CreateSprite(string key, string file, string fileType = "png")
    {
        var path = $"Mods/SW_CreeperKing.Slimipelago/Assets/Images/{file}.{fileType}";
        Texture2D texture = new(2, 2);
        if (!texture.LoadImage(File.ReadAllBytes(path)))
            throw new ArgumentException($"Error sprite not created: [{file}]");
        Spritemap[key] = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f));
    }

    public static Sprite CreateSprite(string file)
    {
        Texture2D texture = new(2, 2);
        if (!texture.LoadImage(File.ReadAllBytes(file)))
            throw new ArgumentException($"Error sprite not created: [{file}]");
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f));
    }

    public static string GetSpriteFromItemFlag(ItemFlags itemFlags)
    {
        if (itemFlags.HasFlag(ItemFlags.Advancement)) return "progressive";
        if (itemFlags.HasFlag(ItemFlags.NeverExclude)) return "useful";
        if (itemFlags.HasFlag(ItemFlags.Trap)) return "trap";
        return "normal";
    }

    public static ItemDisplayOnMap MakeTeleporterMarker(Vector3 pos)
    {
        var region = pos.y > 500 ? RegionSetId.DESERT : RegionSetId.HOME;
        return MakeMarker("fast_travel", pos, () => TeleportPlayer(pos, region), region: region);
    }

    public static ItemDisplayOnMap MakeMarker(string id, Vector3 position, UnityAction onPressed = null,
        RegionSetId region = RegionSetId.HOME)
    {
        if (region is not (RegionSetId.HOME or RegionSetId.DESERT)) return null;
        if (PlayerMap is null)
        {
            MarkerQueue.Enqueue(new MapMarkerData(id, position, onPressed, region));
            return null;
        }

        var map = PlayerMap.mapUI;
        var isRegionDesert = region == RegionSetId.DESERT;

        var cof = map.GetPrivateField<Vector4>($"{(isRegionDesert ? "desert" : "main")}Coefficients");
        var markerPosMin =
            map.GetPrivateField<Vector2>($"{(isRegionDesert ? "desert" : "world")}MarkerPositionMin");
        var markerPosMax =
            map.GetPrivateField<Vector2>($"{(isRegionDesert ? "desert" : "world")}MarkerPositionMax");

        var mapPos = map.CallPrivateMethod<Vector2>("GetMapPos", position, cof);
        var clampPos = map.CallPrivateMethod<Vector2>("GetMapPosClamped", position, cof, markerPosMin, markerPosMax);

        if (mapPos != clampPos) return null;
        var posHash = position.HashPos();

        if (ReplaceNullOnClickWithId && onPressed is null)
        {
            onPressed = () =>
            {
                if (ApSlimeClient.LocationDictionary.TryGetValue(posHash, out var itemName))
                {
                    PopupPatch.AddItemToQueue(new ApPopupData(Spritemap["got_trap"], "Marker Name", itemName,
                        timer: 0.1f));
                }

                Core.Log.Msg($"Marker id: \"{posHash}\"");
            };
        }

        GameObject gobj = new($"Archipelago Marker Display ({id})")
        {
            transform =
            {
                parent = PlayerInWorld.GetComponent<PlayerDisplayOnMap>().transform.parent,
            }
        };
        gobj.SetActive(false);
        // gobj.AddComponent<RegionMember>();

        var obj = gobj.AddComponent<ItemDisplayOnMap>();
        obj.Pos = mapPos;
        obj.OnPress = onPressed;
        // obj.SetPrivateField("regionSetId", region);
        obj.Region = region;
        gobj.SetActive(true);
        obj.Image.overrideSprite = Spritemap[id];

        MarkerDictionary[posHash] = obj;
        return obj;
    }

    public static void ChangeMarkerColor(string hash, Func<Color, Color> modify)
    {
        if (!MarkerDictionary.TryGetValue(hash, out var marker))
        {
            // Core.Log.Msg($"Marker for [{hash}] not found");
            return;
        }

        marker.Image.color = modify(marker.Image.color);
    }

    public static void ResetData() { MarkerDictionary.Clear(); }
}

public readonly struct MapMarkerData(
    string text,
    Vector3 coords,
    UnityAction onClick,
    RegionSetId region)
{
    public readonly string Text = text;
    public readonly Vector3 Coords = coords;
    public readonly UnityAction OnClick = onClick;
    public readonly RegionSetId Region = region;
}
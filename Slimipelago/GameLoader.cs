using MonomiPark.SlimeRancher.Regions;
using Slimipelago.Patches;
using UnityEngine;
using UnityEngine.Events;
using static Slimipelago.Patches.PlayerStatePatch;

namespace Slimipelago;

public static class GameLoader
{
    private static readonly Queue<MapMarkerData> MarkerQueue = [];
    public static readonly Vector3 Home = new(90.9811f, 14.7f, -140.8849f);
    public static Dictionary<string, Sprite> Spritemap = [];

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
        MakeMarker("fast_travel", Home, () => TeleportPlayer(Home));

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

    public static ItemDisplayOnMap MakeMarker(string id, Vector3 position, UnityAction onPressed = null,
        RegionRegistry.RegionSetId region = RegionRegistry.RegionSetId.HOME)
    {
        if (region is not (RegionRegistry.RegionSetId.HOME or RegionRegistry.RegionSetId.DESERT)) return null;
        if (PlayerMap is null)
        {
            MarkerQueue.Enqueue(new MapMarkerData(id, position, onPressed, region));
            return null;
        }

        var map = PlayerMap.mapUI;
        var isRegionDesert = region == RegionRegistry.RegionSetId.DESERT;

        var cof = map.GetPrivateField<Vector4>($"{(isRegionDesert ? "desert" : "main")}Coefficients");
        var markerPosMin =
            map.GetPrivateField<Vector2>($"{(isRegionDesert ? "desert" : "world")}MarkerPositionMin");
        var markerPosMax =
            map.GetPrivateField<Vector2>($"{(isRegionDesert ? "desert" : "world")}MarkerPositionMax");

        var mapPos = map.CallPrivateMethod<Vector2>("GetMapPos", position, cof);
        var clampPos = map.CallPrivateMethod<Vector2>("GetMapPosClamped", position, cof, markerPosMin, markerPosMax);

        if (mapPos != clampPos) return null;
        
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

        return obj;
    }
}

public readonly struct MapMarkerData(
    string text,
    Vector3 coords,
    UnityAction onClick,
    RegionRegistry.RegionSetId region)
{
    public readonly string Text = text;
    public readonly Vector3 Coords = coords;
    public readonly UnityAction OnClick = onClick;
    public readonly RegionRegistry.RegionSetId Region = region;
}
using MonomiPark.SlimeRancher.Regions;
using Slimipelago.Patches;
using UnityEngine;
using UnityEngine.Events;
using static Slimipelago.Patches.PlayerStatePatch;

namespace Slimipelago;

public static class GameLoader
{
    public static readonly Vector3 Home = new Vector3(90.9811f, 14.7f, -140.8849f);
    public static Dictionary<string, Sprite> Spritemap = [];

    public static void LoadSprites()
    {
        CreateSprite("normal", "APSR");
        CreateSprite("trap", "APSR_Trap");
        CreateSprite("progressive", "APSR_Progressive");
        CreateSprite("useful", "APSR_Useful");
        CreateSprite("got_trap", "APSR_Got_Trap");
        CreateSprite("fast_travel", "FastTravel");
    }
    
    public static void LoadMapMarkers()
    {
        MakeMarker("fast_travel", Home, () => TeleportPlayer(Home));
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
    
    public static void MakeMarker(string id, Vector3 position, UnityAction onPressed = null)
    {
        GameObject gobj = new($"Archipelago Marker Display ({id})")
        {
            transform =
            {
                parent = PlayerInWorld.GetComponent<PlayerDisplayOnMap>().transform.parent,
            }
        };
        gobj.SetActive(false);
        gobj.AddComponent<RegionMember>();

        var obj = gobj.AddComponent<ItemDisplayOnMap>();
        obj.Pos = position;
        obj.OnPress = onPressed;
        gobj.SetActive(true);
        obj.Image.overrideSprite = Spritemap[id];
    }
}
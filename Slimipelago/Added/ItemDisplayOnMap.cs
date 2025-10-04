using HarmonyLib;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;
using UnityEngine.UI;

namespace Slimipelago.Patches;

public class ItemDisplayOnMap : DisplayOnMap
{
    public Image Image;
    public MapMarker Marker;
    public ZoneDirector.Zone Zone = ZoneDirector.Zone.RANCH;
    public Vector3 Pos;

    public override void Awake()
    {
        Core.Log.Msg("Marker Wakeup");
        try
        {
            var mainMap = SRSingleton<Map>.Instance;
            // mainMap.OpenMap(ZoneDirector.Zone.NONE);
            
            SRSingleton<Map>.Instance.RegisterMarker(this);

            var gobj = new GameObject("archipelago marker");

            Image = gobj.AddComponent<Image>();
            var rect = gobj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(50, 50);

            Image.sprite = Core.Spritemap["normal"];
            Marker = gobj.AddComponent<ItemMapMarker>();
            Marker.gameObject.SetActive(true);

            var marker = GameObject.Find("HUD Root/Map/MapUI/UIContainer/Panel/Scroll View/Viewport/Content/Markers");
            gobj.transform.parent = marker.transform;

            var map = mainMap.mapUI;
            var isRegionDesert = base.GetRegionSetId() == RegionRegistry.RegionSetId.DESERT;

            var cof = map.GetPrivateField<Vector4>($"{(isRegionDesert ? "desert" : "main")}Coefficients");
            var markerPosMin =
                map.GetPrivateField<Vector2>($"{(isRegionDesert ? "desert" : "world")}MarkerPositionMin");
            var markerPosMax =
                map.GetPrivateField<Vector2>($"{(isRegionDesert ? "desert" : "world")}MarkerPositionMax");

            Pos = PlayerStatePatch.PlayerInWorld.transform.position;
            Core.Log.Msg(
                $"[{map is not null}], player: [{Pos}], [{cof}] : [{markerPosMin}]-[{markerPosMax}]");

            Pos = gobj.transform.localPosition = map.CallPrivateMethod<Vector2>("GetMapPosClamped",
                Pos, cof, markerPosMin, markerPosMax);
        }
        catch (Exception e)
        {
            Core.Log.Error(e);
        }
    }

    public override ZoneDirector.Zone GetZoneId() => Zone;
    public override bool ShowOnMap() => true;
    public override MapMarker GetMarker() => Marker;
    public override Vector3 GetCurrentPosition() => Pos;

    private void Update()
    {
        if (!Marker.gameObject.activeSelf) return;
        if (Marker.transform.localPosition == Pos) return;
        Marker.transform.localPosition = Pos;
        Marker.transform.localScale= Vector3.one;
    }
}
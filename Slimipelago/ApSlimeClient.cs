namespace Slimipelago;

public static class ApSlimeClient
{
    public static Dictionary<string, string> LocationDictionary = [];
    public static Dictionary<string, string> LocationInfoDictionary = [];
    public static HashSet<string> LocationsFound = [];

    public static string[] Zones =
    [
        "The Ranch",
        "The Lab",
        "The Overgrowth",
        "The Grotto",
        "Dry Reef",
        "Indigo Quarry",
        "Moss Blanket",
        "Ancient Ruins Transition",
        "Ancient Ruins",
        "Glass Desert",
// "The Slime Sea",
    ];

    public static string
        GameUUID = "ap_uuid_hf293j0wifkwj09hw0hafw"; // use newgameui.autoSaveDirector.DisplayNameAvailable(text))

    public static string PlayerName = "SW_Creeper_Slime";
}
// using Archipelago.MultiClient.Net.Enums;
// using CreepyUtil.Archipelago;
// using CreepyUtil.Archipelago.ApClient;

using System.Collections.Concurrent;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using CreepyUtil.Archipelago;
using CreepyUtil.Archipelago.ApClient;
using JetBrains.Annotations;
using Slimipelago.Patches.PlayerPatches;

namespace Slimipelago;

public static class ApSlimeClient
{
    public static Dictionary<string, string> LocationDictionary = [];
    public static Dictionary<string, string> LocationInfoDictionary = [];
    public static List<ItemInfo> Items = [];
    public static List<ItemInfo> ItemsWaiting = [];
    public static Dictionary<string, int> ItemCache = [];
    public static ApClient Client = new();

    public static string AddressPort = "archipelago.gg:12345";
    public static string Password = "";
    public static string SlotName = "Rancher1";

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

    public static string GameUUID = "";

    public static void Init()
    {
        if (File.Exists("ApConnection.txt"))
        {
            var fileText = File.ReadAllText("ApConnection.txt").Replace("\r", "").Split('\n');
            AddressPort = fileText[0];
            Password = fileText[1];
            SlotName = fileText[2];
        }

        Client.OnConnectionLost += () =>
        {
            PlayerStatePatch.SaveAndQuitButton?.onClick?.Invoke();
            Core.Log.Error("Lost Connection to Ap");
        };

        Client.OnConnectionEvent += _ =>
        {
            GameUUID = (string)Client.SlotData["uuid"];
        };

        Client.OnConnectionErrorReceived += (e, s) => { Core.Log.Error(e); };
    }
    
    [CanBeNull]
    public static string[] TryConnect(string addressPort, string password, string slotName)
    {
        var addressSplit = addressPort.Split(':');

        if (addressSplit.Length != 2) return ["Address Field is incorrect"];
        if (!int.TryParse(addressSplit[1], out var port)) return ["Port is incorrect"];

        var login = new LoginInfo(port, slotName, addressSplit[0], password);
        return Client.TryConnect(login, "Slime Rancher", ItemsHandlingFlags.AllItems);
    }

    public static void Update()
    {
        if (Client is null) return;
        Client.UpdateConnection();
        
        if (!Client.IsConnected) return;
        ItemsWaiting.AddRange(Client.GetOutstandingItems()!);
        
        if (PlayerModelPatch.Model is null) return;
        foreach (var item in ItemsWaiting)
        {
            ProcessItem(item);
        }

        Items.AddRange(ItemsWaiting);
        ItemsWaiting.Clear();
    }

    public static void SaveFile() => File.WriteAllText("ApConnection.txt", $"{AddressPort}\n{Password}\n{SlotName}");

    public static void WorldOpened()
    {
        ItemCache.Clear();
        foreach (var item in Items)
        {
            ProcessItem(item);
        }
    }

    public static void ProcessItem(ItemInfo item)
    {
        var name = item.ItemName;
        switch (name)
        {
            
        }
    }
}
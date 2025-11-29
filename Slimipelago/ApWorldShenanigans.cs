namespace Slimipelago;

public static class ApWorldShenanigans
{
    public static Dictionary<PlayerState.Upgrade, string> UpgradeLocations;

    public static void RunShenanigans()
    {
        var upgrades = NumberedText(i => $"Buy Personal Upgrade (Max Health lv.{i})", 4)
                      .Concat(NumberedText(i => $"Buy Personal Upgrade (Max Ammo lv.{i})", 4))
                      .Concat(NumberedText(i => $"Buy Personal Upgrade (Run Efficiency lv.{i})", 2))
                      .Concat(NumberedText(i => $"Buy Personal Upgrade (Max Energy lv.{i})", 3))
                      .Concat(NumberedText(i => $"Buy Personal Upgrade (Treasure Cracker lv.{i})", 3)) // require lab
                      .Concat([
                           "Buy Personal Upgrade (Jetpack)",
                           "Buy Personal Upgrade (Jetpack Efficiency)",
                           "Buy Personal Upgrade (Air Burst)",
                           "Buy Personal Upgrade (Liquid Slot)",
                           "Buy Personal Upgrade (Golden Sure Shot)"
                       ])
                      .ToArray();

        PlayerState.Upgrade[] upgradeTypeMap =
        [
            PlayerState.Upgrade.HEALTH_1, PlayerState.Upgrade.HEALTH_2, PlayerState.Upgrade.HEALTH_3,
            PlayerState.Upgrade.HEALTH_4,

            PlayerState.Upgrade.AMMO_1, PlayerState.Upgrade.AMMO_2, PlayerState.Upgrade.AMMO_3,
            PlayerState.Upgrade.AMMO_4,

            PlayerState.Upgrade.RUN_EFFICIENCY, PlayerState.Upgrade.RUN_EFFICIENCY_2,
            PlayerState.Upgrade.ENERGY_1, PlayerState.Upgrade.ENERGY_2, PlayerState.Upgrade.ENERGY_3,
            PlayerState.Upgrade.TREASURE_CRACKER_1, PlayerState.Upgrade.TREASURE_CRACKER_2,
            PlayerState.Upgrade.TREASURE_CRACKER_3,
            PlayerState.Upgrade.JETPACK, PlayerState.Upgrade.JETPACK_EFFICIENCY,
            PlayerState.Upgrade.AIR_BURST,
            PlayerState.Upgrade.LIQUID_SLOT,
            PlayerState.Upgrade.GOLDEN_SURESHOT
        ];

        UpgradeLocations = upgrades.Zip(upgradeTypeMap, (s, u) => (s, u)).ToDictionary(t => t.u, t => t.s);

        // downloaded from spreadsheet:
        // https://docs.google.com/spreadsheets/d/15PdrnGmkYdocX9RU-D5U_9OgihRNN9axX71mm-jOPUQ
        if (!File.Exists("Slimerancher - Sheet1.csv")) return;
        if (!Directory.Exists("output"))
        {
            Directory.CreateDirectory("output");
        }

        var csvRaw = File.ReadAllText("Slimerancher - Sheet1.csv");
        var csv = csvRaw
                 .Replace("\r", "")
                 .Split('\n')
                 .Skip(1)
                 .Select(s => s.Split(',').Skip(1).ToArray())
                 .Where(arr => arr.Length > 0)
                 .ToArray();

        var rawInteractables = csv.Where(arr => arr[0] != "" && ApSlimeClient.Zones.Contains(arr[2]));

        var interactables = rawInteractables
                           .Where(arr => arr[3] != "Secret Style")
                           .Select(arr => arr.Take(7).ToArray())
                           .ToArray();

        var dlcInteractables = rawInteractables
                              .Where(arr => arr[3] == "Secret Style")
                              .Select(arr => arr.Take(7).ToArray())
                              .ToArray();

        var gates = csv.Where(arr => arr[8] != "").Select(arr => arr.Skip(8).Take(5).ToArray()).ToArray();
        var gordos = csv.Where(arr => arr[14] != "").Select(arr => arr.Skip(14).ToArray()).ToArray();

        File.WriteAllText("Mods/SW_CreeperKing.Slimipelago/Data/Locations.txt",
            string.Join("\n",
                interactables.Select(arr => $"{arr[0]},{arr[1]},{arr[6]}")
                             .Concat(gates.Select(arr => $"{arr[0]},{arr[1]}"))
                             .Concat(gordos.Select(arr => $"{arr[0]},{arr[1]},Favorite: {arr[7]}"))));

        File.WriteAllText("output/Locations.py",
            $"""
             # File is Auto-generated, see: https://github.com/SWCreeperKing/Slimipelago/blob/master/Slimipelago/ApWorldShenanigans.cs

             upgrades = [
                 {string.Join(",\n\t", upgrades.Select(s => $"\"{s}\""))}
             ]

             interactables = [
                 {string.Join(",\n\t", interactables.Select(arr => $"[\"{arr[1]}\", \"{arr[2]}\"]"))}
             ]

             dlc_interactables = [
                 {string.Join(",\n\t", dlcInteractables.Select(arr => $"[\"{arr[1]}\", \"{arr[2]}\"]"))}
             ]

             location_dict = [
             	*[items for items in upgrades],
             	*[items[0] for items in interactables],
             	*[items[0] for items in dlc_interactables],
             ]
             """);

        File.WriteAllText("output/Rules.py",
            $$"""
              # File is Auto-generated, see: https://github.com/SWCreeperKing/Slimipelago/blob/master/Slimipelago/ApWorldShenanigans.cs

              def get_rule_map(player):
                  return {
                      {{string.Join(",\n\t\t", interactables.Concat(dlcInteractables).Select(arr => GenRule(arr[1], arr[3], arr[4], arr[5].Split(' ')[0])).Where(s => s != ""))}}
                  }

              def has_cracker(state, player, level) -> bool:
                  return state.has("Progressive Treasure Cracker", player, level)

              def has_energy(state, player, amount) -> bool:
                  return state.has("Progressive Max Energy", player, amount) 

              def has_jetpack(state, player) -> bool:
                  return state.has("Progressive Jetpack", player)
              """);

        if (File.Exists("output/Slimerancher - Sheet1.csv"))
        {
            File.Delete("output/Slimerancher - Sheet1.csv");
        }

        File.Move("Slimerancher - Sheet1.csv", "output/Slimerancher - Sheet1.csv");
        return;

        string GenRule(string location, string cracker, string needsJetpack, string energyNeeded)
        {
            List<string> rules = [];

            if (cracker.Contains("Treasure Cracker"))
            {
                rules.Add($"has_cracker(state, player, {Math.Min(1, cracker.Count(c => c == 'I'))})");
            }

            if (needsJetpack == "Yes")
            {
                rules.Add("has_jetpack(state, player)");
            }

            if (energyNeeded is not ("0" or "50" or "100"))
            {
                rules.Add(
                    $"has_energy(state, player, {(energyNeeded[0] == '1' ? 0 : 2) + (energyNeeded[1] == '0' ? 0 : 1)})");
            }

            return rules.Count == 0 ? "" : $"\"{location}\": lambda state: {string.Join(" and ", rules)}";
        }
    }

    public static string[] NumberedText(Func<int, string> numAction, int toNum, int fromNum = 1)
        => Enumerable.Range(fromNum, toNum - fromNum + 1).Select(numAction).ToArray();
}
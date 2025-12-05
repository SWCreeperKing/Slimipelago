using Slimipzelago.Archipelago;

namespace Slimipelago.Archipelago;

public static class ApWorldShenanigans
{
    public static void RunShenanigans()
    {
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
                 .Where(arr => arr.Length > 0)
                 .Select(s => new CsvLine(s.Split(',').Skip(1).ToArray()))
                 .ToArray();

        var rawInteractables = csv.Where(line => line.HasInteractable && line.IsValidZone).ToArray();
        var interactables = rawInteractables.Where(line => !line.IsSecretStyle).ToArray();
        var dlcInteractables = rawInteractables.Where(line => line.IsSecretStyle).ToArray();

        var gates = csv.Where(line => line.HasGate).ToArray();
        var gordos = csv.Where(line => line.HasGordo).ToArray();

        var upgrades = csv.Where(line => line.HasUpgrade).ToArray();
        var upgradeRules = upgrades.Where(line => line.UpgradeRules.Length > 0)
                                   .ToDictionary(line => line.UpgradeName, line => line.UpgradeRules);

        File.WriteAllText("Mods/SW_CreeperKing.Slimipelago/Data/Locations.txt",
            string.Join("\n",
                interactables.Select(line => line.GetInteractableText)
                             .Concat(dlcInteractables.Select(line => line.GetInteractableText))
                             .Concat(gates.Select(line => line.GetGateText))
                             .Concat(gordos.Select(line => line.GetGordoText))));

        File.WriteAllText("Mods/SW_CreeperKing.Slimipelago/Data/Upgrades.txt",
            string.Join("\n", upgrades.Select(line => $"{line.UpgradeName},{line.UpgradeId}")));

        File.WriteAllText("output/Locations.py",
            $"""
             # File is Auto-generated, see: https://github.com/SWCreeperKing/Slimipelago/blob/master/Slimipelago/ApWorldShenanigans.cs

             upgrades = [
                 {string.Join(",\n\t", upgrades.Select(line => $"\"{line.UpgradeName}\""))}
             ]

             upgrades_7z = [
                 {string.Join(",\n\t", upgradeRules.Where(kv => kv.Value.Contains("7z")).Select(kv => $"\"{kv.Key}\""))}
             ]

             interactables = [
                 {string.Join(",\n\t", interactables.Select(line => $"[\"{line.InteractableName}\", \"{line.InteractableArea}\"]"))}
             ]

             dlc_interactables = [
                 {string.Join(",\n\t", dlcInteractables.Select(line => $"[\"{line.InteractableName}\", \"{line.InteractableArea}\"]"))}
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

              def get_rule_map(player, world):
                  return {
                      {{string.Join(",\n\t\t", interactables.Concat(dlcInteractables).Select(line => GenRule(line.InteractableName, line.InteractableCrackerLevel, line.InteractableJetpackRequirement, line.InteractableMinJetpackEnergy.Split(' ')[0])).Where(s => s != ""))}},
                      {{string.Join(",\n\t\t", upgradeRules.Where(kv => !kv.Value.Contains("7z")).Select(kv => $"\"{kv.Key}\": lambda state: {string.Join(" and ", kv.Value.Select(GenItemRule))}"))}},
                      {{string.Join(",\n\t\t", upgradeRules.Where(kv => kv.Value.Contains("7z")).Select(kv => $"\"{kv.Key}\": lambda state: has_region(state, player, \"Indigo Quarry\")"))}},
                  }

              def has_cracker(state, player, level) -> bool:
                  return state.has("Progressive Treasure Cracker", player, level)

              def has_energy(state, player, amount) -> bool:
                  return state.has("Progressive Max Energy", player, amount) 

              def has_jetpack(state, player) -> bool:
                  return state.has("Progressive Jetpack", player)
                  
              def has_region(state, player, region) -> bool:
                  return state.has(f"Region Unlock: {region}", player) 
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
                rules.Add($"has_cracker(state, player, {Math.Max(1, cracker.Count(c => c == 'I'))})");
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

        string GenItemRule(string rule)
        {
            return rule.StartsWith("Region ") ? $"has_region(state, player, \"{rule.Substring(7)}\")" : "";
        }
    }
}

public readonly struct CsvLine(string[] line)
{
    public readonly string InteractableId = line[0];
    public readonly string InteractableName = line[1];
    public readonly string InteractableArea = line[2];
    public readonly string InteractableCrackerLevel = line[3];
    public readonly string InteractableJetpackRequirement = line[4];
    public readonly string InteractableMinJetpackEnergy = line[5];
    public readonly string InteractableSummary = line[6];

    public readonly string GateId = line[8];
    public readonly string GateName = line[9];
    public readonly string GateFromArea = line[10];
    public readonly string GateToArea = line[11];
    public readonly string GateSkippableWithJetpack = line[12];

    public readonly string GordoId = line[14];
    public readonly string GordoName = line[15];
    public readonly string GordoArea = line[16];
    public readonly string GordoContents = line[17];
    public readonly string GordoTeleporterLocation = line[18];
    public readonly string GordoJetpackRequirement = line[19];
    public readonly string GordoNormalFoodRequirement = line[20];
    public readonly string GordoFavoriteFood = line[21];

    public readonly string UpgradeName = line[23];
    public readonly string UpgradeId = line[24];
    public readonly string[] UpgradeRules = line[25].Split(['&'], StringSplitOptions.RemoveEmptyEntries).Select(rule => rule.Trim()).ToArray();

    public bool HasInteractable => InteractableId != "";
    public bool HasGate => GateId != "";
    public bool HasGordo => GordoId != "";
    public bool HasUpgrade => UpgradeName != "";

    public bool IsValidZone => ApSlimeClient.Zones.Contains(InteractableArea);
    public bool IsSecretStyle => InteractableCrackerLevel != "Secret Style";

    public string GetInteractableText => $"{InteractableId},{InteractableName},{InteractableSummary}";
    public string GetGateText => $"{GateId},{GateName}";
    public string GetGordoText => $"{GordoId},{GordoName},Favorite: {GordoFavoriteFood}";
}
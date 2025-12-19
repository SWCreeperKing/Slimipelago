namespace Slimipelago.Archipelago;

// 1-2: nothing
// 3-10: dry reef
// 11-15: moss or indigo
// 16-19: moss AND indigo
// 20-23: ancient ruins
// 24-28: glass
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
                 .Select((s, i) =>
                  {
                      try
                      {
                          return new CsvLine(s.Split(',').Skip(1).ToArray());
                      }
                      catch (Exception e)
                      {
                          Core.Log.Msg($"=== ERROR ON CSV LINE [{i}] ===");
                          Core.Log.Error(e);
                          throw e;
                      }
                  })
                 .ToArray();

        var rawInteractables = csv.Where(line => line.HasInteractable && line.IsValidZone).ToArray();
        var interactables = rawInteractables.Where(line => !line.IsSecretStyle).ToArray();
        var dlcInteractables = rawInteractables.Where(line => line.IsSecretStyle).ToArray();

        var gates = csv.Where(line => line.HasGate).ToArray();
        var gordos = csv.Where(line => line.HasGordo).ToArray();

        var upgrades = csv.Where(line => line.HasUpgrade).ToArray();
        var upgradeRules = upgrades.Where(line => line.UpgradeRules.Length > 0)
                                   .ToDictionary(line => line.UpgradeName, line => line.UpgradeRules);

        var corporateLocations = csv.Where(line => line.HasCorporate).ToArray();

        File.WriteAllText("Mods/SW_CreeperKing.Slimipelago/Data/Locations.txt",
            string.Join("\n",
                interactables.Select(line => line.GetInteractableText)
                             .Concat(dlcInteractables.Select(line => line.GetInteractableText))
                             .Concat(gates.Select(line => line.GetGateText))
                             .Concat(gordos.Select(line => line.GetGordoText))));

        File.WriteAllText("Mods/SW_CreeperKing.Slimipelago/Data/Upgrades.txt",
            string.Join("\n", upgrades.Select(line => $"{line.UpgradeName},{line.UpgradeId}")));

        File.WriteAllLines("Mods/SW_CreeperKing.Slimipelago/Data/7Zee.txt",
            corporateLocations.Select(line => $"{line.CorporateLocation},{line.CorporateLevel}"));
        
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

             corporate_locations = [
                 {string.Join(",\n\t", corporateLocations.Select(line => $"[\"{line.CorporateLocation}\", \"{line.CorporateArea}\"]"))}
             ]

             location_dict = [
             	*[items for items in upgrades],
             	*[items[0] for items in interactables],
             	*[items[0] for items in dlc_interactables],
             	*[items[0] for items in corporate_locations],
             ]
             """);

        File.WriteAllText("output/Rules.py",
            $$"""
              # File is Auto-generated, see: https://github.com/SWCreeperKing/Slimipelago/blob/master/Slimipelago/ApWorldShenanigans.cs

              def get_rule_map(player, world):
                  return {
                      {{string.Join(",\n\t\t", interactables.Concat(dlcInteractables).Select(line => GenRule(line.InteractableName, line.InteractableCrackerLevel, line.InteractableJetpackRequirement, line.InteractableMinJetpackEnergy.Split(' ')[0], true)).Where(s => s != ""))}},
                      {{string.Join(",\n\t\t", upgradeRules.Select(kv => $"\"{kv.Key}\": lambda state: {string.Join(" and ", kv.Value.Select(GenItemRule).Where(s => s.Trim() != ""))}"))}},
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

        File.WriteAllText("Mods/SW_CreeperKing.Slimipelago/Data/Logic.txt",
            string.Join("\n",
                interactables.Concat(dlcInteractables)
                             .Select(line
                                  => $"{line.InteractableName}:{GenRule(line.InteractableName, line.InteractableCrackerLevel, line.InteractableJetpackRequirement, line.InteractableMinJetpackEnergy.Split(' ')[0], false)}:{line.InteractableArea}")
                             .Where(s => s != "")));

        if (File.Exists("output/Slimerancher - Sheet1.csv"))
        {
            File.Delete("output/Slimerancher - Sheet1.csv");
        }

        File.Move("Slimerancher - Sheet1.csv", "output/Slimerancher - Sheet1.csv");
        return;

        string GenRule(string location, string cracker, string needsJetpack, string energyNeeded, bool isPython)
        {
            List<string> rules = [];

            if (cracker.Contains("Treasure Cracker"))
            {
                var level = Math.Max(1, cracker.Count(c => c == 'I'));
                rules.Add(isPython
                    ? $"has_cracker(state, player, {level})"
                    : string.Join("", Enumerable.Repeat('c', level)));
            }

            if (needsJetpack == "Yes")
            {
                rules.Add(isPython ? "has_jetpack(state, player)" : "j");
            }

            if (energyNeeded is not ("0" or "50" or "100"))
            {
                var energyLevel = (energyNeeded[0] == '1' ? 0 : 2) + (energyNeeded[1] == '0' ? 0 : 1);
                rules.Add(isPython
                    ? $"has_energy(state, player, {energyLevel})"
                    : string.Join("", Enumerable.Repeat('e', energyLevel)));
            }

            return rules.Count == 0 ? "" :
                isPython ? $"\"{location}\": lambda state: {string.Join(" and ", rules)}" : string.Join("", rules);
        }

        string GenItemRule(string rule)
            => rule.StartsWith("Region ") ? $"has_region(state, player, \"{rule.Substring(7)}\")" : "";
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

    public readonly string[] UpgradeRules =
        line[25].Split(['&'], StringSplitOptions.RemoveEmptyEntries).Select(rule => rule.Trim()).ToArray();

    public readonly string CorporateLocation = line[27].Trim();
    public readonly int CorporateLevel = line[27] != "" ? int.Parse(line[27].Split(':')[0].Split('.')[1]) : -1;
    public readonly string CorporatePrice = line[28];
    public readonly string CorporateArea = line[29];
    
    public bool HasInteractable => InteractableId != "";
    public bool HasGate => GateId != "";
    public bool HasGordo => GordoId != "";
    public bool HasUpgrade => UpgradeName != "";
    public bool HasCorporate => CorporateLocation != "";
    public bool IsValidZone => ApSlimeClient.Zones.Contains(InteractableArea);
    public bool IsSecretStyle => InteractableCrackerLevel == "Secret Style";
    public string GetInteractableText => $"{InteractableId},{InteractableName},{InteractableSummary}";
    public string GetGateText => $"{GateId},{GateName}";
    public string GetGordoText => $"{GordoId},{GordoName},Favorite: {GordoFavoriteFood}";
}
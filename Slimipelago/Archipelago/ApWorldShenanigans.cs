using CreepyUtil.Archipelago;

namespace Slimipelago.Archipelago;

// 1-2: nothing
// 3-10: dry reef
// 11-15: moss or indigo
// 16-19: moss AND indigo
// 20-23: ancient ruins
// 24-28: glass
public static class ApWorldShenanigans
{
    public const string CsvFile = "Slimerancher - Checkables.csv";
    public const string DataFolder = "Mods/SW_CreeperKing.Slimipelago/Data";

    public static void RunShenanigans()
    {
        // downloaded from spreadsheet:
        // https://docs.google.com/spreadsheets/d/15PdrnGmkYdocX9RU-D5U_9OgihRNN9axX71mm-jOPUQ
        if (!File.Exists(CsvFile)) return;
        if (!Directory.Exists("output"))
        {
            Directory.CreateDirectory("output");
        }

        new CsvParser(CsvFile, 1, 1)
           .ToFactory((e, i) =>
            {
                Core.Log.Msg($"=== ERROR ON CSV LINE [{i}] ===");
                Core.Log.Error(e);
            })
           .ReadTable(new InteractableCreator(), 7, out var rawInteractables).SkipColumn()
           .ReadTable(new GateCreator(), 5, out var gates).SkipColumn()
           .ReadTable(new GordoCreator(), 8, out var gordos).SkipColumn()
           .ReadTable(new UpgradeCreator(), 3, out var upgrades).SkipColumn()
           .ReadTable(new CorporateCreator(), 3, out var corporateLocations);

        var interactables = rawInteractables.Where(line => !line.IsSecretStyle).ToArray();
        var dlcInteractables = rawInteractables.Where(line => line.IsSecretStyle).ToArray();

        var noteLocations = File.Exists($"{DataFolder}/NoteLocations.txt") ? File.ReadAllText($"{DataFolder}/NoteLocations.txt").Replace("\r", "").Split('\n').ToList() : [];
        noteLocations.AddRange(interactables.Where(inter => inter.IsNote && !noteLocations.Contains(inter.Id)).Select(inter => inter.Id));

        File.WriteAllText($"{DataFolder}/Locations.txt",
            string.Join("\n",
                interactables.Select(line => line.GetText)
                             .Concat(dlcInteractables.Select(line => line.GetText))
                             .Concat(gates.Select(line => line.GetText))
                             .Concat(gordos.Select(line => line.GetText))));

        File.WriteAllText($"{DataFolder}/Upgrades.txt",
            string.Join("\n", upgrades.Select(line => $"{line.Name},{line.Id}")));

        File.WriteAllLines($"{DataFolder}/7Zee.txt",
            corporateLocations.Select(line => $"{line.Location},{line.Level}"));

        WorldFactory worldFactory = new()
        {
            LocationGeneratorLink = "https://github.com/SWCreeperKing/Slimipelago/blob/master/Slimipelago/ApWorldShenanigans.cs",
            LogicGeneratorLink = "https://github.com/SWCreeperKing/Slimipelago/blob/master/Slimipelago/ApWorldShenanigans.cs",
        };

        worldFactory
           .AddLocations("upgrades", upgrades.Select(line => line.Name))
           .AddLocations("upgrades_7z", upgrades.Where(up => up.Rules.Contains("7z")).Select(up => up.Name), addToFinalList: false)
           .AddLocations("interactables", interactables.Select(line => (string[])[line.Name, line.Area]))
           .AddLocations("dlc_interactables", dlcInteractables.Select(line => (string[])[line.Name, line.Area]))
           .AddLocations("corporate_locations", corporateLocations.Select(line => (string[])[line.Location, line.Area]))
           .GenerateLocationFile("output/Locations.py");

        worldFactory
           .SetOnCompilerError((e, s) => Core.Log.Error(s, e))
           .AddLogicFunction("cracker", "has_cracker", "return state.has(\"Progressive Treasure Cracker\", player, level)", "level")
           .AddLogicFunction("energy", "has_energy", "return state.has(\"Progressive Max Energy\", player, amount)", "amount")
           .AddLogicFunction("jetpack", "has_jetpack", "return state.has(\"Progressive Jetpack\", player)")
           .AddLogicFunction("region", "has_region", "return state.has(f\"Region Unlock: {region}\", player)", "region")
           .AddLogicRules(rawInteractables.ToDictionary(inter => inter.Name, inter => inter.GenRule(true)))
           .AddLogicRules(upgrades.ToDictionary(up => up.Name, up => up.GenRule()))
           .GenerateRulesFile("output/Rules.py");

        File.WriteAllText($"{DataFolder}/Logic.txt",
            string.Join("\n",
                interactables.Concat(dlcInteractables)
                             .Select(line => $"{line.Name}:{line.GenRule(false)}:{line.Area}")
                             .Where(s => s != "")));

        if (File.Exists($"output/{CsvFile}"))
        {
            File.Delete($"output/{CsvFile}");
        }

        File.Move(CsvFile, $"output/{CsvFile}");
    }
}

file readonly struct InteractableRowData(string[] line)
{
    public readonly string Id = line[0];
    public readonly string Name = line[1].Trim();
    public readonly string Area = line[2];
    public readonly string CrackerLevel = line[3].Trim();
    public readonly bool NeedsJetpack = line[4] == "Yes";
    public readonly string MinJetpackEnergy = line[5].Split(' ')[0];
    public readonly string Summary = line[6];
    public bool IsSecretStyle => CrackerLevel == "Secret Style";
    public bool IsNote => CrackerLevel == "Note";
    public string GetText => $"{Id},{Name},{Summary}";

    public string GenRule(bool forCompiler)
    {
        List<string> rules = [];

        if (CrackerLevel.Contains("Treasure Cracker"))
        {
            var level = Math.Max(1, CrackerLevel.Count(c => c == 'I'));
            rules.Add(forCompiler ? $"cracker[{level}]" : string.Join("", Enumerable.Repeat('c', level)));
        }

        if (NeedsJetpack)
        {
            rules.Add(forCompiler ? "jetpack" : "j");
        }

        if (MinJetpackEnergy is not ("0" or "50" or "100"))
        {
            var energyLevel = (MinJetpackEnergy[0] == '1' ? 0 : 2) + (MinJetpackEnergy[1] == '0' ? 0 : 1);
            rules.Add(forCompiler ? $"energy[{energyLevel}]" : string.Join("", Enumerable.Repeat('e', energyLevel)));
        }

        return forCompiler ? string.Join(" and ", rules) : string.Join("", rules);
    }
}

file readonly struct GateRowData(string[] line)
{
    public readonly string Id = line[0];
    public readonly string Name = line[1];
    public readonly string FromArea = line[2];
    public readonly string ToArea = line[3];
    public readonly string SkippableWithJetpack = line[4];
    public string GetText => $"{Id},{Name}";
}

file readonly struct GordoRowData(string[] line)
{
    public readonly string Id = line[0];
    public readonly string Name = line[1];
    public readonly string Area = line[2];
    public readonly string Contents = line[3];
    public readonly string TeleporterLocation = line[4];
    public readonly string JetpackRequirement = line[5];
    public readonly string NormalFoodRequirement = line[6];
    public readonly string FavoriteFood = line[7];
    public string GetText => $"{Id},{Name},Favorite: {FavoriteFood}";
}

file readonly struct UpgradeRowData(string[] line)
{
    public readonly string Name = line[0];
    public readonly string Id = line[1];

    public readonly string[] Rules =
        line[2].Split(['&'], StringSplitOptions.RemoveEmptyEntries).Select(rule => rule.Trim()).ToArray();

    public string GenRule()
        => string.Join(" and ", Rules.Where(rule => rule is not "7z").Select(rule => $"region[\"{rule}\"]"));
}

file readonly struct CorporateRowData(string[] line)
{
    public readonly string Location = line[0].Trim();
    public readonly int Level = line[0] != "" ? int.Parse(line[0].Split(':')[0].Split('.')[1]) : -1;
    public readonly string Price = line[1];
    public readonly string Area = line[2];
}

file class InteractableCreator : CsvTableRowCreator<InteractableRowData>
{
    public override InteractableRowData CreateRowData(string[] param) => new(param);
    public override bool IsValidData(InteractableRowData t) => t.Id != "" && ZoneConstants.Zones.Contains(t.Area);
}

file class GateCreator : CsvTableRowCreator<GateRowData>
{
    public override GateRowData CreateRowData(string[] param) => new(param);
    public override bool IsValidData(GateRowData t) => t.Id != "";
}

file class GordoCreator : CsvTableRowCreator<GordoRowData>
{
    public override GordoRowData CreateRowData(string[] param) => new(param);
    public override bool IsValidData(GordoRowData t) => t.Id != "";
}

file class UpgradeCreator : CsvTableRowCreator<UpgradeRowData>
{
    public override UpgradeRowData CreateRowData(string[] param) => new(param);
    public override bool IsValidData(UpgradeRowData t) => t.Name != "";
}

file class CorporateCreator : CsvTableRowCreator<CorporateRowData>
{
    public override CorporateRowData CreateRowData(string[] param) => new(param);
    public override bool IsValidData(CorporateRowData t) => t.Location != "";
}
namespace Slimipelago;

public static class ApWorldShenanigans
{
    public static void RunShenanigans()
    {
        // download from spreadsheet:
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

        var interactables = csv.Where(arr => arr[0] != "" && arr[3] != "Secret Style")
                               .Select(arr => arr.Take(7).ToArray())
                               .ToArray();
        var dlcInteractables = csv.Where(arr => arr[0] != "" && arr[3] == "Secret Style")
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
             # Auto-generated
             interactables = [
                 {string.Join(",\n\t", interactables.Select(arr => $"[{string.Join(", ", arr.Skip(1).Take(5).Select(s => $"\"{s}\""))}]"))}
             ]

             dlc_interactables = [
                 {string.Join(",\n\t", dlcInteractables.Select(arr => $"[{string.Join(", ", arr.Skip(1).Take(5).Select(s => $"\"{s}\""))}]"))}
             ]

             gates = [
                 {string.Join(",\n\t", gates.Select(arr => $"[{string.Join(", ", arr.Skip(1).Select(s => $"\"{s}\""))}]"))}
             ]

             gordos = [
                 {string.Join(",\n\t", gordos.Select(arr => $"[{string.Join(", ", arr.Skip(1).Take(5).Select(s => $"\"{s}\""))}]"))}
             ]

             location_dict = [
             	*[items[0] for items in interactables],
             	*[items[0] for items in dlc_interactables],
             	# *[items[0] for items in gates],
             	# *[items[0] for items in gordos],
             ]
             """);
    }
}
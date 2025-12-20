using System.Reflection;

namespace Slimipelago.Archipelago;

public static class TrapLoader
{
    public enum Trap
    {
        Whoops, // Whoops!
        Home, // send home
        Text, // hobson note
        Tarr, // spawn tarr
    }

    public static Dictionary<string, Action> Traps = [];
    public static List<TrapAttribute> TrapAttributes = [];

    public static void Init()
    {
        var asm = Assembly.GetExecutingAssembly();
        var types = asm.GetTypes();
        var hints = types.Where(t => t.GetCustomAttributes<TrapHintAttribute>().Any()).ToArray();

        foreach (var hint in hints)
        {
            var methods = hint.GetMethods()
                              .Where(m => m.GetCustomAttributes<TrapAttribute>().Any())
                              .Select(m => (method: m, trap: m.GetCustomAttribute<TrapAttribute>()))
                              .ToArray();

            foreach (var method in methods)
            {
                Action action = () => method.method.Invoke(null, []);
                TrapAttributes.Add(method.trap);

                foreach (var trapName in method.trap.TrapNames)
                {
                    Traps[trapName] = action;
                }
            }
        }
        
        if (!Directory.Exists("mod dev/Slimipelago")) return;
        if (File.Exists("mod dev/Slimipelago/Traps.md"))
        {
            File.Delete("mod dev/Slimipelago/Traps.md");
        }

        File.WriteAllText("mod dev/Slimipelago/Traps.md",
            $"""
             | Trap | Description | Acceptable from TrapLink |
             |:-----|:-----------:|:------------------------|
             {string.Join("\n", TrapAttributes.Select(att => string.Join("|", att.TrapNames[0], att.Description, string.Join("<br>", att.TrapNames.OrderBy(s => s)))))}
             """);
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class TrapHintAttribute : Attribute;

[AttributeUsage(AttributeTargets.Method)]
public class TrapAttribute(TrapLoader.Trap trap, string[] trapName, string description) : Attribute
{
    public readonly TrapLoader.Trap Trap = trap;
    public readonly string[] TrapNames = trapName;
    public readonly string Description = description;
}
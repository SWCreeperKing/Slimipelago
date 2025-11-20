using Slimipelago.Patches.PlayerPatches;

namespace Slimipelago.Added;

public static class Playground
{
    public static Task TrapReset;
    
    public static void Woops()
    {
        if (TrapReset is not null) return;
        try
        {
            var beforePos = PlayerStatePatch.PlayerInWorld.transform.position;
            var pos = PlayerStatePatch.PlayerInWorld.transform.position;
            pos.y += 300;
            PlayerStatePatch.PlayerInWorld.transform.position = pos;
            TrapReset = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(5000);
                    PlayerStatePatch.PlayerInWorld.transform.position = beforePos;
                    TrapReset = null;
                }
                catch (Exception e)
                {
                    Core.Log.Error(e);
                }
            });
        }
        catch (Exception e)
        {
            Core.Log.Error(e);
        }
    }

    public static void BanishPlayer() => PlayerStatePatch.TeleportPlayer(GameLoader.Home);
}
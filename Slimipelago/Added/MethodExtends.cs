namespace Slimipelago.Patches;

public static class MethodExtends
{
    public static void Lock(this PlayerState.UpgradeLocker locker)
    {
        locker.SetPrivateField("timeLock", false);
        locker.SetPrivateField("lockedUntil", 0);
    }
}
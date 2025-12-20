using KaitoKid.Utilities.Interfaces;

namespace Slimipelago.Archipelago;

public class Logger : ILogger
{
    public void LogError(string message) => Core.Log?.Error(message);
    public void LogError(string message, Exception e) => Core.Log?.Error(message, e);
    public void LogWarning(string message) => Core.Log?.Warning(message);
    public void LogInfo(string message) => Core.Log?.Msg(message);
    public void LogMessage(string message) => Core.Log?.Msg(message);
    public void LogDebug(string message) => Core.Log?.Msg(message);

    public void LogDebugPatchIsRunning(string patchedType, string patchedMethod, string patchType, string patchMethod,
        params object[] arguments)
        => Core.Log?.Msg($"Debug Patch: [{patchedMethod}] -> [{patchMethod}]");

    public void LogDebug(string message, params object[] arguments) => Core.Log?.Msg(message);
    public void LogErrorException(string prefixMessage, Exception ex, params object[] arguments) => Core.Log?.Error(ex);

    public void LogWarningException(string prefixMessage, Exception ex, params object[] arguments)
        => Core.Log?.Error(ex);

    public void LogErrorException(Exception ex, params object[] arguments) => Core.Log?.Error(ex);
    public void LogWarningException(Exception ex, params object[] arguments) => Core.Log?.Error(ex);
    public void LogErrorMessage(string message, params object[] arguments) => Core.Log?.Error(message);

    public void LogErrorException(string patchType, string patchMethod, Exception ex, params object[] arguments)
        => Core.Log?.Error(ex);
}
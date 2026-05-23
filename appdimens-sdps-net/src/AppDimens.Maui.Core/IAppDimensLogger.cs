namespace AppDimens.Maui.Core;

/// <summary>Application-provided logging for AppDimens runtime diagnostics.</summary>
public interface IAppDimensLogger
{
    void LogDebug(string message);
    void LogWarning(string message);
}

public sealed class DebugAppDimensLogger : IAppDimensLogger
{
    public void LogDebug(string message) => System.Diagnostics.Debug.WriteLine($"[AppDimens] {message}");
    public void LogWarning(string message) => System.Diagnostics.Debug.WriteLine($"[AppDimens:WARN] {message}");
}

public static class AppDimensLogging
{
    public static IAppDimensLogger? Current { get; set; }

    public static void Debug(string message) => Current?.LogDebug(message);
    public static void Warning(string message) => Current?.LogWarning(message);
}

using AppDimens.Net.Core;

namespace AppDimens.Net;

/// <summary>
/// Ambient window context — the .NET counterpart of Compose's <c>LocalDimenMetrics</c>.
/// The MAUI layer assigns <see cref="Current"/> once per live window; every parameterless
/// extension (<c>16.Sdp()</c>) resolves coherently through the cache fast lane and
/// self-heals on resize via the event-driven watcher.
/// </summary>
public static class AppDimensAmbient
{
    private static volatile IAppDimensContext? _current;

    public static IAppDimensContext? Current => _current;

    public static void Set(IAppDimensContext? context) => _current = context;

    public static IAppDimensContext Require() =>
        _current ?? throw new InvalidOperationException(
            "AppDimens ambient context is not initialized. Call AppDimensMaui.Init(this.CreateAppDimensScope()) in MauiProgram, or pass an IAppDimensContext explicitly.");
}

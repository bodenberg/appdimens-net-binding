using AppDimens.Net.Core;

namespace AppDimens.Net.Maui.Platform;

/// <summary>
/// Bootstrap for .NET MAUI apps: wires the ambient context, initializes the cache and
/// installs the event-driven resize watcher. Call once in <c>MauiProgram</c> after
/// building the <c>Application</c>, or attach per-window with <see cref="AttachWindow"/>.
/// </summary>
public static class AppDimensMaui
{
    private static WindowDimensScope? _primaryScope;

    /// <summary>
    /// Initializes AppDimens against the main window. Safe to call multiple times;
    /// later calls replace the ambient scope.
    /// </summary>
    public static WindowDimensScope AttachWindow(Window window, Func<bool>? multiWindowProbe = null)
    {
        var scope = new WindowDimensScope(window, multiWindowProbe);
        DimenCache.Init(scope);
        AppDimensAmbient.Set(scope);
        _primaryScope = scope;
        return scope;
    }

    public static void Detach()
    {
        _primaryScope?.Dispose();
        _primaryScope = null;
        AppDimensAmbient.Set(null);
    }

    public static WindowDimensScope? Primary => _primaryScope;

    /// <summary>Convenience overload: attach inside <c>Application.OnStart</c>.</summary>
    public static void Init(Application app)
    {
        var window = app?.Windows.FirstOrDefault();
        if (window is not null) AttachWindow(window);
    }
}

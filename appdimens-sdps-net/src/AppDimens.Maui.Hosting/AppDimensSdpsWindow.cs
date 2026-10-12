using AppDimens.Maui.Core;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;

namespace AppDimens.Maui;

/// <summary>
/// Window-bound resize watcher: keeps every live (non-<c>i</c>) value in sync with the
/// actual window size — desktop windows, split-screen and foldables — while independent
/// (<c>*i</c>) values remain frozen against the baseline.
/// Attach once after the <see cref="Window"/> is created (e.g. in
/// <c>Application.CreateWindow</c>); detach on close to release handlers.
/// </summary>
public static class AppDimensSdpsWindow
{
    private static Window? _attached;

    /// <summary>The currently watched window, if any.</summary>
    public static Window? Attached => _attached;

    /// <summary>
    /// Starts tracking a window. Safe to call multiple times; a new window replaces the
    /// previous one. Values refresh on <c>SizeChanged</c> and on display changes.
    /// </summary>
    public static void Attach(Window window)
    {
        if (window is null) return;
        if (ReferenceEquals(_attached, window)) return;
        Detach();

        _attached = window;
        window.SizeChanged += OnWindowSizeChanged;
        window.Destroying += OnWindowDestroying;
        PushBounds();
    }

    /// <summary>Stops tracking and restores display-based metrics.</summary>
    public static void Detach()
    {
        var window = _attached;
        _attached = null;
        if (window is null) return;
        window.SizeChanged -= OnWindowSizeChanged;
        window.Destroying -= OnWindowDestroying;
        AppDimensResolver.Instance.UntrackWindowBounds();
    }

    /// <summary>Forces a metrics refresh from the attached window bounds.</summary>
    public static void Refresh()
    {
        if (_attached is not null) PushBounds();
        else AppDimensResolver.Instance.RefreshMetricsFromDevice();
    }

    private static void OnWindowSizeChanged(object? sender, EventArgs e) => PushBounds();

    private static void OnWindowDestroying(object? sender, EventArgs e) => Detach();

    private static void PushBounds()
    {
        var window = _attached;
        if (window is null) return;
        var w = window.Width;
        var h = window.Height;
        if (!(w > 0) || !(h > 0)) return;

        // MAUI Window.Width/Height are already device-independent units (dp-like).
        AppDimensResolver.Instance.TrackWindowBounds(w, h);
        AppDimensLogging.Debug($"window bounds {w:0.#}x{h:0.#} dp applied");
    }
}

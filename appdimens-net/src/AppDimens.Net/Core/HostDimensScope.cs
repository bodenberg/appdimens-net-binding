using AppDimens.Net.Common;

namespace AppDimens.Net.Core;

/// <summary>
/// Reusable live window context for any UI host (WPF, Windows Forms, WinUI 3, Avalonia,
/// game loops…). Push the window size with <see cref="Update"/> from your resize handler;
/// listeners (cache, markup extensions) are notified only when something actually changed.
/// </summary>
/// <remarks>Sizes are device-independent units (dp / DIP); <c>density</c> is pixels per dp.</remarks>
public class HostDimensScope : IAppDimensContext, IDisposable
{
    private readonly List<Action> _listeners = [];
    private readonly object _lock = new();
    private float _density = 1f;
    private bool _disposed;

    public ScreenConfiguration Configuration { get; private set; } = ScreenConfiguration.Default;

    public float Density => _density;

    public float Xdpi => 160f * _density;

    /// <summary>Desktop windows are never "split screen"; override for custom hosts.</summary>
    public virtual bool IsInMultiWindowMode => false;

    public virtual UiModeType UiModeType => UiModeType.Normal;

    /// <summary>Text scale applied to <c>Ssp</c>-family values (1.0 = system default).</summary>
    public float FontScale { get; set; } = 1f;

    /// <summary>
    /// Publishes a new window size. Returns true when the snapshot changed (listeners were notified).
    /// Non-positive sizes are ignored (window not laid out yet).
    /// </summary>
    public bool Update(double widthDp, double heightDp, double density)
    {
        if (_disposed) return false;
        var w = (int)Math.Round(widthDp);
        var h = (int)Math.Round(heightDp);
        if (w <= 0 || h <= 0) return false;
        var d = density > 0 && !double.IsNaN(density) ? (float)density : 1f;

        var orientation = w > h
            ? ScreenConfiguration.OrientationLandscape
            : h > w ? ScreenConfiguration.OrientationPortrait : Configuration.Orientation;

        var cfg = new ScreenConfiguration(
            ScreenWidthDp: w,
            ScreenHeightDp: h,
            SmallestScreenWidthDp: Math.Min(w, h),
            DensityDpi: (int)MathF.Round(160f * d),
            FontScale: FontScale > 0 ? FontScale : 1f,
            Orientation: orientation,
            UiMode: 0);

        var changed = Configuration != cfg || BitConverter.SingleToInt32Bits(_density) != BitConverter.SingleToInt32Bits(d);
        _density = d;
        Configuration = cfg;
        if (!changed) return false;

        Action[] snapshot;
        lock (_lock) snapshot = [.. _listeners];
        foreach (var l in snapshot)
        {
            try { l(); } catch { /* listener isolation */ }
        }
        return true;
    }

    public IDisposable RegisterConfigurationListener(Action listener)
    {
        ArgumentNullException.ThrowIfNull(listener);
        lock (_lock) _listeners.Add(listener);
        return new Unsubscriber(this, listener);
    }

    private void RemoveListener(Action listener)
    {
        lock (_lock) _listeners.Remove(listener);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>Unhook native events in derived classes, then call base.</summary>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        _disposed = true;
        lock (_lock) _listeners.Clear();
        DimenCache.DisposeConfigWatcher(this);
    }

    private sealed class Unsubscriber(HostDimensScope owner, Action listener) : IDisposable
    {
        public void Dispose() => owner.RemoveListener(listener);
    }
}

/// <summary>Helpers to activate a <see cref="HostDimensScope"/> as the app-wide ambient context.</summary>
public static class AppDimensHost
{
    /// <summary>
    /// Makes <paramref name="scope"/> the ambient context: initializes the cache and its resize
    /// watcher so parameterless calls like <c>16.Sdp()</c> follow this window.
    /// </summary>
    public static T Activate<T>(T scope) where T : IAppDimensContext
    {
        DimenCache.Init(scope);
        AppDimensAmbient.Set(scope);
        return scope;
    }
}

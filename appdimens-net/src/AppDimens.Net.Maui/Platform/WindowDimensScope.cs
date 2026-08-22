using AppDimens.Net.Common;
using AppDimens.Net.Core;

namespace AppDimens.Net.Maui.Platform;

/// <summary>
/// Live window scope backed by a MAUI <see cref="Window"/>: recomputes the immutable
/// <see cref="ScreenConfiguration"/> on every SizeChanged / DisplayInfoChanged and
/// notifies registered listeners synchronously — the .NET counterpart of the KMP
/// event-driven config watcher.
/// </summary>
public sealed class WindowDimensScope : IAppDimensContext, IDisposable
{
    private readonly Window _window;
    private readonly Func<bool>? _multiWindowProbe;
    private readonly List<Action> _listeners = [];
    private readonly object _lock = new();
    private bool _attached;

    public WindowDimensScope(Window window, Func<bool>? multiWindowProbe = null)
    {
        _window = window ?? throw new ArgumentNullException(nameof(window));
        _multiWindowProbe = multiWindowProbe;
        Attach();
        Update();
    }

    public ScreenConfiguration Configuration { get; private set; } = ScreenConfiguration.Default;

    public float Density => (float)DeviceDisplay.MainDisplayInfo.Density;

    public float Xdpi => 160f * Density;

    public bool IsInMultiWindowMode =>
        _multiWindowProbe?.Invoke()
        ?? HeuristicMultiWindow(Configuration);

    public UiModeType UiModeType => UiModeType.Normal;

    /// <summary>Optional global font-scale override (platform adapters may refine).</summary>
    public static double FontScaleOverride { get; set; } = 1.0;

    internal static bool HeuristicMultiWindow(ScreenConfiguration cfg)
    {
        var sw = (float)cfg.SmallestScreenWidthDp;
        if (sw <= 0f) return false;
        var cw = (float)cfg.ScreenWidthDp;
        return sw - cw >= sw * 0.1f;
    }

    private void Attach()
    {
        lock (_lock)
        {
            if (_attached) return;
            _window.SizeChanged += OnSizeChanged;
            DeviceDisplay.MainDisplayInfoChanged += OnDisplayInfoChanged;
            _attached = true;
        }
    }

    private void OnSizeChanged(object? sender, EventArgs e) => Update();

    private void OnDisplayInfoChanged(object? sender, DisplayInfoChangedEventArgs e) => Update();

    /// <summary>Recomputes the configuration snapshot from the live window bounds.</summary>
    public void Update()
    {
        var width = _window.Width > 0 ? _window.Width : DeviceDisplay.MainDisplayInfo.Width / Math.Max(Density, 0.01);
        var height = _window.Height > 0 ? _window.Height : DeviceDisplay.MainDisplayInfo.Height / Math.Max(Density, 0.01);

        var w = (int)Math.Round(width);
        var h = (int)Math.Round(height);
        if (w <= 0 && h <= 0) return;

        var orientation = w > h
            ? ScreenConfiguration.OrientationLandscape
            : h > w ? ScreenConfiguration.OrientationPortrait : Configuration.Orientation;

        var cfg = new ScreenConfiguration(
            ScreenWidthDp: w,
            ScreenHeightDp: h,
            SmallestScreenWidthDp: Math.Min(w, h),
            DensityDpi: (int)MathF.Round(160f * Density),
            FontScale: (float)(FontScaleOverride > 0 ? FontScaleOverride : 1.0),
            Orientation: orientation,
            UiMode: 0);

        var changed = !AreEqual(Configuration, cfg);
        Configuration = cfg;
        if (!changed) return;

        Action[] snapshot;
        lock (_lock) snapshot = [.. _listeners];
        foreach (var l in snapshot)
        {
            try { l(); } catch { /* listener isolation */ }
        }
    }

    private static bool AreEqual(ScreenConfiguration a, ScreenConfiguration b) =>
        a.ScreenWidthDp == b.ScreenWidthDp &&
        a.ScreenHeightDp == b.ScreenHeightDp &&
        a.SmallestScreenWidthDp == b.SmallestScreenWidthDp &&
        a.DensityDpi == b.DensityDpi &&
        BitConverter.SingleToInt32Bits(a.FontScale) == BitConverter.SingleToInt32Bits(b.FontScale) &&
        a.Orientation == b.Orientation;

    public IDisposable RegisterConfigurationListener(Action listener)
    {
        lock (_lock) _listeners.Add(listener);
        return new Unsubscriber(this, listener);
    }

    private void RemoveListener(Action listener)
    {
        lock (_lock) _listeners.Remove(listener);
    }

    public void Dispose()
    {
        lock (_lock)
        {
            if (!_attached) return;
            _window.SizeChanged -= OnSizeChanged;
            DeviceDisplay.MainDisplayInfoChanged -= OnDisplayInfoChanged;
            _attached = false;
            _listeners.Clear();
        }
        global::AppDimens.Net.Core.DimenCache.DisposeConfigWatcher(this);
    }

    private sealed class Unsubscriber(WindowDimensScope owner, Action listener) : IDisposable
    {
        public void Dispose() => owner.RemoveListener(listener);
    }
}

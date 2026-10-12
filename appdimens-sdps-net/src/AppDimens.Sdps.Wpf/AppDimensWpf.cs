using System.Windows;
using System.Windows.Media;
using AppDimens.Maui;
using AppDimens.Maui.Core;

namespace AppDimens.Sdps.Wpf;

/// <summary>
/// Entry point for WPF apps. Call <see cref="Initialize(Window, AppDimensOptions?, string?)"/>
/// once (e.g. in the <c>MainWindow</c> constructor, before <c>InitializeComponent()</c>) and every
/// <c>{sdps:Sdp 16}</c> value follows the window size from then on.
/// </summary>
public static class AppDimensWpf
{
    private static readonly object Sync = new();
    private static WpfWindowMetricsSource? _source;
    private static bool _subscribed;

    /// <summary>True after <see cref="Initialize(Window, AppDimensOptions?, string?)"/> (or the automatic screen fallback).</summary>
    public static bool IsInitialized { get; private set; }

    /// <summary>The window whose size drives the values (null in screen fallback mode).</summary>
    public static Window? TrackedWindow => _source?.Window;

    /// <summary>
    /// Raised on the UI thread after the window was resized, moved to another monitor
    /// (DPI change) or re-attached. Use it to recompute values set from code-behind.
    /// </summary>
    public static event EventHandler? Changed;

    /// <summary>
    /// Starts AppDimens for a WPF window. Safe to call again with another window
    /// (e.g. when the main window changes).
    /// </summary>
    /// <param name="window">Window whose size is used (usually <c>this</c> in MainWindow).</param>
    /// <param name="options">Optional scaling options (first call only).</param>
    /// <param name="generatedResourcesPath">Optional folder with <c>buckets.json</c>; auto-detected by default.</param>
    public static void Initialize(Window window, AppDimensOptions? options = null, string? generatedResourcesPath = null)
    {
        ArgumentNullException.ThrowIfNull(window);
        Attach(new WpfWindowMetricsSource(window), options, generatedResourcesPath);
    }

    /// <summary>
    /// Starts AppDimens using the primary screen work area (no window tracking).
    /// Called automatically by the markup extensions when nothing was initialized.
    /// </summary>
    public static void EnsureInitialized()
    {
        if (IsInitialized) return;
        lock (Sync)
        {
            if (IsInitialized) return;
            var main = Application.Current?.MainWindow;
            Attach(new WpfWindowMetricsSource(main), null, null);
        }
    }

    /// <summary>Stops tracking the window (values keep their last size).</summary>
    public static void Detach()
    {
        lock (Sync)
        {
            _source?.Dispose();
            _source = null;
        }
    }

    /// <summary>Forces a re-read of the window size and DPI.</summary>
    public static void Refresh() => AppDimensResolver.Instance.RefreshMetricsFromDevice();

    private static void Attach(WpfWindowMetricsSource source, AppDimensOptions? options, string? generatedPath)
    {
        lock (Sync)
        {
            var previous = _source;
            _source = source;
            var resolver = AppDimensResolver.Instance;
            resolver.UseDisplaySource(source);
            resolver.Initialize(options, generatedPath);
            if (!_subscribed)
            {
                resolver.Metrics.Changed += OnMetricsChanged;
                _subscribed = true;
            }
            IsInitialized = true;
            previous?.Dispose();
        }
        OnMetricsChanged(null, EventArgs.Empty);
    }

    private static void OnMetricsChanged(object? sender, EventArgs e)
    {
        DimenLiveSource.Instance.Notify();
        Changed?.Invoke(null, EventArgs.Empty);
    }
}

/// <summary>
/// <see cref="IDisplayMetricsSource"/> backed by a WPF <see cref="Window"/> (DIPs are dp)
/// or, when no window is given, by the primary screen work area.
/// </summary>
public sealed class WpfWindowMetricsSource : IDisplayMetricsSource, IDisposable
{
    private readonly Window? _window;

    public WpfWindowMetricsSource(Window? window)
    {
        _window = window;
        if (_window != null)
        {
            _window.SizeChanged += OnChanged;
            _window.DpiChanged += OnChanged;
            _window.Loaded += OnChanged;
        }
    }

    public Window? Window => _window;

    public event EventHandler? Changed;

    public bool TryGetMetrics(out double widthDp, out double heightDp, out double density)
    {
        widthDp = 0;
        heightDp = 0;
        density = 1.0;

        var w = _window;
        if (w != null)
        {
            if (!w.Dispatcher.CheckAccess())
            {
                double cw = 0, ch = 0, cd = 1.0;
                w.Dispatcher.Invoke(() => Read(w, out cw, out ch, out cd));
                (widthDp, heightDp, density) = (cw, ch, cd);
            }
            else
            {
                Read(w, out widthDp, out heightDp, out density);
            }
        }

        if (!(widthDp > 0) || !(heightDp > 0))
        {
            // Window not laid out yet (constructor) or screen mode: use the work area.
            var area = SystemParameters.WorkArea;
            widthDp = area.Width;
            heightDp = area.Height;
        }
        return widthDp > 0 && heightDp > 0;
    }

    private static void Read(Window w, out double width, out double height, out double density)
    {
        width = w.ActualWidth > 0 ? w.ActualWidth : (double.IsNaN(w.Width) ? 0 : w.Width);
        height = w.ActualHeight > 0 ? w.ActualHeight : (double.IsNaN(w.Height) ? 0 : w.Height);
        density = VisualTreeHelper.GetDpi(w).DpiScaleX;
    }

    private void OnChanged(object? sender, EventArgs e) => Changed?.Invoke(this, EventArgs.Empty);

    public void Dispose()
    {
        var w = _window;
        if (w == null) return;
        // Routed-event handlers must be removed on the window's own UI thread.
        if (w.Dispatcher.CheckAccess()) Unhook(w);
        else if (!w.Dispatcher.HasShutdownStarted) w.Dispatcher.BeginInvoke(() => Unhook(w));
    }

    private void Unhook(Window w)
    {
        w.SizeChanged -= OnChanged;
        w.DpiChanged -= OnChanged;
        w.Loaded -= OnChanged;
    }
}

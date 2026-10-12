using AppDimens.Maui;
using AppDimens.Maui.Core;
using Microsoft.UI.Xaml;

namespace AppDimens.Sdps.WinUI;

/// <summary>
/// Entry point for WinUI 3 (Windows App SDK) apps. Call
/// <see cref="Initialize(Window, AppDimensOptions?, string?)"/> right after creating your main
/// window (in <c>App.OnLaunched</c>) and <c>{sdps:Sdp Value=16}</c> values follow the window size.
/// </summary>
public static class AppDimensWinUI
{
    private static readonly object Sync = new();
    private static WinUIWindowMetricsSource? _source;
    private static bool _subscribed;

    public static bool IsInitialized { get; private set; }

    /// <summary>The window whose size drives the values.</summary>
    public static Window? TrackedWindow => _source?.Window;

    /// <summary>Raised after the window was resized or its scale (DPI) changed.</summary>
    public static event EventHandler? Changed;

    /// <summary>Starts AppDimens for a WinUI 3 window. Calling again with another window switches tracking.</summary>
    public static void Initialize(Window window, AppDimensOptions? options = null, string? generatedResourcesPath = null)
    {
        ArgumentNullException.ThrowIfNull(window);
        lock (Sync)
        {
            var previous = _source;
            _source = new WinUIWindowMetricsSource(window);
            var resolver = AppDimensResolver.Instance;
            resolver.UseDisplaySource(_source);
            resolver.Initialize(options, generatedResourcesPath);
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

    /// <summary>Stops tracking the window (values keep their last size).</summary>
    public static void Detach()
    {
        lock (Sync)
        {
            _source?.Dispose();
            _source = null;
        }
    }

    /// <summary>Forces a re-read of the window size and scale.</summary>
    public static void Refresh() => AppDimensResolver.Instance.RefreshMetricsFromDevice();

    private static void OnMetricsChanged(object? sender, EventArgs e)
    {
        DimenLiveRegistry.Reapply();
        Changed?.Invoke(null, EventArgs.Empty);
    }
}

/// <summary><see cref="IDisplayMetricsSource"/> backed by a WinUI 3 <see cref="Window"/> (Bounds are DIPs).</summary>
public sealed class WinUIWindowMetricsSource : IDisplayMetricsSource, IDisposable
{
    private readonly Window _window;
    private XamlRoot? _root;

    public WinUIWindowMetricsSource(Window window)
    {
        _window = window;
        _window.SizeChanged += OnSizeChanged;
        _window.Activated += OnActivated;
        HookRoot();
    }

    public Window Window => _window;

    public event EventHandler? Changed;

    public bool TryGetMetrics(out double widthDp, out double heightDp, out double density)
    {
        var bounds = _window.Bounds;
        var root = _window.Content?.XamlRoot;
        density = root?.RasterizationScale is > 0 and var scale ? scale : 1.0;

        widthDp = bounds.Width;
        heightDp = bounds.Height;
        if ((!(widthDp > 0) || !(heightDp > 0)) && root != null)
        {
            widthDp = root.Size.Width;
            heightDp = root.Size.Height;
        }
        if ((!(widthDp > 0) || !(heightDp > 0)) && _window.AppWindow is { } appWindow)
        {
            // Before the first layout: AppWindow reports physical pixels.
            widthDp = appWindow.ClientSize.Width / density;
            heightDp = appWindow.ClientSize.Height / density;
        }
        return widthDp > 0 && heightDp > 0;
    }

    private void HookRoot()
    {
        var root = _window.Content?.XamlRoot;
        if (ReferenceEquals(root, _root)) return;
        if (_root != null) _root.Changed -= OnRootChanged;
        _root = root;
        if (_root != null) _root.Changed += OnRootChanged;
    }

    private void OnSizeChanged(object sender, WindowSizeChangedEventArgs args) => Raise();

    private void OnActivated(object sender, WindowActivatedEventArgs args)
    {
        HookRoot();
        Raise();
    }

    private void OnRootChanged(XamlRoot sender, XamlRootChangedEventArgs args) => Raise();

    private void Raise() => Changed?.Invoke(this, EventArgs.Empty);

    public void Dispose()
    {
        _window.SizeChanged -= OnSizeChanged;
        _window.Activated -= OnActivated;
        if (_root != null) _root.Changed -= OnRootChanged;
        _root = null;
    }
}

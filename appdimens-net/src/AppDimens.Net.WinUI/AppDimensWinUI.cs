using AppDimens.Net.Core;
using Microsoft.UI.Xaml;

namespace AppDimens.Net.WinUI;

/// <summary>
/// Entry point for WinUI 3 (Windows App SDK) apps. Call <see cref="Initialize(Window)"/> right after
/// creating the main window in <c>App.OnLaunched</c>; <c>16.Sdp()</c> and <c>{ad:Sdp Value=16}</c>
/// then follow the window size.
/// </summary>
public static class AppDimensWinUI
{
    private static readonly object Sync = new();
    private static WinUIWindowDimensScope? _scope;
    private static IDisposable? _liveRegistration;

    public static WinUIWindowDimensScope? Scope => _scope;

    public static bool IsInitialized => _scope != null;

    /// <summary>Raised after the window was resized or its scale (DPI) changed.</summary>
    public static event EventHandler? Changed;

    /// <summary>Starts AppDimens for a WinUI 3 window. Calling again with another window switches tracking.</summary>
    public static WinUIWindowDimensScope Initialize(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);
        var scope = new WinUIWindowDimensScope(window);
        lock (Sync)
        {
            _liveRegistration?.Dispose();
            var previous = _scope;
            AppDimensHost.Activate(scope);
            _liveRegistration = scope.RegisterConfigurationListener(OnChanged);
            _scope = scope;
            previous?.Dispose();
        }
        OnChanged();
        return scope;
    }

    /// <summary>Stops tracking and clears the ambient context.</summary>
    public static void Detach()
    {
        lock (Sync)
        {
            _liveRegistration?.Dispose();
            _liveRegistration = null;
            _scope?.Dispose();
            _scope = null;
            AppDimensAmbient.Set(null);
        }
    }

    private static void OnChanged()
    {
        DimenLiveRegistry.Reapply();
        Changed?.Invoke(null, EventArgs.Empty);
    }
}

/// <summary>Live <see cref="IAppDimensContext"/> backed by a WinUI 3 <see cref="Window"/> (Bounds are DIPs).</summary>
public sealed class WinUIWindowDimensScope : HostDimensScope
{
    private readonly Window _window;
    private XamlRoot? _root;

    public WinUIWindowDimensScope(Window window)
    {
        _window = window;
        _window.SizeChanged += OnSizeChanged;
        _window.Activated += OnActivated;
        HookRoot();
        Refresh();
    }

    public Window Window => _window;

    /// <summary>Re-reads the window size and scale.</summary>
    public void Refresh()
    {
        var root = _window.Content?.XamlRoot;
        var density = root?.RasterizationScale is > 0 and var scale ? scale : 1.0;
        var width = _window.Bounds.Width;
        var height = _window.Bounds.Height;
        if ((!(width > 0) || !(height > 0)) && root != null)
        {
            width = root.Size.Width;
            height = root.Size.Height;
        }
        if ((!(width > 0) || !(height > 0)) && _window.AppWindow is { } appWindow)
        {
            width = appWindow.ClientSize.Width / density;
            height = appWindow.ClientSize.Height / density;
        }
        Update(width, height, density);
    }

    private void HookRoot()
    {
        var root = _window.Content?.XamlRoot;
        if (ReferenceEquals(root, _root)) return;
        if (_root != null) _root.Changed -= OnRootChanged;
        _root = root;
        if (_root != null) _root.Changed += OnRootChanged;
    }

    private void OnSizeChanged(object sender, WindowSizeChangedEventArgs args) => Refresh();

    private void OnActivated(object sender, WindowActivatedEventArgs args)
    {
        HookRoot();
        Refresh();
    }

    private void OnRootChanged(XamlRoot sender, XamlRootChangedEventArgs args) => Refresh();

    protected override void Dispose(bool disposing)
    {
        _window.SizeChanged -= OnSizeChanged;
        _window.Activated -= OnActivated;
        if (_root != null) _root.Changed -= OnRootChanged;
        _root = null;
        base.Dispose(disposing);
    }
}

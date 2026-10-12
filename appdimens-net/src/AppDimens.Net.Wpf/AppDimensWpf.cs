using System.Windows;
using System.Windows.Media;
using AppDimens.Net.Core;

namespace AppDimens.Net.Wpf;

/// <summary>
/// Entry point for WPF apps. Call <see cref="Initialize(Window)"/> once (e.g. in the
/// <c>MainWindow</c> constructor, before <c>InitializeComponent()</c>): from then on
/// <c>16.Sdp()</c> and <c>{ad:Sdp 16}</c> follow the window size and DPI.
/// </summary>
public static class AppDimensWpf
{
    private static readonly object Sync = new();
    private static WpfWindowDimensScope? _scope;
    private static IDisposable? _liveRegistration;

    /// <summary>The active window scope (null before initialization).</summary>
    public static WpfWindowDimensScope? Scope => _scope;

    public static bool IsInitialized => _scope != null;

    /// <summary>Raised on the UI thread after a resize / DPI change. Recompute code-behind values here.</summary>
    public static event EventHandler? Changed;

    /// <summary>Starts AppDimens for a WPF window. Calling again with another window switches tracking.</summary>
    public static WpfWindowDimensScope Initialize(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);
        return Attach(new WpfWindowDimensScope(window));
    }

    /// <summary>
    /// Starts AppDimens with the main window, or the primary screen work area when there is no window yet.
    /// Called automatically by the markup extensions.
    /// </summary>
    public static void EnsureInitialized()
    {
        if (_scope != null) return;
        lock (Sync)
        {
            if (_scope != null) return;
            Attach(new WpfWindowDimensScope(Application.Current?.MainWindow));
        }
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

    private static WpfWindowDimensScope Attach(WpfWindowDimensScope scope)
    {
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

    private static void OnChanged()
    {
        DimenLiveSource.Instance.Notify();
        Changed?.Invoke(null, EventArgs.Empty);
    }
}

/// <summary>Live <see cref="IAppDimensContext"/> backed by a WPF window (DIPs = dp) or the primary screen.</summary>
public sealed class WpfWindowDimensScope : HostDimensScope
{
    private readonly Window? _window;

    public WpfWindowDimensScope(Window? window)
    {
        _window = window;
        if (_window != null)
        {
            _window.SizeChanged += OnChanged;
            _window.DpiChanged += OnChanged;
            _window.Loaded += OnChanged;
        }
        Refresh();
    }

    public Window? Window => _window;

    /// <summary>Re-reads the window size and DPI.</summary>
    public void Refresh()
    {
        double width = 0, height = 0, density = 1.0;
        var w = _window;
        if (w != null)
        {
            if (w.Dispatcher.CheckAccess()) Read(w, out width, out height, out density);
            else w.Dispatcher.Invoke(() => Read(w, out width, out height, out density));
        }
        if (!(width > 0) || !(height > 0))
        {
            var area = SystemParameters.WorkArea;
            width = area.Width;
            height = area.Height;
        }
        Update(width, height, density);
    }

    private static void Read(Window w, out double width, out double height, out double density)
    {
        width = w.ActualWidth > 0 ? w.ActualWidth : (double.IsNaN(w.Width) ? 0 : w.Width);
        height = w.ActualHeight > 0 ? w.ActualHeight : (double.IsNaN(w.Height) ? 0 : w.Height);
        density = VisualTreeHelper.GetDpi(w).DpiScaleX;
    }

    private void OnChanged(object? sender, EventArgs e) => Refresh();

    protected override void Dispose(bool disposing)
    {
        if (_window != null)
        {
            _window.SizeChanged -= OnChanged;
            _window.DpiChanged -= OnChanged;
            _window.Loaded -= OnChanged;
        }
        base.Dispose(disposing);
    }
}

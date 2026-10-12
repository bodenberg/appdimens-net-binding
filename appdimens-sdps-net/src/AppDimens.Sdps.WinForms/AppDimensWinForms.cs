using AppDimens.Maui;
using AppDimens.Maui.Core;

namespace AppDimens.Sdps.WinForms;

/// <summary>
/// Entry point for Windows Forms apps. Call <see cref="Initialize(Form, AppDimensOptions?, string?)"/>
/// in your main form constructor, then size controls inside <see cref="Apply(Control, Action)"/>
/// so they are recalculated every time the form is resized or moved to another monitor.
/// </summary>
/// <example>
/// <code>
/// public MainForm()
/// {
///     InitializeComponent();
///     AppDimensWinForms.Initialize(this);
///     AppDimensWinForms.Apply(this, () =>
///     {
///         button1.Width = 120.SdpPx();
///         label1.Font = new Font(label1.Font.FontFamily, 14.SspPt());
///     });
/// }
/// </code>
/// </example>
public static class AppDimensWinForms
{
    private static readonly object Sync = new();
    private static FormMetricsSource? _source;
    private static bool _subscribed;

    public static bool IsInitialized { get; private set; }

    /// <summary>The form whose client area drives the values.</summary>
    public static Form? TrackedForm => _source?.Form;

    /// <summary>Raised (UI thread) after the tracked form was resized or changed DPI.</summary>
    public static event EventHandler? Changed;

    /// <summary>Pixels per dp of the tracked form (1.0 at 96 DPI, 1.5 at 144 DPI…).</summary>
    public static double Density => _source?.CurrentDensity ?? 1.0;

    /// <summary>Starts AppDimens for a form. Calling again with another form switches tracking.</summary>
    public static void Initialize(Form form, AppDimensOptions? options = null, string? generatedResourcesPath = null)
    {
        ArgumentNullException.ThrowIfNull(form);
        lock (Sync)
        {
            var previous = _source;
            _source = new FormMetricsSource(form);
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
    }

    /// <summary>
    /// Runs <paramref name="layout"/> now and again after every resize / DPI change, until
    /// <paramref name="owner"/> is disposed. Put all your AppDimens sizing inside it.
    /// </summary>
    public static IDisposable Apply(Control owner, Action layout)
    {
        ArgumentNullException.ThrowIfNull(owner);
        ArgumentNullException.ThrowIfNull(layout);
        var subscription = new ApplySubscription(owner, layout);
        subscription.Run();
        return subscription;
    }

    /// <summary>Stops tracking the form.</summary>
    public static void Detach()
    {
        lock (Sync)
        {
            _source?.Dispose();
            _source = null;
        }
    }

    private static void OnMetricsChanged(object? sender, EventArgs e) => Changed?.Invoke(null, EventArgs.Empty);

    private sealed class ApplySubscription : IDisposable
    {
        private readonly Control _owner;
        private readonly Action _layout;
        private bool _disposed;

        public ApplySubscription(Control owner, Action layout)
        {
            _owner = owner;
            _layout = layout;
            Changed += OnChanged;
            owner.Disposed += OnOwnerDisposed;
        }

        public void Run()
        {
            if (_disposed || _owner.IsDisposed) return;
            if (_owner.InvokeRequired && _owner.IsHandleCreated) _owner.BeginInvoke(_layout);
            else _layout();
        }

        private void OnChanged(object? sender, EventArgs e) => Run();

        private void OnOwnerDisposed(object? sender, EventArgs e) => Dispose();

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            Changed -= OnChanged;
            _owner.Disposed -= OnOwnerDisposed;
        }
    }
}

/// <summary>Converts AppDimens dp values to Windows Forms units.</summary>
public static class DimenPx
{
    /// <summary>dp → physical pixels of the tracked form (rounded).</summary>
    public static int ToPx(double dp) => (int)Math.Round(dp * AppDimensWinForms.Density, MidpointRounding.AwayFromZero);

    /// <summary>dp → typographic points (1 dp = 1/96 inch = 0.75 pt). Use for <c>Font</c> sizes.</summary>
    public static float ToPt(double dp) => (float)(dp * 72.0 / 96.0);

    /// <summary>Same dp value on all four sides, in pixels.</summary>
    public static Padding Padding(double dp) => new(ToPx(dp));

    /// <summary>Square size in pixels.</summary>
    public static Size Size(double widthDp, double heightDp) => new(ToPx(widthDp), ToPx(heightDp));
}

/// <summary><see cref="IDisplayMetricsSource"/> backed by a form's client area (pixels ÷ DPI scale = dp).</summary>
public sealed class FormMetricsSource : IDisplayMetricsSource, IDisposable
{
    private readonly Form _form;
    private double _density = 1.0;

    public FormMetricsSource(Form form)
    {
        _form = form;
        _form.Resize += OnChanged;
        _form.DpiChanged += OnChanged;
        _form.HandleCreated += OnChanged;
    }

    public Form Form => _form;

    internal double CurrentDensity => _density;

    public event EventHandler? Changed;

    public bool TryGetMetrics(out double widthDp, out double heightDp, out double density)
    {
        if (_form.IsDisposed)
        {
            widthDp = heightDp = 0;
            density = _density;
            return false;
        }

        var dpi = _form.DeviceDpi > 0 ? _form.DeviceDpi : 96;
        density = dpi / 96.0;
        _density = density;

        var client = _form.ClientSize;
        if (client.Width <= 0 || client.Height <= 0 || _form.WindowState == FormWindowState.Minimized)
        {
            // Not shown yet / minimized: fall back to the working area of its screen.
            var area = Screen.FromControl(_form).WorkingArea;
            widthDp = area.Width / density;
            heightDp = area.Height / density;
            return widthDp > 0 && heightDp > 0 && _form.WindowState != FormWindowState.Minimized;
        }

        widthDp = client.Width / density;
        heightDp = client.Height / density;
        return true;
    }

    private void OnChanged(object? sender, EventArgs e) => Changed?.Invoke(this, EventArgs.Empty);

    public void Dispose()
    {
        _form.Resize -= OnChanged;
        _form.DpiChanged -= OnChanged;
        _form.HandleCreated -= OnChanged;
    }
}

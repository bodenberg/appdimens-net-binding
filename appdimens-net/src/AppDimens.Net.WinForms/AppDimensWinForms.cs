using AppDimens.Net.Core;

namespace AppDimens.Net.WinForms;

/// <summary>
/// Entry point for Windows Forms apps. Call <see cref="Initialize(Form)"/> in your main form
/// constructor, then size controls inside <see cref="Apply(Control, Action)"/> so they are
/// recalculated on every resize / monitor change.
/// </summary>
/// <example>
/// <code>
/// public MainForm()
/// {
///     InitializeComponent();
///     AppDimensWinForms.Initialize(this);
///     AppDimensWinForms.Apply(this, () =>
///     {
///         button1.Width = DimenPx.ToPx(120.Sdp());
///         label1.Font = new Font(label1.Font.FontFamily, DimenPx.ToPt(14.Ssp()));
///     });
/// }
/// </code>
/// </example>
public static class AppDimensWinForms
{
    private static readonly object Sync = new();
    private static FormDimensScope? _scope;
    private static IDisposable? _registration;

    public static FormDimensScope? Scope => _scope;

    public static bool IsInitialized => _scope != null;

    /// <summary>Raised (UI thread) after the tracked form was resized or changed DPI.</summary>
    public static event EventHandler? Changed;

    /// <summary>Starts AppDimens for a form. Calling again with another form switches tracking.</summary>
    public static FormDimensScope Initialize(Form form)
    {
        ArgumentNullException.ThrowIfNull(form);
        var scope = new FormDimensScope(form);
        lock (Sync)
        {
            _registration?.Dispose();
            var previous = _scope;
            AppDimensHost.Activate(scope);
            _registration = scope.RegisterConfigurationListener(OnChanged);
            _scope = scope;
            previous?.Dispose();
        }
        return scope;
    }

    /// <summary>
    /// Runs <paramref name="layout"/> now and after every resize / DPI change, until
    /// <paramref name="owner"/> is disposed.
    /// </summary>
    public static IDisposable Apply(Control owner, Action layout)
    {
        ArgumentNullException.ThrowIfNull(owner);
        ArgumentNullException.ThrowIfNull(layout);
        var subscription = new ApplySubscription(owner, layout);
        subscription.Run();
        return subscription;
    }

    /// <summary>Stops tracking and clears the ambient context.</summary>
    public static void Detach()
    {
        lock (Sync)
        {
            _registration?.Dispose();
            _registration = null;
            _scope?.Dispose();
            _scope = null;
            AppDimensAmbient.Set(null);
        }
    }

    private static void OnChanged() => Changed?.Invoke(null, EventArgs.Empty);

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
    public static int ToPx(double dp)
    {
        var density = AppDimensAmbient.Current?.Density ?? 1f;
        return (int)Math.Round(dp * density, MidpointRounding.AwayFromZero);
    }

    /// <summary>dp → typographic points (1 dp = 1/96 inch = 0.75 pt). Use for <c>Font</c> sizes.</summary>
    public static float ToPt(double dp) => (float)(dp * 72.0 / 96.0);

    /// <summary>Same dp value on all four sides, in pixels.</summary>
    public static Padding Padding(double dp) => new(ToPx(dp));

    /// <summary>Width × height in pixels.</summary>
    public static Size Size(double widthDp, double heightDp) => new(ToPx(widthDp), ToPx(heightDp));

    /// <summary>Evaluates any <see cref="DimenFunction"/> straight to pixels: <c>DimenPx.Of(DimenFunction.FSdp, 24)</c>.</summary>
    public static int Of(DimenFunction function, int value) => ToPx(function.Evaluate(value));
}

/// <summary>Live <see cref="IAppDimensContext"/> backed by a form's client area (pixels ÷ DPI scale = dp).</summary>
public sealed class FormDimensScope : HostDimensScope
{
    private readonly Form _form;

    public FormDimensScope(Form form)
    {
        _form = form;
        _form.Resize += OnChanged;
        _form.DpiChanged += OnChanged;
        _form.HandleCreated += OnChanged;
        Refresh();
    }

    public Form Form => _form;

    /// <summary>Re-reads the client size and DPI.</summary>
    public void Refresh()
    {
        if (_form.IsDisposed || _form.WindowState == FormWindowState.Minimized) return;
        var density = (_form.DeviceDpi > 0 ? _form.DeviceDpi : 96) / 96.0;
        var client = _form.ClientSize;
        double width = client.Width, height = client.Height;
        if (width <= 0 || height <= 0)
        {
            var area = Screen.FromControl(_form).WorkingArea;
            width = area.Width;
            height = area.Height;
        }
        Update(width / density, height / density, density);
    }

    private void OnChanged(object? sender, EventArgs e) => Refresh();

    protected override void Dispose(bool disposing)
    {
        _form.Resize -= OnChanged;
        _form.DpiChanged -= OnChanged;
        _form.HandleCreated -= OnChanged;
        base.Dispose(disposing);
    }
}

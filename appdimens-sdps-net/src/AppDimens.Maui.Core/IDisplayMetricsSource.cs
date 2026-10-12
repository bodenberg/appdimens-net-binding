namespace AppDimens.Maui.Core;

/// <summary>
/// Host-provided source of window/display metrics. Lets the resolver run on any UI
/// framework: .NET MAUI (default), WPF, Windows Forms, WinUI 3 or a custom host.
/// </summary>
/// <remarks>
/// Values are in device-independent units (dp / DIP): 1 unit = 1/160 inch on Android,
/// 1/96 inch on Windows. <c>density</c> is the scale between physical pixels and dp
/// (e.g. 1.5 on a 144 DPI Windows monitor).
/// </remarks>
public interface IDisplayMetricsSource
{
    /// <summary>Reads the current metrics. Returns false when the host is not ready yet.</summary>
    bool TryGetMetrics(out double widthDp, out double heightDp, out double density);

    /// <summary>Raised whenever size, density or orientation may have changed.</summary>
    event EventHandler? Changed;
}

/// <summary>
/// Simple push-based <see cref="IDisplayMetricsSource"/>: call <see cref="Update"/> from
/// your own resize handler. Useful for custom hosts, game loops and tests.
/// </summary>
public sealed class ManualDisplayMetricsSource : IDisplayMetricsSource
{
    private readonly object _lock = new();
    private double _width;
    private double _height;
    private double _density = 1.0;

    public ManualDisplayMetricsSource() { }

    public ManualDisplayMetricsSource(double widthDp, double heightDp, double density = 1.0)
    {
        _width = widthDp;
        _height = heightDp;
        _density = density > 0 ? density : 1.0;
    }

    public event EventHandler? Changed;

    public bool TryGetMetrics(out double widthDp, out double heightDp, out double density)
    {
        lock (_lock)
        {
            widthDp = _width;
            heightDp = _height;
            density = _density;
        }
        return widthDp > 0 && heightDp > 0;
    }

    /// <summary>Pushes new metrics and raises <see cref="Changed"/>.</summary>
    public void Update(double widthDp, double heightDp, double density = 1.0)
    {
        lock (_lock)
        {
            _width = widthDp;
            _height = heightDp;
            _density = density > 0 ? density : 1.0;
        }
        Changed?.Invoke(this, EventArgs.Empty);
    }
}

namespace AppDimens.Maui.Core;

public interface IScreenMetricsProvider
{
    ScreenMetricsSnapshot Current { get; }
    event EventHandler? Changed;
}

public sealed class MutableScreenMetricsProvider : IScreenMetricsProvider
{
    private ScreenMetricsSnapshot _current = new(360, 640, 360, 2.0, 320, ScreenOrientation.Portrait, UiModeType.Normal);
    private readonly object _lock = new();

    public ScreenMetricsSnapshot Current
    {
        get { lock (_lock) return _current; }
    }

    public event EventHandler? Changed;

    public void Update(ScreenMetricsSnapshot snapshot)
    {
        lock (_lock)
        {
            if (_current == snapshot) return;
            _current = snapshot;
        }
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void Update(double widthDp, double heightDp, double density, int densityDpi = 320,
        ScreenOrientation? orientation = null, UiModeType uiMode = UiModeType.Normal)
    {
        var smallest = Math.Min(widthDp, heightDp);
        var orient = orientation ?? (widthDp > heightDp ? ScreenOrientation.Landscape : ScreenOrientation.Portrait);
        Update(new ScreenMetricsSnapshot(widthDp, heightDp, smallest, density, densityDpi, orient, uiMode));
    }
}

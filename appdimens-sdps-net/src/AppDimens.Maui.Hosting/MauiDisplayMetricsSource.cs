using AppDimens.Maui.Core;
using Microsoft.Maui.Devices;

namespace AppDimens.Maui;

/// <summary>
/// Default <see cref="IDisplayMetricsSource"/> backed by MAUI <c>DeviceDisplay</c>.
/// Reports the main display in dp; window tracking is layered on top by
/// <see cref="AppDimensSdpsWindow"/>.
/// </summary>
public sealed class MauiDisplayMetricsSource : IDisplayMetricsSource
{
    private EventHandler? _changed;

    public event EventHandler? Changed
    {
        add
        {
            if (_changed == null) DeviceDisplay.MainDisplayInfoChanged += OnInfoChanged;
            _changed += value;
        }
        remove
        {
            _changed -= value;
            if (_changed == null) DeviceDisplay.MainDisplayInfoChanged -= OnInfoChanged;
        }
    }

    public bool TryGetMetrics(out double widthDp, out double heightDp, out double density)
    {
        var info = DeviceDisplay.MainDisplayInfo;
        density = info.Density > 0 ? info.Density : 1.0;
        widthDp = info.Width / density;
        heightDp = info.Height / density;
        return widthDp > 0 && heightDp > 0;
    }

    private void OnInfoChanged(object? sender, DisplayInfoChangedEventArgs e) => _changed?.Invoke(this, EventArgs.Empty);
}

using AppDimens.Net.Common;
using AppDimens.Net.Core;

namespace AppDimens.Net.Testing;

/// <summary>
/// Mutable in-memory window context for tests, benchmarks and design-time previews.
/// Change <see cref="Config"/> and call <see cref="NotifyChange"/> to simulate a
/// resize/rotation through the event-driven watcher.
/// </summary>
public sealed class FakeAppDimensContext : IAppDimensContext
{
    private readonly List<Action> _listeners = [];
    private readonly object _lock = new();

    public FakeAppDimensContext(ScreenConfiguration? config = null)
        => Config = config ?? ScreenConfiguration.Default;

    public ScreenConfiguration Config { get; private set; }

    public ScreenConfiguration Configuration => Config;

    public float Density => Config.DensityDpi / 160f;
    public float Xdpi => (float)Config.DensityDpi;
    public bool IsInMultiWindowMode { get; set; }
    public UiModeType UiModeType { get; set; } = Common.UiModeType.Normal;

    public void SetConfig(int widthDp, int heightDp, int densityDpi = 160,
        int orientation = ScreenConfiguration.OrientationUndefined, float fontScale = 1f)
    {
        Config = new ScreenConfiguration(widthDp, heightDp, Math.Min(widthDp, heightDp),
            densityDpi, fontScale, orientation, 0);
    }

    /// <summary>Simulates a configuration change; fires registered listeners.</summary>
    public void NotifyChange()
    {
        Action[] snapshot;
        lock (_lock) snapshot = [.. _listeners];
        foreach (var l in snapshot) l();
    }

    public IDisposable RegisterConfigurationListener(Action listener)
    {
        lock (_lock) _listeners.Add(listener);
        return new Unsubscriber(this, listener);
    }

    private sealed class Unsubscriber(FakeAppDimensContext owner, Action l) : IDisposable
    {
        public void Dispose()
        {
            lock (owner._lock) owner._listeners.Remove(l);
        }
    }
}

using System.Collections.Concurrent;

namespace AppDimens.Maui.Core;

public sealed class DimensionCache
{
    private readonly ConcurrentDictionary<DimenCacheKey, double> _cache = new();

    public bool TryGet(DimenCacheKey key, out double value) => _cache.TryGetValue(key, out value!);

    public double GetOrAdd(DimenCacheKey key, Func<double> factory)
        => _cache.GetOrAdd(key, _ => factory());

    public void Invalidate() => _cache.Clear();
}

public sealed class AppDimensOptions
{
    public ScalingMode ScalingMode { get; set; } = ScalingMode.Hybrid;
    public bool WarmupAspectRatio { get; set; } = true;
    public float DefaultFontScale { get; set; } = 1f;
}

public interface IFontScaleService
{
    float FontScale { get; }
}

public sealed class DefaultFontScaleService : IFontScaleService
{
    public float FontScale { get; set; } = 1f;
}

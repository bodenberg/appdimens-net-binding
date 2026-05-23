using AppDimens.Maui.Core;
using AppDimens.Maui.Inverters;
using AppDimens.Maui.Responsive;
using Microsoft.Maui.Devices;

namespace AppDimens.Maui;

public sealed class AppDimensResolver
{
    public static AppDimensResolver Instance { get; } = new();

    private readonly MutableScreenMetricsProvider _metrics = new();
    private readonly DimensionCache _cache = new();
    private readonly AspectRatioFactors _aspectRatio = new();
    private ResourceBucketManager? _buckets;
    private ResponsiveManager? _responsive;
    private AppDimensOptions _options = new();
    private IFontScaleService _fontScale = new DefaultFontScaleService();
    private bool _initialized;
    private bool _testMode;

    public IScreenMetricsProvider Metrics => _metrics;
    public AppDimensOptions Options => _options;
    public DimensionCache Cache => _cache;
    public AspectRatioFactors AspectRatio => _aspectRatio;
    public ResourceBucketManager? Buckets => _buckets;

    public void Initialize(AppDimensOptions? options = null, string? generatedResourcesPath = null, bool force = false)
    {
        if (_initialized && !force) return;
        _options = options ?? new AppDimensOptions();
        _fontScale = new DefaultFontScaleService { FontScale = _options.DefaultFontScale };

        var path = generatedResourcesPath ?? FindGeneratedPath();
        if (path != null && File.Exists(Path.Combine(path, "buckets.json")))
        {
            var registry = BucketRegistry.LoadFromGenerated(path);
            _buckets = new ResourceBucketManager(registry);
            _responsive = new ResponsiveManager(_cache, _aspectRatio, _buckets);
        }

        if (!_initialized)
            DeviceDisplay.MainDisplayInfoChanged += OnDisplayChanged;

        if (!_testMode)
            RefreshMetricsFromDevice();

        _initialized = true;

        if (_options.WarmupAspectRatio && !_testMode)
            Warmup();
    }

    public void ResetForTesting()
    {
        DeviceDisplay.MainDisplayInfoChanged -= OnDisplayChanged;
        _initialized = false;
        _testMode = false;
        _buckets = null;
        _responsive = null;
        _cache.Invalidate();
        _aspectRatio.ResetForTests();
    }

    private static string? FindGeneratedPath()
    {
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "Generated"),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..",
                "AppDimens.Maui.Resources", "Generated")),
            Path.GetFullPath(Path.Combine(
                Path.GetDirectoryName(typeof(AppDimensResolver).Assembly.Location) ?? ".",
                "..", "AppDimens.Maui.Resources", "Generated")),
        };
        return candidates.FirstOrDefault(Directory.Exists);
    }

    private void OnDisplayChanged(object? sender, DisplayInfoChangedEventArgs e) => RefreshMetricsFromDevice();

    public void RefreshMetricsFromDevice()
    {
        if (_testMode) return;
        var info = DeviceDisplay.MainDisplayInfo;
        var density = info.Density > 0 ? info.Density : 1.0;
        var widthDp = info.Width / density;
        var heightDp = info.Height / density;
        var dpi = (int)(160 * density);
        var previous = _metrics.Current;
        _metrics.Update(widthDp, heightDp, density, dpi);
        if (previous == _metrics.Current)
            return;

        _cache.Invalidate();
        if (_responsive != null)
            _responsive.OnMetricsChanged(_metrics.Current);
        else if (_buckets != null)
            _aspectRatio.EnsureUpToDate(_metrics.Current, q => _buckets.GetOneUnit(q));
    }

    public void Warmup()
    {
        RefreshMetricsFromDevice();
        if (_buckets != null)
            _aspectRatio.EnsureUpToDate(_metrics.Current, q => _buckets.GetOneUnit(q));
    }

    public double Resolve(int index, DpQualifier baseQualifier, InverterType inverter = InverterType.Default,
        bool applyAspectRatio = false, bool applyFontScale = false, bool allowNegative = true)
    {
        ScaleEngine.ValidateIndex(index, allowNegative);
        var metrics = _metrics.Current;
        var effective = InverterEngine.EffectiveQualifier(metrics, baseQualifier, inverter);
        var mode = _options.ScalingMode;

        var key = new DimenCacheKey(index, effective, inverter, applyAspectRatio, applyFontScale, mode);
        return _cache.GetOrAdd(key, () => Compute(index, effective, applyAspectRatio, applyFontScale, mode, metrics));
    }

    private double Compute(int index, DpQualifier qualifier, bool applyAspectRatio, bool applyFontScale,
        ScalingMode mode, ScreenMetricsSnapshot metrics)
    {
        var key = ScaleEngine.BuildResourceKey(index, qualifier);
        double value;

        if (mode is ScalingMode.Continuous or ScalingMode.HybridPreferContinuous)
        {
            var metric = ScaleEngine.GetMetricDp(qualifier, metrics);
            value = ScaleEngine.Scale(index, metric);
        }
        else if (mode is ScalingMode.Bucket or ScalingMode.Hybrid &&
                 _buckets != null && _buckets.TryGetDimen(qualifier, key, out value))
        {
            // Precomputed bucket value.
        }
        else
        {
            var metric = ScaleEngine.GetMetricDp(qualifier, metrics);
            value = ScaleEngine.Scale(index, metric);
        }

        if (applyAspectRatio)
        {
            _aspectRatio.EnsureUpToDate(metrics, q => _buckets?.GetOneUnit(q) ?? 1.0);
            value *= _aspectRatio.For(qualifier);
        }

        if (applyFontScale && _fontScale.FontScale > 0)
            value *= _fontScale.FontScale;

        return value;
    }

    // Resolve overloads by qualifier and modifier flags.
    public double Sdp(int v, InverterType inv = InverterType.Default) =>
        Resolve(v, DpQualifier.SmallWidth, inv);

    public double Sdpa(int v, InverterType inv = InverterType.Default) =>
        Resolve(v, DpQualifier.SmallWidth, inv, applyAspectRatio: true);

    public double Hdp(int v, InverterType inv = InverterType.Default) =>
        Resolve(v, DpQualifier.Height, inv);

    public double Wdp(int v, InverterType inv = InverterType.Default) =>
        Resolve(v, DpQualifier.Width, inv);

    public double Ssp(int v, InverterType inv = InverterType.Default) =>
        Resolve(v, DpQualifier.SmallWidth, inv, applyFontScale: true, allowNegative: false);

    public double Hsp(int v, InverterType inv = InverterType.Default) =>
        Resolve(v, DpQualifier.Height, inv, applyFontScale: true, allowNegative: false);

    public double Wsp(int v, InverterType inv = InverterType.Default) =>
        Resolve(v, DpQualifier.Width, inv, applyFontScale: true, allowNegative: false);

    public double Sspa(int v, InverterType inv = InverterType.Default) =>
        Resolve(v, DpQualifier.SmallWidth, inv, applyAspectRatio: true, applyFontScale: true, allowNegative: false);

    public double Hspa(int v, InverterType inv = InverterType.Default) =>
        Resolve(v, DpQualifier.Height, inv, applyAspectRatio: true, applyFontScale: true, allowNegative: false);

    public double Wspa(int v, InverterType inv = InverterType.Default) =>
        Resolve(v, DpQualifier.Width, inv, applyAspectRatio: true, applyFontScale: true, allowNegative: false);

    public double Sem(int v, InverterType inv = InverterType.Default) =>
        Resolve(v, DpQualifier.SmallWidth, inv, allowNegative: false);

    public double Sema(int v, InverterType inv = InverterType.Default) =>
        Resolve(v, DpQualifier.SmallWidth, inv, applyAspectRatio: true, allowNegative: false);

    public double Hem(int v, InverterType inv = InverterType.Default) =>
        Resolve(v, DpQualifier.Height, inv, allowNegative: false);

    public double Hema(int v, InverterType inv = InverterType.Default) =>
        Resolve(v, DpQualifier.Height, inv, applyAspectRatio: true, allowNegative: false);

    public double Wem(int v, InverterType inv = InverterType.Default) =>
        Resolve(v, DpQualifier.Width, inv, allowNegative: false);

    public double Wema(int v, InverterType inv = InverterType.Default) =>
        Resolve(v, DpQualifier.Width, inv, applyAspectRatio: true, allowNegative: false);

    public void SetFontScale(float scale) => _fontScale = new DefaultFontScaleService { FontScale = scale };

    public void SetMetricsForTesting(double widthDp, double heightDp, double density = 2.0,
        ScreenOrientation? orientation = null)
    {
        _testMode = true;
        _cache.Invalidate();
        _metrics.Update(widthDp, heightDp, density, orientation: orientation);
        _responsive?.OnMetricsChanged(_metrics.Current);
    }
}

public static class AppDimensSdps
{
    public static AppDimensResolver Resolver => AppDimensResolver.Instance;

    public static void Initialize(AppDimensOptions? options = null, string? generatedPath = null)
        => Resolver.Initialize(options, generatedPath);

    public static void Warmup() => Resolver.Warmup();

    public static double Sdp(int value) => Resolver.Sdp(value);
    public static double Sdpa(int value) => Resolver.Sdpa(value);
    public static double Hdp(int value) => Resolver.Hdp(value);
    public static double Wdp(int value) => Resolver.Wdp(value);
    public static double Ssp(int value) => Resolver.Ssp(value);
    public static double Hsp(int value) => Resolver.Hsp(value);
    public static double Wsp(int value) => Resolver.Wsp(value);
    public static double Sspa(int value) => Resolver.Sspa(value);
    public static double Hspa(int value) => Resolver.Hspa(value);
    public static double Wspa(int value) => Resolver.Wspa(value);
    public static double Sem(int value) => Resolver.Sem(value);
    public static double Sema(int value) => Resolver.Sema(value);
    public static double Hem(int value) => Resolver.Hem(value);
    public static double Hema(int value) => Resolver.Hema(value);
    public static double Wem(int value) => Resolver.Wem(value);
    public static double Wema(int value) => Resolver.Wema(value);
}

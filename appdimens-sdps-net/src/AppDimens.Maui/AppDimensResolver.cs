using AppDimens.Maui.Core;
using AppDimens.Maui.Inverters;
using AppDimens.Maui.Responsive;
using Microsoft.Maui.Devices;

namespace AppDimens.Maui;

/// <summary>
/// Central dimension resolver. Live values track the current display/window and
/// automatically adjust on resize; <c>*i</c> (independent) values resolve against a
/// frozen <b>baseline</b> snapshot captured once at initialization and therefore stay
/// constant when the screen or window is resized.
/// </summary>
public sealed class AppDimensResolver
{
    public static AppDimensResolver Instance { get; } = new();

    private const int SourceLive = 0;
    private const int SourceBaseline = 1;

    private readonly MutableScreenMetricsProvider _metrics = new();
    private readonly DimensionCache _cache = new();
    private readonly AspectRatioFactors _aspectRatio = new();
    private ResourceBucketManager? _buckets;
    private ResponsiveManager? _responsive;
    private AppDimensOptions _options = new();
    private IFontScaleService _fontScale = new DefaultFontScaleService();
    private bool _initialized;
    private bool _testMode;

    // Frozen baseline used by every *i (independent / resize-invariant) API.
    private ScreenMetricsSnapshot? _baselineMetrics;
    private ResourceBucketManager? _baselineBuckets;
    private AspectRatioFactors? _baselineAspectRatio;
    private BucketRegistry? _registry;

    private readonly object _sync = new();

    // Optional window-bounds override installed by AppDimensSdpsWindow.Attach.
    private (double Width, double Height)? _windowBounds;

    public IScreenMetricsProvider Metrics => _metrics;
    public AppDimensOptions Options => _options;
    public DimensionCache Cache => _cache;
    public AspectRatioFactors AspectRatio => _aspectRatio;
    public ResourceBucketManager? Buckets => _buckets;

    /// <summary>Frozen snapshot backing all independent (<c>*i</c>) APIs; null until captured.</summary>
    public ScreenMetricsSnapshot? BaselineMetrics => _baselineMetrics;

    public void Initialize(AppDimensOptions? options = null, string? generatedResourcesPath = null, bool force = false)
    {
        if (_initialized && !force) return;
        _options = options ?? new AppDimensOptions();
        _fontScale = new DefaultFontScaleService { FontScale = _options.DefaultFontScale };

        var path = generatedResourcesPath ?? FindGeneratedPath();
        if (path != null && File.Exists(Path.Combine(path, "buckets.json")))
        {
            _registry = BucketRegistry.LoadFromGenerated(path);
            _buckets = new ResourceBucketManager(_registry);
            _responsive = new ResponsiveManager(_cache, _aspectRatio, _buckets);
        }

        if (!_initialized)
        {
            DeviceDisplay.MainDisplayInfoChanged += OnDisplayChanged;
            // Any metrics source (window watcher, custom provider, tests) flows through
            // the same invalidation pipeline.
            _metrics.Changed += OnMetricsProviderChanged;
        }

        if (!_testMode)
            RefreshMetricsFromDevice();

        _initialized = true;

        if (!_testMode)
            CaptureBaseline();

        if (_options.WarmupAspectRatio && !_testMode)
            Warmup();
    }

    public void ResetForTesting()
    {
        DeviceDisplay.MainDisplayInfoChanged -= OnDisplayChanged;
        _metrics.Changed -= OnMetricsProviderChanged;
        _initialized = false;
        _testMode = false;
        _buckets = null;
        _responsive = null;
        _registry = null;
        _cache.Invalidate();
        _aspectRatio.ResetForTests();
        _baselineMetrics = null;
        _baselineBuckets = null;
        _baselineAspectRatio = null;
        _windowBounds = null;
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

    private void OnMetricsProviderChanged(object? sender, EventArgs e) => ApplyMetricsChange();

    /// <summary>Refreshes derived state (cache, buckets, aspect factors) after any metrics change.</summary>
    private void ApplyMetricsChange()
    {
        _cache.Invalidate();
        if (_responsive != null)
            _responsive.OnMetricsChanged(_metrics.Current);
        else if (_buckets != null)
            _aspectRatio.EnsureUpToDate(_metrics.Current, q => _buckets.GetOneUnit(q));
    }

    /// <summary>
    /// Installs window-bounds tracking so values follow the resized <b>window</b> instead of
    /// the physical display (desktop, tablets in split-screen, foldables).
    /// Provided by <see cref="AppDimensSdpsWindow.Attach(Microsoft.Maui.Controls.Window)"/>.
    /// </summary>
    public void TrackWindowBounds(double widthDp, double heightDp)
    {
        _windowBounds = (widthDp, heightDp);
        RefreshMetricsFromDevice();
    }

    /// <summary>Removes the window-bounds override installed by <see cref="TrackWindowBounds"/>.</summary>
    public void UntrackWindowBounds()
    {
        _windowBounds = null;
        RefreshMetricsFromDevice();
    }

    /// <summary>
    /// Re-reads platform display info (and tracked window bounds, if any) into the live
    /// snapshot, then refreshes buckets and aspect-ratio factors. Called automatically on
    /// <c>MainDisplayInfoChanged</c> and by the window watcher.
    /// </summary>
    public void RefreshMetricsFromDevice()
    {
        if (_testMode) return;
        lock (_sync)
        {
            var info = DeviceDisplay.MainDisplayInfo;
            var density = info.Density > 0 ? info.Density : 1.0;
            double widthDp, heightDp;
            var bounds = _windowBounds;
            if (bounds.HasValue && bounds.Value.Width > 0 && bounds.Value.Height > 0)
            {
                widthDp = bounds.Value.Width;
                heightDp = bounds.Value.Height;
            }
            else
            {
                widthDp = info.Width / density;
                heightDp = info.Height / density;
            }

            var dpi = (int)(160 * density);
            // MutableScreenMetricsProvider.Update raises Changed → ApplyMetricsChange.
            _metrics.Update(widthDp, heightDp, density, dpi);
        }
    }

    public void Warmup()
    {
        RefreshMetricsFromDevice();
        if (_buckets != null)
            _aspectRatio.EnsureUpToDate(_metrics.Current, q => _buckets.GetOneUnit(q));
    }

    // ─────────────────────────────────────────────────────────────────────
    // BASELINE — resize-independent snapshot powering every *i API
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Freezes the current metrics as the new baseline for all independent (<c>*i</c>)
    /// APIs and drops previously cached independent values.
    /// </summary>
    public void CaptureBaseline()
    {
        _baselineMetrics = _metrics.Current;
        _baselineBuckets = null;
        _baselineAspectRatio = null;
        _cache.Invalidate();
    }

    /// <summary>Baseline snapshot, captured lazily from live metrics when missing.</summary>
    private ScreenMetricsSnapshot BaselineOrCurrent =>
        _baselineMetrics ??= _metrics.Current;

    private ResourceBucketManager? BaselineBuckets
    {
        get
        {
            if (_registry is null) return null;
            return _baselineBuckets ??= new ResourceBucketManager(_registry);
        }
    }

    private AspectRatioFactors BaselineAspectRatio => _baselineAspectRatio ??= new AspectRatioFactors();

    // ─────────────────────────────────────────────────────────────────────
    // RESOLUTION
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>Live value — adjusts automatically when the screen/window is resized.</summary>
    public double Resolve(int index, DpQualifier baseQualifier, InverterType inverter = InverterType.Default,
        bool applyAspectRatio = false, bool applyFontScale = false, bool allowNegative = true)
    {
        ScaleEngine.ValidateIndex(index, allowNegative);
        var metrics = _metrics.Current;
        var effective = InverterEngine.EffectiveQualifier(metrics, baseQualifier, inverter);
        var mode = _options.ScalingMode;

        var key = new DimenCacheKey(index, effective, inverter, applyAspectRatio, applyFontScale, mode,
            BitConverter.SingleToInt32Bits(_fontScale.FontScale), SourceLive);
        return _cache.GetOrAdd(key, () => Compute(metrics, index, effective, applyAspectRatio, applyFontScale, mode, _aspectRatio, _buckets));
    }

    /// <summary>
    /// Independent value — resolved against the frozen baseline snapshot; does NOT change
    /// when the screen or window is resized (the <c>i</c> suffix contract).
    /// </summary>
    public double ResolveIndependent(int index, DpQualifier baseQualifier, InverterType inverter = InverterType.Default,
        bool applyAspectRatio = false, bool applyFontScale = false, bool allowNegative = true)
    {
        ScaleEngine.ValidateIndex(index, allowNegative);
        var baseline = BaselineOrCurrent;
        BaselineBuckets?.EnsureUpToDate(baseline);
        var effective = InverterEngine.EffectiveQualifier(baseline, baseQualifier, inverter);
        var mode = _options.ScalingMode;

        var key = new DimenCacheKey(index, effective, inverter, applyAspectRatio, applyFontScale, mode,
            BitConverter.SingleToInt32Bits(_fontScale.FontScale), SourceBaseline);
        return _cache.GetOrAdd(key, () => Compute(
            baseline, index, effective, applyAspectRatio, applyFontScale, mode,
            BaselineAspectRatio, BaselineBuckets));
    }

    private double Compute(
        ScreenMetricsSnapshot metrics, int index, DpQualifier qualifier, bool applyAspectRatio, bool applyFontScale,
        ScalingMode mode, AspectRatioFactors arFactors, ResourceBucketManager? buckets)
    {
        var key = ScaleEngine.BuildResourceKey(index, qualifier);
        double value;

        if (mode is ScalingMode.Continuous or ScalingMode.HybridPreferContinuous)
        {
            var metric = ScaleEngine.GetMetricDp(qualifier, metrics);
            value = ScaleEngine.Scale(index, metric);
        }
        else if (mode is ScalingMode.Bucket or ScalingMode.Hybrid &&
                 buckets != null && buckets.TryGetDimen(qualifier, key, out value))
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
            arFactors.EnsureUpToDate(metrics, q => buckets?.GetOneUnit(q) ?? 1.0);
            value *= arFactors.For(qualifier);
        }

        if (applyFontScale && _fontScale.FontScale > 0)
            value *= _fontScale.FontScale;

        return value;
    }

    // ─────────────────────────────────────────────────────────────────────
    // LIVE SHORTCUTS (auto-adjust on resize)
    // ─────────────────────────────────────────────────────────────────────

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

    // ─────────────────────────────────────────────────────────────────────
    // INDEPENDENT SHORTCUTS (*i — frozen against the baseline; *ia adds AR)
    // ─────────────────────────────────────────────────────────────────────

    public double Sdpi(int v, InverterType inv = InverterType.Default) =>
        ResolveIndependent(v, DpQualifier.SmallWidth, inv);

    public double Sdpia(int v, InverterType inv = InverterType.Default) =>
        ResolveIndependent(v, DpQualifier.SmallWidth, inv, applyAspectRatio: true);

    public double Hdpi(int v, InverterType inv = InverterType.Default) =>
        ResolveIndependent(v, DpQualifier.Height, inv);

    public double Hdpia(int v, InverterType inv = InverterType.Default) =>
        ResolveIndependent(v, DpQualifier.Height, inv, applyAspectRatio: true);

    public double Wdpi(int v, InverterType inv = InverterType.Default) =>
        ResolveIndependent(v, DpQualifier.Width, inv);

    public double Wdpia(int v, InverterType inv = InverterType.Default) =>
        ResolveIndependent(v, DpQualifier.Width, inv, applyAspectRatio: true);

    public double Sspi(int v, InverterType inv = InverterType.Default) =>
        ResolveIndependent(v, DpQualifier.SmallWidth, inv, applyFontScale: true, allowNegative: false);

    public double Sspia(int v, InverterType inv = InverterType.Default) =>
        ResolveIndependent(v, DpQualifier.SmallWidth, inv, applyAspectRatio: true, applyFontScale: true, allowNegative: false);

    public double Hspi(int v, InverterType inv = InverterType.Default) =>
        ResolveIndependent(v, DpQualifier.Height, inv, applyFontScale: true, allowNegative: false);

    public double Wspi(int v, InverterType inv = InverterType.Default) =>
        ResolveIndependent(v, DpQualifier.Width, inv, applyFontScale: true, allowNegative: false);

    public double Semi(int v, InverterType inv = InverterType.Default) =>
        ResolveIndependent(v, DpQualifier.SmallWidth, inv, allowNegative: false);

    public double Semia(int v, InverterType inv = InverterType.Default) =>
        ResolveIndependent(v, DpQualifier.SmallWidth, inv, applyAspectRatio: true, allowNegative: false);

    public double Hemi(int v, InverterType inv = InverterType.Default) =>
        ResolveIndependent(v, DpQualifier.Height, inv, allowNegative: false);

    public double Wemi(int v, InverterType inv = InverterType.Default) =>
        ResolveIndependent(v, DpQualifier.Width, inv, allowNegative: false);

    // ─────────────────────────────────────────────────────────────────────
    // CONFIGURATION / TESTING
    // ─────────────────────────────────────────────────────────────────────

    public void SetFontScale(float scale)
    {
        _fontScale = new DefaultFontScaleService { FontScale = scale };
        // Font scale participates in cache keys; nothing else to invalidate.
    }

    public void SetMetricsForTesting(double widthDp, double heightDp, double density = 2.0,
        ScreenOrientation? orientation = null)
    {
        _testMode = true;
        _metrics.Update(widthDp, heightDp, density, orientation: orientation);
    }

    /// <summary>Freezes an explicit baseline for tests without touching the test-mode flag.</summary>
    public void CaptureBaselineForTesting(double widthDp, double heightDp, double density = 2.0,
        ScreenOrientation? orientation = null)
    {
        SetMetricsForTesting(widthDp, heightDp, density, orientation);
        CaptureBaseline();
        _baselineMetrics = _metrics.Current;
    }
}

/// <summary>Static facade over <see cref="AppDimensResolver"/>.</summary>
public static class AppDimensSdps
{
    public static AppDimensResolver Resolver => AppDimensResolver.Instance;

    public static void Initialize(AppDimensOptions? options = null, string? generatedPath = null)
        => Resolver.Initialize(options, generatedPath);

    public static void Warmup() => Resolver.Warmup();

    /// <summary>Freezes the current screen as the baseline for all *i (independent) APIs.</summary>
    public static void CaptureBaseline() => Resolver.CaptureBaseline();

    // Live — auto-adjust on resize.
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

    // Independent (*i) — frozen against the baseline; do NOT adjust on resize.
    public static double Sdpi(int value) => Resolver.Sdpi(value);
    public static double Sdpia(int value) => Resolver.Sdpia(value);
    public static double Hdpi(int value) => Resolver.Hdpi(value);
    public static double Hdpia(int value) => Resolver.Hdpia(value);
    public static double Wdpi(int value) => Resolver.Wdpi(value);
    public static double Wdpia(int value) => Resolver.Wdpia(value);
    public static double Sspi(int value) => Resolver.Sspi(value);
    public static double Sspia(int value) => Resolver.Sspia(value);
    public static double Hspi(int value) => Resolver.Hspi(value);
    public static double Wspi(int value) => Resolver.Wspi(value);
    public static double Semi(int value) => Resolver.Semi(value);
    public static double Semia(int value) => Resolver.Semia(value);
    public static double Hemi(int value) => Resolver.Hemi(value);
    public static double Wemi(int value) => Resolver.Wemi(value);
}

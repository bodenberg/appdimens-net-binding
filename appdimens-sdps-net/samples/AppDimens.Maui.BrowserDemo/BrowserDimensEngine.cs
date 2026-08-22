using AppDimens.Maui.Core;
using AppDimens.Maui.Inverters;
using AppDimens.Maui.Responsive;

namespace AppDimens.Maui.BrowserDemo;

/// <summary>
/// Server-side dimension engine for the browser demo: Android-parity pre-calculated
/// bucket tables plus the shared <see cref="ScaleEngine"/>/<see cref="InverterEngine"/>.
/// Live values follow the browser window resize; <c>*i</c> values stay frozen against
/// the baseline captured at page load. Mirrors <c>AppDimensResolver</c>: separate
/// bucket managers for live and baseline resolution.
/// </summary>
public sealed class BrowserDimensEngine
{
    private readonly BucketRegistry? _registry;
    private readonly ResourceBucketManager? _liveBuckets;
    private ResourceBucketManager? _baselineBuckets;
    private readonly AspectRatioFactors _ar = new();
    private ScreenMetricsSnapshot _baseline;

    public BrowserDimensEngine(BucketRegistry? registry)
    {
        _registry = registry;
        _liveBuckets = registry is null ? null : new ResourceBucketManager(registry);
    }

    public MutableScreenMetricsProvider Metrics { get; } = CreateInitial();

    public bool BucketsLoaded => _registry is not null;
    public ScreenMetricsSnapshot Baseline => _baseline;

    private ResourceBucketManager? BaselineBuckets =>
        _registry is null ? null : _baselineBuckets ??= new ResourceBucketManager(_registry);

    private static MutableScreenMetricsProvider CreateInitial()
    {
        var p = new MutableScreenMetricsProvider();
        p.Update(360, 800, 2.0);
        return p;
    }

    /// <summary>Called from JS on every window resize event.</summary>
    public void UpdateWindow(double widthCss, double heightCss, double devicePixelRatio)
    {
        var density = devicePixelRatio > 0 ? devicePixelRatio : 1.0;
        Metrics.Update(widthCss, heightCss, density, (int)Math.Round(160 * density),
            widthCss > heightCss ? ScreenOrientation.Landscape : ScreenOrientation.Portrait);
        _liveBuckets?.EnsureUpToDate(Metrics.Current);
    }

    /// <summary>Freezes the current window as the baseline for all *i values.</summary>
    public void CaptureBaseline()
    {
        _baseline = Metrics.Current;
        BaselineBuckets?.EnsureUpToDate(_baseline);
    }

    private double Compute(int index, DpQualifier qualifier, ScreenMetricsSnapshot m,
        bool aspectRatio, bool baseline)
    {
        var effective = InverterEngine.EffectiveQualifier(m, qualifier, InverterType.Default);
        var metric = ScaleEngine.GetMetricDp(effective, m);
        var key = ScaleEngine.BuildResourceKey(index, effective);
        var buckets = baseline ? _baselineBuckets : _liveBuckets;

        double value;
        if (buckets != null && buckets.TryGetDimen(effective, key, out var bucketValue))
            value = bucketValue; // pre-calculated Android-parity value
        else
            value = ScaleEngine.Scale(index, metric); // continuous fallback

        if (aspectRatio)
        {
            var arFactors = baseline ? new AspectRatioFactors() : _ar;
            arFactors.EnsureUpToDate(m, q =>
            {
                var k = ScaleEngine.BuildResourceKey(1, q);
                return buckets != null && buckets.TryGetDimen(q, k, out var one) ? one : 1.0;
            });
            value *= arFactors.For(effective);
        }
        return value;
    }

    // Live — auto-adjust on browser resize.
    public double Sdp(int v) => Compute(v, DpQualifier.SmallWidth, Metrics.Current, aspectRatio: false, baseline: false);
    public double Hdp(int v) => Compute(v, DpQualifier.Height, Metrics.Current, aspectRatio: false, baseline: false);
    public double Wdp(int v) => Compute(v, DpQualifier.Width, Metrics.Current, aspectRatio: false, baseline: false);
    public double Sdpa(int v) => Compute(v, DpQualifier.SmallWidth, Metrics.Current, aspectRatio: true, baseline: false);

    // Independent (*i) — frozen against the baseline.
    public double Sdpi(int v) => Compute(v, DpQualifier.SmallWidth, _baseline, aspectRatio: false, baseline: true);
    public double Hdpi(int v) => Compute(v, DpQualifier.Height, _baseline, aspectRatio: false, baseline: true);
    public double Wdpi(int v) => Compute(v, DpQualifier.Width, _baseline, aspectRatio: false, baseline: true);
    public double Sdpia(int v) => Compute(v, DpQualifier.SmallWidth, _baseline, aspectRatio: true, baseline: true);
}

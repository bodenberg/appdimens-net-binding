using AppDimens.Maui.Core;
using AppDimens.Maui.Inverters;

namespace AppDimens.Maui.BrowserDemo;

/// <summary>
/// Browser-side dimension engine: Android-parity pre-calculated bucket tables loaded
/// over HTTP plus the shared <see cref="ScaleEngine"/>/<see cref="InverterEngine"/>.
/// Live values follow resizes; <c>*i</c> values stay frozen against the baseline.
/// </summary>
public sealed class BrowserDimensEngine(HttpClient http)
{
    private readonly BrowserBucketTable _table = new(http);
    private readonly AspectRatioFactors _ar = new();
    private ScreenMetricsSnapshot _baseline;

    public MutableScreenMetricsProvider Metrics { get; } = CreateInitial();

    public bool BucketsLoaded => _table.Loaded;
    public ScreenMetricsSnapshot Baseline => _baseline;

    private static MutableScreenMetricsProvider CreateInitial()
    {
        var p = new MutableScreenMetricsProvider();
        p.Update(360, 800, 2.0);
        return p;
    }

    private static ScreenMetricsSnapshot Snapshot(double w, double h, double density) =>
        new(w, h, Math.Min(w, h), density, (int)Math.Round(160 * density),
            w > h ? ScreenOrientation.Landscape : ScreenOrientation.Portrait,
            UiModeType.Normal);

    /// <summary>Loads bucket metadata + base values. Call once at startup.</summary>
    public Task InitializeAsync() => _table.LoadAsync();

    /// <summary>Called from JS on every window resize event.</summary>
    public void UpdateWindow(double widthCss, double heightCss, double devicePixelRatio)
    {
        var density = devicePixelRatio > 0 ? devicePixelRatio : 1.0;
        Metrics.Update(Snapshot(widthCss, heightCss, density));
    }

    /// <summary>Freezes the current window as the baseline for all *i values.</summary>
    public void CaptureBaseline() => _baseline = Metrics.Current;

    /// <summary>
    /// Pre-fetches the bucket files for the given snapshot's three axes so subsequent
    /// synchronous computations use pre-calculated values.
    /// </summary>
    public Task EnsureBucketsAsync(ScreenMetricsSnapshot m) => _table.PrefetchAsync(m.SmallestDp, m.WidthDp, m.HeightDp);

    private double Compute(int index, DpQualifier qualifier, ScreenMetricsSnapshot m,
        bool aspectRatio)
    {
        var effective = InverterEngine.EffectiveQualifier(m, qualifier, InverterType.Default);
        var metric = ScaleEngine.GetMetricDp(effective, m);
        var key = ScaleEngine.BuildResourceKey(index, effective);

        double value;
        if (_table.TryGetCached(metric, key, out var bucketValue))
            value = bucketValue; // pre-calculated Android-parity value
        else
            value = ScaleEngine.Scale(index, metric); // continuous fallback

        if (aspectRatio)
        {
            _ar.EnsureUpToDate(m, q =>
            {
                var k = ScaleEngine.BuildResourceKey(1, q);
                return _table.TryGetCached(ScaleEngine.GetMetricDp(q, m), k, out var one)
                    ? one : 1.0;
            });
            value *= _ar.For(effective);
        }
        return value;
    }

    // Live — auto-adjust on browser resize.
    public double Sdp(int v) => Compute(v, DpQualifier.SmallWidth, Metrics.Current, aspectRatio: false);
    public double Hdp(int v) => Compute(v, DpQualifier.Height, Metrics.Current, aspectRatio: false);
    public double Wdp(int v) => Compute(v, DpQualifier.Width, Metrics.Current, aspectRatio: false);
    public double Sdpa(int v) => Compute(v, DpQualifier.SmallWidth, Metrics.Current, aspectRatio: true);

    // Independent (*i) — frozen against the baseline.
    public double Sdpi(int v) => Compute(v, DpQualifier.SmallWidth, _baseline, aspectRatio: false);
    public double Hdpi(int v) => Compute(v, DpQualifier.Height, _baseline, aspectRatio: false);
    public double Wdpi(int v) => Compute(v, DpQualifier.Width, _baseline, aspectRatio: false);
    public double Sdpia(int v) => Compute(v, DpQualifier.SmallWidth, _baseline, aspectRatio: true);
}

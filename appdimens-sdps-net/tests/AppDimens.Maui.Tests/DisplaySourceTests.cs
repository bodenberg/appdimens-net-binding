using AppDimens.Maui.Core;
using Xunit;

namespace AppDimens.Maui.Tests;

/// <summary>
/// Host-agnostic metrics (WPF / WinForms / WinUI adapters plug in through
/// <see cref="IDisplayMetricsSource"/>): the resolver must follow the source without MAUI.
/// </summary>
public class DisplaySourceTests
{
    private static (AppDimensResolver R, ManualDisplayMetricsSource Source) Fresh(double w, double h, double density = 1.0)
    {
        var r = AppDimensResolver.Instance;
        r.ResetForTesting();
        var source = new ManualDisplayMetricsSource(w, h, density);
        r.UseDisplaySource(source);
        r.Initialize(new AppDimensOptions { ScalingMode = ScalingMode.Continuous, WarmupAspectRatio = false }, force: true);
        return (r, source);
    }

    [Fact]
    public void Initialize_reads_metrics_from_custom_source()
    {
        var (r, source) = Fresh(1280, 720, 1.5);

        Assert.Same(source, r.DisplaySource);
        Assert.Equal(1280, r.Metrics.Current.WidthDp);
        Assert.Equal(720, r.Metrics.Current.HeightDp);
        Assert.Equal(ScaleEngine.Sdp(16, 720), r.Sdp(16), precision: 2);
    }

    [Fact]
    public void Source_changes_resize_live_values_but_not_independent_ones()
    {
        var (r, source) = Fresh(800, 600);
        var sdpi = r.Sdpi(16);

        source.Update(1600, 1200);

        Assert.Equal(ScaleEngine.Sdp(16, 1200), r.Sdp(16), precision: 2);
        Assert.Equal(sdpi, r.Sdpi(16), precision: 2);
    }

    [Fact]
    public void Source_without_metrics_keeps_last_snapshot()
    {
        var (r, source) = Fresh(1024, 768);
        source.Update(0, 0);

        Assert.Equal(1024, r.Metrics.Current.WidthDp);
        Assert.Equal(768, r.Metrics.Current.HeightDp);
    }

    [Fact]
    public void UseDisplaySource_after_initialize_switches_and_refreshes()
    {
        var (r, _) = Fresh(400, 800);
        var other = new ManualDisplayMetricsSource(900, 1600, 2.0);

        r.UseDisplaySource(other);

        Assert.Equal(900, r.Metrics.Current.WidthDp);
        other.Update(1000, 1700, 2.0);
        Assert.Equal(1000, r.Metrics.Current.WidthDp);
    }

    [Fact]
    public void Maui_hosting_source_is_resolved_by_default()
    {
        var r = AppDimensResolver.Instance;
        r.ResetForTesting();
        r.Initialize(new AppDimensOptions { ScalingMode = ScalingMode.Continuous, WarmupAspectRatio = false }, force: true);

        // AppDimens.Maui.Hosting is deployed with the MAUI meta-package: picked up by name.
        Assert.IsType<MauiDisplayMetricsSource>(r.DisplaySource);
    }

    [Fact]
    public void Throwing_source_does_not_break_resolver()
    {
        var r = AppDimensResolver.Instance;
        r.ResetForTesting();
        r.UseDisplaySource(new ThrowingSource());
        r.Initialize(new AppDimensOptions { ScalingMode = ScalingMode.Continuous, WarmupAspectRatio = false }, force: true);

        Assert.True(r.Sdp(16) > 0);
    }

    private sealed class ThrowingSource : IDisplayMetricsSource
    {
        public event EventHandler? Changed { add { } remove { } }

        public bool TryGetMetrics(out double widthDp, out double heightDp, out double density)
            => throw new InvalidOperationException("host not ready");
    }
}

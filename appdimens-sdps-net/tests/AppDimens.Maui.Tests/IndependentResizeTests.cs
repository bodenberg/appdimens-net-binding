using AppDimens.Maui;
using AppDimens.Maui.Builders;
using AppDimens.Maui.Converters;
using AppDimens.Maui.Core;
using AppDimens.Maui.Extensions;
using AppDimens.Maui.Inverters;
using Xunit;

namespace AppDimens.Maui.Tests;

/// <summary>
/// Contract for the <c>i</c> suffix: live APIs auto-adjust when the screen/window is
/// resized; every <c>*i</c> API stays frozen against the baseline snapshot.
/// </summary>
public class IndependentResizeTests
{
    private static AppDimensResolver Fresh(AppDimensOptions? options = null)
    {
        var r = AppDimensResolver.Instance;
        r.ResetForTesting();
        r.Initialize(options ?? new AppDimensOptions
        {
            ScalingMode = ScalingMode.Continuous,
            WarmupAspectRatio = false,
        }, force: true);
        return r;
    }

    private static void SimulateResize(AppDimensResolver r, double w, double h, double density = 2.0)
        => ((MutableScreenMetricsProvider)r.Metrics).Update(
            w, h, density, 320,
            w > h ? ScreenOrientation.Landscape : ScreenOrientation.Portrait);

    [Fact]
    public void Sdp_adjusts_on_resize_but_Sdpi_stays_frozen()
    {
        var r = Fresh();
        r.SetMetricsForTesting(360, 800, 2.0);
        r.CaptureBaseline();

        var sdpBefore = r.Sdp(16);
        var sdpiBefore = r.Sdpi(16);
        Assert.Equal(16 * 360.0 / 300.0, sdpBefore, precision: 2);
        Assert.Equal(sdpBefore, sdpiBefore, precision: 2);

        // Window resized from phone to tablet.
        SimulateResize(r, 800, 1280);

        Assert.Equal(16 * 800.0 / 300.0, r.Sdp(16), precision: 2);   // live follows resize
        Assert.Equal(sdpiBefore, r.Sdpi(16), precision: 2);          // independent frozen
    }

    [Theory]
    [InlineData(DpQualifier.Height)]
    [InlineData(DpQualifier.Width)]
    public void Axis_independent_values_freeze_on_resize(DpQualifier qualifier)
    {
        var r = Fresh();
        r.SetMetricsForTesting(360, 800, 2.0);
        r.CaptureBaseline();

        var before = qualifier == DpQualifier.Height ? r.Hdpi(24) : r.Wdpi(24);
        SimulateResize(r, 800, 360, 2.0);

        var after = qualifier == DpQualifier.Height ? r.Hdpi(24) : r.Wdpi(24);
        Assert.Equal(before, after, precision: 3);
    }

    [Fact]
    public void Sdpia_applies_baseline_aspect_ratio_and_stays_frozen()
    {
        var r = Fresh();
        r.SetMetricsForTesting(360, 800, 2.0);
        r.CaptureBaseline();

        var iaBefore = r.Sdpia(32);
        Assert.NotEqual(r.Sdpi(32), iaBefore, precision: 3); // AR 800/360 ≠ 1.78

        SimulateResize(r, 300, 300); // square screen, AR factor becomes ~1
        Assert.Equal(iaBefore, r.Sdpia(32), precision: 3);
    }

    [Fact]
    public void Text_independent_Sspi_frozen_on_resize()
    {
        var r = Fresh();
        r.SetMetricsForTesting(360, 800, 2.0);
        r.CaptureBaseline();

        var before = r.Sspi(14);
        SimulateResize(r, 720, 1280);
        Assert.Equal(before, r.Sspi(14), precision: 3);
        Assert.Equal(before, 14.Sspi(), precision: 3); // int extension parity
    }

    [Fact]
    public void Int_extensions_match_resolver_for_live_and_independent()
    {
        var r = Fresh();
        r.SetMetricsForTesting(411, 823, 2.0);
        r.CaptureBaseline();

        Assert.Equal(r.Sdp(20), 20.Sdp(), precision: 6);
        Assert.Equal(r.Sdpi(20), 20.Sdpi(), precision: 6);
        Assert.Equal(r.Sdpia(20), 20.Sdpia(), precision: 6);
        Assert.Equal(r.Hdpi(48), 48.Hdpi(), precision: 6);
        Assert.Equal(r.Wdpia(100), 100.Wdpia(), precision: 6);
    }

    [Fact]
    public void CaptureBaseline_refreezes_to_current_screen()
    {
        var r = Fresh();
        r.SetMetricsForTesting(360, 800, 2.0);
        r.CaptureBaseline();
        var first = r.Sdpi(16);

        SimulateResize(r, 600, 1000);
        r.CaptureBaseline();
        var second = r.Sdpi(16);

        Assert.Equal(16 * 600.0 / 300.0, second, precision: 2);
        Assert.NotEqual(first, second, precision: 2);
    }

    [Fact]
    public void Baseline_is_captured_at_initialization()
    {
        var r = Fresh();
        Assert.NotNull(r.BaselineMetrics);
        var baseline = r.BaselineMetrics!.Value;

        SimulateResize(r, 1024, 1366);
        Assert.Equal(baseline, r.BaselineMetrics!.Value); // untouched by resizes
        Assert.NotEqual(baseline, r.Metrics.Current);     // live moved on
    }

    [Fact]
    public void Independent_uses_bucket_tables_of_the_baseline()
    {
        var generated = TestPaths.GeneratedResources;
        if (generated is null || !Directory.Exists(generated)) return; // requires generated tables

        var r = AppDimensResolver.Instance;
        r.ResetForTesting();
        r.Initialize(new AppDimensOptions { ScalingMode = ScalingMode.Bucket, WarmupAspectRatio = false },
            generated, force: true);
        r.SetMetricsForTesting(411, 823, 2.0);
        r.CaptureBaseline();

        var expectedBucketValue = 16 * 411.0 / 300.0;
        Assert.Equal(expectedBucketValue, r.Sdpi(16), precision: 2);

        r.SetMetricsForTesting(720, 1280, 2.0);
        Assert.Equal(expectedBucketValue, r.Sdpi(16), precision: 2);   // frozen
        Assert.Equal(16 * 720.0 / 300.0, r.Sdp(16), precision: 2);     // live re-bucketed
    }
}

public class ResizeIntegrationTests
{
    private static AppDimensResolver Fresh()
    {
        var r = AppDimensResolver.Instance;
        r.ResetForTesting();
        r.Initialize(new AppDimensOptions { ScalingMode = ScalingMode.Continuous, WarmupAspectRatio = false },
            force: true);
        return r;
    }

    /// <summary>Regression: changing font scale must affect newly resolved sp values.</summary>
    [Fact]
    public void FontScale_change_is_respected_by_Ssp_and_Semi()
    {
        var r = Fresh();
        r.SetMetricsForTesting(411, 823, 2.0);
        r.CaptureBaseline();
        r.SetFontScale(1f);
        var normal = r.Ssp(16);
        Assert.Equal(16 * 411.0 / 300.0 * 1f, normal, precision: 2);

        r.SetFontScale(1.3f);
        var bigger = r.Ssp(16);
        Assert.Equal(normal * 1.3, bigger, precision: 2);

        var semiNormal = r.Semi(16);
        Assert.Equal(16 * 411.0 / 300.0, semiNormal, precision: 2); // Semi ignores font scale
    }

    /// <summary>Regression: HdpPw must read the WIDTH axis in portrait (was a no-op).</summary>
    [Fact]
    public void HdpPw_portrait_reads_width_axis()
    {
        var r = Fresh();
        r.SetMetricsForTesting(411, 823, 2.0, ScreenOrientation.Portrait);

        var viaExtension = 32.HdpPw();
        Assert.Equal(r.Wdp(32), viaExtension, precision: 4);
        Assert.Equal(DpQualifier.Width, InverterEngine.EffectiveQualifier(
            r.Metrics.Current, DpQualifier.Height, InverterType.LhToPw));
    }

    /// <summary>Regression: WdpPh must read the HEIGHT axis in portrait (was a no-op).</summary>
    [Fact]
    public void WdpPh_portrait_reads_height_axis()
    {
        var r = Fresh();
        r.SetMetricsForTesting(411, 823, 2.0, ScreenOrientation.Portrait);

        var viaExtension = 120.WdpPh();
        Assert.Equal(r.Hdp(120), viaExtension, precision: 4);
        Assert.Equal(DpQualifier.Height, InverterEngine.EffectiveQualifier(
            r.Metrics.Current, DpQualifier.Width, InverterType.LwToPh));
    }

    [Fact]
    public void Landscape_shortcuts_keep_android_parity()
    {
        var r = Fresh();
        r.SetMetricsForTesting(823, 411, 2.0, ScreenOrientation.Landscape);

        Assert.Equal(r.Wdp(32), 32.HdpLw(), precision: 4); // hdpLw → landscape height reads width
        Assert.Equal(r.Hdp(120), 120.WdpLh(), precision: 4); // wdpLh → landscape width reads height
        Assert.Equal(r.Sdp(50), 50.SdpPh(), precision: 4);   // sdpPh keeps sw in landscape
        Assert.Equal(r.Wdp(50), 50.SdpLw(), precision: 4);   // sdpLw → landscape width
        Assert.Equal(r.Hdp(50), 50.SdpLh(), precision: 4);   // sdpLh → landscape height
        Assert.Equal(r.Sdp(50), 50.SdpPw(), precision: 4);   // sdpPw keeps sw in landscape
    }

    /// <summary>Physical units: one inch of any unit converts exactly to dpi pixels.</summary>
    [Theory]
    [InlineData(25.4, 1.0, 160.0)]   // 1 inch @ mdpi
    [InlineData(25.4, 2.0, 320.0)]   // 1 inch @ xhdpi
    [InlineData(2.54, 2.0, 32.0)]    // 1 cm  @ xhdpi → 0.1 inch
    public void Physical_units_convert_exactly(double mm, double density, double expectedPx)
    {
        Assert.Equal(expectedPx, DimenPhysicalUnits.MmToPx(mm, density), precision: 4);
        Assert.Equal(expectedPx, DimenPhysicalUnits.InchToPx(mm / 25.4, density), precision: 4);
        Assert.Equal(expectedPx, DimenPhysicalUnits.CmToPx(mm / 10.0, density), precision: 4);
    }
}

public class BuilderPerformanceTests
{
    /// <summary>The builder resolves identically regardless of entry insertion order.</summary>
    [Fact]
    public void Priority_ordering_is_insertion_order_independent()
    {
        var r = AppDimensResolver.Instance;
        r.ResetForTesting();
        r.Initialize(new AppDimensOptions { ScalingMode = ScalingMode.Continuous, WarmupAspectRatio = false },
            force: true);
        r.SetMetricsForTesting(720, 1280, 2.0);

        var a = Builders.Responsive.Value(14)
            .Screen(UiModeType.Desk, 40)
            .Tablet(18)
            .Landscape(22)
            .Sdp();

        var b = Builders.Responsive.Value(14)
            .Landscape(22)
            .Screen(UiModeType.Desk, 40)
            .Tablet(18)
            .Sdp();

        Assert.Equal(a, b, precision: 6);
        // Priority 3 (qualifier) beats priority 4 (orientation-only).
        Assert.Equal(18 * 720.0 / 300.0, b, precision: 2);
    }
}

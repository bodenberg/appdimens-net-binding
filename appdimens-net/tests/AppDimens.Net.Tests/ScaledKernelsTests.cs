using AppDimens.Net;
using AppDimens.Net.Code.Scaled;
using AppDimens.Net.Common;
using AppDimens.Net.Core;
using AppDimens.Net.Testing;
using Xunit;

namespace AppDimens.Net.Tests;

public class ScaledKernelsTests
{
    [Theory]
    [InlineData(300, 533, 160)]
    [InlineData(360, 740, 420)]
    [InlineData(800, 1280, 240)]
    public void Sdp_fast_lane_matches_full_path(int w, int h, int dpi)
    {
        var ctx = new FakeAppDimensContext(new ScreenConfiguration(w, h, Math.Min(w, h), dpi, 1f,
            ScreenConfiguration.OrientationUndefined, 0));
        for (var v = -40; v <= 600; v += 37)
        {
            var fast = DimenCache.ResolveSdpDp(v, ctx);
            var full = ((float)v).ToDynamicScaledDp(ctx);
            Assert.Equal(full, fast, 3);
        }
    }

    [Fact]
    public void Sdp_at_base_width_is_identity_dp()
    {
        var ctx = new FakeAppDimensContext(new ScreenConfiguration(300, 533, 300, 160, 1f,
            ScreenConfiguration.OrientationUndefined, 0));
        Assert.Equal(16f * 300f * DimenCache.InvBaseRatio, 16.Sdp(ctx));
    }

    [Fact]
    public void Px_lanes_multiply_density_in_legacy_order()
    {
        var ctx = Fx.Phone();
        var dp = DimenCache.ResolveSdpDp(16f, ctx);
        var px = DimenCache.ResolveSdpPx(16f, ctx);
        Assert.Equal(dp * ctx.Density, px, 2);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Suffix_i_returns_unscaled_base_when_multi_window(bool constrained)
    {
        var ctx = new FakeAppDimensContext(new ScreenConfiguration(342, 740, 342, 420, 1f,
            ScreenConfiguration.OrientationPortrait, 0))
        { IsInMultiWindowMode = constrained };
        var heuristic = Fx.HeuristicMw(ctx.Configuration);

        var vi = 24.Sdpi(ctx);
        if (constrained || heuristic)
        {
            Assert.Equal(24f, vi);
            Assert.Equal(24f, 24.Sdpia(ctx)); // ia still respects i
        }
        else
        {
            Assert.NotEqual(24f, vi);
        }
    }

    [Fact]
    public void Inverters_swap_axes_exactly_like_kotlin()
    {
        // landscape phone
        var land = new FakeAppDimensContext(new ScreenConfiguration(740, 360, 360, 420, 1f,
            ScreenConfiguration.OrientationLandscape, 0));

        // PhToLw: in landscape HEIGHT reads WIDTH axis
        var viaPh = 32f.ToDynamicScaledDp(land, DpQualifier.Height, Inverter.PhToLw);
        Assert.Equal(DimenCache.ResolveWdpDp(32f, land), viaPh, 3);

        // PwToLh: in landscape WIDTH reads HEIGHT axis
        var viaPw = 32f.ToDynamicScaledDp(land, DpQualifier.Width, Inverter.PwToLh);
        Assert.Equal(DimenCache.ResolveHdpDp(32f, land), viaPw, 3);

        // portrait
        var port = new FakeAppDimensContext(new ScreenConfiguration(360, 740, 360, 420, 1f,
            ScreenConfiguration.OrientationPortrait, 0));

        var lh = 50f.ToDynamicScaledDp(port, DpQualifier.Height, Inverter.LhToPw);
        Assert.Equal(DimenCache.ResolveWdpDp(50f, port), lh, 3);

        var swPh = 32f.ToDynamicScaledDp(port, DpQualifier.SmallWidth, Inverter.SwToPh);
        Assert.Equal(DimenCache.ResolveHdpDp(32f, port), swPh, 3);
    }

    [Fact]
    public void Custom_sensitivity_never_hits_cache()
    {
        var ctx = Fx.Phone();
        var k1 = 16f.ToDynamicScaledDp(ctx, applyAspectRatio: true, customSensitivityK: 0.02f);
        var k2 = 16f.ToDynamicScaledDp(ctx, applyAspectRatio: true, customSensitivityK: 0.9f);
        Assert.NotEqual(k1, k2);
        // same K is stable across calls
        Assert.Equal(k1, 16f.ToDynamicScaledDp(ctx, applyAspectRatio: true, customSensitivityK: 0.02f));
    }

    [Fact]
    public void Facilitators_route_to_expected_branches()
    {
        var land = new FakeAppDimensContext(new ScreenConfiguration(740, 360, 360, 420, 1f,
            ScreenConfiguration.OrientationLandscape, 0));
        var rotated = 16.SdpRotate(land, 28, Orientation.Landscape);
        Assert.Equal(((float)28).ToDynamicScaledDp(land), rotated, 3);

        var portrait = Fx.Phone();
        var notRotated = 16.SdpRotate(portrait, 28, Orientation.Landscape);
        Assert.Equal(16.Sdp(portrait), notRotated, 3);

        var portraitCtx = new FakeAppDimensContext(new ScreenConfiguration(360, 740, 360,
            420, 1f, ScreenConfiguration.OrientationPortrait, 0));
        var raw = 16.SdpRotateRaw(portraitCtx, 99f, Orientation.Portrait);
        Assert.Equal(99f, raw);
    }
}

using AppDimens.Net;
using AppDimens.Net.Code.Auto;
using AppDimens.Net.Code.Scaled;
using AppDimens.Net.Code.Density;
using AppDimens.Net.Code.Diagonal;
using AppDimens.Net.Code.Fill;
using AppDimens.Net.Code.Fluid;
using AppDimens.Net.Code.Interpolated;
using AppDimens.Net.Code.Logarithmic;
using AppDimens.Net.Code.Percent;
using AppDimens.Net.Code.Perimeter;
using AppDimens.Net.Code.Power;
using AppDimens.Net.Core;
using AppDimens.Net.Testing;
using Xunit;

using AppDimens.Net.Common;

namespace AppDimens.Net.Tests;

/// <summary>Every satellite kernel must equal the independently recomputed formula.</summary>
public class SatellitesParityTests
{
    public static TheoryData<FakeAppDimensContext> Contexts() => new()
    {
        Fx.Phone(), Fx.Tablet(), Fx.Desktop(),
    };

    [Theory]
    [MemberData(nameof(Contexts))]
    public void Percent_matches_formula(FakeAppDimensContext ctx)
    {
        for (var v = 4f; v < 200f; v += 23f)
        {
            var expected = Fx.ExpectedScaledDp(v, ctx.Configuration);
            Assert.Equal(expected, v.ToPercentDp(ctx));
            Assert.Equal(expected * ctx.Density, v.ToPercentPx(ctx));
            // fast entries share the same math
            Assert.Equal(expected, ((int)v).PSdp(ctx));
        }
    }

    [Theory]
    [MemberData(nameof(Contexts))]
    public void Fluid_clamps_between_08_and_12(FakeAppDimensContext ctx)
    {
        var c = ctx.Configuration;
        var dim = (float)c.SmallestScreenWidthDp;

        var small = new FakeAppDimensContext(new ScreenConfiguration(200, 400, 200, c.DensityDpi, 1f, 0, 0));
        Assert.Equal(28f * 0.8f, ((float)28).ToFluidDp(small));

        var large = new FakeAppDimensContext(new ScreenConfiguration(900, 1400, 900, c.DensityDpi, 1f, 0, 0));
        Assert.Equal(28f * 1.2f, ((float)28).ToFluidDp(large));

        if (dim is > 320f and < 768f)
        {
            var expected = Fx.ExpectedFluidDp(28f, c);
            Assert.Equal(expected, ((float)28).ToFluidDp(ctx));
        }
    }

    [Theory]
    [MemberData(nameof(Contexts))]
    public void Auto_log_step_matches_formula(FakeAppDimensContext ctx)
    {
        var expected = Fx.ExpectedAutoDp(28f, ctx.Configuration);
        Assert.Equal(expected, ((float)28).ToAutoDp(ctx));
        Assert.Equal(expected, 28.ASdp(ctx));
    }

    [Theory]
    [MemberData(nameof(Contexts))]
    public void Factor_strategies_use_snapshot_factors(FakeAppDimensContext ctx)
    {
        var m = DimenMetrics.From(ctx.Configuration);

        Assert.Equal(20f * m.PowerScale, ((float)20).ToPowerDp(ctx));
        Assert.Equal(20f * m.DiagonalScale, ((float)20).ToDiagonalDp(ctx));
        Assert.Equal(20f * m.PerimeterScale, ((float)20).ToPerimeterDp(ctx));
        Assert.Equal(20f * m.LogarithmicScale, ((float)20).ToLogarithmicDp(ctx));
        Assert.Equal(20f * m.InterpolatedScale, ((float)20).ToInterpolatedDp(ctx));
        Assert.Equal(20f * m.Density, ((float)20).ToDensityDp(ctx));

        // extension sugar hits the same kernels
        Assert.Equal(20f * m.PowerScale, 20.PWSdp(ctx));
        Assert.Equal(20f * m.DiagonalScale, 20.DGSdp(ctx));
    }

    [Theory]
    [MemberData(nameof(Contexts))]
    public void Fill_uses_largest_side_ratio_and_ignores_qualifier(FakeAppDimensContext ctx)
    {
        var c = ctx.Configuration;
        var sm = (float)Math.Min(c.ScreenWidthDp, c.ScreenHeightDp);
        var lg = (float)Math.Max(c.ScreenWidthDp, c.ScreenHeightDp);
        var expected = 20f * MathF.Max(sm / 300f, lg / 533f);

        var viaQualifierWidth = ((float)20).ToFillDp(ctx, DpQualifier.Width);
        Assert.Equal(expected, viaQualifierWidth);
        Assert.Equal(expected, 20.FLSdp(ctx));
    }

    [Fact]
    public void Identity_anchors_at_reference_window()
    {
        var refCtx = new FakeAppDimensContext(new ScreenConfiguration(300, 533, 300, 160, 1f,
            ScreenConfiguration.OrientationUndefined, 0));
        var m = DimenMetrics.From(refCtx.Configuration);

        Assert.Equal(16f, 16.Sdp(refCtx), 3);              // scale == 1
        Assert.Equal(16f, 16.Sdpa(refCtx), 3);             // AR multiplier == 1 at sw=300
        Assert.Equal(18f, ((float)18).ToLogarithmicDp(refCtx), 2);   // log factor == 1
        Assert.Equal(18f, ((float)18).ToInterpolatedDp(refCtx), 2);  // interp == 1
        Assert.Equal(18f, ((float)18).ToPerimeterDp(refCtx), 2);     // 833/833
        Assert.Equal(18f, ((float)18).ToDiagonalDp(refCtx), 1);      // √(300²+533²)/611.63
        Assert.Equal(12f, ((float)12).ToDensityDp(refCtx), 3);       // dpi160 → density 1
        Assert.Equal(28f, ((float)28).ToAutoDp(refCtx), 3);          // 300 ≤ 480 → dim/300
        Assert.Equal(22.4f, ((float)28).ToFluidDp(refCtx), 3);       // ≤320 → ×0.8
        Assert.True(m.Scale > 0.9999f && m.Scale < 1.0001f);
    }

    [Fact]
    public void Aspect_ratio_multiplier_applies_to_satellites()
    {
        var ctx = Fx.Tablet(); // non-1:1 window
        var m = DimenMetrics.From(ctx.Configuration);
        var plain = ((float)20).ToPowerDp(ctx);
        var withAr = ((float)20).ToPowerDp(ctx, applyAspectRatio: true);
        Assert.Equal(plain * m.DefaultAspectRatioMultiplier, withAr);
    }
}

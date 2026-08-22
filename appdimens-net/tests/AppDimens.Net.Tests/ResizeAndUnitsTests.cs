using AppDimens.Net;
using AppDimens.Net.Common;
using AppDimens.Net.Code.Resize;
using AppDimens.Net.Code.Units;
using AppDimens.Net.Core;
using Xunit;

namespace AppDimens.Net.Tests;

public class ResizeAndUnitsTests
{
    [Fact]
    public void Steps_generation_edges()
    {
        var single = ResizeMath.BuildStepsPx(10f, 20f, 0f);
        Assert.Single(single);

        var steps = ResizeMath.BuildStepsPx(12f, 16f, 1f);
        Assert.Equal([12f, 13f, 14f, 15f, 16f], steps);

        // reversed range normalizes
        var rev = ResizeMath.BuildStepsPx(16f, 12f, 2f);
        Assert.Equal(12f, rev[0]);
        Assert.Equal(16f, rev[^1]);

        // capacity cap at 4096: buffer fills with lo + k*step; hi is only appended
        // when there is room left (KMP parity).
        var capped = ResizeMath.BuildStepsPx(0f, 100000f, 1f);
        Assert.True(capped.Length <= 4096);
        Assert.Equal(4095f, capped[^1]);
        var roomy = ResizeMath.BuildStepsPx(0f, 10_000f, 4f);
        Assert.Equal(10_000f, roomy[^1]);
    }

    [Fact]
    public void Binary_search_returns_largest_fitting()
    {
        var range = new ResizeRangePx(12f, 28f, 1f);
        Assert.Equal(28f, range.ResolveFitting(_ => true));
        Assert.Equal(0f, range.ResolveFitting(_ => false));
        Assert.Equal(17f, range.ResolveFitting(v => v <= 17.5f));
    }

    [Fact]
    public void Inner_dimensions_never_below_one_px()
    {
        var (w, h) = DimenResize.InnerMaxDimensionsPx(10f, 10f, 8f, 8f, 8f, 8f);
        Assert.Equal(1f, w);
        Assert.Equal(1f, h);
    }

    [Fact]
    public void Auto_resize_square_fits_box()
    {
        var ctx = Fx.Phone();
        var side = DimenResize.AutoResizeSquarePx(200f * ctx.Density, 100f * ctx.Density,
            minDp: 24, maxDp: 96, stepDp: 4, ctx: ctx);
        Assert.InRange(side, 24f, 96f);
        Assert.True(side * ctx.Density <= 100f * ctx.Density + 0.01f);
    }

    [Fact]
    public void Auto_resize_square_returns_largest_side_that_fits_inner_box()
    {
        // Regression: the limit must be converted from px to dp exactly once.
        var ctx = Fx.Phone(); // density 2.625 (dpi 420)
        var boxHeightPx = 100f * ctx.Density;
        var side = DimenResize.AutoResizeSquarePx(400f * ctx.Density, boxHeightPx,
            minDp: 16, maxDp: 200, stepDp: 4, ctx: ctx);
        Assert.Equal(100f, side); // largest step ≤ 100dp inner height
    }

    [Theory]
    [InlineData(25.4f)]
    public void Physical_units_are_consistent(float mmValue)
    {
        var ctx = Fx.Phone();
        // KMP parity quirk preserved verbatim: inch conversions also divide by
        // MM_TO_INCH_FACTOR, so ToDpFromInch(v) == ToDpFromMm(v) numerically.
        var dpFromMm = DimenPhysicalUnits.ToDpFromMm(mmValue, ctx);
        var dpFromInch = DimenPhysicalUnits.ToDpFromInch(mmValue, ctx);
        Assert.Equal(dpFromMm, dpFromInch);

        // px == dp * density
        Assert.Equal(dpFromMm * ctx.Density, DimenPhysicalUnits.ToPxFromMm(mmValue, ctx));

        // sp divides by density*fontScale
        var sp = DimenPhysicalUnits.ToSpFromMm(mmValue, ctx);
        Assert.Equal(dpFromMm / Math.Max(ctx.Configuration.FontScale, 0.01f), sp);

        Assert.Equal(DimenPhysicalUnits.ToDpFromCm(1f, ctx), DimenPhysicalUnits.ToDpFromMm(10f, ctx));
        Assert.Equal(dpFromMm / 2f, DimenPhysicalUnits.RadiusFromDiameter(mmValue, UnitType.Mm, ctx));
    }
}

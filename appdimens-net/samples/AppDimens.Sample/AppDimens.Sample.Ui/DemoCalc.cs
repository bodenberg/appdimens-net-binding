using AppDimens.Net;
using AppDimens.Net.Code.Auto;
using AppDimens.Net.Code.Density;
using AppDimens.Net.Code.Diagonal;
using AppDimens.Net.Code.Fill;
using AppDimens.Net.Code.Fit;
using AppDimens.Net.Code.Fluid;
using AppDimens.Net.Code.Interpolated;
using AppDimens.Net.Code.Logarithmic;
using AppDimens.Net.Code.Percent;
using AppDimens.Net.Code.Perimeter;
using AppDimens.Net.Code.Power;
using AppDimens.Net.Code.Scaled;
using AppDimens.Net.Common;
using AppDimens.Net.Core;

namespace AppDimens.Sample.Ui;

/// <summary>
/// Calculation strategy selector for the demo — mirrors the KMP
/// <c>DemoCalcStrategy</c> menu. Every example on the main page routes through
/// <see cref="Dp"/> so the whole screen re-computes with the chosen strategy.
/// </summary>
public enum DemoCalcStrategy
{
    Scaled,
    Percent,
    Power,
    Auto,
    Logarithmic,
    Fluid,
    Interpolated,
    Diagonal,
    Perimeter,
    Fit,
    Fill,
    Density,
}

public static class DemoCalc
{
    public static string LabelEn(this DemoCalcStrategy s) => s switch
    {
        DemoCalcStrategy.Scaled => "Scaled (default)",
        DemoCalcStrategy.Density => "Densidade",
        _ => s.ToString(),
    };

    public static string LabelPt(this DemoCalcStrategy s) => s switch
    {
        DemoCalcStrategy.Scaled => "Scaled (padrão)",
        DemoCalcStrategy.Logarithmic => "Logarítmico",
        DemoCalcStrategy.Interpolated => "Interpolado",
        DemoCalcStrategy.Perimeter => "Perímetro",
        DemoCalcStrategy.Density => "Densidade",
        _ => s.ToString(),
    };

    public static readonly DemoCalcStrategy[] All =
    [
        ..Enum.GetValues<DemoCalcStrategy>(),
    ];

    /// <summary>Smallest-width style dp for the strategy (<c>.sdp / .psdp / …</c>).</summary>
    public static float Dp(this int v, DemoCalcStrategy s, IAppDimensContext ctx,
        DpQualifier qualifier = DpQualifier.SmallWidth, Inverter inverter = Inverter.Default,
        bool imw = false, bool ar = false, float? k = null) =>
        s switch
        {
            DemoCalcStrategy.Scaled => v.ToDynamicScaledDp(ctx, qualifier, inverter, imw, ar, k),
            DemoCalcStrategy.Percent => ((float)v).ToPercentDp(ctx, qualifier, inverter, imw, ar, k),
            DemoCalcStrategy.Power => ((float)v).ToPowerDp(ctx, qualifier, inverter, imw, ar, k),
            DemoCalcStrategy.Auto => ((float)v).ToAutoDp(ctx, qualifier, inverter, imw, ar, k),
            DemoCalcStrategy.Logarithmic => ((float)v).ToLogarithmicDp(ctx, qualifier, inverter, imw, ar, k),
            DemoCalcStrategy.Fluid => ((float)v).ToFluidDp(ctx, qualifier, inverter, imw, ar, k),
            DemoCalcStrategy.Interpolated => ((float)v).ToInterpolatedDp(ctx, qualifier, inverter, imw, ar, k),
            DemoCalcStrategy.Diagonal => ((float)v).ToDiagonalDp(ctx, qualifier, inverter, imw, ar, k),
            DemoCalcStrategy.Perimeter => ((float)v).ToPerimeterDp(ctx, qualifier, inverter, imw, ar, k),
            DemoCalcStrategy.Fit => ((float)v).ToFitDp(ctx, qualifier, inverter, imw, ar, k),
            DemoCalcStrategy.Fill => ((float)v).ToFillDp(ctx, qualifier, inverter, imw, ar, k),
            DemoCalcStrategy.Density => ((float)v).ToDensityDp(ctx, qualifier, inverter, imw, ar, k),
            _ => v.ToDynamicScaledDp(ctx, qualifier, inverter, imw, ar, k),
        };

    /// <summary>Float-receiver variant of the strategy router — same semantics as <c>Dp</c>.</summary>
    public static float DpF(float v, DemoCalcStrategy s, IAppDimensContext ctx,
        DpQualifier qualifier = DpQualifier.SmallWidth, Inverter inverter = Inverter.Default,
        bool imw = false, bool ar = false, float? k = null) =>
        s switch
        {
            DemoCalcStrategy.Scaled => v.ToDynamicScaledDp(ctx, qualifier, inverter, imw, ar, k),
            DemoCalcStrategy.Percent => v.ToPercentDp(ctx, qualifier, inverter, imw, ar, k),
            DemoCalcStrategy.Power => v.ToPowerDp(ctx, qualifier, inverter, imw, ar, k),
            DemoCalcStrategy.Auto => v.ToAutoDp(ctx, qualifier, inverter, imw, ar, k),
            DemoCalcStrategy.Logarithmic => v.ToLogarithmicDp(ctx, qualifier, inverter, imw, ar, k),
            DemoCalcStrategy.Fluid => v.ToFluidDp(ctx, qualifier, inverter, imw, ar, k),
            DemoCalcStrategy.Interpolated => v.ToInterpolatedDp(ctx, qualifier, inverter, imw, ar, k),
            DemoCalcStrategy.Diagonal => v.ToDiagonalDp(ctx, qualifier, inverter, imw, ar, k),
            DemoCalcStrategy.Perimeter => v.ToPerimeterDp(ctx, qualifier, inverter, imw, ar, k),
            DemoCalcStrategy.Fit => v.ToFitDp(ctx, qualifier, inverter, imw, ar, k),
            DemoCalcStrategy.Fill => v.ToFillDp(ctx, qualifier, inverter, imw, ar, k),
            DemoCalcStrategy.Density => v.ToDensityDp(ctx, qualifier, inverter, imw, ar, k),
            _ => v.ToDynamicScaledDp(ctx, qualifier, inverter, imw, ar, k),
        };

    /// <summary>Screen-height axis dp for the strategy (<c>.hdp / .phdp / …</c>).</summary>
    public static float HDp(this int v, DemoCalcStrategy s, IAppDimensContext ctx,
        bool ar = false, bool imw = false) =>
        v.Dp(s, ctx, DpQualifier.Height, Inverter.Default, imw, ar);

    /// <summary>Screen-width axis dp for the strategy (<c>.wdp / .pwdp / …</c>).</summary>
    public static float WDp(this int v, DemoCalcStrategy s, IAppDimensContext ctx,
        bool ar = false, bool imw = false) =>
        v.Dp(s, ctx, DpQualifier.Width, Inverter.Default, imw, ar);

    /// <summary>sp for the strategy — px scaled then converted back through density·fontScale.</summary>
    public static float Sp(this int v, DemoCalcStrategy s, IAppDimensContext ctx)
    {
        var px = v.Dp(s, ctx) * ctx.Density;
        return px / Math.Max(ctx.Density * Math.Max(ctx.Configuration.FontScale, 0.01f), 0.01f);
    }

    /// <summary>
    /// §4 DimenScaled-style builder chain per strategy:
    /// base 100 → TV+sw≥600:250 → TV:500 → FOLD_OPEN:200 → sw≥600:150 → Landscape:120.
    /// </summary>
    public static float BuilderResultDp(DemoCalcStrategy s, IAppDimensContext ctx)
    {
        var c = ctx.Configuration;
        var isTv = ctx.UiModeType == UiModeType.Television;
        var sw = c.SmallestScreenWidthDp;
        int value =
            isTv && sw >= 600 ? 250 :
            isTv ? 500 :
            ctx.UiModeType == UiModeType.FoldOpen ? 200 :
            sw >= 600 ? 150 :
            c.Orientation == ScreenConfiguration.OrientationLandscape ? 120 : 100;
        return value.Dp(s, ctx);
    }
}

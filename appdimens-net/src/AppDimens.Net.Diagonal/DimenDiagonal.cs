using AppDimens.Net.Common;
using AppDimens.Net.Core;
using AppDimens.Net.Code.Scaled;

namespace AppDimens.Net.Code.Diagonal;

/// <summary>Diagonal strategy kernels — cached full path (parity with the Kotlin modules).</summary>
public static class DimenDiagonal
{
    public static float DGSdp(IAppDimensContext context, int value) => value.DGSdp(context);
    public static float DGSdpa(IAppDimensContext context, int value) => value.DGSdpa(context);
    public static float DGSdpi(IAppDimensContext context, int value) => value.DGSdpi(context);
    public static float DGSdpia(IAppDimensContext context, int value) => value.DGSdpia(context);
    public static float DGHdp(IAppDimensContext context, int value) => value.DGHdp(context);
    public static float DGHdpa(IAppDimensContext context, int value) => value.DGHdpa(context);
    public static float DGHdpi(IAppDimensContext context, int value) => value.DGHdpi(context);
    public static float DGHdpia(IAppDimensContext context, int value) => value.DGHdpia(context);
    public static float DGWdp(IAppDimensContext context, int value) => value.DGWdp(context);
    public static float DGWdpa(IAppDimensContext context, int value) => value.DGWdpa(context);
    public static float DGWdpi(IAppDimensContext context, int value) => value.DGWdpi(context);
    public static float DGWdpia(IAppDimensContext context, int value) => value.DGWdpia(context);

    public static float WarmupCache(IAppDimensContext context)
    {
        DimenCache.Init(context);
        return DimenCache.FastMetricsForCode(context).Density;
    }
}

public static class DimenDiagonalKernels
{
    private static float CalculateDiagonalDp(
        float baseValue, ScreenConfiguration configuration, DpQualifier qualifier,
        Inverter inverter, bool ignoreMultiWindows, bool applyAspectRatio,
        float? customSensitivityK, IAppDimensContext? context)
    {
        var m = MetricsScopeHolder.Current ?? DimenMetrics.From(configuration);
        if (DimenCalculationPlumbing.IsMultiWindowConstrained(configuration, ignoreMultiWindows, context))
            return baseValue;
        var result = baseValue * m.DiagonalScale;
        if (applyAspectRatio)
            result *= m.AspectRatioMultiplier(customSensitivityK);
        return result;
    }

    private static float CalculateDiagonalPx(
        float baseValue, ScreenConfiguration configuration, DpQualifier qualifier,
        Inverter inverter, bool ignoreMultiWindows, bool applyAspectRatio,
        float? customSensitivityK, IAppDimensContext? context)
    {
        var dp = CalculateDiagonalDp(baseValue, configuration, qualifier, inverter,
            ignoreMultiWindows, applyAspectRatio, customSensitivityK, context);
        return dp * (context?.Density ?? configuration.DensityDpi / 160f);
    }

    /// <summary>Cached full path (dp).</summary>
    public static float ToDiagonalDp(this float baseValue, IAppDimensContext context,
        DpQualifier qualifier = DpQualifier.SmallWidth, Inverter inverter = Inverter.Default,
        bool ignoreMultiWindows = false, bool applyAspectRatio = false, float? customSensitivityK = null)
    {
        var c = context.Configuration;
        var key = DimenCache.BuildKey(baseValue, c.IsLandscape, ignoreMultiWindows,
            DimenCache.CalcType.Diagonal, qualifier, inverter, applyAspectRatio, DimenCache.ValueType.Dp, customSensitivityK);
        var metrics = DimenCache.FastMetricsForCode(context);
        if (DimenCache.ShouldComputeDirectly(key))
            return CalculateDiagonalDp(baseValue, c, qualifier, inverter, ignoreMultiWindows, applyAspectRatio, customSensitivityK, context);
        if (DimenCache.TryPeek(metrics, key, out var cachedValue)) return cachedValue;
        return ResolveMissedInDp(key, metrics, context, qualifier, inverter,
            ignoreMultiWindows, applyAspectRatio, customSensitivityK, baseValue);
    }

    private static float ResolveMissedInDp(long key, DimenMetrics metrics, IAppDimensContext context,
        DpQualifier qualifier, Inverter inverter, bool ignoreMultiWindows,
        bool applyAspectRatio, float? customSensitivityK, float baseValue)
    {
        var cfg = context.Configuration;
        return DimenCache.Resolve(key, metrics,
            () => CalculateDiagonalDp(baseValue, cfg, qualifier, inverter, ignoreMultiWindows, applyAspectRatio, customSensitivityK, context));
    }

    /// <summary>Cached full path (px).</summary>
    public static float ToDiagonalPx(this float baseValue, IAppDimensContext context,
        DpQualifier qualifier = DpQualifier.SmallWidth, Inverter inverter = Inverter.Default,
        bool ignoreMultiWindows = false, bool applyAspectRatio = false, float? customSensitivityK = null)
    {
        var c = context.Configuration;
        var key = DimenCache.BuildKey(baseValue, c.IsLandscape, ignoreMultiWindows,
            DimenCache.CalcType.Diagonal, qualifier, inverter, applyAspectRatio, DimenCache.ValueType.Px, customSensitivityK);
        var metrics = DimenCache.FastMetricsForCode(context);
        if (DimenCache.ShouldComputeDirectly(key))
            return CalculateDiagonalPx(baseValue, c, qualifier, inverter, ignoreMultiWindows, applyAspectRatio, customSensitivityK, context);
        if (DimenCache.TryPeek(metrics, key, out var cachedValue)) return cachedValue;
        return ResolveMissedInPx(key, metrics, context, qualifier, inverter,
            ignoreMultiWindows, applyAspectRatio, customSensitivityK, baseValue);
    }

    private static float ResolveMissedInPx(long key, DimenMetrics metrics, IAppDimensContext context,
        DpQualifier qualifier, Inverter inverter, bool ignoreMultiWindows,
        bool applyAspectRatio, float? customSensitivityK, float baseValue)
    {
        var cfg = context.Configuration;
        return DimenCache.Resolve(key, metrics,
            () => CalculateDiagonalPx(baseValue, cfg, qualifier, inverter, ignoreMultiWindows, applyAspectRatio, customSensitivityK, context));
    }

    public static float ToDiagonalDp(this int v, IAppDimensContext context,
        DpQualifier qualifier = DpQualifier.SmallWidth, Inverter inverter = Inverter.Default,
        bool ignoreMultiWindows = false, bool applyAspectRatio = false, float? customSensitivityK = null)
        => ((float)v).ToDiagonalDp(context, qualifier, inverter, ignoreMultiWindows, applyAspectRatio, customSensitivityK);

    public static float ToDiagonalPx(this int v, IAppDimensContext context,
        DpQualifier qualifier = DpQualifier.SmallWidth, Inverter inverter = Inverter.Default,
        bool ignoreMultiWindows = false, bool applyAspectRatio = false, float? customSensitivityK = null)
        => ((float)v).ToDiagonalPx(context, qualifier, inverter, ignoreMultiWindows, applyAspectRatio, customSensitivityK);

    public static float ToDiagonalDp(this double v, IAppDimensContext context,
        DpQualifier qualifier = DpQualifier.SmallWidth, Inverter inverter = Inverter.Default,
        bool ignoreMultiWindows = false, bool applyAspectRatio = false, float? customSensitivityK = null)
        => ((float)v).ToDiagonalDp(context, qualifier, inverter, ignoreMultiWindows, applyAspectRatio, customSensitivityK);
}

public static class DiagonalExtensions
{
    public static float DGSdp(this int v, IAppDimensContext ctx) => v.ToDiagonalDp(ctx, DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: false);
    public static float DGSdp(this int v) => v.ToDiagonalDp(AppDimensAmbient.Require(), DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: false);
    public static float DGSdpPx(this int v, IAppDimensContext ctx) => v.ToDiagonalPx(ctx, DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: false);
    public static float DGSdpa(this int v, IAppDimensContext ctx) => v.ToDiagonalDp(ctx, DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: true);
    public static float DGSdpa(this int v) => v.ToDiagonalDp(AppDimensAmbient.Require(), DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: true);
    public static float DGSdpaPx(this int v, IAppDimensContext ctx) => v.ToDiagonalPx(ctx, DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: true);
    public static float DGSdpi(this int v, IAppDimensContext ctx) => v.ToDiagonalDp(ctx, DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: false);
    public static float DGSdpi(this int v) => v.ToDiagonalDp(AppDimensAmbient.Require(), DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: false);
    public static float DGSdpiPx(this int v, IAppDimensContext ctx) => v.ToDiagonalPx(ctx, DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: false);
    public static float DGSdpia(this int v, IAppDimensContext ctx) => v.ToDiagonalDp(ctx, DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: true);
    public static float DGSdpia(this int v) => v.ToDiagonalDp(AppDimensAmbient.Require(), DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: true);
    public static float DGSdpiaPx(this int v, IAppDimensContext ctx) => v.ToDiagonalPx(ctx, DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: true);
    public static float DGSdpCustom(this int v, DpQualifier qualifier, Inverter inverter, bool imw, bool ar, float? k = null) =>
        v.ToDiagonalDp(AppDimensAmbient.Require(), qualifier, inverter, imw, ar, k);
    public static float DGSdpRotateRaw(this int v, IAppDimensContext ctx, float rawAlternate, Orientation orientation = Orientation.Landscape)
        => ScaledExtensionsHelpers.IsTargetOrientation(ctx.Configuration, orientation) ? rawAlternate : v.DGSdp(ctx);

    public static float DGHdp(this int v, IAppDimensContext ctx) => v.ToDiagonalDp(ctx, DpQualifier.Height, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: false);
    public static float DGHdp(this int v) => v.ToDiagonalDp(AppDimensAmbient.Require(), DpQualifier.Height, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: false);
    public static float DGHdpPx(this int v, IAppDimensContext ctx) => v.ToDiagonalPx(ctx, DpQualifier.Height, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: false);
    public static float DGHdpa(this int v, IAppDimensContext ctx) => v.ToDiagonalDp(ctx, DpQualifier.Height, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: true);
    public static float DGHdpa(this int v) => v.ToDiagonalDp(AppDimensAmbient.Require(), DpQualifier.Height, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: true);
    public static float DGHdpaPx(this int v, IAppDimensContext ctx) => v.ToDiagonalPx(ctx, DpQualifier.Height, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: true);
    public static float DGHdpi(this int v, IAppDimensContext ctx) => v.ToDiagonalDp(ctx, DpQualifier.Height, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: false);
    public static float DGHdpi(this int v) => v.ToDiagonalDp(AppDimensAmbient.Require(), DpQualifier.Height, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: false);
    public static float DGHdpiPx(this int v, IAppDimensContext ctx) => v.ToDiagonalPx(ctx, DpQualifier.Height, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: false);
    public static float DGHdpia(this int v, IAppDimensContext ctx) => v.ToDiagonalDp(ctx, DpQualifier.Height, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: true);
    public static float DGHdpia(this int v) => v.ToDiagonalDp(AppDimensAmbient.Require(), DpQualifier.Height, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: true);
    public static float DGHdpiaPx(this int v, IAppDimensContext ctx) => v.ToDiagonalPx(ctx, DpQualifier.Height, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: true);
    public static float DGHdpCustom(this int v, DpQualifier qualifier, Inverter inverter, bool imw, bool ar, float? k = null) =>
        v.ToDiagonalDp(AppDimensAmbient.Require(), qualifier, inverter, imw, ar, k);
    public static float DGHdpRotateRaw(this int v, IAppDimensContext ctx, float rawAlternate, Orientation orientation = Orientation.Landscape)
        => ScaledExtensionsHelpers.IsTargetOrientation(ctx.Configuration, orientation) ? rawAlternate : v.DGHdp(ctx);

    public static float DGWdp(this int v, IAppDimensContext ctx) => v.ToDiagonalDp(ctx, DpQualifier.Width, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: false);
    public static float DGWdp(this int v) => v.ToDiagonalDp(AppDimensAmbient.Require(), DpQualifier.Width, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: false);
    public static float DGWdpPx(this int v, IAppDimensContext ctx) => v.ToDiagonalPx(ctx, DpQualifier.Width, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: false);
    public static float DGWdpa(this int v, IAppDimensContext ctx) => v.ToDiagonalDp(ctx, DpQualifier.Width, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: true);
    public static float DGWdpa(this int v) => v.ToDiagonalDp(AppDimensAmbient.Require(), DpQualifier.Width, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: true);
    public static float DGWdpaPx(this int v, IAppDimensContext ctx) => v.ToDiagonalPx(ctx, DpQualifier.Width, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: true);
    public static float DGWdpi(this int v, IAppDimensContext ctx) => v.ToDiagonalDp(ctx, DpQualifier.Width, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: false);
    public static float DGWdpi(this int v) => v.ToDiagonalDp(AppDimensAmbient.Require(), DpQualifier.Width, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: false);
    public static float DGWdpiPx(this int v, IAppDimensContext ctx) => v.ToDiagonalPx(ctx, DpQualifier.Width, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: false);
    public static float DGWdpia(this int v, IAppDimensContext ctx) => v.ToDiagonalDp(ctx, DpQualifier.Width, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: true);
    public static float DGWdpia(this int v) => v.ToDiagonalDp(AppDimensAmbient.Require(), DpQualifier.Width, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: true);
    public static float DGWdpiaPx(this int v, IAppDimensContext ctx) => v.ToDiagonalPx(ctx, DpQualifier.Width, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: true);
    public static float DGWdpCustom(this int v, DpQualifier qualifier, Inverter inverter, bool imw, bool ar, float? k = null) =>
        v.ToDiagonalDp(AppDimensAmbient.Require(), qualifier, inverter, imw, ar, k);
    public static float DGWdpRotateRaw(this int v, IAppDimensContext ctx, float rawAlternate, Orientation orientation = Orientation.Landscape)
        => ScaledExtensionsHelpers.IsTargetOrientation(ctx.Configuration, orientation) ? rawAlternate : v.DGWdp(ctx);

}

using AppDimens.Net.Common;
using AppDimens.Net.Core;
using AppDimens.Net.Code.Scaled;

namespace AppDimens.Net.Code.Density;

/// <summary>Density strategy kernels — cached full path (parity with the Kotlin modules).</summary>
public static class DimenDensity
{
    public static float DSdp(IAppDimensContext context, int value) => value.DSdp(context);
    public static float DSdpa(IAppDimensContext context, int value) => value.DSdpa(context);
    public static float DSdpi(IAppDimensContext context, int value) => value.DSdpi(context);
    public static float DSdpia(IAppDimensContext context, int value) => value.DSdpia(context);
    public static float DHdp(IAppDimensContext context, int value) => value.DHdp(context);
    public static float DHdpa(IAppDimensContext context, int value) => value.DHdpa(context);
    public static float DHdpi(IAppDimensContext context, int value) => value.DHdpi(context);
    public static float DHdpia(IAppDimensContext context, int value) => value.DHdpia(context);
    public static float DWdp(IAppDimensContext context, int value) => value.DWdp(context);
    public static float DWdpa(IAppDimensContext context, int value) => value.DWdpa(context);
    public static float DWdpi(IAppDimensContext context, int value) => value.DWdpi(context);
    public static float DWdpia(IAppDimensContext context, int value) => value.DWdpia(context);

    public static float WarmupCache(IAppDimensContext context)
    {
        DimenCache.Init(context);
        return DimenCache.FastMetricsForCode(context).Density;
    }
}

public static class DimenDensityKernels
{
    private static float CalculateDensityDp(
        float baseValue, ScreenConfiguration configuration, DpQualifier qualifier,
        Inverter inverter, bool ignoreMultiWindows, bool applyAspectRatio,
        float? customSensitivityK, IAppDimensContext? context)
    {
        var m = MetricsScopeHolder.Current ?? DimenMetrics.From(configuration);
        if (DimenCalculationPlumbing.IsMultiWindowConstrained(configuration, ignoreMultiWindows, context))
            return baseValue;
        var result = baseValue * m.Density;
        if (applyAspectRatio)
            result *= m.AspectRatioMultiplier(customSensitivityK);
        return result;
    }

    private static float CalculateDensityPx(
        float baseValue, ScreenConfiguration configuration, DpQualifier qualifier,
        Inverter inverter, bool ignoreMultiWindows, bool applyAspectRatio,
        float? customSensitivityK, IAppDimensContext? context)
    {
        var dp = CalculateDensityDp(baseValue, configuration, qualifier, inverter,
            ignoreMultiWindows, applyAspectRatio, customSensitivityK, context);
        return dp * (context?.Density ?? configuration.DensityDpi / 160f);
    }

    /// <summary>Cached full path (dp).</summary>
    public static float ToDensityDp(this float baseValue, IAppDimensContext context,
        DpQualifier qualifier = DpQualifier.SmallWidth, Inverter inverter = Inverter.Default,
        bool ignoreMultiWindows = false, bool applyAspectRatio = false, float? customSensitivityK = null)
    {
        var c = context.Configuration;
        var key = DimenCache.BuildKey(baseValue, c.IsLandscape, ignoreMultiWindows,
            DimenCache.CalcType.Density, qualifier, inverter, applyAspectRatio, DimenCache.ValueType.Dp, customSensitivityK);
        var metrics = DimenCache.FastMetricsForCode(context);
        if (DimenCache.ShouldComputeDirectly(key))
            return CalculateDensityDp(baseValue, c, qualifier, inverter, ignoreMultiWindows, applyAspectRatio, customSensitivityK, context);
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
            () => CalculateDensityDp(baseValue, cfg, qualifier, inverter, ignoreMultiWindows, applyAspectRatio, customSensitivityK, context));
    }

    /// <summary>Cached full path (px).</summary>
    public static float ToDensityPx(this float baseValue, IAppDimensContext context,
        DpQualifier qualifier = DpQualifier.SmallWidth, Inverter inverter = Inverter.Default,
        bool ignoreMultiWindows = false, bool applyAspectRatio = false, float? customSensitivityK = null)
    {
        var c = context.Configuration;
        var key = DimenCache.BuildKey(baseValue, c.IsLandscape, ignoreMultiWindows,
            DimenCache.CalcType.Density, qualifier, inverter, applyAspectRatio, DimenCache.ValueType.Px, customSensitivityK);
        var metrics = DimenCache.FastMetricsForCode(context);
        if (DimenCache.ShouldComputeDirectly(key))
            return CalculateDensityPx(baseValue, c, qualifier, inverter, ignoreMultiWindows, applyAspectRatio, customSensitivityK, context);
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
            () => CalculateDensityPx(baseValue, cfg, qualifier, inverter, ignoreMultiWindows, applyAspectRatio, customSensitivityK, context));
    }

    public static float ToDensityDp(this int v, IAppDimensContext context,
        DpQualifier qualifier = DpQualifier.SmallWidth, Inverter inverter = Inverter.Default,
        bool ignoreMultiWindows = false, bool applyAspectRatio = false, float? customSensitivityK = null)
        => ((float)v).ToDensityDp(context, qualifier, inverter, ignoreMultiWindows, applyAspectRatio, customSensitivityK);

    public static float ToDensityPx(this int v, IAppDimensContext context,
        DpQualifier qualifier = DpQualifier.SmallWidth, Inverter inverter = Inverter.Default,
        bool ignoreMultiWindows = false, bool applyAspectRatio = false, float? customSensitivityK = null)
        => ((float)v).ToDensityPx(context, qualifier, inverter, ignoreMultiWindows, applyAspectRatio, customSensitivityK);

    public static float ToDensityDp(this double v, IAppDimensContext context,
        DpQualifier qualifier = DpQualifier.SmallWidth, Inverter inverter = Inverter.Default,
        bool ignoreMultiWindows = false, bool applyAspectRatio = false, float? customSensitivityK = null)
        => ((float)v).ToDensityDp(context, qualifier, inverter, ignoreMultiWindows, applyAspectRatio, customSensitivityK);
}

public static class DensityExtensions
{
    public static float DSdp(this int v, IAppDimensContext ctx) => v.ToDensityDp(ctx, DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: false);
    public static float DSdp(this int v) => v.ToDensityDp(AppDimensAmbient.Require(), DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: false);
    public static float DSdpPx(this int v, IAppDimensContext ctx) => v.ToDensityPx(ctx, DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: false);
    public static float DSdpa(this int v, IAppDimensContext ctx) => v.ToDensityDp(ctx, DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: true);
    public static float DSdpa(this int v) => v.ToDensityDp(AppDimensAmbient.Require(), DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: true);
    public static float DSdpaPx(this int v, IAppDimensContext ctx) => v.ToDensityPx(ctx, DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: true);
    public static float DSdpi(this int v, IAppDimensContext ctx) => v.ToDensityDp(ctx, DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: false);
    public static float DSdpi(this int v) => v.ToDensityDp(AppDimensAmbient.Require(), DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: false);
    public static float DSdpiPx(this int v, IAppDimensContext ctx) => v.ToDensityPx(ctx, DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: false);
    public static float DSdpia(this int v, IAppDimensContext ctx) => v.ToDensityDp(ctx, DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: true);
    public static float DSdpia(this int v) => v.ToDensityDp(AppDimensAmbient.Require(), DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: true);
    public static float DSdpiaPx(this int v, IAppDimensContext ctx) => v.ToDensityPx(ctx, DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: true);
    public static float DSdpCustom(this int v, DpQualifier qualifier, Inverter inverter, bool imw, bool ar, float? k = null) =>
        v.ToDensityDp(AppDimensAmbient.Require(), qualifier, inverter, imw, ar, k);
    public static float DSdpRotateRaw(this int v, IAppDimensContext ctx, float rawAlternate, Orientation orientation = Orientation.Landscape)
        => ScaledExtensionsHelpers.IsTargetOrientation(ctx.Configuration, orientation) ? rawAlternate : v.DSdp(ctx);

    public static float DHdp(this int v, IAppDimensContext ctx) => v.ToDensityDp(ctx, DpQualifier.Height, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: false);
    public static float DHdp(this int v) => v.ToDensityDp(AppDimensAmbient.Require(), DpQualifier.Height, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: false);
    public static float DHdpPx(this int v, IAppDimensContext ctx) => v.ToDensityPx(ctx, DpQualifier.Height, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: false);
    public static float DHdpa(this int v, IAppDimensContext ctx) => v.ToDensityDp(ctx, DpQualifier.Height, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: true);
    public static float DHdpa(this int v) => v.ToDensityDp(AppDimensAmbient.Require(), DpQualifier.Height, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: true);
    public static float DHdpaPx(this int v, IAppDimensContext ctx) => v.ToDensityPx(ctx, DpQualifier.Height, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: true);
    public static float DHdpi(this int v, IAppDimensContext ctx) => v.ToDensityDp(ctx, DpQualifier.Height, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: false);
    public static float DHdpi(this int v) => v.ToDensityDp(AppDimensAmbient.Require(), DpQualifier.Height, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: false);
    public static float DHdpiPx(this int v, IAppDimensContext ctx) => v.ToDensityPx(ctx, DpQualifier.Height, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: false);
    public static float DHdpia(this int v, IAppDimensContext ctx) => v.ToDensityDp(ctx, DpQualifier.Height, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: true);
    public static float DHdpia(this int v) => v.ToDensityDp(AppDimensAmbient.Require(), DpQualifier.Height, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: true);
    public static float DHdpiaPx(this int v, IAppDimensContext ctx) => v.ToDensityPx(ctx, DpQualifier.Height, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: true);
    public static float DHdpCustom(this int v, DpQualifier qualifier, Inverter inverter, bool imw, bool ar, float? k = null) =>
        v.ToDensityDp(AppDimensAmbient.Require(), qualifier, inverter, imw, ar, k);
    public static float DHdpRotateRaw(this int v, IAppDimensContext ctx, float rawAlternate, Orientation orientation = Orientation.Landscape)
        => ScaledExtensionsHelpers.IsTargetOrientation(ctx.Configuration, orientation) ? rawAlternate : v.DHdp(ctx);

    public static float DWdp(this int v, IAppDimensContext ctx) => v.ToDensityDp(ctx, DpQualifier.Width, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: false);
    public static float DWdp(this int v) => v.ToDensityDp(AppDimensAmbient.Require(), DpQualifier.Width, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: false);
    public static float DWdpPx(this int v, IAppDimensContext ctx) => v.ToDensityPx(ctx, DpQualifier.Width, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: false);
    public static float DWdpa(this int v, IAppDimensContext ctx) => v.ToDensityDp(ctx, DpQualifier.Width, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: true);
    public static float DWdpa(this int v) => v.ToDensityDp(AppDimensAmbient.Require(), DpQualifier.Width, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: true);
    public static float DWdpaPx(this int v, IAppDimensContext ctx) => v.ToDensityPx(ctx, DpQualifier.Width, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: true);
    public static float DWdpi(this int v, IAppDimensContext ctx) => v.ToDensityDp(ctx, DpQualifier.Width, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: false);
    public static float DWdpi(this int v) => v.ToDensityDp(AppDimensAmbient.Require(), DpQualifier.Width, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: false);
    public static float DWdpiPx(this int v, IAppDimensContext ctx) => v.ToDensityPx(ctx, DpQualifier.Width, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: false);
    public static float DWdpia(this int v, IAppDimensContext ctx) => v.ToDensityDp(ctx, DpQualifier.Width, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: true);
    public static float DWdpia(this int v) => v.ToDensityDp(AppDimensAmbient.Require(), DpQualifier.Width, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: true);
    public static float DWdpiaPx(this int v, IAppDimensContext ctx) => v.ToDensityPx(ctx, DpQualifier.Width, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: true);
    public static float DWdpCustom(this int v, DpQualifier qualifier, Inverter inverter, bool imw, bool ar, float? k = null) =>
        v.ToDensityDp(AppDimensAmbient.Require(), qualifier, inverter, imw, ar, k);
    public static float DWdpRotateRaw(this int v, IAppDimensContext ctx, float rawAlternate, Orientation orientation = Orientation.Landscape)
        => ScaledExtensionsHelpers.IsTargetOrientation(ctx.Configuration, orientation) ? rawAlternate : v.DWdp(ctx);

}

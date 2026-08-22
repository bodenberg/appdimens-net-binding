using AppDimens.Net.Common;
using AppDimens.Net.Core;
using AppDimens.Net.Code.Scaled;

namespace AppDimens.Net.Code.Logarithmic;

/// <summary>Logarithmic strategy kernels — cached full path (parity with the Kotlin modules).</summary>
public static class DimenLogarithmic
{
    public static float LOGSdp(IAppDimensContext context, int value) => value.LOGSdp(context);
    public static float LOGSdpa(IAppDimensContext context, int value) => value.LOGSdpa(context);
    public static float LOGSdpi(IAppDimensContext context, int value) => value.LOGSdpi(context);
    public static float LOGSdpia(IAppDimensContext context, int value) => value.LOGSdpia(context);
    public static float LOGHdp(IAppDimensContext context, int value) => value.LOGHdp(context);
    public static float LOGHdpa(IAppDimensContext context, int value) => value.LOGHdpa(context);
    public static float LOGHdpi(IAppDimensContext context, int value) => value.LOGHdpi(context);
    public static float LOGHdpia(IAppDimensContext context, int value) => value.LOGHdpia(context);
    public static float LOGWdp(IAppDimensContext context, int value) => value.LOGWdp(context);
    public static float LOGWdpa(IAppDimensContext context, int value) => value.LOGWdpa(context);
    public static float LOGWdpi(IAppDimensContext context, int value) => value.LOGWdpi(context);
    public static float LOGWdpia(IAppDimensContext context, int value) => value.LOGWdpia(context);

    public static float WarmupCache(IAppDimensContext context)
    {
        DimenCache.Init(context);
        return DimenCache.FastMetricsForCode(context).Density;
    }
}

public static class DimenLogarithmicKernels
{
    private static float CalculateLogarithmicDp(
        float baseValue, ScreenConfiguration configuration, DpQualifier qualifier,
        Inverter inverter, bool ignoreMultiWindows, bool applyAspectRatio,
        float? customSensitivityK, IAppDimensContext? context)
    {
        var m = MetricsScopeHolder.Current ?? DimenMetrics.From(configuration);
        if (DimenCalculationPlumbing.IsMultiWindowConstrained(configuration, ignoreMultiWindows, context))
            return baseValue;
        var result = baseValue * m.LogarithmicScale;
        if (applyAspectRatio)
            result *= m.AspectRatioMultiplier(customSensitivityK);
        return result;
    }

    private static float CalculateLogarithmicPx(
        float baseValue, ScreenConfiguration configuration, DpQualifier qualifier,
        Inverter inverter, bool ignoreMultiWindows, bool applyAspectRatio,
        float? customSensitivityK, IAppDimensContext? context)
    {
        var dp = CalculateLogarithmicDp(baseValue, configuration, qualifier, inverter,
            ignoreMultiWindows, applyAspectRatio, customSensitivityK, context);
        return dp * (context?.Density ?? configuration.DensityDpi / 160f);
    }

    /// <summary>Cached full path (dp).</summary>
    public static float ToLogarithmicDp(this float baseValue, IAppDimensContext context,
        DpQualifier qualifier = DpQualifier.SmallWidth, Inverter inverter = Inverter.Default,
        bool ignoreMultiWindows = false, bool applyAspectRatio = false, float? customSensitivityK = null)
    {
        var c = context.Configuration;
        var key = DimenCache.BuildKey(baseValue, c.IsLandscape, ignoreMultiWindows,
            DimenCache.CalcType.Logarithmic, qualifier, inverter, applyAspectRatio, DimenCache.ValueType.Dp, customSensitivityK);
        var metrics = DimenCache.FastMetricsForCode(context);
        if (DimenCache.ShouldComputeDirectly(key))
            return CalculateLogarithmicDp(baseValue, c, qualifier, inverter, ignoreMultiWindows, applyAspectRatio, customSensitivityK, context);
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
            () => CalculateLogarithmicDp(baseValue, cfg, qualifier, inverter, ignoreMultiWindows, applyAspectRatio, customSensitivityK, context));
    }

    /// <summary>Cached full path (px).</summary>
    public static float ToLogarithmicPx(this float baseValue, IAppDimensContext context,
        DpQualifier qualifier = DpQualifier.SmallWidth, Inverter inverter = Inverter.Default,
        bool ignoreMultiWindows = false, bool applyAspectRatio = false, float? customSensitivityK = null)
    {
        var c = context.Configuration;
        var key = DimenCache.BuildKey(baseValue, c.IsLandscape, ignoreMultiWindows,
            DimenCache.CalcType.Logarithmic, qualifier, inverter, applyAspectRatio, DimenCache.ValueType.Px, customSensitivityK);
        var metrics = DimenCache.FastMetricsForCode(context);
        if (DimenCache.ShouldComputeDirectly(key))
            return CalculateLogarithmicPx(baseValue, c, qualifier, inverter, ignoreMultiWindows, applyAspectRatio, customSensitivityK, context);
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
            () => CalculateLogarithmicPx(baseValue, cfg, qualifier, inverter, ignoreMultiWindows, applyAspectRatio, customSensitivityK, context));
    }

    public static float ToLogarithmicDp(this int v, IAppDimensContext context,
        DpQualifier qualifier = DpQualifier.SmallWidth, Inverter inverter = Inverter.Default,
        bool ignoreMultiWindows = false, bool applyAspectRatio = false, float? customSensitivityK = null)
        => ((float)v).ToLogarithmicDp(context, qualifier, inverter, ignoreMultiWindows, applyAspectRatio, customSensitivityK);

    public static float ToLogarithmicPx(this int v, IAppDimensContext context,
        DpQualifier qualifier = DpQualifier.SmallWidth, Inverter inverter = Inverter.Default,
        bool ignoreMultiWindows = false, bool applyAspectRatio = false, float? customSensitivityK = null)
        => ((float)v).ToLogarithmicPx(context, qualifier, inverter, ignoreMultiWindows, applyAspectRatio, customSensitivityK);

    public static float ToLogarithmicDp(this double v, IAppDimensContext context,
        DpQualifier qualifier = DpQualifier.SmallWidth, Inverter inverter = Inverter.Default,
        bool ignoreMultiWindows = false, bool applyAspectRatio = false, float? customSensitivityK = null)
        => ((float)v).ToLogarithmicDp(context, qualifier, inverter, ignoreMultiWindows, applyAspectRatio, customSensitivityK);
}

public static class LogarithmicExtensions
{
    public static float LOGSdp(this int v, IAppDimensContext ctx) => v.ToLogarithmicDp(ctx, DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: false);
    public static float LOGSdp(this int v) => v.ToLogarithmicDp(AppDimensAmbient.Require(), DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: false);
    public static float LOGSdpPx(this int v, IAppDimensContext ctx) => v.ToLogarithmicPx(ctx, DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: false);
    public static float LOGSdpa(this int v, IAppDimensContext ctx) => v.ToLogarithmicDp(ctx, DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: true);
    public static float LOGSdpa(this int v) => v.ToLogarithmicDp(AppDimensAmbient.Require(), DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: true);
    public static float LOGSdpaPx(this int v, IAppDimensContext ctx) => v.ToLogarithmicPx(ctx, DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: true);
    public static float LOGSdpi(this int v, IAppDimensContext ctx) => v.ToLogarithmicDp(ctx, DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: false);
    public static float LOGSdpi(this int v) => v.ToLogarithmicDp(AppDimensAmbient.Require(), DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: false);
    public static float LOGSdpiPx(this int v, IAppDimensContext ctx) => v.ToLogarithmicPx(ctx, DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: false);
    public static float LOGSdpia(this int v, IAppDimensContext ctx) => v.ToLogarithmicDp(ctx, DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: true);
    public static float LOGSdpia(this int v) => v.ToLogarithmicDp(AppDimensAmbient.Require(), DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: true);
    public static float LOGSdpiaPx(this int v, IAppDimensContext ctx) => v.ToLogarithmicPx(ctx, DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: true);
    public static float LOGSdpCustom(this int v, DpQualifier qualifier, Inverter inverter, bool imw, bool ar, float? k = null) =>
        v.ToLogarithmicDp(AppDimensAmbient.Require(), qualifier, inverter, imw, ar, k);
    public static float LOGSdpRotateRaw(this int v, IAppDimensContext ctx, float rawAlternate, Orientation orientation = Orientation.Landscape)
        => ScaledExtensionsHelpers.IsTargetOrientation(ctx.Configuration, orientation) ? rawAlternate : v.LOGSdp(ctx);

    public static float LOGHdp(this int v, IAppDimensContext ctx) => v.ToLogarithmicDp(ctx, DpQualifier.Height, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: false);
    public static float LOGHdp(this int v) => v.ToLogarithmicDp(AppDimensAmbient.Require(), DpQualifier.Height, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: false);
    public static float LOGHdpPx(this int v, IAppDimensContext ctx) => v.ToLogarithmicPx(ctx, DpQualifier.Height, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: false);
    public static float LOGHdpa(this int v, IAppDimensContext ctx) => v.ToLogarithmicDp(ctx, DpQualifier.Height, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: true);
    public static float LOGHdpa(this int v) => v.ToLogarithmicDp(AppDimensAmbient.Require(), DpQualifier.Height, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: true);
    public static float LOGHdpaPx(this int v, IAppDimensContext ctx) => v.ToLogarithmicPx(ctx, DpQualifier.Height, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: true);
    public static float LOGHdpi(this int v, IAppDimensContext ctx) => v.ToLogarithmicDp(ctx, DpQualifier.Height, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: false);
    public static float LOGHdpi(this int v) => v.ToLogarithmicDp(AppDimensAmbient.Require(), DpQualifier.Height, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: false);
    public static float LOGHdpiPx(this int v, IAppDimensContext ctx) => v.ToLogarithmicPx(ctx, DpQualifier.Height, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: false);
    public static float LOGHdpia(this int v, IAppDimensContext ctx) => v.ToLogarithmicDp(ctx, DpQualifier.Height, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: true);
    public static float LOGHdpia(this int v) => v.ToLogarithmicDp(AppDimensAmbient.Require(), DpQualifier.Height, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: true);
    public static float LOGHdpiaPx(this int v, IAppDimensContext ctx) => v.ToLogarithmicPx(ctx, DpQualifier.Height, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: true);
    public static float LOGHdpCustom(this int v, DpQualifier qualifier, Inverter inverter, bool imw, bool ar, float? k = null) =>
        v.ToLogarithmicDp(AppDimensAmbient.Require(), qualifier, inverter, imw, ar, k);
    public static float LOGHdpRotateRaw(this int v, IAppDimensContext ctx, float rawAlternate, Orientation orientation = Orientation.Landscape)
        => ScaledExtensionsHelpers.IsTargetOrientation(ctx.Configuration, orientation) ? rawAlternate : v.LOGHdp(ctx);

    public static float LOGWdp(this int v, IAppDimensContext ctx) => v.ToLogarithmicDp(ctx, DpQualifier.Width, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: false);
    public static float LOGWdp(this int v) => v.ToLogarithmicDp(AppDimensAmbient.Require(), DpQualifier.Width, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: false);
    public static float LOGWdpPx(this int v, IAppDimensContext ctx) => v.ToLogarithmicPx(ctx, DpQualifier.Width, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: false);
    public static float LOGWdpa(this int v, IAppDimensContext ctx) => v.ToLogarithmicDp(ctx, DpQualifier.Width, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: true);
    public static float LOGWdpa(this int v) => v.ToLogarithmicDp(AppDimensAmbient.Require(), DpQualifier.Width, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: true);
    public static float LOGWdpaPx(this int v, IAppDimensContext ctx) => v.ToLogarithmicPx(ctx, DpQualifier.Width, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: true);
    public static float LOGWdpi(this int v, IAppDimensContext ctx) => v.ToLogarithmicDp(ctx, DpQualifier.Width, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: false);
    public static float LOGWdpi(this int v) => v.ToLogarithmicDp(AppDimensAmbient.Require(), DpQualifier.Width, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: false);
    public static float LOGWdpiPx(this int v, IAppDimensContext ctx) => v.ToLogarithmicPx(ctx, DpQualifier.Width, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: false);
    public static float LOGWdpia(this int v, IAppDimensContext ctx) => v.ToLogarithmicDp(ctx, DpQualifier.Width, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: true);
    public static float LOGWdpia(this int v) => v.ToLogarithmicDp(AppDimensAmbient.Require(), DpQualifier.Width, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: true);
    public static float LOGWdpiaPx(this int v, IAppDimensContext ctx) => v.ToLogarithmicPx(ctx, DpQualifier.Width, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: true);
    public static float LOGWdpCustom(this int v, DpQualifier qualifier, Inverter inverter, bool imw, bool ar, float? k = null) =>
        v.ToLogarithmicDp(AppDimensAmbient.Require(), qualifier, inverter, imw, ar, k);
    public static float LOGWdpRotateRaw(this int v, IAppDimensContext ctx, float rawAlternate, Orientation orientation = Orientation.Landscape)
        => ScaledExtensionsHelpers.IsTargetOrientation(ctx.Configuration, orientation) ? rawAlternate : v.LOGWdp(ctx);

}

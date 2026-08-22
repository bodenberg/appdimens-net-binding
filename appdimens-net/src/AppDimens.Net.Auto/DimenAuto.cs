using AppDimens.Net.Common;
using AppDimens.Net.Core;
using AppDimens.Net.Code.Scaled;

namespace AppDimens.Net.Code.Auto;

/// <summary>Auto strategy kernels — cached full path (parity with the Kotlin modules).</summary>
public static class DimenAuto
{
    public static float ASdp(IAppDimensContext context, int value) => value.ASdp(context);
    public static float ASdpa(IAppDimensContext context, int value) => value.ASdpa(context);
    public static float ASdpi(IAppDimensContext context, int value) => value.ASdpi(context);
    public static float ASdpia(IAppDimensContext context, int value) => value.ASdpia(context);
    public static float AHdp(IAppDimensContext context, int value) => value.AHdp(context);
    public static float AHdpa(IAppDimensContext context, int value) => value.AHdpa(context);
    public static float AHdpi(IAppDimensContext context, int value) => value.AHdpi(context);
    public static float AHdpia(IAppDimensContext context, int value) => value.AHdpia(context);
    public static float AWdp(IAppDimensContext context, int value) => value.AWdp(context);
    public static float AWdpa(IAppDimensContext context, int value) => value.AWdpa(context);
    public static float AWdpi(IAppDimensContext context, int value) => value.AWdpi(context);
    public static float AWdpia(IAppDimensContext context, int value) => value.AWdpia(context);

    public static float WarmupCache(IAppDimensContext context)
    {
        DimenCache.Init(context);
        return DimenCache.FastMetricsForCode(context).Density;
    }
}

public static class DimenAutoKernels
{
    private static float CalculateAutoDp(
        float baseValue, ScreenConfiguration configuration, DpQualifier qualifier,
        Inverter inverter, bool ignoreMultiWindows, bool applyAspectRatio,
        float? customSensitivityK, IAppDimensContext? context)
    {
        var m = MetricsScopeHolder.Current ?? DimenMetrics.From(configuration);
        var isLandscape = configuration.IsLandscape;
        var isPortrait = configuration.IsPortrait;
        var q = DimenCalculationPlumbing.EffectiveQualifier(qualifier, inverter, isLandscape, isPortrait);
        if (DimenCalculationPlumbing.IsMultiWindowConstrained(configuration, ignoreMultiWindows, context))
            return baseValue;
        var dim = DimenCalculationPlumbing.ReadScreenDp(configuration, q);
        const float Transition = 480f, Sensitivity = 0.4f;
        var scale = dim <= Transition
            ? dim * DimenCache.InvBaseRatio
            : Transition * DimenCache.InvBaseRatio +
              Sensitivity * MathF.Log(1f + (dim - Transition) * DimenCache.InvBaseRatio);
        var result = baseValue * scale;
        if (applyAspectRatio)
            result *= m.AspectRatioMultiplier(customSensitivityK);
        return result;
    }

    private static float CalculateAutoPx(
        float baseValue, ScreenConfiguration configuration, DpQualifier qualifier,
        Inverter inverter, bool ignoreMultiWindows, bool applyAspectRatio,
        float? customSensitivityK, IAppDimensContext? context)
    {
        var dp = CalculateAutoDp(baseValue, configuration, qualifier, inverter,
            ignoreMultiWindows, applyAspectRatio, customSensitivityK, context);
        return dp * (context?.Density ?? configuration.DensityDpi / 160f);
    }

    /// <summary>Cached full path (dp).</summary>
    public static float ToAutoDp(this float baseValue, IAppDimensContext context,
        DpQualifier qualifier = DpQualifier.SmallWidth, Inverter inverter = Inverter.Default,
        bool ignoreMultiWindows = false, bool applyAspectRatio = false, float? customSensitivityK = null)
    {
        var c = context.Configuration;
        var key = DimenCache.BuildKey(baseValue, c.IsLandscape, ignoreMultiWindows,
            DimenCache.CalcType.Auto, qualifier, inverter, applyAspectRatio, DimenCache.ValueType.Dp, customSensitivityK);
        var metrics = DimenCache.FastMetricsForCode(context);
        if (DimenCache.ShouldComputeDirectly(key))
            return CalculateAutoDp(baseValue, c, qualifier, inverter, ignoreMultiWindows, applyAspectRatio, customSensitivityK, context);
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
            () => CalculateAutoDp(baseValue, cfg, qualifier, inverter, ignoreMultiWindows, applyAspectRatio, customSensitivityK, context));
    }

    /// <summary>Cached full path (px).</summary>
    public static float ToAutoPx(this float baseValue, IAppDimensContext context,
        DpQualifier qualifier = DpQualifier.SmallWidth, Inverter inverter = Inverter.Default,
        bool ignoreMultiWindows = false, bool applyAspectRatio = false, float? customSensitivityK = null)
    {
        var c = context.Configuration;
        var key = DimenCache.BuildKey(baseValue, c.IsLandscape, ignoreMultiWindows,
            DimenCache.CalcType.Auto, qualifier, inverter, applyAspectRatio, DimenCache.ValueType.Px, customSensitivityK);
        var metrics = DimenCache.FastMetricsForCode(context);
        if (DimenCache.ShouldComputeDirectly(key))
            return CalculateAutoPx(baseValue, c, qualifier, inverter, ignoreMultiWindows, applyAspectRatio, customSensitivityK, context);
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
            () => CalculateAutoPx(baseValue, cfg, qualifier, inverter, ignoreMultiWindows, applyAspectRatio, customSensitivityK, context));
    }

    public static float ToAutoDp(this int v, IAppDimensContext context,
        DpQualifier qualifier = DpQualifier.SmallWidth, Inverter inverter = Inverter.Default,
        bool ignoreMultiWindows = false, bool applyAspectRatio = false, float? customSensitivityK = null)
        => ((float)v).ToAutoDp(context, qualifier, inverter, ignoreMultiWindows, applyAspectRatio, customSensitivityK);

    public static float ToAutoPx(this int v, IAppDimensContext context,
        DpQualifier qualifier = DpQualifier.SmallWidth, Inverter inverter = Inverter.Default,
        bool ignoreMultiWindows = false, bool applyAspectRatio = false, float? customSensitivityK = null)
        => ((float)v).ToAutoPx(context, qualifier, inverter, ignoreMultiWindows, applyAspectRatio, customSensitivityK);

    public static float ToAutoDp(this double v, IAppDimensContext context,
        DpQualifier qualifier = DpQualifier.SmallWidth, Inverter inverter = Inverter.Default,
        bool ignoreMultiWindows = false, bool applyAspectRatio = false, float? customSensitivityK = null)
        => ((float)v).ToAutoDp(context, qualifier, inverter, ignoreMultiWindows, applyAspectRatio, customSensitivityK);
}

public static class AutoExtensions
{
    public static float ASdp(this int v, IAppDimensContext ctx) => v.ToAutoDp(ctx, DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: false);
    public static float ASdp(this int v) => v.ToAutoDp(AppDimensAmbient.Require(), DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: false);
    public static float ASdpPx(this int v, IAppDimensContext ctx) => v.ToAutoPx(ctx, DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: false);
    public static float ASdpa(this int v, IAppDimensContext ctx) => v.ToAutoDp(ctx, DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: true);
    public static float ASdpa(this int v) => v.ToAutoDp(AppDimensAmbient.Require(), DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: true);
    public static float ASdpaPx(this int v, IAppDimensContext ctx) => v.ToAutoPx(ctx, DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: true);
    public static float ASdpi(this int v, IAppDimensContext ctx) => v.ToAutoDp(ctx, DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: false);
    public static float ASdpi(this int v) => v.ToAutoDp(AppDimensAmbient.Require(), DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: false);
    public static float ASdpiPx(this int v, IAppDimensContext ctx) => v.ToAutoPx(ctx, DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: false);
    public static float ASdpia(this int v, IAppDimensContext ctx) => v.ToAutoDp(ctx, DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: true);
    public static float ASdpia(this int v) => v.ToAutoDp(AppDimensAmbient.Require(), DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: true);
    public static float ASdpiaPx(this int v, IAppDimensContext ctx) => v.ToAutoPx(ctx, DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: true);
    public static float ASdpCustom(this int v, DpQualifier qualifier, Inverter inverter, bool imw, bool ar, float? k = null) =>
        v.ToAutoDp(AppDimensAmbient.Require(), qualifier, inverter, imw, ar, k);
    public static float ASdpRotateRaw(this int v, IAppDimensContext ctx, float rawAlternate, Orientation orientation = Orientation.Landscape)
        => ScaledExtensionsHelpers.IsTargetOrientation(ctx.Configuration, orientation) ? rawAlternate : v.ASdp(ctx);

    public static float AHdp(this int v, IAppDimensContext ctx) => v.ToAutoDp(ctx, DpQualifier.Height, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: false);
    public static float AHdp(this int v) => v.ToAutoDp(AppDimensAmbient.Require(), DpQualifier.Height, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: false);
    public static float AHdpPx(this int v, IAppDimensContext ctx) => v.ToAutoPx(ctx, DpQualifier.Height, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: false);
    public static float AHdpa(this int v, IAppDimensContext ctx) => v.ToAutoDp(ctx, DpQualifier.Height, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: true);
    public static float AHdpa(this int v) => v.ToAutoDp(AppDimensAmbient.Require(), DpQualifier.Height, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: true);
    public static float AHdpaPx(this int v, IAppDimensContext ctx) => v.ToAutoPx(ctx, DpQualifier.Height, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: true);
    public static float AHdpi(this int v, IAppDimensContext ctx) => v.ToAutoDp(ctx, DpQualifier.Height, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: false);
    public static float AHdpi(this int v) => v.ToAutoDp(AppDimensAmbient.Require(), DpQualifier.Height, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: false);
    public static float AHdpiPx(this int v, IAppDimensContext ctx) => v.ToAutoPx(ctx, DpQualifier.Height, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: false);
    public static float AHdpia(this int v, IAppDimensContext ctx) => v.ToAutoDp(ctx, DpQualifier.Height, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: true);
    public static float AHdpia(this int v) => v.ToAutoDp(AppDimensAmbient.Require(), DpQualifier.Height, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: true);
    public static float AHdpiaPx(this int v, IAppDimensContext ctx) => v.ToAutoPx(ctx, DpQualifier.Height, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: true);
    public static float AHdpCustom(this int v, DpQualifier qualifier, Inverter inverter, bool imw, bool ar, float? k = null) =>
        v.ToAutoDp(AppDimensAmbient.Require(), qualifier, inverter, imw, ar, k);
    public static float AHdpRotateRaw(this int v, IAppDimensContext ctx, float rawAlternate, Orientation orientation = Orientation.Landscape)
        => ScaledExtensionsHelpers.IsTargetOrientation(ctx.Configuration, orientation) ? rawAlternate : v.AHdp(ctx);

    public static float AWdp(this int v, IAppDimensContext ctx) => v.ToAutoDp(ctx, DpQualifier.Width, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: false);
    public static float AWdp(this int v) => v.ToAutoDp(AppDimensAmbient.Require(), DpQualifier.Width, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: false);
    public static float AWdpPx(this int v, IAppDimensContext ctx) => v.ToAutoPx(ctx, DpQualifier.Width, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: false);
    public static float AWdpa(this int v, IAppDimensContext ctx) => v.ToAutoDp(ctx, DpQualifier.Width, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: true);
    public static float AWdpa(this int v) => v.ToAutoDp(AppDimensAmbient.Require(), DpQualifier.Width, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: true);
    public static float AWdpaPx(this int v, IAppDimensContext ctx) => v.ToAutoPx(ctx, DpQualifier.Width, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: true);
    public static float AWdpi(this int v, IAppDimensContext ctx) => v.ToAutoDp(ctx, DpQualifier.Width, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: false);
    public static float AWdpi(this int v) => v.ToAutoDp(AppDimensAmbient.Require(), DpQualifier.Width, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: false);
    public static float AWdpiPx(this int v, IAppDimensContext ctx) => v.ToAutoPx(ctx, DpQualifier.Width, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: false);
    public static float AWdpia(this int v, IAppDimensContext ctx) => v.ToAutoDp(ctx, DpQualifier.Width, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: true);
    public static float AWdpia(this int v) => v.ToAutoDp(AppDimensAmbient.Require(), DpQualifier.Width, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: true);
    public static float AWdpiaPx(this int v, IAppDimensContext ctx) => v.ToAutoPx(ctx, DpQualifier.Width, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: true);
    public static float AWdpCustom(this int v, DpQualifier qualifier, Inverter inverter, bool imw, bool ar, float? k = null) =>
        v.ToAutoDp(AppDimensAmbient.Require(), qualifier, inverter, imw, ar, k);
    public static float AWdpRotateRaw(this int v, IAppDimensContext ctx, float rawAlternate, Orientation orientation = Orientation.Landscape)
        => ScaledExtensionsHelpers.IsTargetOrientation(ctx.Configuration, orientation) ? rawAlternate : v.AWdp(ctx);

}

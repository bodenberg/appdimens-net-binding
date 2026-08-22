using AppDimens.Net.Common;
using AppDimens.Net.Core;

namespace AppDimens.Net.Code.Scaled;

/// <summary>Full-path Scaled kernels with cache-key resolution (parity with KMP <c>toDynamicScaled*</c>).</summary>
public static class DimenScaledKernels
{
    public static float CalculateScaledDp(
        float baseValue, ScreenConfiguration configuration, DpQualifier qualifier,
        Inverter inverter, bool ignoreMultiWindows, bool applyAspectRatio,
        float? customSensitivityK, IAppDimensContext? context = null)
    {
        var isLandscape = configuration.Orientation == ScreenConfiguration.OrientationLandscape;
        var isPortrait = configuration.Orientation == ScreenConfiguration.OrientationPortrait;
        var q = DimenCalculationPlumbing.EffectiveQualifier(qualifier, inverter, isLandscape, isPortrait);
        if (DimenCalculationPlumbing.IsMultiWindowConstrained(configuration, ignoreMultiWindows, context))
            return baseValue;
        var dim = DimenCalculationPlumbing.ReadScreenDp(configuration, q);
        if (!applyAspectRatio) return baseValue * dim * DimenCache.InvBaseRatio;
        var diff = dim - DesignScaleConstants.BaseWidthDp;
        var adj = (customSensitivityK ?? DimenCache.SensitivityDefault) *
                  FastMetrics(configuration).LogNormalizedAspectRatio;
        return baseValue * (1f + diff * (DimenCache.AdjustmentScale + adj));
    }

    public static float CalculateScaledPx(
        float baseValue, ScreenConfiguration configuration, DpQualifier qualifier,
        Inverter inverter, bool ignoreMultiWindows, bool applyAspectRatio,
        float? customSensitivityK, IAppDimensContext? context = null)
    {
        var dp = CalculateScaledDp(baseValue, configuration, qualifier, inverter,
            ignoreMultiWindows, applyAspectRatio, customSensitivityK, context);
        return dp * DensityOf(context, configuration);
    }

    internal static float DensityOf(IAppDimensContext? context, ScreenConfiguration configuration) =>
        context?.Density ?? configuration.DensityDpi / 160f;

    private static DimenMetrics FastMetrics(ScreenConfiguration configuration) =>
        MetricsScopeHolder.Current ?? DimenMetrics.From(configuration);

    /// <summary>Cached full path: builds the 64-bit key and resolves through the snapshot partition.</summary>
    public static float ToDynamicScaledDp(this float baseValue, IAppDimensContext context,
        DpQualifier qualifier = DpQualifier.SmallWidth, Inverter inverter = Inverter.Default,
        bool ignoreMultiWindows = false, bool applyAspectRatio = false, float? customSensitivityK = null)
    {
        var cfg = context.Configuration;
        var key = DimenCache.BuildKey(baseValue, cfg.IsLandscape, ignoreMultiWindows,
            DimenCache.CalcType.Scaled, qualifier, inverter, applyAspectRatio, DimenCache.ValueType.Dp, customSensitivityK);
        var metrics = DimenCache.FastMetricsForCode(context);
        if (DimenCache.ShouldComputeDirectly(key))
            return CalculateScaledDp(baseValue, cfg, qualifier, inverter, ignoreMultiWindows, applyAspectRatio, customSensitivityK, context);
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
            () => CalculateScaledDp(baseValue, cfg, qualifier, inverter, ignoreMultiWindows, applyAspectRatio, customSensitivityK, context));
    }

    public static float ToDynamicScaledPx(this float baseValue, IAppDimensContext context,
        DpQualifier qualifier = DpQualifier.SmallWidth, Inverter inverter = Inverter.Default,
        bool ignoreMultiWindows = false, bool applyAspectRatio = false, float? customSensitivityK = null)
    {
        var cfg = context.Configuration;
        var key = DimenCache.BuildKey(baseValue, cfg.IsLandscape, ignoreMultiWindows,
            DimenCache.CalcType.Scaled, qualifier, inverter, applyAspectRatio, DimenCache.ValueType.Px, customSensitivityK);
        var metrics = DimenCache.FastMetricsForCode(context);
        if (DimenCache.ShouldComputeDirectly(key))
            return CalculateScaledPx(baseValue, cfg, qualifier, inverter, ignoreMultiWindows, applyAspectRatio, customSensitivityK, context);
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
            () => CalculateScaledPx(baseValue, cfg, qualifier, inverter, ignoreMultiWindows, applyAspectRatio, customSensitivityK, context));
    }

    public static float ToDynamicScaledDp(this int baseValue, IAppDimensContext context,
        DpQualifier qualifier = DpQualifier.SmallWidth, Inverter inverter = Inverter.Default,
        bool ignoreMultiWindows = false, bool applyAspectRatio = false, float? customSensitivityK = null) =>
        ((float)baseValue).ToDynamicScaledDp(context, qualifier, inverter, ignoreMultiWindows, applyAspectRatio, customSensitivityK);

    public static float ToDynamicScaledPx(this int baseValue, IAppDimensContext context,
        DpQualifier qualifier = DpQualifier.SmallWidth, Inverter inverter = Inverter.Default,
        bool ignoreMultiWindows = false, bool applyAspectRatio = false, float? customSensitivityK = null) =>
        ((float)baseValue).ToDynamicScaledPx(context, qualifier, inverter, ignoreMultiWindows, applyAspectRatio, customSensitivityK);

    public static float ToDynamicScaledDp(this double baseValue, IAppDimensContext context,
        DpQualifier qualifier = DpQualifier.SmallWidth, Inverter inverter = Inverter.Default,
        bool ignoreMultiWindows = false, bool applyAspectRatio = false, float? customSensitivityK = null) =>
        ((float)baseValue).ToDynamicScaledDp(context, qualifier, inverter, ignoreMultiWindows, applyAspectRatio, customSensitivityK);

    public static float ToDynamicScaledPx(this double baseValue, IAppDimensContext context,
        DpQualifier qualifier = DpQualifier.SmallWidth, Inverter inverter = Inverter.Default,
        bool ignoreMultiWindows = false, bool applyAspectRatio = false, float? customSensitivityK = null) =>
        ((float)baseValue).ToDynamicScaledPx(context, qualifier, inverter, ignoreMultiWindows, applyAspectRatio, customSensitivityK);
}

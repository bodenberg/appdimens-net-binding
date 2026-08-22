using AppDimens.Net.Common;
using AppDimens.Net.Core;
using AppDimens.Net.Code.Scaled;

namespace AppDimens.Net.Code.Percent;

/// <summary>Percent strategy kernels — cached full path (parity with the Kotlin modules).</summary>
public static class DimenPercent
{
    public static float PSdp(IAppDimensContext context, int value) => value.PSdp(context);
    public static float PSdpa(IAppDimensContext context, int value) => value.PSdpa(context);
    public static float PSdpi(IAppDimensContext context, int value) => value.PSdpi(context);
    public static float PSdpia(IAppDimensContext context, int value) => value.PSdpia(context);
    public static float PHdp(IAppDimensContext context, int value) => value.PHdp(context);
    public static float PHdpa(IAppDimensContext context, int value) => value.PHdpa(context);
    public static float PHdpi(IAppDimensContext context, int value) => value.PHdpi(context);
    public static float PHdpia(IAppDimensContext context, int value) => value.PHdpia(context);
    public static float PWdp(IAppDimensContext context, int value) => value.PWdp(context);
    public static float PWdpa(IAppDimensContext context, int value) => value.PWdpa(context);
    public static float PWdpi(IAppDimensContext context, int value) => value.PWdpi(context);
    public static float PWdpia(IAppDimensContext context, int value) => value.PWdpia(context);

    public static float WarmupCache(IAppDimensContext context)
    {
        DimenCache.Init(context);
        return DimenCache.FastMetricsForCode(context).Density;
    }
}

public static class DimenPercentKernels
{
    private static float CalculatePercentDp(
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
        if (!applyAspectRatio) return baseValue * dim * DimenCache.InvBaseRatio;
        var diff = dim - DesignScaleConstants.BaseWidthDp;
        var adj = (customSensitivityK ?? DimenCache.SensitivityDefault) * m.LogNormalizedAspectRatio;
        return baseValue * (1f + diff * (DimenCache.AdjustmentScale + adj));
    }

    private static float CalculatePercentPx(
        float baseValue, ScreenConfiguration configuration, DpQualifier qualifier,
        Inverter inverter, bool ignoreMultiWindows, bool applyAspectRatio,
        float? customSensitivityK, IAppDimensContext? context)
    {
        var dp = CalculatePercentDp(baseValue, configuration, qualifier, inverter,
            ignoreMultiWindows, applyAspectRatio, customSensitivityK, context);
        return dp * (context?.Density ?? configuration.DensityDpi / 160f);
    }

    /// <summary>Cached full path (dp).</summary>
    public static float ToPercentDp(this float baseValue, IAppDimensContext context,
        DpQualifier qualifier = DpQualifier.SmallWidth, Inverter inverter = Inverter.Default,
        bool ignoreMultiWindows = false, bool applyAspectRatio = false, float? customSensitivityK = null)
    {
        var c = context.Configuration;
        var key = DimenCache.BuildKey(baseValue, c.IsLandscape, ignoreMultiWindows,
            DimenCache.CalcType.Percent, qualifier, inverter, applyAspectRatio, DimenCache.ValueType.Dp, customSensitivityK);
        var metrics = DimenCache.FastMetricsForCode(context);
        if (DimenCache.ShouldComputeDirectly(key))
            return CalculatePercentDp(baseValue, c, qualifier, inverter, ignoreMultiWindows, applyAspectRatio, customSensitivityK, context);
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
            () => CalculatePercentDp(baseValue, cfg, qualifier, inverter, ignoreMultiWindows, applyAspectRatio, customSensitivityK, context));
    }

    /// <summary>Cached full path (px).</summary>
    public static float ToPercentPx(this float baseValue, IAppDimensContext context,
        DpQualifier qualifier = DpQualifier.SmallWidth, Inverter inverter = Inverter.Default,
        bool ignoreMultiWindows = false, bool applyAspectRatio = false, float? customSensitivityK = null)
    {
        var c = context.Configuration;
        var key = DimenCache.BuildKey(baseValue, c.IsLandscape, ignoreMultiWindows,
            DimenCache.CalcType.Percent, qualifier, inverter, applyAspectRatio, DimenCache.ValueType.Px, customSensitivityK);
        var metrics = DimenCache.FastMetricsForCode(context);
        if (DimenCache.ShouldComputeDirectly(key))
            return CalculatePercentPx(baseValue, c, qualifier, inverter, ignoreMultiWindows, applyAspectRatio, customSensitivityK, context);
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
            () => CalculatePercentPx(baseValue, cfg, qualifier, inverter, ignoreMultiWindows, applyAspectRatio, customSensitivityK, context));
    }

    public static float ToPercentDp(this int v, IAppDimensContext context,
        DpQualifier qualifier = DpQualifier.SmallWidth, Inverter inverter = Inverter.Default,
        bool ignoreMultiWindows = false, bool applyAspectRatio = false, float? customSensitivityK = null)
        => ((float)v).ToPercentDp(context, qualifier, inverter, ignoreMultiWindows, applyAspectRatio, customSensitivityK);

    public static float ToPercentPx(this int v, IAppDimensContext context,
        DpQualifier qualifier = DpQualifier.SmallWidth, Inverter inverter = Inverter.Default,
        bool ignoreMultiWindows = false, bool applyAspectRatio = false, float? customSensitivityK = null)
        => ((float)v).ToPercentPx(context, qualifier, inverter, ignoreMultiWindows, applyAspectRatio, customSensitivityK);

    public static float ToPercentDp(this double v, IAppDimensContext context,
        DpQualifier qualifier = DpQualifier.SmallWidth, Inverter inverter = Inverter.Default,
        bool ignoreMultiWindows = false, bool applyAspectRatio = false, float? customSensitivityK = null)
        => ((float)v).ToPercentDp(context, qualifier, inverter, ignoreMultiWindows, applyAspectRatio, customSensitivityK);
}

public static class PercentExtensions
{
    public static float PSdp(this int v, IAppDimensContext ctx) => v.ToPercentDp(ctx, DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: false);
    public static float PSdp(this int v) => v.ToPercentDp(AppDimensAmbient.Require(), DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: false);
    public static float PSdpPx(this int v, IAppDimensContext ctx) => v.ToPercentPx(ctx, DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: false);
    public static float PSdpa(this int v, IAppDimensContext ctx) => v.ToPercentDp(ctx, DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: true);
    public static float PSdpa(this int v) => v.ToPercentDp(AppDimensAmbient.Require(), DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: true);
    public static float PSdpaPx(this int v, IAppDimensContext ctx) => v.ToPercentPx(ctx, DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: true);
    public static float PSdpi(this int v, IAppDimensContext ctx) => v.ToPercentDp(ctx, DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: false);
    public static float PSdpi(this int v) => v.ToPercentDp(AppDimensAmbient.Require(), DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: false);
    public static float PSdpiPx(this int v, IAppDimensContext ctx) => v.ToPercentPx(ctx, DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: false);
    public static float PSdpia(this int v, IAppDimensContext ctx) => v.ToPercentDp(ctx, DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: true);
    public static float PSdpia(this int v) => v.ToPercentDp(AppDimensAmbient.Require(), DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: true);
    public static float PSdpiaPx(this int v, IAppDimensContext ctx) => v.ToPercentPx(ctx, DpQualifier.SmallWidth, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: true);
    public static float PSdpCustom(this int v, DpQualifier qualifier, Inverter inverter, bool imw, bool ar, float? k = null) =>
        v.ToPercentDp(AppDimensAmbient.Require(), qualifier, inverter, imw, ar, k);
    public static float PSdpRotateRaw(this int v, IAppDimensContext ctx, float rawAlternate, Orientation orientation = Orientation.Landscape)
        => ScaledExtensionsHelpers.IsTargetOrientation(ctx.Configuration, orientation) ? rawAlternate : v.PSdp(ctx);

    public static float PHdp(this int v, IAppDimensContext ctx) => v.ToPercentDp(ctx, DpQualifier.Height, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: false);
    public static float PHdp(this int v) => v.ToPercentDp(AppDimensAmbient.Require(), DpQualifier.Height, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: false);
    public static float PHdpPx(this int v, IAppDimensContext ctx) => v.ToPercentPx(ctx, DpQualifier.Height, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: false);
    public static float PHdpa(this int v, IAppDimensContext ctx) => v.ToPercentDp(ctx, DpQualifier.Height, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: true);
    public static float PHdpa(this int v) => v.ToPercentDp(AppDimensAmbient.Require(), DpQualifier.Height, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: true);
    public static float PHdpaPx(this int v, IAppDimensContext ctx) => v.ToPercentPx(ctx, DpQualifier.Height, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: true);
    public static float PHdpi(this int v, IAppDimensContext ctx) => v.ToPercentDp(ctx, DpQualifier.Height, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: false);
    public static float PHdpi(this int v) => v.ToPercentDp(AppDimensAmbient.Require(), DpQualifier.Height, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: false);
    public static float PHdpiPx(this int v, IAppDimensContext ctx) => v.ToPercentPx(ctx, DpQualifier.Height, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: false);
    public static float PHdpia(this int v, IAppDimensContext ctx) => v.ToPercentDp(ctx, DpQualifier.Height, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: true);
    public static float PHdpia(this int v) => v.ToPercentDp(AppDimensAmbient.Require(), DpQualifier.Height, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: true);
    public static float PHdpiaPx(this int v, IAppDimensContext ctx) => v.ToPercentPx(ctx, DpQualifier.Height, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: true);
    public static float PHdpCustom(this int v, DpQualifier qualifier, Inverter inverter, bool imw, bool ar, float? k = null) =>
        v.ToPercentDp(AppDimensAmbient.Require(), qualifier, inverter, imw, ar, k);
    public static float PHdpRotateRaw(this int v, IAppDimensContext ctx, float rawAlternate, Orientation orientation = Orientation.Landscape)
        => ScaledExtensionsHelpers.IsTargetOrientation(ctx.Configuration, orientation) ? rawAlternate : v.PHdp(ctx);

    public static float PWdp(this int v, IAppDimensContext ctx) => v.ToPercentDp(ctx, DpQualifier.Width, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: false);
    public static float PWdp(this int v) => v.ToPercentDp(AppDimensAmbient.Require(), DpQualifier.Width, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: false);
    public static float PWdpPx(this int v, IAppDimensContext ctx) => v.ToPercentPx(ctx, DpQualifier.Width, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: false);
    public static float PWdpa(this int v, IAppDimensContext ctx) => v.ToPercentDp(ctx, DpQualifier.Width, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: true);
    public static float PWdpa(this int v) => v.ToPercentDp(AppDimensAmbient.Require(), DpQualifier.Width, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: true);
    public static float PWdpaPx(this int v, IAppDimensContext ctx) => v.ToPercentPx(ctx, DpQualifier.Width, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: true);
    public static float PWdpi(this int v, IAppDimensContext ctx) => v.ToPercentDp(ctx, DpQualifier.Width, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: false);
    public static float PWdpi(this int v) => v.ToPercentDp(AppDimensAmbient.Require(), DpQualifier.Width, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: false);
    public static float PWdpiPx(this int v, IAppDimensContext ctx) => v.ToPercentPx(ctx, DpQualifier.Width, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: false);
    public static float PWdpia(this int v, IAppDimensContext ctx) => v.ToPercentDp(ctx, DpQualifier.Width, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: true);
    public static float PWdpia(this int v) => v.ToPercentDp(AppDimensAmbient.Require(), DpQualifier.Width, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: true);
    public static float PWdpiaPx(this int v, IAppDimensContext ctx) => v.ToPercentPx(ctx, DpQualifier.Width, Inverter.Default, ignoreMultiWindows: true, applyAspectRatio: true);
    public static float PWdpCustom(this int v, DpQualifier qualifier, Inverter inverter, bool imw, bool ar, float? k = null) =>
        v.ToPercentDp(AppDimensAmbient.Require(), qualifier, inverter, imw, ar, k);
    public static float PWdpRotateRaw(this int v, IAppDimensContext ctx, float rawAlternate, Orientation orientation = Orientation.Landscape)
        => ScaledExtensionsHelpers.IsTargetOrientation(ctx.Configuration, orientation) ? rawAlternate : v.PWdp(ctx);

}

/// <summary>
/// Literal percentage of a screen axis or reference length (<c>10.SpaceW()</c> → 10% of width).
/// The <c>i</c> variants return the raw percent when the multi-window heuristic triggers.
/// </summary>
public static class PercentSpaceExtensions
{
    public static float SpaceW(this int v, IAppDimensContext ctx, bool ignoreMultiWindows = false) =>
        PercentOfScreen(v, DpQualifier.Width, ctx.Configuration, ignoreMultiWindows) * ctx.Density;
    public static float SpaceH(this int v, IAppDimensContext ctx, bool ignoreMultiWindows = false) =>
        PercentOfScreen(v, DpQualifier.Height, ctx.Configuration, ignoreMultiWindows) * ctx.Density;
    public static float SpaceSw(this int v, IAppDimensContext ctx, bool ignoreMultiWindows = false) =>
        PercentOfScreen(v, DpQualifier.SmallWidth, ctx.Configuration, ignoreMultiWindows) * ctx.Density;

    public static float SpaceWi(this int v, IAppDimensContext ctx) => v.SpaceW(ctx, ignoreMultiWindows: true);
    public static float SpaceHi(this int v, IAppDimensContext ctx) => v.SpaceH(ctx, ignoreMultiWindows: true);
    public static float SpaceSwi(this int v, IAppDimensContext ctx) => v.SpaceSw(ctx, ignoreMultiWindows: true);

    public static float SpaceWDp(this int v, IAppDimensContext ctx, bool ignoreMultiWindows = false) =>
        PercentOfScreen(v, DpQualifier.Width, ctx.Configuration, ignoreMultiWindows);
    public static float SpaceHDp(this int v, IAppDimensContext ctx, bool ignoreMultiWindows = false) =>
        PercentOfScreen(v, DpQualifier.Height, ctx.Configuration, ignoreMultiWindows);
    public static float SpaceSwDp(this int v, IAppDimensContext ctx, bool ignoreMultiWindows = false) =>
        PercentOfScreen(v, DpQualifier.SmallWidth, ctx.Configuration, ignoreMultiWindows);

    public static float Space(this int v, IAppDimensContext ctx, float referenceDp, bool ignoreMultiWindows = false) =>
        PercentOfReference(v, referenceDp) * ctx.Density;

    public static float SpaceDp(this int v, float referenceDp) => PercentOfReference(v, referenceDp);

    public static float SpaceWSp(this int v, IAppDimensContext ctx, bool fontScale = true, bool ignoreMultiWindows = false)
    {
        var dp = v.SpaceWDp(ctx, ignoreMultiWindows);
        return dp / Math.Max(fontScale ? ctx.Configuration.FontScale : 1f, 0.01f);
    }

    private static float PercentOfScreen(float percent, DpQualifier qualifier, ScreenConfiguration cfg, bool imw)
    {
        if (!float.IsFinite(percent)) return 0f;
        if (imw && DimenCalculationPlumbing.IsMultiWindowConstrained(cfg, true)) return percent;
        var dim = DimenCalculationPlumbing.ReadScreenDp(cfg, qualifier);
        return percent / 100f * dim;
    }

    private static float PercentOfReference(float percent, float referenceDp) =>
        !float.IsFinite(percent) ? 0f : percent / 100f * referenceDp;
}

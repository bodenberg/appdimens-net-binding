using AppDimens.Net.Common;
using AppDimens.Net.Code.Scaled;
using AppDimens.Net.Core;

namespace AppDimens.Net.Code.Fit;

/// <summary>
/// Fit strategy — breakpoint-style conditional entries (priority 1 qualifier+mode,
/// 2 mode-only, 3 qualifier+orientation, 4 orientation-only; first match wins), then
/// the chosen value resolves through the Scaled kernel. Parity with Kotlin
/// <c>DimenFit</c> (<c>ftsdp / fthdp / ftwdp</c>).
/// </summary>
public static class DimenFitKernels
{
    public static float CalculateFitDp(
        float baseValue, ScreenConfiguration configuration, DpQualifier qualifier,
        Inverter inverter, bool ignoreMultiWindows, bool applyAspectRatio,
        float? customSensitivityK, IAppDimensContext? context = null) =>
        DimenScaledKernels.CalculateScaledDp(baseValue, configuration, qualifier, inverter,
            ignoreMultiWindows, applyAspectRatio, customSensitivityK, context);

    public static float ToFitDp(this float baseValue, IAppDimensContext context,
        DpQualifier qualifier = DpQualifier.SmallWidth, Inverter inverter = Inverter.Default,
        bool ignoreMultiWindows = false, bool applyAspectRatio = false, float? customSensitivityK = null)
    {
        var c = context.Configuration;
        var key = DimenCache.BuildKey(baseValue, c.IsLandscape, ignoreMultiWindows,
            DimenCache.CalcType.Fit, qualifier, inverter, applyAspectRatio, DimenCache.ValueType.Dp, customSensitivityK);
        var metrics = DimenCache.FastMetricsForCode(context);
        if (DimenCache.ShouldComputeDirectly(key))
            return CalculateFitDp(baseValue, c, qualifier, inverter, ignoreMultiWindows, applyAspectRatio, customSensitivityK, context);
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
            () => CalculateFitDp(baseValue, cfg, qualifier, inverter, ignoreMultiWindows, applyAspectRatio, customSensitivityK, context));
    }

    public static float ToFitPx(this float baseValue, IAppDimensContext context,
        DpQualifier qualifier = DpQualifier.SmallWidth, Inverter inverter = Inverter.Default,
        bool ignoreMultiWindows = false, bool applyAspectRatio = false, float? customSensitivityK = null)
    {
        var c = context.Configuration;
        var key = DimenCache.BuildKey(baseValue, c.IsLandscape, ignoreMultiWindows,
            DimenCache.CalcType.Fit, qualifier, inverter, applyAspectRatio, DimenCache.ValueType.Px, customSensitivityK);
        var metrics = DimenCache.FastMetricsForCode(context);
        if (DimenCache.ShouldComputeDirectly(key))
            return CalculateFitDp(baseValue, c, qualifier, inverter, ignoreMultiWindows,
                applyAspectRatio, customSensitivityK, context) * context.Density;
        if (DimenCache.TryPeek(metrics, key, out var cachedValue)) return cachedValue;
        return DimenCache.Resolve(key, metrics,
            () => CalculateFitDp(baseValue, c, qualifier, inverter, ignoreMultiWindows,
                applyAspectRatio, customSensitivityK, context) * context.Density);
    }

    public static float ToFitDp(this int v, IAppDimensContext context,
        DpQualifier qualifier = DpQualifier.SmallWidth, Inverter inverter = Inverter.Default,
        bool ignoreMultiWindows = false, bool applyAspectRatio = false, float? customSensitivityK = null)
        => ((float)v).ToFitDp(context, qualifier, inverter, ignoreMultiWindows, applyAspectRatio, customSensitivityK);
}

public sealed class DimenFit
{
    private sealed class Entry(
        float customValue,
        UiModeType? uiModeType,
        DpQualifier? qualifierType,
        float qualifierValue,
        Orientation orientation,
        DpQualifier? finalQualifierResolver,
        Inverter inverter,
        int priority)
    {
        public readonly float CustomValue = customValue;
        public readonly UiModeType? UiModeType = uiModeType;
        public readonly DpQualifier? QualifierType = qualifierType;
        public readonly float QualifierValue = qualifierValue;
        public readonly Orientation Orientation = orientation;
        public readonly DpQualifier? FinalQualifierResolver = finalQualifierResolver;
        public readonly Inverter Inverter = inverter;
        public readonly int Priority = priority;
    }

    private readonly float _baseDp;
    private readonly List<Entry> _entries = [];
    private bool _ignoreMultiWindows;
    private bool _applyAspectRatio;
    private float? _customSensitivityK;

    private DimenFit(float baseDp) => _baseDp = baseDp;

    public static DimenFit Create(float baseDp) => new(baseDp);

    public DimenFit ApplyAspectRatio(bool apply = true) { _applyAspectRatio = apply; return this; }
    public DimenFit IgnoreMultiWindows(bool ignore = true) { _ignoreMultiWindows = ignore; return this; }
    public DimenFit CustomSensitivity(float k) { _customSensitivityK = k; return this; }

    private static int PriorityFor(bool hasQualifier, bool hasMode) =>
        hasQualifier ? (hasMode ? 1 : 3) : hasMode ? 2 : 4;

    public DimenFit Screen(UiModeType uiModeType, float customValue,
        DpQualifier? finalQualifierResolver = null)
    {
        _entries.Add(new Entry(customValue, uiModeType, null, 0f, Orientation.Default,
            finalQualifierResolver, Inverter.Default, PriorityFor(false, true)));
        return this;
    }

    public DimenFit Screen(DpQualifier qualifierType, float qualifierValue, float customValue,
        DpQualifier? finalQualifierResolver = null)
    {
        _entries.Add(new Entry(customValue, null, qualifierType, qualifierValue, Orientation.Default,
            finalQualifierResolver, Inverter.Default, PriorityFor(true, false)));
        return this;
    }

    public DimenFit Screen(UiModeType uiModeType, Orientation orientation, float customValue,
        DpQualifier? finalQualifierResolver = null)
    {
        _entries.Add(new Entry(customValue, uiModeType, null, 0f, orientation,
            finalQualifierResolver, Inverter.Default, PriorityFor(false, true)));
        return this;
    }

    public DimenFit Screen(Orientation orientation, float customValue)
    {
        _entries.Add(new Entry(customValue, null, null, 0f, orientation,
            null, Inverter.Default, PriorityFor(false, false)));
        return this;
    }

    /// <summary>Resolves ftsdp + fthdp + ftwdp in a single pass.</summary>
    public (float FtSdp, float FtHdp, float FtWdp) ResolveAll(IAppDimensContext ctx) =>
        (Resolve(ctx, DpQualifier.SmallWidth), Resolve(ctx, DpQualifier.Height), Resolve(ctx, DpQualifier.Width));

    public float Ftsdp(IAppDimensContext ctx) => Resolve(ctx, DpQualifier.SmallWidth);
    public float Fthdp(IAppDimensContext ctx) => Resolve(ctx, DpQualifier.Height);
    public float Ftwdp(IAppDimensContext ctx) => Resolve(ctx, DpQualifier.Width);

    public float Ftsdp() => Resolve(AppDimensAmbient.Require(), DpQualifier.SmallWidth);
    public float Fthdp() => Resolve(AppDimensAmbient.Require(), DpQualifier.Height);
    public float Ftwdp() => Resolve(AppDimensAmbient.Require(), DpQualifier.Width);

    internal float Resolve(IAppDimensContext ctx, DpQualifier defaultQualifier)
    {
        var cfg = ctx.Configuration;
        var currentUiMode = DimenCache.GetCachedUiModeType(ctx);

        Entry? found = null;
        foreach (var e in _entries.OrderBy(x => x.Priority))
        {
            var uiMatch = e.UiModeType is null || e.UiModeType == currentUiMode;
            var orientMatch = e.Orientation switch
            {
                Orientation.Landscape => cfg.IsLandscape,
                Orientation.Portrait => cfg.IsPortrait,
                _ => true,
            };
            if (!uiMatch || !orientMatch) continue;
            if (e.QualifierType is not null &&
                DimenCalculationPlumbing.ReadScreenDp(cfg, e.QualifierType.Value) < e.QualifierValue)
                continue;
            found = e;
            break;
        }

        var valueToUse = found?.CustomValue ?? _baseDp;
        var qualifier = found?.FinalQualifierResolver ?? defaultQualifier;
        var inverter = found?.Inverter ?? Inverter.Default;
        var dp = valueToUse.ToFitDp(ctx, qualifier, inverter,
            _ignoreMultiWindows, _applyAspectRatio, _customSensitivityK);
        return dp * ctx.Density;
    }
}

public static class DimenFitExtensions
{
    public static DimenFit FitScaledDp(this int value) => DimenFit.Create(value);
    public static DimenFit FitScaledDp(this double value) => DimenFit.Create((float)value);
    public static DimenFit FitScaledSp(this int value) => DimenFit.Create(value);
}

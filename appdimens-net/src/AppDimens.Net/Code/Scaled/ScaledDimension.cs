using AppDimens.Net.Common;
using AppDimens.Net.Core;

namespace AppDimens.Net.Code.Scaled;

/// <summary>
/// Fluent builder mirroring Kotlin <c>DimenScaled</c>: chain conditional entries whose
/// priority is defined inside the builder (not by lexical nesting), then resolve to a
/// final scaled value. Entry priority: 1 qualifier+mode, 2 mode-only, 3
/// qualifier+orientation, 4 orientation-only; first match wins.
/// </summary>
public sealed class ScaledDimension
{
    internal sealed class Entry(
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

    private readonly float _base;
    private readonly List<Entry> _entries = [];
    private bool _ignoreMultiWindows;
    private bool _applyAspectRatio;
    private float? _customSensitivityK;

    private ScaledDimension(float @base) => _base = @base;

    public static ScaledDimension Create(float @base) => new(@base);

    internal static ScaledDimension Create(float @base, Entry single)
    {
        var d = new ScaledDimension(@base);
        d._entries.Add(single);
        return d;
    }

    internal static Entry MakeEntry(float customValue, UiModeType? uiModeType, DpQualifier? qualifierType,
        float qualifierValue, Orientation orientation, DpQualifier? finalQualifierResolver, Inverter inverter) =>
        new(customValue, uiModeType, qualifierType, qualifierValue, orientation,
            finalQualifierResolver, inverter, PriorityFor(qualifierType is not null, uiModeType is not null));

    public ScaledDimension AspectRatio(bool apply = true) { _applyAspectRatio = apply; return this; }
    public ScaledDimension IgnoreMultiWindows(bool ignore = true) { _ignoreMultiWindows = ignore; return this; }
    public ScaledDimension CustomSensitivity(float k) { _customSensitivityK = k; return this; }

    private static int PriorityFor(bool hasQualifier, bool hasMode) =>
        hasQualifier ? (hasMode ? 1 : 3) : hasMode ? 2 : 4;

    public ScaledDimension Screen(UiModeType uiModeType, float customValue,
        DpQualifier? finalQualifierResolver = null)
    {
        _entries.Add(new Entry(customValue, uiModeType, null, 0f, Orientation.Default,
            finalQualifierResolver, Inverter.Default, PriorityFor(false, true)));
        return this;
    }

    public ScaledDimension Screen(DpQualifier qualifierType, float qualifierValue, float customValue,
        DpQualifier? finalQualifierResolver = null)
    {
        _entries.Add(new Entry(customValue, null, qualifierType, qualifierValue, Orientation.Default,
            finalQualifierResolver, Inverter.Default, PriorityFor(true, false)));
        return this;
    }

    public ScaledDimension Screen(UiModeType? uiModeType, DpQualifier? qualifierType,
        float qualifierValue, Orientation orientation, float customValue,
        DpQualifier? finalQualifierResolver = null, Inverter inverter = Inverter.Default)
    {
        _entries.Add(new Entry(customValue, uiModeType, qualifierType, qualifierValue, orientation,
            finalQualifierResolver, inverter, PriorityFor(qualifierType is not null, uiModeType is not null)));
        return this;
    }

    public ScaledDimension Screen(Orientation orientation, float customValue)
    {
        _entries.Add(new Entry(customValue, null, null, 0f, orientation,
            null, Inverter.Default, PriorityFor(false, false)));
        return this;
    }

    /// <summary>Resolves against an explicit window context.</summary>
    public float Resolve(IAppDimensContext ctx, DpQualifier defaultQualifier = DpQualifier.SmallWidth)
        => Resolve(_ignoreMultiWindows, _applyAspectRatio, _customSensitivityK, ctx, defaultQualifier);

    /// <summary>Resolves through the ambient context with the cached full path.</summary>
    public float Sdp() => Resolve(AppDimensAmbient.Require(), DpQualifier.SmallWidth);
    public float Hdp() => Resolve(AppDimensAmbient.Require(), DpQualifier.Height);
    public float Wdp() => Resolve(AppDimensAmbient.Require(), DpQualifier.Width);
    public float Px(DpQualifier defaultQualifier = DpQualifier.SmallWidth)
    {
        var v = Resolve(AppDimensAmbient.Require(), defaultQualifier);
        return v * AppDimensAmbient.Require().Density;
    }

    internal float Resolve(bool imw, bool ar, float? k, IAppDimensContext ctx, DpQualifier defaultQualifier)
    {
        var cfg = ctx.Configuration;
        var currentUiMode = DimenCache.GetCachedUiModeType(ctx);
        var isLandscape = cfg.IsLandscape;
        var isPortrait = cfg.IsPortrait;

        Entry? found = null;
        foreach (var e in _entries.OrderBy(x => x.Priority))
        {
            var modeMatch = e.UiModeType is null || e.UiModeType == currentUiMode;
            var orientationMatch = e.Orientation switch
            {
                Orientation.Landscape => isLandscape,
                Orientation.Portrait => isPortrait,
                _ => true,
            };
            if (!modeMatch || !orientationMatch) continue;
            if (e.QualifierType is not null &&
                DimenCalculationPlumbing.ReadScreenDp(cfg, e.QualifierType.Value) < e.QualifierValue)
                continue;
            found = e;
            break;
        }

        var valueToUse = found?.CustomValue ?? _base;
        var qualifier = found?.FinalQualifierResolver ?? defaultQualifier;
        var inverter = found?.Inverter ?? Inverter.Default;
        return valueToUse.ToDynamicScaledDp(ctx, qualifier, inverter, imw, ar, k);
    }
}

public static class ScaledDimensionExtensions
{
    public static ScaledDimension ScaledDp(this int value) => ScaledDimension.Create(value);
    public static ScaledDimension ScaledDp(this double value) => ScaledDimension.Create((float)value);
    public static ScaledDimension ScaledSp(this int value) => ScaledDimension.Create(value);
    public static ScaledDimension ScaledSp(this double value) => ScaledDimension.Create((float)value);
}

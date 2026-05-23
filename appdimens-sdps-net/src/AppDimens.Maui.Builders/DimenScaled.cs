using AppDimens.Maui.Core;
using AppDimens.Maui.Inverters;

namespace AppDimens.Maui.Builders;

public readonly record struct CustomDpEntry(
    UiModeType? UiModeType,
    DpQualifierEntry? DpQualifierEntry,
    OrientationQualifier? Orientation,
    int CustomValue,
    DpQualifier? FinalQualifierResolver,
    int Priority,
    InverterType Inverter = InverterType.Default);

public sealed class DimenScaled
{
    private readonly int _baseValue;
    private readonly List<CustomDpEntry> _entries;

    public DimenScaled(int baseValue, IEnumerable<CustomDpEntry>? entries = null)
    {
        _baseValue = baseValue;
        _entries = entries?.ToList() ?? new List<CustomDpEntry>();
    }

    public DimenScaled Screen(UiModeType uiMode, DpQualifier qualifierType, int qualifierValue,
        int customValue, DpQualifier? finalQualifier = null,
        OrientationQualifier? orientation = OrientationQualifier.Default,
        InverterType inverter = InverterType.Default)
        => Add(new CustomDpEntry(uiMode, new DpQualifierEntry(qualifierType, qualifierValue),
            orientation, customValue, finalQualifier, 1, inverter));

    public DimenScaled Screen(UiModeType uiMode, int customValue,
        DpQualifier? finalQualifier = null, OrientationQualifier? orientation = OrientationQualifier.Default,
        InverterType inverter = InverterType.Default)
        => Add(new CustomDpEntry(uiMode, null, orientation, customValue, finalQualifier, 2, inverter));

    public DimenScaled Screen(DpQualifier qualifierType, int qualifierValue, int customValue,
        DpQualifier? finalQualifier = null, OrientationQualifier? orientation = OrientationQualifier.Default,
        InverterType inverter = InverterType.Default)
        => Add(new CustomDpEntry(null, new DpQualifierEntry(qualifierType, qualifierValue),
            orientation, customValue, finalQualifier, 3, inverter));

    public DimenScaled Screen(OrientationQualifier orientation, int customValue,
        DpQualifier? finalQualifier = null, InverterType inverter = InverterType.Default)
        => Add(new CustomDpEntry(null, null, orientation, customValue, finalQualifier, 4, inverter));

    public DimenScaled Tablet(int customValue) =>
        Screen(qualifierType: DpQualifier.SmallWidth, qualifierValue: 600, customValue: customValue);

    public DimenScaled Landscape(int customValue) =>
        Screen(orientation: OrientationQualifier.Landscape, customValue: customValue);

    public DimenScaled Desktop(int customValue) =>
        Screen(uiMode: UiModeType.Desk, customValue: customValue);

    private DimenScaled Add(CustomDpEntry entry)
    {
        var list = new List<CustomDpEntry>(_entries) { entry };
        list.Sort((a, b) =>
        {
            var p = a.Priority.CompareTo(b.Priority);
            if (p != 0) return p;
            var av = a.DpQualifierEntry?.Value ?? 0;
            var bv = b.DpQualifierEntry?.Value ?? 0;
            return bv.CompareTo(av);
        });
        return new DimenScaled(_baseValue, list);
    }

    public double Sdp() => Resolve(DpQualifier.SmallWidth);
    public double Hdp() => Resolve(DpQualifier.Height);
    public double Wdp() => Resolve(DpQualifier.Width);

    private double Resolve(DpQualifier defaultQualifier)
    {
        var resolver = AppDimensResolver.Instance;
        var metrics = resolver.Metrics.Current;

        foreach (var entry in _entries)
        {
            if (!MatchesOrientation(entry.Orientation, metrics)) continue;
            if (entry.UiModeType.HasValue && entry.UiModeType.Value != metrics.UiMode) continue;
            if (entry.DpQualifierEntry.HasValue)
            {
                var qe = entry.DpQualifierEntry.Value;
                var metric = ScaleEngine.GetMetricDp(qe.Type, metrics);
                if (metric < qe.Value) continue;
            }

            var finalQ = entry.FinalQualifierResolver ?? defaultQualifier;
            return resolver.Resolve(entry.CustomValue, finalQ, entry.Inverter);
        }

        return resolver.Resolve(_baseValue, defaultQualifier);
    }

    private static bool MatchesOrientation(OrientationQualifier? required, ScreenMetricsSnapshot metrics)
    {
        if (required is null or OrientationQualifier.Default) return true;
        return required switch
        {
            OrientationQualifier.Landscape => metrics.Orientation == ScreenOrientation.Landscape,
            OrientationQualifier.Portrait => metrics.Orientation == ScreenOrientation.Portrait,
            _ => true,
        };
    }
}

public static class Responsive
{
    public static DimenScaled Value(int baseValue) => new(baseValue);
}

public static class DimenScaledExtensions
{
    public static DimenScaled ScaledDp(this int value) => new(value);
}

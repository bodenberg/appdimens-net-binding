namespace AppDimens.Maui.Core;

public static class ScaleEngine
{
    public static double Scale(int nominal, double metricDp)
        => nominal / AppDimensConstants.DesignBaseDp * metricDp;

    public static double Sdp(int nominal, double smallestDp) => Scale(nominal, smallestDp);

    public static double Hdp(int nominal, double heightDp) => Scale(nominal, heightDp);

    public static double Wdp(int nominal, double widthDp) => Scale(nominal, widthDp);

    public static double GetMetricDp(DpQualifier qualifier, ScreenMetricsSnapshot metrics) =>
        qualifier switch
        {
            DpQualifier.Height => metrics.HeightDp,
            DpQualifier.Width => metrics.WidthDp,
            _ => metrics.SmallestDp,
        };

    /// <summary>Axis-neutral resource key; <see cref="DpQualifier"/> selects the bucket axis at lookup.</summary>
    public static string BuildResourceKey(int value, DpQualifier qualifier = DpQualifier.SmallWidth) =>
        value < 0 ? $"_minus{Math.Abs(value)}" : $"_{value}";

    /// <summary>v1 resource keys with axis suffix for backward-compatible bucket lookup.</summary>
    public static string BuildLegacySuffixedKey(string axisNeutralKey, DpQualifier qualifier)
    {
        var suffix = qualifier switch
        {
            DpQualifier.Height => "hdp",
            DpQualifier.Width => "wdp",
            _ => "sdp",
        };
        return $"{axisNeutralKey}{suffix}";
    }

    public static void ValidateIndex(int value, bool allowNegative = true)
    {
        var min = allowNegative ? AppDimensConstants.MinIndex : AppDimensConstants.MinSspIndex;
        if (value < min || value > AppDimensConstants.MaxIndex)
            throw new ArgumentOutOfRangeException(nameof(value),
                $"Value must be between {min} and {AppDimensConstants.MaxIndex}. Current: {value}");
    }
}

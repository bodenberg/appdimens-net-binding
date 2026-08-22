using System.Globalization;
using AppDimens.Maui.Core;

namespace AppDimens.Maui.Converters;

public abstract class DimenConverterBase : IValueConverter
{
    protected abstract double ConvertIndex(int index);

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int i) return ConvertIndex(i);
        if (value is double d) return ConvertIndex((int)d);
        return 0d;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>Smallest-width scaled; set <see cref="Independent"/> for a resize-frozen value.</summary>
public sealed class SdpConverter : DimenConverterBase
{
    /// <summary>When true, resolves against the frozen baseline (suffix <c>i</c>) instead of the live screen.</summary>
    public bool Independent { get; set; }

    protected override double ConvertIndex(int index) =>
        Independent ? AppDimensSdps.Sdpi(index) : AppDimensSdps.Sdp(index);
}

public sealed class SspConverter : DimenConverterBase
{
    public bool Independent { get; set; }

    protected override double ConvertIndex(int index) =>
        Independent ? AppDimensSdps.Sspi(index) : AppDimensSdps.Ssp(index);
}

public sealed class HdpConverter : DimenConverterBase
{
    public bool Independent { get; set; }

    protected override double ConvertIndex(int index) =>
        Independent ? AppDimensSdps.Hdpi(index) : AppDimensSdps.Hdp(index);
}

public sealed class WdpConverter : DimenConverterBase
{
    public bool Independent { get; set; }

    protected override double ConvertIndex(int index) =>
        Independent ? AppDimensSdps.Wdpi(index) : AppDimensSdps.Wdp(index);
}

public sealed class HspConverter : DimenConverterBase
{
    protected override double ConvertIndex(int index) => AppDimensSdps.Hsp(index);
}

public sealed class WspConverter : DimenConverterBase
{
    protected override double ConvertIndex(int index) => AppDimensSdps.Wsp(index);
}

public sealed class SemConverter : DimenConverterBase
{
    protected override double ConvertIndex(int index) => AppDimensSdps.Sem(index);
}

public static class DimenPhysicalUnits
{
    public const double MmToCmFactor = 10.0;
    public const double MmToInchFactor = 25.4;

    /// <summary>px = mm ÷ 25.4 × dpi, where dpi = 160 × density.</summary>
    public static double MmToPx(double mm, double density) => mm / MmToInchFactor * density * 160.0;
    public static double CmToPx(double cm, double density) => MmToPx(cm * MmToCmFactor, density);
    public static double InchToPx(double inch, double density) => inch * density * 160.0;

    public static double MmToCm(double mm) => mm / MmToCmFactor;
    public static double MmToInch(double mm) => mm / MmToInchFactor;
    public static double CmToMm(double cm) => cm * MmToCmFactor;
    public static double CmToInch(double cm) => cm * MmToCmFactor / MmToInchFactor;
    public static double InchToCm(double inch) => inch * MmToInchFactor / MmToCmFactor;
    public static double InchToMm(double inch) => inch * MmToInchFactor;

    public static double Radius(double diameter, UnitType unit, double density) =>
        ConvertToPx(diameter, unit, density) / 2.0;

    public static double Diameter(double radius, UnitType unit, double density) =>
        ConvertToPx(radius, unit, density) * 2.0;

    public static double Circumference(double radius, UnitType unit, double density) =>
        2.0 * Math.PI * ConvertToPx(radius, unit, density);

    public static double Area(double radius, UnitType unit, double density)
    {
        var r = ConvertToPx(radius, unit, density);
        return Math.PI * r * r;
    }

    private static double ConvertToPx(double value, UnitType unit, double density) => unit switch
    {
        UnitType.Mm => MmToPx(value, density),
        UnitType.Cm => CmToPx(value, density),
        UnitType.Inch => InchToPx(value, density),
        UnitType.Dp => value * density,
        UnitType.Px => value,
        _ => value,
    };
}

public sealed class MmToPxConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double mm) return 0d;
        var density = AppDimensResolver.Instance.Metrics.Current.Density;
        return DimenPhysicalUnits.MmToPx(mm, density);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

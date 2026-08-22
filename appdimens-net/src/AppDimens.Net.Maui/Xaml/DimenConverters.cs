using System.Globalization;
using AppDimens.Net.Code.Scaled;

namespace AppDimens.Net.Maui.Xaml;

/// <summary>Converts a raw dp number to smallest-width scaled (16 → 16.Sdp()).</summary>
public sealed class SdpConverter : IValueConverter
{
    public bool AspectRatio { get; set; }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var v = System.Convert.ToSingle(value ?? 0);
        return AspectRatio ? ((float)v).ToDynamicScaledDp(AppDimensAmbient.Require(), applyAspectRatio: true)
            : ((int)v).Sdp(AppDimensAmbient.Require());
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value ?? 0d;
}

/// <summary>Converts a raw sp number to scalable text size.</summary>
public sealed class SspConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        System.Convert.ToInt32(value ?? 0).Ssp(AppDimensAmbient.Require());

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value ?? 0d;
}

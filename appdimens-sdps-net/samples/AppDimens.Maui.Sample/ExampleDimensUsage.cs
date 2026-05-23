using AppDimens.Maui;
using AppDimens.Maui.Builders;
using AppDimens.Maui.Core;
using AppDimens.Maui.Extensions;
using AppDimens.Maui.Helpers;
using AppDimens.Maui.Inverters;

namespace AppDimens.Maui.Sample;

/// <summary>
/// C# usage examples for the AppDimens MAUI API.
/// </summary>
public static class ExampleDimensUsage
{
    public static void Demonstrate()
    {
        AppDimensSdps.Initialize(new AppDimensOptions { ScalingMode = ScalingMode.Hybrid });

        // Core layout and font scaling
        var padding = 16.Sdp();
        var fontSize = 18.Ssp();

        // Axis inverters (portrait/landscape)
        var ph = 16.SdpPh();
        var lw = 16.SdpLw();

        // Aspect-ratio adjustment
        var withAr = 16.Sdpa();

        // Rotation-based value selection
        var rotated = 30.SdpRotate(45, orientation: OrientationQualifier.Landscape);

        // Multi-breakpoint responsive builder
        var scaled = AppDimens.Maui.Builders.Responsive.Value(14)
            .Tablet(18)
            .Landscape(16)
            .Desktop(20)
            .Sdp();

        // Qualifier threshold override
        var qualified = 16.SdpQualifier(20, DpQualifier.SmallWidth, 600);

        _ = (padding, fontSize, ph, lw, withAr, rotated, scaled, qualified);
    }

    public static string XamlExample() =>
        """
        xmlns:dimen="clr-namespace:AppDimens.Maui.Xaml;assembly=AppDimens.Maui.Xaml"

        <VerticalStackLayout Padding="{dimen:Sdp Value=16}" Spacing="{dimen:Sdp Value=8}">
            <Label Text="AppDimens MAUI" FontSize="{dimen:Ssp Value=20}" />
            <Label Text="Inverter" Padding="{dimen:Sdp Value=12}" Margin="{dimen:Sdp Value=4}" />
            <Label Text="SwToPh" FontSize="{dimen:Ssp Value=16, Inverter=SwToPh}" />
            <Label Text="Scaled" FontSize="{dimen:Scaled Value=14, Tablet=18, Landscape=16, Desktop=20}" />
            <BoxView HeightRequest="{dimen:Hdp Value=48}" WidthRequest="{dimen:Sdp Value=24}" CornerRadius="{dimen:Sdp Value=6}" />
        </VerticalStackLayout>
        """;
}

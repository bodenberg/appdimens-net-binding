using AppDimens.Net;
using AppDimens.Net.Code.Auto;
using AppDimens.Net.Code.Density;
using AppDimens.Net.Code.Diagonal;
using AppDimens.Net.Code.Fill;
using AppDimens.Net.Code.Fluid;
using AppDimens.Net.Code.Interpolated;
using AppDimens.Net.Code.Logarithmic;
using AppDimens.Net.Code.Percent;
using AppDimens.Net.Code.Perimeter;
using AppDimens.Net.Code.Power;

namespace AppDimens.Sample.Ui.Pages;

/// <summary>All satellite strategies side by side.</summary>
public class StrategiesPage : ContentPage
{
    public StrategiesPage()
    {
        Title = "Strategies";
        BackgroundColor = M3.Background;
        var s = new VerticalStackLayout { Padding = new Thickness(16), Spacing = 6 };

        Section(s, "Percent (psdp + literal space)");
        Row(s, () => ("24.psdp()", 24.PSdp(AppDimensAmbient.Require())));
        Row(s, () => ("24.psdpa()", 24.PSdpa(AppDimensAmbient.Require())));
        Row(s, () => ("10.spaceWDp() → 10% width", 10.SpaceWDp(AppDimensAmbient.Require())));

        Section(s, "Power / Interpolated");
        Row(s, () => ("20.pwsdp()", 20.PWSdp(AppDimensAmbient.Require())));
        Row(s, () => ("20.isdp()", 20.ISdp(AppDimensAmbient.Require())));

        Section(s, "Fluid / Auto");
        Row(s, () => ("28.fsdp()", ((float)28).ToFluidDp(AppDimensAmbient.Require())));
        Row(s, () => ("28.asdp()", 28.ASdp(AppDimensAmbient.Require())));

        Section(s, "Density");
        Row(s, () => ("12.dsdp()", 12.DSdp(AppDimensAmbient.Require())));

        Section(s, "Diagonal / Perimeter / Logarithmic");
        Row(s, () => ("18.dgsdp()", 18.DGSdp(AppDimensAmbient.Require())));
        Row(s, () => ("18.prsdp()", 18.PRSdp(AppDimensAmbient.Require())));
        Row(s, () => ("18.logsdp()", 18.LOGSdp(AppDimensAmbient.Require())));

        Content = new ScrollView { Content = s };
    }

    private static void Section(VerticalStackLayout parent, string t) => parent.Add(new Label
    {
        Text = t,
        FontSize = 17,
        FontAttributes = FontAttributes.Bold,
        TextColor = M3.Tertiary,
        Margin = new Thickness(0, 10, 0, 2),
    });

    private static void Row(VerticalStackLayout parent, Func<(string, float)> calc)
    {
        try
        {
            var (caption, v) = calc();
            parent.Add(new Border
            {
                StrokeThickness = 0,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(12) },
                BackgroundColor = M3.CardBackground,
                Padding = new Thickness(12, 8),
                Content = new Label { Text = $"{caption}  →  {v:0.##}", FontSize = 14, TextColor = M3.OnSurface },
            });
        }
        catch (Exception e)
        {
            parent.Add(new Label { Text = $"error: {e.Message}", FontSize = 13, TextColor = Colors.Red });
        }
    }
}

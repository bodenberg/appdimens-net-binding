using AppDimens.Net;
using AppDimens.Net.Code.Fit;
using AppDimens.Net.Code.Scaled;
using AppDimens.Net.Common;

namespace AppDimens.Sample.Ui.Pages;

/// <summary>ScaledDimension and DimenFit builders — priority-driven conditional entries.</summary>
public class BuildersPage : ContentPage
{
    public BuildersPage()
    {
        Title = "Builders";
        BackgroundColor = M3.Background;
        var s = new VerticalStackLayout { Padding = new Thickness(16), Spacing = 8 };

        var scaled = 16.ScaledDp()
            .AspectRatio(true)
            .Screen(DpQualifier.SmallWidth, 600, 24)
            .Screen(UiModeType.Television, 40);
        Row(s, () => ("ScaledDimension(16).a.screen(600→24).screen(tv→40).sdp()", scaled.Sdp()));

        var fit = 18.FitScaledDp()
            .ApplyAspectRatio(true)
            .Screen(UiModeType.Television, Orientation.Landscape, 30)
            .Screen(DpQualifier.SmallWidth, 600, 26)
            .Screen(Orientation.Portrait, 22);
        Row(s, () => ("DimenFit(18) chain → ftsdp()", fit.Ftsdp()));

        try
        {
            var (fs, fh, fw) = fit.ResolveAll(AppDimensAmbient.Require());
            s.Add(new Border
            {
                StrokeThickness = 0,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(12) },
                BackgroundColor = Color.FromArgb("#FFF3E0"),
                Padding = new Thickness(12, 8),
                Content = new Label
                {
                    Text = $"ResolveAll:  sw={fs:0.##}   h={fh:0.##}   w={fw:0.##}",
                    FontSize = 14,
                    TextColor = M3.OnSurface,
                },
            });
        }
        catch (Exception e)
        {
            s.Add(new Label { Text = $"error: {e.Message}", FontSize = 13, TextColor = Colors.Red });
        }

        Content = new ScrollView { Content = s };
    }

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
                Margin = new Thickness(0, 2),
                Content = new Label { Text = $"{caption}\n   → {v:0.##}", FontSize = 14, TextColor = M3.OnSurface },
            });
        }
        catch (Exception e)
        {
            parent.Add(new Label { Text = $"error: {e.Message}", FontSize = 13, TextColor = Colors.Red });
        }
    }
}

using AppDimens.Net;
using AppDimens.Net.Code.Scaled;
using AppDimens.Net.Common;
using AppDimens.Net.Core;

namespace AppDimens.Sample.Ui.Pages;

/// <summary>Scaled family: sdp/hdp/wdp/ssp, suffixes a/i/ia, inverters, facilitators.</summary>
public class ScaledDemoPage : ContentPage
{
    public ScaledDemoPage()
    {
        Title = "Scaled";
        BackgroundColor = M3.Background;
        var pad = new Thickness(16);
        var stack = new VerticalStackLayout { Padding = pad, Spacing = 8 };

        stack.Add(Section("Base axes"));
        Row(stack, () => ("16.sdp()", 16.Sdp()));
        Row(stack, () => ("16.sdpa()", 16.Sdpa()));
        Row(stack, () => ("48.hdp()", 48.Hdp()));
        Row(stack, () => ("100.wdp()", 100.Wdp()));

        stack.Add(Section("Ignore multi-window (i)"));
        Row(stack, () => ("24.sdpi()", 24.Sdpi()));
        Row(stack, () => ("24.sdpia()", 24.Sdpia()));

        stack.Add(Section("Orientation inverters"));
        var ctx = AppDimensAmbient.Require();
        Row(stack, () => ("32.sdpPh() portrait→height", 32.SdpPh(ctx)));
        Row(stack, () => ("50.hdpLw() landscape→width", 50.HdpLw(ctx)));
        Row(stack, () => ("50.wdpLh() portrait→height", 50.WdpLh(ctx)));

        stack.Add(Section("Facilitators"));
        Row(stack, () => ("16.sdpRotate(28, Landscape)", 16.SdpRotate(ctx, 28)));
        Row(stack, () => ("30.sdpMode(44, Television)", 30.SdpMode(ctx, 44, UiModeType.Television)));
        Row(stack, () => ("60.sdpQualifier(sw≥600→120)",
            60.SdpQualifier(ctx, DpQualifier.SmallWidth, 600, 120)));

        stack.Add(Section("Builder chain (DimenScaled parity)"));
        var built = 16.ScaledDp()
            .AspectRatio(true)
            .Screen(DpQualifier.SmallWidth, 600, 24)
            .Screen(UiModeType.Television, 40)
            .Sdp();
        stack.Add(Card($"scaledDp(16).a.screen(sw≥600→24).screen(tv→40).sdp()\n   → {built:0.##}"));

        Content = new ScrollView { Content = stack };
    }

    private static Label Section(string t) => new()
    {
        Text = t,
        FontSize = 17,
        FontAttributes = FontAttributes.Bold,
        TextColor = M3.Tertiary,
        Margin = new Thickness(0, 10, 0, 2),
    };

    private static View Card(string text) => new Border
    {
        StrokeThickness = 0,
        StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(12) },
        BackgroundColor = M3.CardBackground,
        Padding = new Thickness(12, 8),
        Margin = new Thickness(0, 2),
        Content = new Label { Text = text, FontSize = 14, TextColor = M3.OnSurface },
    };

    private static void Row(VerticalStackLayout s, Func<(string Caption, float Value)> calc)
    {
        try
        {
            var (caption, v) = calc();
            s.Add(Card($"{caption}  →  {v:0.##}"));
        }
        catch (Exception e)
        {
            s.Add(Card($"error: {e.Message}"));
        }
    }
}

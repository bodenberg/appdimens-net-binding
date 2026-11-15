using AppDimens.Net;
using AppDimens.Net.Custom;

namespace AppDimens.Sample.Ui.Pages;

/// <summary>
/// Custom scaling strategies (AppDimens.Net.Custom): a programmable factor, a typed code formula,
/// a text formula, a per-device builder and a scoped default — each row shows the value in dp
/// for the current window, so resizing the window updates the numbers.
/// PT: Estratégias de escala personalizadas — fator programável, fórmula em código, fórmula em texto,
/// builder por dispositivo e padrão com escopo.
/// </summary>
public class CustomStrategyPage : ContentPage
{
    // Declare strategies once (static readonly), never inside a layout pass.
    private static readonly ScalingStrategy Product =
        CustomScaleStrategy.Create(s => s.Clamp(s.Width / 360f, 0.85f, 1.8f));

    private static readonly ScalingStrategy Hud =
        ScalingStrategy.Formula(s => s.Base * (s.Tablet ? 0.9f : 1f) * s.Clamp(s.Sw / 360f, 0.85f, 1.6f));

    private static readonly ScalingStrategy TextFormula =
        ScalingStrategy.Formula("base * pow(sw / 360, 0.8)");

    private static readonly ScalingStrategy Brand = ScalingStrategy.Define("sample-brand", b =>
    {
        b.SetReference(360, 640);
        b.Curve = ScalingCurve.Power(0.8f);
        b.Clamp(0.85f, 1.6f);
        b.On(DeviceClass.Tablet, r => r.Curve = ScalingCurve.Logarithmic(0.4f));
        b.On(DeviceClass.Tv, r => r.Multiply(0.8f));
    }, register: false);

    private readonly VerticalStackLayout _list = new() { Padding = new Thickness(16), Spacing = 6 };

    public CustomStrategyPage()
    {
        Title = "Custom strategies";
        BackgroundColor = M3.Background;
        Content = new ScrollView { Content = _list };
        SizeChanged += (_, _) => Build();
        Build();
    }

    private void Build()
    {
        _list.Children.Clear();
        _list.Add(M3.Paragraph("Write the math once, use it everywhere: 16.Cdp(strategy) → dp, 14.Csp(strategy) → text dp."));
        _list.Add(M3.Paragraph("Escreva a conta uma vez e use em qualquer lugar.", small: true));

        Section("Factor — CustomScaleStrategy.Create(s => s.Clamp(s.Width / 360f, 0.85f, 1.8f))");
        Row("16.Cdp(Product)", () => 16.Cdp(Product));
        Row("16.CdpPx(Product) (px)", () => 16.CdpPx(Product));
        Row("14.Csp(Product)", () => 14.Csp(Product));
        Swatch(() => 48.Cdp(Product));

        Section("Code formula — ScalingStrategy.Formula(s => s.Base * …)");
        Row("16.Cdp(Hud)", () => 16.Cdp(Hud));
        Row("24.Cdp(Hud)", () => 24.Cdp(Hud));

        Section("Text formula — \"base * pow(sw / 360, 0.8)\"");
        Row("16.Cdp(TextFormula)", () => 16.Cdp(TextFormula));
        Row("16.Cdp(\"base * ratio\") inline", () => 16.Cdp("base * ratio"));

        Section("Builder — reference, curve, clamp, rules per device");
        Row("16.Cdp(Brand)", () => 16.Cdp(Brand));
        Row("14.Csp(Brand)", () => 14.Csp(Brand));

        Section("Scoped default — using (CustomDimens.Use(Brand))");
        Row("16.Cdp() with no scope", () => 16.Cdp());
        Row("16.Cdp() inside Use(Brand)", () =>
        {
            using (CustomDimens.Use(Brand)) return 16.Cdp();
        });
    }

    private void Section(string text) => _list.Add(new Label
    {
        Text = text,
        FontSize = 15,
        FontAttributes = FontAttributes.Bold,
        TextColor = M3.Tertiary,
        Margin = new Thickness(0, 10, 0, 2),
    });

    private void Row(string caption, Func<float> calc)
    {
        string text;
        try { text = $"{caption}  →  {calc():0.##}"; }
        catch (Exception e) { text = $"{caption}  →  error: {e.Message}"; }
        _list.Add(new Border
        {
            StrokeThickness = 0,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(12) },
            BackgroundColor = M3.CardBackground,
            Padding = new Thickness(12, 8),
            Content = new Label { Text = text, FontSize = 14, TextColor = M3.OnSurface },
        });
    }

    private void Swatch(Func<float> size)
    {
        try
        {
            var side = size();
            _list.Add(new BoxView
            {
                WidthRequest = side,
                HeightRequest = side,
                CornerRadius = 8,
                Color = M3.Primary,
                HorizontalOptions = LayoutOptions.Start,
            });
        }
        catch { /* ambient context not ready */ }
    }
}

using AppDimens.Net;
using AppDimens.Net.Code.Resize;
using AppDimens.Net.Code.Units;
using AppDimens.Net.Common;
using AppDimens.Net.Core;

namespace AppDimens.Sample.Ui.Pages;

/// <summary>Auto-resize inside a live-resizable box + physical units.</summary>
public class ResizeDemoPage : ContentPage
{
    private readonly Label _textLabel;
    private readonly Label _squareCaption;
    private readonly Border _square;
    private readonly Grid _box;
    private double _boxWidth = 320;

    public ResizeDemoPage()
    {
        Title = "Resize & units";
        BackgroundColor = M3.Background;
        var root = new VerticalStackLayout { Padding = new Thickness(16), Spacing = 12 };

        var slider = new Slider(160, 480, _boxWidth);
        slider.ValueChanged += (_, e) =>
        {
            _boxWidth = e.NewValue;
            _box.WidthRequest = _boxWidth;
            Refit();
        };

        _textLabel = new Label
        {
            Text = "Headline that must fit",
            FontAttributes = FontAttributes.Bold,
            FontSize = 14,
            TextColor = M3.OnSurface,
            HorizontalOptions = LayoutOptions.Center,
            LineBreakMode = LineBreakMode.WordWrap,
        };
        _squareCaption = new Label
        {
            TextColor = Colors.White,
            FontSize = 12,
            FontAttributes = FontAttributes.Bold,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center,
            InputTransparent = true,
        };
        _square = new Border
        {
            StrokeThickness = 0,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(8) },
            BackgroundColor = Color.FromArgb("#42A5F5"),
            WidthRequest = 64,
            HeightRequest = 64,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Content = _squareCaption,
        };
        _box = new Grid
        {
            BackgroundColor = Color.FromArgb("#EEF2F8"),
            WidthRequest = _boxWidth,
            HeightRequest = 140,
            Padding = 8,
            Children = { _textLabel, _square },
        };

        root.Add(new Label { Text = "Drag to resize the box — sizes re-fit automatically", FontSize = 14, TextColor = M3.CardDescription });
        root.Add(slider);
        root.Add(_box);

        root.Add(new Label
        {
            Text = "Physical units (mm/cm/inch)",
            FontSize = 17,
            FontAttributes = FontAttributes.Bold,
            TextColor = M3.Tertiary,
            Margin = new Thickness(0, 14, 0, 0),
        });
        Row(root, () => ("10mm → dp", DimenPhysicalUnits.ToDpFromMm(10, Ctx())));
        Row(root, () => ("1cm → dp", DimenPhysicalUnits.ToDpFromCm(1, Ctx())));
        Row(root, () => ("0.5in → px", DimenPhysicalUnits.ToPxFromInch(0.5f, Ctx())));
        Row(root, () => ("5mm radius (dp)", DimenPhysicalUnits.RadiusFromDiameter(5, UnitType.Mm, Ctx())));

        Content = new ScrollView { Content = root };
    }

    private static IAppDimensContext Ctx() => AppDimensAmbient.Require();

    private void Refit()
    {
        try
        {
            var ctx = Ctx();
            var density = Math.Max(ctx.Density, 0.01f);
            var fontDiv = density * Math.Max(ctx.Configuration.FontScale, 0.01f);
            var boxPx = (float)(_box.Width > 0 ? _box.Width : _boxWidth) * density;
            var innerPx = Math.Max(boxPx - 16f * density, density);

            var spPx = DimenResize.AutoResizeTextPx("Headline that must fit", innerPx,
                12, 28, 1, maxLines: 2, ctx: ctx);
            _textLabel.FontSize = Math.Clamp(spPx / fontDiv, 12, 28);

            var sq = DimenResize.AutoResizeSquarePx(boxPx, 140f * density, 24, 96, 4, ctx);
            _square.WidthRequest = sq;
            _square.HeightRequest = sq;
            _squareCaption.Text = $"{(int)Math.Round(sq)}dp";
        }
        catch { /* ambient not ready */ }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _box.SizeChanged += (_, _) => Refit();
        Refit();
    }

    private static void Row(Layout p, Func<(string, float)> calc)
    {
        try
        {
            var (caption, v) = calc();
            p.Add(new Border
            {
                StrokeThickness = 0,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(12) },
                BackgroundColor = M3.CardBackground,
                Padding = new Thickness(12, 8),
                Margin = new Thickness(0, 2),
                Content = new Label { Text = $"{caption}  →  {v:0.##}", FontSize = 14, TextColor = M3.OnSurface },
            });
        }
        catch (Exception e)
        {
            p.Add(new Label { Text = $"error: {e.Message}", FontSize = 13, TextColor = Colors.Red });
        }
    }
}

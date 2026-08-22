using AppDimens.Net;
using Microsoft.Maui.Controls.Shapes;

namespace AppDimens.Sample.Ui;

/// <summary>
/// Material 3 light-theme building blocks replicating the Compose demo of
/// appdimens-kmp: baseline color scheme, rounded example cards with a live
/// colored box showing the resolved dimension, and section titles with divider.
/// Font sizes follow the M3 type scale in plain sp (like the KMP typography);
/// only layout paddings and demo boxes scale with the selected strategy.
/// </summary>
public static class M3
{
    // Baseline lightColorScheme() defaults (Material 3).
    public static readonly Color Primary = Color.FromArgb("#6750A4");
    public static readonly Color Tertiary = Color.FromArgb("#7D5260");
    public static readonly Color Background = Color.FromArgb("#FEF7FF");
    public static readonly Color OnSurface = Color.FromArgb("#1C1B1F");
    public static readonly Color SurfaceVariant = Color.FromArgb("#E7E0EC");
    public static readonly Color OutlineVariant = Color.FromArgb("#CAC4D0");

    // Card palette from the KMP ExampleCard composables.
    public static readonly Color CardBackground = Color.FromArgb("#F7F7F7");
    public static readonly Color CardDescription = Color.FromArgb("#616161");
    public static readonly Color SpPlainCard = Color.FromArgb("#E3F2FD");
    public static readonly Color SpPlainText = Color.FromArgb("#1565C0");
    public static readonly Color BuilderCard = Color.FromArgb("#FFF3E0");
    public static readonly Color BuilderBox = Color.FromArgb("#FF9800");
    public static readonly Color ResizeCard = Color.FromArgb("#E8F5E9");
    public static readonly Color ResizeBorder1 = Color.FromArgb("#81C784");
    public static readonly Color ResizeBorder2 = Color.FromArgb("#66BB6A");
    public static readonly Color ResizeBorder3 = Color.FromArgb("#A5D6A7");
    public static readonly Color ResizeSquare = Color.FromArgb("#43A047");
    public static readonly Color ResizeBarW = Color.FromArgb("#1B5E20");
    public static readonly Color ResizeBarH = Color.FromArgb("#33691E");
    public static readonly Color ResizeText = Color.FromArgb("#2E7D32");

    public const double HeadlineMedium = 28;
    public const double TitleLarge = 22;
    public const double TitleSmall = 14;
    public const double BodyMedium = 14;
    public const double BodySmall = 12;
    public const double LabelLarge = 14;

    /// <summary>16dp page padding scaled by the strategy (KMP <c>16.demoSwDp</c>).</summary>
    public static Thickness Pad(DemoCalcStrategy s, double value = 16)
    {
        var v = Scale(s, value);
        return new Thickness(v, v, v, v);
    }

    /// <summary>Scales a dp value by the strategy factor (smallest-width axis).</summary>
    public static double Scale(DemoCalcStrategy s, double value) =>
        Math.Max(1, ((int)value).Dp(s, AppDimensAmbient.Require()));

    public static Label Title(string text) => new()
    {
        Text = text,
        FontSize = HeadlineMedium,
        FontAttributes = FontAttributes.Bold,
        TextColor = Primary,
        HorizontalTextAlignment = TextAlignment.Center,
        LineBreakMode = LineBreakMode.WordWrap,
    };

    public static Label Paragraph(string text, bool small = false, Color? color = null) => new()
    {
        Text = text,
        FontSize = small ? BodySmall : BodyMedium,
        TextColor = color ?? CardDescription,
        HorizontalTextAlignment = TextAlignment.Center,
        LineBreakMode = LineBreakMode.WordWrap,
    };

    public static View SectionTitle(string text, Thickness pad) => new VerticalStackLayout
    {
        Margin = new Thickness(0, pad.Top * 0.4, 0, 0),
        Children =
        {
            new BoxView { Color = OutlineVariant, HeightRequest = 1, Margin = new Thickness(0, 0, 0, 6) },
            new Label
            {
                Text = text,
                FontSize = TitleLarge,
                FontAttributes = FontAttributes.Bold,
                TextColor = Tertiary,
            },
        },
    };

    /// <summary>Rounded box showing the resolved dimension (KMP demonstration Box).</summary>
    public static View DimBox(double sizeDp, Color boxColor, string caption)
    {
        sizeDp = Math.Max(24, sizeDp);
        return new Border
        {
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(12) },
            BackgroundColor = boxColor,
            WidthRequest = sizeDp,
            HeightRequest = sizeDp,
            HorizontalOptions = LayoutOptions.Center,
            Content = new Label
            {
                Text = $"{(int)Math.Round(sizeDp)}dp",
                TextColor = Colors.White,
                FontSize = BodySmall,
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
            },
        };
    }

    /// <summary>KMP ExampleCard: rounded card + title + description + centered colored box.</summary>
    public static View ExampleCard(string title, string description, Func<double> boxSizeDp,
        Color boxColor, Thickness pad)
    {
        var stack = new VerticalStackLayout { Spacing = Math.Max(6, pad.Left * 0.6) };
        stack.Add(new Label { Text = title, FontSize = TitleSmall, FontAttributes = FontAttributes.Bold, TextColor = OnSurface });
        stack.Add(new Label { Text = description, FontSize = BodySmall, TextColor = CardDescription, LineBreakMode = LineBreakMode.WordWrap });
        stack.Add(DimBox(boxSizeDp(), boxColor, "dp"));

        return new Border
        {
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(16) },
            BackgroundColor = CardBackground,
            Margin = new Thickness(0, 4),
            Padding = pad,
            Content = stack,
        };
    }

    /// <summary>Tinted container card (sspRotatePlain / builder / auto-resize cards).</summary>
    public static View TintedCard(Color background, Thickness pad, params View[] content)
    {
        var stack = new VerticalStackLayout { Spacing = Math.Max(6, pad.Left * 0.6) };
        foreach (var v in content) stack.Add(v);
        return new Border
        {
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(16) },
            BackgroundColor = background,
            Padding = pad,
            Margin = new Thickness(0, 4),
            Content = stack,
        };
    }

    public static Label CardTitle(string t) => new()
    { Text = t, FontSize = TitleSmall, FontAttributes = FontAttributes.Bold, TextColor = OnSurface };

    public static Label CardBody(string t, Color? color = null) => new()
    {
        Text = t,
        FontSize = BodySmall,
        TextColor = color ?? CardDescription,
        LineBreakMode = LineBreakMode.WordWrap,
    };

    /// <summary>M3 OutlinedButton-look full-width button.</summary>
    public static Button Outlined(string text, Action? onClick = null)
    {
        var b = new Button
        {
            Text = text,
            FontSize = LabelLarge,
            FontAttributes = FontAttributes.Bold,
            TextColor = Primary,
            BackgroundColor = Colors.Transparent,
            BorderColor = OutlineVariant,
            BorderWidth = 1,
            CornerRadius = 12,
            Padding = new Thickness(16, 12),
            HorizontalOptions = LayoutOptions.Fill,
        };
        if (onClick is not null) b.Clicked += (_, _) => onClick();
        return b;
    }
}

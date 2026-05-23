using AppDimens.Maui.Extensions;

namespace AppDimens.Maui.Sample.Controls;

public sealed class DimenSampleCard : Border
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(DimenSampleCard), string.Empty);

    public static readonly BindableProperty SubtitleProperty =
        BindableProperty.Create(nameof(Subtitle), typeof(string), typeof(DimenSampleCard), string.Empty);

    public static readonly BindableProperty ResolvedPxProperty =
        BindableProperty.Create(nameof(ResolvedPx), typeof(double), typeof(DimenSampleCard), 0.0,
            propertyChanged: OnVisualChanged);

    public static readonly BindableProperty AccentColorProperty =
        BindableProperty.Create(nameof(AccentColor), typeof(Color), typeof(DimenSampleCard), Colors.Indigo,
            propertyChanged: OnVisualChanged);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Subtitle
    {
        get => (string)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    public double ResolvedPx
    {
        get => (double)GetValue(ResolvedPxProperty);
        set => SetValue(ResolvedPxProperty, value);
    }

    public Color AccentColor
    {
        get => (Color)GetValue(AccentColorProperty);
        set => SetValue(AccentColorProperty, value);
    }

    private readonly Label _titleLabel = new() { FontAttributes = FontAttributes.Bold };
    private readonly Label _subtitleLabel = new() { TextColor = Color.FromArgb("#94A3B8") };
    private readonly Label _valueLabel = new() { FontAttributes = FontAttributes.Bold, HorizontalTextAlignment = TextAlignment.End };
    private readonly BoxView _preview = new() { VerticalOptions = LayoutOptions.Center };

    public DimenSampleCard()
    {
        Style = (Style)Application.Current!.Resources["Card"];
        _titleLabel.FontSize = 15.Sdp();
        _subtitleLabel.FontSize = 11.Sdp();
        _valueLabel.FontSize = 13.Sdp();
        _preview.CornerRadius = 6.Sdp();

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(new GridLength(56.Sdp())),
                new ColumnDefinition(new GridLength(72.Sdp())),
            },
            ColumnSpacing = 10.Sdp(),
        };
        var stack = new VerticalStackLayout { Spacing = 2.Sdp(), VerticalOptions = LayoutOptions.Center };
        stack.Children.Add(_titleLabel);
        stack.Children.Add(_subtitleLabel);
        Grid.SetColumn(stack, 0);
        Grid.SetColumn(_preview, 1);
        Grid.SetColumn(_valueLabel, 2);
        grid.Children.Add(stack);
        grid.Children.Add(_preview);
        grid.Children.Add(_valueLabel);
        Content = grid;
        UpdateVisuals();
    }

    private static void OnVisualChanged(BindableObject b, object oldValue, object newValue)
    {
        if (b is DimenSampleCard card)
            card.UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        _titleLabel.Text = Title;
        _titleLabel.TextColor = Colors.White;
        _subtitleLabel.Text = Subtitle;
        _valueLabel.Text = $"{ResolvedPx:F1} px";
        _valueLabel.TextColor = AccentColor;
        var size = Math.Clamp(ResolvedPx, 8, 56);
        _preview.WidthRequest = size;
        _preview.HeightRequest = size;
        _preview.Color = AccentColor;
    }
}

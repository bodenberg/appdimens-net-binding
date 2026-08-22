using AppDimens.Net;
using AppDimens.Net.Core;

namespace AppDimens.Sample.Ui.Pages;

/// <summary>Live cache/metrics inspector — proves resize self-heals without manual invalidation.</summary>
public class CacheInfoPage : ContentPage
{
    private readonly Label _metrics;
    private readonly Label _stats;
    private IDispatcherTimer? _timer;
    private bool _diagEnabled;

    public CacheInfoPage()
    {
        Title = "Cache & metrics";
        BackgroundColor = M3.Background;
        var pad = new Thickness(16);
        var s = new VerticalStackLayout { Padding = pad, Spacing = 10 };

        _metrics = new Label { FontSize = 13, TextColor = M3.OnSurface, LineBreakMode = LineBreakMode.WordWrap };
        _stats = new Label { FontSize = 13, TextColor = M3.CardDescription, LineBreakMode = LineBreakMode.WordWrap };

        s.Add(Section("Current snapshot (auto-updates on resize/rotate)"));
        s.Add(Card(_metrics));
        s.Add(Section("Cache diagnostics"));
        s.Add(Card(_stats));

        var diag = new Switch { IsToggled = false, OnColor = M3.Primary };
        diag.Toggled += (_, e) => { _diagEnabled = e.Value; DimenCache.DiagnosticsEnabled = e.Value; };
        s.Add(new HorizontalStackLayout
        {
            Spacing = 12,
            Children =
            {
                new Label { Text = "Diagnostics", VerticalOptions = LayoutOptions.Center, FontSize = 14 },
                diag,
            },
        });

        s.Add(ActionBtn("InvalidateOnConfigChange (compat hook)",
            () => DimenCache.InvalidateOnConfigChange(AppDimensAmbient.Require().Configuration)));
        s.Add(ActionBtn("Clear cache", () => DimenCache.ClearAll()));

        Refresh();
        Content = new ScrollView { Content = s };
    }

    private static View Card(Label l) => new Border
    {
        StrokeThickness = 0,
        StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(16) },
        BackgroundColor = M3.CardBackground,
        Padding = new Thickness(14),
        Content = l,
    };

    private static Label Section(string t) => new()
    {
        Text = t,
        FontSize = 17,
        FontAttributes = FontAttributes.Bold,
        TextColor = M3.Tertiary,
        Margin = new Thickness(0, 8, 0, 2),
    };

    private static Button OutlinedBtn(string text, Action onClick) => new()
    {
        Text = text,
        FontSize = 14,
        TextColor = M3.Primary,
        BackgroundColor = Colors.Transparent,
        BorderColor = M3.OutlineVariant,
        BorderWidth = 1,
        CornerRadius = 12,
        Padding = new Thickness(14, 10),
    };

    private Button ActionBtn(string text, Action onClick)
    {
        var b = OutlinedBtn(text, onClick);
        b.Clicked += (_, _) => onClick();
        return b;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _timer ??= Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(400);
        if (_timer.IsRunning) return;
        _timer.Tick += OnTick;
        _timer.Start();
    }

    private void OnTick(object? sender, EventArgs e) => Refresh();

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (_timer is not null)
        {
            _timer.Tick -= OnTick;
            _timer.Stop();
        }
    }

    private void Refresh()
    {
        try
        {
            var ctx = AppDimensAmbient.Require();
            var m = DimenCache.FastMetricsForCode(ctx);
            _metrics.Text =
                $"w={m.ScreenWidthDp} h={m.ScreenHeightDp} sw={m.SmallestScreenWidthDp}\n" +
                $"density={m.Density:0.###} fontScale={m.FontScale:0.##} multiWindow={m.IsInMultiWindowMode}\n" +
                $"scale={m.Scale:0.#####} arNorm={m.NormalizedAspectRatio:0.####}\n" +
                $"arMultDefault={m.DefaultAspectRatioMultiplier:0.######} arMultScaled={m.DefaultScaledAspectRatioMultiplier:0.######}";
            _stats.Text = _diagEnabled
                ? $"hits={DimenCache.HitCount} misses={DimenCache.MissCount} evictions={DimenCache.EvictionCount}"
                : "diagnostics off (zero overhead)";
        }
        catch (Exception e) { _metrics.Text = e.Message; }
    }
}

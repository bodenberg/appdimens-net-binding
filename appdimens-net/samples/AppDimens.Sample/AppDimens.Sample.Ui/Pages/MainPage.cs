using AppDimens.Net;
using AppDimens.Net.Code.Resize;
using AppDimens.Net.Core;
using AppDimens.Net.Common;
using AppDimens.Sample.Ui.Pages;
using Microsoft.Maui.Controls.Shapes;

namespace AppDimens.Sample.Ui;

/// <summary>
/// Main demo screen mirroring the Compose Multiplatform demo of appdimens-kmp:
/// strategy selector routes every example, sections 1–5 (core extensions,
/// inverter shortcuts, facilitators, DimenScaled builder, auto-resize) plus
/// live window metrics and .NET-extra navigation.
/// </summary>
public class MainPage : ContentPage
{
    private DemoCalcStrategy _strategy = DemoCalcStrategy.Scaled;
    private bool _building;
    private VerticalStackLayout _root = null!;
    private Picker _strategyPicker = null!;
    private IDispatcherTimer? _metricTimer;
    private Label _metrics = null!;

    public MainPage()
    {
        Title = "AppDimens .NET";
        BackgroundColor = M3.Background;
        Build();
    }

    private void Build()
    {
        _building = true;
        var pad = M3.Pad(_strategy);
        _root = new VerticalStackLayout { Padding = pad, Spacing = pad.Left * 1.25 };

        // Header
        _root.Add(M3.Title("AppDimens .NET Demo"));
        _root.Add(M3.Paragraph(
            "Examples follow the calculation type below (default: Scaled). Same APIs as compose.scaled, routed per strategy."));
        _root.Add(M3.Paragraph(
            "Os exemplos usam o tipo de cálculo abaixo (padrão: Scaled). Mesmos padrões que compose.scaled, por estratégia.",
            small: true));

        // Live metrics chip (.NET extra)
        _metrics = new Label
        {
            FontSize = 12,
            TextColor = M3.CardDescription,
            HorizontalTextAlignment = TextAlignment.Center,
            LineBreakMode = LineBreakMode.WordWrap,
        };
        RefreshMetrics();
        _root.Add(new Border
        {
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) },
            BackgroundColor = M3.SurfaceVariant,
            Padding = new Thickness(12, 8),
            Content = _metrics,
        });

        // Strategy selector (KMP OutlinedButton + DropdownMenu)
        _strategyPicker = new Picker
        {
            ItemsSource = DemoCalc.All.Select(s => $"{s.LabelEn()} · {s.LabelPt()}").ToList(),
            SelectedIndex = (int)_strategy,
            FontSize = M3.LabelLarge,
            FontAttributes = FontAttributes.Bold,
            TextColor = M3.Primary,
            HorizontalTextAlignment = TextAlignment.Center,
        };
        _strategyPicker.SelectedIndexChanged += (_, _) =>
        {
            var idx = Math.Max(0, _strategyPicker.SelectedIndex);
            if (!_building && idx != (int)_strategy) Rebuild((DemoCalcStrategy)idx);
        };
        _root.Add(new Border
        {
            StrokeThickness = 1,
            Stroke = M3.OutlineVariant,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(12) },
            BackgroundColor = Colors.Transparent,
            Padding = new Thickness(4, 0),
            Content = _strategyPicker,
        });

        AppendSections();

        Content = new ScrollView { Content = _root };
        _building = false;
    }

    /// <summary>Rebuilds the whole demo when the strategy changes.</summary>
    private void Rebuild(DemoCalcStrategy s)
    {
        _strategy = s;
        Build();
    }

    private void AppendSections()
    {
        var pad = M3.Pad(_strategy);
        var ctx = AppDimensAmbient.Current;
        var s = _strategy;

        // ── 1. CORE EXTENSIONS ──────────────────────────────────────────
        _root.Add(M3.SectionTitle("1. Core Extensions", pad));
        _root.Add(M3.ExampleCard("Smallest width (strategy dp)",
            "60 × selected formula (e.g. 60.sdp in Scaled, 60.psdp in Percent).",
            () => M3.Scale(s, 60), Color.FromArgb("#42A5F5"), pad));
        _root.Add(M3.ExampleCard("Smallest width + aspect ratio",
            "60 × selected formula with AR (e.g. 60.sdpa in Scaled).",
            () => 60.Dp(s, Ctx(), ar: true), Color.FromArgb("#12D5B5"), pad));
        _root.Add(M3.ExampleCard(".hdp equivalent (Screen Height)",
            "80 × height qualifier for the selected strategy.",
            () => 80.HDp(s, Ctx()), Color.FromArgb("#EF5350"), pad));
        _root.Add(M3.ExampleCard(".wdp equivalent (Screen Width)",
            "100 × width qualifier for the selected strategy.",
            () => 100.WDp(s, Ctx()), Color.FromArgb("#66BB6A"), pad));

        // ── 2. INVERTER SHORTCUTS ───────────────────────────────────────
        _root.Add(M3.SectionTitle("2. Inverter Shortcuts", pad));
        _root.Add(M3.ExampleCard(".sdpPh equivalent (SW → Portrait Height)",
            "70 with SW→PH inverter for the selected strategy.",
            () => 70.Dp(s, Ctx(), inverter: Inverter.SwToPh), Color.FromArgb("#AB47BC"), pad));
        _root.Add(M3.ExampleCard(".sdpLw equivalent (SW → Landscape Width)",
            "70 with SW→LW inverter for the selected strategy.",
            () => 70.Dp(s, Ctx(), inverter: Inverter.SwToLw), Color.FromArgb("#7E57C2"), pad));
        _root.Add(M3.ExampleCard(".hdpLw equivalent (Height → Landscape Width)",
            "80 with height→landscape-width inverter.",
            () => 80.Dp(s, Ctx(), DpQualifier.Height, Inverter.PhToLw), Color.FromArgb("#5C6BC0"), pad));
        _root.Add(M3.ExampleCard(".wdpLh equivalent (Width → Landscape Height)",
            "90 with width→landscape-height inverter.",
            () => 90.Dp(s, Ctx(), DpQualifier.Width, Inverter.PwToLh), Color.FromArgb("#26A69A"), pad));

        // ── 3. FACILITATOR EXTENSIONS ───────────────────────────────────
        _root.Add(M3.SectionTitle("3. Facilitator Extensions", pad));
        var landscape = ctx?.Configuration.Orientation == ScreenConfiguration.OrientationLandscape;
        var isTv = ctx is not null && ctx.UiModeType == UiModeType.Television;
        var sw600 = (ctx?.Configuration.SmallestScreenWidthDp ?? 0) >= 600;

        _root.Add(M3.ExampleCard("sdpRotate (Rotation Override)",
            landscape ? "Rotation rule matches → 50 scaled." : "80 base, 50 when rotation rule matches (landscape).",
            () => landscape ? 50.Dp(s, Ctx()) : 80.Dp(s, Ctx()), Color.FromArgb("#FF7043"), pad));

        _root.Add(M3.ExampleCard("sdpRotate (Custom Qualifier)",
            "60 default, 40 when orientation rule (PORTRAIT/HEIGHT) matches.",
            () => !landscape ? 40.Dp(s, Ctx()) : 60.Dp(s, Ctx()), Color.FromArgb("#FF8A65"), pad));

        _root.Add(M3.ExampleCard("sdpMode (UiModeType Override)",
            $"80 default, 200 on TELEVISION{(isTv ? " — active" : "")}.",
            () => isTv ? 200.Dp(s, Ctx()) : 80.Dp(s, Ctx()), Color.FromArgb("#EC407A"), pad));

        _root.Add(M3.ExampleCard("sdpQualifier (Dp Qualifier Override)",
            $"60 default, 120 when sw ≥ 600{(sw600 ? " — active" : "")}.",
            () => sw600 ? 120.Dp(s, Ctx()) : 60.Dp(s, Ctx()), Color.FromArgb("#26C6DA"), pad));

        _root.Add(M3.ExampleCard("sdpScreen (Combined Override)",
            $"70 default, 150 on TV with sw ≥ 600{(isTv && sw600 ? " — active" : "")}.",
            () => isTv && sw600 ? 150.Dp(s, Ctx()) : 70.Dp(s, Ctx()), Color.FromArgb("#78909C"), pad));

        _root.Add(M3.ExampleCard("sdpRotatePlain (Dp + Dp, no re-scale)",
            "80.demoSwDp when not landscape, 50.demoSwDp in landscape — both sides already scaled.",
            () => M3.Scale(s, landscape ? 50 : 80), Color.FromArgb("#FFAB91"), pad));

        _root.Add(M3.ExampleCard("sdpRotatePlain + sdpModePlain (nested)",
            "Chain: rotate plain then mode plain (TELEVISION → 28.demoSwDp).",
            () => M3.Scale(s, isTv ? 28 : landscape ? 48 : 72), Color.FromArgb("#FFCC80"), pad));

        _root.Add(M3.ExampleCard("sdpQualifierPlain (Dp threshold branch)",
            "60.demoSwDp unless sw ≥ 600 → 100.demoSwDp (plain alternativo).",
            () => M3.Scale(s, sw600 ? 100 : 60), Color.FromArgb("#80DEEA"), pad));

        _root.Add(M3.ExampleCard("sdpScreenPlain (Dp + Dp)",
            "65.demoSwDp unless TV + sw ≥ 600 → 90.demoSwDp.",
            () => M3.Scale(s, isTv && sw600 ? 90 : 65), Color.FromArgb("#B0BEC5"), pad));

        // Plain sp card (#E3F2FD)
        var spPlain = 16f;
        try
        {
            if (ctx is not null)
            {
                var density = Math.Max(ctx.Density, 0.01f);
                var px = DemoCalc.DpF(landscape ? 11f : 16f, s, ctx) * density;
                spPlain = px / (density * Math.Max(ctx.Configuration.FontScale, 0.01f));
            }
        }
        catch { /* keep fallback */ }
        _root.Add(M3.TintedCard(M3.SpPlainCard, pad,
            M3.CardTitle("sspRotatePlain (Sp + Sp)"),
            M3.CardBody(
                "EN: 16.demoSsp.demoSspRotatePlain(11.demoSsp) — no second scaling on receiver or alternate.\nPT: Ambos os lados já vêm da estratégia escolhida; só a condição de orientação.",
                M3.SpPlainText),
            new Label
            {
                Text = $"Sample · Plain Sp rotation branch — {spPlain:0.#}sp",
                FontSize = Math.Max(10, spPlain),
                TextColor = M3.OnSurface,
            }));

        // ── 4. DIMENSCALED BUILDER ──────────────────────────────────────
        _root.Add(M3.SectionTitle("4. DimenScaled Builder", pad));
        var builderDp = DemoCalc.BuilderResultDp(s, Ctx());
        _root.Add(M3.TintedCard(M3.BuilderCard, pad,
            M3.CardTitle("DimenScaled-style builder (per strategy)"),
            M3.CardBody(
                "100 × strategy builder + .screen(...) chain + .sdp / .psdp / .asdp / …\n" +
                "  .screen(TV + sw>=600 → 250)\n  .screen(TV → 500)\n  .screen(FOLD_OPEN → 200)\n" +
                "  .screen(sw>=600 → 150)\n  .screen(LANDSCAPE → 120)\n" +
                $"Current: {(int)Math.Round(builderDp)}dp"),
            M3.DimBox(builderDp, M3.BuilderBox, "dp")));

        // ── 5. AUTO-RESIZE ──────────────────────────────────────────────
        _root.Add(M3.SectionTitle("5. Auto-resize (DimenResize)", pad));
        _root.Add(AutoResizeCard(pad));

        // ── .NET EXTRAS ─────────────────────────────────────────────────
        _root.Add(M3.SectionTitle(".NET extras", pad));
        NavRow("Scaled details (suffixes a/i/ia)", () => new ScaledDemoPage());
        NavRow("All strategies table", () => new StrategiesPage());
        NavRow("Interactive resize & physical units", () => new ResizeDemoPage());
        NavRow("Builders (ScaledDimension / Fit)", () => new BuildersPage());
        NavRow("Cache & live metrics", () => new CacheInfoPage());
    }

    private View AutoResizeCard(Thickness pad)
    {
        const string sample = "This sentence scales between min and max sp so it fits the box (try rotation).";

        var textLabel = new Label
        {
            Text = sample,
            FontSize = 14,
            TextColor = M3.OnSurface,
            LineBreakMode = LineBreakMode.WordWrap,
            MaxLines = 3,
        };
        var textBox = new Border
        {
            StrokeThickness = 1,
            Stroke = M3.ResizeBorder1,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(8) },
            BackgroundColor = Colors.Transparent,
            Padding = new Thickness(8),
            MinimumHeightRequest = 56,
            Content = textLabel,
        };

        var squareCaption = new Label
        {
            TextColor = Colors.White,
            FontSize = M3.BodySmall,
            FontAttributes = FontAttributes.Bold,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center,
            InputTransparent = true,
        };
        var square = new Border
        {
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(8) },
            BackgroundColor = M3.ResizeSquare,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Content = squareCaption,
        };
        var squareHost = new Grid { HeightRequest = 120, Children = { square } };

        var barWBox = new BoxView
        {
            Color = M3.ResizeBarW,
            HeightRequest = 36,
            WidthRequest = 40,
            CornerRadius = new CornerRadius(6),
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
        };
        var barHBox = new BoxView
        {
            Color = M3.ResizeBarH,
            WidthRequest = 40,
            HeightRequest = 24,
            CornerRadius = new CornerRadius(6),
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
        };
        var barWHost = new Grid { HeightRequest = 72, Children = { barWBox } };
        var barHHost = new Grid { HeightRequest = 72, Children = { barHBox } };
        Grid.SetColumn(barWHost, 0);
        Grid.SetColumn(barHHost, 1);

        void Refit()
        {
            try
            {
                var ctx = Ctx();
                var density = Math.Max(ctx.Density, 0.01f);
                var fontDiv = density * Math.Max(ctx.Configuration.FontScale, 0.01f);

                if (textBox.Width > 0)
                {
                    var innerPx = Math.Max((float)textBox.Width * density - 16f * density, density);
                    var spPx = DimenResize.AutoResizeTextPx(sample, innerPx, 10, 22, 1, maxLines: 3, ctx);
                    textLabel.FontSize = Math.Clamp(spPx / fontDiv, 10f, 22f);
                }
                if (squareHost.Width > 0)
                {
                    var sideUnits = DimenResize.AutoResizeSquarePx((float)squareHost.Width, (float)squareHost.Height, 16, 100, 4, ctx);
                    var sidePx = sideUnits * density;
                    square.WidthRequest = sidePx;
                    square.HeightRequest = sidePx;
                    squareCaption.Text = $"{(int)Math.Round(sideUnits)}dp";
                }
                if (barWHost.Width > 0)
                {
                    var wLimitUnits = Math.Max((float)barWHost.Width * density - 8f * density, 20f);
                    var w = DimenResize.AutoResizeSquarePx(wLimitUnits, 36f, 20, Math.Min(200f, wLimitUnits), 4, ctx);
                    barWBox.WidthRequest = Math.Clamp(w, 20f, Math.Max(20f, wLimitUnits / density));
                }
                var h = DimenResize.AutoResizeSquarePx(40f, 64f, 16, 56, 4, ctx);
                barHBox.HeightRequest = Math.Clamp(h, 16f, 64f);
            }
            catch { /* layout not ready */ }
        }

        textBox.SizeChanged += (_, _) => Refit();
        squareHost.SizeChanged += (_, _) => Refit();
        barWHost.SizeChanged += (_, _) => Refit();

        return M3.TintedCard(M3.ResizeCard, pad,
            M3.CardTitle("Auto-resize — fits content to the box"),
            M3.CardBody(
                "EN: APIs: AutoResizeTextPx, AutoResizeSquarePx (AppDimens.Net.Code.Resize).\nPT: APIs equivalentes do módulo resize; tamanhos se ajustam à caixa automaticamente.",
                M3.ResizeText),
            M3.CardBody("Text (autoResizeTextSp)", M3.OnSurface),
            textBox,
            M3.CardBody("Square (autoResizeSquareSize)", M3.OnSurface),
            squareHost,
            M3.CardBody("Width / height (autoResizeWidthSize · autoResizeHeightSize)", M3.OnSurface),
            new Grid
            {
                ColumnDefinitions =
                [
                    new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
                    new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
                ],
                ColumnSpacing = 12,
                Children = { barWHost, barHHost },
            });
    }

    private static IAppDimensContext Ctx() => AppDimensAmbient.Require();

    private void NavRow(string text, Func<Page> factory)
    {
        var page = default(Page);
        _root.Add(M3.Outlined(text, () =>
        {
            page ??= factory();
            Navigation.PushAsync(page);
        }));
    }

    private void RefreshMetrics()
    {
        try
        {
            var ctx = AppDimensAmbient.Require();
            var m = DimenCache.FastMetricsForCode(ctx);
            _metrics.Text =
                $"window: {m.ScreenWidthDp}x{m.ScreenHeightDp}dp · sw={m.SmallestScreenWidthDp} · dpi={m.DensityDpi} · scale={m.Scale:0.###}";
        }
        catch (Exception e) { _metrics.Text = e.Message; }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _metricTimer ??= Dispatcher.CreateTimer();
        _metricTimer.Interval = TimeSpan.FromMilliseconds(500);
        if (_metricTimer.IsRunning) return;
        _metricTimer.Tick += OnMetricTick;
        _metricTimer.Start();
    }

    private void OnMetricTick(object? sender, EventArgs e) => RefreshMetrics();

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (_metricTimer is not null)
        {
            _metricTimer.Tick -= OnMetricTick;
            _metricTimer.Stop();
        }
    }
}

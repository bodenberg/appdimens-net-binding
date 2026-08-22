using AppDimens.BenchLab.Core;
using AppDimens.Net;
using AppDimens.Net.Core;

namespace AppDimens.BenchLab.Ui;

/// <summary>
/// Benchmark dashboard mirroring the KMP BenchlabScreen: dark theme
/// (#0D0F14 bg, #161B24 cards, cyan/green/amber accents), phase indicator,
/// environment card, monospace result rows and report export. Runs chunked
/// with a reentrancy guard so the UI never blocks.
/// </summary>
public class BenchMainPage : ContentPage
{
    // KMP Benchlab colour palette.
    private static readonly Color DarkBg = Color.FromArgb("#0D0F14");
    private static readonly Color SurfaceCard = Color.FromArgb("#161B24");
    private static readonly Color SurfaceBorder = Color.FromArgb("#252D3D");
    private static readonly Color AccentCyan = Color.FromArgb("#00E5FF");
    private static readonly Color AccentGreen = Color.FromArgb("#69FF47");
    private static readonly Color AccentAmber = Color.FromArgb("#FFD740");
    private static readonly Color AccentRed = Color.FromArgb("#FF5252");
    private static readonly Color TextPrimary = Color.FromArgb("#ECF0F8");
    private static readonly Color TextSecondary = Color.FromArgb("#8A95A8");

    private const string Mono = "Monospace";

    private readonly Label _phase = new()
    {
        Text = "PHASE: IDLE",
        FontFamily = Mono,
        FontSize = 13,
        TextColor = AccentAmber,
    };
    private readonly Label _env = new()
    {
        FontFamily = Mono,
        FontSize = 12,
        TextColor = TextSecondary,
        LineBreakMode = LineBreakMode.WordWrap,
    };
    private readonly VerticalStackLayout _rowsLayout = new() { Spacing = 4 };
    private readonly Button _runButton;
    private string _reportText = "Ready.";

    public BenchMainPage()
    {
        Title = "BenchLab";
        BackgroundColor = DarkBg;
        RefreshEnv();

        _runButton = new Button
        {
            Text = "Run benchmark",
            FontAttributes = FontAttributes.Bold,
            FontSize = 15,
            TextColor = DarkBg,
            BackgroundColor = AccentCyan,
            CornerRadius = 10,
            Padding = new Thickness(14, 10),
        };
        _runButton.Clicked += async (_, _) => await RunAsync();

        var copy = new Button
        {
            Text = "Copy report",
            FontSize = 14,
            TextColor = AccentCyan,
            BackgroundColor = Colors.Transparent,
            BorderColor = SurfaceBorder,
            BorderWidth = 1,
            CornerRadius = 10,
            Padding = new Thickness(14, 10),
        };
        copy.Clicked += async (_, _) =>
        {
            try { await Clipboard.SetTextAsync(_reportText); _phase.Text = "PHASE: COPIED"; }
            catch { /* clipboard unavailable */ }
        };

        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = new Thickness(16),
                Spacing = 12,
                Children =
                {
                    new Label
                    {
                        Text = "AppDimens BenchLab — .NET",
                        FontSize = 22,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = AccentCyan,
                    },
                    new Label
                    {
                        Text = "Fast lanes vs raw multiply vs cached/uncached path vs legacy XML grid.",
                        FontSize = 12,
                        TextColor = TextSecondary,
                        LineBreakMode = LineBreakMode.WordWrap,
                    },
                    Card(_env),
                    new HorizontalStackLayout { Spacing = 10, Children = { _runButton, copy } },
                    _phase,
                    Card(_rowsLayout),
                },
            },
        };
    }

    private View Card(View content) => new Border
    {
        StrokeThickness = 1,
        Stroke = SurfaceBorder,
        StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(12) },
        BackgroundColor = SurfaceCard,
        Padding = new Thickness(12),
        Content = content,
    };

    private void RefreshEnv()
    {
        try
        {
            var ctx = AppDimens.Net.AppDimensAmbient.Require();
            var m = DimenCache.FastMetricsForCode(ctx);
            _env.Text =
                $"device=.NET MAUI\nwindow={m.ScreenWidthDp}x{m.ScreenHeightDp}dp dpi={m.DensityDpi}\n" +
                $"scale={m.Scale:0.####} density={m.Density:0.###}\n" +
                $"workload: warmup=10k measured=200k × 8 rows";
        }
        catch (Exception e)
        {
            _env.Text = "ambient not ready: " + e.Message;
        }
    }

    private void SetPhase(string text, Color color)
    {
        _phase.Text = $"PHASE: {text}";
        _phase.TextColor = color;
    }

    private void AddRow(string name, string stats, bool accent)
    {
        _rowsLayout.Add(new Label
        {
            Text = $"{name}\n   {stats}",
            FontFamily = Mono,
            FontSize = 11,
            TextColor = accent ? AccentCyan : TextPrimary,
            LineBreakMode = LineBreakMode.WordWrap,
        });
    }

    private async Task RunAsync()
    {
        if (BenchmarkRunner.IsRunning) return; // reentrancy guard (KMP parity)

        _runButton.IsEnabled = false;
        SetPhase("WARMUP", AccentAmber);
        _rowsLayout.Clear();
        RefreshEnv();

        try
        {
            var report = await BenchmarkRunner.RunAsync(
                onRowCompleted: row => MainThread.BeginInvokeOnMainThread(() =>
                {
                    SetPhase("RUNNING", AccentAmber);
                    AddRow(row.Name,
                        $"{row.NsPerOp,8:0.0} ns/op  {row.OpsPerSec,12:N0} ops/s  alloc={row.AllocatedBytes}B",
                        accent: row.Name.Contains("fast lane") || row.Name.Contains("cached path"));
                }));

            _reportText = report.ToText();
            MainThread.BeginInvokeOnMainThread(() => SetPhase($"DONE · total {report.TotalMs:0} ms", AccentGreen));
        }
        catch (Exception e)
        {
            SetPhase("FAILED", AccentRed);
            AddRow("benchmark failed", e.Message, accent: false);
            _reportText = "FAILED: " + e;
        }
        finally
        {
            _runButton.IsEnabled = true;
        }
    }
}

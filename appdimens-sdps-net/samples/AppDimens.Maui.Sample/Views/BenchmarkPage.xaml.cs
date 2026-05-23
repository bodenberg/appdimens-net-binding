using AppDimens.Maui.Extensions;
using AppDimens.Maui.Sample.Services;

namespace AppDimens.Maui.Sample.Views;

public partial class BenchmarkPage : ContentPage
{
    private IReadOnlyList<SampleBenchmarkResult>? _lastResults;

    public BenchmarkPage()
    {
        InitializeComponent();
        DimensSampleRefresh.WhenMetricsChange(this, OnMetricsChanged);
    }

    private void OnMetricsChanged()
    {
        if (_lastResults is null)
            return;

        _lastResults = SampleBenchmarkRunner.WithFreshDisplayValues(_lastResults);
        RenderResults(_lastResults);
    }

    private async void OnRunClicked(object? sender, EventArgs e)
    {
        RunButton.IsEnabled = false;
        Spinner.IsVisible = true;
        Spinner.IsRunning = true;
        StatusLabel.Text = "Executando…";
        ResultsHost.Children.Clear();
        _lastResults = null;

        var results = await Task.Run(SampleBenchmarkRunner.RunAll);
        _lastResults = results;
        RenderResults(results);

        StatusLabel.Text = $"Concluído em {DateTime.Now:HH:mm:ss}";
        Spinner.IsRunning = false;
        Spinner.IsVisible = false;
        RunButton.IsEnabled = true;
    }

    private void RenderResults(IReadOnlyList<SampleBenchmarkResult> results)
    {
        var appResources = Application.Current!.Resources;
        ResultsHost.Children.Clear();

        foreach (var r in results)
        {
            var ns = r.NanosecondsPerOp;
            var display = ns < 1000 ? $"{ns:F0} ns/op" : $"{ns / 1000:F2} µs/op";

            var info = new VerticalStackLayout
            {
                Spacing = 2.Sdp(),
                Children =
                {
                    new Label { Text = r.Name, FontAttributes = FontAttributes.Bold, FontSize = 14.Ssp(), TextColor = (Color)appResources["TextPrimary"] },
                    new Label
                    {
                        Text = $"último valor: {r.ResultValue:F2} · {r.Iterations:N0} ops",
                        Style = (Style)appResources["Caption"],
                    },
                },
            };
            var timing = new Label
            {
                Text = display,
                FontAttributes = FontAttributes.Bold,
                TextColor = (Color)appResources["Accent"],
                VerticalOptions = LayoutOptions.Center,
            };
            var grid = new Grid
            {
                ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
            };
            grid.Add(info, 0, 0);
            grid.Add(timing, 1, 0);

            ResultsHost.Children.Add(new Border
            {
                Style = (Style)appResources["Card"],
                Content = grid,
            });
        }
    }
}

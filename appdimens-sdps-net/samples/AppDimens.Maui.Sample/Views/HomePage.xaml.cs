using AppDimens.Maui;
using AppDimens.Maui.Extensions;
using AppDimens.Maui.Sample.Services;

namespace AppDimens.Maui.Sample.Views;

public partial class HomePage : ContentPage
{
    public HomePage()
    {
        InitializeComponent();
        DimensSampleRefresh.WhenMetricsChange(this, RefreshMetrics);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        RefreshMetrics();
    }

    private void RefreshMetrics()
    {
        MetricsLabel.Text = ScreenMetricsFormatter.FormatCurrent();
        LiveSdpLabel.Text = 16.Sdp().ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
        FrozenSdpiLabel.Text = 16.Sdpi().ToString("F2", System.Globalization.CultureInfo.InvariantCulture);

        var m = AppDimensResolver.Instance.Metrics.Current;
        SampleLog.Info($"metrics sw={m.SmallestDp:0} w={m.WidthDp:0} h={m.HeightDp:0} " +
                       $"sdp16={LiveSdpLabel.Text} sdpi16={FrozenSdpiLabel.Text}");
    }

    private void OnRefreshClicked(object? sender, EventArgs e)
    {
        AppDimensResolver.Instance.RefreshMetricsFromDevice();
        RefreshMetrics();
    }

    private static async Task GoToAsync(string route) =>
        await Shell.Current.GoToAsync($"//{route}");

    private void OnDimensTapped(object? sender, TappedEventArgs e) => _ = GoToAsync("dimens");
    private void OnInvertersTapped(object? sender, TappedEventArgs e) => _ = GoToAsync("inverters");
    private void OnAdvancedTapped(object? sender, TappedEventArgs e) => _ = GoToAsync("advanced");
    private void OnBenchmarkTapped(object? sender, TappedEventArgs e) => _ = GoToAsync("benchmark");
}

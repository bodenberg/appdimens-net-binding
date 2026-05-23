using AppDimens.Maui;
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

    private void RefreshMetrics() => MetricsLabel.Text = ScreenMetricsFormatter.FormatCurrent();

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

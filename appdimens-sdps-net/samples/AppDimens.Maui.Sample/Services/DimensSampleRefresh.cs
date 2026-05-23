using AppDimens.Maui;

namespace AppDimens.Maui.Sample.Services;

/// <summary>
/// Invokes a refresh callback when screen metrics change while the page is visible.
/// </summary>
/// <remarks>
/// On Android, call <see cref="AppDimensResolver.RefreshMetricsFromDevice"/> from
/// <c>OnConfigurationChanged</c> when rotation does not raise <c>MainDisplayInfoChanged</c>.
/// </remarks>
public static class DimensSampleRefresh
{
    public static void WhenMetricsChange(ContentPage page, Action refresh)
    {
        EventHandler? handler = null;
        handler = (_, _) => MainThread.BeginInvokeOnMainThread(refresh);

        page.Appearing += (_, _) =>
        {
            AppDimensResolver.Instance.Metrics.Changed += handler;
            refresh();
        };

        page.Disappearing += (_, _) =>
            AppDimensResolver.Instance.Metrics.Changed -= handler;
    }
}

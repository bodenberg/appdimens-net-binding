using AppDimens.Maui.Core;
using Microsoft.Maui.Hosting;

namespace AppDimens.Maui;

public static class AppDimensSdpsExtensions
{
    public static MauiAppBuilder UseAppDimensSdps(this MauiAppBuilder builder, Action<AppDimensOptions>? configure = null)
    {
        var options = new AppDimensOptions();
        configure?.Invoke(options);
        AppDimensSdps.Initialize(options);
        return builder;
    }
}

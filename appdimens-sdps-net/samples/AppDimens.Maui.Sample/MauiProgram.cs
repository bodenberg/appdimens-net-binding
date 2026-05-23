using AppDimens.Maui;
using AppDimens.Maui.Core;

namespace AppDimens.Maui.Sample;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        var generated = Path.Combine(AppContext.BaseDirectory, "Generated");
        builder.UseMauiApp<App>();

        // Generated bucket tables copied to output by the sample project.
        AppDimensSdps.Initialize(
            new AppDimensOptions
            {
                ScalingMode = ScalingMode.Hybrid,
                WarmupAspectRatio = true,
                DefaultFontScale = 1.0f,
            },
            Directory.Exists(generated) ? generated : null);

        return builder.Build();
    }
}

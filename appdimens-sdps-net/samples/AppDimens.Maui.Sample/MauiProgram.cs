using AppDimens.Maui;
using AppDimens.Maui.Core;
using AppDimens.Maui.Sample.Services;

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

        AppDimensLogging.Current = new SampleLogger();

        return builder.Build();
    }

    private sealed class SampleLogger : IAppDimensLogger
    {
        public void LogDebug(string message) => SampleLog.Info(message);
        public void LogWarning(string message) => SampleLog.Info("WARN " + message);
    }
}

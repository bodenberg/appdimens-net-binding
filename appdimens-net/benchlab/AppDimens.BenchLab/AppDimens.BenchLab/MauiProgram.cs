using AppDimens.BenchLab.Ui;
using AppDimens.Net.Maui.Platform;

namespace AppDimens.BenchLab;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();
        return builder.Build();
    }
}

public class App : Application
{
    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window();
        AppDimensMaui.AttachWindow(window);
        window.Page = new NavigationPage(new BenchMainPage());
        return window;
    }
}

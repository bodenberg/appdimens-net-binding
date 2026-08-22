using AppDimens.Net.Maui.Platform;
using AppDimens.Sample.Ui;
using AppDimens.Sample.Ui.Pages;

namespace AppDimens.Sample;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();
        return builder.Build();
    }
}

/// <summary>Attaches the AppDimens live window scope at startup.</summary>
public class App : Application
{
    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window();
        AppDimensMaui.AttachWindow(window);
        window.Page = new NavigationPage(new MainPage());
        return window;
    }
}

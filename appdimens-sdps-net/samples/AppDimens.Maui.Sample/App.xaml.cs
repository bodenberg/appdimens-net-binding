using AppDimens.Maui;

namespace AppDimens.Maui.Sample;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        UserAppTheme = AppTheme.Dark;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new AppShell());
        // Live values now follow this window's size (desktop/split-screen/foldables);
        // *i values stay frozen against the baseline captured at startup.
        AppDimensSdpsWindow.Attach(window);
        return window;
    }
}

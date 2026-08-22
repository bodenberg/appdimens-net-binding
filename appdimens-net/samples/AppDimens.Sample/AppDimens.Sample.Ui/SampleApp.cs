using AppDimens.Sample.Ui.Pages;

namespace AppDimens.Sample.Ui;

/// <summary>Sample Application — pure C# (no XAML), shared by every platform head.</summary>
public class SampleApp : Application
{
    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = base.CreateWindow(activationState);
        window.Page = new NavigationPage(new MainPage());
        return window;
    }
}

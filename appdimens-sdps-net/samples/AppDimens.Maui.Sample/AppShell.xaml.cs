namespace AppDimens.Maui.Sample;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Navigated += OnShellNavigated;
    }

    private void OnShellNavigated(object? sender, ShellNavigatedEventArgs e)
    {
        if (CurrentPage is not ContentPage page)
            return;

        var onHome = e.Current?.Location?.OriginalString?.EndsWith("/home", StringComparison.Ordinal) == true
                     || e.Current?.Location?.OriginalString == "//home";

        Shell.SetBackButtonBehavior(page, onHome
            ? new BackButtonBehavior { IsVisible = false }
            : new BackButtonBehavior
            {
                IsVisible = true,
                TextOverride = "Voltar",
                Command = new Command(async () => await GoToAsync("//home")),
            });
    }
}

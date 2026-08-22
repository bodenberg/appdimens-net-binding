using Android.App;
using Android.Content.PM;

namespace AppDimens.Sample.Platforms.Android;

[Activity(Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation |
        ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize |
        ConfigChanges.Density | ConfigChanges.KeyboardHidden)]
public class MainActivity : MauiAppCompatActivity
{
}

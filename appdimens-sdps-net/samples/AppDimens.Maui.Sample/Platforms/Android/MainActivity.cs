using Android.App;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using AppDimens.Maui;

namespace AppDimens.Maui.Sample;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode |
                           ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    public override void OnConfigurationChanged(Configuration newConfig)
    {
        base.OnConfigurationChanged(newConfig);
        AppDimensResolver.Instance.RefreshMetricsFromDevice();
    }
}

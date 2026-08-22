using Foundation;
using UIKit;

namespace AppDimens.BenchLab.Platforms.IOS;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}

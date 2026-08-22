namespace AppDimens.Maui.Sample.Services;

/// <summary>Sample-side diagnostics sink (logcat on Android, Debug elsewhere).</summary>
public static class SampleLog
{
    public static void Info(string message)
    {
#if ANDROID
        Android.Util.Log.Info("AppDimensSample", message);
#else
        System.Diagnostics.Debug.WriteLine($"[AppDimensSample] {message}");
#endif
    }
}

using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Com.Appdimens.Ssps;
using Com.Appdimens.Ssps.Code;

namespace AppDimens.Ssps.SmokeTest;

[Activity(
    Label = "@string/app_name",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation
        | ConfigChanges.ScreenSize
        | ConfigChanges.ScreenLayout
        | ConfigChanges.SmallestScreenSize
        | ConfigChanges.UiMode
        | ConfigChanges.Density)]
public class MainActivity : Activity
{
    TextView? _result;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        SetContentView(Resource.Layout.activity_main);
        _result = FindViewById<TextView>(Resource.Id.smoke_result);

        SspsConfig.Warmup(this);
        SspsConfig.DimensionsShouldRefresh += OnDimensionsShouldRefresh;
        ApplyDimens("OnCreate");
    }

    protected override void OnDestroy()
    {
        SspsConfig.DimensionsShouldRefresh -= OnDimensionsShouldRefresh;
        base.OnDestroy();
    }

    void OnDimensionsShouldRefresh(object? sender, Configuration? e)
    {
        Android.Util.Log.Info("SmokeTest", "SspsConfig.DimensionsShouldRefresh fired");
    }

    public override void OnConfigurationChanged(Configuration newConfig)
    {
        base.OnConfigurationChanged(newConfig);
        SspsConfig.OnConfigurationChanged(this, newConfig);
        ApplyDimens("OnConfigurationChanged");
    }

    void ApplyDimens(string reason)
    {
        float sspPx = DimenSsp.Ssp(this, 100);
        float sspaPx = DimenSsp.Sspa(this, 100);
        float rotatePx = DimenSsp.SspRotate(this, 16, 24);
        float modePx = DimenSsp.SspMode(this, 16, 28, Com.Appdimens.Ssps.Common.UiModeType.Normal);
        // Novo: sobrecarga com FoldingFeature (dobráveis) — null = sem dobra
        float modeFoldPx = DimenSsp.SspMode(this, 16, 28, Com.Appdimens.Ssps.Common.UiModeType.Normal, null);

        Android.Util.Log.Info(
            "SmokeTest",
            "{0}: DimenSsp.Ssp(100)={1}px Sspa(100)={2}px Rotate={3}px Mode={4}px ModeFold={5}px",
            reason, sspPx, sspaPx, rotatePx, modePx, modeFoldPx);

        if (_result != null)
        {
            _result.Text =
                $"{GetString(Resource.String.smoke_hint)}\n" +
                $"{reason}\n" +
                $"ssp(100) = {sspPx}px\n" +
                $"sspa(100) = {sspaPx}px (aspect-ratio)\n" +
                $"sspRotate(16/24) = {rotatePx}px\n" +
                $"sspMode(16/28) = {modePx}px\n" +
                $"(binding OK · {reason})";
        }
    }
}

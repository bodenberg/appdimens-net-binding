using Android.Content.PM;
using Android.Content.Res;
using Com.Appdimens.Sdps;
using Com.Appdimens.Sdps.Code;

namespace AppDimens.Sdps.SmokeTest;

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

        SdpsConfig.Warmup(this);
        SdpsConfig.DimensionsShouldRefresh += OnDimensionsShouldRefresh;
        ApplyDimens("OnCreate");
    }

    protected override void OnDestroy()
    {
        SdpsConfig.DimensionsShouldRefresh -= OnDimensionsShouldRefresh;
        base.OnDestroy();
    }

    public override void OnConfigurationChanged(Configuration newConfig)
    {
        base.OnConfigurationChanged(newConfig);
        // Ensure AR factors + notify subscribers (this Activity re-applies below).
        SdpsConfig.OnConfigurationChanged(this, newConfig);
        ApplyDimens("OnConfigurationChanged");
    }

    void OnDimensionsShouldRefresh(object? sender, Configuration? e)
    {
        Android.Util.Log.Info("SmokeTest", "SdpsConfig.DimensionsShouldRefresh fired");
    }

    void ApplyDimens(string reason)
    {
        float sdpPx = DimenSdp.Sdp(this, 100);
        float sdpaPx = DimenSdp.Sdpa(this, 100);
        float rotatePx = DimenSdp.SdpRotate(this, 16, 24);
        float modePx = DimenSdp.SdpMode(this, 16, 28, UiModeType.Normal!);

        Android.Util.Log.Info(
            "SmokeTest",
            "{0}: Sdp(100)={1}px Sdpa(100)={2}px Rotate={3}px Mode={4}px",
            reason, sdpPx, sdpaPx, rotatePx, modePx);

        if (_result != null)
        {
            _result.Text =
                $"{GetString(Resource.String.smoke_hint)}\n" +
                $"{reason}\n" +
                $"sdp(100) = {sdpPx}px\n" +
                $"sdpa(100) = {sdpaPx}px\n" +
                $"sdpRotate(16/24) = {rotatePx}px\n" +
                $"(binding OK · {reason})";
        }
    }
}

using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
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

    void OnDimensionsShouldRefresh(object? sender, Configuration? e)
    {
        Android.Util.Log.Info("SmokeTest", "SdpsConfig.DimensionsShouldRefresh fired");
    }

    public override void OnConfigurationChanged(Configuration newConfig)
    {
        base.OnConfigurationChanged(newConfig);
        SdpsConfig.OnConfigurationChanged(this, newConfig);
        ApplyDimens("OnConfigurationChanged");
    }

    void ApplyDimens(string reason)
    {
        float sdpPx = DimenSdp.Sdp(this, 100);
        float sdpPxA = DimenSdp.Sdpa(this, 100);
        float rotatePx = DimenSdp.SdpRotate(this, 16, 24);
        float modePx = DimenSdp.SdpMode(this, 16, 28, Com.Appdimens.Sdps.Common.UiModeType.Normal);
        // Novo: sobrecarga com FoldingFeature (dobráveis) — null = sem dobra
        float modeFoldPx = DimenSdp.SdpMode(this, 16, 28, Com.Appdimens.Sdps.Common.UiModeType.Normal, null);

        Android.Util.Log.Info(
            "SmokeTest",
            "{0}: DimenSdp.Sdp(100)={1}px Sdpa(100)={2}px Rotate={3}px Mode={4}px ModeFold={5}px",
            reason, sdpPx, sdpPxA, rotatePx, modePx, modeFoldPx);

        if (_result != null)
        {
            _result.Text =
                $"{GetString(Resource.String.smoke_hint)}\n" +
                $"{reason}\n" +
                $"sdp(100) = {sdpPx}px\n" +
                $"sdpa(100) = {sdpPxA}px (aspect-ratio)\n" +
                $"sdpRotate(16/24) = {rotatePx}px\n" +
                $"sdpMode(16/28) = {modePx}px\n" +
                $"(binding OK · {reason})";
        }
    }
}

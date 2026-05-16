namespace AppDimens.Sdps.SmokeTest;

[Activity(Label = "@string/app_name", MainLauncher = true)]
public class MainActivity : Activity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        SetContentView(Resource.Layout.activity_main);

        // Referência ao binding (API Kotlin/JVM exposta como Com.Appdimens.Sdps.Code)
        global::Com.Appdimens.Sdps.Code.DimenSdp.WarmupSdpsFactors(this);
        float sdpPx = global::Com.Appdimens.Sdps.Code.DimenSdp.Sdp(this, 100);

        Android.Util.Log.Info("SmokeTest", "DimenSdp.Sdp(Context, 100) = {0} px", sdpPx);

        var tv = FindViewById<TextView>(Resource.Id.smoke_result);
        tv!.Text =
            $"{GetString(Resource.String.smoke_hint)}\n sdp(100) = {sdpPx}px\n(binding OK)";
    }
}

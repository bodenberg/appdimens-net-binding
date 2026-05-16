namespace AppDimens.Ssps.SmokeTest;

[Activity(Label = "@string/app_name", MainLauncher = true)]
public class MainActivity : Activity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        SetContentView(Resource.Layout.activity_main);

        global::Com.Appdimens.Ssps.Code.DimenSsp.WarmupSspsFactors(this);
        float sspPx = global::Com.Appdimens.Ssps.Code.DimenSsp.Ssp(this, 100);

        Android.Util.Log.Info("SmokeTest", "DimenSsp.Ssp(Context, 100) = {0} px", sspPx);

        var tv = FindViewById<TextView>(Resource.Id.smoke_result);
        tv!.Text =
            $"{GetString(Resource.String.smoke_hint)}\n ssp(100) = {sspPx}px\n(binding OK)";
    }
}

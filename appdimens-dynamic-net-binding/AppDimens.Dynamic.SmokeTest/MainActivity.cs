using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Com.Appdimens.Dynamic;
using Com.Appdimens.Dynamic.Code;
using Com.Appdimens.Dynamic.Common;

namespace AppDimens.Dynamic.SmokeTest;

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

        FindViewById<Button>(Resource.Id.btn_benchmark)!.Click += (_, _) =>
            StartActivity(new Intent(this, typeof(BenchmarkActivity)));

        DynamicConfig.Warmup(this);
        DynamicConfig.DimensionsShouldRefresh += OnDimensionsShouldRefresh;
        ApplyDimens("OnCreate");
    }

    protected override void OnDestroy()
    {
        DynamicConfig.DimensionsShouldRefresh -= OnDimensionsShouldRefresh;
        base.OnDestroy();
    }

    void OnDimensionsShouldRefresh(object? sender, Configuration? e)
    {
        Android.Util.Log.Info("SmokeTest", "DynamicConfig.DimensionsShouldRefresh fired");
    }

    public override void OnConfigurationChanged(Configuration newConfig)
    {
        base.OnConfigurationChanged(newConfig);
        DynamicConfig.OnConfigurationChanged(this, newConfig);
        ApplyDimens("OnConfigurationChanged");
    }

    void ApplyDimens(string reason)
    {
        // ── Valores do binding ──
        float sdpPx = DimenSdp.Sdp(this, 100);
        float hdpPx = DimenSdp.Hdp(this, 32);
        float wdpPx = DimenSdp.Wdp(this, 100);
        float sspPx = DimenSsp.Ssp(this, 16);

        // Caixas da seção 1 (60/80/100 dp de referência)
        SetBox(Resource.Id.box_sdp, Resource.Id.label_sdp,
            DimenSdp.Sdp(this, 60), "sdp(60)");
        SetBox(Resource.Id.box_sdpa, Resource.Id.label_sdpa,
            DimenSdp.Sdpa(this, 60), $"sdpa(60)  Δ={(DimenSdp.Sdpa(this, 60) - DimenSdp.Sdp(this, 60)):F1}px");
        SetBox(Resource.Id.box_sdpi, Resource.Id.label_sdpi,
            DimenSdp.Sdpi(this, 60), "sdpi(60) ignore multi-window");
        SetBox(Resource.Id.box_hdp, Resource.Id.label_hdp,
            DimenSdp.Hdp(this, 80), "hdp(80)");
        SetBox(Resource.Id.box_wdp, Resource.Id.label_wdp,
            DimenSdp.Wdp(this, 100), "wdp(100)");

        // Textos escaláveis (px → tamanho direto)
        SetText(Resource.Id.text_ssp, DimenSsp.Ssp(this, 16), "16.ssp — scalable text");
        SetText(Resource.Id.text_sspa, DimenSsp.Sspa(this, 16), "16.sspa — +aspect ratio");
        SetText(Resource.Id.text_sem, DimenSsp.Sei(this, 16), "16.sei — ignores font scale");

        // Builder: base(24) + aspect ratio + regra TV + sw600
        float builderPx = new DimenScaled(24)
            .ApplyAspectRatio(true)
            .Screen(DpQualifier.SmallWidth, 600, 32f)
            .Screen(UiModeType.Television, 40f)
            .Sdp(this);
        SetBox(Resource.Id.box_builder, Resource.Id.label_builder,
            builderPx, $"builder → {builderPx:F1}px");

        Android.Util.Log.Info(
            "SmokeTest",
            "{0}: sdp(100)={1}px hdp(32)={2}px wdp(100)={3}px ssp(16)={4}px builder={5:F1}px",
            reason, sdpPx, hdpPx, wdpPx, sspPx, builderPx);

        if (_result != null)
        {
            _result.Text =
                $"{reason}\n" +
                $"sdp(100) = {sdpPx}px · hdp(32) = {hdpPx}px\n" +
                $"wdp(100) = {wdpPx}px · ssp(16) = {sspPx}px\n" +
                $"builder(24+AR+rules) = {builderPx:F1}px\n" +
                $"(binding OK · modular AARs 3.1.9)";
        }
    }

    void SetBox(int boxId, int labelId, float pxSide, string label)
    {
        var box = FindViewById<View>(boxId);
        if (box != null && pxSide > 0)
        {
            float maxSide = Resources!.DisplayMetrics!.WidthPixels / 2f;
            int side = (int)Math.Clamp(pxSide, 8f, maxSide);
            var lp = box.LayoutParameters!;
            lp.Width = side;
            lp.Height = side;
            box.LayoutParameters = lp;
        }
        var l = FindViewById<TextView>(labelId);
        if (l != null) l.Text = $"{label} = {pxSide:F1}px";
    }

    void SetText(int viewId, float sizePx, string text)
    {
        var tv = FindViewById<TextView>(viewId);
        if (tv == null) return;
        tv.Text = text;
        tv.SetTextSize(Android.Util.ComplexUnitType.Px, MathF.Max(sizePx, 10f));
    }
}

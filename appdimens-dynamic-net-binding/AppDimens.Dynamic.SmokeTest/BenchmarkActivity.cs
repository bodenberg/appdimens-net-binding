using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using Com.Appdimens.Dynamic.Code;
using Com.Appdimens.Dynamic.Code.Auto;
using Com.Appdimens.Dynamic.Code.Percent;
using Com.Appdimens.Dynamic.Code.Resize;

namespace AppDimens.Dynamic.SmokeTest;

/// <summary>
/// Dashboard de benchmark escuro, espelhando o BenchmarkActivity Compose do app
/// Android (bodenberg/appdimens-dynamic). Executa o microbenchmark fora da main
/// thread: warmup 10k → medição 100k iterações/op para sdp/hdp/wdp/sdpa + single
/// value com/sem aspect ratio. Suporta AUTO_START_MICRO via Intent extra (adb).
/// </summary>
[Activity(
    Label = "AppDimens Benchmark",
    Exported = true,
    ConfigurationChanges = ConfigChanges.Orientation
        | ConfigChanges.ScreenSize
        | ConfigChanges.ScreenLayout
        | ConfigChanges.SmallestScreenSize
        | ConfigChanges.UiMode
        | ConfigChanges.Density)]
public class BenchmarkActivity : Activity
{
    const string TagMicro = "APPDIMENS_MICRO";
    const int WarmupIterations = 10_000;
    const int MeasureIterations = 100_000;
    const float SingleValue = 64f;

    TextView? _status;
    TextView? _results;
    Button? _runMicro;
    Button? _runAuto;
    bool _running;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        SetContentView(Resource.Layout.activity_benchmark);

        _status = FindViewById<TextView>(Resource.Id.bench_status);
        _results = FindViewById<TextView>(Resource.Id.bench_results);
        _runMicro = FindViewById<Button>(Resource.Id.btn_run_micro);
        _runAuto = FindViewById<Button>(Resource.Id.btn_run_auto);

        FindViewById<Button>(Resource.Id.btn_back)!.Click += (_, _) => Finish();
        _runMicro!.Click += (_, _) => RunBenchmark(includeStrategies: false);
        _runAuto!.Click += (_, _) => RunBenchmark(includeStrategies: true);

        if (Intent?.GetBooleanExtra("AUTO_START_MICRO", false) == true)
            RunBenchmark(includeStrategies: false);
        else if (Intent?.GetBooleanExtra("AUTO_START_FULL", false) == true)
            RunBenchmark(includeStrategies: true);
    }

    void RunBenchmark(bool includeStrategies)
    {
        if (_running) return;
        _running = true;
        _runMicro!.Enabled = false;
        _runAuto!.Enabled = false;
        SetStatus("Running… warmup on background thread");

        Task.Run(() =>
        {
            var r = ExecuteMicroBenchmark(includeStrategies);
            RunOnUiThread(() =>
            {
                _results!.Text = r.Text;
                SetStatus($"Done · total ops measured: {r.TotalOps:N0} · wall time {r.WallMs} ms");
                _runMicro!.Enabled = true;
                _runAuto!.Enabled = true;
                _running = false;
            });
        });
    }

    void SetStatus(string s) => _status!.Text = $"Status: {s}";

    (string Text, long WallMs, long TotalOps) ExecuteMicroBenchmark(bool includeStrategies)
    {
        // ── WARMUP ───────────────────────────────────────────────
        SetStatusUi("Warming up JIT (10k × 4 calls)…");
        float warmupAcc = 0f;
        for (int i = 0; i < WarmupIterations; i++)
        {
            warmupAcc += DimenSdp.Sdp(this, 100);
            warmupAcc += DimenSdp.Hdp(this, 50);
            warmupAcc += DimenSdp.Wdp(this, 30);
            warmupAcc += DimenSdp.Sdpa(this, 40);
        }
        Android.Util.Log.Verbose(TagMicro, "Warmup complete acc={0}", warmupAcc);

        // ── MEASUREMENT ──────────────────────────────────────────
        SetStatusUi("Measuring (100k iters per call type)…");
        const Android.OS.ThreadPriority PriorityUrgentAudio = (Android.OS.ThreadPriority)(-19), PriorityDefault = Android.OS.ThreadPriority.Default;
        try { Android.OS.Process.SetThreadPriority(PriorityUrgentAudio); }
        catch (Exception) { }

        var wall = System.Diagnostics.Stopwatch.StartNew();

        float sdpAcc = Measure(() => DimenSdp.Sdp(this, 100), out double sdpNs);
        float hdpAcc = Measure(() => DimenSdp.Hdp(this, 50), out double hdpNs);
        float wdpAcc = Measure(() => DimenSdp.Wdp(this, 30), out double wdpNs);
        float sdpaAcc = Measure(() => DimenSdp.Sdpa(this, 40), out double sdpaNs);
        float singleNoArAcc = Measure(() => DimenSdp.Sdp(this, (int)SingleValue), out double singleNoArNs);
        float singleWithArAcc = Measure(() => DimenSdp.Sdpa(this, (int)SingleValue), out double singleWithArNs);

        double percentNs = 0, autoNs = 0, resizeNs = 0;
        if (includeStrategies)
        {
            float p1 = Measure(() => DimenPercentDp.Psdp(this, 50), out percentNs);
            float p2 = Measure(() => DimenAutoDp.Asdp(this, 50), out autoNs);
            float p3 = Measure(
                () =>
                {
                    var pair = DimenResize.InnerMaxDimensionsPx(300f, 120f);
                    return pair?.First is Java.Lang.Float f ? f.FloatValue() : 0f;
                },
                out resizeNs);
            Android.Util.Log.Info(TagMicro, "strategy checksum={0}", p1 + p2 + p3);
        }

        try { Android.OS.Process.SetThreadPriority(PriorityDefault); }
        catch (Exception) { }

        long wallMs = wall.ElapsedMilliseconds;

        // ── Combined average ─────────────────────────────────────
        long totalOps = 6L * MeasureIterations;
        double combinedNs = (sdpNs + hdpNs + wdpNs + sdpaNs + singleNoArNs + singleWithArNs) / 6.0;
        float checksum = sdpAcc + hdpAcc + wdpAcc + sdpaAcc + singleNoArAcc + singleWithArAcc;

        // ── Logcat export (mesmo formato do app Android) ─────────
        Android.Util.Log.Info(TagMicro, "╔══════════════════ MICRO BENCHMARK RESULT ══════════════════╗");
        Android.Util.Log.Info(TagMicro, "║ Mode: SCALED (.NET binding)");
        Android.Util.Log.Info(TagMicro, "║ Combined avg: {0}/op · Total ops: {1}", Fmt(combinedNs), totalOps);
        Android.Util.Log.Info(TagMicro, "║ sdp  (bypass): {0}/op", Fmt(sdpNs));
        Android.Util.Log.Info(TagMicro, "║ hdp  (bypass): {0}/op", Fmt(hdpNs));
        Android.Util.Log.Info(TagMicro, "║ wdp  (bypass): {0}/op", Fmt(wdpNs));
        Android.Util.Log.Info(TagMicro, "║ sdpa (cache) : {0}/op", Fmt(sdpaNs));
        Android.Util.Log.Info(TagMicro, "║ single {0} no-AR: {1}/op", SingleValue, Fmt(singleNoArNs));
        Android.Util.Log.Info(TagMicro, "║ single {0} +AR  : {1}/op", SingleValue, Fmt(singleWithArNs));
        if (includeStrategies)
        {
            Android.Util.Log.Info(TagMicro, "║ percent psdp : {0}/op", Fmt(percentNs));
            Android.Util.Log.Info(TagMicro, "║ auto asdp    : {0}/op", Fmt(autoNs));
            Android.Util.Log.Info(TagMicro, "║ resize innerMax: {0}/op", Fmt(resizeNs));
        }
        Android.Util.Log.Info(TagMicro, "║ Total wall time: {0}ms", wallMs);
        Android.Util.Log.Info(TagMicro, "║ Accumulator checksum: {0}", checksum);
        Android.Util.Log.Info(TagMicro, "╚════════════════════════════════════════════════════════════╝");

        string text =
            $".NET binding — scaled family\n\n" +
            $"sdp  (bypass): {Fmt(sdpNs)}/op\n" +
            $"hdp  (bypass): {Fmt(hdpNs)}/op\n" +
            $"wdp  (bypass): {Fmt(wdpNs)}/op\n" +
            $"sdpa (cache) : {Fmt(sdpaNs)}/op\n\n" +
            $"single no-AR : {Fmt(singleNoArNs)}/op\n" +
            $"single +AR   : {Fmt(singleWithArNs)}/op\n" +
            $"combined avg : {Fmt(combinedNs)}/op\n" +
            $"checksum     : {checksum:F0}\n" +
            $"wall time    : {wallMs} ms";

        if (includeStrategies)
        {
            text += "\n\nStrategy modules\n" +
                    $"percent psdp   : {Fmt(percentNs)}/op\n" +
                    $"auto asdp      : {Fmt(autoNs)}/op\n" +
                    $"resize innerMax: {Fmt(resizeNs)}/op";
        }

        return (text, wallMs, totalOps);
    }

    static float Measure(Func<float> call, out double avgNsPerOp)
    {
        // Call-site warmup (absorve transientes de JIT/inline-cache)
        float acc = 0f;
        for (int i = 0; i < WarmupIterations; i++) acc += call();

        var sw = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < MeasureIterations; i++) acc += call();
        sw.Stop();

        avgNsPerOp = (double)(sw.Elapsed.TotalMilliseconds * 1_000_000.0) / MeasureIterations;
        return acc;
    }

    static string Fmt(double ns) =>
        ns >= 1000 ? $"{ns / 1000:F2}µs" : $"{ns:F0}ns";

    void SetStatusUi(string s) => RunOnUiThread(() => SetStatus(s));
}

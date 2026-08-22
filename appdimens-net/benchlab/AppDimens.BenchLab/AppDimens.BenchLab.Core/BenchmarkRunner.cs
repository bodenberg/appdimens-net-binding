using System.Diagnostics;
using AppDimens.Net;
using AppDimens.Net.Code.Fluid;
using AppDimens.Net.Code.Scaled;
using AppDimens.Net.Testing;
using AppDimens.Net.Core;

namespace AppDimens.BenchLab.Core;

/// <summary>One measured row of the benchmark report.</summary>
public sealed record BenchRow(string Name, double NsPerOp, double OpsPerSec, long AllocatedBytes);

/// <summary>Full report with environment metadata.</summary>
public sealed record BenchReport(
    string Device, int WidthDp, int HeightDp, int DensityDpi,
    IReadOnlyList<BenchRow> Rows, double TotalMs)
{
    public string ToText() =>
        "AppDimens BenchLab — .NET\n" +
        $"device={Device} window={WidthDp}x{HeightDp}dp dpi={DensityDpi}\n" +
        new string('-', 62) + "\n" +
        string.Join("\n", Rows.Select(r =>
            $"{r.Name,-38} {r.NsPerOp,8:0.0} ns/op {r.OpsPerSec,12:N0} ops/s alloc={r.AllocatedBytes}B")) + "\n" +
        $"total: {TotalMs:0.0} ms";
}

/// <summary>
/// Micro-benchmark engine comparing the AppDimens fast lanes against raw multiplies,
/// the full cached path, the uncached path and a legacy XML-grid approximation
/// (dictionary lookup, mirroring the deprecated MAUI v1 approach).
/// Mobile-friendly by default: bounded warmup/measured counts, chunked row
/// execution with progress callbacks so callers can keep the UI alive, and a
/// reentrancy guard mirroring the KMP BenchlabController fix.
/// </summary>
public static class BenchmarkRunner
{
    private const int Warmup = 10_000;
    private const int Measured = 200_000;
    private const int MaxMeasured = 1_000_000;

    /// <summary>True while RunAsync/Run is executing (per-process guard).</summary>
    public static bool IsRunning { get; private set; }

    public static BenchReport Run(FakeAppDimensContext? context = null,
        Action<BenchRow>? onRowCompleted = null, CancellationToken ct = default)
    {
        if (IsRunning) throw new InvalidOperationException("A benchmark run is already in flight.");
        IsRunning = true;
        try
        {
            var ctx = context ?? NewPhoneContext();
            DimenCache.Init(ctx);
            DimenCache.ClearAll();
            var cfg = ctx.Config;

            var ops = new (string Name, Func<int, float> Op)[]
            {
                ("baseline raw multiply (dp)", _ => 16f * DimenCache.InvBaseRatio * cfg.SmallestScreenWidthDp),
                ("sdp fast lane — ResolveSdpDp", _ => DimenCache.ResolveSdpDp(16f, ctx)),
                ("sdpa fast lane (aspect-ratio)", _ => 16.Sdpa(ctx)),
                ("hdp fast lane", _ => 48.Hdp(ctx)),
                ("full cached path ToDynamicScaledDp", _ => 16f.ToDynamicScaledDp(ctx)),
                ("uncached full formula", _ => UncachedSdp(cfg)),
                ("legacy v1 XML-grid lookup", _ => LegacyGridLookup(16)),
                ("fluid cached", _ => ((float)28).ToFluidDp(ctx)),
            };

            var rows = new List<BenchRow>(ops.Length);
            foreach (var (name, op) in ops)
            {
                ct.ThrowIfCancellationRequested();
                var row = Measure(name, op);
                rows.Add(row);
                onRowCompleted?.Invoke(row);
            }

            return new BenchReport("dotnet-bench", cfg.ScreenWidthDp, cfg.ScreenHeightDp,
                cfg.DensityDpi, rows, rows.Sum(r => r.NsPerOp));
        }
        finally
        {
            IsRunning = false;
        }
    }

    /// <summary>Chunked async run: yields between rows so UI threads stay responsive.</summary>
    public static async Task<BenchReport> RunAsync(FakeAppDimensContext? context = null,
        Action<BenchRow>? onRowCompleted = null, CancellationToken ct = default)
    {
        return await Task.Run(() => Run(context, onRowCompleted, ct), ct).ConfigureAwait(false);
    }

    public static FakeAppDimensContext NewPhoneContext() =>
        new(new ScreenConfiguration(360, 740, 360, 420, 1f,
            ScreenConfiguration.OrientationUndefined, 0));

    private static float UncachedSdp(ScreenConfiguration cfg)
    {
        var dim = (float)cfg.SmallestScreenWidthDp;
        return 16f * dim * DimenCache.InvBaseRatio;
    }

    // Deprecated MAUI v1 style: pre-generated resource table per integer index.
    private static readonly Dictionary<int, float> LegacyGrid =
        Enumerable.Range(-600, 1201).ToDictionary(i => i, i => i / 300f * 1.2f);
    private static float LegacyGridLookup(int idx) => LegacyGrid[idx];

    internal static BenchRow Measure(string name, Func<int, float> op, int measuredOverride = Measured)
    {
        var measured = Math.Clamp(measuredOverride, 1, MaxMeasured);
        for (var i = 0; i < Warmup; i++) op(i);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var startAlloc = GC.GetAllocatedBytesForCurrentThread();
        var sw = Stopwatch.StartNew();
        float sink = 0;
        for (var i = 0; i < measured; i++) sink += op(i);
        sw.Stop();
        var alloc = GC.GetAllocatedBytesForCurrentThread() - startAlloc;
        GC.KeepAlive(sink);

        var nsPerOp = sw.Elapsed.TotalMilliseconds * 1_000_000d / measured;
        return new BenchRow(name, nsPerOp, measured / sw.Elapsed.TotalSeconds, alloc / measured);
    }
}

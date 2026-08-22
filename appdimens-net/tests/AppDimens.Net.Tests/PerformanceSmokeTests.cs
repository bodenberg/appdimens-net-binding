using System.Diagnostics;
using AppDimens.Net;
using AppDimens.Net.Code.Scaled;
using AppDimens.Net.Core;
using Xunit;

namespace AppDimens.Net.Tests;

/// <summary>
/// Performance smoke gates — generous thresholds for shared CI runners, strict
/// allocation checks (hit path must be zero-alloc, mirroring the KMP guarantee).
/// </summary>
public class PerformanceSmokeTests
{
    [Fact]
    public void Fast_lane_hit_path_is_zero_allocation()
    {
        var ctx = Fx.Phone();
        DimenCache.Init(ctx);
        _ = DimenCache.ResolveSdpDp(16f, ctx); // warm

        var before = GC.GetAllocatedBytesForCurrentThread();
        float sink = 0;
        for (var i = 0; i < 100_000; i++) sink += DimenCache.ResolveSdpDp(16f, ctx);
        var after = GC.GetAllocatedBytesForCurrentThread();
        GC.KeepAlive(sink);

        Assert.Equal(0, after - before);
    }

    [Fact]
    public void One_million_fast_resolutions_under_300ms()
    {
        var ctx = Fx.Phone();
        DimenCache.Init(ctx);
        _ = DimenCache.ResolveSdpDp(16f, ctx);

        var sw = Stopwatch.StartNew();
        float sink = 0;
        for (var i = 0; i < 1_000_000; i++) sink += DimenCache.ResolveSdpDp(16f, ctx);
        sw.Stop();
        GC.KeepAlive(sink);

        Assert.True(sw.ElapsedMilliseconds < 300,
            $"fast lane too slow: {sw.ElapsedMilliseconds}ms / 1M ops");
    }

    [Fact]
    public void Bench_report_is_well_formed()
    {
        var report = AppDimens.BenchLab.Core.BenchmarkRunner.Run(Fx.Phone());
        Assert.Equal(8, report.Rows.Count);
        Assert.All(report.Rows, r => Assert.True(r.NsPerOp > 0));
        var text = report.ToText();
        Assert.Contains("AppDimens BenchLab", text);
    }
}

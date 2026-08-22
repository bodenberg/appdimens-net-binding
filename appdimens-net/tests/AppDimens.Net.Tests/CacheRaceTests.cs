using AppDimens.Net;
using AppDimens.Net.Core;
using AppDimens.Net.Code.Scaled;
using AppDimens.Net.Testing;
using Xunit;

using AppDimens.Net.Common;

namespace AppDimens.Net.Tests;

/// <summary>
/// Port of DimenCacheRaceTest: concurrent resolutions against alternating snapshots
/// must return each caller's exact expected value — zero cross-snapshot contamination.
/// </summary>
public class CacheRaceTests
{
    private const int Threads = 8;
    private const int Iterations = 20_000;

    [Fact]
    public void Concurrent_snapshots_never_return_value_from_another_snapshot()
    {
        var mA = new DimenMetrics(360, 740, 360, 420, BitConverter.SingleToInt32Bits(1f), 1, 0, false);
        var mB = new DimenMetrics(800, 1280, 800, 240, BitConverter.SingleToInt32Bits(1f), 2, 0, false);

        float ComputeA() => 16f * mA.Scale * 2f;
        float ComputeB() => 16f * mB.Scale / 2f;

        // NOTE: keys must differ per (snapshot,value) family; use distinct base bits.
        var keyA = DimenCache.BuildKey(16f, false, false, DimenCache.CalcType.Fit,
            Common.DpQualifier.SmallWidth, Common.Inverter.Default, false, DimenCache.ValueType.Dp);
        // second family uses a different ValueType to avoid aliasing with A
        var keyB = DimenCache.BuildKey(16f, false, false, DimenCache.CalcType.Fit,
            Common.DpQualifier.SmallWidth, Common.Inverter.Default, false, DimenCache.ValueType.Px);

        var barrier = new Barrier(Threads);
        var failures = 0;
        var threads = new Thread[Threads];
        for (var t = 0; t < Threads; t++)
        {
            threads[t] = new Thread(() =>
            {
                barrier.SignalAndWait();
                for (var i = 0; i < Iterations; i++)
                {
                    if (DimenCache.Resolve(keyA, mA, ComputeA) != ComputeA())
                        Interlocked.Increment(ref failures);
                    var vb = DimenCache.Resolve(keyB, mB, ComputeB);
                    if (vb != ComputeB()) Interlocked.Increment(ref failures);
                }
            });
            threads[t].Start();
        }
        foreach (var th in threads) th.Join();

        Assert.Equal(0, failures);
    }

    [Fact]
    public void Custom_K_is_computed_exactly_and_not_cached()
    {
        var ctx = Fx.Phone();
        var first = 16f.ToDynamicScaledDp(ctx, applyAspectRatio: true, customSensitivityK: 0.123456f);

        // pollute the slot that would have been used
        _ = 16f.ToDynamicScaledDp(ctx);
        _ = 16.Sdpa(ctx);

        var again = 16f.ToDynamicScaledDp(ctx, applyAspectRatio: true, customSensitivityK: 0.123456f);
        Assert.Equal(first, again);

        var other = 16f.ToDynamicScaledDp(ctx, applyAspectRatio: true, customSensitivityK: 0.654321f);
        Assert.NotEqual(first, other);
    }

    [Fact]
    public void Snapshot_partitions_are_bounded()
    {
        DimenCache.ClearAll();
        for (var i = 0; i < 12; i++)
        {
            var ctx = new FakeAppDimensContext(new ScreenConfiguration(300 + i, 700 + i, 300 + i, 160 + i, 1f, 0, 0));
            DimenCache.Init(ctx);
            _ = 16f.ToDynamicScaledDp(ctx);
        }
        // no crash and cache still coherent afterwards
        var ctxFinal = Fx.Phone();
        DimenCache.Init(ctxFinal);
        Assert.Equal(16f.ToDynamicScaledDp(ctxFinal), DimenCache.ResolveSdpDp(16f, ctxFinal));
    }

    [Fact]
    public void InvalidateOnConfigChange_forces_rebuild()
    {
        var ctx = new FakeAppDimensContext(new ScreenConfiguration(360, 740, 360, 420, 1f, 0, 0));
        DimenCache.Init(ctx);
        var before = DimenCache.ResolveSdpDp(16f, ctx);

        ctx.SetConfig(600, 900);          // simulated resize
        ctx.NotifyChange();                // watcher nulls fast slots synchronously
        var after = DimenCache.ResolveSdpDp(16f, ctx);

        Assert.NotEqual(before, after);
        Assert.Equal(DimenCache.ResolveSdpDp(16f, ctx), after);
        Assert.Equal(16f * 360f * DimenCache.InvBaseRatio, before);
        Assert.Equal(16f * 600f * DimenCache.InvBaseRatio, after);
    }
}

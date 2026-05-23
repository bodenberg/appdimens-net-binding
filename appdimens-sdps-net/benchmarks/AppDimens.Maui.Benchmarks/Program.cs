using AppDimens.Maui;
using AppDimens.Maui.Core;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace AppDimens.Maui.Benchmarks;

[MemoryDiagnoser]
public class DimenLookupBenchmarks
{
    private AppDimensResolver _resolver = null!;

    [GlobalSetup]
    public void Setup()
    {
        _resolver = AppDimensResolver.Instance;
        _resolver.Initialize(new AppDimensOptions { ScalingMode = ScalingMode.Continuous });
        _resolver.SetMetricsForTesting(411, 823, 2.0);
    }

    [Benchmark]
    public double CachedSdpLookup() => _resolver.Sdp(16);

    [Benchmark]
    public double UncachedKeyPath()
    {
        _resolver.Cache.Invalidate();
        return _resolver.Sdp(16);
    }
}

public static class Program
{
    public static void Main(string[] args) => BenchmarkRunner.Run<DimenLookupBenchmarks>();
}

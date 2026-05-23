using System.Diagnostics;
using AppDimens.Maui;
using AppDimens.Maui.Core;

namespace AppDimens.Maui.Sample.Services;

public sealed class SampleBenchmarkResult
{
    public required string Name { get; init; }
    public double NanosecondsPerOp { get; init; }
    public double ResultValue { get; init; }
    public int Iterations { get; init; }
}

public static class SampleBenchmarkRunner
{
    private const int Warmup = 500;
    private const int Iterations = 50_000;

    public static IReadOnlyList<SampleBenchmarkResult> RunAll()
    {
        var resolver = AppDimensResolver.Instance;
        resolver.RefreshMetricsFromDevice();

        return
        [
            Run("SDP cache hit (16)", () => resolver.Sdp(16)),
            Run("SSP cache hit (18)", () => resolver.Ssp(18)),
            Run("HDP cache hit (48)", () => resolver.Hdp(48)),
            Run("WDP cache hit (120)", () => resolver.Wdp(120)),
            Run("SDPA cache hit (16)", () => resolver.Sdpa(16)),
            RunUncached("SDP after cache invalidate", resolver, () => resolver.Sdp(16)),
        ];
    }

    /// <summary>
    /// Recomputes display values for existing benchmark rows without rerunning timing.
    /// </summary>
    public static IReadOnlyList<SampleBenchmarkResult> WithFreshDisplayValues(IReadOnlyList<SampleBenchmarkResult> results)
    {
        var resolver = AppDimensResolver.Instance;
        return results
            .Select(r => new SampleBenchmarkResult
            {
                Name = r.Name,
                NanosecondsPerOp = r.NanosecondsPerOp,
                Iterations = r.Iterations,
                ResultValue = ResolveDisplayValue(r.Name, resolver),
            })
            .ToList();
    }

    private static double ResolveDisplayValue(string name, AppDimensResolver resolver) =>
        name switch
        {
            "SDP cache hit (16)" or "SDP after cache invalidate" => resolver.Sdp(16),
            "SSP cache hit (18)" => resolver.Ssp(18),
            "HDP cache hit (48)" => resolver.Hdp(48),
            "WDP cache hit (120)" => resolver.Wdp(120),
            "SDPA cache hit (16)" => resolver.Sdpa(16),
            _ => throw new ArgumentOutOfRangeException(nameof(name), name, "Unknown benchmark name."),
        };

    private static SampleBenchmarkResult Run(string name, Func<double> op)
    {
        double last = 0;
        for (var i = 0; i < Warmup; i++)
            last = op();

        var sw = Stopwatch.StartNew();
        for (var i = 0; i < Iterations; i++)
            last = op();
        sw.Stop();

        var ns = sw.Elapsed.TotalNanoseconds / Iterations;
        return new SampleBenchmarkResult
        {
            Name = name,
            NanosecondsPerOp = ns,
            ResultValue = last,
            Iterations = Iterations,
        };
    }

    private static SampleBenchmarkResult RunUncached(string name, AppDimensResolver resolver, Func<double> op)
    {
        double last = 0;
        for (var i = 0; i < Warmup; i++)
        {
            resolver.Cache.Invalidate();
            last = op();
        }

        var sw = Stopwatch.StartNew();
        for (var i = 0; i < Iterations; i++)
        {
            resolver.Cache.Invalidate();
            last = op();
        }
        sw.Stop();

        var ns = sw.Elapsed.TotalNanoseconds / Iterations;
        return new SampleBenchmarkResult
        {
            Name = name,
            NanosecondsPerOp = ns,
            ResultValue = last,
            Iterations = Iterations,
        };
    }
}

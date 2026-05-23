using AppDimens.Maui;
using AppDimens.Maui.Converters;
using AppDimens.Maui.Core;
using AppDimens.Maui.Extensions;
using AppDimens.Maui.Inverters;
using AppDimens.Maui.Responsive;
using Xunit;

namespace AppDimens.Maui.Tests;

public class BucketRegistryTests
{
    private static string GeneratedPath => TestPaths.GeneratedResources
        ?? throw new InvalidOperationException("Run scripts/generate-dimens.py before tests.");

    [Fact]
    public void LoadFromGenerated_MergedLayout_HasLayoutVersion2()
    {
        var registry = BucketRegistry.LoadFromGenerated(GeneratedPath);
        Assert.Equal(2, registry.LayoutVersion);
        Assert.True(registry.BucketValues.Count > 300);
        Assert.True(registry.BaseValues.ContainsKey("_16"));
        Assert.False(registry.BaseValues.ContainsKey("_16sdp"));
    }

    [Fact]
    public void TryGetValue_MergedLayout_Bucket411_MatchesScale()
    {
        var registry = BucketRegistry.LoadFromGenerated(GeneratedPath);
        Assert.True(registry.TryGetValue(DpQualifier.SmallWidth, 411, "_16", out var v));
        Assert.Equal(16 * 411.0 / 300.0, v, precision: 2);
    }

    [Theory]
    [InlineData(300)]
    [InlineData(585)]
    [InlineData(720)]
    public void BucketFiles_ExistForCommonSizes(int size)
    {
        var path = Path.Combine(GeneratedPath, $"Dimens.{size}.xaml");
        Assert.True(File.Exists(path), $"Missing {path}");
    }
}

public class HybridModeTests
{
    private static string? GeneratedPath => TestPaths.GeneratedResources;

    [Fact]
    public void HybridMode_WithGenerated_MatchesBucketFormula()
    {
        var r = AppDimensResolver.Instance;
        r.ResetForTesting();
        r.Initialize(new AppDimensOptions { ScalingMode = ScalingMode.Hybrid }, GeneratedPath, force: true);
        r.SetMetricsForTesting(411, 823, 2.0);

        Assert.Equal(16 * 411.0 / 300.0, r.Sdp(16), precision: 2);
    }

    [Fact]
    public void HybridMode_WithoutGenerated_FallsBackToContinuous()
    {
        var r = AppDimensResolver.Instance;
        r.ResetForTesting();
        r.Initialize(new AppDimensOptions { ScalingMode = ScalingMode.Hybrid }, generatedResourcesPath: null, force: true);
        r.SetMetricsForTesting(411, 823, 2.0);

        Assert.Equal(ScaleEngine.Sdp(16, 411), r.Sdp(16), precision: 2);
    }
}

public class InverterEngineExtendedTests
{
    private static ScreenMetricsSnapshot Landscape => new(800, 360, 360, 2, 320, ScreenOrientation.Landscape, UiModeType.Normal);
    private static ScreenMetricsSnapshot Portrait => new(360, 800, 360, 2, 320, ScreenOrientation.Portrait, UiModeType.Normal);

    [Theory]
    [InlineData(InverterType.PhToLw, DpQualifier.Height, DpQualifier.Width)]
    [InlineData(InverterType.PwToLh, DpQualifier.Width, DpQualifier.Height)]
    [InlineData(InverterType.SwToLh, DpQualifier.SmallWidth, DpQualifier.Height)]
    [InlineData(InverterType.SwToLw, DpQualifier.SmallWidth, DpQualifier.Width)]
    public void LandscapeInverters_SwitchAxis(InverterType inv, DpQualifier input, DpQualifier expected)
    {
        Assert.Equal(expected, InverterEngine.EffectiveQualifier(Landscape, input, inv));
    }

    [Theory]
    [InlineData(InverterType.LhToPw, DpQualifier.Height, DpQualifier.Width)]
    [InlineData(InverterType.LwToPh, DpQualifier.Width, DpQualifier.Height)]
    [InlineData(InverterType.SwToPh, DpQualifier.SmallWidth, DpQualifier.Height)]
    [InlineData(InverterType.SwToPw, DpQualifier.SmallWidth, DpQualifier.Width)]
    public void PortraitInverters_SwitchAxis(InverterType inv, DpQualifier input, DpQualifier expected)
    {
        Assert.Equal(expected, InverterEngine.EffectiveQualifier(Portrait, input, inv));
    }
}

public class ConverterTests
{
    [Fact]
    public void SdpConverter_Convert_Int_ReturnsScaled()
    {
        var r = AppDimensResolver.Instance;
        r.ResetForTesting();
        r.Initialize(new AppDimensOptions { ScalingMode = ScalingMode.Continuous, WarmupAspectRatio = false }, force: true);
        r.SetMetricsForTesting(411, 823, 2.0);

        var c = new SdpConverter();
        var result = c.Convert(16, typeof(double), null, null!);
        Assert.Equal(16.Sdp(), result);
    }

    [Fact]
    public void SdpConverter_ConvertBack_Throws()
    {
        var c = new SdpConverter();
        Assert.Throws<NotSupportedException>(() => c.ConvertBack(1.0, typeof(int), null, null!));
    }
}

public class DimensionCacheStressTests
{
    [Fact]
    public void ConcurrentGetOrAdd_IsStable()
    {
        var cache = new DimensionCache();
        var key = new DimenCacheKey(16, DpQualifier.SmallWidth, InverterType.Default, false, false, ScalingMode.Continuous);
        Parallel.For(0, 100, _ => cache.GetOrAdd(key, () => 42.0));
        Assert.Equal(42.0, cache.GetOrAdd(key, () => 99.0));
    }
}

public class ScaleEngineValidationTests
{
    [Fact]
    public void ValidateIndex_RejectsOutOfRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ScaleEngine.ValidateIndex(601, allowNegative: false));
        Assert.Throws<ArgumentOutOfRangeException>(() => ScaleEngine.ValidateIndex(-301, allowNegative: true));
    }
}

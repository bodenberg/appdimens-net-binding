using AppDimens.Maui.Builders;
using AppDimens.Maui.Core;
using AppDimens.Maui.Extensions;
using AppDimens.Maui.Helpers;
using AppDimens.Maui.Inverters;
using AppDimens.Maui.Responsive;
using Markup = AppDimens.Maui.Markup;
using Xaml = AppDimens.Maui.Xaml;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Xunit;

namespace AppDimens.Maui.Tests;

public class ScaleEngineTests
{
    [Theory]
    [InlineData(16, 411, 21.92)]
    [InlineData(16, 300, 16.0)]
    [InlineData(16, 360, 19.2)]
    public void Sdp_Continuous_MatchesFormula(int nominal, double smallestDp, double expected)
    {
        var result = ScaleEngine.Sdp(nominal, smallestDp);
        Assert.Equal(expected, result, precision: 2);
    }

    [Fact]
    public void BuildResourceKey_AxisNeutral()
    {
        Assert.Equal("_minus8", ScaleEngine.BuildResourceKey(-8, DpQualifier.SmallWidth));
        Assert.Equal("_16", ScaleEngine.BuildResourceKey(16, DpQualifier.Width));
        Assert.Equal("_16sdp", ScaleEngine.BuildLegacySuffixedKey("_16", DpQualifier.SmallWidth));
    }
}

public class QualifierManagerTests
{
    private static readonly int[] Buckets = [320, 360, 400, 411, 480, 600, 720];

    [Theory]
    [InlineData(411, 411)]
    [InlineData(420, 411)]
    [InlineData(319, 300)]
    [InlineData(720, 720)]
    [InlineData(800, 720)]
    public void SelectBucket_AndroidLike(int metric, int expected)
    {
        Assert.Equal(expected, QualifierManager.SelectBucket(metric, Buckets));
    }
}

public class InverterEngineTests
{
    [Fact]
    public void SdpPh_Portrait_UsesHeightAxis()
    {
        var metrics = new ScreenMetricsSnapshot(360, 800, 360, 2, 320, ScreenOrientation.Portrait, UiModeType.Normal);
        var effective = InverterEngine.EffectiveQualifier(metrics, DpQualifier.SmallWidth, InverterEngine.ForSdpPh());
        Assert.Equal(DpQualifier.Height, effective);
    }

    [Fact]
    public void SdpPh_Landscape_KeepsSmallWidth()
    {
        var metrics = new ScreenMetricsSnapshot(800, 360, 360, 2, 320, ScreenOrientation.Landscape, UiModeType.Normal);
        var effective = InverterEngine.EffectiveQualifier(metrics, DpQualifier.SmallWidth, InverterEngine.ForSdpPh());
        Assert.Equal(DpQualifier.SmallWidth, effective);
    }
}

public class AppDimensResolverTests
{
    private static string? GeneratedPath => TestPaths.GeneratedResources;

    private static AppDimensResolver R
    {
        get
        {
            var r = AppDimensResolver.Instance;
            r.ResetForTesting();
            return r;
        }
    }

    [Fact]
    public void BucketMode_MatchesAndroidFormula()
    {
        var resolver = R;
        resolver.Initialize(new AppDimensOptions { ScalingMode = ScalingMode.Bucket }, GeneratedPath, force: true);
        resolver.SetMetricsForTesting(411, 823, 2.0);

        var bucket = resolver.Buckets!.GetActiveBucket(DpQualifier.SmallWidth);
        Assert.Equal(411, bucket);

        var value = resolver.Sdp(16);
        Assert.Equal(16 * 411.0 / 300.0, value, precision: 2);
    }

    [Fact]
    public void Hdp_HeightChange_UpdatesResolvedValue()
    {
        var resolver = R;
        resolver.Initialize(new AppDimensOptions { ScalingMode = ScalingMode.Continuous, WarmupAspectRatio = false }, force: true);
        resolver.SetMetricsForTesting(360, 800, 2.0);
        var tall = resolver.Hdp(48);
        resolver.SetMetricsForTesting(360, 400, 2.0);
        var shorter = resolver.Hdp(48);
        Assert.Equal(48 * 800.0 / 300.0, tall, precision: 2);
        Assert.Equal(48 * 400.0 / 300.0, shorter, precision: 2);
    }

    [Fact]
    public void SdpPh_Rotation_SwitchesAxis()
    {
        var resolver = R;
        resolver.Initialize(new AppDimensOptions { ScalingMode = ScalingMode.Continuous }, GeneratedPath, force: true);

        resolver.SetMetricsForTesting(800, 360, 2.0);
        var landscape = resolver.Sdp(16, InverterEngine.ForSdpPh());

        resolver.SetMetricsForTesting(360, 800, 2.0);
        var portrait = resolver.Sdp(16, InverterEngine.ForSdpPh());

        Assert.Equal(16 * 800.0 / 300.0, portrait, precision: 2);
        Assert.Equal(16 * 360.0 / 300.0, landscape, precision: 2);
    }
}

public class DimenScaledTests
{
    [Fact]
    public void Scaled_LandscapeRule_Wins()
    {
        var r = AppDimensResolver.Instance;
        r.ResetForTesting();
        r.Initialize(new AppDimensOptions { ScalingMode = ScalingMode.Continuous, WarmupAspectRatio = false }, force: true);
        r.SetMetricsForTesting(800, 360, 2.0, ScreenOrientation.Landscape);

        var value = AppDimens.Maui.Builders.Responsive.Value(14).Landscape(16).Sdp();
        Assert.Equal(16 * 360.0 / 300.0, value, precision: 2);
    }

    [Fact]
    public void Scaled_TabletRule_Wins()
    {
        var r = AppDimensResolver.Instance;
        r.ResetForTesting();
        r.Initialize(new AppDimensOptions { ScalingMode = ScalingMode.Continuous, WarmupAspectRatio = false }, force: true);
        r.SetMetricsForTesting(720, 1280, 2.0);

        var value = AppDimens.Maui.Builders.Responsive.Value(14).Tablet(18).Sdp();
        Assert.Equal(18 * 720.0 / 300.0, value, precision: 2);
    }
}

public class MarkupParityTests
{
    [Fact]
    public void ShortAndNormal_Sdp_AreEqual()
    {
        var r = AppDimensResolver.Instance;
        r.ResetForTesting();
        r.Initialize(new AppDimensOptions { ScalingMode = ScalingMode.Continuous, WarmupAspectRatio = false }, force: true);
        r.SetMetricsForTesting(411, 823, 2.0);

        var shortForm = new Markup.SdpExtension { Value = 16 }.ProvideValue(null!);
        var normalForm = new Xaml.SdpExtension { Value = 16 }.ProvideValue(null!);

        Assert.Equal(shortForm, normalForm);
        Assert.Equal(16.Sdp(), shortForm);
    }

    [Fact]
    public void ShortAndNormal_SdpPh_AreEqual()
    {
        var r = AppDimensResolver.Instance;
        r.ResetForTesting();
        r.Initialize(new AppDimensOptions { ScalingMode = ScalingMode.Continuous, WarmupAspectRatio = false }, force: true);
        r.SetMetricsForTesting(360, 800, 2.0);

        var shortForm = new Markup.SdpPhExtension { Value = 16 }.ProvideValue(null!);
        var normalForm = new Xaml.SdpExtension
        {
            Value = 16,
            Inverter = InverterType.SwToPh,
        }.ProvideValue(null!);

        Assert.Equal(shortForm, normalForm);
    }

    [Fact]
    public void ShortAndNormal_SdpLwA_AreEqual()
    {
        var r = AppDimensResolver.Instance;
        r.ResetForTesting();
        r.Initialize(new AppDimensOptions { ScalingMode = ScalingMode.Continuous, WarmupAspectRatio = false }, force: true);
        r.SetMetricsForTesting(360, 800, 2.0);

        var shortForm = new Markup.SdpLwAExtension { Value = 16 }.ProvideValue(null!);
        var normalForm = new Xaml.SdpaExtension
        {
            Value = 16,
            Inverter = InverterType.SwToLw,
        }.ProvideValue(null!);

        Assert.Equal(shortForm, normalForm);
        Assert.Equal(16.SdpLwa(), shortForm);
    }

    [Fact]
    public void ShortAndNormal_SemaPh_AreEqual()
    {
        var r = AppDimensResolver.Instance;
        r.ResetForTesting();
        r.Initialize(new AppDimensOptions { ScalingMode = ScalingMode.Continuous, WarmupAspectRatio = false }, force: true);
        r.SetMetricsForTesting(360, 800, 2.0);

        var shortForm = new Markup.SemaPhExtension { Value = 18 }.ProvideValue(null!);
        var normalForm = new Xaml.SemaExtension
        {
            Value = 18,
            Inverter = InverterType.SwToPh,
        }.ProvideValue(null!);

        Assert.Equal(shortForm, normalForm);
        Assert.Equal(18.SemaPh(), shortForm);
    }

    [Fact]
    public void ShortAndNormal_HdpRotate_AreEqual()
    {
        var r = AppDimensResolver.Instance;
        r.ResetForTesting();
        r.Initialize(new AppDimensOptions { ScalingMode = ScalingMode.Continuous, WarmupAspectRatio = false }, force: true);
        r.SetMetricsForTesting(360, 800, 2.0);

        var shortForm = new Markup.HdpRotateExtension
        {
            Base = 48,
            Rotation = 64,
            Orientation = OrientationQualifier.Landscape,
        }.ProvideValue(null!);
        var normalForm = new Xaml.HdpRotateExtension
        {
            Base = 48,
            Rotation = 64,
            Orientation = OrientationQualifier.Landscape,
        }.ProvideValue(null!);

        Assert.Equal(shortForm, normalForm);
        Assert.Equal(48.HdpRotate(64, orientation: OrientationQualifier.Landscape), shortForm);
    }

    [Fact]
    public void ShortAndNormal_SdpMode_AreEqual()
    {
        var r = AppDimensResolver.Instance;
        r.ResetForTesting();
        r.Initialize(new AppDimensOptions { ScalingMode = ScalingMode.Continuous, WarmupAspectRatio = false }, force: true);
        r.SetMetricsForTesting(360, 800, 2.0);

        var shortForm = new Markup.SdpModeExtension
        {
            Base = 16,
            Mode = 24,
            UiMode = UiModeType.Desk,
        }.ProvideValue(null!);
        var normalForm = new Xaml.SdpModeExtension
        {
            Base = 16,
            Mode = 24,
            UiMode = UiModeType.Desk,
        }.ProvideValue(null!);

        Assert.Equal(shortForm, normalForm);
        Assert.Equal(16.SdpMode(24, UiModeType.Desk), shortForm);
    }

    [Fact]
    public void ShortAndNormal_SdpQualifier_AreEqual()
    {
        var r = AppDimensResolver.Instance;
        r.ResetForTesting();
        r.Initialize(new AppDimensOptions { ScalingMode = ScalingMode.Continuous, WarmupAspectRatio = false }, force: true);
        r.SetMetricsForTesting(360, 800, 2.0);

        var shortForm = new Markup.SdpQualifierExtension
        {
            Base = 16,
            Qualified = 20,
            QualifierType = DpQualifier.SmallWidth,
            Threshold = 600,
        }.ProvideValue(null!);
        var normalForm = new Xaml.SdpQualifierExtension
        {
            Base = 16,
            Qualified = 20,
            QualifierType = DpQualifier.SmallWidth,
            Threshold = 600,
        }.ProvideValue(null!);

        Assert.Equal(shortForm, normalForm);
        Assert.Equal(16.SdpQualifier(20, DpQualifier.SmallWidth, 600), shortForm);
    }

    [Fact]
    public void Short_Sdp_PaddingTarget_ReturnsThickness()
    {
        var r = AppDimensResolver.Instance;
        r.ResetForTesting();
        r.Initialize(new AppDimensOptions { ScalingMode = ScalingMode.Continuous, WarmupAspectRatio = false }, force: true);
        r.SetMetricsForTesting(411, 823, 2.0);

        var target = new ThicknessTargetStub();
        var sp = new ServiceProviderStub(target);
        var result = new Markup.SdpExtension { Value = 20 }.ProvideValue(sp);

        Assert.IsType<Thickness>(result);
        var t = (Thickness)result;
        Assert.Equal(t.Left, t.Top);
        Assert.Equal(20.Sdp(), t.Left, precision: 2);
    }

    [Fact]
    public void Short_Sdp_SpacingTarget_ReturnsDouble()
    {
        var r = AppDimensResolver.Instance;
        r.ResetForTesting();
        r.Initialize(new AppDimensOptions { ScalingMode = ScalingMode.Continuous, WarmupAspectRatio = false }, force: true);
        r.SetMetricsForTesting(411, 823, 2.0);

        var target = new SpacingTargetStub();
        var sp = new ServiceProviderStub(target);
        var result = new Markup.SdpExtension { Value = 4 }.ProvideValue(sp);

        Assert.Equal(typeof(double), StackLayout.SpacingProperty.ReturnType);
        Assert.False(result is Thickness, $"Expected double, got {result?.GetType().Name}");
        Assert.IsType<double>(result);
        Assert.Equal(4.Sdp(), (double)result, precision: 2);
    }

    [Fact]
    public void Short_Sdp_TextTarget_ReturnsString()
    {
        var r = AppDimensResolver.Instance;
        r.ResetForTesting();
        r.Initialize(new AppDimensOptions { ScalingMode = ScalingMode.Continuous, WarmupAspectRatio = false }, force: true);
        r.SetMetricsForTesting(411, 823, 2.0);

        var target = new TextTargetStub();
        var sp = new ServiceProviderStub(target);
        var result = new Markup.SdpExtension { Value = 16 }.ProvideValue(sp);

        Assert.IsType<string>(result);
        Assert.Equal(16.Sdp().ToString(System.Globalization.CultureInfo.InvariantCulture), result);
    }

    [Theory]
    [InlineData("FontSize=\"{ssp:14}\"", "FontSize=\"{ssp:Ssp Value=14}\"")]
    [InlineData("Text=\"{}{sdp:16}\"", "Text=\"{}{sdp:16}\"")]
    [InlineData("Width=\"{sdpRotate:30, Rotation=45}\"", "Width=\"{sdpRotate:SdpRotate Base=30, Rotation=45}\"")]
    [InlineData("FontSize=\"{scaled:14, Tablet=18}\"", "FontSize=\"{scaled:Scaled Value=14, Tablet=18}\"")]
    [InlineData("HeightRequest=\"{hdpPw:48}\"", "HeightRequest=\"{hdpPw:HdpPw Value=48}\"")]
    [InlineData("WidthRequest=\"{wdpLh:120}\"", "WidthRequest=\"{wdpLh:WdpLh Value=120}\"")]
    public void ExpandShortMarkup_RewritesToMauiSyntax(string input, string expected)
    {
        Assert.Equal(expected, Markup.ShortDimenMarkupExpander.Expand(input));
    }
}

public class SpDimenTests
{
    [Fact]
    public void SspPh_Portrait_UsesHeightAxis()
    {
        var r = AppDimensResolver.Instance;
        r.ResetForTesting();
        r.Initialize(new AppDimensOptions { ScalingMode = ScalingMode.Continuous, WarmupAspectRatio = false }, force: true);
        r.SetMetricsForTesting(360, 800, 2.0);

        var portrait = r.Ssp(16, InverterEngine.ForSdpPh());
        Assert.Equal(16 * 800.0 / 300.0, portrait, precision: 2);
    }

    [Fact]
    public void Sspa_DiffersFromSsp_WhenNotReferenceAr()
    {
        var r = AppDimensResolver.Instance;
        r.ResetForTesting();
        r.Initialize(new AppDimensOptions { ScalingMode = ScalingMode.Continuous, WarmupAspectRatio = false }, force: true);
        r.SetMetricsForTesting(480, 854, 2.0);

        Assert.NotEqual(r.Ssp(32), r.Sspa(32));
    }

    [Fact]
    public void Hsp_And_Wsp_UseCorrectAxis()
    {
        var r = AppDimensResolver.Instance;
        r.ResetForTesting();
        r.Initialize(new AppDimensOptions { ScalingMode = ScalingMode.Continuous, WarmupAspectRatio = false }, force: true);
        r.SetMetricsForTesting(411, 823, 2.0);

        Assert.Equal(16 * 823.0 / 300.0, r.Hsp(16), precision: 2);
        Assert.Equal(16 * 411.0 / 300.0, r.Wsp(16), precision: 2);
    }

    [Fact]
    public void SspRotate_Landscape_UsesRotationValue()
    {
        var r = AppDimensResolver.Instance;
        r.ResetForTesting();
        r.Initialize(new AppDimensOptions { ScalingMode = ScalingMode.Continuous, WarmupAspectRatio = false }, force: true);
        r.SetMetricsForTesting(800, 360, 2.0, ScreenOrientation.Landscape);

        var value = 16.SspRotate(20);
        Assert.Equal(20 * 360.0 / 300.0, value, precision: 2);
    }
}

public class AspectRatioFactorsTests
{
    [Fact]
    public void Sdpa_DiffersFromSdp_WhenNotReferenceAr()
    {
        var r = AppDimensResolver.Instance;
        r.ResetForTesting();
        r.Initialize(new AppDimensOptions { ScalingMode = ScalingMode.Continuous, WarmupAspectRatio = false }, force: true);
        r.SetMetricsForTesting(480, 854, 2.0);

        var sdp = r.Sdp(32);
        var sdpa = r.Sdpa(32);
        Assert.NotEqual(sdp, sdpa);
    }
}

file sealed class TextTargetStub : IProvideValueTarget
{
    public object TargetObject => new Label();
    public object TargetProperty => Label.TextProperty;
}

file sealed class SpacingTargetStub : IProvideValueTarget
{
    public object TargetObject => new VerticalStackLayout();
    public object TargetProperty => StackLayout.SpacingProperty;
}

file sealed class ThicknessTargetStub : IProvideValueTarget
{
    public object TargetObject => new VerticalStackLayout();
    public object TargetProperty => VerticalStackLayout.PaddingProperty;
}

file sealed class ServiceProviderStub(IProvideValueTarget target) : IServiceProvider
{
    public object? GetService(Type serviceType) =>
        serviceType == typeof(IProvideValueTarget) ? target : null;
}

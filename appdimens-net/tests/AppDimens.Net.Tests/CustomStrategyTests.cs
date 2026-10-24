using AppDimens.Net;
using AppDimens.Net.Code.Scaled;
using AppDimens.Net.Common;
using AppDimens.Net.Core;
using AppDimens.Net.Custom;
using AppDimens.Net.Testing;
using Xunit;

namespace AppDimens.Net.Tests;

/// <summary>Custom strategies: engine, JSON, free-form formulas, cross-platform fixture and core parity.</summary>
public class CustomStrategyTests : IDisposable
{
    private static readonly ScreenSnapshot Phone = new(411, 891, 411, 2.625f);
    private static readonly ScreenSnapshot Tablet = new(1280, 800, 800, 2f);

    public CustomStrategyTests() => ScalingStrategies.ResetForTesting();

    public void Dispose()
    {
        ScalingStrategies.ResetForTesting();
        CustomDimens.DeviceClassResolver = null;
    }

    // ── engine ──────────────────────────────────────────────────────────────

    [Fact]
    public void BuiltIns_MatchCoreMetrics()
    {
        foreach (var ctx in new[] { Fx.Phone(), Fx.Tablet(), Fx.Desktop() })
        {
            var m = DimenMetrics.From(ctx.Configuration);
            Assert.Equal(m.Scale, BuiltInStrategies.Scaled.Resolve(ctx).Factor, 5);
            Assert.Equal(m.PowerScale, BuiltInStrategies.Power.Resolve(ctx).Factor, 5);
            Assert.Equal(m.LogarithmicScale, BuiltInStrategies.Logarithmic.Resolve(ctx).Factor, 4);
            Assert.Equal(m.InterpolatedScale, BuiltInStrategies.Interpolated.Resolve(ctx).Factor, 5);
            Assert.Equal(m.DiagonalScale, BuiltInStrategies.Diagonal.Resolve(ctx).Factor, 4);
            Assert.Equal(m.PerimeterScale, BuiltInStrategies.Perimeter.Resolve(ctx).Factor, 5);
        }
    }

    [Fact]
    public void DefaultStrategy_EqualsSdp()
    {
        var ctx = Fx.Tablet();
        Assert.Equal(16.Sdp(ctx), 16.Cdp(ctx), 4);
        Assert.Equal(16.SdpPx(ctx), 16.CdpPx(ctx), 3);
    }

    [Fact]
    public void Rules_ApplyInOrder_LaterWins()
    {
        var s = ScalingStrategy.Define("rules", b =>
        {
            b.On(DeviceClass.Tablet, r => { r.Curve = ScalingCurve.Power(0.5f); r.Multiplier = 0.9f; }, "tab");
            b.Landscape(r => r.Multiply(0.8f));
        }, register: false);
        var r = s.Resolve(Tablet);
        Assert.Equal(new[] { "tab", "landscape" }, r.AppliedRules);
        Assert.Equal(MathF.Pow(800f / 300f, 0.5f) * 0.8f, r.Factor, 5);
        Assert.Equal(411f / 300f, s.Resolve(Phone).Factor, 5);
    }

    [Fact]
    public void Clamps_FontScale_AndValidation()
    {
        var s = ScalingStrategy.Define("c", b => { b.ClampValue(4, 20); b.ClampFontScale(1.3f); }, register: false);
        Assert.True(s.IsValueDependent);
        Assert.Equal(20f, s.Resolve(Phone).Scale(100));
        Assert.Equal(-20f, s.Resolve(Phone).Scale(-100));
        Assert.Equal(0f, s.Resolve(Phone).Scale(0));
        var big = new ScreenSnapshot(411, 891, fontScale: 2f);
        Assert.Equal(10f * (411f / 300f) * 1.3f, s.Resolve(big).ScaleSpDp(10), 4);
        Assert.Throws<ArgumentException>(() => ScalingStrategy.Define("x", b => b.Clamp(2, 1)));
        Assert.Throws<ArgumentException>(() => ScalingStrategy.Define(" ", _ => { }));
    }

    [Fact]
    public void Memo_And_CacheKeys()
    {
        var plain = ScalingStrategy.Define("plain", _ => { }, register: false);
        var clamped = ScalingStrategy.Define("clamped", b => b.ClampValue(max: 24), register: false);
        Assert.Same(plain.Resolve(Phone), plain.Resolve(new ScreenSnapshot(411, 891, 411, 2.625f)));
        Assert.Equal(0L, plain.CacheKey(16f, DimenCache.ValueType.Dp) & 0x4L);
        Assert.Equal(0x4L, clamped.CacheKey(16f, DimenCache.ValueType.Dp) & 0x4L);
        Assert.True(DimenCache.ShouldComputeDirectly(plain.CacheKey(16f, DimenCache.ValueType.Dp)));
        Assert.NotEqual(plain.CacheKey(16f, DimenCache.ValueType.Dp), clamped.CacheKey(16f, DimenCache.ValueType.Dp));
        var ctx = Fx.Phone();
        Assert.Equal(16f * 360f / 300f, 16.Cdp(plain, ctx), 4);
        Assert.Equal(19.2f, 16.Cdp(clamped, ctx), 4);
        Assert.Equal(24f, 100.Cdp(clamped, ctx), 4);
        Assert.Equal(24f, 100.Cdp(clamped, ctx), 4); // cache hit path
    }

    [Fact]
    public void DeviceClass_FromUiMode_AndResolver()
    {
        var tv = Fx.Desktop();
        tv.UiModeType = UiModeType.Television;
        Assert.Equal(DeviceClass.Tv, tv.ScreenSnapshot().Device);
        Assert.Equal(new[] { "tv" }, CustomPresets.TvSafe.Resolve(tv).AppliedRules);
        CustomDimens.DeviceClassResolver = _ => DeviceClass.Watch;
        Assert.Equal(DeviceClass.Watch, Fx.Phone().ScreenSnapshot().Device);
        var mw = Fx.FoldableMultiWindow();
        Assert.True(mw.ScreenSnapshot().IsMultiWindow);
    }

    [Fact]
    public void Scope_OverridesDefault()
    {
        var brand = ScalingStrategy.Define("brand", b => b.Curve = ScalingCurve.Constant(2f));
        var ctx = Fx.Phone();
        using (CustomDimens.Use(brand)) Assert.Equal(32f, 16.Cdp(ctx), 4);
        Assert.Equal(16f * 1.2f, 16.Cdp(ctx), 4);
        ScalingStrategies.Default = brand;
        Assert.Equal(32f, 16.Cdp(ctx), 4);
    }

    [Fact]
    public void Extend_WithAxis_Explain()
    {
        var soft = BuiltInStrategies.Power.Extend("soft", b => b.Curve = ScalingCurve.Power(0.6f));
        Assert.Same(soft, ScalingStrategies.Get("soft"));
        Assert.Same(soft.WithAxis(ScalingAxis.Height), soft.WithAxis(ScalingAxis.Height));
        Assert.Equal("soft@height", soft.WithAxis(ScalingAxis.Height).Id);
        Assert.Contains("[soft]", soft.Explain(Phone));
    }

    // ── JSON ────────────────────────────────────────────────────────────────

    [Fact]
    public void Json_LoadExtendsDefault_AndRoundTrip()
    {
        var loaded = ScalingStrategies.LoadJson("""
            {"version": 1, "default": "brand", "strategies": [
              {"id": "brand", "extends": "tabletFriendly", "curve": {"type": "power", "exponent": 0.8}},
              {"id": "text", "extends": "brand", "fontScale": "ignore", "valueClamp": {"min": 10}},
              {"id": "free", "curve": {"type": "expression", "expression": "ratio * (tablet ? 0.9 : 1)"}, "formula": "max(scaled, 12)"}
            ]}
            """);
        Assert.Same(ScalingStrategies.Default, loaded["brand"]);
        Assert.Single(loaded["text"].Config.Rules);
        foreach (var s in CustomPresets.All.Concat(BuiltInStrategies.All).Concat(loaded.Values))
        {
            var back = ScalingStrategyJson.DecodeStrategy(s.ToJson());
            Assert.Equal(s.Resolve(Tablet).Scale(4f), back.Resolve(Tablet).Scale(4f), 5);
        }
    }

    [Theory]
    [InlineData("""[{"id":"a","extends":"b"},{"id":"b","extends":"a"}]""", "Cyclic")]
    [InlineData("""{"id":"a","curve":{"type":"wave"}}""", "type 'wave'")]
    [InlineData("""{"id":"a","extends":"nope"}""", "unknown strategy 'nope'")]
    [InlineData("""{"id": a}""", "Invalid JSON")]
    [InlineData("""{"id":"x","curve":{"type":"expression","expression":"base"}}""", "x.curve.expression: Invalid expression")]
    [InlineData("""{"id":"x","formula":"foo"}""", "unknown variable 'foo'")]
    public void Json_ErrorsAreDescriptive(string json, string expected)
    {
        var e = Assert.ThrowsAny<Exception>(() => ScalingStrategyJson.Decode(json));
        Assert.Contains(expected, e.Message);
    }

    // ── free-form formulas ──────────────────────────────────────────────────

    private static double Eval(string src, ScreenSnapshot? screen = null) =>
        ScalingExpression.Parse(src).Evaluate(
            ScalingExpression.Variables(screen ?? Phone, 1.5f, 411f, 300f, ReferenceScreen.Default, 16f, 20f, 1.25f));

    [Theory]
    [InlineData("1 + 2 * 3", 7)]
    [InlineData("(1 + 2) * 3", 9)]
    [InlineData("2 ^ 3 ^ 2", 512)]
    [InlineData("-2 ^ 2", -4)]
    [InlineData("-7 % 3", -1)]
    [InlineData("1 < 2 && 2 <= 2 && !(3 == 4) && 3 != 4", 1)]
    [InlineData("0 || 5", 1)]
    [InlineData("0 ? 2 : 1 ? 3 : 4", 3)]
    [InlineData("true && !false", 1)]
    [InlineData("1.5e3", 1500)]
    [InlineData("min(3, 1, 2) + max(3, 1, 2)", 4)]
    [InlineData("clamp(5, 0, 2)", 2)]
    [InlineData("round(2.5) + round(-2.5)", 1)]
    [InlineData("smoothstep(0, 10, 5)", 0.5)]
    [InlineData("step(1, 1) + lerp(10, 20, 0.5)", 16)]
    [InlineData("sw", 411)]
    [InlineData("phone && portrait && !landscape && !tablet", 1)]
    [InlineData("base + scaled + ratio", 37.5)]
    public void Expression_Evaluates(string src, double expected) => Assert.Equal(expected, Eval(src), 9);

    [Theory]
    [InlineData("sw +", "unexpected end")]
    [InlineData("foo * 2", "unknown variable 'foo'")]
    [InlineData("wave(1)", "unknown function 'wave'")]
    [InlineData("clamp(1, 2)", "'clamp' takes 3")]
    [InlineData("(1 + 2", "expected ')'")]
    [InlineData("1 ? 2", "expected ':'")]
    [InlineData("", "empty")]
    public void Expression_ParseErrors(string src, string expected) =>
        Assert.Contains(expected, Assert.Throws<ArgumentException>(() => ScalingExpression.Parse(src)).Message);

    [Fact]
    public void Expression_CurveIsFactorOnly_FormulaIsValueDependent()
    {
        var curve = ScalingStrategy.Define("expr", b => b.Curve = ScalingCurve.Expression("pow(ratio, 0.75)"), register: false);
        Assert.False(curve.IsValueDependent);
        Assert.Equal(BuiltInStrategies.Power.Resolve(Tablet).Factor, curve.Resolve(Tablet).Factor, 5);
        Assert.Throws<ArgumentException>(() => ScalingCurve.Expression("base"));

        var free = ScalingStrategy.Define("free", b => b.SetFormula("base * pow(sw / 360, 0.7) + 2"), register: false);
        Assert.True(free.IsValueDependent);
        Assert.Equal(16 * Math.Pow(800.0 / 360, 0.7) + 2, free.Resolve(Tablet).Scale(16), 3);

        var bad = ScalingStrategy.Define("bad", b => b.Curve = ScalingCurve.Expression("1 / (sw - sw)"), register: false);
        Assert.Throws<InvalidOperationException>(() => bad.Resolve(Phone));
    }

    [Fact]
    public void Json_RangeConditionMustBeObject()
    {
        var ex = Assert.Throws<ArgumentException>(() => ScalingStrategyJson.Decode(
            """{"strategies":[{"id":"x","rules":[{"when":{"width":600},"set":{"multiplier":2}}]}]}"""));
        Assert.Contains("must be an object", ex.Message);
    }

    [Fact]
    public void IgnoreMultiWindow_SkipsFormula()
    {
        var s = ScalingStrategy.Define("mwf", b => { b.IgnoreMultiWindow = true; b.SetFormula("base * 3"); }, register: false);
        Assert.Equal(16f, s.Resolve(new ScreenSnapshot(411, 891, isMultiWindow: true)).Scale(16));
        Assert.Equal(48f, s.Resolve(new ScreenSnapshot(411, 891)).Scale(16));
    }

    [Fact]
    public void ReviewFixes_MalformedJsonFailsLoudly_AndLoadIsAtomic()
    {
        Assert.Contains("when must be an object", Assert.Throws<ArgumentException>(() => ScalingStrategies.LoadJson(
            """{"strategies":[{"id":"x","rules":[{"when":"tablet","set":{"multiplier":2}}]}]}""")).Message);
        Assert.Contains("must be a boolean", Assert.Throws<ArgumentException>(() => ScalingStrategies.LoadJson(
            """{"strategies":[{"id":"x","ignoreMultiWindow":"true"}]}""")).Message);
        Assert.Contains("clamp must be an object", Assert.Throws<ArgumentException>(() => ScalingStrategies.LoadJson(
            """{"strategies":[{"id":"x","clamp":1.5}]}""")).Message);
        Assert.Contains("maxValue must be >= 0", Assert.Throws<ArgumentException>(() =>
            ScalingStrategy.Define("neg", b => b.ClampValue(max: -1), register: false)).Message);
        Assert.Contains("missing", Assert.Throws<KeyNotFoundException>(() => ScalingStrategies.LoadJson(
            """{"default":"missing","strategies":[{"id":"atomic","multiplier":2}]}""")).Message);
        Assert.Null(ScalingStrategies.Get("atomic")); // nothing registered by the failed load

        // WithAxis is cached per source instance, not per id.
        var a = ScalingStrategy.Define("same", b => b.Multiply(1f), register: false);
        var b2 = ScalingStrategy.Define("same", b => b.Multiply(2f), register: false);
        Assert.Equal(1f, a.WithAxis(ScalingAxis.Height).Config.Multiplier);
        Assert.Equal(2f, b2.WithAxis(ScalingAxis.Height).Config.Multiplier);
        Assert.Same(b2.WithAxis(ScalingAxis.Height), b2.WithAxis(ScalingAxis.Height));

        // A sensitivity-only rule patch survives export + import.
        var s = ScalingStrategies.LoadJson(
            """{"strategies":[{"id":"sens","aspectRatio":true,"rules":[{"when":{"device":"tablet"},"set":{"sensitivity":0.01}}]}]}""")["sens"];
        var again = ScalingStrategies.LoadJson("{\"strategies\":[" + s.ToJson() + "]}")["sens"];
        Assert.Equal(s.Factor(Tablet), again.Factor(Tablet));
        Assert.NotEqual(ScalingStrategy.Define("noSens", b => b.EnableAspectRatio(), register: false).Factor(Tablet), s.Factor(Tablet));
    }

    // ── cross-platform fixture ──────────────────────────────────────────────

    [Fact]
    public void CrossPlatformFixture()
    {
        var text = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "custom-strategy-fixture.json"));
        var root = (Dictionary<string, object?>)new MiniJson(text).Parse()!;
        var tolerance = (double)root["tolerance"]!;
        var start = text.IndexOf("\"document\":", StringComparison.Ordinal) + "\"document\":".Length;
        var end = text.IndexOf(",\n  \"screens\"", StringComparison.Ordinal);
        var strategies = ScalingStrategyJson.Decode(text.Substring(start, end - start)).Strategies;
        var screens = ((Dictionary<string, object?>)root["screens"]!).ToDictionary(kv => kv.Key, kv =>
        {
            var s = (Dictionary<string, object?>)kv.Value!;
            return new ScreenSnapshot((float)(double)s["width"]!, (float)(double)s["height"]!, (float)(double)s["smallestWidth"]!,
                (float)(double)s["density"]!, (float)(double)s["fontScale"]!, (bool)s["landscape"]!, (bool)s["multiWindow"]!,
                CustomNames.DeviceFromJson((string)s["device"]!));
        });
        var cases = (List<object?>)root["cases"]!;
        Assert.True(cases.Count > 500);
        foreach (Dictionary<string, object?> c in cases)
        {
            var r = strategies[(string)c["strategy"]!].Resolve(screens[(string)c["screen"]!]);
            var b = (float)(double)c["base"]!;
            var label = $"{c["strategy"]}@{c["screen"]}x{b}";
            foreach (var (expected, actual) in new[] { ((double)c["factor"]!, r.Factor), ((double)c["dp"]!, r.Scale(b)), ((double)c["spDp"]!, r.ScaleSpDp(b)) })
                Assert.True(Math.Abs(expected - actual) <= tolerance * Math.Max(1, Math.Abs(expected)), $"{label}: expected {expected}, got {actual}");
            Assert.Equal(((List<object?>)c["rules"]!).Cast<string>(), r.AppliedRules);
        }
    }
}

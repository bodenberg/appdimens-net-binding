using AppDimens.Net.Core;
using AppDimens.Net.Custom;
using Xunit;

namespace AppDimens.Net.Tests;

/// <summary>"Just write the formula": text and code one-liners, the proportional fast path and inline helpers.</summary>
public class ScalingFormulaTests : IDisposable
{
    private static readonly ScreenSnapshot Phone = new(411, 891, 411, 2.625f);
    private static readonly ScreenSnapshot Tablet = new(1280, 800, 800, 2f, isLandscape: true, device: DeviceClass.Tablet);

    public ScalingFormulaTests() => ScalingStrategies.ResetForTesting();

    public void Dispose() => ScalingStrategies.ResetForTesting();

    [Fact]
    public void TextFormula_IsTheFinalValue()
    {
        var s = ScalingStrategy.Formula("base * pow(sw / 360, 0.8)");
        Assert.Equal((float)(16.0 * Math.Pow(411.0 / 360.0, 0.8)), s.Scale(16f, Phone));
        Assert.Equal(18f, ScalingStrategy.Formula("base + 2").Scale(16f, Phone));
        Assert.Equal(16f * 411f / 300f, ScalingStrategy.Formula("base * ratio").Scale(16f, Phone), 4); // ratio = sw / 300, like Sdp
        Assert.Equal(16f * 0.9f, ScalingStrategy.Formula("base * (tablet ? 0.9 : 1)").Scale(16f, Tablet), 5);
    }

    [Theory]
    [InlineData("base")]
    [InlineData("base * pow(sw / 360, 0.8)")]
    [InlineData("(sw / 360) * base")]
    [InlineData("base * (tablet ? 0.9 : 1.1)")]
    [InlineData("base * (ratio + axis / ref)")]
    public void ProportionalFormula_StaysOnTheFastPath_AndMatchesTheSlowPathBitForBit(string src)
    {
        var fast = ScalingStrategy.Formula(src);
        Assert.False(fast.IsValueDependent, $"{src} should be screen-only");
        Assert.True(fast.Config.Formula!.IsProportional);
        // Same text, but not proportional (`+ 0` forces the per-value path): identical numbers.
        var slow = ScalingStrategy.Formula(src + " + 0");
        Assert.True(slow.IsValueDependent);
        foreach (var screen in new[] { Phone, Tablet, new ScreenSnapshot(333.3f, 712.7f, density: 1.75f) })
            foreach (var b in new[] { 1f, 16f, 48f, -8f, 0.5f, 13.37f, 0f })
            {
                Assert.Equal(BitConverter.SingleToInt32Bits(slow.Scale(b, screen)), BitConverter.SingleToInt32Bits(fast.Scale(b, screen)));
                Assert.Equal(slow.Resolve(screen).ScaleSpDp(b), fast.Resolve(screen).ScaleSpDp(b));
            }
    }

    [Theory]
    [InlineData("base * min(base, 20)")]
    [InlineData("max(base * sw / 360, base)")]
    [InlineData("base * sw / 360")] // (base * sw) / 360: evaluated as written, per value
    [InlineData("base * scaled")]
    [InlineData("factor * base")]
    [InlineData("base * base")]
    public void NonProportionalFormula_IsValueDependent(string src)
    {
        var s = ScalingStrategy.Formula(src);
        Assert.True(s.IsValueDependent);
        Assert.False(s.Config.Formula!.IsProportional);
    }

    [Fact]
    public void ProportionalFormula_InsideTheBuilderAndJson_IsScreenOnlyToo()
    {
        Assert.False(ScalingStrategy.Define("p", b => b.SetFormula("base * (sw / 360)"), register: false).IsValueDependent);
        var json = ScalingStrategies.LoadJson("""{"strategies":[{"id":"j","formula":"base * 1.1"}]}""")["j"];
        Assert.False(json.IsValueDependent);
        Assert.Equal(16f * 1.1f, json.Scale(16f, Phone), 5);
        Assert.True(ScalingStrategy.Define("v", b => b.SetFormula("round(scaled)"), register: false).IsValueDependent);
        // A value clamp still makes it value-dependent; so does a rule patch with a non-proportional formula.
        Assert.True(ScalingStrategy.Define("c", b => { b.SetFormula("base * 2"); b.ClampValue(max: 20); }, register: false).IsValueDependent);
        Assert.True(ScalingStrategy.Define("r", b => b.On(DeviceClass.Tablet, r => r.SetFormula("base + 1")), register: false).IsValueDependent);
        Assert.False(ScalingStrategy.Define("rp", b => b.On(DeviceClass.Tablet, r => r.SetFormula("base * 2")), register: false).IsValueDependent);
    }

    [Fact]
    public void ProportionalFormula_UsesTheNonCacheableKey()
    {
        var fast = ScalingStrategies.Formula("base * (sw / 360)");
        var slow = ScalingStrategies.Formula("base * sw / 360");
        Assert.Equal(0L, fast.CacheKey(16f, DimenCache.ValueType.Dp) & 0x4L);
        Assert.True(DimenCache.ShouldComputeDirectly(fast.CacheKey(16f, DimenCache.ValueType.Dp)));
        Assert.Equal(0x4L, slow.CacheKey(16f, DimenCache.ValueType.Dp) & 0x4L);
    }

    [Fact]
    public void TextFormula_IsSharedPerText_UnlessIdIsGiven()
    {
        Assert.Same(ScalingStrategy.Formula("base * 2"), ScalingStrategy.Formula("base * 2"));
        Assert.Same(ScalingStrategies.Formula("base * 2"), ScalingStrategy.Formula("base * 2"));
        Assert.Same(CustomDimens.Formula("base * 2"), ScalingStrategy.Formula("base * 2"));
        Assert.Equal("formula:base * 2", ScalingStrategy.Formula("base * 2").Id);
        Assert.Null(ScalingStrategies.Get("formula:base * 2")); // shared ones are not registered
        var named = ScalingStrategy.Formula("base * 2", id: "double");
        Assert.NotSame(named, ScalingStrategy.Formula("base * 2"));
        Assert.Same(named, ScalingStrategies.Get("double")); // an id registers by default
        Assert.Null(ScalingStrategies.Get(ScalingStrategy.Formula("base * 3", id: "triple", register: false).Id));
    }

    [Fact]
    public void SharedCache_IsBounded_AndThreadSafe()
    {
        var all = Enumerable.Range(0, 300).AsParallel().Select(i => ScalingStrategies.Formula($"base * {i}")).ToArray();
        Assert.Equal(300, all.Length);
        Assert.Equal(64f, ScalingStrategies.Formula("base * 4").Scale(16f, Phone));
        var cached = Enumerable.Range(0, 300).Count(i => ReferenceEquals(ScalingStrategies.Formula($"base * {i}"), ScalingStrategies.Formula($"base * {i}")));
        Assert.Equal(256, cached);
        Assert.Equal(16f * 299f, ScalingStrategies.Formula("base * 299").Scale(16f, Phone)); // past the bound: still correct
    }

    [Fact]
    public void CodeFormula_HasNoLimits()
    {
        var s = ScalingStrategy.Formula(x => x.Base * x.Pow(x.Sw / 360f, 0.8f) + (x.Landscape ? 2f : 0f));
        Assert.Equal(16f * MathF.Pow(411f / 360f, 0.8f), s.Scale(16f, Phone), 5);
        Assert.Equal(16f * MathF.Pow(800f / 360f, 0.8f) + 2f, s.Scale(16f, Tablet), 4);
        Assert.True(s.IsValueDependent);
        Assert.StartsWith("formula#", s.Id);
        Assert.NotEqual(s.Id, CustomDimens.Formula(x => x.Base).Id);
        var named = ScalingStrategy.Formula(x => x.Base * 3f, id: "hud");
        Assert.Same(named, ScalingStrategies.Get("hud"));
        Assert.Throws<InvalidOperationException>(() => s.ToJson());
    }

    [Fact]
    public void ProgrammableFactor_IsScreenOnly_AndMultipliesEveryValue()
    {
        var product = CustomScaleStrategy.Create(x => x.Clamp(x.Width / 360f, 0.85f, 1.80f));
        Assert.False(product.IsValueDependent);
        Assert.Equal(411f / 360f, product.Factor(Phone), 6);
        Assert.Equal(1.8f, product.Factor(Tablet)); // 1280 / 360 clamped
        Assert.Equal(16f * 411f / 360f, product.Scale(16f, Phone), 5);
        Assert.StartsWith("scale#", product.Id);
        Assert.NotEqual(product.Id, CustomScaleStrategy.Create(_ => 1f).Id);
        Assert.True(DimenCache.ShouldComputeDirectly(product.CacheKey(16f, DimenCache.ValueType.Dp)));
        var named = CustomDimens.CustomScaleStrategy(_ => 1.25f, id: "product");
        Assert.Same(named, ScalingStrategies.Get("product"));
        Assert.Equal(20f, named.Scale(16f, Phone));
        var e = Assert.Throws<InvalidOperationException>(() => CustomScaleStrategy.Create(_ => float.NaN).Factor(Phone));
        Assert.Contains("non-finite", e.Message);
    }

    [Fact]
    public void CodeScope_MatchesTheTextLanguage()
    {
        // Same names, same numbers: the code form and the text form agree.
        var pairs = new (ScalingStrategy Code, string Text)[]
        {
            (ScalingStrategy.Formula(x => x.Base * x.Pow(x.Sw / 360f, 0.8f)), "base * pow(sw / 360, 0.8)"),
            (ScalingStrategy.Formula(x => x.Base * x.Ratio), "base * ratio"),
            (ScalingStrategy.Formula(x => x.Base * (x.Tablet ? 0.9f : 1f) * x.Clamp(x.Width / 360f, 0.85f, 1.6f)),
                "base * (tablet ? 0.9 : 1) * clamp(width / 360, 0.85, 1.6)"),
            (ScalingStrategy.Formula(x => x.Round(x.Base * x.Lerp(1f, x.AspectRatio, 0.1f))), "round(base * lerp(1, aspectRatio, 0.1))"),
            (ScalingStrategy.Formula(x => x.Base * x.Smoothstep(300f, 900f, x.Diagonal) + x.Sqrt(x.ShortSide) - x.Ln(x.LongSide)),
                "base * smoothstep(300, 900, diagonal) + sqrt(shortSide) - ln(longSide)"),
            (ScalingStrategy.Formula(x => x.Max(x.Base, x.Min(x.Perimeter / 100f, x.Exp(x.Log10(x.Density))))),
                "max(base, min(perimeter / 100, exp(log10(density))))"),
            (ScalingStrategy.Formula(x => x.Abs(-x.Base) * (x.Landscape && !x.Portrait ? 2f : 1f) * (x.Phone || x.Desktop ? 1f : 0.5f)),
                "abs(-base) * (landscape && !portrait ? 2 : 1) * (phone || desktop ? 1 : 0.5)"),
        };
        foreach (var (code, text) in pairs)
            foreach (var screen in new[] { Phone, Tablet })
                foreach (var b in new[] { 1f, 16f, 48f })
                    Assert.True(Math.Abs(ScalingStrategy.Formula(text).Scale(b, screen) - code.Scale(b, screen)) <= 1e-4f * Math.Max(1f, Math.Abs(code.Scale(b, screen))), text);
        var tvScreen = new ScreenSnapshot(960, 540, 540, 2f, isLandscape: true, device: DeviceClass.Tv);
        var factor = CustomScaleStrategy.Create(x => x.Tv ? x.Height / 540f : x.Ratio);
        Assert.Equal(1f, factor.Factor(tvScreen));
        Assert.Equal(411f / 300f, factor.Factor(Phone), 6);
    }

    [Fact]
    public void TextFormula_ErrorsAreReportedEarly()
    {
        var e = Assert.Throws<ArgumentException>(() => ScalingStrategy.Formula("base * sww"));
        Assert.Contains("unknown variable 'sww'", e.Message);
        Assert.Throws<ArgumentException>(() => 16.Cdp("base * sww", Fx.Phone()));
    }

    [Fact]
    public void FontScale_IsFollowedForText()
    {
        var big = new ScreenSnapshot(411, 891, 411, 2.625f, fontScale: 1.5f);
        Assert.Equal(16f * 2f * 1.5f, ScalingStrategy.Formula("base * 2").Resolve(big).ScaleSpDp(16f), 5);
    }

    [Fact]
    public void Explain_ShowsTheFormula()
    {
        Assert.EndsWith("then Formula(base * 2)", ScalingStrategy.Formula("base * 2").Explain(Phone));
        Assert.EndsWith("then transform", ScalingStrategy.Formula(x => x.Base).Explain(Phone));
    }

    [Fact]
    public void InlineHelpers_TakeTheFormulaText()
    {
        var ctx = Fx.Phone();
        var density = ctx.ScreenSnapshot().Density;
        Assert.Equal(32f, 16f.Cdp("base * 2", ctx), 4);
        Assert.Equal(32f, 16.Cdp("base * 2", ctx), 4);
        Assert.Equal(32f * density, 16f.CdpPx("base * 2", ctx), 3);
        Assert.Equal(32f * density, 16.CdpPx("base * 2", ctx), 3);
        Assert.Equal(16.Csp(ScalingStrategies.Formula("base * 2"), ctx), 16.Csp("base * 2", ctx));
        Assert.Equal(28f, 14f.Csp("base * 2", ctx), 4);
        Assert.Equal(28f, 14.CspDp("base * 2", ctx), 4);
        Assert.Equal(28f, 14f.CspDp("base * 2", ctx), 4);
        Assert.Equal(28f * density, 14.CspPx("base * 2", ctx), 3);
        Assert.Equal(28f * density, 14f.CspPx("base * 2", ctx), 3);
        // Per-value path through the shared cache (twice: miss, then hit).
        Assert.Equal(18f, 16.Cdp("base + 2", ctx), 4);
        Assert.Equal(18f, 16.Cdp("base + 2", ctx), 4);
    }
}

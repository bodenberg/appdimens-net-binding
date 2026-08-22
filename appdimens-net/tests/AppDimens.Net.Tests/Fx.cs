using AppDimens.Net;
using AppDimens.Net.Code.Scaled;
using AppDimens.Net.Core;
using AppDimens.Net.Testing;

using AppDimens.Net.Common;

namespace AppDimens.Net.Tests;

/// <summary>Shared helpers: contexts and independent formula recomputation.</summary>
internal static class Fx
{
    public static FakeAppDimensContext Phone() =>
        new(new ScreenConfiguration(360, 740, 360, 420, 1f,
            ScreenConfiguration.OrientationUndefined, 0));

    public static FakeAppDimensContext Tablet() =>
        new(new ScreenConfiguration(800, 1280, 800, 240, 1.3f,
            ScreenConfiguration.OrientationLandscape, 0));

    public static FakeAppDimensContext Desktop() =>
        new(new ScreenConfiguration(1366, 768, 768, 96, 1f,
            ScreenConfiguration.OrientationLandscape, 0));

    public static FakeAppDimensContext FoldableMultiWindow()
    {
        var ctx = new FakeAppDimensContext(new ScreenConfiguration(342, 740, 342, 420, 1f,
            ScreenConfiguration.OrientationPortrait, 0))
        { IsInMultiWindowMode = true };
        return ctx;
    }

    // ── Independent recomputation of the canonical formulas (KMP parity) ──

    public const float Inv = DimenCache.InvBaseRatio;         // 0.0033333334f
    public const float Adj = 0.10f / 30f;
    public const float Sens = 0.08f / 30f;

    public static float LogAr(ScreenConfiguration c)
    {
        var min = (float)Math.Min(c.ScreenWidthDp, c.ScreenHeightDp);
        var max = (float)Math.Max(c.ScreenWidthDp, c.ScreenHeightDp);
        var raw = min > 0 ? max / min : 1f;
        var nar = raw / 1.78f;
        if (!(float.IsFinite(nar) && nar > 0)) nar = 1f;
        return MathF.Log(nar);
    }

    public static float ExpectedScaledDp(float b, ScreenConfiguration c,
        DpQualifier q = DpQualifier.SmallWidth, bool ar = false, bool imw = false, float? k = null)
    {
        if (imw && HeuristicMw(c)) return b;
        var dim = q switch
        {
            DpQualifier.Height => (float)c.ScreenHeightDp,
            DpQualifier.Width => (float)c.ScreenWidthDp,
            _ => (float)c.SmallestScreenWidthDp,
        };
        if (!ar) return b * dim * Inv;
        var diff = dim - 300f;
        var adj = (k ?? Sens) * LogAr(c);
        return b * (1f + diff * (Adj + adj));
    }

    public static float ExpectedFluidDp(float b, ScreenConfiguration c)
    {
        var dim = (float)c.SmallestScreenWidthDp;
        const float minW = 320f, maxW = 768f;
        var r = dim <= minW ? b * 0.8f
            : dim >= maxW ? b * 1.2f
            : b * 0.8f + b * 0.4f * (dim - minW) / (maxW - minW);
        return r;
    }

    public static float ExpectedAutoDp(float b, ScreenConfiguration c)
    {
        var dim = (float)c.SmallestScreenWidthDp;
        const float t = 480f, s = 0.4f;
        var scale = dim <= t ? dim * Inv : t * Inv + s * MathF.Log(1f + (dim - t) * Inv);
        return b * scale;
    }

    public static bool HeuristicMw(ScreenConfiguration c)
    {
        var sw = (float)c.SmallestScreenWidthDp;
        if (sw <= 0) return false;
        return sw - c.ScreenWidthDp >= sw * 0.1f;
    }
}

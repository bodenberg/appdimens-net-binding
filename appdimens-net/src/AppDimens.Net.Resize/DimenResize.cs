using AppDimens.Net.Core;

namespace AppDimens.Net.Code.Resize;

/// <summary>
/// Resize strategy — picks the largest size in a min..max range that still fits an
/// available box (binary search over generated steps). Port of Kotlin <c>ResizeMath</c>.
/// </summary>
public static class ResizeMath
{
    private const int MaxResizeSteps = 4096;

    public static float[] BuildStepsPx(float minPx, float maxPx, float stepPx)
    {
        var lo = Math.Min(minPx, maxPx);
        var hi = Math.Max(minPx, maxPx);
        if (stepPx <= 0f) return [lo];
        var capacity = Math.Clamp((int)((hi - lo) / stepPx) + 2, 1, MaxResizeSteps);
        var buf = new float[capacity];
        var x = lo;
        var epsilon = stepPx * 1e-4f;
        var count = 0;
        while (x <= hi + epsilon && count < capacity)
        {
            buf[count++] = Math.Min(x, hi);
            x += stepPx;
        }
        if (count == 0) buf[count++] = lo;
        if (buf[count - 1] < hi - epsilon && count < capacity) buf[count++] = hi;
        return count == capacity ? buf : buf[..count];
    }

    public static float FindLargestFitting(float[] sortedSteps, Func<float, bool> fits)
    {
        if (sortedSteps.Length == 0) return 0f;
        if (sortedSteps.Length == 1) return fits(sortedSteps[0]) ? sortedSteps[0] : 0f;
        int left = 0, right = sortedSteps.Length - 1;
        var best = 0f;
        while (left <= right)
        {
            var mid = (left + right) >> 1;
            if (fits(sortedSteps[mid])) { best = sortedSteps[mid]; left = mid + 1; }
            else right = mid - 1;
        }
        return best;
    }
}

/// <summary>Immutable px range with precomputed steps.</summary>
public sealed class ResizeRangePx
{
    public ResizeRangePx(float minPx, float maxPx, float stepPx)
    {
        MinPx = minPx;
        MaxPx = maxPx;
        StepPx = stepPx;
        LowPx = Math.Min(minPx, maxPx);
        HighPx = Math.Max(minPx, maxPx);
        Steps = ResizeMath.BuildStepsPx(LowPx, HighPx, stepPx);
    }

    public float MinPx { get; }
    public float MaxPx { get; }
    public float StepPx { get; }
    public float LowPx { get; }
    public float HighPx { get; }
    public float[] Steps { get; }

    public float ResolveFitting(Func<float, bool> fits) =>
        ResizeMath.FindLargestFitting(Steps, fits);
}

public enum AutoResizePercentBasis
{
    BoxWidth = 0,
    BoxHeight = 1,
    MinBoxSide = 2,
    MaxBoxSide = 3,
}

public static class DimenResize
{
    /// <summary>Inner available dimensions after padding.</summary>
    public static (float Width, float Height) InnerMaxDimensionsPx(
        float boxWidthPx, float boxHeightPx,
        float paddingLeftPx = 0f, float paddingRightPx = 0f,
        float paddingTopPx = 0f, float paddingBottomPx = 0f)
    {
        var innerW = Math.Max(boxWidthPx - paddingLeftPx - paddingRightPx, 1f);
        var innerH = Math.Max(boxHeightPx - paddingTopPx - paddingBottomPx, 1f);
        return (innerW, innerH);
    }

    public static float PercentOfBoxToFactor(float percent) =>
        Math.Clamp(percent / 100f, 0f, 1f);

    /// <summary>
    /// Largest text size in [minSp..maxSp] (stepped) whose estimated width still fits.
    /// Estimation uses average char width factor of the font size in px.
    /// </summary>
    public static float AutoResizeTextPx(string text, float boxWidthPx,
        float minSp, float maxSp, float stepSp, int maxLines,
        IAppDimensContext ctx, float avgCharWidthFactor = 0.55f)
    {
        if (string.IsNullOrEmpty(text)) return minSp * ctx.Density;
        var range = new ResizeRangePx(minSp, maxSp, stepSp <= 0f ? 1f : stepSp);
        var lines = maxLines is <= 0 ? int.MaxValue : maxLines;
        return range.ResolveFitting(candidatePx =>
        {
            var candidateTextPx = candidatePx * ctx.Density;
            var estLineWidth = text.Length * candidateTextPx * avgCharWidthFactor;
            var totalLines = Math.Max(1, (int)MathF.Ceiling(estLineWidth / Math.Max(boxWidthPx, 1f)));
            return totalLines <= lines && estLineWidth / lines <= boxWidthPx;
        });
    }

    /// <summary>Largest square side in [min..max] (dp) that fits the inner box (px input).</summary>
    public static float AutoResizeSquarePx(float boxWidthPx, float boxHeightPx,
        float minDp, float maxDp, float stepDp, IAppDimensContext ctx)
    {
        var (innerW, innerH) = InnerMaxDimensionsPx(boxWidthPx, boxHeightPx);
        // Convert the inner box once from px to dp; candidates are already dp
        // (KMP parity: side ≤ min(maxWidthDp, maxHeightDp)).
        var limitDp = Math.Min(innerW, innerH) / Math.Max(ctx.Density, 0.01f);
        var range = new ResizeRangePx(minDp, maxDp, stepDp <= 0f ? 1f : stepDp);
        return range.ResolveFitting(candidate => candidate <= limitDp);
    }

    public static float ResolveFixed(float valueDp, bool asSp, IAppDimensContext ctx) =>
        valueDp * ctx.Density / (asSp ? Math.Max(ctx.Configuration.FontScale, 0.01f) : 1f);
}

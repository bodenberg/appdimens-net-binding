namespace AppDimens.Maui.Core;

public sealed class AspectRatioFactors
{
    private readonly object _lock = new();
    private int _signature = int.MinValue;
    private float _arSw = 1f;
    private float _arW = 1f;
    private float _arH = 1f;

    public float For(DpQualifier qualifier) => qualifier switch
    {
        DpQualifier.Width => _arW,
        DpQualifier.Height => _arH,
        _ => _arSw,
    };

    public void EnsureUpToDate(ScreenMetricsSnapshot metrics, Func<DpQualifier, double> bucketOneUnitDp)
    {
        var sig = ComputeSignature(metrics);
        if (sig == _signature) return;

        lock (_lock)
        {
            if (sig == _signature) return;
            Rebuild(metrics, bucketOneUnitDp);
            _signature = sig;
        }
    }

    public void ResetForTests()
    {
        lock (_lock)
        {
            _signature = int.MinValue;
            _arSw = _arW = _arH = 1f;
        }
    }

    private static int ComputeSignature(ScreenMetricsSnapshot m) =>
        HashCode.Combine(
            (int)m.SmallestDp,
            (int)m.WidthDp,
            (int)m.HeightDp,
            m.DensityDpi);

    private void Rebuild(ScreenMetricsSnapshot metrics, Func<DpQualifier, double> bucketOneUnitDp)
    {
        var wDp = (float)Math.Max(metrics.WidthDp, 0);
        var hDp = (float)Math.Max(metrics.HeightDp, 0);
        var minDim = Math.Min(wDp, hDp);
        var maxDim = Math.Max(wDp, hDp);
        var rawAr = minDim > 0 ? maxDim / minDim : 1f;
        var normalizedAr = rawAr / AppDimensConstants.ReferenceAspectRatio;
        var logNormalizedAr = Math.Log(normalizedAr);

        _arSw = ComputeAxisAdjustment(metrics.SmallestDp, bucketOneUnitDp(DpQualifier.SmallWidth), logNormalizedAr);
        _arW = ComputeAxisAdjustment(metrics.WidthDp, bucketOneUnitDp(DpQualifier.Width), logNormalizedAr);
        _arH = ComputeAxisAdjustment(metrics.HeightDp, bucketOneUnitDp(DpQualifier.Height), logNormalizedAr);
    }

    private static float ComputeAxisAdjustment(double dimDp, double bucketOneUnitDp, double logNormalizedAr)
    {
        if (bucketOneUnitDp <= 0) return 1f;
        var bucketDp = bucketOneUnitDp * AppDimensConstants.DesignBaseDp;
        if (bucketDp <= 0) return 1f;
        var arMultiplier = 1f + (float)(dimDp - AppDimensConstants.DesignBaseDp)
            * (AppDimensConstants.AdjustmentScale + AppDimensConstants.SensitivityDefault * (float)logNormalizedAr);
        return arMultiplier * AppDimensConstants.DesignBaseDp / (float)bucketDp;
    }
}

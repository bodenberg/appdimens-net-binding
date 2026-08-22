namespace AppDimens.Net.Core;

/// <summary>
/// Immutable resolution snapshot. The constructor contains only the inputs that affect
/// a result, so equality is an exact cache-partition key; derived values are computed
/// once (bit-identical float math to the Kotlin/Android originals).
/// </summary>
public sealed class DimenMetrics : IEquatable<DimenMetrics>
{
    public DimenMetrics(
        int screenWidthDp, int screenHeightDp, int smallestScreenWidthDp,
        int densityDpi, int fontScaleBits, int orientation, int uiMode,
        bool isInMultiWindowMode)
    {
        ScreenWidthDp = screenWidthDp;
        ScreenHeightDp = screenHeightDp;
        SmallestScreenWidthDp = smallestScreenWidthDp;
        DensityDpi = densityDpi;
        FontScaleBits = fontScaleBits;
        Orientation = orientation;
        UiMode = uiMode;
        IsInMultiWindowMode = isInMultiWindowMode;

        var fs = BitConverter.Int32BitsToSingle(fontScaleBits);
        FontScale = float.IsFinite(fs) && fs > 0f ? fs : 1f;

        MinDimensionDp = Math.Max(Math.Min(screenWidthDp, screenHeightDp), 0);
        MaxDimensionDp = Math.Max(Math.Max(screenWidthDp, screenHeightDp), 0);

        float sw = smallestScreenWidthDp > 0
            ? smallestScreenWidthDp
            : MinDimensionDp > 0f ? MinDimensionDp : DesignScaleConstants.BaseWidthDp;
        SmallestWidthDp = sw;

        var d = densityDpi / 160f;
        Density = float.IsFinite(d) && d > 0f ? d : 1f;

        Scale = sw * DimenCache.InvBaseRatio;
        ScreenWidthFactor = screenWidthDp * DimenCache.InvBaseRatio;
        ScreenHeightFactor = screenHeightDp * DimenCache.InvBaseRatio;

        var rawAr = MinDimensionDp > 0f ? MaxDimensionDp / MinDimensionDp : 1f;
        var nar = rawAr / DesignScaleConstants.ReferenceAspectRatio;
        NormalizedAspectRatio = float.IsFinite(nar) && nar > 0f ? nar : 1f;
        LogNormalizedAspectRatio = MathF.Log(NormalizedAspectRatio);

        DefaultAspectRatioMultiplier = 1f + DimenCache.SensitivityDefault * LogNormalizedAspectRatio;
        DefaultScaledAspectRatioMultiplier = 1f + (sw - DesignScaleConstants.BaseWidthDp) *
            (DimenCache.AdjustmentScale + DimenCache.SensitivityDefault * LogNormalizedAspectRatio);
    }

    public static DimenMetrics From(ScreenConfiguration screen, bool isInMultiWindowMode = false) =>
        new(screen.ScreenWidthDp, screen.ScreenHeightDp, screen.SmallestScreenWidthDp,
            screen.DensityDpi, BitConverter.SingleToInt32Bits(screen.FontScale),
            screen.Orientation, screen.UiMode, isInMultiWindowMode);

    public int ScreenWidthDp { get; }
    public int ScreenHeightDp { get; }
    public int SmallestScreenWidthDp { get; }
    public int DensityDpi { get; }
    public int FontScaleBits { get; }
    public int Orientation { get; }
    public int UiMode { get; }
    public bool IsInMultiWindowMode { get; }

    /// <summary>Configured font scale normalized to a safe value.</summary>
    public float FontScale { get; }

    /// <summary>Current window bounds in dp.</summary>
    public float MinDimensionDp { get; }
    public float MaxDimensionDp { get; }
    public float SmallestWidthDp { get; }
    public float Density { get; }

    /// <summary>sw / 300 — the Scaled multiplier for the SMALL_WIDTH axis.</summary>
    public float Scale { get; }
    public float ScreenWidthFactor { get; }
    public float ScreenHeightFactor { get; }

    public float NormalizedAspectRatio { get; }
    public float LogNormalizedAspectRatio { get; }
    public float DefaultAspectRatioMultiplier { get; }
    public float DefaultScaledAspectRatioMultiplier { get; }

    // Satellite factors — computed at most once per snapshot, only when read.
    private float? _powerScale;
    public float PowerScale => _powerScale ??=
        MathF.Pow(SmallestWidthDp / DesignScaleConstants.BaseWidthDp, 0.75f);

    private float? _interpolatedScale;
    public float InterpolatedScale => _interpolatedScale ??=
        1f + (SmallestWidthDp * DimenCache.InvBaseRatio - 1f) * 0.5f;

    private float? _diagonalScale;
    public float DiagonalScale => _diagonalScale ??=
        MathF.Sqrt(MinDimensionDp * MinDimensionDp + MaxDimensionDp * MaxDimensionDp) /
        DesignScaleConstants.BaseDiagonalDp;

    private float? _perimeterScale;
    public float PerimeterScale => _perimeterScale ??=
        (MinDimensionDp + MaxDimensionDp) / DesignScaleConstants.BasePerimeterDp;

    private float? _logarithmicScale;
    public float LogarithmicScale => _logarithmicScale ??=
        SmallestWidthDp > DesignScaleConstants.BaseWidthDp
            ? 1f + 0.4f * MathF.Log(SmallestWidthDp * DimenCache.InvBaseRatio)
            : SmallestWidthDp > 0f
                ? 1f - 0.4f * MathF.Log(DesignScaleConstants.BaseWidthDp / SmallestWidthDp)
                : 1f;

    /// <summary>Multiplier used by the scaled SDP/SSP path; invalid K rejected.</summary>
    public float ScaledMultiplier(bool applyAspectRatio, float? customSensitivityK)
    {
        if (!applyAspectRatio) return Scale;
        if (customSensitivityK is null) return DefaultScaledAspectRatioMultiplier;
        if (!float.IsFinite(customSensitivityK.Value))
            throw new ArgumentException("customSensitivityK must be finite", nameof(customSensitivityK));
        var result = 1f + (SmallestWidthDp - DesignScaleConstants.BaseWidthDp) *
            (DimenCache.AdjustmentScale + customSensitivityK.Value * LogNormalizedAspectRatio);
        if (!float.IsFinite(result))
            throw new ArgumentException("customSensitivityK produces a non-finite dimension multiplier");
        return result;
    }

    /// <summary>Multiplier shared by satellite strategies applying AR after their base formula.</summary>
    public float AspectRatioMultiplier(float? customSensitivityK)
    {
        if (customSensitivityK is null) return DefaultAspectRatioMultiplier;
        if (!float.IsFinite(customSensitivityK.Value))
            throw new ArgumentException("customSensitivityK must be finite", nameof(customSensitivityK));
        var result = 1f + customSensitivityK.Value * LogNormalizedAspectRatio;
        if (!float.IsFinite(result))
            throw new ArgumentException("customSensitivityK produces a non-finite aspect-ratio multiplier");
        return result;
    }

    public bool Equals(DimenMetrics? other) => other is not null &&
        ScreenWidthDp == other.ScreenWidthDp &&
        ScreenHeightDp == other.ScreenHeightDp &&
        SmallestScreenWidthDp == other.SmallestScreenWidthDp &&
        DensityDpi == other.DensityDpi &&
        FontScaleBits == other.FontScaleBits &&
        Orientation == other.Orientation &&
        UiMode == other.UiMode &&
        IsInMultiWindowMode == other.IsInMultiWindowMode;

    public override bool Equals(object? obj) => Equals(obj as DimenMetrics);

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;
            hash = hash * 31 + ScreenWidthDp;
            hash = hash * 31 + ScreenHeightDp;
            hash = hash * 31 + SmallestScreenWidthDp;
            hash = hash * 31 + DensityDpi;
            hash = hash * 31 + FontScaleBits;
            hash = hash * 31 + Orientation;
            hash = hash * 31 + UiMode;
            hash = hash * 31 + (IsInMultiWindowMode ? 1 : 0);
            return hash;
        }
    }

    public static readonly DimenMetrics DefaultInstance =
        new(300, 533, 300, 160, BitConverter.SingleToInt32Bits(1f),
            ScreenConfiguration.OrientationUndefined, 0, false);
}

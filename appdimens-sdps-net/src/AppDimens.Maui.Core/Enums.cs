namespace AppDimens.Maui.Core;

public enum DpQualifier
{
    SmallWidth,
    Height,
    Width,
}

public enum ScreenOrientation
{
    Portrait,
    Landscape,
}

public enum DimenAxis
{
    Sdp,
    Hdp,
    Wdp,
    Ssp,
    Hsp,
    Wsp,
    Sdpa,
    Hdpa,
    Wdpa,
    Sem,
    Hem,
    Wem,
}

public enum ScalingMode
{
    Continuous,
    Bucket,
    Hybrid,
    HybridPreferContinuous,
}

public enum UiModeType
{
    Normal,
    Television,
    Car,
    Watch,
    Desk,
    Appliance,
    VrHeadset,
    Undefined,
}

public enum OrientationQualifier
{
    Default,
    Portrait,
    Landscape,
}

public enum UnitType
{
    Inch,
    Cm,
    Mm,
    Sp,
    Dp,
    Px,
}

public readonly record struct DpQualifierEntry(DpQualifier Type, int Value);

public readonly record struct ScreenMetricsSnapshot(
    double WidthDp,
    double HeightDp,
    double SmallestDp,
    double Density,
    int DensityDpi,
    ScreenOrientation Orientation,
    UiModeType UiMode);

public readonly record struct DimenCacheKey(
    int Index,
    DpQualifier Qualifier,
    InverterType Inverter,
    bool ApplyAspectRatio,
    bool ApplyFontScale,
    ScalingMode Mode);

public enum InverterType
{
    Default,
    PhToLw,
    PwToLh,
    LhToPw,
    LwToPh,
    SwToLh,
    SwToLw,
    SwToPh,
    SwToPw,
}

using AppDimens.Net.Common;
using AppDimens.Net.Core;

namespace AppDimens.Net.Code.Units;

/// <summary>
/// Physical units (mm / cm / inch) — approximate real-world size on screen,
/// density-based. Bit-parity with the Kotlin <c>DimenPhysicalUnits</c>.
/// </summary>
public static class DimenPhysicalUnits
{
    private const float MmToInchFactor = 25.4f;

    private static float DensityOf(IAppDimensContext? ctx) => ctx?.Density ?? 1f;
    private static float XdpiOf(IAppDimensContext? ctx) => ctx?.Xdpi ?? 160f;
    private static float FontScaleOf(IAppDimensContext? ctx) => ctx?.Configuration.FontScale ?? 1f;

    public static float ToDpFromMm(float mm, IAppDimensContext? ctx = null) =>
        mm * XdpiOf(ctx) / MmToInchFactor / DensityOf(ctx);

    public static float ToDpFromCm(float cm, IAppDimensContext? ctx = null) =>
        ToDpFromMm(cm * 10f, ctx);

    public static float ToDpFromInch(float inch, IAppDimensContext? ctx = null) =>
        inch * XdpiOf(ctx) / MmToInchFactor / DensityOf(ctx);

    public static float ToPxFromMm(float mm, IAppDimensContext? ctx = null) =>
        ToDpFromMm(mm, ctx) * DensityOf(ctx);

    public static float ToPxFromCm(float cm, IAppDimensContext? ctx = null) =>
        ToPxFromMm(cm * 10f, ctx);

    public static float ToPxFromInch(float inch, IAppDimensContext? ctx = null) =>
        ToDpFromInch(inch, ctx) * DensityOf(ctx);

    public static float ToSpFromMm(float mm, IAppDimensContext? ctx = null) =>
        ToPxFromMm(mm, ctx) / (DensityOf(ctx) * FontScaleOf(ctx));

    public static float ToSpFromCm(float cm, IAppDimensContext? ctx = null) =>
        ToPxFromCm(cm, ctx) / (DensityOf(ctx) * FontScaleOf(ctx));

    public static float ToSpFromInch(float inch, IAppDimensContext? ctx = null) =>
        ToPxFromInch(inch, ctx) / (DensityOf(ctx) * FontScaleOf(ctx));

    public static float RadiusFromDiameter(float diameter, UnitType unitType, IAppDimensContext? ctx = null)
    {
        var diameterInDp = unitType switch
        {
            UnitType.Mm => ToDpFromMm(diameter, ctx),
            UnitType.Cm => ToDpFromCm(diameter, ctx),
            UnitType.Inch => ToDpFromInch(diameter, ctx),
            UnitType.Dp => diameter,
            UnitType.Sp => diameter * FontScaleOf(ctx),
            UnitType.Px => diameter / DensityOf(ctx),
            _ => diameter,
        };
        return diameterInDp / 2f;
    }
}

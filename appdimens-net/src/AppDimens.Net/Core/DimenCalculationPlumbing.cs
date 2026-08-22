using AppDimens.Net.Common;

namespace AppDimens.Net.Core;

/// <summary>
/// Strategy-agnostic screen plumbing: inverter resolution, multi-window detection,
/// dp reads. Each strategy module applies its own formula on top of this.
/// </summary>
public static class DimenCalculationPlumbing
{
    public static DpQualifier EffectiveQualifier(DpQualifier qualifier, Inverter inverter, bool isLandscape, bool isPortrait)
    {
        var actual = qualifier;
        switch (inverter)
        {
            case Inverter.PhToLw when isLandscape && qualifier == DpQualifier.Height:
                actual = DpQualifier.Width; break;
            case Inverter.PwToLh when isLandscape && qualifier == DpQualifier.Width:
                actual = DpQualifier.Height; break;
            case Inverter.LhToPw when isPortrait && qualifier == DpQualifier.Height:
                actual = DpQualifier.Width; break;
            case Inverter.LwToPh when isPortrait && qualifier == DpQualifier.Width:
                actual = DpQualifier.Height; break;
            case Inverter.SwToLh when isLandscape && qualifier == DpQualifier.SmallWidth:
            case Inverter.SwToPh when isPortrait && qualifier == DpQualifier.SmallWidth:
                actual = DpQualifier.Height; break;
            case Inverter.SwToLw when isLandscape && qualifier == DpQualifier.SmallWidth:
            case Inverter.SwToPw when isPortrait && qualifier == DpQualifier.SmallWidth:
                actual = DpQualifier.Width; break;
        }
        return actual;
    }

    /// <summary>
    /// True when in multi-window mode AND the caller opted into suppressing scaling via
    /// <paramref name="ignoreMultiWindows"/>; heuristic fallback when no context.
    /// </summary>
    public static bool IsMultiWindowConstrained(ScreenConfiguration configuration, bool ignoreMultiWindows, IAppDimensContext? context = null)
    {
        if (!ignoreMultiWindows) return false;
        var mw = context?.IsInMultiWindowMode;
        if (mw.HasValue) return mw.Value;
        var swDp = (float)configuration.SmallestScreenWidthDp;
        if (swDp <= 0f) return false;
        var cwDp = (float)configuration.ScreenWidthDp;
        return swDp - cwDp >= swDp * 0.1f;
    }

    public static float ReadScreenDp(ScreenConfiguration configuration, DpQualifier actualQualifier) =>
        actualQualifier switch
        {
            DpQualifier.Height => configuration.ScreenHeightDp,
            DpQualifier.Width => configuration.ScreenWidthDp,
            _ => configuration.SmallestScreenWidthDp,
        };

    public static float SmallestSideDp(ScreenConfiguration configuration) =>
        Math.Min(configuration.ScreenWidthDp, configuration.ScreenHeightDp);

    public static float LargestSideDp(ScreenConfiguration configuration) =>
        Math.Max(configuration.ScreenWidthDp, configuration.ScreenHeightDp);

    /// <summary>Multiplicative factor for optional aspect-ratio correction.</summary>
    public static float AspectRatioMultiplier(ScreenConfiguration configuration, float sensitivity)
    {
        var sm = SmallestSideDp(configuration);
        var lg = LargestSideDp(configuration);
        if (sm <= 0f) return 1f;
        var ar = lg / sm;
        if (!float.IsFinite(ar)) return 1f;
        return 1f + sensitivity * MathF.Log(ar * DesignScaleConstants.InvReferenceAspectRatio);
    }
}

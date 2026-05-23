using AppDimens.Maui.Core;
using ResponsiveQualifierManager = AppDimens.Maui.Responsive.QualifierManager;

namespace AppDimens.Maui.Helpers;

public static class DimenFacilitators
{
    private static AppDimensResolver R => AppDimensResolver.Instance;

    // Layout dimensions (SDP, HDP, WDP)

    public static double SdpRotate(this int baseValue, int rotationValue,
        DpQualifier finalQualifier = DpQualifier.SmallWidth,
        OrientationQualifier orientation = OrientationQualifier.Landscape) =>
        Rotate(baseValue, rotationValue, v => R.Sdp(v), finalQualifier, orientation);

    public static double HdpRotate(this int baseValue, int rotationValue,
        DpQualifier finalQualifier = DpQualifier.Height,
        OrientationQualifier orientation = OrientationQualifier.Landscape) =>
        Rotate(baseValue, rotationValue, v => R.Hdp(v), finalQualifier, orientation);

    public static double WdpRotate(this int baseValue, int rotationValue,
        DpQualifier finalQualifier = DpQualifier.Width,
        OrientationQualifier orientation = OrientationQualifier.Landscape) =>
        Rotate(baseValue, rotationValue, v => R.Wdp(v), finalQualifier, orientation);

    public static double SdpQualifier(this int baseValue, int qualifiedValue,
        DpQualifier qualifierType, int qualifierThreshold,
        DpQualifier? finalQualifier = null) =>
        Qualifier(baseValue, qualifiedValue, qualifierType, qualifierThreshold, v => R.Sdp(v), finalQualifier ?? DpQualifier.SmallWidth);

    public static double SdpMode(this int baseValue, int modeValue, UiModeType uiMode,
        DpQualifier? finalQualifier = null) =>
        Mode(baseValue, modeValue, uiMode, v => R.Sdp(v), finalQualifier ?? DpQualifier.SmallWidth);

    public static double SdpScreen(this int baseValue, int screenValue, UiModeType uiMode,
        DpQualifier qualifierType, int qualifierThreshold, DpQualifier? finalQualifier = null) =>
        Screen(baseValue, screenValue, uiMode, qualifierType, qualifierThreshold, v => R.Sdp(v), finalQualifier ?? DpQualifier.SmallWidth);

    // Font-scaled dimensions (SSP, HSP, WSP)

    public static double SspRotate(this int baseValue, int rotationValue,
        DpQualifier finalQualifier = DpQualifier.SmallWidth,
        OrientationQualifier orientation = OrientationQualifier.Landscape) =>
        RotateSp(baseValue, rotationValue, v => R.Ssp(v), finalQualifier, orientation, applyFontScale: true);

    public static double HspRotate(this int baseValue, int rotationValue,
        DpQualifier finalQualifier = DpQualifier.Height,
        OrientationQualifier orientation = OrientationQualifier.Landscape) =>
        RotateSp(baseValue, rotationValue, v => R.Hsp(v), finalQualifier, orientation, applyFontScale: true);

    public static double WspRotate(this int baseValue, int rotationValue,
        DpQualifier finalQualifier = DpQualifier.Width,
        OrientationQualifier orientation = OrientationQualifier.Landscape) =>
        RotateSp(baseValue, rotationValue, v => R.Wsp(v), finalQualifier, orientation, applyFontScale: true);

    public static double SspQualifier(this int baseValue, int qualifiedValue,
        DpQualifier qualifierType, int qualifierThreshold,
        DpQualifier? finalQualifier = null) =>
        QualifierSp(baseValue, qualifiedValue, qualifierType, qualifierThreshold, v => R.Ssp(v), finalQualifier ?? DpQualifier.SmallWidth, applyFontScale: true);

    public static double HspQualifier(this int baseValue, int qualifiedValue,
        DpQualifier qualifierType, int qualifierThreshold,
        DpQualifier? finalQualifier = null) =>
        QualifierSp(baseValue, qualifiedValue, qualifierType, qualifierThreshold, v => R.Hsp(v), finalQualifier ?? DpQualifier.Height, applyFontScale: true);

    public static double WspQualifier(this int baseValue, int qualifiedValue,
        DpQualifier qualifierType, int qualifierThreshold,
        DpQualifier? finalQualifier = null) =>
        QualifierSp(baseValue, qualifiedValue, qualifierType, qualifierThreshold, v => R.Wsp(v), finalQualifier ?? DpQualifier.Width, applyFontScale: true);

    public static double SspMode(this int baseValue, int modeValue, UiModeType uiMode,
        DpQualifier? finalQualifier = null) =>
        ModeSp(baseValue, modeValue, uiMode, v => R.Ssp(v), finalQualifier ?? DpQualifier.SmallWidth, applyFontScale: true);

    public static double HspMode(this int baseValue, int modeValue, UiModeType uiMode,
        DpQualifier? finalQualifier = null) =>
        ModeSp(baseValue, modeValue, uiMode, v => R.Hsp(v), finalQualifier ?? DpQualifier.Height, applyFontScale: true);

    public static double WspMode(this int baseValue, int modeValue, UiModeType uiMode,
        DpQualifier? finalQualifier = null) =>
        ModeSp(baseValue, modeValue, uiMode, v => R.Wsp(v), finalQualifier ?? DpQualifier.Width, applyFontScale: true);

    public static double SspScreen(this int baseValue, int screenValue, UiModeType uiMode,
        DpQualifier qualifierType, int qualifierThreshold, DpQualifier? finalQualifier = null) =>
        ScreenSp(baseValue, screenValue, uiMode, qualifierType, qualifierThreshold, v => R.Ssp(v), finalQualifier ?? DpQualifier.SmallWidth, applyFontScale: true);

    public static double HspScreen(this int baseValue, int screenValue, UiModeType uiMode,
        DpQualifier qualifierType, int qualifierThreshold, DpQualifier? finalQualifier = null) =>
        ScreenSp(baseValue, screenValue, uiMode, qualifierType, qualifierThreshold, v => R.Hsp(v), finalQualifier ?? DpQualifier.Height, applyFontScale: true);

    public static double WspScreen(this int baseValue, int screenValue, UiModeType uiMode,
        DpQualifier qualifierType, int qualifierThreshold, DpQualifier? finalQualifier = null) =>
        ScreenSp(baseValue, screenValue, uiMode, qualifierType, qualifierThreshold, v => R.Wsp(v), finalQualifier ?? DpQualifier.Width, applyFontScale: true);

    // Em-scaled dimensions (SEM, HEM, WEM)

    public static double SemRotate(this int baseValue, int rotationValue,
        DpQualifier finalQualifier = DpQualifier.SmallWidth,
        OrientationQualifier orientation = OrientationQualifier.Landscape) =>
        RotateSp(baseValue, rotationValue, v => R.Sem(v), finalQualifier, orientation);

    public static double HemRotate(this int baseValue, int rotationValue,
        DpQualifier finalQualifier = DpQualifier.Height,
        OrientationQualifier orientation = OrientationQualifier.Landscape) =>
        RotateSp(baseValue, rotationValue, v => R.Hem(v), finalQualifier, orientation);

    public static double WemRotate(this int baseValue, int rotationValue,
        DpQualifier finalQualifier = DpQualifier.Width,
        OrientationQualifier orientation = OrientationQualifier.Landscape) =>
        RotateSp(baseValue, rotationValue, v => R.Wem(v), finalQualifier, orientation);

    public static double SemQualifier(this int baseValue, int qualifiedValue,
        DpQualifier qualifierType, int qualifierThreshold,
        DpQualifier? finalQualifier = null) =>
        QualifierSp(baseValue, qualifiedValue, qualifierType, qualifierThreshold, v => R.Sem(v), finalQualifier ?? DpQualifier.SmallWidth);

    public static double HemQualifier(this int baseValue, int qualifiedValue,
        DpQualifier qualifierType, int qualifierThreshold,
        DpQualifier? finalQualifier = null) =>
        QualifierSp(baseValue, qualifiedValue, qualifierType, qualifierThreshold, v => R.Hem(v), finalQualifier ?? DpQualifier.Height);

    public static double WemQualifier(this int baseValue, int qualifiedValue,
        DpQualifier qualifierType, int qualifierThreshold,
        DpQualifier? finalQualifier = null) =>
        QualifierSp(baseValue, qualifiedValue, qualifierType, qualifierThreshold, v => R.Wem(v), finalQualifier ?? DpQualifier.Width);

    public static double SemMode(this int baseValue, int modeValue, UiModeType uiMode,
        DpQualifier? finalQualifier = null) =>
        ModeSp(baseValue, modeValue, uiMode, v => R.Sem(v), finalQualifier ?? DpQualifier.SmallWidth);

    public static double HemMode(this int baseValue, int modeValue, UiModeType uiMode,
        DpQualifier? finalQualifier = null) =>
        ModeSp(baseValue, modeValue, uiMode, v => R.Hem(v), finalQualifier ?? DpQualifier.Height);

    public static double WemMode(this int baseValue, int modeValue, UiModeType uiMode,
        DpQualifier? finalQualifier = null) =>
        ModeSp(baseValue, modeValue, uiMode, v => R.Wem(v), finalQualifier ?? DpQualifier.Width);

    public static double SemScreen(this int baseValue, int screenValue, UiModeType uiMode,
        DpQualifier qualifierType, int qualifierThreshold, DpQualifier? finalQualifier = null) =>
        ScreenSp(baseValue, screenValue, uiMode, qualifierType, qualifierThreshold, v => R.Sem(v), finalQualifier ?? DpQualifier.SmallWidth);

    public static double HemScreen(this int baseValue, int screenValue, UiModeType uiMode,
        DpQualifier qualifierType, int qualifierThreshold, DpQualifier? finalQualifier = null) =>
        ScreenSp(baseValue, screenValue, uiMode, qualifierType, qualifierThreshold, v => R.Hem(v), finalQualifier ?? DpQualifier.Height);

    public static double WemScreen(this int baseValue, int screenValue, UiModeType uiMode,
        DpQualifier qualifierType, int qualifierThreshold, DpQualifier? finalQualifier = null) =>
        ScreenSp(baseValue, screenValue, uiMode, qualifierType, qualifierThreshold, v => R.Wem(v), finalQualifier ?? DpQualifier.Width);

    // Orientation, qualifier, and UI mode selection

    private static bool MatchesOrientation(OrientationQualifier orientation, ScreenMetricsSnapshot metrics) =>
        orientation switch
        {
            OrientationQualifier.Landscape => metrics.Orientation == ScreenOrientation.Landscape,
            OrientationQualifier.Portrait => metrics.Orientation == ScreenOrientation.Portrait,
            _ => false,
        };

    private static double Rotate(int baseValue, int rotationValue,
        Func<int, double> resolveBase, DpQualifier finalQualifier, OrientationQualifier orientation) =>
        MatchesOrientation(orientation, R.Metrics.Current)
            ? R.Resolve(rotationValue, finalQualifier)
            : resolveBase(baseValue);

    private static double RotateSp(int baseValue, int rotationValue,
        Func<int, double> resolveBase, DpQualifier finalQualifier, OrientationQualifier orientation,
        bool applyFontScale = false) =>
        MatchesOrientation(orientation, R.Metrics.Current)
            ? R.Resolve(rotationValue, finalQualifier, applyFontScale: applyFontScale, allowNegative: false)
            : resolveBase(baseValue);

    private static double Qualifier(int baseValue, int qualifiedValue,
        DpQualifier qualifierType, int qualifierThreshold,
        Func<int, double> resolveBase, DpQualifier resolveQualifier)
    {
        var metrics = R.Metrics.Current;
        var screenValue = ResponsiveQualifierManager.GetMetricForQualifier(qualifierType, metrics);
        return screenValue >= qualifierThreshold
            ? R.Resolve(qualifiedValue, resolveQualifier)
            : resolveBase(baseValue);
    }

    private static double QualifierSp(int baseValue, int qualifiedValue,
        DpQualifier qualifierType, int qualifierThreshold,
        Func<int, double> resolveBase, DpQualifier resolveQualifier, bool applyFontScale = false)
    {
        var metrics = R.Metrics.Current;
        var screenValue = ResponsiveQualifierManager.GetMetricForQualifier(qualifierType, metrics);
        return screenValue >= qualifierThreshold
            ? R.Resolve(qualifiedValue, resolveQualifier, applyFontScale: applyFontScale, allowNegative: false)
            : resolveBase(baseValue);
    }

    private static double Mode(int baseValue, int modeValue, UiModeType uiMode,
        Func<int, double> resolveBase, DpQualifier resolveQualifier) =>
        R.Metrics.Current.UiMode == uiMode
            ? R.Resolve(modeValue, resolveQualifier)
            : resolveBase(baseValue);

    private static double ModeSp(int baseValue, int modeValue, UiModeType uiMode,
        Func<int, double> resolveBase, DpQualifier resolveQualifier, bool applyFontScale = false) =>
        R.Metrics.Current.UiMode == uiMode
            ? R.Resolve(modeValue, resolveQualifier, applyFontScale: applyFontScale, allowNegative: false)
            : resolveBase(baseValue);

    private static double Screen(int baseValue, int screenValue, UiModeType uiMode,
        DpQualifier qualifierType, int qualifierThreshold,
        Func<int, double> resolveBase, DpQualifier resolveQualifier)
    {
        var metrics = R.Metrics.Current;
        var screenMetric = ResponsiveQualifierManager.GetMetricForQualifier(qualifierType, metrics);
        if (metrics.UiMode == uiMode && screenMetric >= qualifierThreshold)
            return R.Resolve(screenValue, resolveQualifier);
        return resolveBase(baseValue);
    }

    private static double ScreenSp(int baseValue, int screenValue, UiModeType uiMode,
        DpQualifier qualifierType, int qualifierThreshold,
        Func<int, double> resolveBase, DpQualifier resolveQualifier, bool applyFontScale = false)
    {
        var metrics = R.Metrics.Current;
        var screenMetric = ResponsiveQualifierManager.GetMetricForQualifier(qualifierType, metrics);
        if (metrics.UiMode == uiMode && screenMetric >= qualifierThreshold)
            return R.Resolve(screenValue, resolveQualifier, applyFontScale: applyFontScale, allowNegative: false);
        return resolveBase(baseValue);
    }
}

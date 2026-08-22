using AppDimens.Net.Common;
using AppDimens.Net.Core;

namespace AppDimens.Net.Code.Scaled;

/// <summary>
/// Scaled extension surface. Parameterless overloads resolve through the ambient
/// window context (<see cref="AppDimensAmbient"/>); overloads taking an explicit
/// <see cref="IAppDimensContext"/> mirror the Kotlin <c>code</c> API. Default results
/// are logical dp/sp units; <c>*Px</c> variants return raw pixels.
///
/// Suffixes: <c>a</c> aspect-ratio-aware curve · <c>i</c> ignore multi-window
/// heuristic (returns unscaled base when it triggers) · <c>ia</c> both.
/// </summary>
public static class ScaledExtensions
{
    // ───────────────────────── SDP — smallest-width axis ─────────────────────────

    public static float Sdp(this int v, IAppDimensContext ctx) =>
        DimenCache.CacheEnabled ? DimenCache.ResolveSdpDp(v, ctx) : v.ToDynamicScaledDp(ctx);
    public static float Sdpa(this int v, IAppDimensContext ctx) =>
        DimenCache.CacheEnabled ? DimenCache.ResolveSdpaDp(v, ctx) : v.ToDynamicScaledDp(ctx, applyAspectRatio: true);
    public static float Sdpi(this int v, IAppDimensContext ctx) => v.ToDynamicScaledDp(ctx, ignoreMultiWindows: true);
    public static float Sdpia(this int v, IAppDimensContext ctx) =>
        v.ToDynamicScaledDp(ctx, ignoreMultiWindows: true, applyAspectRatio: true);

    public static float SdpPx(this int v, IAppDimensContext ctx) =>
        DimenCache.CacheEnabled ? DimenCache.ResolveSdpPx(v, ctx) : v.ToDynamicScaledPx(ctx);
    public static float SdpaPx(this int v, IAppDimensContext ctx) =>
        DimenCache.CacheEnabled ? DimenCache.ResolveSdpaPx(v, ctx) : v.ToDynamicScaledPx(ctx, applyAspectRatio: true);
    public static float SdpiPx(this int v, IAppDimensContext ctx) => v.ToDynamicScaledPx(ctx, ignoreMultiWindows: true);
    public static float SdpiaPx(this int v, IAppDimensContext ctx) =>
        v.ToDynamicScaledPx(ctx, ignoreMultiWindows: true, applyAspectRatio: true);

    public static float Sdp(this int v) { var c = AppDimensAmbient.Require(); return DimenCache.CacheEnabled ? DimenCache.ResolveSdpDp(v, c) : v.ToDynamicScaledDp(c); }
    public static float Sdpa(this int v) { var c = AppDimensAmbient.Require(); return DimenCache.CacheEnabled ? DimenCache.ResolveSdpaDp(v, c) : v.ToDynamicScaledDp(c, applyAspectRatio: true); }
    public static float Sdpi(this int v) => v.ToDynamicScaledDp(AppDimensAmbient.Require(), ignoreMultiWindows: true);
    public static float Sdpia(this int v) => v.ToDynamicScaledDp(AppDimensAmbient.Require(), ignoreMultiWindows: true, applyAspectRatio: true);
    public static float SdpPx(this int v) { var c = AppDimensAmbient.Require(); return DimenCache.CacheEnabled ? DimenCache.ResolveSdpPx(v, c) : v.ToDynamicScaledPx(c); }
    public static float SdpaPx(this int v) { var c = AppDimensAmbient.Require(); return DimenCache.CacheEnabled ? DimenCache.ResolveSdpaPx(v, c) : v.ToDynamicScaledPx(c, applyAspectRatio: true); }

    public static float Sdp(this double v, IAppDimensContext ctx) => ((float)v).ToDynamicScaledDp(ctx);
    public static float Sdpa(this double v, IAppDimensContext ctx) => ((float)v).ToDynamicScaledDp(ctx, applyAspectRatio: true);
    public static float Sdp(this double v) => v.Sdp(AppDimensAmbient.Require());
    public static float Sdpa(this double v) => v.Sdpa(AppDimensAmbient.Require());

    // Orientation inverters for the SW axis: Ph (portrait→uses height), Lh (landscape
    //→height), Pw (portrait→width), Lw (landscape→width).
    public static float SdpPh(this int v, IAppDimensContext ctx, bool ar = false, bool imw = false, float? k = null) =>
        v.ToDynamicScaledDp(ctx, DpQualifier.SmallWidth, Inverter.PhToLw, imw, ar, k);
    public static float SdpLh(this int v, IAppDimensContext ctx, bool ar = false, bool imw = false, float? k = null) =>
        v.ToDynamicScaledDp(ctx, DpQualifier.SmallWidth, Inverter.LhToPw, imw, ar, k);
    public static float SdpPw(this int v, IAppDimensContext ctx, bool ar = false, bool imw = false, float? k = null) =>
        v.ToDynamicScaledDp(ctx, DpQualifier.SmallWidth, Inverter.PwToLh, imw, ar, k);
    public static float SdpLw(this int v, IAppDimensContext ctx, bool ar = false, bool imw = false, float? k = null) =>
        v.ToDynamicScaledDp(ctx, DpQualifier.SmallWidth, Inverter.SwToLw, imw, ar, k);

    /// <summary>Generic escape hatch covering every qualifier/inverter/flag combination.</summary>
    public static float SdpCustom(this int v, IAppDimensContext ctx,
        DpQualifier qualifier = DpQualifier.SmallWidth, Inverter inverter = Inverter.Default,
        bool ignoreMultiWindows = false, bool applyAspectRatio = false, float? customSensitivityK = null) =>
        v.ToDynamicScaledDp(ctx, qualifier, inverter, ignoreMultiWindows, applyAspectRatio, customSensitivityK);

    public static float SdpCustomPx(this int v, IAppDimensContext ctx,
        DpQualifier qualifier = DpQualifier.SmallWidth, Inverter inverter = Inverter.Default,
        bool ignoreMultiWindows = false, bool applyAspectRatio = false, float? customSensitivityK = null) =>
        v.ToDynamicScaledPx(ctx, qualifier, inverter, ignoreMultiWindows, applyAspectRatio, customSensitivityK);

    // ───────────────────────── HDP — height axis ─────────────────────────

    public static float Hdp(this int v, IAppDimensContext ctx) =>
        DimenCache.CacheEnabled ? DimenCache.ResolveHdpDp(v, ctx) : v.ToDynamicScaledDp(ctx, DpQualifier.Height);
    public static float Hdpa(this int v, IAppDimensContext ctx) => v.ToDynamicScaledDp(ctx, DpQualifier.Height, applyAspectRatio: true);
    public static float Hdpi(this int v, IAppDimensContext ctx) => v.ToDynamicScaledDp(ctx, DpQualifier.Height, ignoreMultiWindows: true);
    public static float Hdpia(this int v, IAppDimensContext ctx) => v.ToDynamicScaledDp(ctx, DpQualifier.Height, ignoreMultiWindows: true, applyAspectRatio: true);
    public static float HdpPx(this int v, IAppDimensContext ctx) =>
        DimenCache.CacheEnabled ? DimenCache.ResolveHdpPx(v, ctx) : v.ToDynamicScaledPx(ctx, DpQualifier.Height);
    public static float HdpaPx(this int v, IAppDimensContext ctx) => v.ToDynamicScaledPx(ctx, DpQualifier.Height, applyAspectRatio: true);

    public static float Hdp(this int v) { var c = AppDimensAmbient.Require(); return DimenCache.CacheEnabled ? DimenCache.ResolveHdpDp(v, c) : v.ToDynamicScaledDp(c, DpQualifier.Height); }
    public static float Hdpa(this int v) => v.ToDynamicScaledDp(AppDimensAmbient.Require(), DpQualifier.Height, applyAspectRatio: true);
    public static float Hdpi(this int v) => v.ToDynamicScaledDp(AppDimensAmbient.Require(), DpQualifier.Height, ignoreMultiWindows: true);
    public static float HdpPx(this int v) { var c = AppDimensAmbient.Require(); return DimenCache.CacheEnabled ? DimenCache.ResolveHdpPx(v, c) : v.ToDynamicScaledPx(c, DpQualifier.Height); }

    /// <summary>Height-based; in landscape uses width instead.</summary>
    public static float HdpLw(this int v, IAppDimensContext ctx, bool ar = false, bool imw = false, float? k = null) =>
        v.ToDynamicScaledDp(ctx, DpQualifier.Height, Inverter.PwToLh, imw, ar, k);
    public static float HdpLwa(this int v, IAppDimensContext ctx) => v.HdpLw(ctx, ar: true);
    public static float HdpLwi(this int v, IAppDimensContext ctx) => v.HdpLw(ctx, imw: true);
    public static float HdpLwia(this int v, IAppDimensContext ctx) => v.HdpLw(ctx, ar: true, imw: true);

    // ───────────────────────── WDP — width axis ─────────────────────────

    public static float Wdp(this int v, IAppDimensContext ctx) =>
        DimenCache.CacheEnabled ? DimenCache.ResolveWdpDp(v, ctx) : v.ToDynamicScaledDp(ctx, DpQualifier.Width);
    public static float Wdpa(this int v, IAppDimensContext ctx) => v.ToDynamicScaledDp(ctx, DpQualifier.Width, applyAspectRatio: true);
    public static float Wdpi(this int v, IAppDimensContext ctx) => v.ToDynamicScaledDp(ctx, DpQualifier.Width, ignoreMultiWindows: true);
    public static float Wdpia(this int v, IAppDimensContext ctx) => v.ToDynamicScaledDp(ctx, DpQualifier.Width, ignoreMultiWindows: true, applyAspectRatio: true);
    public static float WdpPx(this int v, IAppDimensContext ctx) =>
        DimenCache.CacheEnabled ? DimenCache.ResolveWdpPx(v, ctx) : v.ToDynamicScaledPx(ctx, DpQualifier.Width);
    public static float WdpaPx(this int v, IAppDimensContext ctx) => v.ToDynamicScaledPx(ctx, DpQualifier.Width, applyAspectRatio: true);

    public static float Wdp(this int v) { var c = AppDimensAmbient.Require(); return DimenCache.CacheEnabled ? DimenCache.ResolveWdpDp(v, c) : v.ToDynamicScaledDp(c, DpQualifier.Width); }
    public static float Wdpa(this int v) => v.ToDynamicScaledDp(AppDimensAmbient.Require(), DpQualifier.Width, applyAspectRatio: true);
    public static float Wdpi(this int v) => v.ToDynamicScaledDp(AppDimensAmbient.Require(), DpQualifier.Width, ignoreMultiWindows: true);
    public static float WdpPx(this int v) { var c = AppDimensAmbient.Require(); return DimenCache.CacheEnabled ? DimenCache.ResolveWdpPx(v, c) : v.ToDynamicScaledPx(c, DpQualifier.Width); }

    /// <summary>Width-based; in portrait uses height instead.</summary>
    public static float WdpLh(this int v, IAppDimensContext ctx, bool ar = false, bool imw = false, float? k = null) =>
        v.ToDynamicScaledDp(ctx, DpQualifier.Width, Inverter.LwToPh, imw, ar, k);
    public static float WdpLha(this int v, IAppDimensContext ctx) => v.WdpLh(ctx, ar: true);
    public static float WdpLhi(this int v, IAppDimensContext ctx) => v.WdpLh(ctx, imw: true);
    public static float WdpLhia(this int v, IAppDimensContext ctx) => v.WdpLh(ctx, ar: true, imw: true);

    // ───────────────────────── SSP — scalable text (sw axis) ─────────────────────────

    private static float SpFromPx(float px, IAppDimensContext ctx) =>
        px / (ctx.Density * Math.Max(ctx.Configuration.FontScale, 0.01f));

    public static float Ssp(this int v, IAppDimensContext ctx) => SpFromPx(v.SdpPx(ctx), ctx);
    public static float Sspa(this int v, IAppDimensContext ctx) => SpFromPx(v.SdpaPx(ctx), ctx);
    public static float Sspi(this int v, IAppDimensContext ctx) => SpFromPx(v.SdpiPx(ctx), ctx);
    public static float Sspia(this int v, IAppDimensContext ctx) => SpFromPx(v.SdpiaPx(ctx), ctx);
    public static float Ssp(this int v) => SpFromPx(v.SdpPx(AppDimensAmbient.Require()), AppDimensAmbient.Require());
    public static float Sspa(this int v) => SpFromPx(v.SdpaPx(AppDimensAmbient.Require()), AppDimensAmbient.Require());

    public static float Hsp(this int v, IAppDimensContext ctx) => SpFromPx(v.HdpPx(ctx), ctx);
    public static float Hspa(this int v, IAppDimensContext ctx) => SpFromPx(v.HdpaPx(ctx), ctx);
    public static float Wsp(this int v, IAppDimensContext ctx) => SpFromPx(v.WdpPx(ctx), ctx);
    public static float Wspa(this int v, IAppDimensContext ctx) => SpFromPx(v.WdpaPx(ctx), ctx);

    /// <summary>Ignores the system font scale (value equals the dp-scaled result).</summary>
    public static float Sem(this int v, IAppDimensContext ctx) => v.Sdp(ctx);
    public static float Hem(this int v, IAppDimensContext ctx) => v.Hdp(ctx);
    public static float Wem(this int v, IAppDimensContext ctx) => v.Wdp(ctx);
    public static float Sem(this int v) => v.Sdp();
    public static float Hem(this int v) => v.Hdp();
    public static float Wem(this int v) => v.Wdp();

    // ───────────────────────── Facilitators — rotate / mode / qualifier ─────────────────────────

    private static bool IsTargetOrientation(ScreenConfiguration cfg, Orientation orientation) => orientation switch
    {
        Orientation.Landscape => cfg.Orientation == ScreenConfiguration.OrientationLandscape,
        Orientation.Portrait => cfg.Orientation == ScreenConfiguration.OrientationPortrait,
        _ => false,
    };

    public static float SdpRotate(this int v, IAppDimensContext ctx, float rotationValue,
        Orientation orientation = Orientation.Landscape, DpQualifier finalQualifierResolver = DpQualifier.SmallWidth,
        bool ignoreMultiWindows = false, bool applyAspectRatio = false, float? customSensitivityK = null)
    {
        var target = IsTargetOrientation(ctx.Configuration, orientation);
        return (target ? rotationValue : v)
            .ToDynamicScaledDp(ctx, target ? finalQualifierResolver : DpQualifier.SmallWidth,
                Inverter.Default, ignoreMultiWindows, applyAspectRatio, customSensitivityK);
    }

    public static float HdpRotate(this int v, IAppDimensContext ctx, float rotationValue,
        Orientation orientation = Orientation.Landscape, DpQualifier finalQualifierResolver = DpQualifier.Height,
        bool ignoreMultiWindows = false, bool applyAspectRatio = false, float? customSensitivityK = null)
    {
        var target = IsTargetOrientation(ctx.Configuration, orientation);
        return (target ? rotationValue : v)
            .ToDynamicScaledDp(ctx, target ? finalQualifierResolver : DpQualifier.Height,
                Inverter.Default, ignoreMultiWindows, applyAspectRatio, customSensitivityK);
    }

    public static float WdpRotate(this int v, IAppDimensContext ctx, float rotationValue,
        Orientation orientation = Orientation.Landscape, DpQualifier finalQualifierResolver = DpQualifier.Width,
        bool ignoreMultiWindows = false, bool applyAspectRatio = false, float? customSensitivityK = null)
    {
        var target = IsTargetOrientation(ctx.Configuration, orientation);
        return (target ? rotationValue : v)
            .ToDynamicScaledDp(ctx, target ? finalQualifierResolver : DpQualifier.Width,
                Inverter.Default, ignoreMultiWindows, applyAspectRatio, customSensitivityK);
    }

    /// <summary>Plain/raw variant: when active, the already-scaled alternate is returned untouched.</summary>
    public static float SdpRotateRaw(this int v, IAppDimensContext ctx, float rawRotationValue,
        Orientation orientation = Orientation.Landscape)
        => IsTargetOrientation(ctx.Configuration, orientation)
            ? rawRotationValue
            : v.Sdp(ctx);

    public static float SdpMode(this int v, IAppDimensContext ctx, float modeValue, UiModeType uiModeType,
        DpQualifier? finalQualifierResolver = null, bool ignoreMultiWindows = false,
        bool applyAspectRatio = false, float? customSensitivityK = null)
    {
        var matches = DimenCache.GetCachedUiModeType(ctx) == uiModeType;
        return (matches ? modeValue : v).ToDynamicScaledDp(ctx,
            matches ? finalQualifierResolver ?? DpQualifier.SmallWidth : DpQualifier.SmallWidth,
            Inverter.Default, ignoreMultiWindows, applyAspectRatio, customSensitivityK);
    }

    public static float HdpMode(this int v, IAppDimensContext ctx, float modeValue, UiModeType uiModeType,
        DpQualifier? finalQualifierResolver = null, bool ignoreMultiWindows = false,
        bool applyAspectRatio = false, float? customSensitivityK = null)
    {
        var matches = DimenCache.GetCachedUiModeType(ctx) == uiModeType;
        return (matches ? modeValue : v).ToDynamicScaledDp(ctx,
            matches ? finalQualifierResolver ?? DpQualifier.Height : DpQualifier.Height,
            Inverter.Default, ignoreMultiWindows, applyAspectRatio, customSensitivityK);
    }

    public static float WdpMode(this int v, IAppDimensContext ctx, float modeValue, UiModeType uiModeType,
        DpQualifier? finalQualifierResolver = null, bool ignoreMultiWindows = false,
        bool applyAspectRatio = false, float? customSensitivityK = null)
    {
        var matches = DimenCache.GetCachedUiModeType(ctx) == uiModeType;
        return (matches ? modeValue : v).ToDynamicScaledDp(ctx,
            matches ? finalQualifierResolver ?? DpQualifier.Width : DpQualifier.Width,
            Inverter.Default, ignoreMultiWindows, applyAspectRatio, customSensitivityK);
    }

    /// <summary>Plain/raw variant for UI-mode switching.</summary>
    public static float SdpModeRaw(this int v, IAppDimensContext ctx, float rawModeValue, UiModeType uiModeType)
        => DimenCache.GetCachedUiModeType(ctx) == uiModeType ? rawModeValue : v.Sdp(ctx);

    public static float SdpQualifier(this int v, IAppDimensContext ctx, DpQualifier qualifierType,
        float qualifierValue, float alternateValue, bool ignoreMultiWindows = false,
        bool applyAspectRatio = false, float? customSensitivityK = null)
    {
        var current = DimenCalculationPlumbing.ReadScreenDp(ctx.Configuration, qualifierType);
        var useAlternate = current >= qualifierValue;
        return (useAlternate ? alternateValue : v).ToDynamicScaledDp(ctx,
            qualifierType, Inverter.Default, ignoreMultiWindows, applyAspectRatio, customSensitivityK);
    }

    public static float SdpQualifierRaw(this int v, IAppDimensContext ctx, DpQualifier qualifierType,
        float qualifierValue, float rawAlternateValue)
        => DimenCalculationPlumbing.ReadScreenDp(ctx.Configuration, qualifierType) >= qualifierValue
            ? rawAlternateValue
            : v.Sdp(ctx);

    public static float SdpScreen(this int v, IAppDimensContext ctx, UiModeType? uiModeType,
        DpQualifier? qualifierType, float qualifierValue, Orientation orientation,
        float customValue, DpQualifier? finalQualifierResolver = null, Inverter inverter = Inverter.Default,
        bool ignoreMultiWindows = false, bool applyAspectRatio = false, float? customSensitivityK = null)
        => ScaledDimension.Create((float)v, ScaledDimension.MakeEntry(customValue, uiModeType,
            qualifierType, qualifierValue, orientation, finalQualifierResolver, inverter)).Resolve(
            ignoreMultiWindows, applyAspectRatio, customSensitivityK, ctx, DpQualifier.SmallWidth);

    public static float SdpScreen(this int v, UiModeType? uiModeType, DpQualifier? qualifierType,
        float qualifierValue, Orientation orientation, float customValue,
        DpQualifier? finalQualifierResolver = null, Inverter inverter = Inverter.Default,
        bool ignoreMultiWindows = false, bool applyAspectRatio = false, float? customSensitivityK = null)
        => v.SdpScreen(AppDimensAmbient.Require(), uiModeType, qualifierType, qualifierValue,
            orientation, customValue, finalQualifierResolver, inverter,
            ignoreMultiWindows, applyAspectRatio, customSensitivityK);
}

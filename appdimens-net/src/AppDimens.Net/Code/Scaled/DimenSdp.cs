using AppDimens.Net.Common;
using AppDimens.Net.Core;

namespace AppDimens.Net.Code.Scaled;

/// <summary>
/// Scaled strategy kernels (sdp / hdp / wdp families) — the canonical AppDimens
/// scaling: <c>base * axisFactor [* density]</c> with optional aspect-ratio
/// multiplier and multi-window suppression.
/// </summary>
public static class DimenSdp
{
    public static float Sdp(IAppDimensContext context, int value) => value.Sdp(context);
    public static float Hdp(IAppDimensContext context, int value) => value.Hdp(context);
    public static float Wdp(IAppDimensContext context, int value) => value.Wdp(context);

    public static float WarmupCache(IAppDimensContext context)
    {
        DimenCache.Init(context);
        return DimenCache.ResolveSdpDp(1f, context);
    }
}

/// <summary>Scaled strategy for text (ssp family), including no-font-scale variants (sem).</summary>
public static class DimenSsp
{
    public static float Ssp(IAppDimensContext context, int value,
        bool ignoreMultiWindows = false, bool applyAspectRatio = false, float? customSensitivityK = null) =>
        value.ToDynamicScaledPx(context, DpQualifier.SmallWidth, Inverter.Default,
            ignoreMultiWindows, applyAspectRatio, customSensitivityK) /
        (context.Density * context.Configuration.FontScale);

    public static float Sem(IAppDimensContext context, int value) =>
        DimenSsp.Ssp(context, value) * context.Configuration.FontScale;
}

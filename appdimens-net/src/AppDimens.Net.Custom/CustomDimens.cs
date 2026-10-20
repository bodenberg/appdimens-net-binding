using AppDimens.Net.Common;
using AppDimens.Net.Core;

namespace AppDimens.Net.Custom;

/// <summary>
/// Bridge between the portable custom engine and the AppDimens .NET core: window snapshots,
/// device classes, cache keys and the scoped strategy.
/// </summary>
public static class CustomDimens
{
    private static readonly AsyncLocal<ScalingStrategy?> Scoped = new();

    /// <summary>Optional override for the device class (e.g. force <see cref="DeviceClass.Tv"/> on a kiosk build).</summary>
    public static Func<IAppDimensContext, DeviceClass?>? DeviceClassResolver { get; set; }

    /// <summary>Strategy used by parameterless <c>Cdp()</c>/<c>Csp()</c>: the scoped one, else <see cref="ScalingStrategies.Default"/>.</summary>
    public static ScalingStrategy Current => Scoped.Value ?? ScalingStrategies.Default;

    /// <summary>
    /// Makes <paramref name="strategy"/> the default for this async flow until disposed
    /// (<c>using (CustomDimens.Use(brand)) { BuildPage(); }</c>).
    /// </summary>
    public static IDisposable Use(ScalingStrategy strategy)
    {
        var previous = Scoped.Value;
        Scoped.Value = strategy;
        return new Restore(previous);
    }

    /// <summary>Default device class: UI mode first, then desktop operating systems, then size.</summary>
    public static DeviceClass DefaultDeviceClass(IAppDimensContext context)
    {
        var sw = context.Configuration.SmallestScreenWidthDp > 0
            ? context.Configuration.SmallestScreenWidthDp
            : Math.Min(context.Configuration.ScreenWidthDp, context.Configuration.ScreenHeightDp);
        switch (context.UiModeType)
        {
            case UiModeType.Television: return DeviceClass.Tv;
            case UiModeType.Car: return DeviceClass.Car;
            case UiModeType.Watch: return DeviceClass.Watch;
            case UiModeType.Desk: return DeviceClass.Desktop;
            case UiModeType.VrHeadset: return DeviceClass.Vr;
        }
        if (OperatingSystem.IsWindows() || OperatingSystem.IsMacOS() || OperatingSystem.IsMacCatalyst() || OperatingSystem.IsLinux())
            return DeviceClass.Desktop;
        return CustomNames.FromSmallestWidth(sw);
    }

    /// <summary>Converts a core snapshot into the platform-free <see cref="ScreenSnapshot"/>.</summary>
    public static ScreenSnapshot ToScreenSnapshot(this DimenMetrics m, DeviceClass device) => new(
        widthDp: m.ScreenWidthDp,
        heightDp: m.ScreenHeightDp,
        smallestWidthDp: m.SmallestWidthDp,
        density: m.Density,
        fontScale: m.FontScale,
        isLandscape: m.Orientation switch
        {
            ScreenConfiguration.OrientationLandscape => true,
            ScreenConfiguration.OrientationPortrait => false,
            _ => m.ScreenWidthDp > m.ScreenHeightDp,
        },
        isMultiWindow: m.IsInMultiWindowMode,
        device: device);

    /// <summary>Platform-free snapshot of the window of <paramref name="context"/>.</summary>
    public static ScreenSnapshot ScreenSnapshot(this IAppDimensContext context) =>
        DimenCache.FastMetricsForCode(context).ToScreenSnapshot(DeviceOf(context));

    /// <summary>Resolves for the window of <paramref name="context"/>; memoized per core snapshot.</summary>
    public static ResolvedScaling Resolve(this ScalingStrategy strategy, IAppDimensContext context)
    {
        var m = DimenCache.FastMetricsForCode(context);
        return strategy.Resolve(m, () => m.ToScreenSnapshot(DeviceOf(context)));
    }

    /// <summary>
    /// Shared-cache key for this strategy. Factor-only strategies are flagged non-cacheable (one
    /// multiply); value-dependent ones are stored per window snapshot.
    /// </summary>
    public static long CacheKey(this ScalingStrategy strategy, float baseValue, DimenCache.ValueType valueType) =>
        DimenCache.BuildCustomKey(baseValue, strategy.CacheId >= 0 ? strategy.CacheId : 0,
            isLandscape: false, ignoreMultiWindows: false, valueType,
            cacheable: strategy.IsValueDependent && strategy.CacheId >= 0);

    internal static float Cached(ScalingStrategy strategy, float baseValue, IAppDimensContext context,
        DimenCache.ValueType valueType, Func<ResolvedScaling, float> compute)
    {
        var key = strategy.CacheKey(baseValue, valueType);
        if (DimenCache.ShouldComputeDirectly(key)) return compute(strategy.Resolve(context));
        var metrics = DimenCache.FastMetricsForCode(context);
        if (DimenCache.TryPeek(metrics, key, out var cached)) return cached;
        return DimenCache.Resolve(key, metrics, () => compute(strategy.Resolve(context)));
    }

    private static DeviceClass DeviceOf(IAppDimensContext context) =>
        DeviceClassResolver?.Invoke(context) ?? DefaultDeviceClass(context);

    private sealed class Restore(ScalingStrategy? previous) : IDisposable
    {
        public void Dispose() => Scoped.Value = previous;
    }
}

/// <summary>
/// <c>Cdp</c>/<c>Csp</c> extensions. Like the core surface, parameterless overloads resolve through the
/// ambient window (<see cref="AppDimensAmbient"/>); results are logical dp/sp units and <c>*Px</c>
/// variants return pixels. Without a strategy argument, <see cref="CustomDimens.Current"/> is used.
/// </summary>
public static class CustomExtensions
{
    // ── dp ──
    public static float Cdp(this float v, ScalingStrategy strategy, IAppDimensContext ctx) =>
        CustomDimens.Cached(strategy, v, ctx, DimenCache.ValueType.Dp, r => r.Scale(v));
    public static float Cdp(this float v, IAppDimensContext ctx) => v.Cdp(CustomDimens.Current, ctx);
    public static float Cdp(this float v, ScalingStrategy strategy) => v.Cdp(strategy, AppDimensAmbient.Require());
    public static float Cdp(this float v) => v.Cdp(CustomDimens.Current, AppDimensAmbient.Require());
    public static float Cdp(this int v, ScalingStrategy strategy, IAppDimensContext ctx) => ((float)v).Cdp(strategy, ctx);
    public static float Cdp(this int v, IAppDimensContext ctx) => ((float)v).Cdp(ctx);
    public static float Cdp(this int v, ScalingStrategy strategy) => ((float)v).Cdp(strategy);
    public static float Cdp(this int v) => ((float)v).Cdp();
    public static float Cdp(this double v, ScalingStrategy strategy) => ((float)v).Cdp(strategy);
    public static float Cdp(this double v) => ((float)v).Cdp();

    // ── px ──
    public static float CdpPx(this float v, ScalingStrategy strategy, IAppDimensContext ctx) =>
        CustomDimens.Cached(strategy, v, ctx, DimenCache.ValueType.Px, r => r.Scale(v) * r.Screen.Density);
    public static float CdpPx(this float v, IAppDimensContext ctx) => v.CdpPx(CustomDimens.Current, ctx);
    public static float CdpPx(this int v, ScalingStrategy strategy, IAppDimensContext ctx) => ((float)v).CdpPx(strategy, ctx);
    public static float CdpPx(this int v, IAppDimensContext ctx) => ((float)v).CdpPx(ctx);
    public static float CdpPx(this int v, ScalingStrategy strategy) => ((float)v).CdpPx(strategy, AppDimensAmbient.Require());
    public static float CdpPx(this int v) => ((float)v).CdpPx(AppDimensAmbient.Require());

    // ── sp (font-scale policy applied) ──

    /// <summary>
    /// Text size in <b>sp units</b>, like <c>Ssp</c>: the toolkit applies the system font scale and the
    /// strategy policy (<c>follow</c> / <c>ignore</c> / <c>clamp</c>) is pre-compensated.
    /// </summary>
    public static float Csp(this float v, ScalingStrategy strategy, IAppDimensContext ctx) =>
        CustomDimens.Cached(strategy, v, ctx, DimenCache.ValueType.SpWithScale, r => r.ScaleSpUnits(v));
    public static float Csp(this float v, IAppDimensContext ctx) => v.Csp(CustomDimens.Current, ctx);
    public static float Csp(this int v, ScalingStrategy strategy, IAppDimensContext ctx) => ((float)v).Csp(strategy, ctx);
    public static float Csp(this int v, IAppDimensContext ctx) => ((float)v).Csp(ctx);
    public static float Csp(this int v, ScalingStrategy strategy) => ((float)v).Csp(strategy, AppDimensAmbient.Require());
    public static float Csp(this int v) => ((float)v).Csp(AppDimensAmbient.Require());

    /// <summary>Final text size in dp (policy applied) — for toolkits that do not scale fonts themselves.</summary>
    public static float CspDp(this int v, ScalingStrategy strategy, IAppDimensContext ctx) =>
        CustomDimens.Cached(strategy, v, ctx, DimenCache.ValueType.SpNoScale, r => r.ScaleSpDp(v));
    public static float CspDp(this int v, ScalingStrategy strategy) => v.CspDp(strategy, AppDimensAmbient.Require());
    public static float CspDp(this int v) => v.CspDp(CustomDimens.Current, AppDimensAmbient.Require());

    /// <summary>Text size in pixels (policy applied).</summary>
    public static float CspPx(this int v, ScalingStrategy strategy, IAppDimensContext ctx) =>
        CustomDimens.Cached(strategy, v, ctx, DimenCache.ValueType.SpPxWithScale, r => r.ScaleSpDp(v) * r.Screen.Density);
    public static float CspPx(this int v, ScalingStrategy strategy) => v.CspPx(strategy, AppDimensAmbient.Require());
    public static float CspPx(this int v) => v.CspPx(CustomDimens.Current, AppDimensAmbient.Require());

    // ── strategy-first facilitators ──
    public static float Dp(this ScalingStrategy s, float value) => value.Cdp(s);
    public static float Sp(this ScalingStrategy s, float value) => value.Csp(s, AppDimensAmbient.Require());
    public static float Px(this ScalingStrategy s, float value) => value.CdpPx(s, AppDimensAmbient.Require());
    public static float CurrentFactor(this ScalingStrategy s, IAppDimensContext? ctx = null) =>
        s.Resolve(ctx ?? AppDimensAmbient.Require()).Factor;
    public static string ExplainCurrent(this ScalingStrategy s, IAppDimensContext? ctx = null) =>
        s.Resolve(ctx ?? AppDimensAmbient.Require()).Explain(s.Id);

    /// <summary>Reads a strategies JSON file and registers it (see <see cref="ScalingStrategies.LoadJson"/>).</summary>
    public static IReadOnlyDictionary<string, ScalingStrategy> LoadFile(string path) =>
        ScalingStrategies.LoadJson(File.ReadAllText(path));
}

using System;
using Android.Content;
using Android.Content.Res;
using Com.Appdimens.Sdps.Code;
using Com.Appdimens.Sdps.Core;

namespace Com.Appdimens.Sdps;

/// <summary>
/// Thin C# orchestration helpers for AppDimens SDPS warmup and configuration refresh.
/// Does not reimplement AAR math — forwards to <see cref="AppDimensSdpsFactors"/> /
/// <see cref="DimenSdp"/>. Call from <c>OnCreate</c> and <c>OnConfigurationChanged</c>
/// when the Activity handles config changes without recreation.
/// </summary>
public static class SdpsConfig
{
    /// <summary>
    /// Raised after <see cref="OnConfigurationChanged"/> updates aspect-ratio factors.
    /// Subscribe to re-apply View padding, layout params, and text sizes that were set
    /// from previous <c>DimenSdp</c>/<c>DimenSsp</c> pixel values.
    /// </summary>
    public static event EventHandler<Configuration?>? DimensionsShouldRefresh;

    /// <summary>
    /// Prefetch aspect-ratio factors at app start (recommended).
    /// </summary>
    public static void Warmup(Context context)
    {
        ArgumentNullException.ThrowIfNull(context);
        DimenSdp.WarmupSdpsFactors(context);
    }

    /// <summary>
    /// Recompute AR factors when screen geometry or density may have changed.
    /// Safe to call frequently — the AAR no-ops when the config signature is unchanged.
    /// </summary>
    public static void EnsureUpToDate(Context context)
    {
        ArgumentNullException.ThrowIfNull(context);
        AppDimensSdpsFactors.Instance.EnsureUpToDate(context);
    }

    /// <summary>
    /// Call from <c>Activity.OnConfigurationChanged</c> (or equivalent) when the
    /// activity is not recreated. Updates factors and raises
    /// <see cref="DimensionsShouldRefresh"/> so the UI can re-resolve and re-apply dimens.
    /// </summary>
    public static void OnConfigurationChanged(Context context, Configuration? newConfig)
    {
        ArgumentNullException.ThrowIfNull(context);
        EnsureUpToDate(context);
        DimensionsShouldRefresh?.Invoke(null, newConfig);
    }
}

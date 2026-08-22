// Copyright © Jean Bodenberg — see https://github.com/bodenberg/appdimens-net-binding
//
// Wraps the Java/Kotlin AppDimens SDP factor warmup so .NET/MAUI callers can
// preload density factors on a background thread and refresh them on
// configuration changes (rotation, fold/unfold, dark mode, density).

using System;
using Android.Content;
using Android.Content.Res;
using Com.Appdimens.Sdps.Code;
using Com.Appdimens.Sdps.Core;

namespace Com.Appdimens.Sdps;

/// <summary>
/// Orquestra o warmup dos fatores de densidade SDP/HDP/WDP e dispara um evento
/// quando a configuração mudou e os fatores foram recalculados.
/// </summary>
public static class SdpsConfig
{
    /// <summary>
    /// Disparado após <see cref="OnConfigurationChanged(Context, Configuration)"/>
    /// recomputar os fatores — os consumidores devem reler <c>DimenSdp.*</c>.
    /// </summary>
    public static event EventHandler<Configuration?>? DimensionsShouldRefresh;

    /// <summary>
    /// Pré-carrega os fatores de densidade SDP (equivalente a
    /// <c>DimenSdp.WarmupSdpsFactors(context)</c>).
    /// </summary>
    public static void Warmup(Context context)
    {
        ArgumentNullException.ThrowIfNull(context);
        DimenSdp.WarmupSdpsFactors(context);
    }

    /// <summary>
    /// Garante que <c>AppDimensSdpsFactors.Instance</c> está atualizado para o
    /// contexto informado.
    /// </summary>
    public static void EnsureUpToDate(Context context)
    {
        ArgumentNullException.ThrowIfNull(context);
        AppDimensSdpsFactors.Instance.EnsureUpToDate(context);
    }

    /// <summary>
    /// Chame a partir de <c>Activity.OnConfigurationChanged</c>: recalcula os
    /// fatores e dispara <see cref="DimensionsShouldRefresh"/>.
    /// </summary>
    public static void OnConfigurationChanged(Context context, Configuration? newConfig)
    {
        ArgumentNullException.ThrowIfNull(context);
        EnsureUpToDate(context);
        DimensionsShouldRefresh?.Invoke(null, newConfig);
    }
}

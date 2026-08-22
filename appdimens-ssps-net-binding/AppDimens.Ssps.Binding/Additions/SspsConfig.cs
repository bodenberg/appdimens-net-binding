// Copyright © Jean Bodenberg — see https://github.com/bodenberg/appdimens-net-binding
//
// Wraps the Java/Kotlin AppDimens SSPS factor warmup so .NET/MAUI callers can
// preload typography (SSP/HSP/WSP) factors on a background thread and refresh
// them on configuration changes (rotation, fold/unfold, dark mode, density).

using System;
using Android.Content;
using Android.Content.Res;
using Com.Appdimens.Ssps.Code;
using Com.Appdimens.Ssps.Core;

namespace Com.Appdimens.Ssps;

/// <summary>
/// Orquestra o warmup dos fatores de tipografia SSP/HSP/WSP e dispara um evento
/// quando a configuração mudou e os fatores foram recalculados.
/// </summary>
public static class SspsConfig
{
    /// <summary>
    /// Disparado após <see cref="OnConfigurationChanged(Context, Configuration)"/>
    /// recomputar os fatores — os consumidores devem reler <c>DimenSsp.*</c>.
    /// </summary>
    public static event EventHandler<Configuration?>? DimensionsShouldRefresh;

    /// <summary>
    /// Pré-carrega os fatores de tipografia SSP (equivalente a
    /// <c>DimenSsp.WarmupSspsFactors(context)</c>).
    /// </summary>
    public static void Warmup(Context context)
    {
        ArgumentNullException.ThrowIfNull(context);
        DimenSsp.WarmupSspsFactors(context);
    }

    /// <summary>
    /// Garante que <c>AppDimensSspsFactors.Instance</c> está atualizado para o
    /// contexto informado.
    /// </summary>
    public static void EnsureUpToDate(Context context)
    {
        ArgumentNullException.ThrowIfNull(context);
        AppDimensSspsFactors.Instance.EnsureUpToDate(context);
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

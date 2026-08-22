// Copyright © Jean Bodenberg — see https://github.com/bodenberg/appdimens-net-binding
//
// Wraps the AppDimens Dynamic cache so .NET/MAUI callers can preload dimensions
// and refresh them on configuration changes (rotation, fold/unfold, density,
// multi-window resize). The library is event-driven since 3.1.8: call
// DimenCache.invalidateOnConfigChange on every real configuration change.

using System;
using Android.Content;
using Android.Content.Res;
using Com.Appdimens.Dynamic.Core;

namespace Com.Appdimens.Dynamic;

/// <summary>
/// Orquestra o warmup do cache dinâmico e dispara um evento quando a
/// configuração mudou e os fatores foram recalculados.
/// </summary>
public static class DynamicConfig
{
    /// <summary>
    /// Disparado após <see cref="OnConfigurationChanged(Context, Configuration)"/>
    /// reprocessar o cache — os consumidores devem reler <c>DimenSdp.*</c> / <c>DimenSsp.*</c>.
    /// </summary>
    public static event EventHandler<Configuration?>? DimensionsShouldRefresh;

    /// <summary>
    /// Pré-carrega o cache de dimensões (equivalente a <c>DimenCache.init(context)</c>).
    /// </summary>
    public static void Warmup(Context context)
    {
        ArgumentNullException.ThrowIfNull(context);
        DimenCache.Init(context);
    }

    /// <summary>
    /// Chame a partir de <c>Activity.OnConfigurationChanged</c>: invalida o cache por
    /// janela/configuração e dispara <see cref="DimensionsShouldRefresh"/>.
    /// </summary>
    public static void OnConfigurationChanged(Context context, Configuration? newConfig)
    {
        if (newConfig != null)
        {
            DimenCache.InvalidateOnConfigChange(newConfig);
        }
        DimensionsShouldRefresh?.Invoke(null, newConfig);
    }
}

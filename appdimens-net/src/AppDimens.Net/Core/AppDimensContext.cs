using AppDimens.Net.Common;

namespace AppDimens.Net.Core;

/// <summary>
/// Platform-neutral window handle exposing everything needed to resolve a dynamic
/// dimension. MAUI adapters wrap the native window per OS.
/// </summary>
public interface IAppDimensContext
{
    /// <summary>Current immutable window snapshot.</summary>
    ScreenConfiguration Configuration { get; }

    /// <summary>Logical display density (<c>DensityDpi / 160f</c>).</summary>
    float Density { get; }

    /// <summary>Physical horizontal dots-per-inch (physical-unit conversions).</summary>
    float Xdpi { get; }

    /// <summary>True when the window is in multi-window / split-screen mode.</summary>
    bool IsInMultiWindowMode { get; }

    /// <summary>Resolved UI mode type (foldable detection lives in platform adapters).</summary>
    UiModeType UiModeType { get; }

    /// <summary>
    /// Registers a listener invoked synchronously whenever this window's configuration
    /// changes (rotation, resize, density, font scale...). Dispose to unregister.
    /// </summary>
    IDisposable RegisterConfigurationListener(Action listener) => NullRegistration.Instance;

    /// <summary>Builds the coherent <see cref="DimenMetrics"/> snapshot for this window.</summary>
    DimenMetrics ToMetrics() => DimenMetrics.From(Configuration, IsInMultiWindowMode);

    sealed class NullRegistration : IDisposable
    {
        public static readonly NullRegistration Instance = new();
        public void Dispose() { }
    }
}

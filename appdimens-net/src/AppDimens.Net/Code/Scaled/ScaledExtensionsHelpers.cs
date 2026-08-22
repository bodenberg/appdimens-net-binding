using AppDimens.Net.Common;
using AppDimens.Net.Core;

namespace AppDimens.Net.Code.Scaled;

/// <summary>Shared helper used across strategy modules.</summary>
public static class ScaledExtensionsHelpers
{
    public static bool IsTargetOrientation(ScreenConfiguration cfg, Orientation orientation) =>
        orientation switch
        {
            Orientation.Landscape => cfg.Orientation == ScreenConfiguration.OrientationLandscape,
            Orientation.Portrait => cfg.Orientation == ScreenConfiguration.OrientationPortrait,
            _ => false,
        };
}

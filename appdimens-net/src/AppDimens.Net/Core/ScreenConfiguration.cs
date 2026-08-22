namespace AppDimens.Net.Core;

/// <summary>
/// Immutable snapshot of the window fields that drive dimension resolution.
/// Platform-neutral replacement for Android <c>Configuration</c>; every platform
/// adapter produces this from its native window state.
/// </summary>
public sealed record ScreenConfiguration(
    int ScreenWidthDp,
    int ScreenHeightDp,
    int SmallestScreenWidthDp,
    int DensityDpi,
    float FontScale,
    int Orientation,
    int UiMode)
{
    public const int OrientationUndefined = 0;
    public const int OrientationPortrait = 1;
    public const int OrientationLandscape = 2;

    /// <summary>Synthetic baseline window (300x533 @ 160 dpi) used by tests and defaults.</summary>
    public static readonly ScreenConfiguration Default =
        new(300, 533, 300, 160, 1f, OrientationUndefined, 0);

    public bool IsLandscape => Orientation == OrientationLandscape;
    public bool IsPortrait => Orientation == OrientationPortrait;
}

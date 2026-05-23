using AppDimens.Maui;
using AppDimens.Maui.Core;

namespace AppDimens.Maui.Sample.Services;

public static class ScreenMetricsFormatter
{
    public static string FormatCurrent()
    {
        var m = AppDimensResolver.Instance.Metrics.Current;
        var opts = AppDimensResolver.Instance.Options;
        return $"SW {m.SmallestDp:F0} dp · W {m.WidthDp:F0} · H {m.HeightDp:F0} dp\n" +
               $"DPI {m.DensityDpi} · Densidade {m.Density:F2} · {m.Orientation}\n" +
               $"Modo: {opts.ScalingMode} · AR warmup: {opts.WarmupAspectRatio}";
    }
}

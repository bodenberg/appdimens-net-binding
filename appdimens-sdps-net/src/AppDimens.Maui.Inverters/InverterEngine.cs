using AppDimens.Maui.Core;

namespace AppDimens.Maui.Inverters;

public static class InverterEngine
{
    public static DpQualifier EffectiveQualifier(
        ScreenMetricsSnapshot metrics,
        DpQualifier qualifier,
        InverterType inverter)
    {
        var isLandscape = metrics.Orientation == ScreenOrientation.Landscape;
        var isPortrait = metrics.Orientation == ScreenOrientation.Portrait;
        var actual = qualifier;

        switch (inverter)
        {
            case InverterType.PhToLw when isLandscape && qualifier == DpQualifier.Height:
                actual = DpQualifier.Width;
                break;
            case InverterType.PwToLh when isLandscape && qualifier == DpQualifier.Width:
                actual = DpQualifier.Height;
                break;
            case InverterType.LhToPw when isPortrait && qualifier == DpQualifier.Height:
                actual = DpQualifier.Width;
                break;
            case InverterType.LwToPh when isPortrait && qualifier == DpQualifier.Width:
                actual = DpQualifier.Height;
                break;
            case InverterType.SwToLh when isLandscape && qualifier == DpQualifier.SmallWidth:
                actual = DpQualifier.Height;
                break;
            case InverterType.SwToLw when isLandscape && qualifier == DpQualifier.SmallWidth:
                actual = DpQualifier.Width;
                break;
            case InverterType.SwToPh when isPortrait && qualifier == DpQualifier.SmallWidth:
                actual = DpQualifier.Height;
                break;
            case InverterType.SwToPw when isPortrait && qualifier == DpQualifier.SmallWidth:
                actual = DpQualifier.Width;
                break;
        }

        return actual;
    }

    public static InverterType ForSdpPh() => InverterType.SwToPh;
    public static InverterType ForSdpLw() => InverterType.SwToLw;
    public static InverterType ForSdpLh() => InverterType.SwToLh;
    public static InverterType ForSdpPw() => InverterType.SwToPw;
    public static InverterType ForHdpLw() => InverterType.PhToLw;
    public static InverterType ForHdpPw() => InverterType.PwToLh;
    public static InverterType ForWdpLh() => InverterType.PwToLh;
    public static InverterType ForWdpPh() => InverterType.PhToLw;
}

using AppDimens.Maui.Core;
using AppDimens.Maui.Inverters;
using Microsoft.Maui.Controls;

namespace AppDimens.Maui.Extensions;

public static class DimenIntExtensions
{
    private static AppDimensResolver R => AppDimensResolver.Instance;

    public static double Sdp(this int value) => R.Sdp(value);
    public static double Ssp(this int value) => R.Ssp(value);
    public static double Hdp(this int value) => R.Hdp(value);
    public static double Wdp(this int value) => R.Wdp(value);
    public static double Sdpa(this int value) => R.Sdpa(value);
    public static double Hdpa(this int value) => R.Resolve(value, DpQualifier.Height, applyAspectRatio: true);
    public static double Wdpa(this int value) => R.Resolve(value, DpQualifier.Width, applyAspectRatio: true);
    public static double Sem(this int value) => R.Sem(value);
    public static double Hem(this int value) => R.Hem(value);
    public static double Wem(this int value) => R.Wem(value);

    public static double SdpPh(this int value) => R.Sdp(value, InverterEngine.ForSdpPh());
    public static double SdpLw(this int value) => R.Sdp(value, InverterEngine.ForSdpLw());
    public static double SdpLh(this int value) => R.Sdp(value, InverterEngine.ForSdpLh());
    public static double SdpPw(this int value) => R.Sdp(value, InverterEngine.ForSdpPw());
    public static double HdpLw(this int value) => R.Hdp(value, InverterEngine.ForHdpLw());
    public static double HdpPw(this int value) => R.Hdp(value, InverterEngine.ForHdpPw());
    public static double WdpLh(this int value) => R.Wdp(value, InverterEngine.ForWdpLh());
    public static double WdpPh(this int value) => R.Wdp(value, InverterEngine.ForWdpPh());

    public static double SdpPha(this int value) => R.Sdpa(value, InverterEngine.ForSdpPh());
    public static double SdpLwa(this int value) => R.Sdpa(value, InverterEngine.ForSdpLw());
    public static double SdpLha(this int value) => R.Sdpa(value, InverterEngine.ForSdpLh());
    public static double SdpPwa(this int value) => R.Sdpa(value, InverterEngine.ForSdpPw());

    public static double SdpPx(this int value) => R.Sdp(value) * R.Metrics.Current.Density;
    public static double SspPx(this int value) => R.Ssp(value) * R.Metrics.Current.Density;
}

public static class DimenViewExtensions
{
    public static Layout PaddingSdp(this Layout layout, int value)
    {
        layout.Padding = new Thickness(value.Sdp());
        return layout;
    }

    public static Border PaddingSdp(this Border border, int value)
    {
        border.Padding = new Thickness(value.Sdp());
        return border;
    }

    public static Label FontSsp(this Label label, int value)
    {
        label.FontSize = value.Ssp();
        return label;
    }

    public static Label FontHsp(this Label label, int value)
    {
        label.FontSize = value.Hsp();
        return label;
    }

    public static Label FontWsp(this Label label, int value)
    {
        label.FontSize = value.Wsp();
        return label;
    }

    public static Label FontSem(this Label label, int value)
    {
        label.FontSize = value.Sem();
        return label;
    }

    public static View MarginSdp(this View view, int value)
    {
        view.Margin = new Thickness(value.Sdp());
        return view;
    }
}

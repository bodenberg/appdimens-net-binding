using AppDimens.Maui.Core;
using AppDimens.Maui.Inverters;

namespace AppDimens.Maui.Extensions;

public static class DimenSpIntExtensions
{
    private static AppDimensResolver R => AppDimensResolver.Instance;

    // Font and em extensions (HSP, WSP, SSPA, …)
    public static double Hsp(this int value) => R.Hsp(value);
    public static double Wsp(this int value) => R.Wsp(value);
    public static double Sspa(this int value) => R.Sspa(value);
    public static double Hspa(this int value) => R.Hspa(value);
    public static double Wspa(this int value) => R.Wspa(value);
    public static double Sema(this int value) => R.Sema(value);
    public static double Hema(this int value) => R.Hema(value);
    public static double Wema(this int value) => R.Wema(value);

    // SSP with axis inverters
    public static double SspPh(this int value) => R.Ssp(value, InverterEngine.ForSdpPh());
    public static double SspLw(this int value) => R.Ssp(value, InverterEngine.ForSdpLw());
    public static double SspLh(this int value) => R.Ssp(value, InverterEngine.ForSdpLh());
    public static double SspPw(this int value) => R.Ssp(value, InverterEngine.ForSdpPw());

    // HSP with axis inverters
    public static double HspLw(this int value) => R.Hsp(value, InverterEngine.ForHdpLw());
    public static double HspPw(this int value) => R.Hsp(value, InverterEngine.ForHdpPw());

    // WSP with axis inverters
    public static double WspLh(this int value) => R.Wsp(value, InverterEngine.ForWdpLh());
    public static double WspPh(this int value) => R.Wsp(value, InverterEngine.ForWdpPh());

    // SEM with axis inverters
    public static double SemPh(this int value) => R.Sem(value, InverterEngine.ForSdpPh());
    public static double SemLw(this int value) => R.Sem(value, InverterEngine.ForSdpLw());
    public static double SemLh(this int value) => R.Sem(value, InverterEngine.ForSdpLh());
    public static double SemPw(this int value) => R.Sem(value, InverterEngine.ForSdpPw());

    // HEM with axis inverters
    public static double HemLw(this int value) => R.Hem(value, InverterEngine.ForHdpLw());
    public static double HemPw(this int value) => R.Hem(value, InverterEngine.ForHdpPw());

    // WEM with axis inverters
    public static double WemLh(this int value) => R.Wem(value, InverterEngine.ForWdpLh());
    public static double WemPh(this int value) => R.Wem(value, InverterEngine.ForWdpPh());

    // SSP with aspect ratio and axis inverters
    public static double SspPha(this int value) => R.Sspa(value, InverterEngine.ForSdpPh());
    public static double SspLwa(this int value) => R.Sspa(value, InverterEngine.ForSdpLw());
    public static double SspLha(this int value) => R.Sspa(value, InverterEngine.ForSdpLh());
    public static double SspPwa(this int value) => R.Sspa(value, InverterEngine.ForSdpPw());

    // HSP with aspect ratio and axis inverters
    public static double HspLwa(this int value) => R.Hspa(value, InverterEngine.ForHdpLw());
    public static double HspPwa(this int value) => R.Hspa(value, InverterEngine.ForHdpPw());

    // WSP with aspect ratio and axis inverters
    public static double WspLha(this int value) => R.Wspa(value, InverterEngine.ForWdpLh());
    public static double WspPha(this int value) => R.Wspa(value, InverterEngine.ForWdpPh());

    // SEM with aspect ratio and axis inverters
    public static double SemaPh(this int value) => R.Sema(value, InverterEngine.ForSdpPh());
    public static double SemaLw(this int value) => R.Sema(value, InverterEngine.ForSdpLw());
    public static double SemaLh(this int value) => R.Sema(value, InverterEngine.ForSdpLh());
    public static double SemaPw(this int value) => R.Sema(value, InverterEngine.ForSdpPw());

    // HEM with aspect ratio and axis inverters
    public static double HemaLw(this int value) => R.Hema(value, InverterEngine.ForHdpLw());
    public static double HemaPw(this int value) => R.Hema(value, InverterEngine.ForHdpPw());

    // WEM with aspect ratio and axis inverters
    public static double WemaLh(this int value) => R.Wema(value, InverterEngine.ForWdpLh());
    public static double WemaPh(this int value) => R.Wema(value, InverterEngine.ForWdpPh());

    // Convert scaled values to device pixels
    public static double HspPx(this int value) => R.Hsp(value) * R.Metrics.Current.Density;
    public static double WspPx(this int value) => R.Wsp(value) * R.Metrics.Current.Density;
    public static double SemPx(this int value) => R.Sem(value) * R.Metrics.Current.Density;
    public static double HemPx(this int value) => R.Hem(value) * R.Metrics.Current.Density;
    public static double WemPx(this int value) => R.Wem(value) * R.Metrics.Current.Density;
}

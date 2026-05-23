using AppDimens.Maui.Extensions;
using AppDimens.Maui.Sample.Controls;
using AppDimens.Maui.Sample.Services;

namespace AppDimens.Maui.Sample.Views;

public partial class InvertersPage : ContentPage
{
    private static readonly (string Title, string Code, string ShortMarkup, string LongMarkup, int Index, Func<int, double> Resolve)[] Samples =
    [
        ("SDP + PH", "16.SdpPh()", "{sdpPh:16}", "{dimen:Sdp Value=16, Inverter=SwToPh}", 16, v => v.SdpPh()),
        ("SDP + LW", "16.SdpLw()", "{sdpLw:16}", "{dimen:Sdp Value=16, Inverter=SwToLw}", 16, v => v.SdpLw()),
        ("SDP + LH", "16.SdpLh()", "{sdpLh:16}", "{dimen:Sdp Value=16, Inverter=SwToLh}", 16, v => v.SdpLh()),
        ("SDP + PW", "16.SdpPw()", "{sdpPw:16}", "{dimen:Sdp Value=16, Inverter=SwToPw}", 16, v => v.SdpPw()),
        ("HDP + LW", "48.HdpLw()", "{hdpLw:48}", "{dimen:Hdp Value=48, Inverter=PwToLh}", 48, v => v.HdpLw()),
        ("HDP + PW", "48.HdpPw()", "{hdpPw:48}", "{dimen:Hdp Value=48, Inverter=PwToLh}", 48, v => v.HdpPw()),
        ("WDP + LH", "120.WdpLh()", "{wdpLh:120}", "{dimen:Wdp Value=120, Inverter=PwToLh}", 120, v => v.WdpLh()),
        ("WDP + PH", "120.WdpPh()", "{wdpPh:120}", "{dimen:Wdp Value=120, Inverter=SwToPh}", 120, v => v.WdpPh()),
        ("SDPA + PH", "16.SdpPha()", "{sdpPhA:16}", "{dimen:Sdpa Value=16, Inverter=SwToPh}", 16, v => v.SdpPha()),
        ("SDPA + LW", "16.SdpLwa()", "{sdpLwA:16}", "{dimen:Sdpa Value=16, Inverter=SwToLw}", 16, v => v.SdpLwa()),
        ("SDPA + LH", "16.SdpLha()", "{sdpLhA:16}", "{dimen:Sdpa Value=16, Inverter=SwToLh}", 16, v => v.SdpLha()),
        ("SDPA + PW", "16.SdpPwa()", "{sdpPwA:16}", "{dimen:Sdpa Value=16, Inverter=SwToPw}", 16, v => v.SdpPwa()),
    ];

    private static readonly (string Title, string Code, string ShortMarkup, string LongMarkup, int Index, Func<int, double> Resolve)[] TypographyArSamples =
    [
        ("SEMA + PH", "18.SemaPh()", "{semaPh:18}", "{dimen:Sema Value=18, Inverter=SwToPh}", 18, v => v.SemaPh()),
        ("SEMA + LW", "18.SemaLw()", "{semaLw:18}", "{dimen:Sema Value=18, Inverter=SwToLw}", 18, v => v.SemaLw()),
        ("SEMA + LH", "18.SemaLh()", "{semaLh:18}", "{dimen:Sema Value=18, Inverter=SwToLh}", 18, v => v.SemaLh()),
        ("SEMA + PW", "18.SemaPw()", "{semaPw:18}", "{dimen:Sema Value=18, Inverter=SwToPw}", 18, v => v.SemaPw()),
        ("HEMA + LW", "18.HemaLw()", "{hemaLw:18}", "{dimen:Hema Value=18, Inverter=PhToLw}", 18, v => v.HemaLw()),
        ("HEMA + PW", "18.HemaPw()", "{hemaPw:18}", "{dimen:Hema Value=18, Inverter=PwToLh}", 18, v => v.HemaPw()),
        ("WEMA + LH", "18.WemaLh()", "{wemaLh:18}", "{dimen:Wema Value=18, Inverter=PwToLh}", 18, v => v.WemaLh()),
        ("WEMA + PH", "18.WemaPh()", "{wemaPh:18}", "{dimen:Wema Value=18, Inverter=PhToLw}", 18, v => v.WemaPh()),
    ];

    public InvertersPage()
    {
        InitializeComponent();
        DimensSampleRefresh.WhenMetricsChange(this, RefreshAll);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        RefreshAll();
    }

    private void RefreshAll()
    {
        RefreshResolvedLabels();
        BuildCodeCards();
    }

    private void RefreshResolvedLabels()
    {
        var sdpPh = $"{16.SdpPh():F1} px";
        var sdpLw = $"{16.SdpLw():F1} px";
        var hdpPw = $"{48.HdpPw():F1} px";
        var wdpLh = $"{120.WdpLh():F1} px";
        var sdpPha = $"{16.SdpPha():F1} px";
        var sdpLwa = $"{16.SdpLwa():F1} px";
        var sdpLha = $"{16.SdpLha():F1} px";
        var sdpPwa = $"{16.SdpPwa():F1} px";

        SdpPhShortPxLabel.Text = sdpPh;
        SdpLwShortPxLabel.Text = sdpLw;
        HdpPwShortPxLabel.Text = hdpPw;
        WdpLhShortPxLabel.Text = wdpLh;
        SdpPhaShortPxLabel.Text = sdpPha;
        SdpLwaShortPxLabel.Text = sdpLwa;
        SdpLhaShortPxLabel.Text = sdpLha;
        SdpPwaShortPxLabel.Text = sdpPwa;

        SdpPhPxLabel.Text = sdpPh;
        SdpLwPxLabel.Text = sdpLw;
        HdpPwPxLabel.Text = hdpPw;
        WdpLhPxLabel.Text = wdpLh;
        SdpPhaPxLabel.Text = sdpPha;
        SdpLwaPxLabel.Text = sdpLwa;
        SdpLhaPxLabel.Text = sdpLha;
        SdpPwaPxLabel.Text = sdpPwa;
    }

    private void BuildCodeCards()
    {
        CardsHost.Children.Clear();
        TypographyCardsHost.Children.Clear();
        var accent = (Color)Application.Current!.Resources["PrimaryLight"];
        var typographyAccent = (Color)Application.Current.Resources["SspColor"];

        foreach (var (title, code, shortMarkup, longMarkup, index, resolve) in Samples)
        {
            CardsHost.Children.Add(new DimenSampleCard
            {
                Title = title,
                Subtitle = $"{code}\n{shortMarkup}  ·  {longMarkup}",
                ResolvedPx = resolve(index),
                AccentColor = accent,
            });
        }

        foreach (var (title, code, shortMarkup, longMarkup, index, resolve) in TypographyArSamples)
        {
            TypographyCardsHost.Children.Add(new DimenSampleCard
            {
                Title = title,
                Subtitle = $"{code}\n{shortMarkup}  ·  {longMarkup}",
                ResolvedPx = resolve(index),
                AccentColor = typographyAccent,
            });
        }
    }
}

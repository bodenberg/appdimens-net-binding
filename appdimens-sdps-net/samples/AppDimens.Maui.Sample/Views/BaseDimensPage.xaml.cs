using AppDimens.Maui.Extensions;
using AppDimens.Maui.Sample.Controls;
using AppDimens.Maui.Sample.Services;

namespace AppDimens.Maui.Sample.Views;

public partial class BaseDimensPage : ContentPage
{
    private static readonly (string Title, string Code, string ShortMarkup, string LongMarkup, int Index, Func<int, double> Resolve, string ColorKey)[] Samples =
    [
        ("SDP", "16.Sdp()", "{sdp:16}", "{dimen:Sdp Value=16}", 16, v => v.Sdp(), "SdpColor"),
        ("SSP", "18.Ssp()", "{ssp:18}", "{dimen:Ssp Value=18}", 18, v => v.Ssp(), "SspColor"),
        ("HDP", "48.Hdp()", "{hdp:48}", "{dimen:Hdp Value=48}", 48, v => v.Hdp(), "HdpColor"),
        ("WDP", "120.Wdp()", "{wdp:120}", "{dimen:Wdp Value=120}", 120, v => v.Wdp(), "WdpColor"),
        ("SEM", "18.Sem()", "{sem:18}", "{dimen:Sem Value=18}", 18, v => v.Sem(), "SspColor"),
        ("HEM", "18.Hem()", "{hem:18}", "{dimen:Hem Value=18}", 18, v => v.Hem(), "HdpColor"),
        ("WEM", "18.Wem()", "{wem:18}", "{dimen:Wem Value=18}", 18, v => v.Wem(), "WdpColor"),
        ("SDPA", "16.Sdpa()", "{sdpa:16}", "{dimen:Sdpa Value=16}", 16, v => v.Sdpa(), "ArColor"),
        ("HDPA", "16.Hdpa()", "{hdpa:16}", "{dimen:Hdpa Value=16}", 16, v => v.Hdpa(), "ArColor"),
        ("WDPA", "16.Wdpa()", "{wdpa:16}", "{dimen:Wdpa Value=16}", 16, v => v.Wdpa(), "ArColor"),
    ];

    public BaseDimensPage()
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
        var sdp = $"{16.Sdp():F1} px";
        var hdp = $"{48.Hdp():F1} px";
        var wdp = $"{120.Wdp():F1} px";
        var sdpa = $"{16.Sdpa():F1} px";

        SdpShortPxLabel.Text = sdp;
        HdpShortPxLabel.Text = hdp;
        WdpShortPxLabel.Text = wdp;
        SdpaShortPxLabel.Text = sdpa;

        SdpPxLabel.Text = sdp;
        HdpPxLabel.Text = hdp;
        WdpPxLabel.Text = wdp;
        SdpaPxLabel.Text = sdpa;
    }

    private void BuildCodeCards()
    {
        CardsHost.Children.Clear();

        foreach (var (title, code, shortMarkup, longMarkup, index, resolve, colorKey) in Samples)
        {
            var px = resolve(index);
            var accent = (Color)Application.Current!.Resources[colorKey];
            CardsHost.Children.Add(new DimenSampleCard
            {
                Title = title,
                Subtitle = $"{code}\n{shortMarkup}  ·  {longMarkup}",
                ResolvedPx = px,
                AccentColor = accent,
            });
        }
    }
}

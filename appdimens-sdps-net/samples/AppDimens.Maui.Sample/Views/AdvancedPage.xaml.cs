using AppDimens.Maui.Core;
using AppDimens.Maui.Extensions;
using AppDimens.Maui.Helpers;
using AppDimens.Maui.Sample.Controls;
using AppDimens.Maui.Sample.Services;

namespace AppDimens.Maui.Sample.Views;

public partial class AdvancedPage : ContentPage
{
    public AdvancedPage()
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
        RefreshXamlLabels();
        BuildCodeCards();
    }

    private void RefreshXamlLabels()
    {
        var scaled = AppDimens.Maui.Builders.Responsive.Value(14)
            .Tablet(18)
            .Landscape(16)
            .Desktop(20)
            .Sdp();
        var scaledText = $"resolvido: {scaled:F1} px";
        ScaledShortPxLabel.Text = scaledText;
        ScaledPxLabel.Text = scaledText;

        var rotate = $"{30.SdpRotate(45, orientation: OrientationQualifier.Landscape):F1} px";
        RotateShortPxLabel.Text = rotate;
        RotatePxLabel.Text = rotate;

        var hdpRotate = $"{48.HdpRotate(64, orientation: OrientationQualifier.Landscape):F1} px";
        HdpRotateShortPxLabel.Text = hdpRotate;
        HdpRotatePxLabel.Text = hdpRotate;

        var qualifier = $"{16.SdpQualifier(20, DpQualifier.SmallWidth, 600):F1} px";
        QualifierShortPxLabel.Text = qualifier;
        QualifierPxLabel.Text = qualifier;

        var mode = $"{16.SdpMode(24, UiModeType.Desk):F1} px";
        ModeShortPxLabel.Text = mode;
        ModePxLabel.Text = mode;

        var screen = $"{16.SdpScreen(20, UiModeType.Television, DpQualifier.SmallWidth, 600):F1} px";
        ScreenShortPxLabel.Text = screen;
        ScreenPxLabel.Text = screen;
    }

    private void BuildCodeCards()
    {
        CardsHost.Children.Clear();

        var accent = (Color)Application.Current!.Resources["Warning"];

        AddCard("SdpRotate",
            "30.SdpRotate(45, Landscape)\n{sdpRotate:30, Rotation=45, Orientation=Landscape}  ·  {dimen:SdpRotate Base=30, Rotation=45, Orientation=Landscape}",
            30.SdpRotate(45, orientation: OrientationQualifier.Landscape), accent);
        AddCard("HdpRotate",
            "48.HdpRotate(64, Landscape)\n{hdpRotate:48, Rotation=64, Orientation=Landscape}  ·  {dimen:HdpRotate Base=48, Rotation=64, Orientation=Landscape}",
            48.HdpRotate(64, orientation: OrientationQualifier.Landscape), accent);
        AddCard("WdpRotate",
            "120.WdpRotate(160, Portrait)\n{wdpRotate:120, Rotation=160, Orientation=Portrait}  ·  {dimen:WdpRotate Base=120, Rotation=160, Orientation=Portrait}",
            120.WdpRotate(160, orientation: OrientationQualifier.Portrait), accent);
        AddCard("SspRotate",
            "16.SspRotate(20, Landscape)\n{sspRotate:16, Rotation=20, Orientation=Landscape}  ·  {dimen:SspRotate Base=16, Rotation=20, Orientation=Landscape}",
            16.SspRotate(20, orientation: OrientationQualifier.Landscape), accent);
        AddCard("SemRotate",
            "16.SemRotate(20, Landscape)\n{semRotate:16, Rotation=20, Orientation=Landscape}  ·  {dimen:SemRotate Base=16, Rotation=20, Orientation=Landscape}",
            16.SemRotate(20, orientation: OrientationQualifier.Landscape), accent);
        AddCard("SdpQualifier",
            "16.SdpQualifier(20, sw≥600)\n{sdpQualifier:16, Qualified=20, QualifierType=SmallWidth, Threshold=600}  ·  {dimen:SdpQualifier Base=16, Qualified=20, QualifierType=SmallWidth, Threshold=600}",
            16.SdpQualifier(20, DpQualifier.SmallWidth, 600), accent);
        AddCard("SspQualifier",
            "18.SspQualifier(22, sw≥600)\n{sspQualifier:18, Qualified=22, QualifierType=SmallWidth, Threshold=600}  ·  {dimen:SspQualifier Base=18, Qualified=22, QualifierType=SmallWidth, Threshold=600}",
            18.SspQualifier(22, DpQualifier.SmallWidth, 600), accent);
        AddCard("SdpMode",
            "16.SdpMode(24, Desk)\n{sdpMode:16, Mode=24, UiMode=Desk}  ·  {dimen:SdpMode Base=16, Mode=24, UiMode=Desk}",
            16.SdpMode(24, UiModeType.Desk), accent);
        AddCard("SspMode",
            "18.SspMode(26, Television)\n{sspMode:18, Mode=26, UiMode=Television}  ·  {dimen:SspMode Base=18, Mode=26, UiMode=Television}",
            18.SspMode(26, UiModeType.Television), accent);
        AddCard("SdpScreen",
            "16.SdpScreen(20, TV, sw≥600)\n{sdpScreen:16, Screen=20, UiMode=Television, QualifierType=SmallWidth, Threshold=600}  ·  {dimen:SdpScreen Base=16, Screen=20, UiMode=Television, QualifierType=SmallWidth, Threshold=600}",
            16.SdpScreen(20, UiModeType.Television, DpQualifier.SmallWidth, 600), accent);
        AddCard("SspScreen",
            "18.SspScreen(22, TV, sw≥600)\n{sspScreen:18, Screen=22, UiMode=Television, QualifierType=SmallWidth, Threshold=600}  ·  {dimen:SspScreen Base=18, Screen=22, UiMode=Television, QualifierType=SmallWidth, Threshold=600}",
            18.SspScreen(22, UiModeType.Television, DpQualifier.SmallWidth, 600), accent);

        var scaled = AppDimens.Maui.Builders.Responsive.Value(14)
            .Tablet(18)
            .Landscape(16)
            .Desktop(20)
            .Sdp();
        AddCard("Responsive", "Value(14).Tablet(18)…\n{scaled:14, Tablet=18, …}  ·  {dimen:Scaled Value=14, …}", scaled, (Color)Application.Current!.Resources["Success"]!);

        CardsHost.Children.Add(new Label { Text = "Modos de escala", Style = (Style)Application.Current.Resources["SectionTitle"]! });
        foreach (ScalingMode mode in Enum.GetValues<ScalingMode>())
        {
            CardsHost.Children.Add(new Border
            {
                Style = (Style)Application.Current.Resources["Card"]!,
                Content = new Label
                {
                    Text = mode.ToString(),
                    TextColor = (Color)Application.Current.Resources["TextPrimary"]!,
                    FontSize = 14.Ssp(),
                },
            });
        }
    }

    private void AddCard(string title, string code, double px, Color accent) =>
        CardsHost.Children.Add(new DimenSampleCard
        {
            Title = title,
            Subtitle = code,
            ResolvedPx = px,
            AccentColor = accent,
        });
}

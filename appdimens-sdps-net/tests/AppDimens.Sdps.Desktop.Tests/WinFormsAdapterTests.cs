using System;
using System.Drawing;
using System.Windows.Forms;
using AppDimens.Maui;
using AppDimens.Sdps.WinForms;
using Xunit;

namespace AppDimens.Sdps.Desktop.Tests;

public class WinFormsAdapterTests
{
    [Fact]
    public void Initialize_uses_client_size_in_dp() => Sta.Run(() =>
    {
        using var form = new Form { ClientSize = new Size(800, 600) };
        AppDimensWinForms.Initialize(form);

        var density = AppDimensWinForms.Density;
        Assert.Equal(800 / density, AppDimensResolver.Instance.Metrics.Current.WidthDp, 3);
        Assert.Equal(600 / density, AppDimensResolver.Instance.Metrics.Current.HeightDp, 3);
        AppDimensWinForms.Detach();
    });

    [Fact]
    public void Px_and_pt_helpers_convert_dp() => Sta.Run(() =>
    {
        using var form = new Form { ClientSize = new Size(800, 600) };
        AppDimensWinForms.Initialize(form);

        var dp = AppDimensSdps.Sdp(120);
        Assert.Equal((int)Math.Round(dp * AppDimensWinForms.Density, MidpointRounding.AwayFromZero), 120.SdpPx());
        Assert.Equal((float)(AppDimensSdps.Ssp(14) * 0.75), 14.SspPt(), 3);
        Assert.Equal(new Padding(DimenPx.ToPx(dp)), DimenPx.Padding(dp));
        AppDimensWinForms.Detach();
    });

    [Fact]
    public void Apply_runs_now_and_after_resize() => Sta.Run(() =>
    {
        using var form = new Form { ClientSize = new Size(400, 800) };
        AppDimensWinForms.Initialize(form);
        var runs = 0;
        var width = 0;
        using var sub = AppDimensWinForms.Apply(form, () =>
        {
            runs++;
            width = 100.SdpPx();
        });
        var first = width;

        form.ClientSize = new Size(1200, 1600);
        AppDimensResolver.Instance.RefreshMetricsFromDevice();

        Assert.True(runs >= 2, $"runs={runs}");
        Assert.True(width > first, $"{width} > {first}");
        AppDimensWinForms.Detach();
    });
}

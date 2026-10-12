using System;
using System.Drawing;
using System.Windows.Forms;
using AppDimens.Net;
using AppDimens.Net.Code.Scaled;
using AppDimens.Net.WinForms;
using Xunit;

namespace AppDimens.Net.Desktop.Tests;

public class WinFormsAdapterTests
{
    [Fact]
    public void Initialize_uses_client_size_in_dp() => Sta.Run(() =>
    {
        using var form = new Form { ClientSize = new Size(800, 600) };
        var scope = AppDimensWinForms.Initialize(form);

        Assert.Same(scope, AppDimensAmbient.Current);
        Assert.Equal((int)Math.Round(800 / scope.Density), scope.Configuration.ScreenWidthDp);
        AppDimensWinForms.Detach();
    });

    [Fact]
    public void Px_helpers_use_form_density() => Sta.Run(() =>
    {
        using var form = new Form { ClientSize = new Size(800, 600) };
        var scope = AppDimensWinForms.Initialize(form);

        var dp = 120.Sdp();
        Assert.Equal((int)Math.Round(dp * scope.Density, MidpointRounding.AwayFromZero), DimenPx.ToPx(dp));
        Assert.Equal(DimenPx.ToPx(dp), DimenPx.Of(DimenFunction.Sdp, 120));
        Assert.Equal((float)(dp * 0.75), DimenPx.ToPt(dp), 3);
        AppDimensWinForms.Detach();
    });

    [Fact]
    public void Apply_runs_now_and_after_resize() => Sta.Run(() =>
    {
        using var form = new Form { ClientSize = new Size(400, 800) };
        var scope = AppDimensWinForms.Initialize(form);
        var runs = 0;
        var width = 0;
        using var sub = AppDimensWinForms.Apply(form, () =>
        {
            runs++;
            width = DimenPx.ToPx(100.Sdp());
        });
        var first = width;

        form.ClientSize = new Size(1200, 1600);
        scope.Refresh();

        Assert.True(runs >= 2, $"runs={runs}");
        Assert.True(width > first, $"{width} > {first}");
        AppDimensWinForms.Detach();
    });
}

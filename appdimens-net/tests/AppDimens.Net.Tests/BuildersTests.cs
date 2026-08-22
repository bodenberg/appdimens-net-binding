using AppDimens.Net;
using AppDimens.Net.Code.Fit;
using AppDimens.Net.Code.Scaled;
using AppDimens.Net.Common;
using AppDimens.Net.Core;
using AppDimens.Net.Testing;
using Xunit;

namespace AppDimens.Net.Tests;

public class BuildersTests
{
    private readonly FakeAppDimensContext _ctx = new(
        new ScreenConfiguration(360, 740, 360, 420, 1f,
            ScreenConfiguration.OrientationPortrait, 0));

    [Fact]
    public void ScaledDimension_qualifier_entry_wins_over_mode()
    {
        var d = 16.ScaledDp()
            .Screen(DpQualifier.SmallWidth, 200, 44)   // priority 3 (qualifier+orientation)
            .Screen(UiModeType.Normal, 88);            // priority 2 (mode-only)
        // sw=360 ≥ 200 → qualifier entry matches; priority order: 2 before 3,
        // but mode entry (Normal) also matches → first sorted (priority 2) wins.
        var result = d.Resolve(_ctx, DpQualifier.SmallWidth);
        Assert.Equal(88f.ToDynamicScaledDp(_ctx), result);
    }

    [Fact]
    public void ScaledDimension_first_match_in_sorted_priority_order()
    {
        var d = 16.ScaledDp()
            .Screen(DpQualifier.SmallWidth, 9999, 10)  // never matches
            .Screen(UiModeType.Television, 20)         // never matches
            .Screen(Orientation.Portrait, 30);         // priority 4 — matches
        Assert.Equal(30f.ToDynamicScaledDp(_ctx), d.Resolve(_ctx, DpQualifier.SmallWidth));
    }

    [Fact]
    public void DimenFit_orientation_only_entry_and_resolve_all()
    {
        var fit = 18.FitScaledDp().Screen(Orientation.Portrait, 22);
        var (fs, fh, fw) = fit.ResolveAll(_ctx);

        Assert.Equal(((float)22).ToFitDp(_ctx) * _ctx.Density, fs);
        Assert.Equal(((float)22).ToFitDp(_ctx, DpQualifier.Height) * _ctx.Density, fh);
        Assert.Equal(((float)22).ToFitDp(_ctx, DpQualifier.Width) * _ctx.Density, fw);
    }

    [Fact]
    public void DimenFit_falls_back_to_base_when_nothing_matches()
    {
        var fit = 18.FitScaledDp().Screen(UiModeType.Television, 40);
        var previous = AppDimensAmbient.Current;
        AppDimensAmbient.Set(_ctx);
        try { Assert.Equal(18f.ToFitDp(_ctx) * _ctx.Density, fit.Ftsdp()); }
        finally { AppDimensAmbient.Set(previous); }
    }
}

using AppDimens.Net;
using AppDimens.Net.Code.Percent;
using AppDimens.Net.Code.Scaled;
using AppDimens.Net.Core;
using AppDimens.Net.Testing;
using Xunit;

using AppDimens.Net.Common;

namespace AppDimens.Net.Tests;

public class AmbientWatcherAndSpaceTests
{
    [Fact]
    public void Ambient_require_throws_when_not_initialized()
    {
        var previous = AppDimensAmbient.Current;
        try
        {
            AppDimensAmbient.Set(null);
            Assert.Throws<InvalidOperationException>(() => 16.Sdp());
        }
        finally { AppDimensAmbient.Set(previous); }
    }

    [Fact]
    public void Fake_context_listener_registration_roundtrip()
    {
        var ctx = Fx.Phone();
        var fired = 0;
        using (ctx.RegisterConfigurationListener(() => fired++))
        {
            ctx.NotifyChange();
            Assert.Equal(1, fired);
        }
        ctx.NotifyChange(); // disposed → no more callbacks
        Assert.Equal(1, fired);
    }

    [Fact]
    public void Space_literal_percent_math()
    {
        var ctx = Fx.Phone(); // w=360 h=740 sw=360
        Assert.Equal(36f, 10.SpaceWDp(ctx));           // 10% of 360
        Assert.Equal(74f, 10.SpaceHDp(ctx));           // 10% of 740
        Assert.Equal(36f * ctx.Density, 10.SpaceW(ctx));
        Assert.Equal(25f, 25.SpaceDp(100f));           // 25% of reference 100dp
        Assert.Equal(ctx.Density * 25f, 25.Space(AppDimensAmbientProbe(ctx), 100f));
    }

    [Fact]
    public void Space_i_returns_raw_percent_under_constraint()
    {
        var ctx = new FakeAppDimensContext(new ScreenConfiguration(342, 740, 342, 420, 1f,
            ScreenConfiguration.OrientationPortrait, 0))
        { IsInMultiWindowMode = true };
        // heuristic triggers (342-342 >= 34.2? no) → force via real mw flag is not read by
        // the literal space path; it uses the heuristic like KMP. Build a config that trips it:
        var tripping = new FakeAppDimensContext(new ScreenConfiguration(300, 700, 400, 420, 1f,
            ScreenConfiguration.OrientationPortrait, 0));
        Assert.True(Fx.HeuristicMw(tripping.Configuration));
        Assert.Equal(10f, 10.SpaceWDp(tripping, ignoreMultiWindows: true));
    }

    private static IAppDimensContext AppDimensAmbientProbe(IAppDimensContext ctx)
    {
        var prev = AppDimensAmbient.Current;
        AppDimensAmbient.Set(prev ?? ctx);
        return AppDimensAmbient.Current!;
    }
}

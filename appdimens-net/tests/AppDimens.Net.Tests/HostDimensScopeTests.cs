using AppDimens.Net;
using AppDimens.Net.Code.Scaled;
using AppDimens.Net.Core;
using Xunit;

namespace AppDimens.Net.Tests;

/// <summary>
/// <see cref="HostDimensScope"/> is the base of the WPF / Windows Forms / WinUI 3 window scopes:
/// pushing a new window size must refresh ambient values through the cache watcher.
/// </summary>
public class HostDimensScopeTests
{
    [Fact]
    public void Update_builds_configuration_from_dips()
    {
        using var scope = new HostDimensScope();
        Assert.True(scope.Update(1280, 720, 1.5));

        var cfg = scope.Configuration;
        Assert.Equal(1280, cfg.ScreenWidthDp);
        Assert.Equal(720, cfg.ScreenHeightDp);
        Assert.Equal(720, cfg.SmallestScreenWidthDp);
        Assert.Equal(240, cfg.DensityDpi);
        Assert.Equal(ScreenConfiguration.OrientationLandscape, cfg.Orientation);
        Assert.Equal(1.5f, scope.Density);
        Assert.False(scope.IsInMultiWindowMode);
    }

    [Fact]
    public void Update_ignores_empty_sizes_and_duplicates()
    {
        using var scope = new HostDimensScope();
        var fired = 0;
        using var _ = scope.RegisterConfigurationListener(() => fired++);

        Assert.False(scope.Update(0, 0, 1));
        Assert.True(scope.Update(800, 600, 1));
        Assert.False(scope.Update(800, 600, 1));
        Assert.Equal(1, fired);
    }

    [Fact]
    public void Ambient_values_follow_host_resize()
    {
        var previous = AppDimensAmbient.Current;
        using var scope = new HostDimensScope();
        try
        {
            scope.Update(400, 800, 1);
            AppDimensHost.Activate(scope);
            var small = 16.Sdp();
            Assert.Equal(16.Sdp(scope), small, 3);

            scope.Update(1200, 1600, 1);
            var large = 16.Sdp();

            Assert.True(large > small, $"expected {large} > {small}");
            Assert.Equal(16.Sdp(scope), large, 3);
        }
        finally
        {
            AppDimensAmbient.Set(previous);
            if (previous != null) DimenCache.Init(previous);
        }
    }

    [Fact]
    public void Disposed_scope_stops_updating()
    {
        var scope = new HostDimensScope();
        scope.Update(500, 900, 1);
        scope.Dispose();

        Assert.False(scope.Update(900, 500, 1));
        Assert.Equal(500, scope.Configuration.ScreenWidthDp);
    }
}

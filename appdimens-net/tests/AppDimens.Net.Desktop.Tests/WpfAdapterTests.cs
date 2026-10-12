using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using AppDimens.Net;
using AppDimens.Net.Code.Fluid;
using AppDimens.Net.Code.Scaled;
using AppDimens.Net.Wpf;
using Xunit;

namespace AppDimens.Net.Desktop.Tests;

public class WpfAdapterTests
{
    [Fact]
    public void Initialize_sets_ambient_window_scope() => Sta.Run(() =>
    {
        var window = new Window { Width = 1000, Height = 500 };
        var scope = AppDimensWpf.Initialize(window);

        Assert.Same(scope, AppDimensAmbient.Current);
        Assert.Equal(1000, scope.Configuration.ScreenWidthDp);
        Assert.Equal(500, scope.Configuration.ScreenHeightDp);
        AppDimensWpf.Detach();
        Assert.Null(AppDimensAmbient.Current);
    });

    [Fact]
    public void Resize_refreshes_ambient_values() => Sta.Run(() =>
    {
        var window = new Window { Width = 400, Height = 800 };
        var scope = AppDimensWpf.Initialize(window);
        var before = 16.Sdp();
        var raised = 0;
        EventHandler handler = (_, _) => raised++;
        AppDimensWpf.Changed += handler;
        try
        {
            window.Width = 1200;
            window.Height = 1600;
            scope.Refresh();

            Assert.True(raised > 0);
            Assert.True(16.Sdp() > before);
        }
        finally
        {
            AppDimensWpf.Changed -= handler;
            AppDimensWpf.Detach();
        }
    });

    [Fact]
    public void Markup_extensions_resolve_scaled_and_fluid() => Sta.Run(() =>
    {
        AppDimensWpf.Initialize(new Window { Width = 600, Height = 900 });

        Assert.Equal(16.Sdp(), (double)new SdpExtension(16) { Live = false }.ProvideValue(null!), 4);
        Assert.Equal(24.FSdp(), (double)new FSdpExtension(24) { Live = false }.ProvideValue(null!), 4);
        Assert.Equal(new Thickness(8.Sdp()), new SdpExtension(8) { Live = false, As = DimenTarget.Thickness }.ProvideValue(null!));
        AppDimensWpf.Detach();
    });

    [Fact]
    public void Live_binding_follows_resize() => Sta.Run(() =>
    {
        var window = new Window { Width = 400, Height = 800 };
        var scope = AppDimensWpf.Initialize(window);
        var border = new Border();
        BindingOperations.SetBinding(border, FrameworkElement.WidthProperty, new Binding(nameof(DimenLiveSource.Version))
        {
            Source = DimenLiveSource.Instance,
            Converter = new DimenConverter { Function = DimenFunction.Sdp, Value = 100 },
        });
        var before = border.Width;

        window.Width = 1200;
        window.Height = 1600;
        scope.Refresh();

        Assert.Equal(100.Sdp(), border.Width, 4);
        Assert.True(border.Width > before);
        AppDimensWpf.Detach();
    });
}

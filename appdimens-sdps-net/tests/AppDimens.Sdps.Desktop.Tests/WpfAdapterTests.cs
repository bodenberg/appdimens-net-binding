using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using AppDimens.Maui;
using AppDimens.Sdps.Wpf;
using Xunit;

namespace AppDimens.Sdps.Desktop.Tests;

public class WpfAdapterTests
{
    [Fact]
    public void Initialize_tracks_window_size() => Sta.Run(() =>
    {
        var window = new Window { Width = 1000, Height = 500 };
        AppDimensWpf.Initialize(window);

        Assert.True(AppDimensWpf.IsInitialized);
        Assert.Same(window, AppDimensWpf.TrackedWindow);
        Assert.Equal(1000, AppDimensResolver.Instance.Metrics.Current.WidthDp);
        Assert.Equal(500, AppDimensResolver.Instance.Metrics.Current.HeightDp);
        AppDimensWpf.Detach();
    });

    [Fact]
    public void Resize_updates_values_and_raises_changed() => Sta.Run(() =>
    {
        var window = new Window { Width = 400, Height = 800 };
        AppDimensWpf.Initialize(window);
        var before = 16.Sdp();
        var raised = 0;
        EventHandler handler = (_, _) => raised++;
        AppDimensWpf.Changed += handler;
        try
        {
            window.Width = 1200;
            window.Height = 1600;
            AppDimensWpf.Refresh();

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
    public void Markup_extension_converts_to_target_type() => Sta.Run(() =>
    {
        AppDimensWpf.Initialize(new Window { Width = 600, Height = 900 });
        var expected = AppDimensSdps.Sdp(16);

        Assert.Equal(expected, (double)new SdpExtension(16) { Live = false }.ProvideValue(null!), 6);
        Assert.Equal(new Thickness(expected), new SdpExtension(16) { Live = false, As = DimenTarget.Thickness }.ProvideValue(null!));
        Assert.Equal(new CornerRadius(expected), new SdpExtension(16) { Live = false, As = DimenTarget.CornerRadius }.ProvideValue(null!));
        Assert.Equal(new GridLength(expected), new SdpExtension(16) { Live = false, As = DimenTarget.GridLength }.ProvideValue(null!));
        AppDimensWpf.Detach();
    });

    [Fact]
    public void Live_binding_follows_resize() => Sta.Run(() =>
    {
        var window = new Window { Width = 400, Height = 800 };
        AppDimensWpf.Initialize(window);
        var border = new Border();
        BindingOperations.SetBinding(border, FrameworkElement.WidthProperty, new Binding(nameof(DimenLiveSource.Version))
        {
            Source = DimenLiveSource.Instance,
            Converter = new DimenConverter { Function = DimenFunction.Sdp, Value = 100 },
        });
        var before = border.Width;

        window.Width = 1200;
        window.Height = 1600;
        AppDimensWpf.Refresh();

        Assert.Equal(AppDimensSdps.Sdp(100), border.Width, 6);
        Assert.True(border.Width > before);
        AppDimensWpf.Detach();
    });

    [Fact]
    public void Converter_reads_bound_number_and_parameter() => Sta.Run(() =>
    {
        AppDimensWpf.Initialize(new Window { Width = 700, Height = 1000 });
        var converter = new DimenConverter();

        var value = converter.Convert(24, typeof(double), "Ssp", System.Globalization.CultureInfo.InvariantCulture);

        Assert.Equal(AppDimensSdps.Ssp(24), (double)value, 6);
        AppDimensWpf.Detach();
    });
}

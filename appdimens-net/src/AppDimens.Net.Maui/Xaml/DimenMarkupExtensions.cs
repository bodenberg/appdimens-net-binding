using AppDimens.Net.Code.Fluid;
using AppDimens.Net.Code.Percent;
using AppDimens.Net.Code.Scaled;
using AppDimens.Net.Core;
using Microsoft.Maui.Controls.Xaml;

namespace AppDimens.Net.Maui.Xaml;

/// <summary>Base for dimension markup extensions: resolves through the ambient context.</summary>
public abstract class DimenMarkupExtension : IMarkupExtension<double>
{
    public double Value { get; set; }
    public bool AspectRatio { get; set; }
    public bool IgnoreMultiWindows { get; set; }

    public double ProvideValue(IServiceProvider serviceProvider) => Resolve((float)Value);

    object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) => ProvideValue(serviceProvider);

    protected abstract float Resolve(float value);
}

/// <summary>{appdimens:Sdp Value=16} — smallest-width scaled.</summary>
[ContentProperty(nameof(Value))]
public sealed class SdpExtension : DimenMarkupExtension
{
    protected override float Resolve(float v) =>
        AspectRatio || IgnoreMultiWindows
            ? v.ToDynamicScaledDp(AppDimensAmbient.Require(), ignoreMultiWindows: IgnoreMultiWindows, applyAspectRatio: AspectRatio)
            : ((int)v).Sdp();
}

/// <summary>{appdimens:Sdpa Value=16} — with aspect-ratio curve.</summary>
[ContentProperty(nameof(Value))]
public sealed class SdpaExtension : DimenMarkupExtension
{
    protected override float Resolve(float v) => ((int)v).Sdpa();
}

/// <summary>{appdimens:Hdp Value=48} — height axis.</summary>
[ContentProperty(nameof(Value))]
public sealed class HdpExtension : DimenMarkupExtension
{
    protected override float Resolve(float v) => ((int)v).Hdp();
}

/// <summary>{appdimens:Wdp Value=100} — width axis.</summary>
[ContentProperty(nameof(Value))]
public sealed class WdpExtension : DimenMarkupExtension
{
    protected override float Resolve(float v) => ((int)v).Wdp();
}

/// <summary>{appdimens:Ssp Value=16} — scalable text.</summary>
[ContentProperty(nameof(Value))]
public sealed class SspExtension : DimenMarkupExtension
{
    protected override float Resolve(float v) => ((int)v).Ssp();
}

/// <summary>{appdimens:Fsdp Value=24} — fluid strategy.</summary>
[ContentProperty(nameof(Value))]
public sealed class FsdpExtension : DimenMarkupExtension
{
    protected override float Resolve(float v) => ((float)v).ToFluidDp(AppDimensAmbient.Require());
}

/// <summary>{appdimens:Psdpx Value=50} — literal percent of width (Value = %).</summary>
[ContentProperty(nameof(Value))]
public sealed class PsdpxExtension : DimenMarkupExtension
{
    protected override float Resolve(float v) => ((int)v).SpaceWDp(AppDimensAmbient.Require());
}

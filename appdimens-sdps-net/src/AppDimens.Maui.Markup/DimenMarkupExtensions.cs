using System.Reflection;
using AppDimens.Maui.Core;
using AppDimens.Maui.Helpers;
using AppDimens.Maui.Inverters;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace AppDimens.Maui.Markup;

[ContentProperty(nameof(Value))]
[AcceptEmptyServiceProvider]
#if NET9_0_OR_GREATER
[RequireService([typeof(IProvideValueTarget)])]
#endif
public abstract class DimenMarkupExtensionBase : IMarkupExtension
{
    public int Value { get; set; }
    protected abstract double Resolve(AppDimensResolver resolver, int value);

    public object ProvideValue(IServiceProvider serviceProvider)
    {
        var px = Resolve(AppDimensResolver.Instance, Value);
        return MarkupThicknessHelper.CoerceForTarget(serviceProvider, px);
    }
}

internal static class MarkupThicknessHelper
{
    public static object CoerceForTarget(IServiceProvider? serviceProvider, double px)
    {
        if (serviceProvider is null ||
            serviceProvider.GetService(typeof(IProvideValueTarget)) is not IProvideValueTarget target)
            return px;

        return CoerceForProperty(target.TargetProperty, px);
    }

    private static object CoerceForProperty(object? targetProperty, double px) =>
        targetProperty switch
        {
            BindableProperty bp when bp.PropertyName is "Padding" or "Margin" => new Thickness(px),
            BindableProperty bp when bp.ReturnType == typeof(string) => px.ToString(System.Globalization.CultureInfo.InvariantCulture),
            PropertyInfo pi when pi.Name is "Padding" or "Margin" => new Thickness(px),
            PropertyInfo pi when pi.PropertyType == typeof(string) => px.ToString(System.Globalization.CultureInfo.InvariantCulture),
            _ => px,
        };
}

public sealed class SdpExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Sdp(v);
}

public sealed class SspExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Ssp(v);
}

public sealed class HdpExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Hdp(v);
}

public sealed class WdpExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Wdp(v);
}

public sealed class SdpaExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Sdpa(v);
}

public sealed class HdpaExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) =>
        r.Resolve(v, DpQualifier.Height, applyAspectRatio: true);
}

public sealed class WdpaExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) =>
        r.Resolve(v, DpQualifier.Width, applyAspectRatio: true);
}

public sealed class SemExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Sem(v);
}

public sealed class HemExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) =>
        r.Resolve(v, DpQualifier.Height, allowNegative: false);
}

public sealed class WemExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) =>
        r.Resolve(v, DpQualifier.Width, allowNegative: false);
}

public sealed class SdpPhExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Sdp(v, InverterEngine.ForSdpPh());
}

public sealed class SdpLwExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Sdp(v, InverterEngine.ForSdpLw());
}

public sealed class SdpLhExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Sdp(v, InverterEngine.ForSdpLh());
}

public sealed class SdpPwExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Sdp(v, InverterEngine.ForSdpPw());
}

public sealed class HdpLwExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Hdp(v, InverterEngine.ForHdpLw());
}

public sealed class HdpPwExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Hdp(v, InverterEngine.ForHdpPw());
}

public sealed class WdpLhExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Wdp(v, InverterEngine.ForWdpLh());
}

public sealed class WdpPhExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Wdp(v, InverterEngine.ForWdpPh());
}

public sealed class SdpPhAExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Sdpa(v, InverterEngine.ForSdpPh());
}

public sealed class SdpLwAExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Sdpa(v, InverterEngine.ForSdpLw());
}

public sealed class SdpLhAExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Sdpa(v, InverterEngine.ForSdpLh());
}

public sealed class SdpPwAExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Sdpa(v, InverterEngine.ForSdpPw());
}

[ContentProperty(nameof(Value))]
[AcceptEmptyServiceProvider]
public sealed class ScaledExtension : IMarkupExtension
{
    public int Value { get; set; }
    public int? Tablet { get; set; }
    public int? Landscape { get; set; }
    public int? Desktop { get; set; }

    public object ProvideValue(IServiceProvider serviceProvider)
    {
        var builder = Builders.Responsive.Value(Value);
        if (Tablet.HasValue) builder = builder.Tablet(Tablet.Value);
        if (Landscape.HasValue) builder = builder.Landscape(Landscape.Value);
        if (Desktop.HasValue) builder = builder.Desktop(Desktop.Value);
        return MarkupThicknessHelper.CoerceForTarget(serviceProvider, builder.Sdp());
    }
}

[ContentProperty(nameof(Base))]
[AcceptEmptyServiceProvider]
public sealed class SdpRotateExtension : DimenRotateExtensionBase
{
    protected override double Resolve() => Base.SdpRotate(Rotation, orientation: Orientation);
}

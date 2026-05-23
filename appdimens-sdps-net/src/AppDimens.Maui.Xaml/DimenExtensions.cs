using System.Reflection;
using AppDimens.Maui.Builders;
using AppDimens.Maui.Core;
using AppDimens.Maui.Helpers;
using AppDimens.Maui.Inverters;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace AppDimens.Maui.Xaml;

public static class DimenMarkupResolver
{
    public static double Resolve(DimenMarkupKind kind, int value, InverterType inverter = InverterType.Default)
    {
        var r = AppDimensResolver.Instance;
        return kind switch
        {
            DimenMarkupKind.Sdp => r.Sdp(value, inverter),
            DimenMarkupKind.Ssp => r.Ssp(value, inverter),
            DimenMarkupKind.Hdp => r.Hdp(value, inverter),
            DimenMarkupKind.Wdp => r.Wdp(value, inverter),
            DimenMarkupKind.Hsp => r.Hsp(value, inverter),
            DimenMarkupKind.Wsp => r.Wsp(value, inverter),
            DimenMarkupKind.Sdpa => r.Sdpa(value, inverter),
            DimenMarkupKind.Hdpa => r.Resolve(value, DpQualifier.Height, inverter, applyAspectRatio: true),
            DimenMarkupKind.Wdpa => r.Resolve(value, DpQualifier.Width, inverter, applyAspectRatio: true),
            DimenMarkupKind.Sspa => r.Sspa(value, inverter),
            DimenMarkupKind.Hspa => r.Hspa(value, inverter),
            DimenMarkupKind.Wspa => r.Wspa(value, inverter),
            DimenMarkupKind.Sem => r.Sem(value, inverter),
            DimenMarkupKind.Hem => r.Hem(value, inverter),
            DimenMarkupKind.Wem => r.Wem(value, inverter),
            DimenMarkupKind.Sema => r.Sema(value, inverter),
            DimenMarkupKind.Hema => r.Hema(value, inverter),
            DimenMarkupKind.Wema => r.Wema(value, inverter),
            _ => r.Sdp(value, inverter),
        };
    }
}

public enum DimenMarkupKind
{
    Sdp, Ssp, Hdp, Wdp, Hsp, Wsp,
    Sdpa, Hdpa, Wdpa, Sspa, Hspa, Wspa,
    Sem, Hem, Wem, Sema, Hema, Wema,
}

[ContentProperty(nameof(Value))]
#if NET9_0_OR_GREATER
[RequireService([typeof(IProvideValueTarget)])]
#endif
public abstract class DimenExtensionBase : IMarkupExtension
{
    public int Value { get; set; }
    public InverterType Inverter { get; set; } = InverterType.Default;
    protected abstract DimenMarkupKind Kind { get; }

    public object ProvideValue(IServiceProvider serviceProvider)
    {
        var px = DimenMarkupResolver.Resolve(Kind, Value, Inverter);
        if (serviceProvider is null ||
            serviceProvider.GetService(typeof(IProvideValueTarget)) is not IProvideValueTarget target)
            return px;

        return DimenThicknessCoercion.CoerceForProperty(target.TargetProperty, px);
    }
}

internal static class DimenThicknessCoercion
{
    internal static object CoerceForProperty(object? targetProperty, double px) =>
        targetProperty switch
        {
            BindableProperty bp when bp.PropertyName is "Padding" or "Margin" => new Thickness(px),
            BindableProperty bp when bp.ReturnType == typeof(string) => px.ToString(System.Globalization.CultureInfo.InvariantCulture),
            PropertyInfo pi when pi.Name is "Padding" or "Margin" => new Thickness(px),
            PropertyInfo pi when pi.PropertyType == typeof(string) => px.ToString(System.Globalization.CultureInfo.InvariantCulture),
            _ => px,
        };
}

public sealed class SdpExtension : DimenExtensionBase { protected override DimenMarkupKind Kind => DimenMarkupKind.Sdp; }
public sealed class SspExtension : DimenExtensionBase { protected override DimenMarkupKind Kind => DimenMarkupKind.Ssp; }
public sealed class HspExtension : DimenExtensionBase { protected override DimenMarkupKind Kind => DimenMarkupKind.Hsp; }
public sealed class WspExtension : DimenExtensionBase { protected override DimenMarkupKind Kind => DimenMarkupKind.Wsp; }
public sealed class HdpExtension : DimenExtensionBase { protected override DimenMarkupKind Kind => DimenMarkupKind.Hdp; }
public sealed class WdpExtension : DimenExtensionBase { protected override DimenMarkupKind Kind => DimenMarkupKind.Wdp; }
public sealed class SdpaExtension : DimenExtensionBase { protected override DimenMarkupKind Kind => DimenMarkupKind.Sdpa; }
public sealed class HdpaExtension : DimenExtensionBase { protected override DimenMarkupKind Kind => DimenMarkupKind.Hdpa; }
public sealed class WdpaExtension : DimenExtensionBase { protected override DimenMarkupKind Kind => DimenMarkupKind.Wdpa; }
public sealed class SspaExtension : DimenExtensionBase { protected override DimenMarkupKind Kind => DimenMarkupKind.Sspa; }
public sealed class HspaExtension : DimenExtensionBase { protected override DimenMarkupKind Kind => DimenMarkupKind.Hspa; }
public sealed class WspaExtension : DimenExtensionBase { protected override DimenMarkupKind Kind => DimenMarkupKind.Wspa; }
public sealed class SemExtension : DimenExtensionBase { protected override DimenMarkupKind Kind => DimenMarkupKind.Sem; }
public sealed class HemExtension : DimenExtensionBase { protected override DimenMarkupKind Kind => DimenMarkupKind.Hem; }
public sealed class WemExtension : DimenExtensionBase { protected override DimenMarkupKind Kind => DimenMarkupKind.Wem; }
public sealed class SemaExtension : DimenExtensionBase { protected override DimenMarkupKind Kind => DimenMarkupKind.Sema; }
public sealed class HemaExtension : DimenExtensionBase { protected override DimenMarkupKind Kind => DimenMarkupKind.Hema; }
public sealed class WemaExtension : DimenExtensionBase { protected override DimenMarkupKind Kind => DimenMarkupKind.Wema; }

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
        var builder = AppDimens.Maui.Builders.Responsive.Value(Value);
        if (Tablet.HasValue) builder = builder.Tablet(Tablet.Value);
        if (Landscape.HasValue) builder = builder.Landscape(Landscape.Value);
        if (Desktop.HasValue) builder = builder.Desktop(Desktop.Value);
        return builder.Sdp();
    }
}

[AcceptEmptyServiceProvider]
public sealed class SdpRotateExtension : DimenRotateExtensionBase
{
    protected override double Resolve() => Base.SdpRotate(Rotation, orientation: Orientation);
}

public sealed class SspRotateExtension : DimenRotateExtensionBase
{
    protected override double Resolve() => Base.SspRotate(Rotation, orientation: Orientation);
}

public sealed class HspRotateExtension : DimenRotateExtensionBase
{
    protected override double Resolve() => Base.HspRotate(Rotation, orientation: Orientation);
}

public sealed class WspRotateExtension : DimenRotateExtensionBase
{
    protected override double Resolve() => Base.WspRotate(Rotation, orientation: Orientation);
}

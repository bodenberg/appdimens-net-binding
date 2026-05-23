using AppDimens.Maui.Core;
using AppDimens.Maui.Helpers;
using Microsoft.Maui.Controls.Xaml;

namespace AppDimens.Maui.Markup;

[ContentProperty(nameof(Base))]
[AcceptEmptyServiceProvider]
public abstract class DimenRotateExtensionBase : IMarkupExtension
{
    public int Base { get; set; }
    public int Rotation { get; set; }
    public OrientationQualifier Orientation { get; set; } = OrientationQualifier.Landscape;

    protected abstract double Resolve();

    public object ProvideValue(IServiceProvider serviceProvider) =>
        MarkupThicknessHelper.CoerceForTarget(serviceProvider, Resolve());
}

public sealed class HdpRotateExtension : DimenRotateExtensionBase
{
    protected override double Resolve() => Base.HdpRotate(Rotation, orientation: Orientation);
}

public sealed class WdpRotateExtension : DimenRotateExtensionBase
{
    protected override double Resolve() => Base.WdpRotate(Rotation, orientation: Orientation);
}

public sealed class HemRotateExtension : DimenRotateExtensionBase
{
    protected override double Resolve() => Base.HemRotate(Rotation, orientation: Orientation);
}

public sealed class WemRotateExtension : DimenRotateExtensionBase
{
    protected override double Resolve() => Base.WemRotate(Rotation, orientation: Orientation);
}

[ContentProperty(nameof(Base))]
[AcceptEmptyServiceProvider]
public abstract class DimenModeExtensionBase : IMarkupExtension
{
    public int Base { get; set; }
    public int Mode { get; set; }
    public UiModeType UiMode { get; set; } = UiModeType.Normal;
    public DpQualifier? FinalQualifier { get; set; }

    protected abstract double Resolve();

    public object ProvideValue(IServiceProvider serviceProvider) =>
        MarkupThicknessHelper.CoerceForTarget(serviceProvider, Resolve());
}

public sealed class SdpModeExtension : DimenModeExtensionBase
{
    protected override double Resolve() =>
        FinalQualifier.HasValue ? Base.SdpMode(Mode, UiMode, FinalQualifier.Value) : Base.SdpMode(Mode, UiMode);
}

public sealed class SspModeExtension : DimenModeExtensionBase
{
    protected override double Resolve() =>
        FinalQualifier.HasValue ? Base.SspMode(Mode, UiMode, FinalQualifier.Value) : Base.SspMode(Mode, UiMode);
}

public sealed class HspModeExtension : DimenModeExtensionBase
{
    protected override double Resolve() =>
        FinalQualifier.HasValue ? Base.HspMode(Mode, UiMode, FinalQualifier.Value) : Base.HspMode(Mode, UiMode);
}

public sealed class WspModeExtension : DimenModeExtensionBase
{
    protected override double Resolve() =>
        FinalQualifier.HasValue ? Base.WspMode(Mode, UiMode, FinalQualifier.Value) : Base.WspMode(Mode, UiMode);
}

public sealed class SemModeExtension : DimenModeExtensionBase
{
    protected override double Resolve() =>
        FinalQualifier.HasValue ? Base.SemMode(Mode, UiMode, FinalQualifier.Value) : Base.SemMode(Mode, UiMode);
}

public sealed class HemModeExtension : DimenModeExtensionBase
{
    protected override double Resolve() =>
        FinalQualifier.HasValue ? Base.HemMode(Mode, UiMode, FinalQualifier.Value) : Base.HemMode(Mode, UiMode);
}

public sealed class WemModeExtension : DimenModeExtensionBase
{
    protected override double Resolve() =>
        FinalQualifier.HasValue ? Base.WemMode(Mode, UiMode, FinalQualifier.Value) : Base.WemMode(Mode, UiMode);
}

[ContentProperty(nameof(Base))]
[AcceptEmptyServiceProvider]
public abstract class DimenQualifierExtensionBase : IMarkupExtension
{
    public int Base { get; set; }
    public int Qualified { get; set; }
    public DpQualifier QualifierType { get; set; } = DpQualifier.SmallWidth;
    public int Threshold { get; set; }
    public DpQualifier? FinalQualifier { get; set; }

    protected abstract double Resolve();

    public object ProvideValue(IServiceProvider serviceProvider) =>
        MarkupThicknessHelper.CoerceForTarget(serviceProvider, Resolve());
}

public sealed class SdpQualifierExtension : DimenQualifierExtensionBase
{
    protected override double Resolve() =>
        FinalQualifier.HasValue
            ? Base.SdpQualifier(Qualified, QualifierType, Threshold, FinalQualifier.Value)
            : Base.SdpQualifier(Qualified, QualifierType, Threshold);
}

public sealed class SspQualifierExtension : DimenQualifierExtensionBase
{
    protected override double Resolve() =>
        FinalQualifier.HasValue
            ? Base.SspQualifier(Qualified, QualifierType, Threshold, FinalQualifier.Value)
            : Base.SspQualifier(Qualified, QualifierType, Threshold);
}

public sealed class HspQualifierExtension : DimenQualifierExtensionBase
{
    protected override double Resolve() =>
        FinalQualifier.HasValue
            ? Base.HspQualifier(Qualified, QualifierType, Threshold, FinalQualifier.Value)
            : Base.HspQualifier(Qualified, QualifierType, Threshold);
}

public sealed class WspQualifierExtension : DimenQualifierExtensionBase
{
    protected override double Resolve() =>
        FinalQualifier.HasValue
            ? Base.WspQualifier(Qualified, QualifierType, Threshold, FinalQualifier.Value)
            : Base.WspQualifier(Qualified, QualifierType, Threshold);
}

public sealed class SemQualifierExtension : DimenQualifierExtensionBase
{
    protected override double Resolve() =>
        FinalQualifier.HasValue
            ? Base.SemQualifier(Qualified, QualifierType, Threshold, FinalQualifier.Value)
            : Base.SemQualifier(Qualified, QualifierType, Threshold);
}

public sealed class HemQualifierExtension : DimenQualifierExtensionBase
{
    protected override double Resolve() =>
        FinalQualifier.HasValue
            ? Base.HemQualifier(Qualified, QualifierType, Threshold, FinalQualifier.Value)
            : Base.HemQualifier(Qualified, QualifierType, Threshold);
}

public sealed class WemQualifierExtension : DimenQualifierExtensionBase
{
    protected override double Resolve() =>
        FinalQualifier.HasValue
            ? Base.WemQualifier(Qualified, QualifierType, Threshold, FinalQualifier.Value)
            : Base.WemQualifier(Qualified, QualifierType, Threshold);
}

[ContentProperty(nameof(Base))]
[AcceptEmptyServiceProvider]
public abstract class DimenScreenExtensionBase : IMarkupExtension
{
    public int Base { get; set; }
    public int Screen { get; set; }
    public UiModeType UiMode { get; set; } = UiModeType.Normal;
    public DpQualifier QualifierType { get; set; } = DpQualifier.SmallWidth;
    public int Threshold { get; set; }
    public DpQualifier? FinalQualifier { get; set; }

    protected abstract double Resolve();

    public object ProvideValue(IServiceProvider serviceProvider) =>
        MarkupThicknessHelper.CoerceForTarget(serviceProvider, Resolve());
}

public sealed class SdpScreenExtension : DimenScreenExtensionBase
{
    protected override double Resolve() =>
        FinalQualifier.HasValue
            ? Base.SdpScreen(Screen, UiMode, QualifierType, Threshold, FinalQualifier.Value)
            : Base.SdpScreen(Screen, UiMode, QualifierType, Threshold);
}

public sealed class SspScreenExtension : DimenScreenExtensionBase
{
    protected override double Resolve() =>
        FinalQualifier.HasValue
            ? Base.SspScreen(Screen, UiMode, QualifierType, Threshold, FinalQualifier.Value)
            : Base.SspScreen(Screen, UiMode, QualifierType, Threshold);
}

public sealed class HspScreenExtension : DimenScreenExtensionBase
{
    protected override double Resolve() =>
        FinalQualifier.HasValue
            ? Base.HspScreen(Screen, UiMode, QualifierType, Threshold, FinalQualifier.Value)
            : Base.HspScreen(Screen, UiMode, QualifierType, Threshold);
}

public sealed class WspScreenExtension : DimenScreenExtensionBase
{
    protected override double Resolve() =>
        FinalQualifier.HasValue
            ? Base.WspScreen(Screen, UiMode, QualifierType, Threshold, FinalQualifier.Value)
            : Base.WspScreen(Screen, UiMode, QualifierType, Threshold);
}

public sealed class SemScreenExtension : DimenScreenExtensionBase
{
    protected override double Resolve() =>
        FinalQualifier.HasValue
            ? Base.SemScreen(Screen, UiMode, QualifierType, Threshold, FinalQualifier.Value)
            : Base.SemScreen(Screen, UiMode, QualifierType, Threshold);
}

public sealed class HemScreenExtension : DimenScreenExtensionBase
{
    protected override double Resolve() =>
        FinalQualifier.HasValue
            ? Base.HemScreen(Screen, UiMode, QualifierType, Threshold, FinalQualifier.Value)
            : Base.HemScreen(Screen, UiMode, QualifierType, Threshold);
}

public sealed class WemScreenExtension : DimenScreenExtensionBase
{
    protected override double Resolve() =>
        FinalQualifier.HasValue
            ? Base.WemScreen(Screen, UiMode, QualifierType, Threshold, FinalQualifier.Value)
            : Base.WemScreen(Screen, UiMode, QualifierType, Threshold);
}

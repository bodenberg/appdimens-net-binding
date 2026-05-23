using AppDimens.Maui.Core;
using AppDimens.Maui.Helpers;
using AppDimens.Maui.Inverters;

namespace AppDimens.Maui.Markup;

// Font and em markup extensions

public sealed class HspExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Hsp(v);
}

public sealed class WspExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Wsp(v);
}

public sealed class SspaExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Sspa(v);
}

public sealed class HspaExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Hspa(v);
}

public sealed class WspaExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Wspa(v);
}

public sealed class SemaExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Sema(v);
}

public sealed class HemaExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Hema(v);
}

public sealed class WemaExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Wema(v);
}

// SSP with axis inverters

public sealed class SspPhExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Ssp(v, InverterEngine.ForSdpPh());
}

public sealed class SspLwExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Ssp(v, InverterEngine.ForSdpLw());
}

public sealed class SspLhExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Ssp(v, InverterEngine.ForSdpLh());
}

public sealed class SspPwExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Ssp(v, InverterEngine.ForSdpPw());
}

// HSP with axis inverters

public sealed class HspLwExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Hsp(v, InverterEngine.ForHdpLw());
}

public sealed class HspPwExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Hsp(v, InverterEngine.ForHdpPw());
}

// WSP with axis inverters

public sealed class WspLhExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Wsp(v, InverterEngine.ForWdpLh());
}

public sealed class WspPhExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Wsp(v, InverterEngine.ForWdpPh());
}

// SEM with axis inverters

public sealed class SemPhExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Sem(v, InverterEngine.ForSdpPh());
}

public sealed class SemLwExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Sem(v, InverterEngine.ForSdpLw());
}

public sealed class SemLhExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Sem(v, InverterEngine.ForSdpLh());
}

public sealed class SemPwExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Sem(v, InverterEngine.ForSdpPw());
}

// HEM with axis inverters

public sealed class HemLwExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Hem(v, InverterEngine.ForHdpLw());
}

public sealed class HemPwExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Hem(v, InverterEngine.ForHdpPw());
}

// WEM with axis inverters

public sealed class WemLhExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Wem(v, InverterEngine.ForWdpLh());
}

public sealed class WemPhExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Wem(v, InverterEngine.ForWdpPh());
}

// SSP with aspect ratio and axis inverters

public sealed class SspPhAExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Sspa(v, InverterEngine.ForSdpPh());
}

public sealed class SspLwAExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Sspa(v, InverterEngine.ForSdpLw());
}

public sealed class SspLhAExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Sspa(v, InverterEngine.ForSdpLh());
}

public sealed class SspPwAExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Sspa(v, InverterEngine.ForSdpPw());
}

// HSP with aspect ratio and axis inverters

public sealed class HspLwAExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Hspa(v, InverterEngine.ForHdpLw());
}

public sealed class HspPwAExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Hspa(v, InverterEngine.ForHdpPw());
}

// WSP with aspect ratio and axis inverters

public sealed class WspLhAExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Wspa(v, InverterEngine.ForWdpLh());
}

public sealed class WspPhAExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Wspa(v, InverterEngine.ForWdpPh());
}

// SEMA with axis inverters

public sealed class SemaPhExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Sema(v, InverterEngine.ForSdpPh());
}

public sealed class SemaLwExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Sema(v, InverterEngine.ForSdpLw());
}

public sealed class SemaLhExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Sema(v, InverterEngine.ForSdpLh());
}

public sealed class SemaPwExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Sema(v, InverterEngine.ForSdpPw());
}

// HEMA with axis inverters

public sealed class HemaLwExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Hema(v, InverterEngine.ForHdpLw());
}

public sealed class HemaPwExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Hema(v, InverterEngine.ForHdpPw());
}

// WEMA with axis inverters

public sealed class WemaLhExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Wema(v, InverterEngine.ForWdpLh());
}

public sealed class WemaPhExtension : DimenMarkupExtensionBase
{
    protected override double Resolve(AppDimensResolver r, int v) => r.Wema(v, InverterEngine.ForWdpPh());
}

// Font rotation markup extensions

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

public sealed class SemRotateExtension : DimenRotateExtensionBase
{
    protected override double Resolve() => Base.SemRotate(Rotation, orientation: Orientation);
}

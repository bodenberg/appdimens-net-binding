---
name: appdimens-android-dynamic
description: Use the .NET for Android binding Bodenberg.AppDimens.Dynamic, which embeds the Kotlin AARs appdimens-dynamic + 13 strategy modules 3.2.0 — code-only responsive sizing with 15 strategies (scaled, percent, auto, fluid, power, diagonal, fill, fit, interpolated, logarithmic, perimeter, density, resize, units) via DimenSdp/DimenSsp, DimenScaled builders, DimenCache and DynamicConfig. Use when a net10.0-android (or MAUI Android head) project references this package or Com.Appdimens.Dynamic namespaces, or wants runtime scaling strategies on Android without XML @dimen grids.
---

# AppDimens Dynamic — .NET for Android binding

Binding of [appdimens-dynamic](https://github.com/bodenberg/appdimens-dynamic): **code-only** scaling (no pre-built
`@dimen` grids), 15 strategies, cache with DataStore persistence. NuGet **3.6.1**, AARs **3.2.0** (core + 13 modules),
`net10.0-android36.0`, min API **24**. **Android only** — for the same strategies on every platform (MAUI, WPF,
WinForms, WinUI, tests) use the pure C# port, skill `appdimens-net`. For XML `@dimen/_16sdp` use skill
`appdimens-android-sdps-ssps`.

```bash
dotnet add package Bodenberg.AppDimens.Dynamic --version 3.6.1
```

## Setup and the Scaled API (`Com.Appdimens.Dynamic.Code`) — values are **pixels** (`float`)

```csharp
using Android.Util;
using Com.Appdimens.Dynamic;          // DynamicConfig
using Com.Appdimens.Dynamic.Code;     // DimenSdp, DimenSsp, DimenScaled

protected override void OnCreate(Bundle? savedInstanceState)
{
    base.OnCreate(savedInstanceState);
    DynamicConfig.Warmup(this);                         // = DimenCache.Init(this); or DimenSdp.WarmupCache(this)
    int pad = (int)DimenSdp.Sdp(this, 16);
    card.SetPadding(pad, pad, pad, pad);
    title.SetTextSize(ComplexUnitType.Px, DimenSsp.Ssp(this, 18));   // NEVER title.TextSize = value
    DynamicConfig.DimensionsShouldRefresh += (_, _) => ApplySizes();
}

public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
{
    base.OnConfigurationChanged(newConfig);
    DynamicConfig.OnConfigurationChanged(this, newConfig);   // = DimenCache.InvalidateOnConfigChange(newConfig) + event
}
```

| Size… | Method |
|-------|--------|
| padding, margins, icons (default) | `DimenSdp.Sdp(ctx, 16)` |
| height / width axis | `DimenSdp.Hdp` / `DimenSdp.Wdp` |
| text | `DimenSsp.Ssp` / `Hsp` / `Wsp` |
| suffix `a` / `i` / `ia` | aspect ratio / don't shrink in split-screen / both (`Sdpa`, `Sdpi`, `Sspia`) |
| inverters | `SdpPh`, `SdpLw`, `HdpLw`, `WspLh`… (+`a`/`i`/`ia`) |

Facilitators and builder (enums in `Com.Appdimens.Dynamic.Common`: `DpQualifier`, `Inverter`, `Orientation`, `UiModeType`, `UnitType`):

```csharp
using Com.Appdimens.Dynamic.Common;
float rot  = DimenSdp.SdpRotate(this, 16, 24);                                    // landscape → 24
float tv   = DimenSsp.SspMode(this, 16, 22, UiModeType.Television);
float tab  = DimenSdp.SdpQualifier(this, 16, 20, DpQualifier.SmallWidth, 600);
float scr  = DimenSdp.SdpScreen(this, 16, 28, UiModeType.Television, DpQualifier.SmallWidth, 600);
float px   = DimenSdp.Scaled(16)                                                  // priority rules, not nesting order
                .Screen(UiModeType.Television, DpQualifier.SmallWidth, 600, 40f)
                .Screen(UiModeType.Television, 32f)
                .Screen(Orientation.Landscape, 20f)
                .Sdp(this);                                                       // DimenSsp.Scaled(...) for text
```

Prefer the plain `float` overloads (`*Plain`, `Com.Appdimens.Dynamic.Code.Plain`) when chaining already-resolved
pixel values, to avoid scaling twice.

## Other strategies

Each lives in `Com.Appdimens.Dynamic.Code.<Strategy>` with `Dimen<Strategy>Dp` (layout), `Dimen<Strategy>Sp` (text),
a `Dimen<Strategy>` builder (`.Screen(...)`, `.ApplyAspectRatio()`, `.IgnoreMultiWindows()`) and `*PlainPxKt`.
Methods take `(Context, int)` and return pixels; suffixes `a`/`i`/`ia` and inverters as above.

| Namespace | Layout class → method | Character |
|-----------|-----------------------|-----------|
| `.Percent` | `DimenPercentDp.Psdp` / `Phdp` / `Pwdp` | linear fraction of the axis |
| `.Auto` | `DimenAutoDp.Asdp` | proportional, damped on big screens |
| `.Fluid` | `DimenFluidDp.Fsdp` | clamped smooth growth |
| `.Power` | `DimenPowerDp.Pwsdp` | power curve (sensitivity `k`) |
| `.Diagonal` | `DimenDiagonalDp.Dgsdp` | screen diagonal, rotation-stable |
| `.Fill` / `.Fit` | `DimenFillDp.Flsdp` / `DimenFitDp.Ftsdp` | fill / fit a proportion |
| `.Interpolated` | `DimenInterpolatedDp.Isdp` | linear between anchors |
| `.Logarithmic` | `DimenLogarithmicDp.Logsdp` | gentle log growth |
| `.Perimeter` | `DimenPerimeterDp.Prsdp` | width + height |
| `.Density` | `DimenDensityDp.Dsdp` | follows DPI |

Text twins follow the same prefix (`DimenFluidSp.Fssp`, `DimenAutoSp.Assp`, `DimenPercentSp.Pssp`…); check the exact
generated name in `docs/Bodenberg.AppDimens.Dynamic/API.md` before using a less common one. Start with Scaled, then
Percent/Auto; adopt the others for specialized layouts.

Also: `Com.Appdimens.Dynamic.Code.Units.DimenPhysicalUnits` (`ToDpFromMm(10f, Resources)`, `ToPxFromCm`, `ToSpFromInch`,
`RadiusFromDiameter`), `Com.Appdimens.Dynamic.Code.Resize.DimenResize` (auto-fit text/squares),
`Com.Appdimens.Dynamic.Core.DimenCache` (`Init`, `InvalidateOnConfigChange`, `Clear`/`ClearAll`, stats).

## .NET MAUI (Android head)

```csharp
#if ANDROID
using Com.Appdimens.Dynamic.Code;
var context = Platform.CurrentActivity ?? Android.App.Application.Context;
var density = DeviceDisplay.MainDisplayInfo.Density;   // values are px → divide for MAUI units
card.Padding = new Thickness(DimenSdp.Sdp(context, 16) / density);
titleLabel.FontSize = DimenSsp.Ssp(context, 18) / density;
#endif
```

## Limitations

* Jetpack Compose helpers in the AARs are **not usable from C#** (need `Composer`).
* `DimenCache` needs `Xamarin.AndroidX.DataStore.Preferences` at runtime (pulled in by the package).
* Never hand-edit generated JNI wrappers; binding fixes go in `Transforms/Metadata.xml` / `Additions/*.cs`.

References: [Dynamic guide](../../docs/Bodenberg.AppDimens.Dynamic/README.md) · generated C# API:
`docs/Bodenberg.AppDimens.Dynamic/API.md` · upstream strategy docs: https://github.com/bodenberg/appdimens-dynamic/tree/main/DOCUMENTATION

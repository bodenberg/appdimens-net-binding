---
name: appdimens-android-sdps-ssps
description: Use the .NET for Android bindings Bodenberg.AppDimens.Sdps (layout @dimen/_16sdp, _48hdp, _200wdp + DimenSdp/DimenSsp code APIs) and Bodenberg.AppDimens.Ssps (text @dimen/_14ssp, _32hsp, _24wsp + DimenSsp), which embed the Kotlin AARs appdimens-sdps / appdimens-ssps 3.2.0. Use when a net10.0-android (or MAUI Android head) project references these packages, uses Com.Appdimens.Sdps / Com.Appdimens.Ssps namespaces, SdpsConfig / SspsConfig, or wants Android SDP/SSP resource sizing from C#.
---

# AppDimens SDPS / SSPS — .NET for Android bindings

Bindings of the Kotlin libraries [appdimens-sdps](https://github.com/bodenberg/appdimens-sdps) and
[appdimens-ssps](https://github.com/bodenberg/appdimens-ssps). They ship thousands of pre-generated
`values-sw*/w*/h*` dimen resources (sizes **1–600**) plus imperative C# APIs. NuGet **3.6.1**, AAR **3.2.0**,
`net10.0-android36.0`, min API **24**. **Android only** — for MAUI on every platform use skill `appdimens-sdps-net`
(`Bodenberg.AppDimens.Maui.Sdps`); for code-only strategies use skill `appdimens-android-dynamic`.

| Package | XML resources | Code | Namespaces |
|---------|---------------|------|------------|
| `Bodenberg.AppDimens.Sdps` | **layout**: `@dimen/_16sdp`, `_48hdp`, `_200wdp`, `_minus8sdp` | `DimenSdp` (layout) + `DimenSsp` (text) | `Com.Appdimens.Sdps.Code`, `.Common`, `.Core`; config `Com.Appdimens.Sdps.SdpsConfig` |
| `Bodenberg.AppDimens.Ssps` | **text**: `@dimen/_14ssp`, `_32hsp`, `_24wsp` | `DimenSsp` | `Com.Appdimens.Ssps.Code`, `.Common`, `.Core`; config `Com.Appdimens.Ssps.SspsConfig` |

Text in **XML** needs Ssps (Sdps only computes text in code). Both packages can be installed together.

```bash
dotnet add package Bodenberg.AppDimens.Sdps --version 3.6.1
dotnet add package Bodenberg.AppDimens.Ssps --version 3.6.1
```

## XML layouts (easiest)

```xml
<LinearLayout android:padding="@dimen/_16sdp" android:layout_width="match_parent" android:layout_height="wrap_content">
    <ImageView android:layout_width="@dimen/_48sdp" android:layout_height="@dimen/_48sdp" />
    <TextView android:layout_width="@dimen/_200wdp" android:layout_height="@dimen/_48hdp"
              android:textSize="@dimen/_14ssp" />   <!-- _Nssp requires Bodenberg.AppDimens.Ssps -->
</LinearLayout>
```

Pattern `@dimen/_<n><unit>`: `sdp` smallest width, `hdp` height, `wdp` width; `ssp`/`hsp`/`wsp` for text.

## C# — every `Dimen*` method returns **pixels** (`float`)

```csharp
using Android.Util;               // ComplexUnitType
using Com.Appdimens.Sdps;         // SdpsConfig
using Com.Appdimens.Sdps.Code;    // DimenSdp, DimenSsp

SdpsConfig.Warmup(this);                                  // optional (= DimenSdp.WarmupSdpsFactors(this))
int pad = (int)DimenSdp.Sdp(this, 16);
card.SetPadding(pad, pad, pad, pad);
title.SetTextSize(ComplexUnitType.Px, DimenSsp.Ssp(this, 18));   // NEVER title.TextSize = value (double scaling)
float header = DimenSdp.Hdp(this, 56), banner = DimenSdp.Wdp(this, 200);
```

Ssps-only apps: same calls with `using Com.Appdimens.Ssps;` / `Com.Appdimens.Ssps.Code;`, `SspsConfig`,
`DimenSsp.WarmupSspsFactors(this)`.

| Size… | Method |
|-------|--------|
| padding, margins, icons (default) | `DimenSdp.Sdp` |
| height / width axis | `DimenSdp.Hdp` / `DimenSdp.Wdp` |
| text (user font scale) | `DimenSsp.Ssp` / `Hsp` / `Wsp` |
| text ignoring font scale | `DimenSsp.Sem` / `Hem` / `Wem` |
| suffix `a` / `i` / `ia` | aspect ratio / ignore multi-window / both (`Sdpa`, `Sdpi`, `Sspia`…) |
| inverters | `SdpPh`, `SdpLw`, `HdpLw`, `WdpLh`, `SspPh`, `SspLw`, `HspLw`, `WspLh` (+`a`/`i`/`ia`) |

Facilitators and builders (`Com.Appdimens.Sdps.Common` enums: `DpQualifier`, `Inverter`, `Orientation`, `UiModeType`, `UnitType`):

```csharp
using Com.Appdimens.Sdps.Common;
float rot   = DimenSdp.SdpRotate(this, 16, 24, Orientation.Landscape);
float plain = DimenSdp.SdpRotatePlain(this, 48f, 72f, Orientation.Landscape);   // values already in px
float tv    = DimenSdp.SdpMode(this, 16, 24, UiModeType.Television);
float tab   = DimenSdp.SdpQualifier(this, 16, 20, DpQualifier.SmallWidth, 600);
float scr   = DimenSdp.SdpScreen(this, 16, 28, UiModeType.Television, DpQualifier.SmallWidth, 600);
float b     = DimenSdp.Scaled(16)                                   // priority-ordered rules, first match wins
                 .Screen(UiModeType.Television, DpQualifier.SmallWidth, 600, 40)
                 .Screen(Orientation.Landscape, 20)
                 .Sdp(this);                                         // DimenSsp.Scaled(16)...Ssp(this) / .Sem(this) for text
int resId   = DimenSdp.SdpRes(this, 16);                             // resource id → Resources.GetDimension(resId)
float mm    = DimenPhysicalUnits.ToDpFromMm(10f, Resources);         // Com.Appdimens.Sdps.Code
```

HDP/WDP/HSP/WSP twins exist (`HdpRotate`, `WspMode`, `HspScreen`…). Foldable overloads take
`AndroidX.Window.Layout.IFoldingFeature`.

## Rotation / fold / resize

```csharp
public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
{
    base.OnConfigurationChanged(newConfig);
    SdpsConfig.OnConfigurationChanged(this, newConfig);   // or SspsConfig.OnConfigurationChanged
}
SdpsConfig.DimensionsShouldRefresh += (_, _) => ApplySizes();   // re-read DimenSdp.* values
```

## .NET MAUI (Android head)

Values are pixels; MAUI wants device-independent units — divide by density and guard with `#if ANDROID`:

```csharp
#if ANDROID
using Com.Appdimens.Sdps.Code;
var context = Platform.CurrentActivity ?? Android.App.Application.Context;
var density = DeviceDisplay.MainDisplayInfo.Density;
titleLabel.FontSize = DimenSsp.Ssp(context, 18) / density;
card.Padding = new Thickness(DimenSdp.Sdp(context, 16) / density);
#endif
```

## Limitations

* Jetpack Compose helpers in the AARs (`16.sdp`, `scaledSp()`…) are **not usable from C#** (need `Composer`).
* Never hand-edit generated JNI wrappers; binding fixes go in `Transforms/Metadata.xml` / `Additions/*.cs`.
* Dependencies pulled in: Xamarin.Kotlin.StdLib, AndroidX.Core, AndroidX.Window, AndroidX.Lifecycle.Runtime.

References: [Sdps guide](../../docs/Bodenberg.AppDimens.Sdps/README.md) · [Ssps guide](../../docs/Bodenberg.AppDimens.Ssps/README.md) ·
generated C# API: `docs/Bodenberg.AppDimens.Sdps/API.md`, `docs/Bodenberg.AppDimens.Ssps/API.md`.

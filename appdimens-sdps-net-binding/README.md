# Bodenberg.AppDimens.Sdps

📖 **Documentation:** [Bodenberg.AppDimens.Sdps guide](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/Bodenberg.AppDimens.Sdps/README.md) · [complete API reference](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/Bodenberg.AppDimens.Sdps/API.md) · [all AppDimens .NET packages](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/README.md)

**.NET 10 binding for Android** (`net10.0-android`) that wraps **[AppDimens SDPS](https://github.com/bodenberg/appdimens-sdps)** — responsive **layout** (`SDP` / `HDP` / `WDP`) and **typography** (`SSP` / `HSP` / `WSP`) in one library, backed by thousands of pre-generated **`@dimen`** resources, plus imperative APIs, **conditional facilitators**, **aspect-ratio** variants, **accessibility** text modes, and **foldable-aware** overloads.

Kotlin/Java sources, XML naming, and Android-focused guides live upstream:

**→ [bodenberg/appdimens-sdps](https://github.com/bodenberg/appdimens-sdps)**

Maven artifact embedded in this package: **`io.github.bodenberg:appdimens-sdps`** [`3.2.0`](https://central.sonatype.com/artifact/io.github.bodenberg/appdimens-sdps/3.2.0).

---

## Install

```bash
dotnet add package Bodenberg.AppDimens.Sdps --version 3.6.1
```

Pin a version explicitly when you rely on a specific package revision (see **Package version vs embedded AAR** below).

---

## Getting started (step by step)

New to AppDimens? Follow these steps in order — each one is copy-and-paste.

### Step 1 — Is this the right package?

| Your app | Use |
|----------|-----|
| .NET for Android (`net10.0-android`) or **.NET MAUI targeting Android** | ✅ **Bodenberg.AppDimens.Sdps** (this package) |
| .NET MAUI on **iOS / Windows / macOS** too | [`Bodenberg.AppDimens.Maui.Sdps`](https://www.nuget.org/packages/Bodenberg.AppDimens.Maui.Sdps) or [`AppDimens.Net.Sdk`](https://www.nuget.org/packages/AppDimens.Net.Sdk) (pure C#, all platforms) |
| **WPF**, **Windows Forms** or **WinUI 3** | [`Bodenberg.AppDimens.Sdps.Wpf`](https://www.nuget.org/packages/Bodenberg.AppDimens.Sdps.Wpf) / [`.WinForms`](https://www.nuget.org/packages/Bodenberg.AppDimens.Sdps.WinForms) / [`.WinUI`](https://www.nuget.org/packages/Bodenberg.AppDimens.Sdps.WinUI), or [`AppDimens.Net.Wpf`](https://www.nuget.org/packages/AppDimens.Net.Wpf) / [`.WinForms`](https://www.nuget.org/packages/AppDimens.Net.WinForms) / [`.WinUI`](https://www.nuget.org/packages/AppDimens.Net.WinUI) |

This package wraps a native **Android** library, so it only runs on Android.

### Step 2 — Install

**Visual Studio:** right-click the project → *Manage NuGet Packages…* → search **Bodenberg.AppDimens.Sdps** → *Install*.

**Command line:**

```bash
dotnet add package Bodenberg.AppDimens.Sdps --version 3.6.1
```

In a MAUI app the package only affects the Android target — wrap Android-only code in `#if ANDROID`.

### Step 3 — Use it in Android XML layouts (easiest)

The package ships ready-made `@dimen` resources. Just replace fixed `dp` / `sp` values:

```xml
<!-- BEFORE: fixed sizes, too small on tablets, too big on small phones -->
<ImageView android:layout_width="48dp" android:layout_height="48dp" android:layout_margin="16dp" />

<!-- AFTER: sizes scale with the screen -->
<ImageView
    android:layout_width="@dimen/_48sdp"
    android:layout_height="@dimen/_48sdp"
    android:layout_margin="@dimen/_16sdp" />
```

Name pattern: `@dimen/_<number><unit>` — `_16sdp` (smallest width), `_120hdp` (height), `_200wdp` (width). Numbers go from **1 to 600**.

> 📝 The XML resources in this package are for **layout** (`sdp` / `hdp` / `wdp`). For **text**, use `DimenSsp` from C# (Step 4), or add [`Bodenberg.AppDimens.Ssps`](https://www.nuget.org/packages/Bodenberg.AppDimens.Ssps) to get `@dimen/_14ssp` resources in XML.

### Step 4 — Use it from C#

All `Dimen*` methods return **pixels** (`float`). Android views expect pixels, so you can apply them directly:

```csharp
using Android.Util;          // ComplexUnitType
using Com.Appdimens.Sdps.Code;

// In an Activity (this = Context)
SdpsConfig.Warmup(this);   // optional, makes the first calls faster

int padding = (int)DimenSdp.Sdp(this, 16);
myCard.SetPadding(padding, padding, padding, padding);

// Text: tell Android the value is already in pixels
myTitle.SetTextSize(ComplexUnitType.Px, DimenSsp.Ssp(this, 18));
```

> ⚠️ **Common mistake:** `myTitle.TextSize = value;` interprets the number as **sp** and scales it a second time. Always use `SetTextSize(ComplexUnitType.Px, value)` with AppDimens values.

### Step 5 — Use it in a .NET MAUI app (Android)

MAUI sizes are device-independent units, so divide the pixel value by the screen density:

```csharp
#if ANDROID
using Com.Appdimens.Sdps.Code;
using Microsoft.Maui.Devices;

var context = Platform.CurrentActivity ?? Android.App.Application.Context;
var density = DeviceDisplay.MainDisplayInfo.Density;

titleLabel.FontSize = DimenSsp.Ssp(context, 18) / density;
card.Padding = new Thickness(DimenSdp.Sdp(context, 16) / density);
#endif
```

### Step 6 — Keep sizes right after rotation / fold / resize

```csharp
using Com.Appdimens.Sdps;

public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
{
    base.OnConfigurationChanged(newConfig);
    SdpsConfig.OnConfigurationChanged(this, newConfig);   // recalculates factors
}

// Anywhere (e.g. OnCreate): re-apply sizes when they change
SdpsConfig.DimensionsShouldRefresh += (_, _) => ApplySizes();
```

### Cheat sheet — which function do I use?

| I want to size… | Use | Example |
|-----------------|-----|---------|
| Padding, margins, icons, cards (most cases) | **`Sdp`** — follows the *smallest* screen side | `DimenSdp.Sdp(this, 16)` |
| Something that should follow the **height** (headers, bottom bars) | **`Hdp`** | `DimenSdp.Hdp(this, 56)` |
| Something that should follow the **width** (banners, side panels) | **`Wdp`** | `DimenSdp.Wdp(this, 200)` |
| Text | **`Ssp`** (respects the user's font-size setting) | `DimenSsp.Ssp(this, 14)` |
| Text that must **ignore** the user's font-size setting | **`Sem`** | `DimenSsp.Sem(this, 14)` |
| Same as above but slightly bigger on very wide/tall screens | add **`a`**: `Sdpa`, `Sspa` | `DimenSdp.Sdpa(this, 16)` |

**Rule of thumb:** write the same numbers you would normally use in `dp`/`sp`. AppDimens treats them as sizes on a **300 dp-wide reference phone** and scales them proportionally: `16` becomes ~19 dp on a 360 dp phone and ~32 dp on a 600 dp tablet.

---

## Requirements

| Requirement | Notes |
|-------------|--------|
| **Target framework** | `net10.0-android` (.NET for Android / .NET MAUI Android) |
| **Minimum Android API** | **24** (matches the packaged AAR) |
| **Workload** | Android (`dotnet workload install android`) or MAUI |
| **JDK** | **17 or 21** for the Xamarin.Android toolchain on .NET 10 |

Use **Android SDK platform 36+** (or MSBuild `InstallAndroidDependencies`) when building bindings locally on **.NET 10**.

The AAR bundles **`values-sw*`** / **`values-w*`** / **`values-h*`** dimen XML (sizes **1–600**) for the **dp** layout buckets (`_Nsdp`, `_Nhdp`, `_Nwdp`). Text sizes are computed in code by `DimenSsp`; XML `_Nssp` resources ship in [Bodenberg.AppDimens.Ssps](https://www.nuget.org/packages/Bodenberg.AppDimens.Ssps).

---

## What this library provides

| Capability | Summary |
|------------|---------|
| **SDP / HDP / WDP** | Scale **margins, padding, sizes** by smallest width, height, or width |
| **SSP / HSP / WSP** | Scale **text** on the same three axes |
| **SEM / HEM / WEM** | Typography curves that **ignore system font scale** |
| **XML `@dimen`** | e.g. `@dimen/_16sdp`, `@dimen/_48hdp` — zero-config in layouts (text XML dimens: Ssps package) |
| **Code API** | **`DimenSdp`** / **`DimenSsp`** → pixel values for Views, custom drawing, MAUI |
| **Aspect ratio** | **`Sdpa`**, **`Sspa`**, … — geometry multiplier on top of the XML bucket |
| **Multi-window** | **`Sdpi`**, **`Sspia`**, … — parity with Dynamic “ignore scaling” paths |
| **Inverters** | **`SdpPh`**, **`HspLw`**, **`WdpLh`**, … — orientation-aware axis switch |
| **Facilitators** | **`Rotate`**, **`Mode`**, **`Qualifier`**, **`Screen`** (+ **Plain** px variants for dp) |
| **Builders** | **`DimenScaled`** (layout) and **`ScaledSp`** (text) — priority-based rule lists |
| **Physical units** | **`DimenPhysicalUnits`** — mm / cm / inch helpers |
| **Prefetch** | **`WarmupSdpsFactors`** — optional aspect-ratio factor warmup |

**SDPS** is the **all-in-one** responsive dimen package (layout + type). Use **[AppDimens SSPS](https://github.com/bodenberg/appdimens-ssps)** if you only need typography, or **[AppDimens Dynamic](https://github.com/bodenberg/appdimens-dynamic)** for code-only multi-strategy scaling without XML grids.

---

## C# namespaces (binding layout)

Public Kotlin **`@JvmStatic`** APIs appear under **`Com.Appdimens.Sdps.*`**:

| Area | Namespace / types |
|------|---------------------|
| **Views / imperative (recommended)** | `Com.Appdimens.Sdps.Code` |
| **Layout (dp)** | `DimenSdp`, `DimenScaled`, `CustomDpEntry` |
| **Typography (sp)** | `DimenSsp`, `ScaledSp`, `CustomSpEntry` |
| **Physical units** | `DimenPhysicalUnits` |
| **Extensions** | `DimenExtensionsKt`, `DimenSspExtensionsKt`, `DimenSspScaledKt` |
| **Shared enums** | `Com.Appdimens.Sdps.Common` — `DpQualifier`, `Inverter`, `Orientation`, `UiModeType`, `UnitType` |
| **AR factors** | `Com.Appdimens.Sdps.Core` — `AppDimensSdpsFactors` |
| **Compose (limited)** | `Com.Appdimens.Sdps.Compose.*` — see **Jetpack Compose** below |

Foldable overloads accept **`AndroidX.Window.Layout.IFoldingFeature`** from **Xamarin.AndroidX.Window**.

---

## Quick start (Activity / Views)

```csharp
using Android.App;
using Android.OS;
using Com.Appdimens.Sdps.Code;

[Activity(Label = "My App", MainLauncher = true)]
public class MainActivity : Activity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        DimenSdp.WarmupSdpsFactors(this);

        float paddingPx = DimenSdp.Sdp(this, 16);
        float cardHeight  = DimenSdp.Hdp(this, 120);
        float bannerWidth = DimenSdp.Wdp(this, 200);

        float titlePx = DimenSsp.Ssp(this, 20);
        float bodyPx  = DimenSsp.Ssp(this, 14);
    }
}
```

Apply **`paddingPx`** / **`titlePx`** as pixel sizes on `View` layout params or `TextView.TextSize`.

---

## Layout dimensions (`DimenSdp`)

All methods return **pixels** (`float`) for a nominal **dp** integer (e.g. `16` → `_16sdp` resource bucket).

### Primary axes

| Method family | Scales by |
|---------------|-----------|
| **`Sdp` / `Sdpa` / `Sdpi` / `Sdpia`** | Smallest width (+ AR / multi-window) |
| **`Hdp` / `Hdpa` / …** | Screen height |
| **`Wdp` / `Wdpa` / …** | Screen width |

### Inverter shortcuts (orientation-aware)

| Example | Typical behaviour |
|---------|-------------------|
| **`SdpPh` / `SdpPha`** | Smallest width → **height** in portrait |
| **`SdpLw` / `SdpLwa`** | Smallest width → **width** in landscape |
| **`HdpLw` / `HdpLwa`** | Height → **width** in landscape |
| **`WdpLh` / `WdpLha`** | Width → **height** in landscape |

Each base method has matching **`a`** / **`i`** / **`ia`** variants where applicable.

### Resource IDs (XML or `GetDimension`)

```csharp
int padRes = DimenSdp.SdpRes(this, 16);
float padPx = Resources!.GetDimension(padRes);

// Or generic qualifier path:
int resId = DimenSdp.GetResourceId(this,
    Com.Appdimens.Sdps.Common.DpQualifier.SmallWidth, 16,
    Com.Appdimens.Sdps.Common.Inverter.None);

float px = DimenSdp.GetDimensionInPx(this,
    Com.Appdimens.Sdps.Common.DpQualifier.SmallWidth, 16,
    Com.Appdimens.Sdps.Common.Inverter.None,
    ignoreFontScale: false);
```

**`*Res`** helpers exist for inverter variants (`SdpPhRes`, `HdpLwRes`, …).

### Facilitators (conditional layout)

```csharp
using Com.Appdimens.Sdps.Code;
using Com.Appdimens.Sdps.Common;

float pad = DimenSdp.SdpRotate(this, 16, 24);
float padLandscape = DimenSdp.SdpRotate(this, 16, 24, Orientation.Landscape);

// Already-resolved px values (no second resource pass):
float plain = DimenSdp.SdpRotatePlain(this, 48f, 72f, Orientation.Landscape);

float tvPad = DimenSdp.SdpMode(this, 16, 24, UiModeType.Television);
float tablet = DimenSdp.SdpQualifier(this, 16, 20, DpQualifier.SmallWidth, 600);
float fold = DimenSdp.SdpScreen(this, 16, 28,
    UiModeType.Television, DpQualifier.SmallWidth, 600);
```

**`HdpRotate`**, **`WdpMode`**, **`HdpScreen`**, etc. mirror the **HDP** / **WDP** families.

### `DimenScaled` builder (layout)

Priority-ordered rules — **first match wins** (not the same as nested extension order):

```csharp
var chain = DimenSdp.Scaled(16)
    .Screen(UiModeType.Television, DpQualifier.SmallWidth, 600, 40)
    .Screen(UiModeType.Television, 32)
    .Screen(Orientation.Landscape, 20);

float px = chain.Sdp(this);
float heightPx = chain.Hdp(this);
```

Explore generated **`Screen(...)`** overloads on **`DimenScaled`** for qualifiers, inverters, and foldables.

---

## Typography (`DimenSsp`)

Same conceptual model as layout, but for **text** (sp), computed in code. (XML `_16ssp` / `_32hsp` resources live in the companion **Bodenberg.AppDimens.Ssps** package.)

### Primary axes

| Method | Scales by |
|--------|-----------|
| **`Ssp` / `Sspa` / `Sspi` / `Sspia`** | Smallest width |
| **`Hsp` / `Hspa` / …** | Height |
| **`Wsp` / `Wspa` / …** | Width |

### Accessibility (ignore font scale)

| Method | Use when |
|--------|----------|
| **`Sem` / `Sema` / …** | Smallest-width curve, fixed vs user font scale |
| **`Hem` / `Hema` / …** | Height axis, fixed scale |
| **`Wem` / `Wema` / …** | Width axis, fixed scale |

### Resource IDs

```csharp
int textRes = DimenSsp.SspRes(this, 16);
float textPx = DimenSsp.Ssp(this, 16);

float lowLevel = DimenSsp.GetDimensionInSpPx(this,
    DpQualifier.SmallWidth, 16, Inverter.None, ignoreFontScale: false);
```

### Facilitators & `ScaledSp` builder

```csharp
float rotated = DimenSsp.SspRotate(this, 16, 24, ignoreFontScale: false);
float plain = DimenSsp.SspRotatePlain(this, 42f, 56f, Orientation.Landscape);

var textChain = DimenSsp.Scaled(16)
    .Screen(UiModeType.Television, DpQualifier.SmallWidth, 600, 40)
    .Screen(Orientation.Landscape, 20);

float titlePx = textChain.Ssp(this);
float fixedPx = textChain.Sem(this);   // ignore font scale
```

**`HspRotate`**, **`WspMode`**, **`SspScreen`**, etc. complete the **HSP** / **WSP** surface.

---

## XML layouts (bundled resources)

Use dimens directly in Android XML / MAUI Android layouts:

```xml
<LinearLayout
    android:padding="@dimen/_16sdp"
    android:layout_width="match_parent"
    android:layout_height="wrap_content">

    <TextView
        android:layout_width="@dimen/_200wdp"
        android:layout_height="@dimen/_48hdp"
        android:layout_marginTop="@dimen/_8sdp" />
</LinearLayout>
```

Naming pattern: **`_<n>sdp`**, **`_<n>hdp`**, **`_<n>wdp`**. Values **1–600** are pre-calculated per qualifier bucket inside the AAR. For XML text sizes (`_<n>ssp`, `_<n>hsp`, `_<n>wsp`) add **Bodenberg.AppDimens.Ssps**; both packages can be installed together.

---

## Aspect ratio & warmup

**`Sdpa`**, **`Sspa`**, **`Hdpa`**, … apply an **aspect-ratio multiplier** on top of the XML-resolved size (aligned with **appdimens-dynamic** when the same bucket is selected).

```csharp
DimenSdp.WarmupSdpsFactors(this);

float layout = DimenSdp.Sdp(this, 32);
float layoutAr = DimenSdp.Sdpa(this, 32);

float text = DimenSsp.Ssp(this, 16);
float textAr = DimenSsp.Sspa(this, 16);
```

Factors live in **`Com.Appdimens.Sdps.Core.AppDimensSdpsFactors`** and invalidate when screen geometry or density changes.

---

## Physical units

**`Com.Appdimens.Sdps.Code.DimenPhysicalUnits`** converts real-world units using display metrics:

```csharp
float dpFromMm = DimenPhysicalUnits.ToDpFromMm(10f, Resources);
float pxFromCm = DimenPhysicalUnits.ToPxFromCm(2f, Resources);
float spFromInch = DimenPhysicalUnits.ToSpFromInch(0.25f, Resources);
```

Also: unit conversions (**`MmToCm`**, **`InchToMm`**, …) and geometry helpers (**`RadiusFromDiameter`**, …).

---

## Shared types (`Com.Appdimens.Sdps.Common`)

| Type | Purpose |
|------|---------|
| **`DpQualifier`** | `SmallWidth`, `Width`, `Height`, … |
| **`DpQualifierEntry`** | Qualifier + threshold for builders |
| **`Inverter`** | Axis inversion for low-level resolution |
| **`Orientation`** | Portrait / landscape facilitators |
| **`UiModeType`** | TV, automotive, watch, … |
| **`UnitType`** | Physical unit enums |

---

## Custom entries

**`CustomDpEntry`** and **`CustomSpEntry`** (in **`Com.Appdimens.Sdps.Code`**) support advanced builder/custom bucket scenarios exposed by the upstream library. Inspect generated members or upstream docs when you outgrow static **`Sdp`/`Ssp`** helpers.

---

## Jetpack Compose (`com.appdimens.sdps.compose`)

The AAR includes Compose extensions (`16.sdp`, `18.ssp`, `scaledSp()`, facilitators, …) that require **`androidx.compose.runtime.Composer`**. With the current **class-parse** binding pipeline, **usable C# wrappers for most Compose helpers are not generated**.

**Recommended for .NET:**

- **`Com.Appdimens.Sdps.Code`** — Activities, Fragments, custom views, MAUI Android handlers  
- **XML `@dimen/_Nsdp`** in layout resources (and `@dimen/_Nssp` with Bodenberg.AppDimens.Ssps)  
- **`DimenScaled`** / **`DimenSsp.Scaled`** builders from C#  

---

## Dependencies brought in by this package

| NuGet | Role |
|-------|------|
| **Xamarin.Kotlin.StdLib** | Kotlin runtime |
| **Xamarin.AndroidX.Core** | Core / resources |
| **Xamarin.AndroidX.Window** | Foldables (`FoldingFeature`) |
| **Xamarin.AndroidX.Lifecycle.Runtime** | Lifecycle alignment with upstream AAR |

---

## Package version vs embedded AAR

| Layer | Version |
|-------|---------|
| **Maven / embedded `.aar`** | **`appdimens-sdps` 3.2.0** |
| **NuGet** | **`3.6.1`** — `net10.0-android` and updated Xamarin AndroidX; Android binary **3.2.0** |

When Maven publishes a new version, run `./scripts/sync-aar-from-maven.sh <maven-version>` and bump the NuGet version accordingly.

---

## Build this binding locally

```bash
cd appdimens-sdps-net-binding
dotnet build AppDimens.Sdps.sln -c Release
```

Smoke test: **`AppDimens.Sdps.SmokeTest`** — produce an APK with:

```bash
dotnet publish AppDimens.Sdps.SmokeTest/AppDimens.Sdps.SmokeTest.csproj -c Release -f net10.0-android -p:AndroidPackageFormat=apk
```

Sync the upstream AAR:

```bash
./scripts/sync-aar-from-maven.sh 3.2.0
```

---

## Related packages

| Package | Focus |
|---------|--------|
| **[Bodenberg.AppDimens.Ssps](https://www.nuget.org/packages/Bodenberg.AppDimens.Ssps)** | **Typography only** (SSP/HSP/WSP) — smaller dependency if you do not need sdp/hdp/wdp XML |
| **[Bodenberg.AppDimens.Dynamic](https://www.nuget.org/packages/Bodenberg.AppDimens.Dynamic)** | **Code-only** scaling — 15 strategies, no pre-built `@dimen` grids |
| **[Bodenberg.AppDimens.Maui.Sdps](https://www.nuget.org/packages/Bodenberg.AppDimens.Maui.Sdps)** | **.NET MAUI on every platform** (Android, iOS, Windows, macOS) — same SDP math, native C# |
| **[Bodenberg.AppDimens.Sdps.Wpf](https://www.nuget.org/packages/Bodenberg.AppDimens.Sdps.Wpf)** · **[.WinForms](https://www.nuget.org/packages/Bodenberg.AppDimens.Sdps.WinForms)** · **[.WinUI](https://www.nuget.org/packages/Bodenberg.AppDimens.Sdps.WinUI)** | **Windows desktop** — WPF, Windows Forms, WinUI 3 |
| **[AppDimens.Net.Sdk](https://www.nuget.org/packages/AppDimens.Net.Sdk)** | C# port of AppDimens KMP — 12+ strategies, MAUI / WPF / WinForms / WinUI integrations |

---

## Links

| Resource | URL |
|----------|-----|
| AppDimens (main project — NuGet **Project website**) | https://github.com/bodenberg/appdimens |
| Android library (NuGet **Source repository**; implementation &amp; issues) | https://github.com/bodenberg/appdimens-sdps |
| Maven Central | https://central.sonatype.com/artifact/io.github.bodenberg/appdimens-sdps |
| .NET binding monorepo | https://github.com/bodenberg/appdimens-net-binding |

---

## License

**Apache 2.0**, consistent with the upstream library.

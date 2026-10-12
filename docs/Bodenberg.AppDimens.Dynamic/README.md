# Bodenberg.AppDimens.Dynamic

> 📄 This guide is the package README of **Bodenberg.AppDimens.Dynamic** (mirrored from [`appdimens-dynamic-net-binding/README.md`](https://github.com/bodenberg/appdimens-net-binding/blob/main/appdimens-dynamic-net-binding/README.md) by `scripts/generate-docs.sh`). Complete API reference: [API.md](API.md) · [All packages](../README.md)

**.NET 10 binding for Android** (`net10.0-android`) that wraps **[AppDimens Dynamic](https://github.com/bodenberg/appdimens-dynamic)** — responsive **`dp` / `sp` / `px`** sizing in code (no XML dimen grids), with **15 scaling strategies**, conditional **facilitators**, **aspect-ratio** and **multi-window** variants, **physical units**, **auto-resize** helpers, and optional **DataStore-backed** caching.

Kotlin/Java sources, strategy semantics, and Android-focused guides live upstream:

**→ [bodenberg/appdimens-dynamic](https://github.com/bodenberg/appdimens-dynamic)** · [DOCUMENTATION](https://github.com/bodenberg/appdimens-dynamic/tree/main/DOCUMENTATION) · [KDoc search](https://appdimens3.web.app/)

Maven artifact embedded in this package: **`io.github.bodenberg:appdimens-dynamic`** [`3.2.0`](https://repo1.maven.org/maven2/io/github/bodenberg/appdimens-dynamic/3.2.0/).

---

## Install

```bash
dotnet add package Bodenberg.AppDimens.Dynamic --version 3.6.1
```

---

## Getting started (step by step)

New to AppDimens? Follow these steps in order — each one is copy-and-paste.

### Step 1 — Is this the right package?

| Your app | Use |
|----------|-----|
| .NET for Android (`net10.0-android`) or **.NET MAUI targeting Android** | ✅ **Bodenberg.AppDimens.Dynamic** (this package) |
| .NET MAUI on **iOS / Windows / macOS** too | [`Bodenberg.AppDimens.Maui.Sdps`](https://www.nuget.org/packages/Bodenberg.AppDimens.Maui.Sdps) or [`AppDimens.Net.Sdk`](https://www.nuget.org/packages/AppDimens.Net.Sdk) (pure C#, all platforms) |
| **WPF**, **Windows Forms** or **WinUI 3** | [`Bodenberg.AppDimens.Sdps.Wpf`](https://www.nuget.org/packages/Bodenberg.AppDimens.Sdps.Wpf) / [`.WinForms`](https://www.nuget.org/packages/Bodenberg.AppDimens.Sdps.WinForms) / [`.WinUI`](https://www.nuget.org/packages/Bodenberg.AppDimens.Sdps.WinUI), or [`AppDimens.Net.Wpf`](https://www.nuget.org/packages/AppDimens.Net.Wpf) / [`.WinForms`](https://www.nuget.org/packages/AppDimens.Net.WinForms) / [`.WinUI`](https://www.nuget.org/packages/AppDimens.Net.WinUI) |

This package wraps a native **Android** library, so it only runs on Android.

### Step 2 — Install

**Visual Studio:** right-click the project → *Manage NuGet Packages…* → search **Bodenberg.AppDimens.Dynamic** → *Install*.

**Command line:**

```bash
dotnet add package Bodenberg.AppDimens.Dynamic --version 3.6.1
```

In a MAUI app the package only affects the Android target — wrap Android-only code in `#if ANDROID`.

### Step 3 — Use it from C#

All `Dimen*` methods return **pixels** (`float`). Android views expect pixels, so you can apply them directly:

```csharp
using Android.Util;          // ComplexUnitType
using Com.Appdimens.Dynamic.Code;

// In an Activity (this = Context)
DynamicConfig.Warmup(this);   // optional, warms the cache

int padding = (int)DimenSdp.Sdp(this, 16);
myCard.SetPadding(padding, padding, padding, padding);

// Text: tell Android the value is already in pixels
myTitle.SetTextSize(ComplexUnitType.Px, DimenSsp.Ssp(this, 18));
```

> ⚠️ **Common mistake:** `myTitle.TextSize = value;` interprets the number as **sp** and scales it a second time. Always use `SetTextSize(ComplexUnitType.Px, value)` with AppDimens values.

### Step 4 — Use it in a .NET MAUI app (Android)

MAUI sizes are device-independent units, so divide the pixel value by the screen density:

```csharp
#if ANDROID
using Com.Appdimens.Dynamic.Code;
using Microsoft.Maui.Devices;

var context = Platform.CurrentActivity ?? Android.App.Application.Context;
var density = DeviceDisplay.MainDisplayInfo.Density;

titleLabel.FontSize = DimenSsp.Ssp(context, 18) / density;
card.Padding = new Thickness(DimenSdp.Sdp(context, 16) / density);
#endif
```

### Step 5 — Keep sizes right after rotation / fold / resize

```csharp
using Com.Appdimens.Dynamic;

public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
{
    base.OnConfigurationChanged(newConfig);
    DynamicConfig.OnConfigurationChanged(this, newConfig);   // recalculates factors
}

// Anywhere (e.g. OnCreate): re-apply sizes when they change
DynamicConfig.DimensionsShouldRefresh += (_, _) => ApplySizes();
```

### Cheat sheet — which function do I use?

| I want to size… | Use | Example |
|-----------------|-----|---------|
| Padding, margins, icons, cards (most cases) | **`Sdp`** — follows the *smallest* screen side | `DimenSdp.Sdp(this, 16)` |
| Something that should follow the **height** (headers, bottom bars) | **`Hdp`** | `DimenSdp.Hdp(this, 56)` |
| Something that should follow the **width** (banners, side panels) | **`Wdp`** | `DimenSdp.Wdp(this, 200)` |
| Text | **`Ssp`** (respects the user's font-size setting) | `DimenSsp.Ssp(this, 14)` |
| A size that must **not** shrink in split-screen / multi-window | add **`i`**: `Sdpi`, `Sspi` | `DimenSdp.Sdpi(this, 16)` |
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

---

## What this library provides

| Capability | Summary |
|------------|---------|
| **Scaled (default)** | **SDP / HDP / WDP** and **SSP / HSP / WSP** — design reference **300 dp**; recommended starting point |
| **14 extra strategies** | **percent**, **auto**, **power**, **fluid**, **diagonal**, **fill**, **fit**, **interpolated**, **logarithmic**, **perimeter**, **density** — same suffix patterns, different math |
| **Suffixes** | **`a`** (aspect ratio), **`i`** / **`ia`** (multi-window ignore paths), combined with axis helpers |
| **Inverters** | **`Ph`**, **`Lh`**, **`Pw`**, **`Lw`** — switch width/height/smallest-width basis by orientation (e.g. `SdpPh`, `HspLw`) |
| **Facilitators** | **`Rotate`**, **`Mode`**, **`Qualifier`**, **`Screen`** — conditional overrides (orientation, UI mode, `sw`/`w`/`h` buckets, foldables) |
| **Builders** | **`DimenScaled`** / per-strategy **`*Scaled`** — priority-ordered rule lists (unlike nested facilitator order) |
| **Physical units** | **mm / cm / inch** → dp/sp/px via **`DimenPhysicalUnits`** |
| **Resize** | **`DimenResize`** — auto-resize text/layout helpers (see upstream `DOCUMENTATION/resize.md`) |
| **Cache** | **`DimenCache`** — lock-free shards, optional persistence (DataStore); call **`InvalidateOnConfigChange`** on configuration changes |

---

## C# namespaces (binding layout)

Public Kotlin **`@JvmStatic`** APIs appear under **`Com.Appdimens.Dynamic.*`** (Java package → C# namespace):

| Area | Namespace (examples) |
|------|----------------------|
| **Views / imperative (recommended)** | `Com.Appdimens.Dynamic.Code` |
| **Scaled helpers** | `Com.Appdimens.Dynamic.Code` — `DimenSdp`, `DimenSsp`, `DimenScaled` |
| **Other strategies** | `Com.Appdimens.Dynamic.Code.Percent`, `.Auto`, `.Power`, `.Fluid`, `.Diagonal`, `.Fill`, `.Fit`, `.Interpolated`, `.Logarithmic`, `.Perimeter`, `.Density` |
| **Plain px branching** | `Com.Appdimens.Dynamic.Code.Plain`, `*.Dimen*PlainPxKt` per strategy |
| **Physical units** | `Com.Appdimens.Dynamic.Code.Units` — `DimenPhysicalUnits` |
| **Resize** | `Com.Appdimens.Dynamic.Code.Resize` |
| **Shared enums** | `Com.Appdimens.Dynamic.Common` — `DpQualifier`, `Inverter`, `Orientation`, `UiModeType`, `UnitType` |
| **Cache / provider** | `Com.Appdimens.Dynamic.Core` — `DimenCache`, `AppDimensProvider`, … |
| **Compose (limited)** | `Com.Appdimens.Dynamic.Compose.*` — see **Jetpack Compose** below |

Foldable-aware overloads use **`AndroidX.Window.Layout.IFoldingFeature`** from **Xamarin.AndroidX.Window**.

---

## Quick start (Activity / Views)

Warm the cache once (recommended), then read pixel sizes:

```csharp
using Android.App;
using Android.OS;
using Com.Appdimens.Dynamic.Code;

[Activity(Label = "My App", MainLauncher = true)]
public class MainActivity : Activity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        DimenSdp.WarmupCache(this);

        float paddingPx = DimenSdp.Sdp(this, 16);
        float heightPx  = DimenSdp.Hdp(this, 32);
        float widthPx   = DimenSdp.Wdp(this, 100);
        float fontPx    = DimenSsp.Ssp(this, 16);
    }

    public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
    {
        base.OnConfigurationChanged(newConfig);
        Com.Appdimens.Dynamic.Core.DimenCache.InvalidateOnConfigChange(newConfig);
    }
}
```

---

## Scaled API (`DimenSdp` / `DimenSsp`)

Primary entry points in **`Com.Appdimens.Dynamic.Code`**:

| Family | Methods (pattern) | Axis |
|--------|-------------------|------|
| **SDP** | `Sdp`, `Sdpa`, `Sdpi`, `Sdpia`, … | Smallest width |
| **HDP** | `Hdp`, `Hdpa`, `Hdpi`, … | Height |
| **WDP** | `Wdp`, `Wdpa`, `Wdpi`, … | Width |
| **SSP** | `Ssp`, `Sspa`, `Sspi`, … | Text (smallest width) |
| **HSP / WSP** | `Hsp`, `Wsp`, + `a` / `i` / `ia` variants | Text by height / width |

**Inverter shortcuts** (orientation-aware axis switch), examples:

- `SdpPh` / `SdpPha` — smallest width in portrait → height in landscape  
- `SdpLw` / `SdpLwa` — smallest width → width in landscape  
- `HdpLw`, `WspLh`, … — same idea for **HDP** / **WSP**

**Facilitators** (imperative), examples:

```csharp
// Landscape uses 24 sdp, otherwise 16
float pad = DimenSdp.SdpRotate(this, 16, 24);

// TV UI mode override
float tvTitle = DimenSsp.SspMode(this, 16, 22, Com.Appdimens.Dynamic.Common.UiModeType.Television);

// sw ≥ 600 bucket
float tablet = DimenSdp.SdpQualifier(this, 16, 20,
    Com.Appdimens.Dynamic.Common.DpQualifier.SmallWidth, 600);

// Combined UI mode + qualifier (+ optional FoldingFeature overloads)
float fold = DimenSdp.SdpScreen(this, 16, 28,
    Com.Appdimens.Dynamic.Common.UiModeType.Television,
    Com.Appdimens.Dynamic.Common.DpQualifier.SmallWidth, 600);
```

Kotlin extension-style facilitators (`sdpRotatePlain`, nested chains) are exposed as **`DimenSdpExtensionsKt`** / **`DimenSspExtensionsKt`** when generated; prefer **plain `Dp`/`float` overloads** in C# when chaining to avoid double scaling (see upstream **COMPOSE-API-CONVENTIONS**).

---

## `DimenScaled` builder

Build a rule list; resolution uses **priority inside the builder**, not lexical nesting:

```csharp
using Com.Appdimens.Dynamic.Code;
using Com.Appdimens.Dynamic.Common;

var scaled = DimenSdp.Scaled(16)
    .Screen(UiModeType.Television, DpQualifier.SmallWidth, 600, 40f)
    .Screen(UiModeType.Television, 32f)
    .Screen(Orientation.Landscape, 20f);

float px = scaled.Sdp(this);
```

Use **`DimenSsp.Scaled`** / **`ScaledSp`** for typography with the same `.Screen(...)` pattern.

---

## Other scaling strategies

Each strategy lives under **`Com.Appdimens.Dynamic.Code.<Strategy>`** with **`Dimen<Strategy>`**, **`Dimen<Strategy>Dp`**, **`Dimen<Strategy>Sp`**, extension/facilitator types, and optional **`*Scaled`** builders.

| Strategy | Typical use |
|----------|-------------|
| **percent** | Size as **fraction of width/height/smallest width** |
| **auto** | **Breakpoint-style** steps (similar to responsive CSS) |
| **power** | **Power-curve** scaling |
| **fluid** | **Fluid** interpolation between bounds |
| **diagonal** | **Screen diagonal** based |
| **fill** / **fit** | **Fill** or **fit** a proportion of the canvas |
| **interpolated** | **Interpolated** curves between anchors |
| **logarithmic** | **Log**-shaped response |
| **perimeter** | **Perimeter**-based scaling |
| **density** | **Density-aware** adjustments |

**Recommendation:** start with **Scaled** → then **percent** / **auto** when needed; adopt others for specialized layouts ([strategy guide](https://github.com/bodenberg/appdimens-dynamic/blob/main/DOCUMENTATION/README.md)).

Method prefixes mirror Scaled (`psdp`, `asdp`, `fsdp`, …) — import the matching **`Dimen*`** type for that strategy.

---

## Physical units

**`Com.Appdimens.Dynamic.Code.Units.DimenPhysicalUnits`** converts real-world units using display metrics:

```csharp
using Com.Appdimens.Dynamic.Code.Units;

float dpFromMm = DimenPhysicalUnits.ToDpFromMm(10f, Resources);
float pxFromCm = DimenPhysicalUnits.ToPxFromCm(2f, Resources);
float spFromInch = DimenPhysicalUnits.ToSpFromInch(0.25f, Resources);
```

Also: **`MmToCm`**, **`RadiusFromDiameter`**, **`RadiusFromCircumference`**, etc.

---

## Resize helpers

**`Com.Appdimens.Dynamic.Code.Resize.DimenResize`** / **`DimenResizeKt`** expose auto-resize utilities (text size, square size, bounds). Compose-oriented variants may reference **`Composer`**; for .NET, check generated members or follow upstream **`DOCUMENTATION/resize.md`**.

---

## Cache and configuration

**`Com.Appdimens.Dynamic.Core.DimenCache`**:

| API | When to use |
|-----|-------------|
| **`WarmupCache` / `Init`** | App start (also invoked via **`DimenSdp.WarmupCache`**) |
| **`InvalidateOnConfigChange(Configuration)`** | `onConfigurationChanged` / after locale, density, or size class changes |
| **`Clear` / `ClearAll`** | Debug or user “reset layout” flows |
| **`Stats` / diagnostics** | Performance tuning |

Persistence uses **AndroidX DataStore**; this NuGet pulls **`Xamarin.AndroidX.DataStore.Preferences`** (required at runtime for `DimenCache`).

---

## Jetpack Compose (`com.appdimens.dynamic.compose`)

The AAR ships Compose extensions (`DimenSdpKt`, `DimenAutoKt`, `autoResizeTextSp`, …) that take **`androidx.compose.runtime.Composer`**. With the current **class-parse** binding pipeline, **many Compose helpers are not practical from C#** (missing or incomplete `Composer` wrappers).

**Use this package for:**

- **`Com.Appdimens.Dynamic.Code`** — Activities, Fragments, custom `View` measurement, MAUI Android handlers  
- **`Com.Appdimens.Dynamic.Core`** / **`Common`** — cache, qualifiers, shared types  

Compose-first apps on .NET may need a future binding strategy or shared logic called from Kotlin.

---

## Dependencies brought in by this package

| NuGet | Role |
|-------|------|
| **Xamarin.Kotlin.StdLib** | Kotlin runtime for bound types |
| **Xamarin.AndroidX.Core** | Core / `Context` extensions |
| **Xamarin.AndroidX.Window** | Foldables (`FoldingFeature`) |
| **Xamarin.AndroidX.Lifecycle.Runtime** | Lifecycle alignment with upstream AAR |
| **Xamarin.AndroidX.DataStore.Preferences** | **`DimenCache`** persistence (**1.2.1**, aligned with Maven `datastore-preferences`) |

---

## Package version vs embedded AAR

| Layer | Version |
|-------|---------|
| **Maven / embedded `.aar`** | **`appdimens-dynamic` 3.2.0** (core + 13 módulos satélite) |
| **NuGet** | **`3.6.1`** — `net10.0-android` and updated Xamarin AndroidX/DataStore; Android binary **3.2.0** |

When Maven publishes a new version, run `./scripts/sync-aar-from-maven.sh <maven-version>` and bump the NuGet version accordingly.

---

## Build this binding locally

```bash
cd appdimens-dynamic-net-binding
dotnet build AppDimens.Dynamic.sln -c Release
```

Optional smoke APK: **`AppDimens.Dynamic.SmokeTest`** — produce it with:

```bash
dotnet publish AppDimens.Dynamic.SmokeTest/AppDimens.Dynamic.SmokeTest.csproj -c Release -f net10.0-android -p:AndroidPackageFormat=apk
```

Sync the upstream AAR:

```bash
./scripts/sync-aar-from-maven.sh 3.2.0
```

---

## Related packages

| Package | Focus |
|---------|--------|
| **[Bodenberg.AppDimens.Maui.Sdps](https://www.nuget.org/packages/Bodenberg.AppDimens.Maui.Sdps)** | **.NET MAUI on every platform** (Android, iOS, Windows, macOS) — same SDP math, native C# |
| **[Bodenberg.AppDimens.Sdps.Wpf](https://www.nuget.org/packages/Bodenberg.AppDimens.Sdps.Wpf)** · **[.WinForms](https://www.nuget.org/packages/Bodenberg.AppDimens.Sdps.WinForms)** · **[.WinUI](https://www.nuget.org/packages/Bodenberg.AppDimens.Sdps.WinUI)** | **Windows desktop** — WPF, Windows Forms, WinUI 3 |
| **[AppDimens.Net.Sdk](https://www.nuget.org/packages/AppDimens.Net.Sdk)** | C# port of AppDimens KMP — 12+ strategies, MAUI / WPF / WinForms / WinUI integrations |

---

## Links

| Resource | URL |
|----------|-----|
| AppDimens (main project — NuGet **Project website**) | https://github.com/bodenberg/appdimens |
| Android library (NuGet **Source repository**; implementation &amp; issues) | https://github.com/bodenberg/appdimens-dynamic |
| Strategy documentation | https://github.com/bodenberg/appdimens-dynamic/tree/main/DOCUMENTATION |
| Maven Central | https://repo1.maven.org/maven2/io/github/bodenberg/appdimens-dynamic/3.2.0/ |
| .NET binding monorepo | https://github.com/bodenberg/appdimens-net-binding |

---

## License

**Apache 2.0**, consistent with the upstream library.

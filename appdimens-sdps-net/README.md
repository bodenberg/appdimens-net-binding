# Bodenberg.AppDimens.Maui.Sdps

**Fully native .NET MAUI** responsive sizing library with architectural, functional, and ergonomic parity with [AppDimens SDP Android](https://github.com/bodenberg/appdimens-sdps) v3.1.5.

It brings the Android experience of `@dimen/_16sdp`, orientation inverters, discrete buckets, aspect ratio, and conditional builders to MAUI — **with no JNI bindings, AARs, or embedded Android code**.

---

## Table of contents

- [What the library does](#what-the-library-does)
- [Requirements](#requirements)
- [Installation](#installation)
- [Quick start](#quick-start)
- [Screen rotation and resize](#screen-rotation-and-resize)
- [How it works](#how-it-works)
- [Pre-generated resources (`Generated/`)](#pre-generated-resources-generated)
- [Configuration](#configuration)
- [XAML namespaces](#xaml-namespaces)
- [XAML usage — complete reference](#xaml-usage-complete-reference)
- [C# usage — complete reference](#c-usage-complete-reference)
- [Orientation inverters](#orientation-inverters)
- [Responsive builders](#responsive-builders)
- [Conditional helpers](#conditional-helpers)
- [Converters and physical units](#converters-and-physical-units)
- [Source Generator](#source-generator)
- [Static `AppDimensSdps` API](#static-appdimenssdps-api)
- [Screen metrics](#screen-metrics)
- [Compatible MAUI properties](#compatible-maui-properties)
- [Migration Android → MAUI](#migration-android-maui)
- [Performance and caching](#performance-and-caching)
- [Known limitations](#known-limitations)
- [Frequently asked questions](#frequently-asked-questions)
- [Package contents](#package-contents)
- [License](#license)

---

## What the library does

| Capability | Description |
|------------|-------------|
| **Scalable dimensions** | `sdp`, `hdp`, `wdp`, `ssp`, and variants with aspect ratio (`*a`) or without font scale (`sem`/`hem`/`wem`) |
| **Android buckets** | ~358 screen sizes (30…5120 dp) precomputed on three axes (`sw`, `w`, `h`) |
| **Scaling modes** | Continuous, bucket, hybrid (default) |
| **Inverters** | 8 inverters + `Default` to swap axis by orientation |
| **Builders** | `Responsive.Value()` / `ScaledDp()` with tablet, landscape, desktop rules and advanced `Screen()` |
| **Helpers** | `SdpRotate`, `SdpQualifier`, `SdpMode`, `SdpScreen` |
| **Dual XAML** | **Short** syntax `{sdp:16}` and **normal** `{dimen:Sdp Value=16}` |
| **Fluent C#** | `16.Sdp()`, `PaddingSdp()`, `FontSsp()` |
| **Binding** | `IValueConverter` for `{Binding}` |
| **Source generator** | Typed compile-time `Dimen._16sdp` |
| **Reactivity** | Resolver refreshes metrics on rotation/resize; XAML markup is load-time (see [Screen rotation and resize](#screen-rotation-and-resize)) |

---

## Requirements

| Requirement | Version |
|-------------|---------|
| .NET | **8**, **9**, or **10** |
| .NET MAUI | 8.0+ / 9.0+ / 10.0+ (per SDK) |
| Platforms | Android, iOS, macOS, Windows (via MAUI) |
| NuGet version | **1.0.0** |

NuGet package: **`Bodenberg.AppDimens.Maui.Sdps`**

Publishing to NuGet.org: see [`NUGET-PUBLISH.md`](NUGET-PUBLISH.md) and the `scripts/publish-nuget.sh` script.

---

## Installation

```bash
dotnet add package Bodenberg.AppDimens.Maui.Sdps --version 1.0.0
```

Project reference (local development):

```xml
<ProjectReference Include="path/to/AppDimens.Maui.Sdk/AppDimens.Maui.Sdk.csproj" />
```

---

## Quick start

### 1. Register in `MauiProgram.cs`

Required — initializes metrics, buckets, and `MainDisplayInfoChanged` listener:

```csharp
using AppDimens.Maui;
using AppDimens.Maui.Core;

public static MauiApp CreateMauiApp()
{
    var builder = MauiApp.CreateBuilder();
    builder.UseAppDimensSdps(options =>
    {
        options.ScalingMode = ScalingMode.Hybrid;   // recommended
        options.WarmupAspectRatio = true;
        options.DefaultFontScale = 1.0f;
    });
    // ... rest of app
    return builder.Build();
}
```

Alternative without `MauiAppBuilder`:

```csharp
AppDimensSdps.Initialize(new AppDimensOptions { ScalingMode = ScalingMode.Hybrid });
```

### 2. Declare XAML namespaces

See [XAML namespaces](#xaml-namespaces) for the full list. Minimum:

```xml
xmlns:sdp="clr-namespace:AppDimens.Maui.Markup;assembly=AppDimens.Maui.Markup"
xmlns:ssp="clr-namespace:AppDimens.Maui.Markup;assembly=AppDimens.Maui.Markup"
xmlns:dimen="clr-namespace:AppDimens.Maui.Xaml;assembly=AppDimens.Maui.Xaml"
```

### 3. Use dimensions

```xml
<VerticalStackLayout Padding="{sdp:16}" Spacing="{sdp:8}">
    <Label Text="Title" FontSize="{ssp:20}" />
    <Label Text="Responsive" FontSize="{scaled:14, Tablet=18, Landscape=16}" />
</VerticalStackLayout>
```

```csharp
using AppDimens.Maui.Extensions;

double padding = 16.Sdp();
double fontSize = 18.Ssp();
myLabel.FontSsp(20);
myLayout.PaddingSdp(16);
```

### 4. Screen rotation

Initialize once in step 1 — **no extra AppDimens config**. On Android, keep `ConfigurationChanges` on `MainActivity` (MAUI default). XAML markup is evaluated at page load; see [Screen rotation and resize](#screen-rotation-and-resize) for what updates automatically vs what you may need to refresh in code.

---

## Screen rotation and resize

AppDimens **does not use** Android `-land` / `-port` resource qualifiers. Rotation is handled at **runtime** by recalculating screen metrics, re-selecting buckets per axis, and (optionally) axis **inverters** and conditional helpers.

### What you must do (once)

| Step | Required? | Where |
|------|-----------|--------|
| Call `UseAppDimensSdps()` / `AppDimensSdps.Initialize()` | **Yes** | `MauiProgram.cs` — registers `DeviceDisplay.MainDisplayInfoChanged` and loads buckets |
| Android `ConfigurationChanges` on `MainActivity` | **Recommended** | `Platforms/Android/MainActivity.cs` — keeps the process alive on rotation (MAUI default) |
| Android `OnConfigurationChanged` → `RefreshMetricsFromDevice()` | **Recommended** | Same file — ensures metrics/cache refresh when the activity is not recreated (see sample) |
| Declare anything in `AndroidManifest.xml` for rotation | **No** | Rotation is allowed by default; lock orientation only if your app requires it |
| Extra AppDimens config file / handler class | **No** | Metrics refresh is built into the resolver |

**Android `MainActivity`** — keep (or add) configuration change handling so rotation does not tear down the activity unexpectedly:

```csharp
[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.ScreenSize
        | ConfigChanges.Orientation
        | ConfigChanges.UiMode
        | ConfigChanges.ScreenLayout
        | ConfigChanges.SmallestScreenSize
        | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity { }
```

This matches the MAUI template and the sample app. **iOS, Mac Catalyst, and Windows** do not need an equivalent declaration.

**Android — refresh metrics when configuration changes** (included in the sample app):

```csharp
public override void OnConfigurationChanged(Configuration newConfig)
{
    base.OnConfigurationChanged(newConfig);
    AppDimensResolver.Instance.RefreshMetricsFromDevice();
}
```

With `ConfigurationChanges`, the activity may stay alive without recreating the page. This call guarantees `widthDp` / `heightDp` / cache invalidation even if `MainDisplayInfoChanged` is delayed or skipped on some devices.

### What the library updates automatically

After initialization, on rotation, resize, DPI change, or multi-window:

1. `AppDimensResolver` reads new values from `DeviceDisplay.MainDisplayInfo`
2. Updates `widthDp`, `heightDp`, `smallestDp`, and `Orientation` in `Metrics.Current`
3. Invalidates the dimension cache and responsive/bucket state (always when metrics change, all platforms)
4. **New** calls to `16.Sdp()`, converters, and resolver APIs use the updated metrics

You do **not** need to call `RefreshMetricsFromDevice()` manually on iOS, Mac Catalyst, or Windows for normal rotation. On **Android** with `ConfigurationChanges`, calling it from `OnConfigurationChanged` (as in the sample) is recommended. Use manual refresh only for diagnostics, tests, or a “refresh metrics” button.

### XAML markup vs rotation (important)

| Approach | Updates on rotation? |
|----------|----------------------|
| `{sdp:16}`, `{dimen:Sdp Value=16}`, `{sdpPh:16}`, `{scaled:…}`, `{sdpRotate:…}` | **Only when the page/layout is built again** — markup runs at load time |
| `16.Sdp()` in C# (e.g. `OnAppearing`, `SizeChanged`, button handlers) | **Yes** — always uses current metrics |
| `{Binding … Converter={StaticResource SdpConv}}` | **Yes** — if the binding re-evaluates when you refresh the source or layout |

With `ConfigurationChanges` (recommended on Android), the activity **stays alive** and XAML-set properties (`Padding`, `WidthRequest`, `FontSize`, …) **keep their previous pixel values** until you update them.

**Practical guidance:**

- **Most screens:** no extra work — dimensions are resolved once; small drift on rotate is often acceptable, or the user navigates away and back (`OnAppearing` rebuilds labels you set in code).
- **Layouts that must track rotation:** update sizes in code when the page resizes, or re-apply values in `OnAppearing` / `SizeChanged`:

```csharp
protected override void OnSizeAllocated(double width, double height)
{
    base.OnSizeAllocated(width, height);
    MyLayout.Padding = new Thickness(16.Sdp());
    TitleLabel.FontSize = 20.Ssp();
}
```

- **Sample app pattern:** subscribe to `AppDimensResolver.Instance.Metrics.Changed` while the page is visible and re-run your C# refresh (see `DimensSampleRefresh.WhenMetricsChange` in `samples/AppDimens.Maui.Sample/Services/DimensSampleRefresh.cs`). All demo tabs (Home, Dimensions, Inverters, Advanced, Benchmark) use this helper.

- **Orientation-specific values:** use `{scaled:14, Landscape=16}`, `{sdpRotate:30, Rotation=45, Orientation=Landscape}`, or inverters (`{sdpPh:16}`, `{wdpLh:120}`) — the chosen branch depends on **orientation at resolve time** (typically page load; re-apply in code after rotation if the UI must update live).

### Inverters on rotation

Inverters (`sdpPh`, `sdpLw`, `hdpPw`, …) choose width/height/smallest axis from **current orientation at resolve time**. After rotation, the **next** `16.SdpPh()` (or a refreshed XAML property) uses the new orientation. No separate inverter registration is required.

---

## How it works

### Dimension types

| Type | Axis / base | Typical use |
|------|-------------|-------------|
| **sdp** | `smallestWidth` (shorter screen side) | padding, margin, general sizing |
| **hdp** | screen height | heights tied to the vertical axis |
| **wdp** | screen width | widths tied to the horizontal axis |
| **ssp** | same index as sdp + system **font scale** | `FontSize` with accessibility |
| **sem / hem / wem** | like ssp/hsp/wsp, **without** font scale | fixed typography |
| **sdpa / hdpa / wdpa** | dimension + **aspect ratio** adjustment | non-16:9 screens |

### Formulas

**Continuous mode (runtime):**

```
value = (index / 300) × axisMetric
```

- **sdp** → metric = `min(widthDp, heightDp)`
- **hdp** → metric = `heightDp`
- **wdp** → metric = `widthDp`

**Bucket mode (discrete, Android parity):**

```
value = index × (bucketDp / 300)
```

Precomputed buckets for ~358 sizes (30…5120, step 15 + real devices: 384, 411, 640, etc.) on three independent axes: **sw**, **w**, **h**.

**Aspect ratio (`*a`):**

After resolving the base dimension, applies a multiplier derived from the current aspect ratio vs. the 1.78 reference (formula identical to Android `AppDimensSdpsFactors`).

### Index ranges

| Type | Range | Equivalent Android key |
|------|-------|------------------------|
| sdp / hdp / wdp / *a | **-300 … 600** | `_16sdp`, `_minus8sdp`, `_100wdp` |
| ssp / sem / hem / wem | **1 … 600** | `_18ssp` (no negatives) |

Negative indexes are useful for inverted offsets and margins (e.g. `-8` → `_minus8sdp`).

### Qualifier system (buckets)

Equivalent to the Android merge `values-sw{N}dp`, `values-w{N}dp`, `values-h{N}dp`:

1. For each axis, selects the **largest bucket N where N ≤ current metric**
2. Loads precomputed values from that bucket
3. Falls back to base (`scale = 1.0`) when needed

There are **no** `-land` or `-port` qualifiers (Android SDP parity). Rotation is handled by:

- Recalculating `widthDp`, `heightDp`, `smallestDp` on `DeviceDisplay.MainDisplayInfoChanged`
- Re-selecting the bucket per axis (sw / w / h)
- **Inverters** (`sdpPh`, `sdpLw`, …) that swap axis by orientation
- Conditional APIs (`SdpRotate`, `Responsive.Value().Landscape()`, builders)

### Scaling modes (`ScalingMode`)

| Mode | Behavior |
|------|----------|
| `Hybrid` **(default)** | Uses bucket if the key exists; otherwise continuous formula |
| `Continuous` | Always the runtime math formula |
| `Bucket` | Always generated table/bucket lookup |
| `HybridPreferContinuous` | Always continuous (bucket ignored) |

If the `Generated` bucket folder is not found at runtime, `Hybrid` and `Bucket` **automatically fall back** to the continuous formula.

### Reacting to screen changes

The **resolver** automatically invalidates cache and refreshes metrics on:

- Rotation
- Resize (desktop, split-screen, foldables)
- DPI / density change
- Multi-window

Event: `DeviceDisplay.MainDisplayInfoChanged` (subscribed in `AppDimensSdps.Initialize`).

**UI note:** XAML markup values are applied at page load (see [Screen rotation and resize](#screen-rotation-and-resize)). C# calls and bindings re-read current metrics whenever you invoke them.

---

## Pre-generated resources (`Generated/`)

This package ships thousands of pre-calculated dimension entries under `AppDimens.Maui.Resources/Generated/`. That is intentional: **MAUI SDP mirrors how [AppDimens SDP on Android](https://github.com/bodenberg/appdimens-sdps) works**, where sizes come from **discrete resource buckets** (`values-sw{N}dp`, `values-w{N}dp`, `values-h{N}dp`), not from one formula applied to every key at build time.

The tables are produced by [`scripts/generate-dimens.py`](scripts/generate-dimens.py) (same role as the Android `generator-sdps.py` pipeline) and packaged with the NuGet as `contentFiles/.../Generated/`.

### What is in `Generated/`

| Artifact | Role |
|----------|------|
| `buckets.json` | Bucket list (`layoutVersion: 2`) — ≈358 sizes from 30…5120 dp, step 15 + common devices |
| `Dimens.Base.xaml` | Base scale (`1.0`) — axis-neutral keys `_1` … `_600`, `_minus1` … |
| `Dimens.{N}.xaml` | One dictionary per bucket **N** with keys `_index` / `_minusindex` (values scaled by `index × (N / 300)`); axis (sw/w/h) is chosen at runtime, not duplicated in XML |
| Runtime load | `BucketRegistry.LoadFromGenerated()` parses these files when `AppDimensSdps.Initialize(..., generatedPath)` finds the folder |

At runtime the resolver picks the **largest bucket N where N ≤ current metric** on each axis (`sw`, `w`, `h`) — the same qualifier merge idea as Android — and reads the precomputed value for the axis-neutral key `_16` (not separate `_16sdp` / `_16wdp` / `_16hdp` entries).

### What the pre-generated data is for

1. **`Bucket` and `Hybrid` modes (default)** — return the **same discrete values as the Android `@dimen` tables** for the active bucket, instead of only the continuous approximation `index / 300 × metric`.
2. **Faithful porting** — layouts migrated from Android SDP can match pixel results when bucket mode is active.
3. **Performance on the hot path** — after the first resolve, `DimensionCache` stores the result; bucket mode resolves via parsed dictionaries (see [Performance and caching](#performance-and-caching)).
4. **Aspect ratio (`sdpa`, `hdpa`, `wdpa`)** — `AspectRatioFactors` uses `_1` from the **active bucket** per axis when buckets are loaded (`GetOneUnit`), aligned with Android’s bucket-based AR behavior.

### You can ignore the files if you want

- Set `ScalingMode.Continuous` or `ScalingMode.HybridPreferContinuous` — only the runtime formula is used; no bucket lookup.
- If `Generated/` is missing at runtime, `Hybrid` and `Bucket` **fall back automatically** to the continuous formula (see [Scaling modes](#scaling-modes-scalingmode)).

The library still includes the data so **hybrid/bucket parity with Android SDP is available by default** without regenerating tables in your app.

### Prefer runtime-only scaling (no pre-generated grids)?

If you do **not** want bundled bucket/XML-style dimension tables and prefer **code-driven strategies** computed at runtime only, use **[AppDimens Dynamic](https://github.com/bodenberg/appdimens-dynamic)**:

| | **This package (MAUI SDP)** | **AppDimens Dynamic** |
|---|---------------------------|------------------------|
| **Monorepo folder** | [`appdimens-sdps-net/`](.) | [`appdimens-dynamic-net-binding/`](../appdimens-dynamic-net-binding/) |
| **NuGet** | `Bodenberg.AppDimens.Maui.Sdps` | `Bodenberg.AppDimens.Dynamic` |
| **Model** | SDP/HDP/WDP + SSP and Android-style buckets | 15 scaling strategies (percent, auto, fluid, …), facilitators, builders — **no pre-built `@dimen` grids** |
| **Platforms** | Native **.NET MAUI** (Android, iOS, Windows, macOS) | **.NET for Android** binding (usable from MAUI’s Android head; not a cross-platform MAUI-native port) |

See the [Dynamic binding README](../appdimens-dynamic-net-binding/README.md) and the [monorepo overview](../README.md#packages).

---

## Configuration

### `AppDimensOptions`

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ScalingMode` | `ScalingMode` | `Hybrid` | Calculation mode (see table above) |
| `WarmupAspectRatio` | `bool` | `true` | Precomputes AR factors on startup (`Warmup()`) |
| `DefaultFontScale` | `float` | `1.0` | Initial font scale for `ssp`/`sem` |

### Manual initialization (optional)

```csharp
using AppDimens.Maui;
using AppDimens.Maui.Core;

AppDimensSdps.Initialize(new AppDimensOptions
{
    ScalingMode = ScalingMode.Bucket,
    WarmupAspectRatio = true,
});

AppDimensSdps.Warmup(); // optional, forces immediate warmup
```

### Custom bucket path (advanced)

```csharp
AppDimensSdps.Initialize(
    options: new AppDimensOptions { ScalingMode = ScalingMode.Bucket },
    generatedPath: "/path/to/AppDimens.Maui.Resources/Generated");
```

Useful in tests or when generated files live outside the default location.

### System font scale

`ssp` applies the configured font scale. To reflect the user’s accessibility preference:

```csharp
AppDimensResolver.Instance.SetFontScale(/* system value */);
```

---

## XAML namespaces

Each short markup extension uses a **prefix** equal to the extension name (no `Extension` suffix). They all target the same assembly `AppDimens.Maui.Markup`.

Recommended: create `Resources/Styles/AppDimensImports.xaml` and merge it in `App.xaml`:

```xml
<?xml version="1.0" encoding="UTF-8" ?>
<ResourceDictionary
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    xmlns:sdp="clr-namespace:AppDimens.Maui.Markup;assembly=AppDimens.Maui.Markup"
    xmlns:ssp="clr-namespace:AppDimens.Maui.Markup;assembly=AppDimens.Maui.Markup"
    xmlns:hsp="clr-namespace:AppDimens.Maui.Markup;assembly=AppDimens.Maui.Markup"
    xmlns:wsp="clr-namespace:AppDimens.Maui.Markup;assembly=AppDimens.Maui.Markup"
    xmlns:sspa="clr-namespace:AppDimens.Maui.Markup;assembly=AppDimens.Maui.Markup"
    xmlns:hdp="clr-namespace:AppDimens.Maui.Markup;assembly=AppDimens.Maui.Markup"
    xmlns:wdp="clr-namespace:AppDimens.Maui.Markup;assembly=AppDimens.Maui.Markup"
    xmlns:sdpa="clr-namespace:AppDimens.Maui.Markup;assembly=AppDimens.Maui.Markup"
    xmlns:hdpa="clr-namespace:AppDimens.Maui.Markup;assembly=AppDimens.Maui.Markup"
    xmlns:wdpa="clr-namespace:AppDimens.Maui.Markup;assembly=AppDimens.Maui.Markup"
    xmlns:sem="clr-namespace:AppDimens.Maui.Markup;assembly=AppDimens.Maui.Markup"
    xmlns:hem="clr-namespace:AppDimens.Maui.Markup;assembly=AppDimens.Maui.Markup"
    xmlns:wem="clr-namespace:AppDimens.Maui.Markup;assembly=AppDimens.Maui.Markup"
    xmlns:sdpPh="clr-namespace:AppDimens.Maui.Markup;assembly=AppDimens.Maui.Markup"
    xmlns:sdpLw="clr-namespace:AppDimens.Maui.Markup;assembly=AppDimens.Maui.Markup"
    xmlns:sdpLh="clr-namespace:AppDimens.Maui.Markup;assembly=AppDimens.Maui.Markup"
    xmlns:sdpPw="clr-namespace:AppDimens.Maui.Markup;assembly=AppDimens.Maui.Markup"
    xmlns:hdpLw="clr-namespace:AppDimens.Maui.Markup;assembly=AppDimens.Maui.Markup"
    xmlns:hdpPw="clr-namespace:AppDimens.Maui.Markup;assembly=AppDimens.Maui.Markup"
    xmlns:wdpLh="clr-namespace:AppDimens.Maui.Markup;assembly=AppDimens.Maui.Markup"
    xmlns:wdpPh="clr-namespace:AppDimens.Maui.Markup;assembly=AppDimens.Maui.Markup"
    xmlns:sdpPhA="clr-namespace:AppDimens.Maui.Markup;assembly=AppDimens.Maui.Markup"
    xmlns:sspPh="clr-namespace:AppDimens.Maui.Markup;assembly=AppDimens.Maui.Markup"
    xmlns:sspPhA="clr-namespace:AppDimens.Maui.Markup;assembly=AppDimens.Maui.Markup"
    xmlns:scaled="clr-namespace:AppDimens.Maui.Markup;assembly=AppDimens.Maui.Markup"
    xmlns:sdpRotate="clr-namespace:AppDimens.Maui.Markup;assembly=AppDimens.Maui.Markup"
    xmlns:sspRotate="clr-namespace:AppDimens.Maui.Markup;assembly=AppDimens.Maui.Markup"
    xmlns:dimen="clr-namespace:AppDimens.Maui.Xaml;assembly=AppDimens.Maui.Xaml" />
```

In `App.xaml`:

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="Resources/Styles/AppDimensImports.xaml" />
            <!-- your other dictionaries -->
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

### Two coexisting syntaxes

| Form | Pattern | Example |
|------|---------|---------|
| **Short** | `{prefix:value}` | `{sdp:16}` |
| **Normal** | `{dimen:Type …}` | `{dimen:Sdp Value=16}` |

Both produce the **same `double`** at runtime. The normal syntax accepts `Inverter=` on all base types.

---

## XAML usage — complete reference

### Base dimensions — short syntax

| Markup | Android | Typical MAUI property |
|--------|---------|------------------------|
| `{sdp:16}` | `@dimen/_16sdp` | `Padding`, `HeightRequest`, `WidthRequest`, `Spacing`, `CornerRadius` |
| `{sdp:-8}` | `@dimen/_minus8sdp` | negative offsets |
| `{ssp:18}` | scaled sp | `FontSize` |
| `{hdp:48}` | `@dimen/_48hdp` | heights on the height axis |
| `{wdp:120}` | `@dimen/_120wdp` | widths on the width axis |
| `{sdpa:16}` | `16.sdpa` | dimension + aspect ratio |
| `{hdpa:16}` | `16.hdpa` | hdp + aspect ratio |
| `{wdpa:16}` | `16.wdpa` | wdp + aspect ratio |
| `{sem:18}` | `18.sem` | font without font scale |
| `{hem:18}` | `18.hem` | hem without font scale |
| `{wem:18}` | `18.wem` | wem without font scale |
| `{hsp:18}` | `18.hsp` | font on height axis + font scale |
| `{wsp:18}` | `18.wsp` | font on width axis + font scale |
| `{sspa:18}` | `18.sspa` | ssp + aspect ratio |
| `{hspa:18}` | `18.hspa` | hsp + aspect ratio |
| `{wspa:18}` | `18.wspa` | wsp + aspect ratio |
| `{sema:18}` | `18.sema` | sem + aspect ratio |
| `{hema:18}` | `18.hema` | hem + aspect ratio |
| `{wema:18}` | `18.wema` | wem + aspect ratio |

### Base dimensions — normal syntax

| Markup | Description |
|--------|-------------|
| `{dimen:Sdp Value=16}` | SDP smallest width |
| `{dimen:Ssp Value=18}` | SSP with font scale |
| `{dimen:Hdp Value=48}` | HDP |
| `{dimen:Wdp Value=120}` | WDP |
| `{dimen:Sdpa Value=16}` | SDPA |
| `{dimen:Hdpa Value=16}` | HDPA |
| `{dimen:Wdpa Value=16}` | WDPA |
| `{dimen:Sem Value=18}` | SEM |
| `{dimen:Hem Value=18}` | HEM |
| `{dimen:Wem Value=18}` | WEM |
| `{dimen:Hsp Value=18}` | HSP |
| `{dimen:Wsp Value=18}` | WSP |
| `{dimen:Sspa Value=18}` | SSPA |
| `{dimen:Hspa Value=18}` | HSPA |
| `{dimen:Wspa Value=18}` | WSPA |
| `{dimen:Sema Value=18}` | SEMA |
| `{dimen:Hema Value=18}` | HEMA |
| `{dimen:Wema Value=18}` | WEMA |

Optional property on **all** base `dimen:*` types:

```xml
{dimen:Sdp Value=16 Inverter=SwToPh}
```

`Inverter` values: `Default`, `PhToLw`, `PwToLh`, `LhToPw`, `LwToPh`, `SwToLh`, `SwToLw`, `SwToPh`, `SwToPw`.

### Inverter shortcuts — short syntax

| Short markup | Android | Internal inverter |
|--------------|---------|-------------------|
| `{sdpPh:16}` | `16.sdpPh` | `SwToPh` |
| `{sdpLw:16}` | `16.sdpLw` | `SwToLw` |
| `{sdpLh:16}` | `16.sdpLh` | `SwToLh` |
| `{sdpPw:16}` | `16.sdpPw` | `SwToPw` |
| `{hdpLw:12}` | `12.hdpLw` | `PhToLw` |
| `{hdpPw:12}` | `12.hdpPw` | `PwToLh` |
| `{wdpLh:12}` | `12.wdpLh` | `PwToLh` |
| `{wdpPh:12}` | `12.wdpPh` | `PhToLw` |
| `{sdpPhA:16}` | `16.sdpPha` | `SwToPh` + aspect ratio |
| `{sdpLwA:16}` | `16.sdpLwa` | `SwToLw` + aspect ratio |
| `{sdpLhA:16}` | `16.sdpLha` | `SwToLh` + aspect ratio |
| `{sdpPwA:16}` | `16.sdpPwa` | `SwToPw` + aspect ratio |

### Inverter shortcuts — typography (ssp / hsp / wsp / sem / hem / wem)

| Short markup | Android | Inverter |
|--------------|---------|----------|
| `{sspPh:18}` | `18.sspPh` | `SwToPh` |
| `{sspLw:18}` | `18.sspLw` | `SwToLw` |
| `{sspLh:18}` | `18.sspLh` | `SwToLh` |
| `{sspPw:18}` | `18.sspPw` | `SwToPw` |
| `{hspLw:18}` | `18.hspLw` | `PhToLw` |
| `{hspPw:18}` | `18.hspPw` | `LhToPw` |
| `{wspLh:18}` | `18.wspLh` | `PwToLh` |
| `{wspPh:18}` | `18.wspPh` | `LwToPh` |
| `{semPh:18}` | `18.semPh` | `SwToPh` |
| `{semLw:18}` | `18.semLw` | `SwToLw` |
| `{semLh:18}` | `18.semLh` | `SwToLh` |
| `{semPw:18}` | `18.semPw` | `SwToPw` |
| `{hemLw:18}` | `18.hemLw` | `PhToLw` |
| `{hemPw:18}` | `18.hemPw` | `LhToPw` |
| `{wemLh:18}` | `18.wemLh` | `PwToLh` |
| `{wemPh:18}` | `18.wemPh` | `LwToPh` |

### Inverter + aspect ratio shortcuts — typography

| Short markup | Android |
|--------------|---------|
| `{sspPhA:18}` | `18.sspPha` |
| `{sspLwA:18}` | `18.sspLwa` |
| `{sspLhA:18}` | `18.sspLha` |
| `{sspPwA:18}` | `18.sspPwa` |
| `{hspLwA:18}` | `18.hspLwa` |
| `{hspPwA:18}` | `18.hspPwa` |
| `{wspLhA:18}` | `18.wspLha` |
| `{wspPhA:18}` | `18.wspPha` |
| `{semaPh:18}` | `18.semaPh` |
| `{semaLw:18}` | `18.semaLw` |
| `{semaLh:18}` | `18.semaLh` |
| `{semaPw:18}` | `18.semaPw` |
| `{hemaLw:18}` | `18.hemaLw` |
| `{hemaPw:18}` | `18.hemaPw` |
| `{wemaLh:18}` | `18.wemaLh` |
| `{wemaPh:18}` | `18.wemaPh` |

Normal-syntax equivalent (example):

```xml
<Label FontSize="{dimen:Sspa Value=18 Inverter=SwToPh}" />
<Label FontSize="{dimen:Sema Value=18 Inverter=SwToLw}" />
<Label FontSize="{dimen:Sdpa Value=16 Inverter=SwToLw}" />
```

Inverter shortcuts on **hdpa/wdpa/hspa/wspa** use normal syntax only (`{dimen:Hdpa Value=16 Inverter=SwToLw}`) — there are no dedicated short tokens for those axes.

### Inverter shortcuts — normal syntax

```xml
<Label Padding="{dimen:Sdp Value=16 Inverter=SwToPh}" />
<Label Padding="{dimen:Sdp Value=16 Inverter=SwToLw}" />
<Label Padding="{dimen:Hdp Value=12 Inverter=PhToLw}" />
<Label Padding="{dimen:Wdp Value=12 Inverter=PwToLh}" />
<Label Padding="{dimen:Sdpa Value=16 Inverter=SwToPh}" />
```

### Conditional dimensions (responsive markup)

**Scaled builder** — shortcuts for tablet, landscape, and desktop:

```xml
<!-- Short: ContentProperty = Value -->
<Label FontSize="{scaled:14, Tablet=18, Landscape=16, Desktop=20}" />

<!-- Normal -->
<Label FontSize="{dimen:Scaled Value=14 Tablet=18 Landscape=16 Desktop=20}" />
```

| Property | Condition |
|----------|-----------|
| `Tablet` | `smallestWidth ≥ 600` |
| `Landscape` | landscape orientation |
| `Desktop` | `UiModeType.Desk` |

**Rotate** — alternate value by orientation (all 9 axes):

```xml
<!-- DP — short uses ContentProperty Base -->
<Label HeightRequest="{sdpRotate:30, Rotation=45, Orientation=Landscape}" />
<Label HeightRequest="{hdpRotate:48, Rotation=64, Orientation=Landscape}" />
<Label HeightRequest="{wdpRotate:120, Rotation=160, Orientation=Portrait}" />

<!-- DP — normal -->
<Label HeightRequest="{dimen:SdpRotate Base=30, Rotation=45, Orientation=Landscape}" />
<Label HeightRequest="{dimen:HdpRotate Base=48, Rotation=64, Orientation=Landscape}" />
<Label HeightRequest="{dimen:WdpRotate Base=120, Rotation=160, Orientation=Portrait}" />

<!-- SP / SEM — short -->
<Label FontSize="{sspRotate:16, Rotation=20, Orientation=Landscape}" />
<Label FontSize="{hspRotate:16, Rotation=20}" />
<Label FontSize="{wspRotate:16, Rotation=20}" />
<Label FontSize="{semRotate:16, Rotation=20}" />
<Label FontSize="{hemRotate:16, Rotation=20}" />
<Label FontSize="{wemRotate:16, Rotation=20}" />

<!-- SP / SEM — normal -->
<Label FontSize="{dimen:SspRotate Base=16, Rotation=20, Orientation=Landscape}" />
<Label FontSize="{dimen:SemRotate Base=16, Rotation=20, Orientation=Landscape}" />
<Label FontSize="{dimen:HemRotate Base=16, Rotation=20, Orientation=Landscape}" />
<Label FontSize="{dimen:WemRotate Base=16, Rotation=20, Orientation=Landscape}" />
```

| Property | Description |
|----------|-------------|
| `Base` | Value when orientation does **not** match |
| `Rotation` | Value when orientation matches |
| `Orientation` | `Portrait` or `Landscape` (default: `Landscape`) |

**Mode** — alternate value by `UiModeType`:

```xml
<Label FontSize="{sdpMode:16, Mode=24, UiMode=Desk}" />
<Label FontSize="{dimen:SdpMode Base=16, Mode=24, UiMode=Desk}" />
<Label FontSize="{sspMode:18, Mode=26, UiMode=Television}" />
<Label FontSize="{dimen:SspMode Base=18, Mode=26, UiMode=Television}" />
```

Available short tokens: `sdpMode`, `sspMode`, `hspMode`, `wspMode`, `semMode`, `hemMode`, `wemMode` (and matching `dimen:*Mode`).

**Qualifier** — alternate value when a screen metric crosses a threshold:

```xml
<Label FontSize="{sdpQualifier:16, Qualified=20, QualifierType=SmallWidth, Threshold=600}" />
<Label FontSize="{dimen:SdpQualifier Base=16, Qualified=20, QualifierType=SmallWidth, Threshold=600}" />
```

Available short tokens: `sdpQualifier`, `sspQualifier`, `hspQualifier`, `wspQualifier`, `semQualifier`, `hemQualifier`, `wemQualifier`.

**Screen** — alternate value when **both** `UiMode` and qualifier threshold match:

```xml
<Label FontSize="{sdpScreen:16, Screen=20, UiMode=Television, QualifierType=SmallWidth, Threshold=600}" />
<Label FontSize="{dimen:SdpScreen Base=16, Screen=20, UiMode=Television, QualifierType=SmallWidth, Threshold=600}" />
```

Available short tokens: `sdpScreen`, `sspScreen`, `hspScreen`, `wspScreen`, `semScreen`, `hemScreen`, `wemScreen`.

Optional `FinalQualifier` on Mode / Qualifier / Screen selects the axis used to resolve the alternate value (same defaults as C#).

> **C# only:** chained `Responsive.Value(12).Screen(UiModeType.Television, …)` beyond `{scaled:…}` shortcuts; physical pixels (`SdpPx()`, etc.); binding converters.

### Android resource keys (via code)

On Android, `@dimen/_16sdp` references an XML resource. In MAUI there is **no** XAML `ResourceDictionary` with `_16sdp` for `{StaticResource}` — buckets are consumed internally by the resolver.

MAUI equivalents:

| Android | MAUI (recommended) |
|---------|-------------------|
| `@dimen/_16sdp` | `{sdp:16}` or `{dimen:Sdp Value=16}` |
| `@dimen/_16sdp` in Kotlin | `16.Sdp()` or `Dimen._16sdp` (source gen) |
| `@dimen/_minus8sdp` | `{sdp:-8}` or `(-8).Sdp()` |
| `@dimen/_100wdp` | `{wdp:100}` or `100.Wdp()` |

Markups resolve to **pixels at page load** (like a one-shot `{StaticResource}`). The **resolver** keeps metrics up to date on rotation; to refresh on-screen sizes, update properties in code or reload the page — see [Screen rotation and resize](#screen-rotation-and-resize).

### Full page example

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    xmlns:sdp="clr-namespace:AppDimens.Maui.Markup;assembly=AppDimens.Maui.Markup"
    xmlns:ssp="clr-namespace:AppDimens.Maui.Markup;assembly=AppDimens.Maui.Markup"
    xmlns:scaled="clr-namespace:AppDimens.Maui.Markup;assembly=AppDimens.Maui.Markup"
    xmlns:dimen="clr-namespace:AppDimens.Maui.Xaml;assembly=AppDimens.Maui.Xaml"
    x:Class="MyApp.MainPage">

    <ScrollView Padding="{sdp:16}">
        <VerticalStackLayout Spacing="{sdp:12}">

            <Label Text="AppDimens MAUI"
                   FontSize="{ssp:22}"
                   FontAttributes="Bold" />

            <Label Text="Portrait→height inverter"
                   Padding="{sdpPh:12}"
                   FontSize="{ssp:14}" />

            <Label Text="Tablet / Landscape"
                   FontSize="{scaled:14, Tablet=18, Landscape=16}" />

            <BoxView Color="Gray"
                     HeightRequest="{dimen:Hdp Value=48}"
                     CornerRadius="{sdp:8}" />

            <Label Text="Negative offset"
                   Margin="{sdp:-4}" />

            <Label Text="Explicit equivalent"
                   Padding="{dimen:Sdp Value=16 Inverter=Default}" />

        </VerticalStackLayout>
    </ScrollView>
</ContentPage>
```

---

## C# usage — complete reference

### `int` extensions — base dimensions

```csharp
using AppDimens.Maui.Extensions;

int index = 16;

double a = index.Sdp();    // smallest width
double b = index.Ssp();    // sp + font scale
double c = index.Hdp();    // height
double d = index.Wdp();    // width
double e = index.Sdpa();   // sdp + aspect ratio
double f = index.Hdpa();   // hdp + aspect ratio
double g = index.Wdpa();   // wdp + aspect ratio
double h = index.Sem();    // sp without font scale
double i = index.Hem();
double j = index.Wem();

// Typography per axis + font scale
double k = index.Hsp();
double l = index.Wsp();

// Typography + aspect ratio
double m = index.Sspa();
double n = index.Hspa();
double o = index.Wspa();
double p = index.Sema();   // sem + aspect ratio
double q = index.Hema();
double r = index.Wema();

// Negatives (sdp/hdp/wdp only)
double offset = (-8).Sdp();
```

### `int` extensions — inverters (DP)

```csharp
double ph  = 16.SdpPh();   // SwToPh
double lw  = 16.SdpLw();   // SwToLw
double lh  = 16.SdpLh();   // SwToLh
double pw  = 16.SdpPw();   // SwToPw
double hlw = 12.HdpLw();
double hpw = 12.HdpPw();
double wlh = 12.WdpLh();
double wph = 12.WdpPh();

// DP + aspect ratio
double pha = 16.SdpPha();
double lwa = 16.SdpLwa();
double lha = 16.SdpLha();
double pwa = 16.SdpPwa();
```

### `int` extensions — inverters (SP)

```csharp
// SSP (smallest width + font scale)
18.SspPh(); 18.SspLw(); 18.SspLh(); 18.SspPw();
18.SspPha(); 18.SspLwa(); 18.SspLha(); 18.SspPwa();

// HSP / WSP
18.HspLw(); 18.HspPw();
18.WspLh(); 18.WspPh();
18.HspLwa(); 18.HspPwa();
18.WspLha(); 18.WspPha();

// SEM / HEM / WEM (no font scale)
18.SemPh(); 18.SemLw(); 18.SemLh(); 18.SemPw();
18.HemLw(); 18.HemPw();
18.WemLh(); 18.WemPh();
18.SemaPh(); 18.SemaLw(); 18.SemaLh(); 18.SemaPw();
18.HemaLw(); 18.HemaPw();
18.WemaLh(); 18.WemaPh();
```

### `int` extensions — physical pixels

```csharp
double pxSdp = 16.SdpPx();  // sdp × density
double pxSsp = 18.SspPx();
double pxHsp = 18.HspPx();
double pxWsp = 18.WspPx();
double pxSem = 18.SemPx();
```

### MAUI control extensions

```csharp
using AppDimens.Maui.Extensions;

// Uniform padding on Layout or Border
myStack.PaddingSdp(16);
myBorder.PaddingSdp(12);

// Uniform margin on any View
myLabel.MarginSdp(8);

// FontSize on Label
myLabel.FontSsp(20);
myLabel.FontHsp(20);
myLabel.FontWsp(20);
myLabel.FontSem(20);
```

For asymmetric padding/margin, compute each side:

```csharp
var h = 16.Sdp();
var v = 8.Sdp();
myStack.Padding = new Thickness(h, v);
```

### `AppDimensResolver` — advanced API

```csharp
using AppDimens.Maui;
using AppDimens.Maui.Core;
using AppDimens.Maui.Inverters;

var r = AppDimensResolver.Instance;

// Typed shortcuts
r.Sdp(16);
r.Ssp(18);
r.Hsp(18);
r.Wsp(18);
r.Hdp(48);
r.Wdp(120);
r.Sdpa(16);
r.Sspa(18);
r.Hspa(18);
r.Wspa(18);
r.Sem(18);
r.Sema(18);
r.Hem(18);
r.Hema(18);
r.Wem(18);
r.Wema(18);

// With inverter
r.Sdp(16, InverterType.SwToPh);
r.Ssp(18, InverterType.SwToPh);
r.Sspa(18, InverterType.SwToLw);
r.Sem(18, InverterType.SwToLw);

// Generic resolve
r.Resolve(
    index: 16,
    baseQualifier: DpQualifier.SmallWidth,
    inverter: InverterType.Default,
    applyAspectRatio: false,
    applyFontScale: false,
    allowNegative: true);

// Active bucket (Bucket/Hybrid mode)
var bucketSw = r.Buckets?.GetActiveBucket(DpQualifier.SmallWidth);

// Font scale
r.SetFontScale(1.15f);
```

---

## Orientation inverters

Inverters swap the **resource axis** (sw / h / w) by orientation — equivalent to the Android `Inverter` enum.

| `InverterType` | In landscape | In portrait |
|----------------|--------------|-------------|
| `SwToPh` | — | sw → **height** |
| `SwToPw` | — | sw → **width** |
| `SwToLh` | sw → **height** | — |
| `SwToLw` | sw → **width** | — |
| `PhToLw` | height → **width** | — |
| `PwToLh` | width → **height** | — |
| `LhToPw` | — | height → **width** |
| `LwToPh` | — | width → **height** |
| `Default` | no swap | no swap |

Shortcut → inverter mapping:

| C# / XAML shortcut | Inverter |
|--------------------|----------|
| `SdpPh` / `{sdpPh:N}` | `SwToPh` |
| `SdpLw` / `{sdpLw:N}` | `SwToLw` |
| `SdpLh` / `{sdpLh:N}` | `SwToLh` |
| `SdpPw` / `{sdpPw:N}` | `SwToPw` |
| `HdpLw` / `{hdpLw:N}` | `PhToLw` |
| `HdpPw` / `{hdpPw:N}` | `PwToLh` |
| `WdpLh` / `{wdpLh:N}` | `PwToLh` |
| `WdpPh` / `{wdpPh:N}` | `PhToLw` |

---

## Responsive builders

Equivalent to Android `Int.scaledDp()` / `DimenScaled`.

```csharp
using AppDimens.Maui.Builders;
using AppDimens.Maui.Core;

// Chained shortcuts
double size = Responsive.Value(14)
    .Tablet(18)      // smallestWidth ≥ 600
    .Landscape(16)   // landscape orientation
    .Desktop(20)     // UiModeType.Desk
    .Sdp();

// From int
double fromScaledDp = 14.ScaledDp()
    .Screen(DpQualifier.SmallWidth, 480, 16)
    .Sdp();

// h/w axes
double h = Responsive.Value(14).Tablet(18).Hdp();
double w = Responsive.Value(14).Landscape(16).Wdp();
```

### Builder priorities (`Screen`)

Rules evaluated in order (Android parity):

| Priority | Method | Condition |
|----------|--------|-----------|
| **1** | `Screen(UiModeType, DpQualifier, qualifierValue, customValue, …)` | UI mode + metric ≥ value |
| **2** | `Screen(UiModeType, customValue, …)` | UI mode (+ optional orientation) |
| **3** | `Screen(DpQualifier, value, customValue, …)` | metric ≥ value (+ optional orientation) |
| **4** | `Screen(OrientationQualifier, customValue, …)` | orientation |

Tie-break: lower priority wins; within the same priority, larger `qualifierValue` wins.

### Optional `Screen()` parameters

```csharp
.Screen(
    uiMode: UiModeType.Television,
    qualifierType: DpQualifier.SmallWidth,
    qualifierValue: 720,
    customValue: 24,
    finalQualifier: DpQualifier.SmallWidth,  // axis used to resolve customValue
    orientation: OrientationQualifier.Landscape,
    inverter: InverterType.SwToPh)
```

Advanced example:

```csharp
var dimen = Responsive.Value(12)
    .Screen(UiModeType.Television, DpQualifier.SmallWidth, 720, 24)
    .Screen(DpQualifier.SmallWidth, 600, 18)
    .Screen(OrientationQualifier.Landscape, 16)
    .Sdp();
```

### `UiModeType` values

`Normal`, `Television`, `Car`, `Watch`, `Desk`, `Appliance`, `VrHeadset`, `Undefined`.

---

## Conditional helpers

Equivalent to Android `sdpRotate`, `sspRotate`, `sdpQualifier`, etc. Available for **all axes**: `sdp`/`hdp`/`wdp` and `ssp`/`hsp`/`wsp`/`sem`/`hem`/`wem`.

Each helper has **short** (`AppDimens.Maui.Markup`), **normal** (`AppDimens.Maui.Xaml`), and **C#** (`DimenFacilitators`) forms.

```csharp
using AppDimens.Maui.Core;
using AppDimens.Maui.Helpers;

// Rotate — DP
30.SdpRotate(45);
30.HdpRotate(45);
30.WdpRotate(45);

// Rotate — SP (font)
16.SspRotate(20);
16.HspRotate(20);
16.WspRotate(20);
16.SemRotate(20);
16.HemRotate(20);
16.WemRotate(20);

// Qualifier — DP and SP
16.SdpQualifier(qualifiedValue: 20, qualifierType: DpQualifier.SmallWidth, qualifierThreshold: 600);
18.SspQualifier(22, DpQualifier.SmallWidth, 600);
18.HspQualifier(22, DpQualifier.Height, 800);
18.SemQualifier(20, DpQualifier.SmallWidth, 600);

// Mode — DP and SP
16.SdpMode(modeValue: 24, uiMode: UiModeType.Television);
18.SspMode(26, UiModeType.Television);
18.WemMode(26, UiModeType.Desk);

// Screen — DP and SP
16.SdpScreen(screenValue: 20, uiMode: UiModeType.Television,
    qualifierType: DpQualifier.SmallWidth, qualifierThreshold: 600);
18.SspScreen(22, UiModeType.Television, DpQualifier.SmallWidth, 600);
```

XAML examples (short · normal):

```xml
<!-- Rotate -->
<BoxView WidthRequest="{sdpRotate:30, Rotation=45, Orientation=Landscape}" />
<BoxView WidthRequest="{dimen:SdpRotate Base=30, Rotation=45, Orientation=Landscape}" />

<!-- Qualifier -->
<Label FontSize="{sdpQualifier:16, Qualified=20, QualifierType=SmallWidth, Threshold=600}" />
<Label FontSize="{dimen:SdpQualifier Base=16, Qualified=20, QualifierType=SmallWidth, Threshold=600}" />

<!-- Mode -->
<Label FontSize="{sdpMode:16, Mode=24, UiMode=Desk}" />
<Label FontSize="{dimen:SdpMode Base=16, Mode=24, UiMode=Desk}" />

<!-- Screen -->
<Label FontSize="{sdpScreen:16, Screen=20, UiMode=Television, QualifierType=SmallWidth, Threshold=600}" />
<Label FontSize="{dimen:SdpScreen Base=16, Screen=20, UiMode=Television, QualifierType=SmallWidth, Threshold=600}" />
```

Optional parameter `finalQualifier` sets the axis used to resolve the alternate value (default by type: `SmallWidth` for sdp/ssp/sem, `Height` for hdp/hsp/hem, `Width` for wdp/wsp/wem).

> **Note:** helpers like `hdpMode`, `wdpQualifier`, etc. for **DP** still use **sdp-prefixed** names on Android only; in MAUI use `SdpMode`/`SdpQualifier` or `AppDimensResolver.Resolve()` with an explicit axis.

---

## Converters and physical units

For **data binding** with `{Binding}`:

```xml
<ContentPage xmlns:conv="clr-namespace:AppDimens.Maui.Converters;assembly=AppDimens.Maui.Converters">
    <ContentPage.Resources>
        <conv:SdpConverter x:Key="SdpConv" />
        <conv:SspConverter x:Key="SspConv" />
        <conv:HspConverter x:Key="HspConv" />
        <conv:WspConverter x:Key="WspConv" />
        <conv:SemConverter x:Key="SemConv" />
        <conv:HdpConverter x:Key="HdpConv" />
        <conv:WdpConverter x:Key="WdpConv" />
        <conv:MmToPxConverter x:Key="MmConv" />
    </ContentPage.Resources>

    <BoxView HeightRequest="{Binding Source=48, Converter={StaticResource HdpConv}}" />
</ContentPage>
```

| Converter | Input | Output |
|-----------|-------|--------|
| `SdpConverter` | `int` or `double` index | `double` sdp |
| `SspConverter` | `int` or `double` index | `double` ssp |
| `HspConverter` | `int` or `double` index | `double` hsp |
| `WspConverter` | `int` or `double` index | `double` wsp |
| `SemConverter` | `int` or `double` index | `double` sem |
| `HdpConverter` | `int` or `double` index | `double` hdp |
| `WdpConverter` | `int` or `double` index | `double` wdp |
| `MmToPxConverter` | `double` mm | pixels (uses current density) |

### `DimenPhysicalUnits` (C#)

```csharp
using AppDimens.Maui.Converters;
using AppDimens.Maui.Core;

double density = AppDimensResolver.Instance.Metrics.Current.Density;

// To pixels
double px = DimenPhysicalUnits.MmToPx(10, density);
double px2 = DimenPhysicalUnits.CmToPx(1, density);
double px3 = DimenPhysicalUnits.InchToPx(1, density);

// Pure conversions (no density)
double inch = DimenPhysicalUnits.MmToInch(25.4);
double cm = DimenPhysicalUnits.MmToCm(100);
double mm = DimenPhysicalUnits.CmToMm(10);
double inchFromCm = DimenPhysicalUnits.CmToInch(2.54);
double mmFromInch = DimenPhysicalUnits.InchToMm(1);

// Geometry
double r = DimenPhysicalUnits.Radius(20, UnitType.Mm, density);
double d = DimenPhysicalUnits.Diameter(10, UnitType.Mm, density);
double circ = DimenPhysicalUnits.Circumference(5, UnitType.Cm, density);
double area = DimenPhysicalUnits.Area(5, UnitType.Cm, density);
```

`UnitType`: `Inch`, `Cm`, `Mm`, `Sp`, `Dp`, `Px`.

---

## Source Generator

The package includes the `AppDimens.Maui.SourceGen` analyzer for typed static access:

```csharp
using AppDimens.Maui.Generated;

double p = Dimen._16sdp;   // AppDimensSdps.Sdp(16)
double f = Dimen._20ssp;   // AppDimensSdps.Ssp(20)
// Generated indices: 1…96 (sdp and ssp)
```

Active automatically when referencing `Bodenberg.AppDimens.Maui.Sdps` (no extra configuration).

For indices > 96 or negatives, use extensions (`16.Sdp()`) or XAML markups.

---

## Static `AppDimensSdps` API

Alternative without extensions:

```csharp
using AppDimens.Maui;

AppDimensSdps.Initialize();
AppDimensSdps.Warmup();

double a = AppDimensSdps.Sdp(16);
double b = AppDimensSdps.Ssp(18);
double c = AppDimensSdps.Hsp(18);
double d = AppDimensSdps.Wsp(18);
double e = AppDimensSdps.Hdp(48);
double f = AppDimensSdps.Wdp(120);
double g = AppDimensSdps.Sdpa(16);
double h = AppDimensSdps.Sspa(18);
double i = AppDimensSdps.Sem(18);
double j = AppDimensSdps.Sema(18);

// Advanced access
var resolver = AppDimensSdps.Resolver;
var metrics = resolver.Metrics.Current;
```

Static methods: `Sdp`, `Ssp`, `Hsp`, `Wsp`, `Hdp`, `Wdp`, `Sdpa`, `Sspa`, `Hspa`, `Wspa`, `Sem`, `Sema`, `Hem`, `Hema`, `Wem`, `Wema`. Inverters via `AppDimensResolver` or extensions.

---

## Screen metrics

`AppDimensResolver.Instance.Metrics.Current` returns `ScreenMetricsSnapshot`:

| Field | Description |
|-------|-------------|
| `WidthDp` | Logical width in dp |
| `HeightDp` | Logical height in dp |
| `SmallestDp` | `min(WidthDp, HeightDp)` |
| `Density` | Pixel density |
| `DensityDpi` | DPI (160 × density) |
| `Orientation` | `Portrait` or `Landscape` |
| `UiMode` | Device `UiModeType` |

Updated automatically on `MainDisplayInfoChanged` and on `Warmup()`.

---

## Compatible MAUI properties

Markups return `double`. Use them on properties that accept `double`:

| Property | Recommended markup |
|----------|-------------------|
| `Padding`, `Margin` | `{sdp:N}` (uniform) or `Thickness` in C# |
| `FontSize` | `{ssp:N}` or `{sem:N}` |
| `HeightRequest`, `WidthRequest` | `{sdp:N}`, `{hdp:N}`, `{wdp:N}` |
| `Spacing`, `RowSpacing`, `ColumnSpacing` | `{sdp:N}` |
| `CornerRadius` | `{sdp:N}` |
| `MinimumHeightRequest`, `MaximumWidthRequest`, etc. | any type per axis |

`Padding` and `Margin` with markups apply a **uniform** value on all four sides. For per-side values, use C# `Thickness`.

---

## Migration Android → MAUI

### XML / resources

| Android | MAUI (short) | MAUI (normal) |
|---------|--------------|---------------|
| `@dimen/_16sdp` | `{sdp:16}` | `{dimen:Sdp Value=16}` |
| `android:padding="@dimen/_16sdp"` | `Padding="{sdp:16}"` | `Padding="{dimen:Sdp Value=16}"` |
| `@dimen/_18sdp` (text) | `{ssp:18}` | `{dimen:Ssp Value=18}` |
| `@dimen/_48hdp` | `{hdp:48}` | `{dimen:Hdp Value=48}` |
| `@dimen/_120wdp` | `{wdp:120}` | `{dimen:Wdp Value=120}` |
| `@dimen/_minus8sdp` | `{sdp:-8}` | `{dimen:Sdp Value=-8}` |
| `values-sw600dp/` | automatic via bucket when `smallestDp ≥ 600` | — |

### Kotlin / Java → C#

| Android | MAUI C# |
|---------|---------|
| `16.sdp` | `16.Sdp()` or `AppDimensSdps.Sdp(16)` |
| `16.ssp` | `16.Ssp()` |
| `16.sdpPh` | `16.SdpPh()` or `{sdpPh:16}` |
| `16.sdpa` | `16.Sdpa()` or `{sdpa:16}` |
| `16.sdpRotate(45)` | `16.SdpRotate(45)` |
| `16.scaledDp().screen(...)` | `16.ScaledDp().Screen(...).Sdp()` or `Responsive.Value(16)...` |
| `DimenSdp.warmup(ctx)` | `AppDimensSdps.Warmup()` |
| `Dimen._16sdp` (generated) | `Dimen._16sdp` (MAUI source gen) |

### What **does not** exist (by design)

- Android qualifiers `-land` / `-port` → use inverters + builders
- `@dimen` in project Android `res/values` → use MAUI markup or C# extensions
- Compose `@Composable` API → use MAUI markup or C#
- `{StaticResource _16sdp}` XAML ResourceDictionary → use `{sdp:16}` or source gen

---

## Performance and caching

- **`DimensionCache`**: thread-safe `ConcurrentDictionary`; O(1) lookup after first calculation
- **Automatic invalidation** on screen change (rotation, DPI, resize)
- **`Warmup()`**: precomputes aspect-ratio factors (cold start)
- **Hot path**: zero reflection; source generator for frequent indices (1–96)
- Suitable for virtualized lists, animations, and dashboards

Internal BenchmarkDotNet run: cached lookup hit is typically **< 50 ns** on desktop-class hardware.

---

## Known limitations

| Item | Status |
|------|--------|
| DP helpers `hdpMode`, `wdpQualifier`, etc. | Helper is **sdp**-named only; use `Resolve()` for an explicit axis |
| Source generator | Indices **1–96** (sdp/ssp); others via extensions |
| `PaddingSdp` / `MarginSdp` | Uniform value only |
| Buckets in `Bucket` mode | Requires `Generated` folder accessible at runtime |
| `sema`/`hema`/`wema` | MAUI extension beyond Android (sem + aspect ratio) |
| Release trimming | Handled by the package (`ILLink.Descriptors.xml` + `buildTransitive` targets); no app-side linker config required |

---

## Frequently asked questions

**Do I need to call `UseAppDimensSdps()`?**  
Yes. Without initialization, metrics stay zero and dimensions will be wrong.

**Which mode should I use?**  
`Hybrid` (default) for Android parity with a safe fallback. `Continuous` for math-only sizing. `Bucket` for discrete values matching Android XML.

**Do markups recalculate on rotation?**  
The resolver **yes** — metrics and cache update automatically after `UseAppDimensSdps()`. XAML markup (`{sdp:16}`, etc.) is evaluated **when the page loads**; it does not live-bind to rotation unless you update properties in code or re-navigate. C# (`16.Sdp()` in `OnAppearing` / `SizeChanged`) always uses current metrics. See [Screen rotation and resize](#screen-rotation-and-resize).

**Can I use code-behind without XAML?**  
Yes — `16.Sdp()` and `AppDimensSdps.Sdp(16)` are the primary path.

**How do I integrate accessibility font scale?**  
Call `AppDimensResolver.Instance.SetFontScale(...)` when system preference changes.

**Does it work on Windows/macOS?**  
Yes — it uses MAUI Essentials `DeviceDisplay` on all supported platforms.

**Release build crashes with `MarkupExtension not found for sdp:…`?**  
That was IL trimming removing XAML-only types. **Fixed in the package:** `AppDimens.Maui.Markup`, `AppDimens.Maui.Xaml`, and `AppDimens.Maui.Converters` ship trimmer roots automatically (NuGet `buildTransitive` + embedded `ILLink.Descriptors.xml`). Update to a version that includes this fix; no extra linker XML is required in your app.

---

## Package contents

`Bodenberg.AppDimens.Maui.Sdps` bundles:

| Assembly | Role |
|----------|------|
| `AppDimens.Maui` | `AppDimensResolver`, `UseAppDimensSdps()` |
| `AppDimens.Maui.Core` | Formulas, enums, cache, metrics |
| `AppDimens.Maui.Responsive` | Buckets, qualifiers, merge |
| `AppDimens.Maui.Inverters` | Axis inversion engine |
| `AppDimens.Maui.Markup` | Short markup `{sdp:16}` |
| `AppDimens.Maui.Xaml` | Normal markup `{dimen:Sdp Value=16}` |
| `AppDimens.Maui.Extensions` | `16.Sdp()`, `PaddingSdp()` |
| `AppDimens.Maui.Helpers` | Rotate, qualifier, mode, screen |
| `AppDimens.Maui.Builders` | `Responsive.Value()` |
| `AppDimens.Maui.Converters` | `IValueConverter`, physical units |
| `AppDimens.Maui.Resources` | Bucket data (~358×3 axes) |
| `AppDimens.Maui.SourceGen` | `Dimen._Nsdp` analyzer |

---

## License

Apache-2.0 — Copyright © Jean Bodenberg

Repository: [appdimens-net](https://github.com/bodenberg/appdimens-net) · Android reference: [appdimens-sdps](https://github.com/bodenberg/appdimens-sdps)

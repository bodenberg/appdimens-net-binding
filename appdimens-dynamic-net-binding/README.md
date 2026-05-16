# Bodenberg.AppDimens.Dynamic

**.NET binding for Android** that wraps **[AppDimens Dynamic](https://github.com/bodenberg/appdimens-dynamic)** — responsive **`dp` / `sp` / `px`** sizing in code (no XML dimen grids), with **15 scaling strategies**, conditional **facilitators**, **aspect-ratio** and **multi-window** variants, **physical units**, **auto-resize** helpers, and optional **DataStore-backed** caching.

Kotlin/Java sources, strategy semantics, and Android-focused guides live upstream:

**→ [bodenberg/appdimens-dynamic](https://github.com/bodenberg/appdimens-dynamic)** · [DOCUMENTATION](https://github.com/bodenberg/appdimens-dynamic/tree/main/DOCUMENTATION) · [KDoc search](https://appdimens3.web.app/)

Maven artifact embedded in this package: **`io.github.bodenberg:appdimens-dynamic`** [`3.1.5`](https://repo1.maven.org/maven2/io/github/bodenberg/appdimens-dynamic/3.1.5/).

---

## Install

```bash
dotnet add package Bodenberg.AppDimens.Dynamic --version 3.5.1.2
```

---

## Requirements

| Requirement | Notes |
|-------------|--------|
| **Target framework** | `net8.0-android` (.NET for Android / .NET MAUI Android) |
| **Minimum Android API** | **24** (matches the packaged AAR) |
| **Workload** | Android (`dotnet workload install android`) or MAUI |
| **JDK** | **17 or 21** for the Xamarin.Android toolchain on .NET 8 |

Use **Android SDK platform 34+** (or MSBuild `InstallAndroidDependencies`) when building bindings locally.

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
| **Xamarin.AndroidX.DataStore.Preferences** | **`DimenCache`** persistence (**1.1.7.1** on net8; Maven library targets **1.2.1**) |

---

## Package version vs embedded AAR

| Layer | Version |
|-------|---------|
| **Maven / embedded `.aar`** | **`appdimens-dynamic` 3.1.5** |
| **NuGet** | **`3.5.1.2`** — binding packaging/readme/metadata (NuGet project site &amp; source URLs) without changing the **3.1.5** Android binary |

When Maven publishes **`3.5.1`**, run `./scripts/sync-aar-from-maven.sh 3.5.1` and align the fourth NuGet segment as needed.

---

## Build this binding locally

```bash
cd appdimens-dynamic-net-binding
dotnet build AppDimens.Dynamic.sln -c Release
```

Optional smoke APK: **`AppDimens.Dynamic.SmokeTest`**. See [NUGET-PUBLISH.md](NUGET-PUBLISH.md).

Sync the upstream AAR:

```bash
./scripts/sync-aar-from-maven.sh 3.1.5
```

---

## Links

| Resource | URL |
|----------|-----|
| AppDimens (main project — NuGet **Project website**) | https://github.com/bodenberg/appdimens |
| Android library (NuGet **Source repository**; implementation &amp; issues) | https://github.com/bodenberg/appdimens-dynamic |
| Strategy documentation | https://github.com/bodenberg/appdimens-dynamic/tree/main/DOCUMENTATION |
| Maven Central | https://repo1.maven.org/maven2/io/github/bodenberg/appdimens-dynamic/3.1.5/ |
| .NET binding monorepo | https://github.com/bodenberg/appdimens-net-binding |

---

## License

**Apache 2.0**, consistent with the upstream library.

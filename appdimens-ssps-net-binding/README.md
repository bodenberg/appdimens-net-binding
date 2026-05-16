# Bodenberg.AppDimens.Ssps

**.NET binding for Android** that wraps **[AppDimens SSPS](https://github.com/bodenberg/appdimens-ssps)** — responsive **typography and `sp`** scaling via thousands of pre-generated **`@dimen`** resources (**SSP / HSP / WSP**), plus imperative APIs, **conditional facilitators**, **aspect-ratio** variants, **accessibility** modes, and **foldable-aware** overloads.

Kotlin/Java sources, XML resource naming, and Android-focused docs live upstream:

**→ [bodenberg/appdimens-ssps](https://github.com/bodenberg/appdimens-ssps)**

Maven artifact embedded in this package: **`io.github.bodenberg:appdimens-ssps`** [`3.1.5`](https://repo1.maven.org/maven2/io/github/bodenberg/appdimens-ssps/3.1.5/).

---

## Install

```bash
dotnet add package Bodenberg.AppDimens.Ssps --version 3.5.1.2
```

---

## Requirements

| Requirement | Notes |
|-------------|--------|
| **Target framework** | `net8.0-android` (.NET for Android / .NET MAUI Android) |
| **Minimum Android API** | **24** |
| **Workload** | Android or MAUI |
| **JDK** | **17 or 21** on .NET 8 |

The AAR bundles **`values-sw*`** / **`values-w*`** / **`values-h*`** dimen XML (sizes **1–600**) — usable from Android XML layouts and from code.

---

## What this library provides

| Capability | Summary |
|------------|---------|
| **SSP / HSP / WSP** | Scale **text** by **smallest width**, **height**, or **width** |
| **SEM / HEM / WEM** | Same axes but **ignore system font scale** (accessibility-fixed curves) |
| **XML `@dimen`** | e.g. `@dimen/_16ssp`, `@dimen/_32hsp`, `@dimen/_100wsp` in layouts |
| **Code API** | **`DimenSsp`** static methods → **pixel** values for `TextView`, custom drawing, MAUI labels |
| **Aspect ratio** | **`Sspa`**, **`Hspa`**, **`Wspa`**, **`Sema`**, … — extra geometry multiplier on top of XML bucket |
| **Inverters** | **`SspPh`**, **`SspLw`**, **`HspLw`**, **`WspLh`**, … — orientation-aware axis switch |
| **Facilitators** | **`SspRotate`**, **`SspMode`**, **`SspQualifier`**, **`SspScreen`** (+ **HSP** / **WSP** twins) |
| **`ScaledSp` builder** | Multi-condition chains with **priority-based** rule matching |
| **Prefetch** | **`WarmupSspsFactors`** — optional AR factor warmup |
| **Low-level** | **`GetDimensionInSpPx`**, **`GetResourceId`** — custom qualifier/inverter paths |

SSPS is **typography-focused** (companion to **AppDimens SDPS** for general **dp** layout, and **AppDimens Dynamic** for code-only multi-strategy scaling).

---

## C# namespaces (binding layout)

| Area | Namespace |
|------|-----------|
| **Views / imperative (recommended)** | `Com.Appdimens.Ssps.Code` — `DimenSsp`, `ScaledSp`, `CustomSpEntry` |
| **Shared types** | `Com.Appdimens.Ssps.Common` — `DpQualifier`, `DpQualifierEntry`, `Inverter`, `Orientation`, `UiModeType` |
| **Factors / internals** | `Com.Appdimens.Ssps.Core` — `AppDimensSspsFactors` |
| **Compose (limited)** | `Com.Appdimens.Ssps.Compose` — see **Jetpack Compose** below |

Foldable overloads accept **`AndroidX.Window.Layout.IFoldingFeature`**.

---

## Quick start (Activity / Views)

```csharp
using Android.App;
using Android.OS;
using Com.Appdimens.Ssps.Code;

[Activity(Label = "My App", MainLauncher = true)]
public class MainActivity : Activity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        DimenSsp.WarmupSspsFactors(this);

        float titlePx = DimenSsp.Ssp(this, 18);
        float bodyPx  = DimenSsp.Ssp(this, 14);
        float bannerPx = DimenSsp.Hsp(this, 32);
    }
}
```

Apply **`titlePx`** to `TextView.TextSize` (pixels) or convert as needed for your UI stack.

---

## Core methods (`DimenSsp`)

All return **pixels** (`float`) for a nominal **sp** integer (e.g. `16` → `_16ssp` resource bucket).

### Primary axes

| Method | Scales by |
|--------|-----------|
| **`Ssp` / `Sspa` / `Sspi` / `Sspia`** | Smallest width (+ AR / multi-window variants) |
| **`Hsp` / `Hspa` / …** | Screen height |
| **`Wsp` / `Wspa` / …** | Screen width |

### Accessibility (ignore font scale)

| Method | Behavior |
|--------|----------|
| **`Sem` / `Sema` / …** | Smallest width, **no** user font-scale factor |
| **`Hem` / `Hema` / …** | Height axis, fixed scale |
| **`Wem` / `Wema` / …** | Width axis, fixed scale |

Use when typography must stay on the library curve regardless of system **font scale** settings.

### Inverter shortcuts

Switch which axis drives the value depending on orientation:

| Example | Meaning (simplified) |
|---------|----------------------|
| **`SspPh` / `SspPha`** | SSP by smallest width → **height** in portrait |
| **`SspLw` / `SspLwa`** | SSP → **width** in landscape |
| **`HspLw` / `HspLwa`** | Height-based → **width** in landscape |
| **`WspLh` / `WspLha`** | Width-based → **height** in landscape |

Each has **`a`** / **`i`** / **`ia`** variants matching the base API.

### Resource IDs

```csharp
int resId = DimenSsp.GetResourceId(this,
    Com.Appdimens.Ssps.Common.DpQualifier.SmallWidth, 16,
    Com.Appdimens.Ssps.Common.Inverter.None);
// Use with Resources.GetDimension(resId) if needed
```

### Low-level resolution

```csharp
float px = DimenSsp.GetDimensionInSpPx(this,
    Com.Appdimens.Ssps.Common.DpQualifier.SmallWidth,
    16,
    Com.Appdimens.Ssps.Common.Inverter.None,
    ignoreFontScale: false);
```

---

## Facilitators (conditional typography)

Same concepts as **AppDimens Dynamic**, resolved through XML buckets:

```csharp
using Com.Appdimens.Ssps.Code;
using Com.Appdimens.Ssps.Common;

// 16 sp default, 24 sp in landscape
float rotated = DimenSsp.SspRotate(this, 16, 24);
float rotatedLandscape = DimenSsp.SspRotate(this, 16, 24, Orientation.Landscape);

// UI mode (e.g. TV)
float tv = DimenSsp.SspMode(this, 16, 40, UiModeType.Television);

// Qualifier bucket (e.g. sw600)
float tablet = DimenSsp.SspQualifier(this, 16, 22, DpQualifier.SmallWidth, 600);

// Combined rules (+ optional FoldingFeature)
float fold = DimenSsp.SspScreen(this, 16, 28,
    UiModeType.Television, DpQualifier.SmallWidth, 600);
```

**`HspRotate`**, **`WspMode`**, **`HspScreen`**, etc. mirror the **HSP** / **WSP** families.

Kotlin-style extension facilitators (`sspRotatePlain`, nested chains) appear as **`DimenSspExtensionsKt`** when generated; for C#, prefer **two-argument plain overloads** documented upstream to avoid double resource resolution.

---

## `ScaledSp` builder

Priority-ordered rules (first match wins — **not** the same as nested extensions):

```csharp
using Com.Appdimens.Ssps.Code;
using Com.Appdimens.Ssps.Common;

var chain = DimenSsp.Scaled(16)
    .Screen(UiModeType.Television, DpQualifier.SmallWidth, 600, 40)
    .Screen(UiModeType.Television, 32)
    .Screen(Orientation.Landscape, 20);

float px = chain.Ssp(this);           // smallest-width fallback
float pxNoFs = chain.Sem(this);      // ignore font scale
```

Explore generated members on **`ScaledSp`** for **`Hsp`**, **`Wsp`**, **`Resolve`**, and foldable parameters.

---

## XML layouts (Android resources in the AAR)

Use bundled dimens directly in AXAML/Android XML:

```xml
<TextView
    android:textSize="@dimen/_16ssp"
    android:lineSpacingExtra="@dimen/_4ssp" />

<TextView android:textSize="@dimen/_18wsp" />
<TextView android:textSize="@dimen/_20hsp" />
```

Naming: **`_<n>ssp`**, **`_<n>hsp`**, **`_<n>wsp`** (and related variants). Resources ship inside the embedded AAR; no extra Gradle dependency beyond this NuGet for .NET consumers.

---

## Aspect ratio (`*a` / `*ia`)

**`Sspa`**, **`Hspa`**, **`Wspa`**, **`Sema`**, … apply an **aspect-ratio multiplier** on top of the XML-resolved size (parity with **appdimens-dynamic** when the same bucket is selected).

```csharp
DimenSsp.WarmupSspsFactors(this);
float normal = DimenSsp.Ssp(this, 32);
float withAr = DimenSsp.Sspa(this, 32);
```

Factors invalidate when **`smallestScreenWidthDp`**, **`screenWidthDp`**, **`screenHeightDp`**, or **`densityDpi`** change.

---

## Shared types (`Com.Appdimens.Ssps.Common`)

| Type | Purpose |
|------|---------|
| **`DpQualifier`** | `SmallWidth`, `Width`, `Height`, … |
| **`DpQualifierEntry`** | Qualifier + threshold pairs for builders |
| **`Inverter`** | Axis inversion mode for `GetDimensionInSpPx` |
| **`Orientation`** | Portrait / landscape facilitators |
| **`UiModeType`** | TV, car, watch, etc. |
| **`EffectiveDpQualifierKt`** | Effective qualifier helpers |

---

## Jetpack Compose (`com.appdimens.ssps.compose`)

The AAR includes Compose extensions (`16.ssp`, `scaledSp()`, `sspRotate`, …) requiring **`Composer`**. The current binding pipeline **does not produce fully usable C# Compose wrappers** for most of these APIs.

**Recommended for .NET:**

- **`Com.Appdimens.Ssps.Code.DimenSsp`** in Activities, Fragments, custom views  
- **XML `@dimen/_Nssp`** in Android layout resources  
- **`ScaledSp`** builder from C# where generated  

---

## Dependencies brought in by this package

| NuGet | Role |
|-------|------|
| **Xamarin.Kotlin.StdLib** | Kotlin runtime |
| **Xamarin.AndroidX.Core** | Core / resources |
| **Xamarin.AndroidX.Window** | Foldables |
| **Xamarin.AndroidX.Lifecycle.Runtime** | Lifecycle alignment |

---

## Package version vs embedded AAR

| Layer | Version |
|-------|---------|
| **Maven / embedded `.aar`** | **`appdimens-ssps` 3.1.5** |
| **NuGet** | **`3.5.1.2`** — binding packaging/readme/metadata (NuGet project site &amp; source URLs) without changing the **3.1.5** Android binary |

When Maven publishes **`3.5.1`**, run `./scripts/sync-aar-from-maven.sh 3.5.1` and align the fourth NuGet segment as needed.

---

## Build this binding locally

```bash
cd appdimens-ssps-net-binding
dotnet build AppDimens.Ssps.sln -c Release
```

Smoke test: **`AppDimens.Ssps.SmokeTest`**. See [NUGET-PUBLISH.md](NUGET-PUBLISH.md).

```bash
./scripts/sync-aar-from-maven.sh 3.1.5
```

---

## Related packages

| Package | Focus |
|---------|--------|
| **[Bodenberg.AppDimens.Sdps](https://www.nuget.org/packages/Bodenberg.AppDimens.Sdps)** | **SDP/HDP/WDP** layout dimensions (companion layout library) |
| **[Bodenberg.AppDimens.Dynamic](https://www.nuget.org/packages/Bodenberg.AppDimens.Dynamic)** | Code-only **multi-strategy** dp/sp (no XML grids) |

---

## Links

| Resource | URL |
|----------|-----|
| AppDimens (main project — NuGet **Project website**) | https://github.com/bodenberg/appdimens |
| Android library (NuGet **Source repository**; implementation &amp; issues) | https://github.com/bodenberg/appdimens-ssps |
| Maven Central | https://repo1.maven.org/maven2/io/github/bodenberg/appdimens-ssps/3.1.5/ |
| .NET binding monorepo | https://github.com/bodenberg/appdimens-net-binding |

---

## License

**Apache 2.0**, consistent with the upstream library.

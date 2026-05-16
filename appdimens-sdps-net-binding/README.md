# Bodenberg.AppDimens.Sdps

**.NET binding for Android** that wraps **[AppDimens SDPS](https://github.com/bodenberg/appdimens-sdps)** — responsive **layout** (`SDP` / `HDP` / `WDP`) and **typography** (`SSP` / `HSP` / `WSP`) in one library, backed by thousands of pre-generated **`@dimen`** resources, plus imperative APIs, **conditional facilitators**, **aspect-ratio** variants, **accessibility** text modes, and **foldable-aware** overloads.

Kotlin/Java sources, XML naming, and Android-focused guides live upstream:

**→ [bodenberg/appdimens-sdps](https://github.com/bodenberg/appdimens-sdps)**

Maven artifact embedded in this package: **`io.github.bodenberg:appdimens-sdps`** [`3.1.5`](https://central.sonatype.com/artifact/io.github.bodenberg/appdimens-sdps/3.1.5).

---

## Install

```bash
dotnet add package Bodenberg.AppDimens.Sdps --version 3.5.1.2
```

Pin a version explicitly when you rely on a specific package revision (see **Package version vs embedded AAR** below).

---

## Requirements

| Requirement | Notes |
|-------------|--------|
| **Target framework** | `net8.0-android` (.NET for Android / .NET MAUI Android) |
| **Minimum Android API** | **24** (matches the packaged AAR) |
| **Workload** | Android (`dotnet workload install android`) or MAUI |
| **JDK** | **17 or 21** for the Xamarin.Android toolchain on .NET 8 |

Use **Android SDK platform 34+** (or MSBuild `InstallAndroidDependencies`) when building bindings locally.

The AAR bundles **`values-sw*`** / **`values-w*`** / **`values-h*`** dimen XML (sizes **1–600**) for both **dp** and **sp** buckets.

---

## What this library provides

| Capability | Summary |
|------------|---------|
| **SDP / HDP / WDP** | Scale **margins, padding, sizes** by smallest width, height, or width |
| **SSP / HSP / WSP** | Scale **text** on the same three axes |
| **SEM / HEM / WEM** | Typography curves that **ignore system font scale** |
| **XML `@dimen`** | e.g. `@dimen/_16sdp`, `@dimen/_32hsp` — zero-config in layouts |
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

Same conceptual model as layout, but for **sp** buckets (`_16ssp`, `_32hsp`, …).

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
        android:textSize="@dimen/_18ssp"
        android:lineSpacingExtra="@dimen/_4ssp" />
</LinearLayout>
```

Naming pattern: **`_<n>sdp`**, **`_<n>hdp`**, **`_<n>wdp`**, **`_<n>ssp`**, **`_<n>hsp`**, **`_<n>wsp`** (and **`sem`** / **`hem`** / **`wem`** variants where generated). Values **1–600** are pre-calculated per qualifier bucket inside the AAR.

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
- **XML `@dimen/_Nsdp`** / **`_@dimen/_Nssp`** in layout resources  
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
| **Maven / embedded `.aar`** | **`appdimens-sdps` 3.1.5** |
| **NuGet** | **`3.5.1.2`** — binding packaging/readme/metadata (NuGet project site &amp; source URLs) **without** changing the Android **3.1.5** binary |

When Maven publishes **`3.5.1`**, run `./scripts/sync-aar-from-maven.sh 3.5.1` and align the fourth NuGet segment as needed.

---

## Build this binding locally

```bash
cd appdimens-sdps-net-binding
dotnet build AppDimens.Sdps.sln -c Release
```

Smoke test: **`AppDimens.Sdps.SmokeTest`**. See [NUGET-PUBLISH.md](NUGET-PUBLISH.md).

```bash
./scripts/sync-aar-from-maven.sh 3.1.5
```

---

## Related packages

| Package | Focus |
|---------|--------|
| **[Bodenberg.AppDimens.Ssps](https://www.nuget.org/packages/Bodenberg.AppDimens.Ssps)** | **Typography only** (SSP/HSP/WSP) — smaller dependency if you do not need sdp/hdp/wdp XML |
| **[Bodenberg.AppDimens.Dynamic](https://www.nuget.org/packages/Bodenberg.AppDimens.Dynamic)** | **Code-only** scaling — 15 strategies, no pre-built `@dimen` grids |

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

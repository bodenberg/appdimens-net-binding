# AppDimens.Net.Fit

**The `DimenFit` builder — one base value plus per-screen overrides (size class, orientation, UI mode),
resolved with a deterministic priority chain. No nested `if`s.**

| | |
|---|---|
| **NuGet** | [`AppDimens.Net.Fit`](https://www.nuget.org/packages/AppDimens.Net.Fit) (requires `AppDimens.Net`) |
| **Namespace** | `AppDimens.Net.Code.Fit` |
| **Frameworks** | `net8.0`, `net9.0`, `net10.0` (+ `net11.0`) — MAUI, WPF, Windows Forms, WinUI 3, console |
| **API reference** | [API.md](API.md) |

---

## Table of contents

1. [When to use](#when-to-use)
2. [Install and setup](#install-and-setup)
3. [Your first rule set](#your-first-rule-set)
4. [Every kind of rule](#every-kind-of-rule)
5. [Priority order](#priority-order)
6. [Modifiers](#modifiers)
7. [Resolving: axis, explicit context, all three at once](#resolving-axis-explicit-context-all-three-at-once)
8. [Text sizes](#text-sizes)
9. [Kernel without the builder: `ToFitDp`](#kernel-without-the-builder-tofitdp)
10. [Recipes](#recipes)
11. [Português (resumo)](#português-resumo)

---

## When to use

| Scenario | Recommendation |
|----------|----------------|
| Several device-specific values for one dimension (phone / tablet / TV / landscape) | ✅ Fit |
| Smooth curve, no breakpoints | [Fluid](../AppDimens.Net.Fluid/README.md) |
| A single condition inline | core facilitators (`SdpQualifier`, `SdpRotate`) — see [AppDimens.Net](../AppDimens.Net/README.md) |

## Install and setup

```bash
dotnet add package AppDimens.Net.Fit
```

Create the window context once (MAUI `AppDimensMaui.AttachWindow`, WPF `AppDimensWpf.Initialize`,
WinForms `AppDimensWinForms.Initialize`, WinUI `AppDimensWinUI.Initialize`, or `HostDimensScope`) —
see [AppDimens.Net → Create the window context](../AppDimens.Net/README.md#create-the-window-context).

## Your first rule set

```csharp
using AppDimens.Net.Code.Fit;
using AppDimens.Net.Common;

float cardPaddingPx = 16.FitScaledDp()                // base: 16 (scaled)
    .Screen(DpQualifier.SmallWidth, 600, 24)          // tablets (smallest width ≥ 600) → 24
    .Screen(Orientation.Landscape, 20)                // any landscape → 20
    .Ftsdp();                                         // resolve on the smallest-width axis
```

> ⚠️ `Ftsdp()`, `Fthdp()`, `Ftwdp()` and `ResolveAll()` return **pixels** (scaled dp × density), exactly like the
> Kotlin `ftsdp` code API. For MAUI / WPF / WinUI sizes divide by the density:
> `float dp = px / AppDimensAmbient.Require().Density;` — Windows Forms and canvas drawing can use the pixels directly.

## Every kind of rule

```csharp
using AppDimens.Net.Code.Fit;
using AppDimens.Net.Common;

var fit = DimenFit.Create(18f)                                                    // same as 18.FitScaledDp()
    .Screen(DpQualifier.Width, 1200, 32, finalQualifierResolver: DpQualifier.Width) // width ≥ 1200, scaled on width
    .Screen(DpQualifier.SmallWidth, 600, 26)                                      // size class
    .Screen(Orientation.Landscape, 22)                                            // orientation
    .Screen(UiModeType.Television, 40)                                            // UI mode
    .Screen(UiModeType.FoldOpen, Orientation.Portrait, 30);                       // UI mode + orientation

float value = fit.Ftsdp();
```

## Priority order

Rules are checked by priority group; inside a group the **first declared matching rule wins**:

1. **UI-mode rules** — `Screen(UiModeType, value)` and `Screen(UiModeType, Orientation, value)`
2. **Size-qualifier rules** — `Screen(DpQualifier, threshold, value)` (declare the **largest threshold first**)
3. **Orientation rules** — `Screen(Orientation, value)`
4. otherwise the **base value**

The chosen number is then scaled with the Fit kernel on the requested axis (or on `finalQualifierResolver`).

## Modifiers

```csharp
using AppDimens.Net.Code.Fit;

float v = 16.FitScaledDp()
    .ApplyAspectRatio()          // aspect-ratio correction (false to disable: ApplyAspectRatio(false))
    .IgnoreMultiWindows()        // do not shrink in split screen
    .CustomSensitivity(0.8f)     // flatter curve (bypasses the cache)
    .Ftsdp();
```

## Resolving: axis, explicit context, all three at once

```csharp
using AppDimens.Net;
using AppDimens.Net.Code.Fit;
using AppDimens.Net.Common;

var rules = 48.FitScaledDp().Screen(DpQualifier.SmallWidth, 600, 64);

float onSmallestWidth = rules.Ftsdp();   // ambient context (pixels)
float onHeight        = rules.Fthdp();
float onWidth         = rules.Ftwdp();

var ctx = AppDimensAmbient.Require();
float explicitSw = rules.Ftsdp(ctx);     // explicit context
var (sw, h, w) = rules.ResolveAll(ctx);  // all three axes in one pass (pixels)
float swDp = sw / ctx.Density;           // dp for MAUI / WPF / WinUI
```

## Text sizes

```csharp
using AppDimens.Net.Code.Fit;
using AppDimens.Net.Common;

float title = 20.FitScaledSp()
    .Screen(DpQualifier.SmallWidth, 600, 26)
    .Screen(UiModeType.Television, 34)
    .Ftsdp();
```

## Kernel without the builder: `ToFitDp`

```csharp
using AppDimens.Net;
using AppDimens.Net.Code.Fit;
using AppDimens.Net.Common;

var ctx = AppDimensAmbient.Require();
float dp = 24.ToFitDp(ctx, DpQualifier.Width, Inverter.Default, ignoreMultiWindows: false, applyAspectRatio: true);
float px = 24f.ToFitPx(ctx);
```

## Recipes

```csharp
using AppDimens.Net.Code.Fit;
using AppDimens.Net.Common;

using AppDimens.Net;

var ctx = AppDimensAmbient.Require();

// Card width: phones 280, tablets 360, large windows 480 (largest threshold first!)
float cardWidthDp = DimenFit.Create(280)
    .Screen(DpQualifier.Width, 1200, 480)
    .Screen(DpQualifier.Width, 600, 360)
    .Ftwdp(ctx) / ctx.Density;

// TV-friendly button height in pixels (e.g. Windows Forms / canvas)
float buttonHeightPx = 48.FitScaledDp().Screen(UiModeType.Television, 72).Fthdp();
```

---

## Português (resumo)

`dotnet add package AppDimens.Net.Fit` → `using AppDimens.Net.Code.Fit;` →
`16.FitScaledDp().Screen(DpQualifier.SmallWidth, 600, 24).Screen(Orientation.Landscape, 20).Ftsdp()`.
Regras: tamanho de tela (`DpQualifier`), orientação, modo de UI (TV, dobrável…). Resolva com `Ftsdp()` (menor lado),
`Fthdp()` (altura), `Ftwdp()` (largura) ou `ResolveAll(ctx)` — retornam **pixels** (divida por `ctx.Density` para dp).
Regras de modo de UI têm prioridade, depois tamanho de tela, depois orientação; dentro do grupo vence a primeira declarada.

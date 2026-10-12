# AppDimens.Net.Fluid

**Fluid scaling — smooth, clamped growth between a phone and a tablet reference (0.8× → 1.2×).**

Part of **AppDimens for .NET** — a bit-for-bit C# port of [appdimens-kmp](https://github.com/bodenberg/appdimens-kmp).
Requires the core package [`AppDimens.Net`](../AppDimens.Net/README.md) (installed automatically).

| | |
|---|---|
| **NuGet** | [`AppDimens.Net.Fluid`](https://www.nuget.org/packages/AppDimens.Net.Fluid) |
| **Namespace** | `AppDimens.Net.Code.Fluid` |
| **Method prefix** | `F` → `FSdp`, `FHdp`, `FWdp` |
| **Frameworks** | `net8.0`, `net9.0`, `net10.0` (+ `net11.0` with SDK 11) — MAUI, WPF, Windows Forms, WinUI 3, console, tests |
| **API reference** | [API.md](API.md) (every public type and member) |

---

## Table of contents

1. [What it does](#what-it-does)
2. [What values to expect](#what-values-to-expect)
3. [Install](#install)
4. [Setup (once per app)](#setup-once-per-app)
5. [Everyday usage](#everyday-usage)
6. [All variants: `a`, `i`, `ia`, `Px`](#all-variants-a-i-ia-px)
7. [Explicit context (no ambient window)](#explicit-context-no-ambient-window)
8. [Full control: `FSdpCustom`](#full-control-fsdpcustom)
9. [Different value in landscape: `RotateRaw`](#different-value-in-landscape-rotateraw)
10. [Generic kernel: `ToFluidDp` / `ToFluidPx`](#generic-kernel-tofluiddp--tofluidpx)
11. [Using it in each UI framework](#using-it-in-each-ui-framework)
13. [Testing](#testing)
14. [Performance](#performance)
15. [Português (resumo)](#português-resumo)

---

## What it does

The **Fluid** strategy interpolates smoothly between a minimum (0.8×) on small screens and a maximum (1.2×) on large ones, so values **never explode** on big monitors and never collapse on tiny phones. Great for resizable desktop windows, split view and foldables. `16.FSdp()` gives 12.8 at ≤ 300 dp, 13.4 at 360 dp, 16.8 at 600 dp and 19.2 from 800 dp up.

| Scenario | Recommendation |
|----------|----------------|
| Desktop windows / split view / foldables | ✅ Fluid |
| Discrete per-breakpoint values | Fit builder |
| Strictly proportional layouts | Scaled (core) / Percent |

## What values to expect

`16` scaled with this strategy (density 1.0, default font scale), compared with the core **Scaled** strategy:

| Call | Phone 360×800 | Tablet 800×1280 | Desktop 1280×800 |
|------|--------------:|----------------:|-----------------:|
| `16.FSdp()` | 13.4 | 19.2 | 19.2 |
| `16.FHdp()` | 19.2 | 19.2 | 19.2 |
| `16.FWdp()` | 13.4 | 19.2 | 19.2 |
| `16.Sdp()` (Scaled, for comparison) | 19.2 | 42.7 | 42.7 |

## Install

```bash
dotnet add package AppDimens.Net.Fluid
```

or install everything at once with [`AppDimens.Net.Sdk`](../AppDimens.Net.Sdk/README.md).

## Setup (once per app)

Parameterless calls such as `16.FSdp()` use the **ambient window context**. Create it once, before building UI:

| Framework | Package | Call |
|-----------|---------|------|
| .NET MAUI | `AppDimens.Net.Maui` | `AppDimensMaui.AttachWindow(window)` in `CreateWindow` |
| WPF | `AppDimens.Net.Wpf` | `AppDimensWpf.Initialize(this)` before `InitializeComponent()` |
| Windows Forms | `AppDimens.Net.WinForms` | `AppDimensWinForms.Initialize(this)` |
| WinUI 3 | `AppDimens.Net.WinUI` | `AppDimensWinUI.Initialize(window)` in `OnLaunched` |
| Anything else | `AppDimens.Net` | `AppDimensHost.Activate(new HostDimensScope())` + `scope.Update(w, h, density)` |

See the [beginner's guide](https://github.com/bodenberg/appdimens-net-binding/blob/main/appdimens-net/DOCUMENTATION/GUIDE-FOR-BEGINNERS.md) for complete per-framework examples.

## Everyday usage

```csharp
using AppDimens.Net.Code.Fluid;

float padding = 16.FSdp();    // follows the smaller side of the window (default choice)
float header  = 56.FHdp();    // follows the window height
float sidebar = 200.FWdp();   // follows the window width
```

All calls return `float` **dp** (device-independent units): use them directly for MAUI `WidthRequest`, WPF/WinUI
`Width`, `Margin`, `FontSize`…

## All variants: `a`, `i`, `ia`, `Px`

| Suffix | Meaning | Example |
|--------|---------|---------|
| *(none)* | default curve | `16.FSdp()` |
| `a` | adds the **aspect-ratio** correction (very tall/wide screens) | `16.FSdpa()` |
| `i` | **ignores multi-window / split-screen** shrinking | `16.FSdpi()` |
| `ia` | both | `16.FSdpia()` |
| `Px` | same value in **physical pixels** (needs a context) | `16.FSdpPx(ctx)` |

Every axis has every suffix: `FSdp`, `FSdpa`, `FSdpi`, `FSdpia`, `FHdp`, `FHdpa`, `FHdpi`, `FHdpia`,
`FWdp`, `FWdpa`, `FWdpi`, `FWdpia`, and the `Px` forms `FSdpPx`, `FSdpaPx`, `FSdpiPx`, `FSdpiaPx` (same for `H`/`W`).

```csharp
using AppDimens.Net;
using AppDimens.Net.Code.Fluid;

var ctx = AppDimensAmbient.Require();

float tall       = 16.FSdpa();
float splitSafe  = 16.FSdpi();
float both       = 16.FSdpia();
float pixels     = 16.FSdpPx(ctx);       // e.g. for SkiaSharp / Android canvas / WinForms
float heightPx   = 48.FHdpaPx(ctx);
```

## Explicit context (no ambient window)

Every lane accepts an `IAppDimensContext`, and the static class `DimenFluid` offers the same lanes:

```csharp
using AppDimens.Net.Core;
using AppDimens.Net.Testing;
using AppDimens.Net.Code.Fluid;

IAppDimensContext ctx = new FakeAppDimensContext();   // or your window scope

float a = 16.FSdp(ctx);
float b = DimenFluid.FSdp(ctx, 16);       // static form
float c = DimenFluid.FWdpia(ctx, 120);

DimenFluid.WarmupCache(ctx);               // optional: pre-computes the factors for this window
```

## Full control: `FSdpCustom`

```csharp
using AppDimens.Net.Common;
using AppDimens.Net.Code.Fluid;

float custom = 16.FSdpCustom(
    qualifier: DpQualifier.SmallWidth,  // axis: SmallWidth, Height or Width
    inverter: Inverter.SwToLh,          // swap axis by orientation (see core docs)
    imw: false,                         // ignore multi-window
    ar: true,                           // aspect-ratio correction
    k: 0.8f);                           // custom sensitivity (null = library default)

float customH = 48.FHdpCustom(DpQualifier.Height, Inverter.Default, imw: false, ar: false);
float customW = 120.FWdpCustom(DpQualifier.Width, Inverter.Default, imw: true, ar: false);
```

## Different value in landscape: `RotateRaw`

Returns the scaled base value, or the **raw** alternate value when the screen is in the given orientation:

```csharp
using AppDimens.Net;
using AppDimens.Net.Common;
using AppDimens.Net.Code.Fluid;

var ctx = AppDimensAmbient.Require();

float columnWidth = 120.FSdpRotateRaw(ctx, rawAlternate: 180f);                     // landscape → 180
float barHeight   = 56.FHdpRotateRaw(ctx, rawAlternate: 48f, Orientation.Portrait);  // portrait  → 48
```

## Generic kernel: `ToFluidDp` / `ToFluidPx`

One method with named options — useful when the axis or flags come from data:

```csharp
using AppDimens.Net;
using AppDimens.Net.Common;
using AppDimens.Net.Code.Fluid;

var ctx = AppDimensAmbient.Require();

float dp = 24.ToFluidDp(ctx, qualifier: DpQualifier.Width, ignoreMultiWindows: true);
float px = 24.ToFluidPx(ctx, qualifier: DpQualifier.Height, applyAspectRatio: true);
float fromFloat = 12.5f.ToFluidDp(ctx);
```

## Using it in each UI framework

### .NET MAUI

```csharp
using AppDimens.Net.Code.Fluid;

card.Padding         = new Thickness(16.FSdp());
titleLabel.FontSize  = 20.FSdp();
banner.HeightRequest = 120.FHdp();
```

### WPF / WinUI 3

```csharp
using AppDimens.Net.Code.Fluid;

card.Padding  = new Thickness(16.FSdp());
banner.Height = 120.FHdp();
// re-apply after resizes:  AppDimensWpf.Changed += (_, _) => banner.Height = 120.FHdp();
```

### Windows Forms

```csharp
using AppDimens.Net.WinForms;
using AppDimens.Net.Code.Fluid;

AppDimensWinForms.Apply(this, () =>
{
    panel1.Padding = DimenPx.Padding(16.FSdp());
    banner.Height  = DimenPx.ToPx(120.FHdp());
});
```

### XAML (WPF / WinUI 3 / MAUI)

`FSdp`, `FHdp`, `FWdp` and their `a` / `i` / `ia` variants have ready-made markup extensions:

```xml
<!-- WPF: AppDimens.Net.Wpf  (live, updates on resize) -->
<Border xmlns:ad="clr-namespace:AppDimens.Net.Wpf;assembly=AppDimens.Net.Wpf"
        Width="{ad:FWdp 200}" Height="{ad:FHdp 80}" Padding="{ad:FSdp 16}" />

<!-- WinUI 3: AppDimens.Net.WinUI  (re-applied on resize) -->
<Border xmlns:ad="using:AppDimens.Net.WinUI"
        Width="{ad:FWdp Value=200}" Height="{ad:FHdp Value=80}" Padding="{ad:FSdp Value=16}" />
```

MAUI (`AppDimens.Net.Maui`) ships `{appdimens:Fsdp Value=24}`.

## Testing

```csharp
using AppDimens.Net.Testing;
using AppDimens.Net.Code.Fluid;

var ctx = new FakeAppDimensContext();
ctx.SetConfig(360, 800);                // phone
float phone = 16.FSdp(ctx);
ctx.SetConfig(800, 1280);               // tablet
ctx.NotifyChange();                     // fire watchers like a real resize
float tablet = 16.FSdp(ctx);
```

## Performance

Same engine as the core: snapshot-partitioned cache, allocation-free fast lanes for the parameterless calls, and
automatic invalidation when the window changes. Custom sensitivity (`k`) values bypass the cache by design.

---

## Português (resumo)

* **O que é:** Fluid scaling — smooth, clamped growth between a phone and a tablet reference (0.8× → 1.2×).
* **Instalar:** `dotnet add package AppDimens.Net.Fluid` (ou `AppDimens.Net.Sdk` com todas as estratégias).
* **Inicializar uma vez** (MAUI `AppDimensMaui.AttachWindow`, WPF `AppDimensWpf.Initialize`, WinForms
  `AppDimensWinForms.Initialize`, WinUI `AppDimensWinUI.Initialize`).
* **Usar:** `using AppDimens.Net.Code.Fluid;` → `16.FSdp()` (tamanhos), `56.FHdp()` (altura), `200.FWdp()` (largura);
  sufixos `a` (proporção da tela), `i` (ignora multi-janela) e `Px` (pixels).
* **Referência completa:** [API.md](API.md).

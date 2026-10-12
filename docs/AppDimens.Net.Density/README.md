# AppDimens.Net.Density

**Density-aware scaling — multiplies by the display density (dpi ÷ 160), not by the window size.**

Part of **AppDimens for .NET** — a bit-for-bit C# port of [appdimens-kmp](https://github.com/bodenberg/appdimens-kmp).
Requires the core package [`AppDimens.Net`](../AppDimens.Net/README.md) (installed automatically).

| | |
|---|---|
| **NuGet** | [`AppDimens.Net.Density`](https://www.nuget.org/packages/AppDimens.Net.Density) |
| **Namespace** | `AppDimens.Net.Code.Density` |
| **Method prefix** | `D` → `DSdp`, `DHdp`, `DWdp` |
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
8. [Full control: `DSdpCustom`](#full-control-dsdpcustom)
9. [Different value in landscape: `RotateRaw`](#different-value-in-landscape-rotateraw)
10. [Generic kernel: `ToDensityDp` / `ToDensityPx`](#generic-kernel-todensitydp--todensitypx)
11. [Using it in each UI framework](#using-it-in-each-ui-framework)
13. [Testing](#testing)
14. [Performance](#performance)
15. [Português (resumo)](#português-resumo)

---

## What it does

The **Density** strategy scales the value by the display density relative to 160 dpi (Android *mdpi*): `16.DSdp()` is 16 at 160 dpi, 32 at 320 dpi and 48 at 480 dpi. It does **not** follow the window size — use it when an element should keep the same *physical* proportion across low- and high-density screens. On Windows the density is the monitor scale (100 % = 1.0, 150 % = 1.5).

| Scenario | Recommendation |
|----------|----------------|
| Physical-size-critical elements across DPI buckets | ✅ Density |
| Layouts that follow the window size | Scaled (core) / Auto |
| Real-world units (mm / inch) | Units |

## What values to expect

`16` scaled with this strategy (density 1.0, default font scale), compared with the core **Scaled** strategy:

| Call | Phone 360×800 | Tablet 800×1280 | Desktop 1280×800 |
|------|--------------:|----------------:|-----------------:|
| `16.DSdp()` | 16 | 16 | 16 |
| `16.DHdp()` | 16 | 16 | 16 |
| `16.DWdp()` | 16 | 16 | 16 |
| `16.Sdp()` (Scaled, for comparison) | 19.2 | 42.7 | 42.7 |

All three screens in the table use 160 dpi, so the value stays 16. With the same 360×800 screen: 120 dpi → 12, 320 dpi → 32, 480 dpi → 48.

## Install

```bash
dotnet add package AppDimens.Net.Density
```

or install everything at once with [`AppDimens.Net.Sdk`](../AppDimens.Net.Sdk/README.md).

## Setup (once per app)

Parameterless calls such as `16.DSdp()` use the **ambient window context**. Create it once, before building UI:

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
using AppDimens.Net.Code.Density;

float padding = 16.DSdp();    // follows the smaller side of the window (default choice)
float header  = 56.DHdp();    // follows the window height
float sidebar = 200.DWdp();   // follows the window width
```

All calls return `float` **dp** (device-independent units): use them directly for MAUI `WidthRequest`, WPF/WinUI
`Width`, `Margin`, `FontSize`…

## All variants: `a`, `i`, `ia`, `Px`

| Suffix | Meaning | Example |
|--------|---------|---------|
| *(none)* | default curve | `16.DSdp()` |
| `a` | adds the **aspect-ratio** correction (very tall/wide screens) | `16.DSdpa()` |
| `i` | **ignores multi-window / split-screen** shrinking | `16.DSdpi()` |
| `ia` | both | `16.DSdpia()` |
| `Px` | same value in **physical pixels** (needs a context) | `16.DSdpPx(ctx)` |

Every axis has every suffix: `DSdp`, `DSdpa`, `DSdpi`, `DSdpia`, `DHdp`, `DHdpa`, `DHdpi`, `DHdpia`,
`DWdp`, `DWdpa`, `DWdpi`, `DWdpia`, and the `Px` forms `DSdpPx`, `DSdpaPx`, `DSdpiPx`, `DSdpiaPx` (same for `H`/`W`).

```csharp
using AppDimens.Net;
using AppDimens.Net.Code.Density;

var ctx = AppDimensAmbient.Require();

float tall       = 16.DSdpa();
float splitSafe  = 16.DSdpi();
float both       = 16.DSdpia();
float pixels     = 16.DSdpPx(ctx);       // e.g. for SkiaSharp / Android canvas / WinForms
float heightPx   = 48.DHdpaPx(ctx);
```

## Explicit context (no ambient window)

Every lane accepts an `IAppDimensContext`, and the static class `DimenDensity` offers the same lanes:

```csharp
using AppDimens.Net.Core;
using AppDimens.Net.Testing;
using AppDimens.Net.Code.Density;

IAppDimensContext ctx = new FakeAppDimensContext();   // or your window scope

float a = 16.DSdp(ctx);
float b = DimenDensity.DSdp(ctx, 16);       // static form
float c = DimenDensity.DWdpia(ctx, 120);

DimenDensity.WarmupCache(ctx);               // optional: pre-computes the factors for this window
```

## Full control: `DSdpCustom`

```csharp
using AppDimens.Net.Common;
using AppDimens.Net.Code.Density;

float custom = 16.DSdpCustom(
    qualifier: DpQualifier.SmallWidth,  // axis: SmallWidth, Height or Width
    inverter: Inverter.SwToLh,          // swap axis by orientation (see core docs)
    imw: false,                         // ignore multi-window
    ar: true,                           // aspect-ratio correction
    k: 0.8f);                           // custom sensitivity (null = library default)

float customH = 48.DHdpCustom(DpQualifier.Height, Inverter.Default, imw: false, ar: false);
float customW = 120.DWdpCustom(DpQualifier.Width, Inverter.Default, imw: true, ar: false);
```

## Different value in landscape: `RotateRaw`

Returns the scaled base value, or the **raw** alternate value when the screen is in the given orientation:

```csharp
using AppDimens.Net;
using AppDimens.Net.Common;
using AppDimens.Net.Code.Density;

var ctx = AppDimensAmbient.Require();

float columnWidth = 120.DSdpRotateRaw(ctx, rawAlternate: 180f);                     // landscape → 180
float barHeight   = 56.DHdpRotateRaw(ctx, rawAlternate: 48f, Orientation.Portrait);  // portrait  → 48
```

## Generic kernel: `ToDensityDp` / `ToDensityPx`

One method with named options — useful when the axis or flags come from data:

```csharp
using AppDimens.Net;
using AppDimens.Net.Common;
using AppDimens.Net.Code.Density;

var ctx = AppDimensAmbient.Require();

float dp = 24.ToDensityDp(ctx, qualifier: DpQualifier.Width, ignoreMultiWindows: true);
float px = 24.ToDensityPx(ctx, qualifier: DpQualifier.Height, applyAspectRatio: true);
float fromFloat = 12.5f.ToDensityDp(ctx);
```

## Using it in each UI framework

### .NET MAUI

```csharp
using AppDimens.Net.Code.Density;

card.Padding         = new Thickness(16.DSdp());
titleLabel.FontSize  = 20.DSdp();
banner.HeightRequest = 120.DHdp();
```

### WPF / WinUI 3

```csharp
using AppDimens.Net.Code.Density;

card.Padding  = new Thickness(16.DSdp());
banner.Height = 120.DHdp();
// re-apply after resizes:  AppDimensWpf.Changed += (_, _) => banner.Height = 120.DHdp();
```

### Windows Forms

```csharp
using AppDimens.Net.WinForms;
using AppDimens.Net.Code.Density;

AppDimensWinForms.Apply(this, () =>
{
    panel1.Padding = DimenPx.Padding(16.DSdp());
    banner.Height  = DimenPx.ToPx(120.DHdp());
});
```

### XAML

There is no dedicated markup extension for this strategy: set the values from C# (as above) and re-apply
them in the framework's change event (`AppDimensWpf.Changed`, `AppDimensWinUI.Changed`,
`AppDimensWinForms.Apply`, or MAUI `SizeChanged`).

## Testing

```csharp
using AppDimens.Net.Testing;
using AppDimens.Net.Code.Density;

var ctx = new FakeAppDimensContext();
ctx.SetConfig(360, 800);                // phone
float phone = 16.DSdp(ctx);
ctx.SetConfig(800, 1280);               // tablet
ctx.NotifyChange();                     // fire watchers like a real resize
float tablet = 16.DSdp(ctx);
```

## Performance

Same engine as the core: snapshot-partitioned cache, allocation-free fast lanes for the parameterless calls, and
automatic invalidation when the window changes. Custom sensitivity (`k`) values bypass the cache by design.

---

## Português (resumo)

* **O que é:** Density-aware scaling — multiplies by the display density (dpi ÷ 160), not by the window size.
* **Instalar:** `dotnet add package AppDimens.Net.Density` (ou `AppDimens.Net.Sdk` com todas as estratégias).
* **Inicializar uma vez** (MAUI `AppDimensMaui.AttachWindow`, WPF `AppDimensWpf.Initialize`, WinForms
  `AppDimensWinForms.Initialize`, WinUI `AppDimensWinUI.Initialize`).
* **Usar:** `using AppDimens.Net.Code.Density;` → `16.DSdp()` (tamanhos), `56.DHdp()` (altura), `200.DWdp()` (largura);
  sufixos `a` (proporção da tela), `i` (ignora multi-janela) e `Px` (pixels).
* **Referência completa:** [API.md](API.md).

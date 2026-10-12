# AppDimens.Net.Perimeter

**Perimeter scaling — balances width and height in a single number.**

Part of **AppDimens for .NET** — a bit-for-bit C# port of [appdimens-kmp](https://github.com/bodenberg/appdimens-kmp).
Requires the core package [`AppDimens.Net`](../AppDimens.Net/README.md) (installed automatically).

| | |
|---|---|
| **NuGet** | [`AppDimens.Net.Perimeter`](https://www.nuget.org/packages/AppDimens.Net.Perimeter) |
| **Namespace** | `AppDimens.Net.Code.Perimeter` |
| **Method prefix** | `PR` → `PRSdp`, `PRHdp`, `PRWdp` |
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
8. [Full control: `PRSdpCustom`](#full-control-prsdpcustom)
9. [Different value in landscape: `RotateRaw`](#different-value-in-landscape-rotateraw)
10. [Generic kernel: `ToPerimeterDp` / `ToPerimeterPx`](#generic-kernel-toperimeterdp--toperimeterpx)
11. [Using it in each UI framework](#using-it-in-each-ui-framework)
13. [Testing](#testing)
14. [Performance](#performance)
15. [Português (resumo)](#português-resumo)

---

## What it does

The **Perimeter** strategy derives the scale from the sum of both screen axes, balancing width *and* height contributions. It gives stable results for edge-anchored UI: gutters, rails, borders, safe areas. All axes give the same result.

| Scenario | Recommendation |
|----------|----------------|
| Edge / gutter systems | ✅ Perimeter |
| Rotation-invariant sizing | Diagonal |
| Literal fractions of the window | Percent |

## What values to expect

`16` scaled with this strategy (density 1.0, default font scale), compared with the core **Scaled** strategy:

| Call | Phone 360×800 | Tablet 800×1280 | Desktop 1280×800 |
|------|--------------:|----------------:|-----------------:|
| `16.PRSdp()` | 22.3 | 40 | 40 |
| `16.PRHdp()` | 22.3 | 40 | 40 |
| `16.PRWdp()` | 22.3 | 40 | 40 |
| `16.Sdp()` (Scaled, for comparison) | 19.2 | 42.7 | 42.7 |

## Install

```bash
dotnet add package AppDimens.Net.Perimeter
```

or install everything at once with [`AppDimens.Net.Sdk`](../AppDimens.Net.Sdk/README.md).

## Setup (once per app)

Parameterless calls such as `16.PRSdp()` use the **ambient window context**. Create it once, before building UI:

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
using AppDimens.Net.Code.Perimeter;

float padding = 16.PRSdp();    // follows the smaller side of the window (default choice)
float header  = 56.PRHdp();    // follows the window height
float sidebar = 200.PRWdp();   // follows the window width
```

All calls return `float` **dp** (device-independent units): use them directly for MAUI `WidthRequest`, WPF/WinUI
`Width`, `Margin`, `FontSize`…

## All variants: `a`, `i`, `ia`, `Px`

| Suffix | Meaning | Example |
|--------|---------|---------|
| *(none)* | default curve | `16.PRSdp()` |
| `a` | adds the **aspect-ratio** correction (very tall/wide screens) | `16.PRSdpa()` |
| `i` | **ignores multi-window / split-screen** shrinking | `16.PRSdpi()` |
| `ia` | both | `16.PRSdpia()` |
| `Px` | same value in **physical pixels** (needs a context) | `16.PRSdpPx(ctx)` |

Every axis has every suffix: `PRSdp`, `PRSdpa`, `PRSdpi`, `PRSdpia`, `PRHdp`, `PRHdpa`, `PRHdpi`, `PRHdpia`,
`PRWdp`, `PRWdpa`, `PRWdpi`, `PRWdpia`, and the `Px` forms `PRSdpPx`, `PRSdpaPx`, `PRSdpiPx`, `PRSdpiaPx` (same for `H`/`W`).

```csharp
using AppDimens.Net;
using AppDimens.Net.Code.Perimeter;

var ctx = AppDimensAmbient.Require();

float tall       = 16.PRSdpa();
float splitSafe  = 16.PRSdpi();
float both       = 16.PRSdpia();
float pixels     = 16.PRSdpPx(ctx);       // e.g. for SkiaSharp / Android canvas / WinForms
float heightPx   = 48.PRHdpaPx(ctx);
```

## Explicit context (no ambient window)

Every lane accepts an `IAppDimensContext`, and the static class `DimenPerimeter` offers the same lanes:

```csharp
using AppDimens.Net.Core;
using AppDimens.Net.Testing;
using AppDimens.Net.Code.Perimeter;

IAppDimensContext ctx = new FakeAppDimensContext();   // or your window scope

float a = 16.PRSdp(ctx);
float b = DimenPerimeter.PRSdp(ctx, 16);       // static form
float c = DimenPerimeter.PRWdpia(ctx, 120);

DimenPerimeter.WarmupCache(ctx);               // optional: pre-computes the factors for this window
```

## Full control: `PRSdpCustom`

```csharp
using AppDimens.Net.Common;
using AppDimens.Net.Code.Perimeter;

float custom = 16.PRSdpCustom(
    qualifier: DpQualifier.SmallWidth,  // axis: SmallWidth, Height or Width
    inverter: Inverter.SwToLh,          // swap axis by orientation (see core docs)
    imw: false,                         // ignore multi-window
    ar: true,                           // aspect-ratio correction
    k: 0.8f);                           // custom sensitivity (null = library default)

float customH = 48.PRHdpCustom(DpQualifier.Height, Inverter.Default, imw: false, ar: false);
float customW = 120.PRWdpCustom(DpQualifier.Width, Inverter.Default, imw: true, ar: false);
```

## Different value in landscape: `RotateRaw`

Returns the scaled base value, or the **raw** alternate value when the screen is in the given orientation:

```csharp
using AppDimens.Net;
using AppDimens.Net.Common;
using AppDimens.Net.Code.Perimeter;

var ctx = AppDimensAmbient.Require();

float columnWidth = 120.PRSdpRotateRaw(ctx, rawAlternate: 180f);                     // landscape → 180
float barHeight   = 56.PRHdpRotateRaw(ctx, rawAlternate: 48f, Orientation.Portrait);  // portrait  → 48
```

## Generic kernel: `ToPerimeterDp` / `ToPerimeterPx`

One method with named options — useful when the axis or flags come from data:

```csharp
using AppDimens.Net;
using AppDimens.Net.Common;
using AppDimens.Net.Code.Perimeter;

var ctx = AppDimensAmbient.Require();

float dp = 24.ToPerimeterDp(ctx, qualifier: DpQualifier.Width, ignoreMultiWindows: true);
float px = 24.ToPerimeterPx(ctx, qualifier: DpQualifier.Height, applyAspectRatio: true);
float fromFloat = 12.5f.ToPerimeterDp(ctx);
```

## Using it in each UI framework

### .NET MAUI

```csharp
using AppDimens.Net.Code.Perimeter;

card.Padding         = new Thickness(16.PRSdp());
titleLabel.FontSize  = 20.PRSdp();
banner.HeightRequest = 120.PRHdp();
```

### WPF / WinUI 3

```csharp
using AppDimens.Net.Code.Perimeter;

card.Padding  = new Thickness(16.PRSdp());
banner.Height = 120.PRHdp();
// re-apply after resizes:  AppDimensWpf.Changed += (_, _) => banner.Height = 120.PRHdp();
```

### Windows Forms

```csharp
using AppDimens.Net.WinForms;
using AppDimens.Net.Code.Perimeter;

AppDimensWinForms.Apply(this, () =>
{
    panel1.Padding = DimenPx.Padding(16.PRSdp());
    banner.Height  = DimenPx.ToPx(120.PRHdp());
});
```

### XAML

There is no dedicated markup extension for this strategy: set the values from C# (as above) and re-apply
them in the framework's change event (`AppDimensWpf.Changed`, `AppDimensWinUI.Changed`,
`AppDimensWinForms.Apply`, or MAUI `SizeChanged`).

## Testing

```csharp
using AppDimens.Net.Testing;
using AppDimens.Net.Code.Perimeter;

var ctx = new FakeAppDimensContext();
ctx.SetConfig(360, 800);                // phone
float phone = 16.PRSdp(ctx);
ctx.SetConfig(800, 1280);               // tablet
ctx.NotifyChange();                     // fire watchers like a real resize
float tablet = 16.PRSdp(ctx);
```

## Performance

Same engine as the core: snapshot-partitioned cache, allocation-free fast lanes for the parameterless calls, and
automatic invalidation when the window changes. Custom sensitivity (`k`) values bypass the cache by design.

---

## Português (resumo)

* **O que é:** Perimeter scaling — balances width and height in a single number.
* **Instalar:** `dotnet add package AppDimens.Net.Perimeter` (ou `AppDimens.Net.Sdk` com todas as estratégias).
* **Inicializar uma vez** (MAUI `AppDimensMaui.AttachWindow`, WPF `AppDimensWpf.Initialize`, WinForms
  `AppDimensWinForms.Initialize`, WinUI `AppDimensWinUI.Initialize`).
* **Usar:** `using AppDimens.Net.Code.Perimeter;` → `16.PRSdp()` (tamanhos), `56.PRHdp()` (altura), `200.PRWdp()` (largura);
  sufixos `a` (proporção da tela), `i` (ignora multi-janela) e `Px` (pixels).
* **Referência completa:** [API.md](API.md).

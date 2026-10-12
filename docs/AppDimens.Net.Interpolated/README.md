# AppDimens.Net.Interpolated

**Interpolated scaling — predictable linear steps between a phone anchor and a tablet anchor.**

Part of **AppDimens for .NET** — a bit-for-bit C# port of [appdimens-kmp](https://github.com/bodenberg/appdimens-kmp).
Requires the core package [`AppDimens.Net`](../AppDimens.Net/README.md) (installed automatically).

| | |
|---|---|
| **NuGet** | [`AppDimens.Net.Interpolated`](https://www.nuget.org/packages/AppDimens.Net.Interpolated) |
| **Namespace** | `AppDimens.Net.Code.Interpolated` |
| **Method prefix** | `I` → `ISdp`, `IHdp`, `IWdp` |
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
8. [Full control: `ISdpCustom`](#full-control-isdpcustom)
9. [Different value in landscape: `RotateRaw`](#different-value-in-landscape-rotateraw)
10. [Generic kernel: `ToInterpolatedDp` / `ToInterpolatedPx`](#generic-kernel-tointerpolateddp--tointerpolatedpx)
11. [Using it in each UI framework](#using-it-in-each-ui-framework)
13. [Testing](#testing)
14. [Performance](#performance)
15. [Português (resumo)](#português-resumo)

---

## What it does

The **Interpolated** strategy maps the screen onto a straight line between a small-screen and a large-screen anchor: compact on phones, roomier on tablets, with linear (easy to predict) steps in between — handy when a designer specifies a "phone value" and a "tablet value".

| Scenario | Recommendation |
|----------|----------------|
| Designer-specified phone / tablet values | ✅ Interpolated |
| Gentle curves on very large screens | Logarithmic / Fluid |
| Breakpoint-exact behaviour | Fit builder |

## What values to expect

`16` scaled with this strategy (density 1.0, default font scale), compared with the core **Scaled** strategy:

| Call | Phone 360×800 | Tablet 800×1280 | Desktop 1280×800 |
|------|--------------:|----------------:|-----------------:|
| `16.ISdp()` | 17.6 | 29.3 | 29.3 |
| `16.IHdp()` | 17.6 | 29.3 | 29.3 |
| `16.IWdp()` | 17.6 | 29.3 | 29.3 |
| `16.Sdp()` (Scaled, for comparison) | 19.2 | 42.7 | 42.7 |

## Install

```bash
dotnet add package AppDimens.Net.Interpolated
```

or install everything at once with [`AppDimens.Net.Sdk`](../AppDimens.Net.Sdk/README.md).

## Setup (once per app)

Parameterless calls such as `16.ISdp()` use the **ambient window context**. Create it once, before building UI:

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
using AppDimens.Net.Code.Interpolated;

float padding = 16.ISdp();    // follows the smaller side of the window (default choice)
float header  = 56.IHdp();    // follows the window height
float sidebar = 200.IWdp();   // follows the window width
```

All calls return `float` **dp** (device-independent units): use them directly for MAUI `WidthRequest`, WPF/WinUI
`Width`, `Margin`, `FontSize`…

## All variants: `a`, `i`, `ia`, `Px`

| Suffix | Meaning | Example |
|--------|---------|---------|
| *(none)* | default curve | `16.ISdp()` |
| `a` | adds the **aspect-ratio** correction (very tall/wide screens) | `16.ISdpa()` |
| `i` | **ignores multi-window / split-screen** shrinking | `16.ISdpi()` |
| `ia` | both | `16.ISdpia()` |
| `Px` | same value in **physical pixels** (needs a context) | `16.ISdpPx(ctx)` |

Every axis has every suffix: `ISdp`, `ISdpa`, `ISdpi`, `ISdpia`, `IHdp`, `IHdpa`, `IHdpi`, `IHdpia`,
`IWdp`, `IWdpa`, `IWdpi`, `IWdpia`, and the `Px` forms `ISdpPx`, `ISdpaPx`, `ISdpiPx`, `ISdpiaPx` (same for `H`/`W`).

```csharp
using AppDimens.Net;
using AppDimens.Net.Code.Interpolated;

var ctx = AppDimensAmbient.Require();

float tall       = 16.ISdpa();
float splitSafe  = 16.ISdpi();
float both       = 16.ISdpia();
float pixels     = 16.ISdpPx(ctx);       // e.g. for SkiaSharp / Android canvas / WinForms
float heightPx   = 48.IHdpaPx(ctx);
```

## Explicit context (no ambient window)

Every lane accepts an `IAppDimensContext`, and the static class `DimenInterpolated` offers the same lanes:

```csharp
using AppDimens.Net.Core;
using AppDimens.Net.Testing;
using AppDimens.Net.Code.Interpolated;

IAppDimensContext ctx = new FakeAppDimensContext();   // or your window scope

float a = 16.ISdp(ctx);
float b = DimenInterpolated.ISdp(ctx, 16);       // static form
float c = DimenInterpolated.IWdpia(ctx, 120);

DimenInterpolated.WarmupCache(ctx);               // optional: pre-computes the factors for this window
```

## Full control: `ISdpCustom`

```csharp
using AppDimens.Net.Common;
using AppDimens.Net.Code.Interpolated;

float custom = 16.ISdpCustom(
    qualifier: DpQualifier.SmallWidth,  // axis: SmallWidth, Height or Width
    inverter: Inverter.SwToLh,          // swap axis by orientation (see core docs)
    imw: false,                         // ignore multi-window
    ar: true,                           // aspect-ratio correction
    k: 0.8f);                           // custom sensitivity (null = library default)

float customH = 48.IHdpCustom(DpQualifier.Height, Inverter.Default, imw: false, ar: false);
float customW = 120.IWdpCustom(DpQualifier.Width, Inverter.Default, imw: true, ar: false);
```

## Different value in landscape: `RotateRaw`

Returns the scaled base value, or the **raw** alternate value when the screen is in the given orientation:

```csharp
using AppDimens.Net;
using AppDimens.Net.Common;
using AppDimens.Net.Code.Interpolated;

var ctx = AppDimensAmbient.Require();

float columnWidth = 120.ISdpRotateRaw(ctx, rawAlternate: 180f);                     // landscape → 180
float barHeight   = 56.IHdpRotateRaw(ctx, rawAlternate: 48f, Orientation.Portrait);  // portrait  → 48
```

## Generic kernel: `ToInterpolatedDp` / `ToInterpolatedPx`

One method with named options — useful when the axis or flags come from data:

```csharp
using AppDimens.Net;
using AppDimens.Net.Common;
using AppDimens.Net.Code.Interpolated;

var ctx = AppDimensAmbient.Require();

float dp = 24.ToInterpolatedDp(ctx, qualifier: DpQualifier.Width, ignoreMultiWindows: true);
float px = 24.ToInterpolatedPx(ctx, qualifier: DpQualifier.Height, applyAspectRatio: true);
float fromFloat = 12.5f.ToInterpolatedDp(ctx);
```

## Using it in each UI framework

### .NET MAUI

```csharp
using AppDimens.Net.Code.Interpolated;

card.Padding         = new Thickness(16.ISdp());
titleLabel.FontSize  = 20.ISdp();
banner.HeightRequest = 120.IHdp();
```

### WPF / WinUI 3

```csharp
using AppDimens.Net.Code.Interpolated;

card.Padding  = new Thickness(16.ISdp());
banner.Height = 120.IHdp();
// re-apply after resizes:  AppDimensWpf.Changed += (_, _) => banner.Height = 120.IHdp();
```

### Windows Forms

```csharp
using AppDimens.Net.WinForms;
using AppDimens.Net.Code.Interpolated;

AppDimensWinForms.Apply(this, () =>
{
    panel1.Padding = DimenPx.Padding(16.ISdp());
    banner.Height  = DimenPx.ToPx(120.IHdp());
});
```

### XAML

There is no dedicated markup extension for this strategy: set the values from C# (as above) and re-apply
them in the framework's change event (`AppDimensWpf.Changed`, `AppDimensWinUI.Changed`,
`AppDimensWinForms.Apply`, or MAUI `SizeChanged`).

## Testing

```csharp
using AppDimens.Net.Testing;
using AppDimens.Net.Code.Interpolated;

var ctx = new FakeAppDimensContext();
ctx.SetConfig(360, 800);                // phone
float phone = 16.ISdp(ctx);
ctx.SetConfig(800, 1280);               // tablet
ctx.NotifyChange();                     // fire watchers like a real resize
float tablet = 16.ISdp(ctx);
```

## Performance

Same engine as the core: snapshot-partitioned cache, allocation-free fast lanes for the parameterless calls, and
automatic invalidation when the window changes. Custom sensitivity (`k`) values bypass the cache by design.

---

## Português (resumo)

* **O que é:** Interpolated scaling — predictable linear steps between a phone anchor and a tablet anchor.
* **Instalar:** `dotnet add package AppDimens.Net.Interpolated` (ou `AppDimens.Net.Sdk` com todas as estratégias).
* **Inicializar uma vez** (MAUI `AppDimensMaui.AttachWindow`, WPF `AppDimensWpf.Initialize`, WinForms
  `AppDimensWinForms.Initialize`, WinUI `AppDimensWinUI.Initialize`).
* **Usar:** `using AppDimens.Net.Code.Interpolated;` → `16.ISdp()` (tamanhos), `56.IHdp()` (altura), `200.IWdp()` (largura);
  sufixos `a` (proporção da tela), `i` (ignora multi-janela) e `Px` (pixels).
* **Referência completa:** [API.md](API.md).

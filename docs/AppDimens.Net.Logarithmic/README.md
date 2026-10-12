# AppDimens.Net.Logarithmic

**Logarithmic scaling — fast growth on small screens, gentle growth on big ones.**

Part of **AppDimens for .NET** — a bit-for-bit C# port of [appdimens-kmp](https://github.com/bodenberg/appdimens-kmp).
Requires the core package [`AppDimens.Net`](../AppDimens.Net/README.md) (installed automatically).

| | |
|---|---|
| **NuGet** | [`AppDimens.Net.Logarithmic`](https://www.nuget.org/packages/AppDimens.Net.Logarithmic) |
| **Namespace** | `AppDimens.Net.Code.Logarithmic` |
| **Method prefix** | `LOG` → `LOGSdp`, `LOGHdp`, `LOGWdp` |
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
8. [Full control: `LOGSdpCustom`](#full-control-logsdpcustom)
9. [Different value in landscape: `RotateRaw`](#different-value-in-landscape-rotateraw)
10. [Generic kernel: `ToLogarithmicDp` / `ToLogarithmicPx`](#generic-kernel-tologarithmicdp--tologarithmicpx)
11. [Using it in each UI framework](#using-it-in-each-ui-framework)
13. [Testing](#testing)
14. [Performance](#performance)
15. [Português (resumo)](#português-resumo)

---

## What it does

The **Logarithmic** strategy grows along a log curve: small phones get real relief, while tablets and desktops grow only a little — paddings and fonts don't explode on huge screens.

| Scenario | Recommendation |
|----------|----------------|
| Comfortable scaling from phone to desktop | ✅ Logarithmic |
| Big screens must get much more space | Power |
| Strict proportions | Scaled (core) / Percent |

## What values to expect

`16` scaled with this strategy (density 1.0, default font scale), compared with the core **Scaled** strategy:

| Call | Phone 360×800 | Tablet 800×1280 | Desktop 1280×800 |
|------|--------------:|----------------:|-----------------:|
| `16.LOGSdp()` | 17.2 | 22.3 | 22.3 |
| `16.LOGHdp()` | 17.2 | 22.3 | 22.3 |
| `16.LOGWdp()` | 17.2 | 22.3 | 22.3 |
| `16.Sdp()` (Scaled, for comparison) | 19.2 | 42.7 | 42.7 |

## Install

```bash
dotnet add package AppDimens.Net.Logarithmic
```

or install everything at once with [`AppDimens.Net.Sdk`](../AppDimens.Net.Sdk/README.md).

## Setup (once per app)

Parameterless calls such as `16.LOGSdp()` use the **ambient window context**. Create it once, before building UI:

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
using AppDimens.Net.Code.Logarithmic;

float padding = 16.LOGSdp();    // follows the smaller side of the window (default choice)
float header  = 56.LOGHdp();    // follows the window height
float sidebar = 200.LOGWdp();   // follows the window width
```

All calls return `float` **dp** (device-independent units): use them directly for MAUI `WidthRequest`, WPF/WinUI
`Width`, `Margin`, `FontSize`…

## All variants: `a`, `i`, `ia`, `Px`

| Suffix | Meaning | Example |
|--------|---------|---------|
| *(none)* | default curve | `16.LOGSdp()` |
| `a` | adds the **aspect-ratio** correction (very tall/wide screens) | `16.LOGSdpa()` |
| `i` | **ignores multi-window / split-screen** shrinking | `16.LOGSdpi()` |
| `ia` | both | `16.LOGSdpia()` |
| `Px` | same value in **physical pixels** (needs a context) | `16.LOGSdpPx(ctx)` |

Every axis has every suffix: `LOGSdp`, `LOGSdpa`, `LOGSdpi`, `LOGSdpia`, `LOGHdp`, `LOGHdpa`, `LOGHdpi`, `LOGHdpia`,
`LOGWdp`, `LOGWdpa`, `LOGWdpi`, `LOGWdpia`, and the `Px` forms `LOGSdpPx`, `LOGSdpaPx`, `LOGSdpiPx`, `LOGSdpiaPx` (same for `H`/`W`).

```csharp
using AppDimens.Net;
using AppDimens.Net.Code.Logarithmic;

var ctx = AppDimensAmbient.Require();

float tall       = 16.LOGSdpa();
float splitSafe  = 16.LOGSdpi();
float both       = 16.LOGSdpia();
float pixels     = 16.LOGSdpPx(ctx);       // e.g. for SkiaSharp / Android canvas / WinForms
float heightPx   = 48.LOGHdpaPx(ctx);
```

## Explicit context (no ambient window)

Every lane accepts an `IAppDimensContext`, and the static class `DimenLogarithmic` offers the same lanes:

```csharp
using AppDimens.Net.Core;
using AppDimens.Net.Testing;
using AppDimens.Net.Code.Logarithmic;

IAppDimensContext ctx = new FakeAppDimensContext();   // or your window scope

float a = 16.LOGSdp(ctx);
float b = DimenLogarithmic.LOGSdp(ctx, 16);       // static form
float c = DimenLogarithmic.LOGWdpia(ctx, 120);

DimenLogarithmic.WarmupCache(ctx);               // optional: pre-computes the factors for this window
```

## Full control: `LOGSdpCustom`

```csharp
using AppDimens.Net.Common;
using AppDimens.Net.Code.Logarithmic;

float custom = 16.LOGSdpCustom(
    qualifier: DpQualifier.SmallWidth,  // axis: SmallWidth, Height or Width
    inverter: Inverter.SwToLh,          // swap axis by orientation (see core docs)
    imw: false,                         // ignore multi-window
    ar: true,                           // aspect-ratio correction
    k: 0.8f);                           // custom sensitivity (null = library default)

float customH = 48.LOGHdpCustom(DpQualifier.Height, Inverter.Default, imw: false, ar: false);
float customW = 120.LOGWdpCustom(DpQualifier.Width, Inverter.Default, imw: true, ar: false);
```

## Different value in landscape: `RotateRaw`

Returns the scaled base value, or the **raw** alternate value when the screen is in the given orientation:

```csharp
using AppDimens.Net;
using AppDimens.Net.Common;
using AppDimens.Net.Code.Logarithmic;

var ctx = AppDimensAmbient.Require();

float columnWidth = 120.LOGSdpRotateRaw(ctx, rawAlternate: 180f);                     // landscape → 180
float barHeight   = 56.LOGHdpRotateRaw(ctx, rawAlternate: 48f, Orientation.Portrait);  // portrait  → 48
```

## Generic kernel: `ToLogarithmicDp` / `ToLogarithmicPx`

One method with named options — useful when the axis or flags come from data:

```csharp
using AppDimens.Net;
using AppDimens.Net.Common;
using AppDimens.Net.Code.Logarithmic;

var ctx = AppDimensAmbient.Require();

float dp = 24.ToLogarithmicDp(ctx, qualifier: DpQualifier.Width, ignoreMultiWindows: true);
float px = 24.ToLogarithmicPx(ctx, qualifier: DpQualifier.Height, applyAspectRatio: true);
float fromFloat = 12.5f.ToLogarithmicDp(ctx);
```

## Using it in each UI framework

### .NET MAUI

```csharp
using AppDimens.Net.Code.Logarithmic;

card.Padding         = new Thickness(16.LOGSdp());
titleLabel.FontSize  = 20.LOGSdp();
banner.HeightRequest = 120.LOGHdp();
```

### WPF / WinUI 3

```csharp
using AppDimens.Net.Code.Logarithmic;

card.Padding  = new Thickness(16.LOGSdp());
banner.Height = 120.LOGHdp();
// re-apply after resizes:  AppDimensWpf.Changed += (_, _) => banner.Height = 120.LOGHdp();
```

### Windows Forms

```csharp
using AppDimens.Net.WinForms;
using AppDimens.Net.Code.Logarithmic;

AppDimensWinForms.Apply(this, () =>
{
    panel1.Padding = DimenPx.Padding(16.LOGSdp());
    banner.Height  = DimenPx.ToPx(120.LOGHdp());
});
```

### XAML

There is no dedicated markup extension for this strategy: set the values from C# (as above) and re-apply
them in the framework's change event (`AppDimensWpf.Changed`, `AppDimensWinUI.Changed`,
`AppDimensWinForms.Apply`, or MAUI `SizeChanged`).

## Testing

```csharp
using AppDimens.Net.Testing;
using AppDimens.Net.Code.Logarithmic;

var ctx = new FakeAppDimensContext();
ctx.SetConfig(360, 800);                // phone
float phone = 16.LOGSdp(ctx);
ctx.SetConfig(800, 1280);               // tablet
ctx.NotifyChange();                     // fire watchers like a real resize
float tablet = 16.LOGSdp(ctx);
```

## Performance

Same engine as the core: snapshot-partitioned cache, allocation-free fast lanes for the parameterless calls, and
automatic invalidation when the window changes. Custom sensitivity (`k`) values bypass the cache by design.

---

## Português (resumo)

* **O que é:** Logarithmic scaling — fast growth on small screens, gentle growth on big ones.
* **Instalar:** `dotnet add package AppDimens.Net.Logarithmic` (ou `AppDimens.Net.Sdk` com todas as estratégias).
* **Inicializar uma vez** (MAUI `AppDimensMaui.AttachWindow`, WPF `AppDimensWpf.Initialize`, WinForms
  `AppDimensWinForms.Initialize`, WinUI `AppDimensWinUI.Initialize`).
* **Usar:** `using AppDimens.Net.Code.Logarithmic;` → `16.LOGSdp()` (tamanhos), `56.LOGHdp()` (altura), `200.LOGWdp()` (largura);
  sufixos `a` (proporção da tela), `i` (ignora multi-janela) e `Px` (pixels).
* **Referência completa:** [API.md](API.md).

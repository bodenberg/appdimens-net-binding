# AppDimens.Net.Fill

**Fill scaling — values that expand to occupy their share of the screen.**

Part of **AppDimens for .NET** — a bit-for-bit C# port of [appdimens-kmp](https://github.com/bodenberg/appdimens-kmp).
Requires the core package [`AppDimens.Net`](../AppDimens.Net/README.md) (installed automatically).

| | |
|---|---|
| **NuGet** | [`AppDimens.Net.Fill`](https://www.nuget.org/packages/AppDimens.Net.Fill) |
| **Namespace** | `AppDimens.Net.Code.Fill` |
| **Method prefix** | `FL` → `FLSdp`, `FLHdp`, `FLWdp` |
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
8. [Full control: `FLSdpCustom`](#full-control-flsdpcustom)
9. [Different value in landscape: `RotateRaw`](#different-value-in-landscape-rotateraw)
10. [Generic kernel: `ToFillDp` / `ToFillPx`](#generic-kernel-tofilldp--tofillpx)
11. [Using it in each UI framework](#using-it-in-each-ui-framework)
13. [Testing](#testing)
14. [Performance](#performance)
15. [Português (resumo)](#português-resumo)

---

## What it does

The **Fill** strategy scales toward filling the available space using the larger screen dimension, so it grows a bit faster than Scaled on phones. Use it for surfaces that should *occupy* space — backgrounds, banners, section headers. All axes give the same result.

| Scenario | Recommendation |
|----------|----------------|
| Space-occupying surfaces (banners, headers) | ✅ Fill |
| Literal percentages of the window | Percent (`SpaceWDp`) |
| Text that must shrink to fit a box | Resize |

## What values to expect

`16` scaled with this strategy (density 1.0, default font scale), compared with the core **Scaled** strategy:

| Call | Phone 360×800 | Tablet 800×1280 | Desktop 1280×800 |
|------|--------------:|----------------:|-----------------:|
| `16.FLSdp()` | 24 | 42.7 | 42.7 |
| `16.FLHdp()` | 24 | 42.7 | 42.7 |
| `16.FLWdp()` | 24 | 42.7 | 42.7 |
| `16.Sdp()` (Scaled, for comparison) | 19.2 | 42.7 | 42.7 |

## Install

```bash
dotnet add package AppDimens.Net.Fill
```

or install everything at once with [`AppDimens.Net.Sdk`](../AppDimens.Net.Sdk/README.md).

## Setup (once per app)

Parameterless calls such as `16.FLSdp()` use the **ambient window context**. Create it once, before building UI:

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
using AppDimens.Net.Code.Fill;

float padding = 16.FLSdp();    // follows the smaller side of the window (default choice)
float header  = 56.FLHdp();    // follows the window height
float sidebar = 200.FLWdp();   // follows the window width
```

All calls return `float` **dp** (device-independent units): use them directly for MAUI `WidthRequest`, WPF/WinUI
`Width`, `Margin`, `FontSize`…

## All variants: `a`, `i`, `ia`, `Px`

| Suffix | Meaning | Example |
|--------|---------|---------|
| *(none)* | default curve | `16.FLSdp()` |
| `a` | adds the **aspect-ratio** correction (very tall/wide screens) | `16.FLSdpa()` |
| `i` | **ignores multi-window / split-screen** shrinking | `16.FLSdpi()` |
| `ia` | both | `16.FLSdpia()` |
| `Px` | same value in **physical pixels** (needs a context) | `16.FLSdpPx(ctx)` |

Every axis has every suffix: `FLSdp`, `FLSdpa`, `FLSdpi`, `FLSdpia`, `FLHdp`, `FLHdpa`, `FLHdpi`, `FLHdpia`,
`FLWdp`, `FLWdpa`, `FLWdpi`, `FLWdpia`, and the `Px` forms `FLSdpPx`, `FLSdpaPx`, `FLSdpiPx`, `FLSdpiaPx` (same for `H`/`W`).

```csharp
using AppDimens.Net;
using AppDimens.Net.Code.Fill;

var ctx = AppDimensAmbient.Require();

float tall       = 16.FLSdpa();
float splitSafe  = 16.FLSdpi();
float both       = 16.FLSdpia();
float pixels     = 16.FLSdpPx(ctx);       // e.g. for SkiaSharp / Android canvas / WinForms
float heightPx   = 48.FLHdpaPx(ctx);
```

## Explicit context (no ambient window)

Every lane accepts an `IAppDimensContext`, and the static class `DimenFill` offers the same lanes:

```csharp
using AppDimens.Net.Core;
using AppDimens.Net.Testing;
using AppDimens.Net.Code.Fill;

IAppDimensContext ctx = new FakeAppDimensContext();   // or your window scope

float a = 16.FLSdp(ctx);
float b = DimenFill.FLSdp(ctx, 16);       // static form
float c = DimenFill.FLWdpia(ctx, 120);

DimenFill.WarmupCache(ctx);               // optional: pre-computes the factors for this window
```

## Full control: `FLSdpCustom`

```csharp
using AppDimens.Net.Common;
using AppDimens.Net.Code.Fill;

float custom = 16.FLSdpCustom(
    qualifier: DpQualifier.SmallWidth,  // axis: SmallWidth, Height or Width
    inverter: Inverter.SwToLh,          // swap axis by orientation (see core docs)
    imw: false,                         // ignore multi-window
    ar: true,                           // aspect-ratio correction
    k: 0.8f);                           // custom sensitivity (null = library default)

float customH = 48.FLHdpCustom(DpQualifier.Height, Inverter.Default, imw: false, ar: false);
float customW = 120.FLWdpCustom(DpQualifier.Width, Inverter.Default, imw: true, ar: false);
```

## Different value in landscape: `RotateRaw`

Returns the scaled base value, or the **raw** alternate value when the screen is in the given orientation:

```csharp
using AppDimens.Net;
using AppDimens.Net.Common;
using AppDimens.Net.Code.Fill;

var ctx = AppDimensAmbient.Require();

float columnWidth = 120.FLSdpRotateRaw(ctx, rawAlternate: 180f);                     // landscape → 180
float barHeight   = 56.FLHdpRotateRaw(ctx, rawAlternate: 48f, Orientation.Portrait);  // portrait  → 48
```

## Generic kernel: `ToFillDp` / `ToFillPx`

One method with named options — useful when the axis or flags come from data:

```csharp
using AppDimens.Net;
using AppDimens.Net.Common;
using AppDimens.Net.Code.Fill;

var ctx = AppDimensAmbient.Require();

float dp = 24.ToFillDp(ctx, qualifier: DpQualifier.Width, ignoreMultiWindows: true);
float px = 24.ToFillPx(ctx, qualifier: DpQualifier.Height, applyAspectRatio: true);
float fromFloat = 12.5f.ToFillDp(ctx);
```

## Using it in each UI framework

### .NET MAUI

```csharp
using AppDimens.Net.Code.Fill;

card.Padding         = new Thickness(16.FLSdp());
titleLabel.FontSize  = 20.FLSdp();
banner.HeightRequest = 120.FLHdp();
```

### WPF / WinUI 3

```csharp
using AppDimens.Net.Code.Fill;

card.Padding  = new Thickness(16.FLSdp());
banner.Height = 120.FLHdp();
// re-apply after resizes:  AppDimensWpf.Changed += (_, _) => banner.Height = 120.FLHdp();
```

### Windows Forms

```csharp
using AppDimens.Net.WinForms;
using AppDimens.Net.Code.Fill;

AppDimensWinForms.Apply(this, () =>
{
    panel1.Padding = DimenPx.Padding(16.FLSdp());
    banner.Height  = DimenPx.ToPx(120.FLHdp());
});
```

### XAML

There is no dedicated markup extension for this strategy: set the values from C# (as above) and re-apply
them in the framework's change event (`AppDimensWpf.Changed`, `AppDimensWinUI.Changed`,
`AppDimensWinForms.Apply`, or MAUI `SizeChanged`).

## Testing

```csharp
using AppDimens.Net.Testing;
using AppDimens.Net.Code.Fill;

var ctx = new FakeAppDimensContext();
ctx.SetConfig(360, 800);                // phone
float phone = 16.FLSdp(ctx);
ctx.SetConfig(800, 1280);               // tablet
ctx.NotifyChange();                     // fire watchers like a real resize
float tablet = 16.FLSdp(ctx);
```

## Performance

Same engine as the core: snapshot-partitioned cache, allocation-free fast lanes for the parameterless calls, and
automatic invalidation when the window changes. Custom sensitivity (`k`) values bypass the cache by design.

---

## Português (resumo)

* **O que é:** Fill scaling — values that expand to occupy their share of the screen.
* **Instalar:** `dotnet add package AppDimens.Net.Fill` (ou `AppDimens.Net.Sdk` com todas as estratégias).
* **Inicializar uma vez** (MAUI `AppDimensMaui.AttachWindow`, WPF `AppDimensWpf.Initialize`, WinForms
  `AppDimensWinForms.Initialize`, WinUI `AppDimensWinUI.Initialize`).
* **Usar:** `using AppDimens.Net.Code.Fill;` → `16.FLSdp()` (tamanhos), `56.FLHdp()` (altura), `200.FLWdp()` (largura);
  sufixos `a` (proporção da tela), `i` (ignora multi-janela) e `Px` (pixels).
* **Referência completa:** [API.md](API.md).

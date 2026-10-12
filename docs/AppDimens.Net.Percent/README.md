# AppDimens.Net.Percent

**Percent scaling — strictly proportional to an axis, plus Space helpers for literal percentages.**

Part of **AppDimens for .NET** — a bit-for-bit C# port of [appdimens-kmp](https://github.com/bodenberg/appdimens-kmp).
Requires the core package [`AppDimens.Net`](../AppDimens.Net/README.md) (installed automatically).

| | |
|---|---|
| **NuGet** | [`AppDimens.Net.Percent`](https://www.nuget.org/packages/AppDimens.Net.Percent) |
| **Namespace** | `AppDimens.Net.Code.Percent` |
| **Method prefix** | `P` → `PSdp`, `PHdp`, `PWdp` |
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
8. [Full control: `PSdpCustom`](#full-control-psdpcustom)
9. [Different value in landscape: `RotateRaw`](#different-value-in-landscape-rotateraw)
10. [Generic kernel: `ToPercentDp` / `ToPercentPx`](#generic-kernel-topercentdp--topercentpx)
11. [Using it in each UI framework](#using-it-in-each-ui-framework)
12. [Literal percentages — the Space helpers](#literal-percentages--the-space-helpers)
13. [Testing](#testing)
14. [Performance](#performance)
15. [Português (resumo)](#português-resumo)

---

## What it does

The **Percent** strategy scales **linearly** with the chosen axis (`value × axis ÷ 300`): `PSdp` follows the smaller side, `PHdp` the height and `PWdp` the width. It also ships the **Space helpers** for literal percentages of the window: `50.SpaceWDp(ctx)` = 50 % of the width, `50.SpaceHDp(ctx)` = 50 % of the height, `25.SpaceSwDp(ctx)` = 25 % of the smaller side.

| Scenario | Recommendation |
|----------|----------------|
| Cards / panels occupying an exact fraction of the window | ✅ Percent (`SpaceWDp`, `SpaceHDp`) |
| Moderate padding growth | Scaled (core) / Auto |
| Text auto-fit | Resize |

## What values to expect

`16` scaled with this strategy (density 1.0, default font scale), compared with the core **Scaled** strategy:

| Call | Phone 360×800 | Tablet 800×1280 | Desktop 1280×800 |
|------|--------------:|----------------:|-----------------:|
| `16.PSdp()` | 19.2 | 42.7 | 42.7 |
| `16.PHdp()` | 42.7 | 68.3 | 42.7 |
| `16.PWdp()` | 19.2 | 42.7 | 68.3 |
| `16.Sdp()` (Scaled, for comparison) | 19.2 | 42.7 | 42.7 |

## Install

```bash
dotnet add package AppDimens.Net.Percent
```

or install everything at once with [`AppDimens.Net.Sdk`](../AppDimens.Net.Sdk/README.md).

## Setup (once per app)

Parameterless calls such as `16.PSdp()` use the **ambient window context**. Create it once, before building UI:

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
using AppDimens.Net.Code.Percent;

float padding = 16.PSdp();    // follows the smaller side of the window (default choice)
float header  = 56.PHdp();    // follows the window height
float sidebar = 200.PWdp();   // follows the window width
```

All calls return `float` **dp** (device-independent units): use them directly for MAUI `WidthRequest`, WPF/WinUI
`Width`, `Margin`, `FontSize`…

## All variants: `a`, `i`, `ia`, `Px`

| Suffix | Meaning | Example |
|--------|---------|---------|
| *(none)* | default curve | `16.PSdp()` |
| `a` | adds the **aspect-ratio** correction (very tall/wide screens) | `16.PSdpa()` |
| `i` | **ignores multi-window / split-screen** shrinking | `16.PSdpi()` |
| `ia` | both | `16.PSdpia()` |
| `Px` | same value in **physical pixels** (needs a context) | `16.PSdpPx(ctx)` |

Every axis has every suffix: `PSdp`, `PSdpa`, `PSdpi`, `PSdpia`, `PHdp`, `PHdpa`, `PHdpi`, `PHdpia`,
`PWdp`, `PWdpa`, `PWdpi`, `PWdpia`, and the `Px` forms `PSdpPx`, `PSdpaPx`, `PSdpiPx`, `PSdpiaPx` (same for `H`/`W`).

```csharp
using AppDimens.Net;
using AppDimens.Net.Code.Percent;

var ctx = AppDimensAmbient.Require();

float tall       = 16.PSdpa();
float splitSafe  = 16.PSdpi();
float both       = 16.PSdpia();
float pixels     = 16.PSdpPx(ctx);       // e.g. for SkiaSharp / Android canvas / WinForms
float heightPx   = 48.PHdpaPx(ctx);
```

## Explicit context (no ambient window)

Every lane accepts an `IAppDimensContext`, and the static class `DimenPercent` offers the same lanes:

```csharp
using AppDimens.Net.Core;
using AppDimens.Net.Testing;
using AppDimens.Net.Code.Percent;

IAppDimensContext ctx = new FakeAppDimensContext();   // or your window scope

float a = 16.PSdp(ctx);
float b = DimenPercent.PSdp(ctx, 16);       // static form
float c = DimenPercent.PWdpia(ctx, 120);

DimenPercent.WarmupCache(ctx);               // optional: pre-computes the factors for this window
```

## Full control: `PSdpCustom`

```csharp
using AppDimens.Net.Common;
using AppDimens.Net.Code.Percent;

float custom = 16.PSdpCustom(
    qualifier: DpQualifier.SmallWidth,  // axis: SmallWidth, Height or Width
    inverter: Inverter.SwToLh,          // swap axis by orientation (see core docs)
    imw: false,                         // ignore multi-window
    ar: true,                           // aspect-ratio correction
    k: 0.8f);                           // custom sensitivity (null = library default)

float customH = 48.PHdpCustom(DpQualifier.Height, Inverter.Default, imw: false, ar: false);
float customW = 120.PWdpCustom(DpQualifier.Width, Inverter.Default, imw: true, ar: false);
```

## Different value in landscape: `RotateRaw`

Returns the scaled base value, or the **raw** alternate value when the screen is in the given orientation:

```csharp
using AppDimens.Net;
using AppDimens.Net.Common;
using AppDimens.Net.Code.Percent;

var ctx = AppDimensAmbient.Require();

float columnWidth = 120.PSdpRotateRaw(ctx, rawAlternate: 180f);                     // landscape → 180
float barHeight   = 56.PHdpRotateRaw(ctx, rawAlternate: 48f, Orientation.Portrait);  // portrait  → 48
```

## Generic kernel: `ToPercentDp` / `ToPercentPx`

One method with named options — useful when the axis or flags come from data:

```csharp
using AppDimens.Net;
using AppDimens.Net.Common;
using AppDimens.Net.Code.Percent;

var ctx = AppDimensAmbient.Require();

float dp = 24.ToPercentDp(ctx, qualifier: DpQualifier.Width, ignoreMultiWindows: true);
float px = 24.ToPercentPx(ctx, qualifier: DpQualifier.Height, applyAspectRatio: true);
float fromFloat = 12.5f.ToPercentDp(ctx);
```

## Using it in each UI framework

### .NET MAUI

```csharp
using AppDimens.Net.Code.Percent;

card.Padding         = new Thickness(16.PSdp());
titleLabel.FontSize  = 20.PSdp();
banner.HeightRequest = 120.PHdp();
```

### WPF / WinUI 3

```csharp
using AppDimens.Net.Code.Percent;

card.Padding  = new Thickness(16.PSdp());
banner.Height = 120.PHdp();
// re-apply after resizes:  AppDimensWpf.Changed += (_, _) => banner.Height = 120.PHdp();
```

### Windows Forms

```csharp
using AppDimens.Net.WinForms;
using AppDimens.Net.Code.Percent;

AppDimensWinForms.Apply(this, () =>
{
    panel1.Padding = DimenPx.Padding(16.PSdp());
    banner.Height  = DimenPx.ToPx(120.PHdp());
});
```

### XAML (WPF / WinUI 3 / MAUI)

`PSdp`, `PHdp`, `PWdp` and their `a` / `i` / `ia` variants have ready-made markup extensions:

```xml
<!-- WPF: AppDimens.Net.Wpf  (live, updates on resize) -->
<Border xmlns:ad="clr-namespace:AppDimens.Net.Wpf;assembly=AppDimens.Net.Wpf"
        Width="{ad:PWdp 200}" Height="{ad:PHdp 80}" Padding="{ad:PSdp 16}" />

<!-- WinUI 3: AppDimens.Net.WinUI  (re-applied on resize) -->
<Border xmlns:ad="using:AppDimens.Net.WinUI"
        Width="{ad:PWdp Value=200}" Height="{ad:PHdp Value=80}" Padding="{ad:PSdp Value=16}" />
```

Literal percentages are available as `{ad:SpaceW 50}` / `{ad:SpaceH 25}` (WPF) and `{ad:SpaceW Value=50}` (WinUI).
MAUI (`AppDimens.Net.Maui`) ships `{appdimens:Psdpx Value=50}` (50 % of the width).

## Literal percentages — the Space helpers

```csharp
using AppDimens.Net;
using AppDimens.Net.Code.Percent;

var ctx = AppDimensAmbient.Require();

float halfWidth   = 50.SpaceWDp(ctx);    // 50 % of the window width   (360×800 → 180)
float quarterH    = 25.SpaceHDp(ctx);    // 25 % of the window height  (360×800 → 200)
float tenthSmall  = 10.SpaceSwDp(ctx);   // 10 % of the smaller side   (360×800 → 36)
float ofReference = 10.SpaceDp(200f);    // 10 % of an arbitrary 200 dp → 20

// pixel versions (Android / WinForms): SpaceW, SpaceH, SpaceSw; frozen versions: SpaceWi, SpaceHi, SpaceSwi
float halfWidthPx = 50.SpaceW(ctx);
```

## Testing

```csharp
using AppDimens.Net.Testing;
using AppDimens.Net.Code.Percent;

var ctx = new FakeAppDimensContext();
ctx.SetConfig(360, 800);                // phone
float phone = 16.PSdp(ctx);
ctx.SetConfig(800, 1280);               // tablet
ctx.NotifyChange();                     // fire watchers like a real resize
float tablet = 16.PSdp(ctx);
```

## Performance

Same engine as the core: snapshot-partitioned cache, allocation-free fast lanes for the parameterless calls, and
automatic invalidation when the window changes. Custom sensitivity (`k`) values bypass the cache by design.

---

## Português (resumo)

* **O que é:** Percent scaling — strictly proportional to an axis, plus Space helpers for literal percentages.
* **Instalar:** `dotnet add package AppDimens.Net.Percent` (ou `AppDimens.Net.Sdk` com todas as estratégias).
* **Inicializar uma vez** (MAUI `AppDimensMaui.AttachWindow`, WPF `AppDimensWpf.Initialize`, WinForms
  `AppDimensWinForms.Initialize`, WinUI `AppDimensWinUI.Initialize`).
* **Usar:** `using AppDimens.Net.Code.Percent;` → `16.PSdp()` (tamanhos), `56.PHdp()` (altura), `200.PWdp()` (largura);
  sufixos `a` (proporção da tela), `i` (ignora multi-janela) e `Px` (pixels).
* **Referência completa:** [API.md](API.md).

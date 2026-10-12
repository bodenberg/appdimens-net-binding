# AppDimens.Net.Resize

**Auto-resize math — find the largest font size or square side that still fits a box, using bounded step
ranges and binary search instead of measure-and-retry layout loops.**

| | |
|---|---|
| **NuGet** | [`AppDimens.Net.Resize`](https://www.nuget.org/packages/AppDimens.Net.Resize) (requires `AppDimens.Net`) |
| **Namespace** | `AppDimens.Net.Code.Resize` |
| **Frameworks** | `net8.0`, `net9.0`, `net10.0` (+ `net11.0`) |
| **API reference** | [API.md](API.md) |

---

## Table of contents

1. [When to use](#when-to-use)
2. [Install](#install)
3. [Fit text in a box](#fit-text-in-a-box)
4. [Fit a square (icon, avatar) in a box](#fit-a-square-icon-avatar-in-a-box)
5. [Inner size after padding](#inner-size-after-padding)
6. [Your own "fits" test: `ResizeRangePx` and `ResizeMath`](#your-own-fits-test-resizerangepx-and-resizemath)
7. [Fixed values: `ResolveFixed`](#fixed-values-resolvefixed)
8. [Using the results per framework](#using-the-results-per-framework)
9. [Português (resumo)](#português-resumo)

---

## When to use

| Scenario | Recommendation |
|----------|----------------|
| Headline / badge / price that must fit a fixed box | ✅ Resize |
| Everything else that should simply scale | [AppDimens.Net](../AppDimens.Net/README.md) (Scaled) |

Box sizes are given in **pixels**; min / max / step are given in **sp** (text) or **dp** (squares), and the result is
returned **in that same unit** (the largest candidate of your range that fits).

> ⚠️ Despite the `Px` suffix (kept for parity with the Kotlin API), `AutoResizeTextPx` returns **sp** and
> `AutoResizeSquarePx` returns **dp**. The only exception: empty text returns `minSp × density`.

## Install

```bash
dotnet add package AppDimens.Net.Resize
```

## Fit text in a box

```csharp
using AppDimens.Net;
using AppDimens.Net.Code.Resize;

var ctx = AppDimensAmbient.Require();

float fontPx = DimenResize.AutoResizeTextPx(   // returns sp (see note above)
    text: "Black Friday — 70% OFF",
    boxWidthPx: 600f,          // available width in pixels
    minSp: 12f, maxSp: 32f,    // allowed range
    stepSp: 1f,                // granularity
    maxLines: 2,
    ctx: ctx);                 // optional: avgCharWidthFactor (default 0.55)

// fontSp is in sp (12…32): use it directly as FontSize in MAUI / WPF / WinUI,
// or multiply by ctx.Density for pixels (Android / WinForms / canvas).
float fontSp = fontPx;
float fontPxReal = fontSp * ctx.Density;
```

## Fit a square (icon, avatar) in a box

```csharp
using AppDimens.Net;
using AppDimens.Net.Code.Resize;

var ctx = AppDimensAmbient.Require();

float sideDp = DimenResize.AutoResizeSquarePx(   // returns dp (24…120)
    boxWidthPx: 300f, boxHeightPx: 180f,
    minDp: 24f, maxDp: 120f, stepDp: 4f,
    ctx: ctx);
float sidePixels = sideDp * ctx.Density;
```

## Inner size after padding

```csharp
using AppDimens.Net.Code.Resize;

var (innerW, innerH) = DimenResize.InnerMaxDimensionsPx(
    boxWidthPx: 400f, boxHeightPx: 200f,
    paddingLeftPx: 16f, paddingRightPx: 16f, paddingTopPx: 8f, paddingBottomPx: 8f);   // → 368 × 184

float factor = DimenResize.PercentOfBoxToFactor(80f);   // 80 % → 0.8
```

## Your own "fits" test: `ResizeRangePx` and `ResizeMath`

```csharp
using AppDimens.Net.Code.Resize;

// Steps 10, 12, 14 … 40 px (at most 4096 steps), binary-searched with your predicate
var range = new ResizeRangePx(minPx: 10f, maxPx: 40f, stepPx: 2f);
float best = range.ResolveFitting(sizePx => sizePx * 12 <= 300);   // largest size whose 12 chars fit 300 px

float[] steps = ResizeMath.BuildStepsPx(10f, 40f, 2f);
float same = ResizeMath.FindLargestFitting(steps, sizePx => sizePx * 12 <= 300);
```

## Fixed values: `ResolveFixed`

```csharp
using AppDimens.Net;
using AppDimens.Net.Code.Resize;

var ctx = AppDimensAmbient.Require();
float px   = DimenResize.ResolveFixed(16f, asSp: false, ctx);   // 16 dp → px (value × density)
float spPx = DimenResize.ResolveFixed(14f, asSp: true, ctx);    // value × density ÷ font scale
```

## Using the results per framework

| Framework | Text result (sp) | Square result (dp) |
|-----------|------------------|--------------------|
| MAUI / WPF / WinUI | use as `FontSize` directly | use as `WidthRequest` / `Width` directly |
| Windows Forms | font points: `sp * 0.75f` | pixels: `DimenPx.ToPx(dp)` |
| Android / SkiaSharp | pixels: `sp * ctx.Density` | pixels: `dp * ctx.Density` |

---

## Português (resumo)

`dotnet add package AppDimens.Net.Resize` → `using AppDimens.Net.Code.Resize;` →
`DimenResize.AutoResizeTextPx(texto, larguraPx, minSp, maxSp, passoSp, maxLinhas, ctx)` devolve o maior tamanho de
fonte que cabe, **em sp** (mesma unidade do intervalo); `AutoResizeSquarePx(...)` faz o mesmo para quadrados e devolve
**dp**. As caixas são informadas em pixels.

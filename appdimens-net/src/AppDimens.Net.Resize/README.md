<div align="center">

# 🔠 AppDimens.Net.Resize

**Auto-resize math — fit text and squares into boxes with bounded step search.**

Part of the **AppDimens for .NET** family · requires [AppDimens.Net](https://www.nuget.org/packages/AppDimens.Net/) (core).

[![NuGet](https://img.shields.io/nuget/v/AppDimens.Net.Resize.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/AppDimens.Net.Resize/)
[![License](https://img.shields.io/badge/license-Apache--2.0-blue.svg?style=flat-square)](https://github.com/bodenberg/appdimens-net-binding/blob/main/LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%209.0%20%7C%2010.0-8A2BE2.svg?style=flat-square)](https://dotnet.microsoft.com/)

</div>

---

## 🎯 What is it?

The **Resize module** solves the "shrink-to-fit" problem: given a box and content, find the
largest font size or square side that still fits — using pre-computed bounded step ranges
(≤ 4096 steps) and binary search instead of guess-and-check layout loops.

## 📦 Installation

```bash
dotnet add package AppDimens.Net.Resize
```

> Depends on [AppDimens.Net](https://www.nuget.org/packages/AppDimens.Net/) (pulled automatically).
> Attach a MAUI window first or pass an explicit `IAppDimensContext`.

## 🚀 Quick start

```csharp
using AppDimens.Net.Resize;

// Largest font (in px) that fits the headline into its box
float fontPx = DimenResize.AutoResizeTextPx(
    text: title, boxWidthPx: boxW,
    minSp: 14, maxSp: 48, stepSp: 1f,
    maxLines: 2);

// MAUI FontSize expects device-independent units:
label.FontSize = fontPx / ctx.Density;   // ÷ density · fontScale for sp semantics

// Largest square badge that fits a box (result already in dp)
float sideDp = DimenResize.AutoResizeSquarePx(boxWpx, boxHpx, minDp: 16, maxDp: 96, stepDp: 2f);
```

## 🧭 API

| Member | Purpose |
|---|---|
| `ResizeRangePx(minPx, maxPx, stepPx)` | bounded candidate range (≤ 4096 steps) |
| `ResizeMath.BuildStepsPx(...)` | sorted step array generation |
| `ResizeMath.FindLargestFitting(steps, fits)` | binary search for the largest passing step |
| `DimenResize.AutoResizeTextPx(text, boxWidthPx, minSp, maxSp, stepSp, maxLines, ctx)` | fitting font size (**px**) |
| `DimenResize.AutoResizeSquarePx(boxWpx, boxHpx, minDp, maxDp, stepDp, ctx)` | fitting square side (**dp**) |
| `DimenResize.InnerMaxDimensionsPx(...)` | usable inner box size |
| `DimenResize.PercentOfBoxToFactor(percent)` | % of box → scale factor |
| `DimenResize.ResolveFixed(valueDp, asSp, ctx)` | plain dp/sp resolution helper |

> ⚠️ `AutoResizeTextPx` returns **pixels**; divide by `density` (and `fontScale` for sp) before assigning to MAUI `FontSize`.

## 💡 When to use

| Scenario | Recommendation |
|---|---|
| Headlines/badges inside fixed boxes | ✅ Resize |
| Everything else scales responsively | core Scaled strategy |

## ⚡ Performance

Step arrays are cached per configuration snapshot; resize changes invalidate automatically
via the event watcher — no manual clearing.

## 📚 Full package family

[AppDimens.Net](https://www.nuget.org/packages/AppDimens.Net/) ·
[Percent](https://www.nuget.org/packages/AppDimens.Net.Percent/) ·
[Power](https://www.nuget.org/packages/AppDimens.Net.Power/) ·
[Auto](https://www.nuget.org/packages/AppDimens.Net.Auto/) ·
[Logarithmic](https://www.nuget.org/packages/AppDimens.Net.Logarithmic/) ·
[Fluid](https://www.nuget.org/packages/AppDimens.Net.Fluid/) ·
[Interpolated](https://www.nuget.org/packages/AppDimens.Net.Interpolated/) ·
[Diagonal](https://www.nuget.org/packages/AppDimens.Net.Diagonal/) ·
[Perimeter](https://www.nuget.org/packages/AppDimens.Net.Perimeter/) ·
[Fill](https://www.nuget.org/packages/AppDimens.Net.Fill/) ·
[Fit](https://www.nuget.org/packages/AppDimens.Net.Fit/) ·
[Density](https://www.nuget.org/packages/AppDimens.Net.Density/) ·
[Units](https://www.nuget.org/packages/AppDimens.Net.Units/) ·
[Maui](https://www.nuget.org/packages/AppDimens.Net.Maui/) ·
[Sdk meta-package](https://www.nuget.org/packages/AppDimens.Net.Sdk/)

---

Apache-2.0 — © Jean Bodenberg · [Repository](https://github.com/bodenberg/appdimens-net-binding) · Formulas are bit-a-bit ports of [appdimens-kmp](https://github.com/bodenberg/appdimens-kmp)

<div align="center">

# 🌊 AppDimens.Net.Fluid

**Fluid scaling (`FSdp`) — smooth interpolation between phone and tablet reference sizes.**

Part of the **AppDimens for .NET** family · requires [AppDimens.Net](https://www.nuget.org/packages/AppDimens.Net/) (core).

[![NuGet](https://img.shields.io/nuget/v/AppDimens.Net.Fluid.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/AppDimens.Net.Fluid/)
[![License](https://img.shields.io/badge/license-Apache--2.0-blue.svg?style=flat-square)](https://github.com/bodenberg/appdimens-net-binding/blob/main/LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%209.0%20%7C%2010.0-8A2BE2.svg?style=flat-square)](https://dotnet.microsoft.com/)

</div>

---

## 🎯 What is it?

The **Fluid strategy** interpolates your base value continuously between device classes,
avoiding jumps at breakpoints. Dimensions "flow" as the window resizes — great for
split-view apps, foldables mid-fold, and desktop windows being dragged around.

## 📦 Installation

```bash
dotnet add package AppDimens.Net.Fluid
```

> Depends on [AppDimens.Net](https://www.nuget.org/packages/AppDimens.Net/) (pulled automatically).
> Attach a MAUI window first or pass an explicit `IAppDimensContext`.

## 🚀 Quick start

```csharp
using AppDimens.Net.Fluid;

float card = 20.FSdp();        // fluid by smallest-width
float side = 240.FLHdp(ctx);   // height axis
float bar  = 8.FSdpaPx();      // px, aspect-ratio adjusted
```

## 🧭 API

| Extension | Result |
|---|---|
| `v.FSdp(ctx)` · `v.FSdpa(ctx)` | smallest-width lane (± aspect ratio) |
| `v.FHdp(ctx)` · `v.FWdp(ctx)` | height / width axis |
| `…i` variants (`FSdpi`…) | ignore multi-window |
| `v.FSdpPx(ctx)` … | pixel result |
| `v.FSdpCustom(qualifier, inverter, imw, ar, k)` | full control incl. sensitivity |
| `DimenFluid.FSdp(context, value)` | static form · `WarmupCache(ctx)` pre-heats |
| `v.ToFluidDp/Px(v, ctx, …)` | generic kernel entry |

## 💡 When to use

| Scenario | Recommendation |
|---|---|
| Foldables / resizable desktop windows | ✅ Fluid |
| Discrete breakpoint behavior | [Fit](https://www.nuget.org/packages/AppDimens.Net.Fit/) |
| Strict proportions | [Percent](https://www.nuget.org/packages/AppDimens.Net.Percent/) |

## ⚡ Performance

Same engine as the core: snapshot-partitioned cache, allocation-free fast lanes,
automatic invalidation on configuration changes.

## 📚 Full package family

[AppDimens.Net](https://www.nuget.org/packages/AppDimens.Net/) ·
[Percent](https://www.nuget.org/packages/AppDimens.Net.Percent/) ·
[Power](https://www.nuget.org/packages/AppDimens.Net.Power/) ·
[Auto](https://www.nuget.org/packages/AppDimens.Net.Auto/) ·
[Logarithmic](https://www.nuget.org/packages/AppDimens.Net.Logarithmic/) ·
[Interpolated](https://www.nuget.org/packages/AppDimens.Net.Interpolated/) ·
[Diagonal](https://www.nuget.org/packages/AppDimens.Net.Diagonal/) ·
[Perimeter](https://www.nuget.org/packages/AppDimens.Net.Perimeter/) ·
[Fill](https://www.nuget.org/packages/AppDimens.Net.Fill/) ·
[Fit](https://www.nuget.org/packages/AppDimens.Net.Fit/) ·
[Density](https://www.nuget.org/packages/AppDimens.Net.Density/) ·
[Resize](https://www.nuget.org/packages/AppDimens.Net.Resize/) ·
[Units](https://www.nuget.org/packages/AppDimens.Net.Units/) ·
[Maui](https://www.nuget.org/packages/AppDimens.Net.Maui/) ·
[Sdk meta-package](https://www.nuget.org/packages/AppDimens.Net.Sdk/)

---

Apache-2.0 — © Jean Bodenberg · [Repository](https://github.com/bodenberg/appdimens-net-binding) · Formulas are bit-a-bit ports of [appdimens-kmp](https://github.com/bodenberg/appdimens-kmp)

<div align="center">

# 🔲 AppDimens.Net.Perimeter

**Perimeter scaling (`PRSdp`) — dimensions proportional to the total screen perimeter.**

Part of the **AppDimens for .NET** family · requires [AppDimens.Net](https://www.nuget.org/packages/AppDimens.Net/) (core).

[![NuGet](https://img.shields.io/nuget/v/AppDimens.Net.Perimeter.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/AppDimens.Net.Perimeter/)
[![License](https://img.shields.io/badge/license-Apache--2.0-blue.svg?style=flat-square)](https://github.com/bodenberg/appdimens-net-binding/blob/main/LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%209.0%20%7C%2010.0-8A2BE2.svg?style=flat-square)](https://dotnet.microsoft.com/)

</div>

---

## 🎯 What is it?

The **Perimeter strategy** derives scale from the **sum of both screen axes**
(perimeter-like measure). It balances width *and* height contributions in one number,
giving stable results for edge-anchored UI: gutters, rails, borders, safe areas.

## 📦 Installation

```bash
dotnet add package AppDimens.Net.Perimeter
```

> Depends on [AppDimens.Net](https://www.nuget.org/packages/AppDimens.Net/) (pulled automatically).
> Attach a MAUI window first or pass an explicit `IAppDimensContext`.

## 🚀 Quick start

```csharp
using AppDimens.Net.Perimeter;

float rail   = 56.PRSdp();     // perimeter-scaled nav rail
float border = 12.PRHdp(ctx);  // height variant
float badge  = 20.PRSdpaPx();  // px, aspect-ratio adjusted
```

## 🧭 API

| Extension | Result |
|---|---|
| `v.PRSdp(ctx)` · `v.PRSdpa(ctx)` | perimeter lane (± aspect ratio) |
| `v.PRHdp(ctx)` · `v.PRWdp(ctx)` | height / width variants |
| `…i` variants (`PRSdpi`…) | ignore multi-window |
| `v.PRSdpPx(ctx)` … | pixel result |
| `v.PRSdpCustom(qualifier, inverter, imw, ar, k)` | full control incl. sensitivity |
| `DimenPerimeter.PRSdp(context, value)` | static form · `WarmupCache(ctx)` pre-heats |
| `v.ToPerimeterDp/Px(v, ctx, …)` | generic kernel entry |

## 💡 When to use

| Scenario | Recommendation |
|---|---|
| Edge/gutter systems | ✅ Perimeter |
| Orientation-invariant sizing | [Diagonal](https://www.nuget.org/packages/AppDimens.Net.Diagonal/) |
| Content-area fractions | [Percent](https://www.nuget.org/packages/AppDimens.Net.Percent/) |

## ⚡ Performance

Same engine as the core: snapshot-partitioned cache, allocation-free fast lanes,
automatic invalidation on configuration changes.

## 📚 Full package family

[AppDimens.Net](https://www.nuget.org/packages/AppDimens.Net/) ·
[Percent](https://www.nuget.org/packages/AppDimens.Net.Percent/) ·
[Power](https://www.nuget.org/packages/AppDimens.Net.Power/) ·
[Auto](https://www.nuget.org/packages/AppDimens.Net.Auto/) ·
[Logarithmic](https://www.nuget.org/packages/AppDimens.Net.Logarithmic/) ·
[Fluid](https://www.nuget.org/packages/AppDimens.Net.Fluid/) ·
[Interpolated](https://www.nuget.org/packages/AppDimens.Net.Interpolated/) ·
[Diagonal](https://www.nuget.org/packages/AppDimens.Net.Diagonal/) ·
[Fill](https://www.nuget.org/packages/AppDimens.Net.Fill/) ·
[Fit](https://www.nuget.org/packages/AppDimens.Net.Fit/) ·
[Density](https://www.nuget.org/packages/AppDimens.Net.Density/) ·
[Resize](https://www.nuget.org/packages/AppDimens.Net.Resize/) ·
[Units](https://www.nuget.org/packages/AppDimens.Net.Units/) ·
[Maui](https://www.nuget.org/packages/AppDimens.Net.Maui/) ·
[Sdk meta-package](https://www.nuget.org/packages/AppDimens.Net.Sdk/)

---

Apache-2.0 — © Jean Bodenberg · [Repository](https://github.com/bodenberg/appdimens-net-binding) · Formulas are bit-a-bit ports of [appdimens-kmp](https://github.com/bodenberg/appdimens-kmp)

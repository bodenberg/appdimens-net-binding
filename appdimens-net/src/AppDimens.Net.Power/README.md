<div align="center">

# ⚡ AppDimens.Net.Power

**Power-law scaling (`PWSdp`) — aggressive growth for large screens with tunable sensitivity `k`.**

Part of the **AppDimens for .NET** family · requires [AppDimens.Net](https://www.nuget.org/packages/AppDimens.Net/) (core).

[![NuGet](https://img.shields.io/nuget/v/AppDimens.Net.Power.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/AppDimens.Net.Power/)
[![License](https://img.shields.io/badge/license-Apache--2.0-blue.svg?style=flat-square)](https://github.com/bodenberg/appdimens-net-binding/blob/main/LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%209.0%20%7C%2010.0-8A2BE2.svg?style=flat-square)](https://dotnet.microsoft.com/)

</div>

---

## 🎯 What is it?

The **Power strategy** applies a **power-law curve** to screen-relative scaling: dimensions
grow faster than linearly as screens get bigger. Use it when large displays should get
*dramatically* more space — dashboards, media grids, map canvases.

A custom sensitivity factor (`k`) lets you flatten or steepen the curve without changing code paths.

## 📦 Installation

```bash
dotnet add package AppDimens.Net.Power
```

> Depends on [AppDimens.Net](https://www.nuget.org/packages/AppDimens.Net/) (pulled automatically).
> Attach a MAUI window first or pass an explicit `IAppDimensContext`.

## 🚀 Quick start

```csharp
using AppDimens.Net.Power;

float tile  = 12.PWSdp();      // power-scaled by smallest-width
float hero  = 18.PWSdpa();     // + aspect-ratio adjustment
float panel = 16.PWWdp(ctx);   // width-axis lane

// custom sensitivity — steeper growth on tablets
float bold = 12.PWSdpCustom(DpQualifier.SmallWidth, Inverter.Default,
                            ignoreMultiWindows: false, applyAspectRatio: false, k: 1.4f);
```

## 🧭 API

| Extension | Result |
|---|---|
| `v.PWSdp(ctx)` · `v.PWSdpa(ctx)` | smallest-width lane (± aspect ratio) |
| `v.PWHdp(ctx)` · `v.PWWdp(ctx)` | height / width axis |
| `…i` variants (`PWSdpi`…) | ignore multi-window |
| `v.PWSdpPx(ctx)` … | pixel result |
| `v.PWSdpCustom(qualifier, inverter, imw, ar, k)` | full control incl. sensitivity |
| `DimenPower.PWSdp(context, value)` | static form · `WarmupCache(ctx)` pre-heats |
| `v.ToPowerDp/Px(v, ctx, …)` | generic kernel entry |

## 💡 When to use

| Scenario | Recommendation |
|---|---|
| Large-screen emphasis (media, dashboards) | ✅ Power |
| Proportional-to-screen layouts | [Percent](https://www.nuget.org/packages/AppDimens.Net.Percent/) |
| Conservative, near-linear scaling | core Scaled / [Logarithmic](https://www.nuget.org/packages/AppDimens.Net.Logarithmic/) |

## ⚡ Performance

Same engine as the core: snapshot-partitioned cache, allocation-free fast lanes,
automatic invalidation on configuration changes. Custom `k` values bypass caching by design.

## 📚 Full package family

[AppDimens.Net](https://www.nuget.org/packages/AppDimens.Net/) ·
[Percent](https://www.nuget.org/packages/AppDimens.Net.Percent/) ·
[Auto](https://www.nuget.org/packages/AppDimens.Net.Auto/) ·
[Logarithmic](https://www.nuget.org/packages/AppDimens.Net.Logarithmic/) ·
[Fluid](https://www.nuget.org/packages/AppDimens.Net.Fluid/) ·
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

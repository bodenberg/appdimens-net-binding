<div align="center">

# 🧮 AppDimens.Net.Density

**Density-aware scaling (`DSdp`) — physical-size fidelity across DPI buckets.**

Part of the **AppDimens for .NET** family · requires [AppDimens.Net](https://www.nuget.org/packages/AppDimens.Net/) (core).

[![NuGet](https://img.shields.io/nuget/v/AppDimens.Net.Density.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/AppDimens.Net.Density/)
[![License](https://img.shields.io/badge/license-Apache--2.0-blue.svg?style=flat-square)](https://github.com/bodenberg/appdimens-net-binding/blob/main/LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%209.0%20%7C%2010.0-8A2BE2.svg?style=flat-square)](https://dotnet.microsoft.com/)

</div>

---

## 🎯 What is it?

The **Density strategy** factors the device **pixel density** into dimension scaling,
keeping perceived physical size more consistent between low-DPI budget phones and
high-DPI flagships than pure dp math alone.

## 📦 Installation

```bash
dotnet add package AppDimens.Net.Density
```

> Depends on [AppDimens.Net](https://www.nuget.org/packages/AppDimens.Net/) (pulled automatically).
> Attach a MAUI window first or pass an explicit `IAppDimensContext`.

## 🚀 Quick start

```csharp
using AppDimens.Net.Density;

float thumb = 72.DSdp();      // density-aware thumbnail
float row   = 56.DHdp(ctx);   // height axis
float dot   = 8.DSdpaPx();    // px, aspect-ratio adjusted
```

## 🧭 API

| Extension | Result |
|---|---|
| `v.DSdp(ctx)` · `v.DSdpa(ctx)` | smallest-width lane (± aspect ratio) |
| `v.DHdp(ctx)` · `v.DWdp(ctx)` | height / width axis |
| `…i` variants (`DSdpi`…) | ignore multi-window |
| `v.DSdpPx(ctx)` … | pixel result |
| `v.DSdpCustom(qualifier, inverter, imw, ar, k)` | full control incl. sensitivity |
| `DimenDensity.DSdp(context, value)` | static form · `WarmupCache(ctx)` pre-heats |
| `v.ToDensityDp/Px(v, ctx, …)` | generic kernel entry |

## 💡 When to use

| Scenario | Recommendation |
|---|---|
| Physical-size-critical elements | ✅ Density |
| Pure layout proportions | [Percent](https://www.nuget.org/packages/AppDimens.Net.Percent/) |
| Real-world unit input (mm/inch) | [Units](https://www.nuget.org/packages/AppDimens.Net.Units/) |

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
[Perimeter](https://www.nuget.org/packages/AppDimens.Net.Perimeter/) ·
[Fill](https://www.nuget.org/packages/AppDimens.Net.Fill/) ·
[Fit](https://www.nuget.org/packages/AppDimens.Net.Fit/) ·
[Resize](https://www.nuget.org/packages/AppDimens.Net.Resize/) ·
[Units](https://www.nuget.org/packages/AppDimens.Net.Units/) ·
[Maui](https://www.nuget.org/packages/AppDimens.Net.Maui/) ·
[Sdk meta-package](https://www.nuget.org/packages/AppDimens.Net.Sdk/)

---

Apache-2.0 — © Jean Bodenberg · [Repository](https://github.com/bodenberg/appdimens-net-binding) · Formulas are bit-a-bit ports of [appdimens-kmp](https://github.com/bodenberg/appdimens-kmp)

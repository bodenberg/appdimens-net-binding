<div align="center">

# 📉 AppDimens.Net.Logarithmic

**Logarithmic scaling (`LOGSdp`) — diminishing-growth dimensions for extreme screen ranges.**

Part of the **AppDimens for .NET** family · requires [AppDimens.Net](https://www.nuget.org/packages/AppDimens.Net/) (core).

[![NuGet](https://img.shields.io/nuget/v/AppDimens.Net.Logarithmic.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/AppDimens.Net.Logarithmic/)
[![License](https://img.shields.io/badge/license-Apache--2.0-blue.svg?style=flat-square)](https://github.com/bodenberg/appdimens-net-binding/blob/main/LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%209.0%20%7C%2010.0-8A2BE2.svg?style=flat-square)](https://dotnet.microsoft.com/)

</div>

---

## 🎯 What is it?

The **Logarithmic strategy** grows values along a **log curve**: fast on small phones,
progressively gentler on big tablets and desktops. Perfect when small devices need real
relief but huge screens shouldn't explode paddings and font sizes.

## 📦 Installation

```bash
dotnet add package AppDimens.Net.Logarithmic
```

> Depends on [AppDimens.Net](https://www.nuget.org/packages/AppDimens.Net/) (pulled automatically).
> Attach a MAUI window first or pass an explicit `IAppDimensContext`.

## 🚀 Quick start

```csharp
using AppDimens.Net.Logarithmic;

float pad  = 16.LOGSdp();      // log-damped by smallest-width
float gap  = 12.LOGWdp(ctx);   // width axis
float icon = 24.LOGSdpaPx();   // px, aspect-ratio adjusted
```

## 🧭 API

| Extension | Result |
|---|---|
| `v.LOGSdp(ctx)` · `v.LOGSdpa(ctx)` | smallest-width lane (± aspect ratio) |
| `v.LOGHdp(ctx)` · `v.LOGWdp(ctx)` | height / width axis |
| `…i` variants (`LOGSdpi`…) | ignore multi-window |
| `v.LOGSdpPx(ctx)` … | pixel result |
| `v.LOGSdpCustom(qualifier, inverter, imw, ar, k)` | full control incl. sensitivity |
| `DimenLogarithmic.LOGSdp(context, value)` | static form · `WarmupCache(ctx)` pre-heats |
| `v.ToLogarithmicDp/Px(v, ctx, …)` | generic kernel entry |

## 💡 When to use

| Scenario | Recommendation |
|---|---|
| Comfortable scaling from phone → desktop | ✅ Logarithmic |
| Aggressive large-screen growth | [Power](https://www.nuget.org/packages/AppDimens.Net.Power/) |
| Strict proportions | [Percent](https://www.nuget.org/packages/AppDimens.Net.Percent/) |

## ⚡ Performance

Same engine as the core: snapshot-partitioned cache, allocation-free fast lanes,
automatic invalidation on configuration changes.

## 📚 Full package family

[AppDimens.Net](https://www.nuget.org/packages/AppDimens.Net/) ·
[Percent](https://www.nuget.org/packages/AppDimens.Net.Percent/) ·
[Power](https://www.nuget.org/packages/AppDimens.Net.Power/) ·
[Auto](https://www.nuget.org/packages/AppDimens.Net.Auto/) ·
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

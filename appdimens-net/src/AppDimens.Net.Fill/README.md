<div align="center">

# 🧲 AppDimens.Net.Fill

**Fill-proportional scaling (`FLSdp`) — dimensions that expand to fill available space.**

Part of the **AppDimens for .NET** family · requires [AppDimens.Net](https://www.nuget.org/packages/AppDimens.Net/) (core).

[![NuGet](https://img.shields.io/nuget/v/AppDimens.Net.Fill.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/AppDimens.Net.Fill/)
[![License](https://img.shields.io/badge/license-Apache--2.0-blue.svg?style=flat-square)](https://github.com/bodenberg/appdimens-net-binding/blob/main/LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%209.0%20%7C%2010.0-8A2BE2.svg?style=flat-square)](https://dotnet.microsoft.com/)

</div>

---

## 🎯 What is it?

The **Fill strategy** scales values so content expands proportionally toward filling the
screen. Where Scaled answers "how big should this be?", Fill answers "how big should this be
to occupy its share of space?" — backgrounds, banners, section headers.

## 📦 Installation

```bash
dotnet add package AppDimens.Net.Fill
```

> Depends on [AppDimens.Net](https://www.nuget.org/packages/AppDimens.Net/) (pulled automatically).
> Attach a MAUI window first or pass an explicit `IAppDimensContext`.

## 🚀 Quick start

```csharp
using AppDimens.Net.Fill;

float banner = 60.FLSdp();     // fill-scaled banner height
float panel  = 120.FLHdp(ctx); // height axis
float strip  = 24.FLWdpaPx();  // px, aspect-ratio adjusted
```

## 🧭 API

| Extension | Result |
|---|---|
| `v.FLSdp(ctx)` · `v.FLSdpa(ctx)` | fill lane (± aspect ratio) |
| `v.FLHdp(ctx)` · `v.FLWdp(ctx)` | height / width axis variants |
| `…i` variants (`FLSdpi`…) | ignore multi-window |
| `v.FLSdpPx(ctx)` … | pixel result |
| `v.FLSdpCustom(qualifier, inverter, imw, ar, k)` | full control incl. sensitivity |
| `DimenFill.FLSdp(context, value)` | static form · `WarmupCache(ctx)` pre-heats |
| `v.ToFillDp/Px(v, ctx, …)` | generic kernel entry |

## 💡 When to use

| Scenario | Recommendation |
|---|---|
| Space-occupying surfaces | ✅ Fill |
| Relative fractions with % semantics | [Percent](https://www.nuget.org/packages/AppDimens.Net.Percent/) |
| Text that must shrink to fit boxes | [Resize](https://www.nuget.org/packages/AppDimens.Net.Resize/) |

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
[Fit](https://www.nuget.org/packages/AppDimens.Net.Fit/) ·
[Density](https://www.nuget.org/packages/AppDimens.Net.Density/) ·
[Resize](https://www.nuget.org/packages/AppDimens.Net.Resize/) ·
[Units](https://www.nuget.org/packages/AppDimens.Net.Units/) ·
[Maui](https://www.nuget.org/packages/AppDimens.Net.Maui/) ·
[Sdk meta-package](https://www.nuget.org/packages/AppDimens.Net.Sdk/)

---

Apache-2.0 — © Jean Bodenberg · [Repository](https://github.com/bodenberg/appdimens-net-binding) · Formulas are bit-a-bit ports of [appdimens-kmp](https://github.com/bodenberg/appdimens-kmp)

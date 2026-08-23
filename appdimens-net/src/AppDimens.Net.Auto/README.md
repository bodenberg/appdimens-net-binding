<div align="center">

# 🤖 AppDimens.Net.Auto

**Auto-balanced scaling (`ASdp`) — a single strategy that behaves well on every device class.**

Part of the **AppDimens for .NET** family · requires [AppDimens.Net](https://www.nuget.org/packages/AppDimens.Net/) (core).

[![NuGet](https://img.shields.io/nuget/v/AppDimens.Net.Auto.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/AppDimens.Net.Auto/)
[![License](https://img.shields.io/badge/license-Apache--2.0-blue.svg?style=flat-square)](https://github.com/bodenberg/appdimens-net-binding/blob/main/LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%209.0%20%7C%2010.0-8A2BE2.svg?style=flat-square)](https://dotnet.microsoft.com/)

</div>

---

## 🎯 What is it?

The **Auto strategy** blends screen-relative scaling with automatic compensation so that
values stay comfortable across phones, foldables and tablets *without* per-breakpoint tuning.
If you want "just scale it sensibly" with one extension method, this is it.

## 📦 Installation

```bash
dotnet add package AppDimens.Net.Auto
```

> Depends on [AppDimens.Net](https://www.nuget.org/packages/AppDimens.Net/) (pulled automatically).
> Attach a MAUI window first or pass an explicit `IAppDimensContext`.

## 🚀 Quick start

```csharp
using AppDimens.Net.Auto;

float pad   = 16.ASdp();      // auto-balanced by smallest-width
float row   = 48.AHdp();      // height axis
float gauge = 120.ASdpaPx();  // pixels, aspect-ratio adjusted
```

## 🧭 API

| Extension | Result |
|---|---|
| `v.ASdp(ctx)` · `v.ASdpa(ctx)` | smallest-width lane (± aspect ratio) |
| `v.AHdp(ctx)` · `v.AWdp(ctx)` | height / width axis |
| `…i` variants (`ASdpi`…) | ignore multi-window |
| `v.ASdpPx(ctx)` … | pixel result |
| `v.ASdpCustom(qualifier, inverter, imw, ar, k)` | full control incl. sensitivity |
| `DimenAuto.ASdp(context, value)` | static form · `WarmupCache(ctx)` pre-heats |
| `v.ToAutoDp/Px(v, ctx, …)` | generic kernel entry |

Rotation-aware helpers (`ASdpRotateRaw`, `AHdp`, `AWdp`…) follow the same conventions as the core module.

## 💡 When to use

| Scenario | Recommendation |
|---|---|
| Default choice when unsure | ✅ Auto |
| Exact proportional layouts | [Percent](https://www.nuget.org/packages/AppDimens.Net.Percent/) |
| Large-screen emphasis | [Power](https://www.nuget.org/packages/AppDimens.Net.Power/) |

## ⚡ Performance

Same engine as the core: snapshot-partitioned cache, allocation-free fast lanes,
automatic invalidation on configuration changes.

## 📚 Full package family

[AppDimens.Net](https://www.nuget.org/packages/AppDimens.Net/) ·
[Percent](https://www.nuget.org/packages/AppDimens.Net.Percent/) ·
[Power](https://www.nuget.org/packages/AppDimens.Net.Power/) ·
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

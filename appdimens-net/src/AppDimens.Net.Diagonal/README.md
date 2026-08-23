<div align="center">

# 📐 AppDimens.Net.Diagonal

**Diagonal scaling (`DGSdp`) — dimensions derived from the screen diagonal. Orientation-stable by design.**

Part of the **AppDimens for .NET** family · requires [AppDimens.Net](https://www.nuget.org/packages/AppDimens.Net/) (core).

[![NuGet](https://img.shields.io/nuget/v/AppDimens.Net.Diagonal.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/AppDimens.Net.Diagonal/)
[![License](https://img.shields.io/badge/license-Apache--2.0-blue.svg?style=flat-square)](https://github.com/bodenberg/appdimens-net-binding/blob/main/LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%209.0%20%7C%2010.0-8A2BE2.svg?style=flat-square)](https://dotnet.microsoft.com/)

</div>

---

## 🎯 What is it?

The **Diagonal strategy** computes scale from the screen's **diagonal** (√(w²+h²)),
exactly like physical screen sizes ("6.7-inch display"). Because rotation swaps w/h but
preserves the diagonal, values stay **stable between portrait and landscape** — ideal for
floating buttons, overlays and chrome that shouldn't jump on rotate.

## 📦 Installation

```bash
dotnet add package AppDimens.Net.Diagonal
```

> Depends on [AppDimens.Net](https://www.nuget.org/packages/AppDimens.Net/) (pulled automatically).
> Attach a MAUI window first or pass an explicit `IAppDimensContext`.

## 🚀 Quick start

```csharp
using AppDimens.Net.Diagonal;

float fab   = 56.DGSdp();      // rotation-stable FAB
float sheet = 320.DGWdp(ctx);  // width variant
float ring  = 40.DGSdpaPx();   // px, aspect-ratio adjusted
```

## 🧭 API

| Extension | Result |
|---|---|
| `v.DGSdp(ctx)` · `v.DGSdpa(ctx)` | diagonal lane (± aspect ratio) |
| `v.DGHdp(ctx)` · `v.DGWdp(ctx)` | height / width variants |
| `…i` variants (`DGSdpi`…) | ignore multi-window |
| `v.DGSdpPx(ctx)` … | pixel result |
| `v.DGSdpCustom(qualifier, inverter, imw, ar, k)` | full control incl. sensitivity |
| `DimenDiagonal.DGSdp(context, value)` | static form · `WarmupCache(ctx)` pre-heats |
| `v.ToDiagonalDp/Px(v, ctx, …)` | generic kernel entry |

## 💡 When to use

| Scenario | Recommendation |
|---|---|
| Rotation-invariant chrome | ✅ Diagonal |
| Edge/gutter systems | [Perimeter](https://www.nuget.org/packages/AppDimens.Net.Perimeter/) |
| Axis-specific behavior | core Scaled (`Sdp`/`Hdp`/`Wdp`) |

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

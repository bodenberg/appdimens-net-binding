<div align="center">

# 🎚️ AppDimens.Net.Interpolated

**Interpolated scaling (`ISdp`) — linear interpolation between small-screen and large-screen anchors.**

Part of the **AppDimens for .NET** family · requires [AppDimens.Net](https://www.nuget.org/packages/AppDimens.Net/) (core).

[![NuGet](https://img.shields.io/nuget/v/AppDimens.Net.Interpolated.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/AppDimens.Net.Interpolated/)
[![License](https://img.shields.io/badge/license-Apache--2.0-blue.svg?style=flat-square)](https://github.com/bodenberg/appdimens-net-binding/blob/main/LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%209.0%20%7C%2010.0-8A2BE2.svg?style=flat-square)](https://dotnet.microsoft.com/)

</div>

---

## 🎯 What is it?

The **Interpolated strategy** maps the current screen onto a straight line between two
anchor scales: compact on phones, roomy on tablets, with **predictable, linear** steps in
between. The easiest strategy to reason about when designers spec "phone value" and
"tablet value".

## 📦 Installation

```bash
dotnet add package AppDimens.Net.Interpolated
```

> Depends on [AppDimens.Net](https://www.nuget.org/packages/AppDimens.Net/) (pulled automatically).
> Attach a MAUI window first or pass an explicit `IAppDimensContext`.

## 🚀 Quick start

```csharp
using AppDimens.Net.Interpolated;

float chip  = 14.ISdp();       // interpolated by smallest-width
float list  = 48.IHdp(ctx);    // height axis
float pill  = 28.ISdpaPx();    // px, aspect-ratio adjusted
```

## 🧭 API

| Extension | Result |
|---|---|
| `v.ISdp(ctx)` · `v.ISdpa(ctx)` | smallest-width lane (± aspect ratio) |
| `v.IHdp(ctx)` · `v.IWdp(ctx)` | height / width axis |
| `…i` variants (`ISdpi`…) | ignore multi-window |
| `v.ISdpPx(ctx)` … | pixel result |
| `v.ISdpCustom(qualifier, inverter, imw, ar, k)` | full control incl. sensitivity |
| `DimenInterpolated.ISdp(context, value)` | static form · `WarmupCache(ctx)` pre-heats |
| `v.ToInterpolatedDp/Px(v, ctx, …)` | generic kernel entry |

## 💡 When to use

| Scenario | Recommendation |
|---|---|
| Designer-specified phone/tablet values | ✅ Interpolated |
| Non-linear comfort curves | [Fluid](https://www.nuget.org/packages/AppDimens.Net.Fluid/) / [Logarithmic](https://www.nuget.org/packages/AppDimens.Net.Logarithmic/) |
| Breakpoint-exact behavior | [Fit](https://www.nuget.org/packages/AppDimens.Net.Fit/) |

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

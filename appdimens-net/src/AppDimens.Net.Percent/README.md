<div align="center">

# 📏 AppDimens.Net.Percent

**Percentage-of-screen scaling (`PSdp`, `SpaceW`) — dimensions that grow exactly with the screen.**

Part of the **AppDimens for .NET** family · requires [AppDimens.Net](https://www.nuget.org/packages/AppDimens.Net/) (core).

[![NuGet](https://img.shields.io/nuget/v/AppDimens.Net.Percent.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/AppDimens.Net.Percent/)
[![License](https://img.shields.io/badge/license-Apache--2.0-blue.svg?style=flat-square)](https://github.com/bodenberg/appdimens-net-binding/blob/main/LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%209.0%20%7C%2010.0-8A2BE2.svg?style=flat-square)](https://dotnet.microsoft.com/)

</div>

---

## 🎯 What is it?

The **Percent strategy** treats your base value as a **percentage of a screen dimension**
(smallest-width, height or width). `24.PSdp()` renders as 24% of the smallest-width axis,
so proportions stay identical on a 5" phone and a 13" tablet — ideal for hero cards,
charts, split layouts and anything that must occupy a *relative* area.

Also ships the **Space helpers** (`SpaceW`, `SpaceH`, `SpaceSw`) for computing
free space and gaps directly.

## 📦 Installation

```bash
dotnet add package AppDimens.Net.Percent
```

> Depends on [AppDimens.Net](https://www.nuget.org/packages/AppDimens.Net/) (pulled automatically).
> Attach a MAUI window first (`AppDimensMaui.AttachWindow`) or pass an explicit
> `IAppDimensContext` — see the [core README](https://www.nuget.org/packages/AppDimens.Net/).

## 🚀 Quick start

```csharp
using AppDimens.Net.Percent;

// 30% of the smallest-width axis
float cardWidth = 30.PSdp();

// 25% of the height axis, aspect-ratio adjusted
float heroHeight = 25.PHdpa();

// free space between two fixed elements (in %)
float gap = 5.SpaceW(ctx);
```

## 🧭 API

### Strategy lanes (prefix `PS`, `PH`, `PW`)

| Extension | Result |
|---|---|
| `v.PSdp(ctx)` · `v.PSdpa(ctx)` | % of smallest-width (± aspect ratio) |
| `v.PHdp(ctx)` · `v.PWdp(ctx)` | % of height / width axis |
| `…i` variants (`PSdpi`…) | ignore multi-window constraints |
| `v.PSdpPx(ctx)` … | pixel result |
| `v.PSdpCustom(qualifier, inverter, imw, ar, k)` | full control |
| `DimenPercent.PSdp(context, value)` | static form |
| `v.ToPercentDp(v, ctx, …)` | generic kernel entry |

### Space helpers

| Extension | Meaning |
|---|---|
| `v.SpaceW(ctx)` · `v.SpaceH(ctx)` · `v.SpaceSw(ctx)` | `v` as % of width / height / smallest-width |
| `v.SpaceWi(ctx)` … `i` variants | multi-window ignored |
| `v.SpaceWDp(ctx)` · `v.SpaceHDp(ctx)` · `v.SpaceSwDp(ctx)` | dp-rounded forms |
| `v.SpaceDp(v, referenceDp)` | % against an arbitrary reference |
| `v.SpaceWSp(ctx)` | text-size (sp) variant |

## 💡 When to use

| Scenario | Recommendation |
|---|---|
| Cards/panels occupying a screen fraction | ✅ Percent |
| Fixed paddings that scale moderately | Scaled (`AppDimens.Net` core) |
| Text auto-fit | [Resize](https://www.nuget.org/packages/AppDimens.Net.Resize/) |

## ⚡ Performance

Same engine as the core: snapshot-partitioned cache, allocation-free fast lanes,
automatic invalidation on configuration changes.

## 📚 Full package family

[AppDimens.Net](https://www.nuget.org/packages/AppDimens.Net/) ·
[Auto](https://www.nuget.org/packages/AppDimens.Net.Auto/) ·
[Logarithmic](https://www.nuget.org/packages/AppDimens.Net.Logarithmic/) ·
[Fluid](https://www.nuget.org/packages/AppDimens.Net.Fluid/) ·
[Interpolated](https://www.nuget.org/packages/AppDimens.Net.Interpolated/) ·
[Diagonal](https://www.nuget.org/packages/AppDimens.Net.Diagonal/) ·
[Perimeter](https://www.nuget.org/packages/AppDimens.Net.Perimeter/) ·
[Power](https://www.nuget.org/packages/AppDimens.Net.Power/) ·
[Fill](https://www.nuget.org/packages/AppDimens.Net.Fill/) ·
[Fit](https://www.nuget.org/packages/AppDimens.Net.Fit/) ·
[Density](https://www.nuget.org/packages/AppDimens.Net.Density/) ·
[Resize](https://www.nuget.org/packages/AppDimens.Net.Resize/) ·
[Units](https://www.nuget.org/packages/AppDimens.Net.Units/) ·
[Maui](https://www.nuget.org/packages/AppDimens.Net.Maui/) ·
[Sdk meta-package](https://www.nuget.org/packages/AppDimens.Net.Sdk/)

---

Apache-2.0 — © Jean Bodenberg · [Repository](https://github.com/bodenberg/appdimens-net-binding) · Formulas are bit-a-bit ports of [appdimens-kmp](https://github.com/bodenberg/appdimens-kmp)

<div align="center">

# 📐 AppDimens.Net

**Responsive dimensions for .NET MAUI — scaled dp/sp with snapshot caching and allocation-free fast lanes.**

The **principal module**: core engine, Scaled strategy (`Sdp/Sdpa/Hdp/Wdp`), inverters,
facilitators, builder, ambient context and cache.

[![NuGet](https://img.shields.io/nuget/v/AppDimens.Net.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/AppDimens.Net/)
[![License](https://img.shields.io/badge/license-Apache--2.0-blue.svg?style=flat-square)](https://github.com/bodenberg/appdimens-net-binding/blob/main/LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%209.0%20%7C%2010.0-8A2BE2.svg?style=flat-square)](https://dotnet.microsoft.com/)
[![Platforms](https://img.shields.io/badge/platforms-Android%20%7C%20iOS%20%7C%20MacCatalyst%20%7C%20Windows-success.svg?style=flat-square)](https://learn.microsoft.com/dotnet/maui)

</div>

---

## ✨ Why AppDimens?

Fixed `dp` values don't scale across phones, foldables, tablets and desktops.
AppDimens scales your dimensions from a **screen qualifier** (smallest-width, height or width),
with bit-a-bit parity to the Kotlin [appdimens-kmp](https://github.com/bodenberg/appdimens-kmp)
library, an event-driven resize watcher, and a snapshot-partitioned cache that makes hot-path
lookups **allocation-free**.

```csharp
float pad = 16.Sdp();   // scales with smallest-width — the classic SDP lane
float h   = 48.Hdp();   // scales with height axis
float w   = 24.Wdp();   // scales with width axis
float sp  = 16.Ssp();   // text size, font-scale aware
```

## 📦 Installation

```bash
dotnet add package AppDimens.Net
```

| | |
|---|---|
| **Targets** | `net8.0` · `net9.0` · `net10.0` |
| **Dependencies** | none |
| **License** | Apache-2.0 |

## 🚀 Getting started

Attach a window once (MAUI) — before creating any page:

```csharp
using AppDimens.Net.Maui.Platform;

protected override Window CreateWindow(IActivationState? state)
{
    var window = new Window();
    AppDimensMaui.AttachWindow(window);          // ambient context + cache + resize watcher
    window.Page = new NavigationPage(new MainPage());
    return window;
}
```

> Parameterless extensions resolve through `AppDimensAmbient.Require()`. Outside MAUI
> (unit tests, design time, Blazor), pass an `IAppDimensContext` explicitly or use
> `FakeAppDimensContext`.

## 🧭 The Scaled strategy

```
scaled = value × screenDp(qualifier) ÷ 300            (base design width: 300 dp)
```

| Extension family | Meaning |
|---|---|
| `v.Sdp()` · `v.Sdpa()` | smallest-width lane (+ aspect-ratio adjustment) |
| `v.Hdp()` · `v.Wdp()` | height / width axis lanes |
| `v.Sdpi()` … `ia` variants | ignore multi-window constraints |
| `v.SdpPx()` … | pixel result in one call |

### Inverters — axis-swap rules for rotation

| Extension | Rule |
|---|---|
| `v.SdpPh(raw)` | portrait → height qualifier |
| `v.SdpLw(raw)` / `v.SdpLh(raw)` | landscape → width / height |
| `v.SdpPw(raw)` | portrait → width |
| `v.HdpLw…`, `v.Wdp…` | same rules on other lanes |

### Facilitators — conditional values without if-chains

```csharp
16.SdpRotate(24, Orientation.Landscape);      // 24 when landscape, else scaled 16
16.SdpMode(20, UiModeType.Foldable);          // UI-mode aware
16.SdpQualifier(DpQualifier.SmallWidth, 600f, 28);  // ≥600 sw → 28
16.SdpScreen(UiModeType.Normal, DpQualifier.SmallWidth, 700f, 32);
// …and *Plain variants that take pre-scaled branches (no re-scaling).
```

### Builder — declarative screens

```csharp
float size = 18.ScaledDp()
    .AspectRatio()
    .Screen(DpQualifier.SmallWidth, 600f, 26f)     // tablet
    .Screen(Orientation.Landscape, 22f)            // landscape override
    .Screen(UiModeType.Foldable, 30f)
    .Sdp();
```

Priority: `qualifier+orientation` → `mode-only` → `qualifier-only` → `orientation-only`.

### Generic kernel

Full control in a single entry point (also used by every satellite module):

```csharp
float v = baseValue.ToDynamicScaledDp(ctx, qualifier, inverter, ignoreMultiWindows, applyAspectRatio, k);
```

## ⚡ Performance

- **Fast lanes** (`DimenCache.ResolveSdpDp/Hdp/Wdp(+Px)`): allocation-free, bitwise-equal
  to the full cached path.
- **Snapshot-partitioned cache**: configuration changes invalidate automatically via the
  event watcher — no manual clearing.
- Custom sensitivity `k` values bypass caching by design.

## 🧪 Testing support

`FakeAppDimensContext` ships in this package: mutable configuration +
`NotifyChange()` to drive watchers deterministically.

## 📚 Full package family

| Package | Strategy |
|---|---|
| **AppDimens.Net** *(this)* | Scaled core + inverters + facilitators |
| [AppDimens.Net.Percent](https://www.nuget.org/packages/AppDimens.Net.Percent/) | percentage of screen dimension |
| [AppDimens.Net.Power](https://www.nuget.org/packages/AppDimens.Net.Power/) | power-law curve |
| [AppDimens.Net.Auto](https://www.nuget.org/packages/AppDimens.Net.Auto/) | auto-balanced scaling |
| [AppDimens.Net.Logarithmic](https://www.nuget.org/packages/AppDimens.Net.Logarithmic/) | logarithmic growth |
| [AppDimens.Net.Fluid](https://www.nuget.org/packages/AppDimens.Net.Fluid/) | fluid interpolation |
| [AppDimens.Net.Interpolated](https://www.nuget.org/packages/AppDimens.Net.Interpolated/) | interpolated curve |
| [AppDimens.Net.Diagonal](https://www.nuget.org/packages/AppDimens.Net.Diagonal/) | screen diagonal basis |
| [AppDimens.Net.Perimeter](https://www.nuget.org/packages/AppDimens.Net.Perimeter/) | screen perimeter basis |
| [AppDimens.Net.Fill](https://www.nuget.org/packages/AppDimens.Net.Fill/) | fill-proportional |
| [AppDimens.Net.Fit](https://www.nuget.org/packages/AppDimens.Net.Fit/) | priority-driven fit builder |
| [AppDimens.Net.Density](https://www.nuget.org/packages/AppDimens.Net.Density/) | density-aware scaling |
| [AppDimens.Net.Resize](https://www.nuget.org/packages/AppDimens.Net.Resize/) | auto-resize text/squares |
| [AppDimens.Net.Units](https://www.nuget.org/packages/AppDimens.Net.Units/) | mm/cm/inch conversions |
| [AppDimens.Net.Maui](https://www.nuget.org/packages/AppDimens.Net.Maui/) | window attach + XAML markup |
| [AppDimens.Net.Sdk](https://www.nuget.org/packages/AppDimens.Net.Sdk/) | meta-package (everything) |

---

Apache-2.0 — © Jean Bodenberg · [Repository](https://github.com/bodenberg/appdimens-net-binding) · [Documentation](https://github.com/bodenberg/appdimens-net-binding/tree/main/appdimens-net/DOCUMENTATION) · Formulas are bit-a-bit ports of [appdimens-kmp](https://github.com/bodenberg/appdimens-kmp)

<div align="center">

# 🎛️ AppDimens.Net.Fit

**The `DimenFit` builder — priority-driven per-screen overrides resolved in one call.**

Part of the **AppDimens for .NET** family · requires [AppDimens.Net](https://www.nuget.org/packages/AppDimens.Net/) (core).

[![NuGet](https://img.shields.io/nuget/v/AppDimens.Net.Fit.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/AppDimens.Net.Fit/)
[![License](https://img.shields.io/badge/license-Apache--2.0-blue.svg?style=flat-square)](https://github.com/bodenberg/appdimens-net-binding/blob/main/LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%209.0%20%7C%2010.0-8A2BE2.svg?style=flat-square)](https://dotnet.microsoft.com/)

</div>

---

## 🎯 What is it?

**Fit** is the declarative sibling of the Scaled strategy: declare a base value plus any
number of conditional `.Screen(...)` entries (qualifier, orientation, UI mode) and let the
builder pick the winner using a deterministic priority chain — no nested ifs anywhere.

## 📦 Installation

```bash
dotnet add package AppDimens.Net.Fit
```

> Depends on [AppDimens.Net](https://www.nuget.org/packages/AppDimens.Net/) (pulled automatically).
> Attach a MAUI window first or pass an explicit `IAppDimensContext`.

## 🚀 Quick start

```csharp
using AppDimens.Net.Fit;

float size = 18.FitScaledDp()                       // base = 18 dp
    .ApplyAspectRatio()
    .Screen(DpQualifier.SmallWidth, 600f, 26f)      // ≥600 sw → 26
    .Screen(Orientation.Landscape, 22f)             // landscape → 22
    .Screen(UiModeType.Foldable, 30f)               // foldable → 30
    .Ftsdp();                                       // resolve (smallest-width)

// also available: .Fthdp() (height) · .Ftwdp() (width) · .Px(...)
```

Priority resolution:
`qualifier+orientation` → `mode-only` → `qualifier-only` → `orientation-only` → base value.

## 🧭 API

### Builder — `DimenFit`

| Member | Purpose |
|---|---|
| `DimenFit.Create(baseDp)` / `value.FitScaledDp()` | start a definition |
| `.Screen(qualifier, threshold, customValue)` | qualifier entry (e.g. sw ≥ 600) |
| `.Screen(orientation, customValue)` | orientation entry |
| `.Screen(uiModeType, customValue)` | UI-mode entry (foldables…) |
| `.Screen(uiModeType, orientation, customValue…)` | combined entries (highest priority) |
| `.ApplyAspectRatio(bool)` · `.IgnoreMultiWindows(bool)` · `.CustomSensitivity(k)` | modifiers |
| `.Ftsdp()` / `.Fthdp()` / `.Ftwdp()` / `.Px(...)` | resolve against ambient context |
| `.Resolve(ctx, defaultQualifier)` | explicit-context resolution |

### Kernel

```csharp
float v = baseValue.ToFitDp(context, qualifier, inverter, ignoreMultiWindows, applyAspectRatio, k);
```

## 💡 When to use

| Scenario | Recommendation |
|---|---|
| Multiple device-specific overrides | ✅ Fit builder |
| Continuous curves without breakpoints | [Fluid](https://www.nuget.org/packages/AppDimens.Net.Fluid/) |
| Inline single-condition checks | core facilitators (`SdpQualifier`, `SdpRotate`) |

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
[Density](https://www.nuget.org/packages/AppDimens.Net.Density/) ·
[Resize](https://www.nuget.org/packages/AppDimens.Net.Resize/) ·
[Units](https://www.nuget.org/packages/AppDimens.Net.Units/) ·
[Maui](https://www.nuget.org/packages/AppDimens.Net.Maui/) ·
[Sdk meta-package](https://www.nuget.org/packages/AppDimens.Net.Sdk/)

---

Apache-2.0 — © Jean Bodenberg · [Repository](https://github.com/bodenberg/appdimens-net-binding) · Formulas are bit-a-bit ports of [appdimens-kmp](https://github.com/bodenberg/appdimens-kmp)

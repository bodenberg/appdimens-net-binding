<div align="center">

# 🧰 AppDimens.Net.Sdk

**Meta-package ("BOM") — one reference, the whole AppDimens .NET toolkit.**

Installs the [core engine](https://www.nuget.org/packages/AppDimens.Net/) plus **every strategy satellite** with pinned, compatible versions.

[![NuGet](https://img.shields.io/nuget/v/AppDimens.Net.Sdk.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/AppDimens.Net.Sdk/)
[![License](https://img.shields.io/badge/license-Apache--2.0-blue.svg?style=flat-square)](https://github.com/bodenberg/appdimens-net-binding/blob/main/LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%209.0%20%7C%2010.0-8A2BE2.svg?style=flat-square)](https://dotnet.microsoft.com/)

</div>

---

## 🎯 What is it?

`AppDimens.Net.Sdk` is a **bill of materials**: add a single package and get responsive
dp/sp scaling with all 14+ strategies — Scaled core, Percent, Power, Auto, Logarithmic,
Fluid, Interpolated, Diagonal, Perimeter, Fill, Fit builder, Density, Resize math and
physical Units.

Prefer minimal footprint? Reference only what you use — every satellite works standalone
on top of the tiny core.

## 📦 Installation

```bash
dotnet add package AppDimens.Net.Sdk
```

## 🚀 Getting started (MAUI)

```csharp
using AppDimens.Net.Maui.Platform;   // from AppDimens.Net.Maui (pulled transitively)

protected override Window CreateWindow(IActivationState? state)
{
    var window = new Window();
    AppDimensMaui.AttachWindow(window);      // BEFORE creating any page
    window.Page = new NavigationPage(new MainPage());
    return window;
}
```

```csharp
// Then mix strategies freely — everything is in one dependency graph:
float pad   = 16.Sdp();          // Scaled core
float card  = 30.PSdp();         // Percent
float hero  = 18.LOGSdp();       // Logarithmic
float fitV  = 18.FitScaledDp().Screen(DpQualifier.SmallWidth, 600f, 26f).Ftsdp();
```

## 📥 What it pulls in

| Package | Strategy |
|---|---|
| [AppDimens.Net](https://www.nuget.org/packages/AppDimens.Net/) | core + Scaled (`Sdp/Sdpa/Hdp/Wdp`, inverters, facilitators) |
| [AppDimens.Net.Percent](https://www.nuget.org/packages/AppDimens.Net.Percent/) | % of screen + Space helpers |
| [AppDimens.Net.Power](https://www.nuget.org/packages/AppDimens.Net.Power/) | power-law curve |
| [AppDimens.Net.Auto](https://www.nuget.org/packages/AppDimens.Net.Auto/) | auto-balanced |
| [AppDimens.Net.Logarithmic](https://www.nuget.org/packages/AppDimens.Net.Logarithmic/) | log curve |
| [AppDimens.Net.Fluid](https://www.nuget.org/packages/AppDimens.Net.Fluid/) | fluid interpolation |
| [AppDimens.Net.Interpolated](https://www.nuget.org/packages/AppDimens.Net.Interpolated/) | linear anchors |
| [AppDimens.Net.Diagonal](https://www.nuget.org/packages/AppDimens.Net.Diagonal/) | diagonal basis |
| [AppDimens.Net.Perimeter](https://www.nuget.org/packages/AppDimens.Net.Perimeter/) | perimeter basis |
| [AppDimens.Net.Fill](https://www.nuget.org/packages/AppDimens.Net.Fill/) | fill-proportional |
| [AppDimens.Net.Fit](https://www.nuget.org/packages/AppDimens.Net.Fit/) | priority-driven builder |
| [AppDimens.Net.Density](https://www.nuget.org/packages/AppDimens.Net.Density/) | density-aware |
| [AppDimens.Net.Resize](https://www.nuget.org/packages/AppDimens.Net.Resize/) | auto-resize text/squares |
| [AppDimens.Net.Units](https://www.nuget.org/packages/AppDimens.Net.Units/) | mm/cm/inch |

> ℹ️ `AppDimens.Net.Maui` (window attach + XAML markup) is **not** pulled automatically —
> add it explicitly if you want the MAUI bootstrap and markup extensions.

## 💡 When to use

| Scenario | Recommendation |
|---|---|
| Explore all strategies / full toolkit apps | ✅ Sdk meta-package |
| Lean production apps | individual satellites you need |
| MAUI bootstrap + XAML | also add [AppDimens.Net.Maui](https://www.nuget.org/packages/AppDimens.Net.Maui/) |

---

Apache-2.0 — © Jean Bodenberg · [Repository](https://github.com/bodenberg/appdimens-net-binding) · Formulas are bit-a-bit ports of [appdimens-kmp](https://github.com/bodenberg/appdimens-kmp)

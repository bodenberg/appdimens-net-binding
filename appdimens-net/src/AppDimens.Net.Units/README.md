<div align="center">

# 📏 AppDimens.Net.Units

**Physical units — mm, cm and inch converted to dp/px/sp with real device DPI.**

Part of the **AppDimens for .NET** family · requires [AppDimens.Net](https://www.nuget.org/packages/AppDimens.Net/) (core).

[![NuGet](https://img.shields.io/nuget/v/AppDimens.Net.Units.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/AppDimens.Net.Units/)
[![License](https://img.shields.io/badge/license-Apache--2.0-blue.svg?style=flat-square)](https://github.com/bodenberg/appdimens-net-binding/blob/main/LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%209.0%20%7C%2010.0-8A2BE2.svg?style=flat-square)](https://dotnet.microsoft.com/)

</div>

---

## 🎯 What is it?

When UI must match the **physical world** — rulers, ID/photo frames, credit-card-sized
widgets, print previews — `dp` isn't enough. This module converts millimeters,
centimeters and inches into screen units using the actual device DPI.

## 📦 Installation

```bash
dotnet add package AppDimens.Net.Units
```

> Depends on [AppDimens.Net](https://www.nuget.org/packages/AppDimens.Net/) (pulled automatically).
> Attach a MAUI window first or pass an explicit `IAppDimensContext`.

## 🚀 Quick start

```csharp
using AppDimens.Net.Units;

// A standard credit card is 85.6 × 53.98 mm
float wDp = DimenPhysicalUnits.ToDpFromMm(85.6f);
float hDp = DimenPhysicalUnits.ToDpFromMm(53.98f);
card.WidthRequest = wDp;
card.HeightRequest = hDp;

// 1 inch ruler bar in pixels
float rulerPx = DimenPhysicalUnits.ToPxFromInch(1f);

// Circle radius from a 25 mm diameter
float r = DimenPhysicalUnits.RadiusFromDiameter(25f, UnitType.Mm);
```

## 🧭 API — `DimenPhysicalUnits`

| Method | Converts |
|---|---|
| `ToDpFromMm(mm)` · `ToDpFromCm(cm)` · `ToDpFromInch(inch)` | physical length → dp |
| `ToPxFromMm` · `ToPxFromCm` · `ToPxFromInch` | physical length → px |
| `ToSpFromMm` · `ToSpFromCm` · `ToSpFromInch` | physical length → sp |
| `RadiusFromDiameter(diameter, unitType)` | diameter → radius (screen units) |

All methods accept an optional `IAppDimensContext` (defaults to the ambient context).

## 💡 When to use

| Scenario | Recommendation |
|---|---|
| Real-world sized UI (rulers, cards, print) | ✅ Units |
| Density-flavored responsive sizing | [Density](https://www.nuget.org/packages/AppDimens.Net.Density/) |
| Everything else | core Scaled strategy |

## ⚡ Performance

Conversions run through the shared snapshot cache; DPI changes invalidate automatically
via the event watcher.

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
[Density](https://www.nuget.org/packages/AppDimens.Net.Density/) ·
[Resize](https://www.nuget.org/packages/AppDimens.Net.Resize/) ·
[Maui](https://www.nuget.org/packages/AppDimens.Net.Maui/) ·
[Sdk meta-package](https://www.nuget.org/packages/AppDimens.Net.Sdk/)

---

Apache-2.0 — © Jean Bodenberg · [Repository](https://github.com/bodenberg/appdimens-net-binding) · Formulas are bit-a-bit ports of [appdimens-kmp](https://github.com/bodenberg/appdimens-kmp)

# AppDimens.Net.Units

**Physical units — millimetres, centimetres and inches converted to dp, px and sp using the real display DPI.**

| | |
|---|---|
| **NuGet** | [`AppDimens.Net.Units`](https://www.nuget.org/packages/AppDimens.Net.Units) (requires `AppDimens.Net`) |
| **Namespace** | `AppDimens.Net.Code.Units` (enum `UnitType` in `AppDimens.Net.Common`) |
| **Frameworks** | `net8.0`, `net9.0`, `net10.0` (+ `net11.0`) |
| **API reference** | [API.md](API.md) |

---

## When to use

| Scenario | Recommendation |
|----------|----------------|
| UI that must match the real world: rulers, ID-card frames, print previews, touch targets in mm | ✅ Units |
| Density-flavoured responsive sizing | [Density](../AppDimens.Net.Density/README.md) |
| Everything else | [AppDimens.Net](../AppDimens.Net/README.md) (Scaled) |

## Install

```bash
dotnet add package AppDimens.Net.Units
```

## Every conversion

```csharp
using AppDimens.Net;
using AppDimens.Net.Code.Units;

var ctx = AppDimensAmbient.Require();   // the context gives the real DPI (Xdpi)

// → dp (use for MAUI / WPF / WinUI sizes)
float cardWidthDp  = DimenPhysicalUnits.ToDpFromMm(85.6f, ctx);   // credit-card width
float rulerCmDp    = DimenPhysicalUnits.ToDpFromCm(1f, ctx);
float inchDp       = DimenPhysicalUnits.ToDpFromInch(1f, ctx);

// → px (canvas drawing, WinForms, Android)
float tickPx       = DimenPhysicalUnits.ToPxFromMm(1f, ctx);
float cmPx         = DimenPhysicalUnits.ToPxFromCm(2.5f, ctx);
float inchPx       = DimenPhysicalUnits.ToPxFromInch(0.5f, ctx);

// → sp (text sized in physical units)
float labelSp      = DimenPhysicalUnits.ToSpFromMm(4f, ctx);
float labelSpCm    = DimenPhysicalUnits.ToSpFromCm(0.4f, ctx);
float labelSpInch  = DimenPhysicalUnits.ToSpFromInch(0.15f, ctx);
```

The context parameter is optional: without it the ambient context is used.

## Radius from a diameter

```csharp
using AppDimens.Net.Code.Units;
using AppDimens.Net.Common;

float coinRadius = DimenPhysicalUnits.RadiusFromDiameter(24f, UnitType.Mm);    // in dp
float dotRadius  = DimenPhysicalUnits.RadiusFromDiameter(0.25f, UnitType.Inch);
```

## Example: an on-screen ruler (MAUI / WPF / WinUI)

```csharp
using AppDimens.Net.Code.Units;

float mm = DimenPhysicalUnits.ToDpFromMm(1f);
for (int i = 0; i <= 100; i++)
{
    float x = i * mm;                    // position of the i-th millimetre tick (dp)
    float tickHeight = i % 10 == 0 ? 12f : i % 5 == 0 ? 8f : 4f;
    // draw a line at x with tickHeight …
}
```

> Accuracy depends on the DPI the OS reports. On Windows the integrations derive it from the monitor scale
> (96 DPI × scale), which matches the physical size only on correctly configured monitors.

---

## Português (resumo)

`dotnet add package AppDimens.Net.Units` → `using AppDimens.Net.Code.Units;` →
`DimenPhysicalUnits.ToDpFromMm(85.6f)` (mm → dp), `ToPxFromCm(...)`, `ToSpFromInch(...)`, `RadiusFromDiameter(24, UnitType.Mm)`.

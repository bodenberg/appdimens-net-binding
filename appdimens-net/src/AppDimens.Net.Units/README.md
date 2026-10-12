# AppDimens.Net.Units

**Physical units — mm, cm and inch converted to dp / px / sp with the real display DPI.**

Part of **AppDimens for .NET** · requires `AppDimens.Net` · `net8.0`–`net10.0`.

📖 **Complete guide:** [AppDimens.Net.Units documentation](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.Units/README.md) · **API reference:** [API.md](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.Units/API.md)

## Install

```bash
dotnet add package AppDimens.Net.Units
```

## Use

```csharp
using AppDimens.Net.Code.Units;
using AppDimens.Net.Common;

float cardWidthDp = DimenPhysicalUnits.ToDpFromMm(85.6f);     // credit-card width in dp
float tickPx      = DimenPhysicalUnits.ToPxFromMm(1f);        // 1 mm in pixels
float labelSp     = DimenPhysicalUnits.ToSpFromCm(0.4f);      // text 4 mm tall
float inchDp      = DimenPhysicalUnits.ToDpFromInch(1f);
float radiusDp    = DimenPhysicalUnits.RadiusFromDiameter(24f, UnitType.Mm);
```

All methods accept an optional `IAppDimensContext` (default: the ambient window).

## Português

Converte unidades físicas: `DimenPhysicalUnits.ToDpFromMm(85.6f)`, `ToPxFromCm(...)`, `ToSpFromInch(...)`.

---

Apache-2.0 — © Jean Bodenberg · [Repository](https://github.com/bodenberg/appdimens-net-binding) · Formulas are bit-for-bit ports of [appdimens-kmp](https://github.com/bodenberg/appdimens-kmp)

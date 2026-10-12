# AppDimens.Net.Sdk

**Meta-package: one reference that brings the whole AppDimens for .NET family** — the core plus every scaling
strategy. It contains no code of its own (no `API.md`); each package below has its own guide.

| | |
|---|---|
| **NuGet** | [`AppDimens.Net.Sdk`](https://www.nuget.org/packages/AppDimens.Net.Sdk) |
| **Frameworks** | `net8.0`, `net9.0`, `net10.0` (+ `net11.0`) |

## Install

```bash
dotnet add package AppDimens.Net.Sdk
# + the integration for your UI framework:
dotnet add package AppDimens.Net.Maui      # or .Wpf / .WinForms / .WinUI
```

## What you get

| Package | Namespace | Prefix / entry point | Guide |
|---------|-----------|----------------------|-------|
| `AppDimens.Net` | `AppDimens.Net.Code.Scaled` | `Sdp`, `Hdp`, `Wdp`, `Ssp`, builders, inverters | [guide](../AppDimens.Net/README.md) |
| `AppDimens.Net.Auto` | `AppDimens.Net.Code.Auto` | `ASdp` | [guide](../AppDimens.Net.Auto/README.md) |
| `AppDimens.Net.Density` | `AppDimens.Net.Code.Density` | `DSdp` | [guide](../AppDimens.Net.Density/README.md) |
| `AppDimens.Net.Diagonal` | `AppDimens.Net.Code.Diagonal` | `DGSdp` | [guide](../AppDimens.Net.Diagonal/README.md) |
| `AppDimens.Net.Fill` | `AppDimens.Net.Code.Fill` | `FLSdp` | [guide](../AppDimens.Net.Fill/README.md) |
| `AppDimens.Net.Fit` | `AppDimens.Net.Code.Fit` | `FitScaledDp()` builder | [guide](../AppDimens.Net.Fit/README.md) |
| `AppDimens.Net.Fluid` | `AppDimens.Net.Code.Fluid` | `FSdp` | [guide](../AppDimens.Net.Fluid/README.md) |
| `AppDimens.Net.Interpolated` | `AppDimens.Net.Code.Interpolated` | `ISdp` | [guide](../AppDimens.Net.Interpolated/README.md) |
| `AppDimens.Net.Logarithmic` | `AppDimens.Net.Code.Logarithmic` | `LOGSdp` | [guide](../AppDimens.Net.Logarithmic/README.md) |
| `AppDimens.Net.Percent` | `AppDimens.Net.Code.Percent` | `PSdp`, `SpaceWDp` | [guide](../AppDimens.Net.Percent/README.md) |
| `AppDimens.Net.Perimeter` | `AppDimens.Net.Code.Perimeter` | `PRSdp` | [guide](../AppDimens.Net.Perimeter/README.md) |
| `AppDimens.Net.Power` | `AppDimens.Net.Code.Power` | `PWSdp` | [guide](../AppDimens.Net.Power/README.md) |
| `AppDimens.Net.Resize` | `AppDimens.Net.Code.Resize` | `DimenResize.AutoResizeTextPx` | [guide](../AppDimens.Net.Resize/README.md) |
| `AppDimens.Net.Units` | `AppDimens.Net.Code.Units` | `DimenPhysicalUnits.ToDpFromMm` | [guide](../AppDimens.Net.Units/README.md) |

UI integrations (install separately): [Maui](../AppDimens.Net.Maui/README.md) · [Wpf](../AppDimens.Net.Wpf/README.md) ·
[WinForms](../AppDimens.Net.WinForms/README.md) · [WinUI](../AppDimens.Net.WinUI/README.md).

## Which strategy should I use?

| `16` on… | Phone 360×800 | Tablet 800×1280 | Desktop 1280×800 | Character |
|----------|--:|--:|--:|-----------|
| `Sdp` (Scaled) | 19.2 | 42.7 | 42.7 | proportional — the default |
| `ASdp` (Auto) | 19.2 | 30.2 | 30.2 | proportional, damped on big screens |
| `FSdp` (Fluid) | 13.4 | 19.2 | 19.2 | clamped 0.8×–1.2× — great for desktop |
| `LOGSdp` (Logarithmic) | 17.2 | 22.3 | 22.3 | gentle growth |
| `ISdp` (Interpolated) | 17.6 | 29.3 | 29.3 | linear between anchors |
| `PWSdp` (Power) | 18.3 | 33.4 | 33.4 | tunable with `k` |
| `DGSdp` (Diagonal) | 22.9 | 39.5 | 39.5 | same in portrait and landscape |
| `PRSdp` (Perimeter) | 22.3 | 40 | 40 | balances width + height |
| `FLSdp` (Fill) | 24 | 42.7 | 42.7 | fills space |
| `PSdp` (Percent) | 19.2 | 42.7 | 42.7 | strictly linear per axis |
| `DSdp` (Density) | 16 | 16 | 16 | follows DPI, not window size |

**Beginner rule:** use `Sdp` for layout and `Ssp` for text; switch a value to `FSdp` or `ASdp` if it gets too big on
large windows.

## Example — mixing strategies

```csharp
using AppDimens.Net.Code.Scaled;
using AppDimens.Net.Code.Fluid;
using AppDimens.Net.Code.Auto;
using AppDimens.Net.Code.Diagonal;

float padding = 16.Sdp();      // layout
float title   = 22.Ssp();      // text
float hero    = 240.FSdp();    // big image that must not explode on desktop
float card    = 320.ASdp();    // card width, damped on tablets
float fab     = 56.DGSdp();    // floating button, stable on rotation
```

## Português (resumo)

`dotnet add package AppDimens.Net.Sdk` instala o núcleo e todas as estratégias. Some o pacote do seu framework
(`AppDimens.Net.Maui`, `.Wpf`, `.WinForms` ou `.WinUI`). Regra simples: `Sdp` para layout, `Ssp` para texto, `FSdp`/`ASdp`
quando algo ficar grande demais em telas grandes.

# AppDimens.Net.Power

**Power-law scaling — tunable growth with a sensitivity exponent `k`.**

Part of **AppDimens for .NET** (C# port of appdimens-kmp) · requires `AppDimens.Net` (installed automatically).
Works with .NET MAUI, WPF, Windows Forms, WinUI 3 and plain .NET (`net8.0` / `net9.0` / `net10.0`).

📖 **Complete guide:** [AppDimens.Net.Power documentation](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.Power/README.md) · **API reference:** [API.md](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.Power/API.md)

## What it does

The **Power** strategy applies a power-law curve to the screen-relative scale. With the default sensitivity it grows a little slower than Scaled; pass a custom `k` (`PWSdpCustom(..., k: 1.4f)`) to make big screens grow faster or `k < 1` to flatten the curve — dashboards, media grids and map canvases.

| `16` with this strategy | Phone 360×800 | Tablet 800×1280 | Desktop 1280×800 |
|---|--:|--:|--:|
| `16.PWSdp()` | 18.3 | 33.4 | 33.4 |
| `16.PWHdp()` | 18.3 | 33.4 | 33.4 |
| `16.PWWdp()` | 18.3 | 33.4 | 33.4 |
| `16.Sdp()` (core Scaled) | 19.2 | 42.7 | 42.7 |

## Install

```bash
dotnet add package AppDimens.Net.Power
```

## Use

1. Initialize once for your UI framework: MAUI `AppDimensMaui.AttachWindow(window)` · WPF `AppDimensWpf.Initialize(this)` ·
   WinForms `AppDimensWinForms.Initialize(this)` · WinUI `AppDimensWinUI.Initialize(window)`.
2. Call the extensions:

```csharp
using AppDimens.Net.Code.Power;

float padding = 16.PWSdp();    // smaller side of the window
float header  = 56.PWHdp();    // height
float sidebar = 200.PWWdp();   // width
float tall    = 16.PWSdpa();   // + aspect-ratio correction
float split   = 16.PWSdpi();   // ignores split-screen shrinking
```

With an explicit context: `16.PWSdp(ctx)`, `16.PWSdpPx(ctx)`, `DimenPower.PWSdp(ctx, 16)`, `24.ToPowerDp(ctx, qualifier: DpQualifier.Width)`.

## Português

`dotnet add package AppDimens.Net.Power` → inicialize a janela uma vez → `using AppDimens.Net.Code.Power;` → `16.PWSdp()`, `56.PWHdp()`, `200.PWWdp()`.
Guia completo: [https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.Power/README.md](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.Power/README.md)

---

Apache-2.0 — © Jean Bodenberg · [Repository](https://github.com/bodenberg/appdimens-net-binding)

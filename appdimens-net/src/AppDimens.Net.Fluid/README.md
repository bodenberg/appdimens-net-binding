# AppDimens.Net.Fluid

**Fluid scaling — smooth, clamped growth between a phone and a tablet reference (0.8× → 1.2×).**

Part of **AppDimens for .NET** (C# port of appdimens-kmp) · requires `AppDimens.Net` (installed automatically).
Works with .NET MAUI, WPF, Windows Forms, WinUI 3 and plain .NET (`net8.0` / `net9.0` / `net10.0`).

📖 **Complete guide:** [AppDimens.Net.Fluid documentation](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.Fluid/README.md) · **API reference:** [API.md](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.Fluid/API.md)

## What it does

The **Fluid** strategy interpolates smoothly between a minimum (0.8×) on small screens and a maximum (1.2×) on large ones, so values **never explode** on big monitors and never collapse on tiny phones. Great for resizable desktop windows, split view and foldables. `16.FSdp()` gives 12.8 at ≤ 300 dp, 13.4 at 360 dp, 16.8 at 600 dp and 19.2 from 800 dp up.

| `16` with this strategy | Phone 360×800 | Tablet 800×1280 | Desktop 1280×800 |
|---|--:|--:|--:|
| `16.FSdp()` | 13.4 | 19.2 | 19.2 |
| `16.FHdp()` | 19.2 | 19.2 | 19.2 |
| `16.FWdp()` | 13.4 | 19.2 | 19.2 |
| `16.Sdp()` (core Scaled) | 19.2 | 42.7 | 42.7 |

## Install

```bash
dotnet add package AppDimens.Net.Fluid
```

## Use

1. Initialize once for your UI framework: MAUI `AppDimensMaui.AttachWindow(window)` · WPF `AppDimensWpf.Initialize(this)` ·
   WinForms `AppDimensWinForms.Initialize(this)` · WinUI `AppDimensWinUI.Initialize(window)`.
2. Call the extensions:

```csharp
using AppDimens.Net.Code.Fluid;

float padding = 16.FSdp();    // smaller side of the window
float header  = 56.FHdp();    // height
float sidebar = 200.FWdp();   // width
float tall    = 16.FSdpa();   // + aspect-ratio correction
float split   = 16.FSdpi();   // ignores split-screen shrinking
```

With an explicit context: `16.FSdp(ctx)`, `16.FSdpPx(ctx)`, `DimenFluid.FSdp(ctx, 16)`, `24.ToFluidDp(ctx, qualifier: DpQualifier.Width)`.

## Português

`dotnet add package AppDimens.Net.Fluid` → inicialize a janela uma vez → `using AppDimens.Net.Code.Fluid;` → `16.FSdp()`, `56.FHdp()`, `200.FWdp()`.
Guia completo: [https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.Fluid/README.md](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.Fluid/README.md)

---

Apache-2.0 — © Jean Bodenberg · [Repository](https://github.com/bodenberg/appdimens-net-binding)

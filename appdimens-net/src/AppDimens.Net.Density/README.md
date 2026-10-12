# AppDimens.Net.Density

**Density-aware scaling — multiplies by the display density (dpi ÷ 160), not by the window size.**

Part of **AppDimens for .NET** (C# port of appdimens-kmp) · requires `AppDimens.Net` (installed automatically).
Works with .NET MAUI, WPF, Windows Forms, WinUI 3 and plain .NET (`net8.0` / `net9.0` / `net10.0`).

📖 **Complete guide:** [AppDimens.Net.Density documentation](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.Density/README.md) · **API reference:** [API.md](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.Density/API.md)

## What it does

The **Density** strategy scales the value by the display density relative to 160 dpi (Android *mdpi*): `16.DSdp()` is 16 at 160 dpi, 32 at 320 dpi and 48 at 480 dpi. It does **not** follow the window size — use it when an element should keep the same *physical* proportion across low- and high-density screens. On Windows the density is the monitor scale (100 % = 1.0, 150 % = 1.5).

| `16` with this strategy | Phone 360×800 | Tablet 800×1280 | Desktop 1280×800 |
|---|--:|--:|--:|
| `16.DSdp()` | 16 | 16 | 16 |
| `16.DHdp()` | 16 | 16 | 16 |
| `16.DWdp()` | 16 | 16 | 16 |
| `16.Sdp()` (core Scaled) | 19.2 | 42.7 | 42.7 |

## Install

```bash
dotnet add package AppDimens.Net.Density
```

## Use

1. Initialize once for your UI framework: MAUI `AppDimensMaui.AttachWindow(window)` · WPF `AppDimensWpf.Initialize(this)` ·
   WinForms `AppDimensWinForms.Initialize(this)` · WinUI `AppDimensWinUI.Initialize(window)`.
2. Call the extensions:

```csharp
using AppDimens.Net.Code.Density;

float padding = 16.DSdp();    // smaller side of the window
float header  = 56.DHdp();    // height
float sidebar = 200.DWdp();   // width
float tall    = 16.DSdpa();   // + aspect-ratio correction
float split   = 16.DSdpi();   // ignores split-screen shrinking
```

With an explicit context: `16.DSdp(ctx)`, `16.DSdpPx(ctx)`, `DimenDensity.DSdp(ctx, 16)`, `24.ToDensityDp(ctx, qualifier: DpQualifier.Width)`.

## Português

`dotnet add package AppDimens.Net.Density` → inicialize a janela uma vez → `using AppDimens.Net.Code.Density;` → `16.DSdp()`, `56.DHdp()`, `200.DWdp()`.
Guia completo: [https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.Density/README.md](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.Density/README.md)

---

Apache-2.0 — © Jean Bodenberg · [Repository](https://github.com/bodenberg/appdimens-net-binding)

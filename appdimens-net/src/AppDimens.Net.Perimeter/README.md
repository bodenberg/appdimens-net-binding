# AppDimens.Net.Perimeter

**Perimeter scaling — balances width and height in a single number.**

Part of **AppDimens for .NET** (C# port of appdimens-kmp) · requires `AppDimens.Net` (installed automatically).
Works with .NET MAUI, WPF, Windows Forms, WinUI 3 and plain .NET (`net8.0` / `net9.0` / `net10.0`).

📖 **Complete guide:** [AppDimens.Net.Perimeter documentation](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.Perimeter/README.md) · **API reference:** [API.md](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.Perimeter/API.md)

## What it does

The **Perimeter** strategy derives the scale from the sum of both screen axes, balancing width *and* height contributions. It gives stable results for edge-anchored UI: gutters, rails, borders, safe areas. All axes give the same result.

| `16` with this strategy | Phone 360×800 | Tablet 800×1280 | Desktop 1280×800 |
|---|--:|--:|--:|
| `16.PRSdp()` | 22.3 | 40 | 40 |
| `16.PRHdp()` | 22.3 | 40 | 40 |
| `16.PRWdp()` | 22.3 | 40 | 40 |
| `16.Sdp()` (core Scaled) | 19.2 | 42.7 | 42.7 |

## Install

```bash
dotnet add package AppDimens.Net.Perimeter
```

## Use

1. Initialize once for your UI framework: MAUI `AppDimensMaui.AttachWindow(window)` · WPF `AppDimensWpf.Initialize(this)` ·
   WinForms `AppDimensWinForms.Initialize(this)` · WinUI `AppDimensWinUI.Initialize(window)`.
2. Call the extensions:

```csharp
using AppDimens.Net.Code.Perimeter;

float padding = 16.PRSdp();    // smaller side of the window
float header  = 56.PRHdp();    // height
float sidebar = 200.PRWdp();   // width
float tall    = 16.PRSdpa();   // + aspect-ratio correction
float split   = 16.PRSdpi();   // ignores split-screen shrinking
```

With an explicit context: `16.PRSdp(ctx)`, `16.PRSdpPx(ctx)`, `DimenPerimeter.PRSdp(ctx, 16)`, `24.ToPerimeterDp(ctx, qualifier: DpQualifier.Width)`.

## Português

`dotnet add package AppDimens.Net.Perimeter` → inicialize a janela uma vez → `using AppDimens.Net.Code.Perimeter;` → `16.PRSdp()`, `56.PRHdp()`, `200.PRWdp()`.
Guia completo: [https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.Perimeter/README.md](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.Perimeter/README.md)

---

Apache-2.0 — © Jean Bodenberg · [Repository](https://github.com/bodenberg/appdimens-net-binding)

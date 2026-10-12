# AppDimens.Net.Diagonal

**Diagonal scaling — derived from the screen diagonal, identical in portrait and landscape.**

Part of **AppDimens for .NET** (C# port of appdimens-kmp) · requires `AppDimens.Net` (installed automatically).
Works with .NET MAUI, WPF, Windows Forms, WinUI 3 and plain .NET (`net8.0` / `net9.0` / `net10.0`).

📖 **Complete guide:** [AppDimens.Net.Diagonal documentation](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.Diagonal/README.md) · **API reference:** [API.md](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.Diagonal/API.md)

## What it does

The **Diagonal** strategy computes the scale from the screen diagonal (√(w² + h²)), like physical screen sizes ("6.7-inch display"). Rotation swaps width and height but keeps the diagonal, so values **do not jump when the device rotates** — ideal for floating buttons, overlays and app chrome. `DGSdp`, `DGHdp` and `DGWdp` give the same result.

| `16` with this strategy | Phone 360×800 | Tablet 800×1280 | Desktop 1280×800 |
|---|--:|--:|--:|
| `16.DGSdp()` | 22.9 | 39.5 | 39.5 |
| `16.DGHdp()` | 22.9 | 39.5 | 39.5 |
| `16.DGWdp()` | 22.9 | 39.5 | 39.5 |
| `16.Sdp()` (core Scaled) | 19.2 | 42.7 | 42.7 |

## Install

```bash
dotnet add package AppDimens.Net.Diagonal
```

## Use

1. Initialize once for your UI framework: MAUI `AppDimensMaui.AttachWindow(window)` · WPF `AppDimensWpf.Initialize(this)` ·
   WinForms `AppDimensWinForms.Initialize(this)` · WinUI `AppDimensWinUI.Initialize(window)`.
2. Call the extensions:

```csharp
using AppDimens.Net.Code.Diagonal;

float padding = 16.DGSdp();    // smaller side of the window
float header  = 56.DGHdp();    // height
float sidebar = 200.DGWdp();   // width
float tall    = 16.DGSdpa();   // + aspect-ratio correction
float split   = 16.DGSdpi();   // ignores split-screen shrinking
```

With an explicit context: `16.DGSdp(ctx)`, `16.DGSdpPx(ctx)`, `DimenDiagonal.DGSdp(ctx, 16)`, `24.ToDiagonalDp(ctx, qualifier: DpQualifier.Width)`.

## Português

`dotnet add package AppDimens.Net.Diagonal` → inicialize a janela uma vez → `using AppDimens.Net.Code.Diagonal;` → `16.DGSdp()`, `56.DGHdp()`, `200.DGWdp()`.
Guia completo: [https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.Diagonal/README.md](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.Diagonal/README.md)

---

Apache-2.0 — © Jean Bodenberg · [Repository](https://github.com/bodenberg/appdimens-net-binding)

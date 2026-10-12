# AppDimens.Net.Auto

**Auto-balanced scaling — grows like the Scaled strategy on phones and slows down on tablets and desktops.**

Part of **AppDimens for .NET** (C# port of appdimens-kmp) · requires `AppDimens.Net` (installed automatically).
Works with .NET MAUI, WPF, Windows Forms, WinUI 3 and plain .NET (`net8.0` / `net9.0` / `net10.0`).

📖 **Complete guide:** [AppDimens.Net.Auto documentation](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.Auto/README.md) · **API reference:** [API.md](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.Auto/API.md)

## What it does

The **Auto** strategy follows the screen proportionally on small devices and automatically damps the growth on large ones, so a single number stays comfortable from phones to desktops without breakpoints. If you want "just scale it sensibly" with one call, start here.

| `16` with this strategy | Phone 360×800 | Tablet 800×1280 | Desktop 1280×800 |
|---|--:|--:|--:|
| `16.ASdp()` | 19.2 | 30.2 | 30.2 |
| `16.AHdp()` | 30.2 | 33.9 | 30.2 |
| `16.AWdp()` | 19.2 | 30.2 | 33.9 |
| `16.Sdp()` (core Scaled) | 19.2 | 42.7 | 42.7 |

## Install

```bash
dotnet add package AppDimens.Net.Auto
```

## Use

1. Initialize once for your UI framework: MAUI `AppDimensMaui.AttachWindow(window)` · WPF `AppDimensWpf.Initialize(this)` ·
   WinForms `AppDimensWinForms.Initialize(this)` · WinUI `AppDimensWinUI.Initialize(window)`.
2. Call the extensions:

```csharp
using AppDimens.Net.Code.Auto;

float padding = 16.ASdp();    // smaller side of the window
float header  = 56.AHdp();    // height
float sidebar = 200.AWdp();   // width
float tall    = 16.ASdpa();   // + aspect-ratio correction
float split   = 16.ASdpi();   // ignores split-screen shrinking
```

With an explicit context: `16.ASdp(ctx)`, `16.ASdpPx(ctx)`, `DimenAuto.ASdp(ctx, 16)`, `24.ToAutoDp(ctx, qualifier: DpQualifier.Width)`.

## Português

`dotnet add package AppDimens.Net.Auto` → inicialize a janela uma vez → `using AppDimens.Net.Code.Auto;` → `16.ASdp()`, `56.AHdp()`, `200.AWdp()`.
Guia completo: [https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.Auto/README.md](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.Auto/README.md)

---

Apache-2.0 — © Jean Bodenberg · [Repository](https://github.com/bodenberg/appdimens-net-binding)

# AppDimens.Net.Fill

**Fill scaling — values that expand to occupy their share of the screen.**

Part of **AppDimens for .NET** (C# port of appdimens-kmp) · requires `AppDimens.Net` (installed automatically).
Works with .NET MAUI, WPF, Windows Forms, WinUI 3 and plain .NET (`net8.0` / `net9.0` / `net10.0`).

📖 **Complete guide:** [AppDimens.Net.Fill documentation](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.Fill/README.md) · **API reference:** [API.md](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.Fill/API.md)

## What it does

The **Fill** strategy scales toward filling the available space using the larger screen dimension, so it grows a bit faster than Scaled on phones. Use it for surfaces that should *occupy* space — backgrounds, banners, section headers. All axes give the same result.

| `16` with this strategy | Phone 360×800 | Tablet 800×1280 | Desktop 1280×800 |
|---|--:|--:|--:|
| `16.FLSdp()` | 24 | 42.7 | 42.7 |
| `16.FLHdp()` | 24 | 42.7 | 42.7 |
| `16.FLWdp()` | 24 | 42.7 | 42.7 |
| `16.Sdp()` (core Scaled) | 19.2 | 42.7 | 42.7 |

## Install

```bash
dotnet add package AppDimens.Net.Fill
```

## Use

1. Initialize once for your UI framework: MAUI `AppDimensMaui.AttachWindow(window)` · WPF `AppDimensWpf.Initialize(this)` ·
   WinForms `AppDimensWinForms.Initialize(this)` · WinUI `AppDimensWinUI.Initialize(window)`.
2. Call the extensions:

```csharp
using AppDimens.Net.Code.Fill;

float padding = 16.FLSdp();    // smaller side of the window
float header  = 56.FLHdp();    // height
float sidebar = 200.FLWdp();   // width
float tall    = 16.FLSdpa();   // + aspect-ratio correction
float split   = 16.FLSdpi();   // ignores split-screen shrinking
```

With an explicit context: `16.FLSdp(ctx)`, `16.FLSdpPx(ctx)`, `DimenFill.FLSdp(ctx, 16)`, `24.ToFillDp(ctx, qualifier: DpQualifier.Width)`.

## Português

`dotnet add package AppDimens.Net.Fill` → inicialize a janela uma vez → `using AppDimens.Net.Code.Fill;` → `16.FLSdp()`, `56.FLHdp()`, `200.FLWdp()`.
Guia completo: [https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.Fill/README.md](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.Fill/README.md)

---

Apache-2.0 — © Jean Bodenberg · [Repository](https://github.com/bodenberg/appdimens-net-binding)

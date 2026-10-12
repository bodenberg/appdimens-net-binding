# AppDimens.Net.Logarithmic

**Logarithmic scaling — fast growth on small screens, gentle growth on big ones.**

Part of **AppDimens for .NET** (C# port of appdimens-kmp) · requires `AppDimens.Net` (installed automatically).
Works with .NET MAUI, WPF, Windows Forms, WinUI 3 and plain .NET (`net8.0` / `net9.0` / `net10.0`).

📖 **Complete guide:** [AppDimens.Net.Logarithmic documentation](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.Logarithmic/README.md) · **API reference:** [API.md](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.Logarithmic/API.md)

## What it does

The **Logarithmic** strategy grows along a log curve: small phones get real relief, while tablets and desktops grow only a little — paddings and fonts don't explode on huge screens.

| `16` with this strategy | Phone 360×800 | Tablet 800×1280 | Desktop 1280×800 |
|---|--:|--:|--:|
| `16.LOGSdp()` | 17.2 | 22.3 | 22.3 |
| `16.LOGHdp()` | 17.2 | 22.3 | 22.3 |
| `16.LOGWdp()` | 17.2 | 22.3 | 22.3 |
| `16.Sdp()` (core Scaled) | 19.2 | 42.7 | 42.7 |

## Install

```bash
dotnet add package AppDimens.Net.Logarithmic
```

## Use

1. Initialize once for your UI framework: MAUI `AppDimensMaui.AttachWindow(window)` · WPF `AppDimensWpf.Initialize(this)` ·
   WinForms `AppDimensWinForms.Initialize(this)` · WinUI `AppDimensWinUI.Initialize(window)`.
2. Call the extensions:

```csharp
using AppDimens.Net.Code.Logarithmic;

float padding = 16.LOGSdp();    // smaller side of the window
float header  = 56.LOGHdp();    // height
float sidebar = 200.LOGWdp();   // width
float tall    = 16.LOGSdpa();   // + aspect-ratio correction
float split   = 16.LOGSdpi();   // ignores split-screen shrinking
```

With an explicit context: `16.LOGSdp(ctx)`, `16.LOGSdpPx(ctx)`, `DimenLogarithmic.LOGSdp(ctx, 16)`, `24.ToLogarithmicDp(ctx, qualifier: DpQualifier.Width)`.

## Português

`dotnet add package AppDimens.Net.Logarithmic` → inicialize a janela uma vez → `using AppDimens.Net.Code.Logarithmic;` → `16.LOGSdp()`, `56.LOGHdp()`, `200.LOGWdp()`.
Guia completo: [https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.Logarithmic/README.md](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.Logarithmic/README.md)

---

Apache-2.0 — © Jean Bodenberg · [Repository](https://github.com/bodenberg/appdimens-net-binding)

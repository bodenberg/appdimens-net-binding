# AppDimens.Net.Interpolated

**Interpolated scaling — predictable linear steps between a phone anchor and a tablet anchor.**

Part of **AppDimens for .NET** (C# port of appdimens-kmp) · requires `AppDimens.Net` (installed automatically).
Works with .NET MAUI, WPF, Windows Forms, WinUI 3 and plain .NET (`net8.0` / `net9.0` / `net10.0`).

📖 **Complete guide:** [AppDimens.Net.Interpolated documentation](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.Interpolated/README.md) · **API reference:** [API.md](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.Interpolated/API.md)

## What it does

The **Interpolated** strategy maps the screen onto a straight line between a small-screen and a large-screen anchor: compact on phones, roomier on tablets, with linear (easy to predict) steps in between — handy when a designer specifies a "phone value" and a "tablet value".

| `16` with this strategy | Phone 360×800 | Tablet 800×1280 | Desktop 1280×800 |
|---|--:|--:|--:|
| `16.ISdp()` | 17.6 | 29.3 | 29.3 |
| `16.IHdp()` | 17.6 | 29.3 | 29.3 |
| `16.IWdp()` | 17.6 | 29.3 | 29.3 |
| `16.Sdp()` (core Scaled) | 19.2 | 42.7 | 42.7 |

## Install

```bash
dotnet add package AppDimens.Net.Interpolated
```

## Use

1. Initialize once for your UI framework: MAUI `AppDimensMaui.AttachWindow(window)` · WPF `AppDimensWpf.Initialize(this)` ·
   WinForms `AppDimensWinForms.Initialize(this)` · WinUI `AppDimensWinUI.Initialize(window)`.
2. Call the extensions:

```csharp
using AppDimens.Net.Code.Interpolated;

float padding = 16.ISdp();    // smaller side of the window
float header  = 56.IHdp();    // height
float sidebar = 200.IWdp();   // width
float tall    = 16.ISdpa();   // + aspect-ratio correction
float split   = 16.ISdpi();   // ignores split-screen shrinking
```

With an explicit context: `16.ISdp(ctx)`, `16.ISdpPx(ctx)`, `DimenInterpolated.ISdp(ctx, 16)`, `24.ToInterpolatedDp(ctx, qualifier: DpQualifier.Width)`.

## Português

`dotnet add package AppDimens.Net.Interpolated` → inicialize a janela uma vez → `using AppDimens.Net.Code.Interpolated;` → `16.ISdp()`, `56.IHdp()`, `200.IWdp()`.
Guia completo: [https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.Interpolated/README.md](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.Interpolated/README.md)

---

Apache-2.0 — © Jean Bodenberg · [Repository](https://github.com/bodenberg/appdimens-net-binding)

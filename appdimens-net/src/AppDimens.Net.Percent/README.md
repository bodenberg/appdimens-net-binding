# AppDimens.Net.Percent

**Percent scaling — strictly proportional to an axis, plus Space helpers for literal percentages.**

Part of **AppDimens for .NET** (C# port of appdimens-kmp) · requires `AppDimens.Net` (installed automatically).
Works with .NET MAUI, WPF, Windows Forms, WinUI 3 and plain .NET (`net8.0` / `net9.0` / `net10.0`).

📖 **Complete guide:** [AppDimens.Net.Percent documentation](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.Percent/README.md) · **API reference:** [API.md](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.Percent/API.md)

## What it does

The **Percent** strategy scales **linearly** with the chosen axis (`value × axis ÷ 300`): `PSdp` follows the smaller side, `PHdp` the height and `PWdp` the width. It also ships the **Space helpers** for literal percentages of the window: `50.SpaceWDp(ctx)` = 50 % of the width, `50.SpaceHDp(ctx)` = 50 % of the height, `25.SpaceSwDp(ctx)` = 25 % of the smaller side.

| `16` with this strategy | Phone 360×800 | Tablet 800×1280 | Desktop 1280×800 |
|---|--:|--:|--:|
| `16.PSdp()` | 19.2 | 42.7 | 42.7 |
| `16.PHdp()` | 42.7 | 68.3 | 42.7 |
| `16.PWdp()` | 19.2 | 42.7 | 68.3 |
| `16.Sdp()` (core Scaled) | 19.2 | 42.7 | 42.7 |

## Install

```bash
dotnet add package AppDimens.Net.Percent
```

## Use

1. Initialize once for your UI framework: MAUI `AppDimensMaui.AttachWindow(window)` · WPF `AppDimensWpf.Initialize(this)` ·
   WinForms `AppDimensWinForms.Initialize(this)` · WinUI `AppDimensWinUI.Initialize(window)`.
2. Call the extensions:

```csharp
using AppDimens.Net.Code.Percent;

float padding = 16.PSdp();    // smaller side of the window
float header  = 56.PHdp();    // height
float sidebar = 200.PWdp();   // width
float tall    = 16.PSdpa();   // + aspect-ratio correction
float split   = 16.PSdpi();   // ignores split-screen shrinking
```

With an explicit context: `16.PSdp(ctx)`, `16.PSdpPx(ctx)`, `DimenPercent.PSdp(ctx, 16)`, `24.ToPercentDp(ctx, qualifier: DpQualifier.Width)`.

## Português

`dotnet add package AppDimens.Net.Percent` → inicialize a janela uma vez → `using AppDimens.Net.Code.Percent;` → `16.PSdp()`, `56.PHdp()`, `200.PWdp()`.
Guia completo: [https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.Percent/README.md](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.Percent/README.md)

---

Apache-2.0 — © Jean Bodenberg · [Repository](https://github.com/bodenberg/appdimens-net-binding)

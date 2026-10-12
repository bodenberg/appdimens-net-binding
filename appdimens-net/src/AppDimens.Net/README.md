# AppDimens.Net

**Responsive dimensions for .NET — the core of AppDimens for .NET.** Scaled dp/sp (`16.Sdp()`, `14.Ssp()`),
orientation inverters, conditional facilitators, builders, snapshot cache with allocation-free fast lanes, an
event-driven resize watcher, `HostDimensScope` for any UI host and `FakeAppDimensContext` for tests.
Bit-for-bit port of [appdimens-kmp](https://github.com/bodenberg/appdimens-kmp).

📖 **Complete guide:** [AppDimens.Net documentation](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net/README.md) ·
**API reference:** [API.md](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net/API.md) ·
**All packages:** [docs index](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/README.md)

| | |
|---|---|
| **Targets** | `net8.0` · `net9.0` · `net10.0` (+ `net11.0` with SDK 11) |
| **Dependencies** | none |
| **UI frameworks** | .NET MAUI (`AppDimens.Net.Maui`), WPF (`AppDimens.Net.Wpf`), Windows Forms (`AppDimens.Net.WinForms`), WinUI 3 (`AppDimens.Net.WinUI`), any other host (`HostDimensScope`) |

## Why?

Fixed sizes look right on one screen only. AppDimens treats your numbers as sizes on a **300 dp-wide reference
screen** and scales them to the real window: `16.Sdp()` is 19.2 on a 360 dp phone, 32 on 600 dp, 48 on 900 dp.

## Install

```bash
dotnet add package AppDimens.Net
# + your UI framework integration, e.g.
dotnet add package AppDimens.Net.Maui     # or .Wpf / .WinForms / .WinUI
```

## Start (one line per framework)

| Framework | Call once |
|-----------|-----------|
| .NET MAUI | `AppDimensMaui.AttachWindow(window)` in `App.CreateWindow` |
| WPF | `AppDimensWpf.Initialize(this)` before `InitializeComponent()` |
| Windows Forms | `AppDimensWinForms.Initialize(this)` |
| WinUI 3 | `AppDimensWinUI.Initialize(window)` in `OnLaunched` |
| Any other host | `AppDimensHost.Activate(new HostDimensScope()).Update(widthDp, heightDp, density)` |

## Use

```csharp
using AppDimens.Net.Code.Scaled;

float pad    = 16.Sdp();    // smallest width — default for padding, margins, icons
float header = 56.Hdp();    // height axis
float side   = 200.Wdp();   // width axis
float text   = 14.Ssp();    // text, follows the user's font scale
float tall   = 16.Sdpa();   // + aspect-ratio correction
float split  = 16.Sdpi();   // ignores split-screen shrinking

float tablet = 16.ScaledDp()
    .Screen(AppDimens.Net.Common.DpQualifier.SmallWidth, 600, 24)   // sw ≥ 600 → 24
    .Sdp();
```

## Test

```csharp
using AppDimens.Net.Code.Scaled;
using AppDimens.Net.Testing;

var ctx = new FakeAppDimensContext();
ctx.SetConfig(360, 800);
float phone = 16.Sdp(ctx);   // 19.2
```

## Português

`dotnet add package AppDimens.Net` + o pacote do seu framework → inicialize a janela uma vez → `16.Sdp()` (tamanhos),
`14.Ssp()` (texto), `56.Hdp()` / `200.Wdp()` (altura/largura). Guia completo:
[docs/AppDimens.Net](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net/README.md).

---

Apache-2.0 — © Jean Bodenberg · [Repository](https://github.com/bodenberg/appdimens-net-binding)

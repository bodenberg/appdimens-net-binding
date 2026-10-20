# AppDimens .NET — MAUI · WPF · Windows Forms · WinUI 3 · Android · iOS

**AppDimens for .NET** — a complete C# port of the
[AppDimens KMP/Dynamic](https://github.com/bodenberg/appdimens-kmp) scaling library.
12+ scaling strategies, bit-for-bit parity with the Kotlin/Compose formulas, an
allocation-free fast lane and an event-driven resize watcher — now with first-class
**WPF**, **Windows Forms** and **WinUI 3** integrations.

> **New in 3.6.1:** `AppDimens.Net.Wpf`, `AppDimens.Net.WinForms`, `AppDimens.Net.WinUI` and the reusable
> `HostDimensScope` for any other UI host. Repository: https://github.com/bodenberg/appdimens-net-binding

---

## Which package do I install?

| Your app | Install | Then call (once) |
|----------|---------|------------------|
| **.NET MAUI** (Android, iOS, Windows, macOS) | `AppDimens.Net.Maui` (+ `AppDimens.Net.Sdk` for every strategy) | `AppDimensMaui.AttachWindow(window)` |
| **WPF** | `AppDimens.Net.Wpf` | `AppDimensWpf.Initialize(this)` |
| **Windows Forms** | `AppDimens.Net.WinForms` | `AppDimensWinForms.Initialize(this)` |
| **WinUI 3** | `AppDimens.Net.WinUI` | `AppDimensWinUI.Initialize(window)` |
| Anything else (Avalonia, Uno, console, tests, game loop) | `AppDimens.Net` | `AppDimensHost.Activate(new HostDimensScope())` + `scope.Update(w, h, density)` |

After that, the same C# works everywhere:

```csharp
using AppDimens.Net.Code.Scaled;

float padding = 16.Sdp();   // margins, padding, icons — most cases
float header  = 56.Hdp();   // follows the window height
float sidebar = 200.Wdp();  // follows the window width
float title   = 22.Ssp();   // text size
```

📘 **New here?** Read the [Beginner's guide](DOCUMENTATION/GUIDE-FOR-BEGINNERS.md) — step by step for each framework.

---

## Modules

| NuGet package | What it gives you |
|---|---|
| `AppDimens.Net` | Core + **Scaled** strategy (`Sdp/Sdpa/Hdp/Wdp/Ssp`, inverters, facilitators), cache, `HostDimensScope`, `FakeAppDimensContext` |
| `AppDimens.Net.Percent` | Percent (`PSdp`, `SpaceWDp`…) |
| `AppDimens.Net.Power` | Power (`PWSdp`) |
| `AppDimens.Net.Auto` | Auto (`ASdp`) |
| `AppDimens.Net.Logarithmic` | Logarithmic (`LOGSdp`) |
| `AppDimens.Net.Fluid` | Fluid (`FSdp`, `ToFluidDp`) |
| `AppDimens.Net.Interpolated` | Interpolated (`ISdp`) |
| `AppDimens.Net.Diagonal` | Diagonal (`DGSdp`) |
| `AppDimens.Net.Perimeter` | Perimeter (`PRSdp`) |
| `AppDimens.Net.Fit` / `.Fill` | Fit (`ToFitDp`, `DimenFit` builder) / Fill (`ToFillDp`) |
| `AppDimens.Net.Density` | Density (`DSdp`) |
| `AppDimens.Net.Resize` | Auto-resize math (`AutoResizeTextPx`, `AutoResizeSquarePx`) |
| `AppDimens.Net.Units` | Physical units (mm / cm / inch) |
| `AppDimens.Net.Custom` *(unreleased)* | Custom strategies (`Cdp` / `Csp`): builder + JSON, rules per device/screen, free-form formulas, presets |
| `AppDimens.Net.Sdk` | Meta-package: core + every strategy |
| `AppDimens.Net.Maui` | .NET MAUI: window scope, `{appdimens:Sdp Value=16}` markup, converters |
| `AppDimens.Net.Wpf` | WPF: window scope, **live** `{ad:Sdp 16}` markup (scaled/fluid/percent), converters |
| `AppDimens.Net.WinForms` | Windows Forms: form scope, `Apply(...)` re-layout helper, `DimenPx` px/pt converters |
| `AppDimens.Net.WinUI` | WinUI 3: window scope, `{ad:Sdp Value=16}` markup re-applied on resize, converters |

## Quick start per framework

### .NET MAUI

```csharp
using AppDimens.Net.Maui.Platform;

protected override Window CreateWindow(IActivationState? s)
{
    var window = new Window();
    AppDimensMaui.AttachWindow(window);      // attach BEFORE creating pages
    window.Page = new NavigationPage(new MainPage());
    return window;
}
```

### WPF

```csharp
public MainWindow()
{
    AppDimens.Net.Wpf.AppDimensWpf.Initialize(this);
    InitializeComponent();
}
```

```xml
<Window xmlns:ad="clr-namespace:AppDimens.Net.Wpf;assembly=AppDimens.Net.Wpf" ...>
    <Button Width="{ad:Sdp 120}" Margin="{ad:Sdp 16}" FontSize="{ad:Ssp 14}" />
</Window>
```

### Windows Forms

```csharp
using AppDimens.Net.Code.Scaled;
using AppDimens.Net.WinForms;

AppDimensWinForms.Initialize(this);
AppDimensWinForms.Apply(this, () =>
{
    button1.Width = DimenPx.ToPx(120.Sdp());
    label1.Font   = new Font(label1.Font.FontFamily, DimenPx.ToPt(14.Ssp()));
});
```

### WinUI 3

```csharp
m_window = new MainWindow();
AppDimens.Net.WinUI.AppDimensWinUI.Initialize(m_window);
m_window.Activate();
```

```xml
<Page xmlns:ad="using:AppDimens.Net.WinUI" ...>
    <Button Width="{ad:Sdp Value=120}" Margin="{ad:Sdp Value=16}" FontSize="{ad:Ssp Value=14}" />
</Page>
```

### Custom host

```csharp
using AppDimens.Net.Code.Scaled;
using AppDimens.Net.Core;

var scope = AppDimensHost.Activate(new HostDimensScope());
scope.Update(widthDp: 1280, heightDp: 720, density: 1.5);   // call again from your resize handler
float pad = 16.Sdp();
```

## Demo & BenchLab apps

* `samples/AppDimens.Sample` — full KMP-parity demo screen (MAUI).
* `benchlab/AppDimens.BenchLab` — benchmark dashboard (MAUI).
* `samples/AppDimens.WebDemo` — Blazor WebAssembly self-test suite.

## Build, test & pack

```bash
dotnet test tests/AppDimens.Net.Tests                  # cross-platform tests
dotnet test tests/AppDimens.Net.Desktop.Tests          # Windows only (WPF + WinForms)
./scripts/pack-nuget.sh                                # every package → artifacts/nuget
```

Desktop adapters compile on Linux/macOS (`EnableWindowsTargeting`) but run only on Windows.

## Documentation

* [`GUIDE-FOR-BEGINNERS.md`](DOCUMENTATION/GUIDE-FOR-BEGINNERS.md)
* [`MODULES.md`](DOCUMENTATION/MODULES.md) · [`PARITY.md`](DOCUMENTATION/PARITY.md) ·
  [`PERFORMANCE.md`](DOCUMENTATION/PERFORMANCE.md) · [`TRIMMING-AOT.md`](DOCUMENTATION/TRIMMING-AOT.md)
* [`LLMS.txt`](LLMS.txt) — LLM-friendly API digest

## License

Apache-2.0 — © Jean Bodenberg

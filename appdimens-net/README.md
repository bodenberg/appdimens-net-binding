# AppDimens .NET — Android / iOS / MAUI

**AppDimens for .NET (MAUI)** — a complete C# port of the
[AppDimens KMP/Dynamic](https://github.com/bodenberg/appdimens-kmp) scaling library.
12 scaling strategies, bit-a-bit parity with the Kotlin/Compose formulas, an
allocation-free fast lane, an event-driven resize watcher, and Material-3 demo +
benchmark apps.

> GIT: https://github.com/bodenberg/appdimens-net-binding

## Modules

| NuGet package | Strategy | KMP counterpart |
|---|---|---|
| `AppDimens.Net` | Scaled (sdp/sdpa/hdp/wdp + inverters + facilitators) | `compose` / `android` core |
| `AppDimens.Net.Percent` | Percent (`PSdp`, `SpaceWDp`…) | `compose.percent` |
| `AppDimens.Net.Power` | Power (`PWSdp`) | `compose.power` |
| `AppDimens.Net.Auto` | Auto (`ASdp`) | `compose.auto` |
| `AppDimens.Net.Logarithmic` | Logarithmic (`LOGSdp`) | `compose.logarithmic` |
| `AppDimens.Net.Fluid` | Fluid (`FSdp`, `ToFluidDp`) | `compose.fluid` |
| `AppDimens.Net.Interpolated` | Interpolated (`ISdp`) | `compose.interpolated` |
| `AppDimens.Net.Diagonal` | Diagonal (`DGSdp`) | `compose.diagonal` |
| `AppDimens.Net.Perimeter` | Perimeter (`PRSdp`) | `compose.perimeter` |
| `AppDimens.Net.Fit` | Fit (`ToFitDp`, `DimenFit` builder) | `compose.fit` |
| `AppDimens.Net.Fill` | Fill (`ToFillDp`) | `compose.fill` |
| `AppDimens.Net.Density` | Density (`DSdp`) | `compose.density` |
| `AppDimens.Net.Resize` | Auto-resize math (`AutoResizeTextPx`, `AutoResizeSquarePx`) | `resize` |
| `AppDimens.Net.Units` | Physical units (mm/cm/inch) | units module |
| `AppDimens.Net.Maui` | MAUI bootstrap: `AppDimensMaui.AttachWindow(window)` + ambient scope | — |
| `AppDimens.Net.Sdk` | Meta-package with all satellites | — |
| `AppDimens.Net.Testing` | `FakeAppDimensContext` for tests/benchmarks/design-time | test fixtures |

## Quick start (MAUI)

```csharp
// App.xaml.cs or MauiProgram window creation
protected override Window CreateWindow(IActivationState? s)
{
    var window = new Window();
    AppDimensMaui.AttachWindow(window);      // 1. attach BEFORE creating pages
    window.Page = new NavigationPage(new MainPage());
    return window;
}

// Anywhere in the UI — parameterless extensions use the ambient context:
float pad = 16.Sdp();          // scaled by smallest-width (fast lane, zero alloc)
float h   = 48.Hdp();          // height axis
float sp  = 16.Ssp();          // scaled text size (font-scale aware)
float pct = 24.PSdp(ctx);      // Percent strategy (explicit context)
```

**Important:** call `AppDimensMaui.AttachWindow(window)` **before** constructing any
page that uses the parameterless extensions — they resolve through
`AppDimensAmbient.Require()`.

## Demo & BenchLab apps

* `samples/AppDimens.Sample` — full KMP-parity demo screen (strategy selector,
  sections 1–5, live metrics, auto-resize playground).
* `benchlab/AppDimens.BenchLab` — dark-theme benchmark dashboard (KMP BenchlabScreen
  parity): fast lanes vs raw multiply vs cached path vs legacy XML-grid lookup,
  chunked run with reentrancy guard.
* `samples/AppDimens.WebDemo` — Blazor WebAssembly self-test suite that runs the
  parity/invariant checks inside a real browser.

## Build & test (Linux)

```bash
dotnet test tests/AppDimens.Net.Tests        # 48/48 green
dotnet build AppDimens.Net.slnx              # solution (libs net10.0/net8.0; heads android on Linux)
dotnet publish samples/AppDimens.Sample/AppDimens.Sample/AppDimens.Sample.csproj \
    -c Release -f net10.0-android36.1        # APK → bin/Release/net10.0-android36.1/publish/
```

On Windows/macOS the heads additionally target `net8.0-android`, `net8.0-ios`,
`net8.0-maccatalyst`, `net8.0-windows10.0.19041.0` (see csproj conditions).

## Documentation

* [`GUIDE-FOR-BEGINNERS.md`](DOCUMENTATION/GUIDE-FOR-BEGINNERS.md)
* [`DOCUMENTATION/MODULES.md`](DOCUMENTATION/MODULES.md) ·
  [`DOCUMENTATION/PARITY.md`](DOCUMENTATION/PARITY.md) ·
  [`DOCUMENTATION/PERFORMANCE.md`](DOCUMENTATION/PERFORMANCE.md) ·
  [`DOCUMENTATION/TRIMMING-AOT.md`](DOCUMENTATION/TRIMMING-AOT.md)
* [`LLMS.txt`](LLMS.txt) — LLM-friendly API digest
* [`CHANGELOG.md`](CHANGELOG.md)

## License

Apache-2.0 — © Jean Bodenberg

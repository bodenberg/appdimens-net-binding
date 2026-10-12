# AppDimens .NET

Monorepo of the **AppDimens** responsive-dimension ecosystem for .NET: **.NET MAUI**, **WPF**, **Windows Forms**,
**WinUI 3** and **.NET for Android**. Write `16` the way you always do — AppDimens scales it to every phone, tablet,
foldable, TV and desktop window.

* **Native .NET libraries** (any OS, any UI framework): the **SDPS family** (`Bodenberg.AppDimens.Maui.Sdps` and its
  WPF / WinForms / WinUI adapters — Android SDP parity) and the **AppDimens.Net family** (complete C# port of
  appdimens-kmp, 12+ strategies, with MAUI / WPF / WinForms / WinUI integrations).
* **Android bindings** (`net10.0-android`) embedding the upstream Kotlin AARs **3.2.0**: `Bodenberg.AppDimens.Sdps`,
  `.Ssps`, `.Dynamic`.

📖 **Documentation for every package (guides + complete API reference): [docs/](docs/README.md)** ·
🇧🇷 [Resumo em português](#resumo-em-português)

---

## Which package do I need?

| I am building… | Install | Docs |
|----------------|---------|------|
| **.NET MAUI** app with `{sdp:16}` XAML and Android-style buckets | `Bodenberg.AppDimens.Maui.Sdps` | [guide](docs/Bodenberg.AppDimens.Maui.Sdps/README.md) |
| **.NET MAUI** app with the KMP strategies (fluid, percent, auto…) | `AppDimens.Net.Maui` (+ `AppDimens.Net.Sdk`) | [guide](docs/AppDimens.Net.Maui/README.md) |
| **WPF** app | `Bodenberg.AppDimens.Sdps.Wpf` or `AppDimens.Net.Wpf` | [SDPS](docs/Bodenberg.AppDimens.Sdps.Wpf/README.md) · [AppDimens.Net](docs/AppDimens.Net.Wpf/README.md) |
| **Windows Forms** app | `Bodenberg.AppDimens.Sdps.WinForms` or `AppDimens.Net.WinForms` | [SDPS](docs/Bodenberg.AppDimens.Sdps.WinForms/README.md) · [AppDimens.Net](docs/AppDimens.Net.WinForms/README.md) |
| **WinUI 3** app | `Bodenberg.AppDimens.Sdps.WinUI` or `AppDimens.Net.WinUI` | [SDPS](docs/Bodenberg.AppDimens.Sdps.WinUI/README.md) · [AppDimens.Net](docs/AppDimens.Net.WinUI/README.md) |
| Native **.NET for Android** app, XML `@dimen/_16sdp` (layout + text) | `Bodenberg.AppDimens.Sdps` | [guide](docs/Bodenberg.AppDimens.Sdps/README.md) |
| Native **.NET for Android**, text only `@dimen/_14ssp` | `Bodenberg.AppDimens.Ssps` | [guide](docs/Bodenberg.AppDimens.Ssps/README.md) |
| Native **.NET for Android**, code-only, 15 strategies | `Bodenberg.AppDimens.Dynamic` | [guide](docs/Bodenberg.AppDimens.Dynamic/README.md) |
| Console / Avalonia / Uno / game / tests | `AppDimens.Net` (`HostDimensScope`) | [guide](docs/AppDimens.Net/README.md) |

**SDPS or AppDimens.Net?** Both give the same `Sdp`/`Ssp` results. **SDPS** = Android SDP parity (pre-computed buckets,
`{sdp:16}` short markup, frozen `*i` values). **AppDimens.Net** = KMP parity with 12+ strategies.

## Framework compatibility

| Package | MAUI | WPF | WinForms | WinUI 3 | .NET Android | Target frameworks |
|---------|:----:|:---:|:--------:|:-------:|:------------:|-------------------|
| `Bodenberg.AppDimens.Maui.Sdps` | ✅ | — | — | — | ✅ (via MAUI) | `net8.0`, `net9.0`, `net10.0` (+`net11.0`) |
| `Bodenberg.AppDimens.Sdps.Wpf` | — | ✅ | — | — | — | `net8.0/9.0/10.0-windows` |
| `Bodenberg.AppDimens.Sdps.WinForms` | — | — | ✅ | — | — | `net8.0/9.0/10.0-windows` |
| `Bodenberg.AppDimens.Sdps.WinUI` | — | — | — | ✅ | — | `net8.0/9.0/10.0-windows10.0.19041.0` |
| `AppDimens.Net` + strategies | ✅ | ✅ | ✅ | ✅ | ✅ | `net8.0`, `net9.0`, `net10.0` (+`net11.0`) |
| `AppDimens.Net.Maui` | ✅ | — | — | — | ✅ (via MAUI) | `net8.0`, `net9.0`, `net10.0` |
| `AppDimens.Net.Wpf` / `.WinForms` / `.WinUI` | — | ✅ (`.Wpf`) | ✅ (`.WinForms`) | ✅ (`.WinUI`) | — | `-windows` / `-windows10.0.19041.0` |
| `Bodenberg.AppDimens.Sdps` / `.Ssps` / `.Dynamic` | Android head only | — | — | — | ✅ | `net10.0-android36.0` |

> The Android bindings wrap native Kotlin libraries, so they can only run on Android. For Windows desktop use the
> native packages.

---

## Packages and versions

All packages ship as **3.6.1**.

| NuGet package | Folder | Notes |
|---------------|--------|-------|
| [Bodenberg.AppDimens.Maui.Sdps](https://www.nuget.org/packages/Bodenberg.AppDimens.Maui.Sdps) | [`appdimens-sdps-net/`](appdimens-sdps-net/) | MAUI meta-package (engine is MAUI-free since 3.6.1) |
| [Bodenberg.AppDimens.Sdps.Wpf](https://www.nuget.org/packages/Bodenberg.AppDimens.Sdps.Wpf) · [.WinForms](https://www.nuget.org/packages/Bodenberg.AppDimens.Sdps.WinForms) · [.WinUI](https://www.nuget.org/packages/Bodenberg.AppDimens.Sdps.WinUI) | [`appdimens-sdps-net/src/`](appdimens-sdps-net/src/) | **new in 3.6.1** — same engine, no MAUI dependency |
| [AppDimens.Net.Sdk](https://www.nuget.org/packages/AppDimens.Net.Sdk) (+ core and 13 satellites) | [`appdimens-net/`](appdimens-net/) | C# port of [appdimens-kmp](https://github.com/bodenberg/appdimens-kmp) |
| [AppDimens.Net.Wpf](https://www.nuget.org/packages/AppDimens.Net.Wpf) · [.WinForms](https://www.nuget.org/packages/AppDimens.Net.WinForms) · [.WinUI](https://www.nuget.org/packages/AppDimens.Net.WinUI) | [`appdimens-net/src/`](appdimens-net/src/) | **new in 3.6.1** |
| [Bodenberg.AppDimens.Sdps](https://www.nuget.org/packages/Bodenberg.AppDimens.Sdps) | [`appdimens-sdps-net-binding/`](appdimens-sdps-net-binding/) | AAR `appdimens-sdps` **3.2.0** |
| [Bodenberg.AppDimens.Ssps](https://www.nuget.org/packages/Bodenberg.AppDimens.Ssps) | [`appdimens-ssps-net-binding/`](appdimens-ssps-net-binding/) | AAR `appdimens-ssps` **3.2.0** |
| [Bodenberg.AppDimens.Dynamic](https://www.nuget.org/packages/Bodenberg.AppDimens.Dynamic) | [`appdimens-dynamic-net-binding/`](appdimens-dynamic-net-binding/) | AARs `appdimens-dynamic` + 13 modules **3.2.0** |

```bash
dotnet add package Bodenberg.AppDimens.Maui.Sdps --version 3.6.1   # MAUI
dotnet add package Bodenberg.AppDimens.Sdps.Wpf --version 3.6.1    # WPF
dotnet add package AppDimens.Net.Sdk --version 3.6.1               # all AppDimens.Net strategies
dotnet add package Bodenberg.AppDimens.Sdps --version 3.6.1        # Android binding
```

### What's new in 3.6.1

* **WPF, Windows Forms and WinUI 3 support** for both native families (6 new packages).
* `Bodenberg.AppDimens.Maui.Sdps`: host-agnostic resolver (`IDisplayMetricsSource`, `UseDisplaySource`), MAUI glue moved
  to `AppDimens.Maui.Hosting` (same namespace), `DimenFunction` enum.
* `AppDimens.Net`: `HostDimensScope` / `AppDimensHost` for any UI host.
* Android bindings embed the upstream AARs **3.2.0** (public API identical to 3.1.x).
* Complete per-package documentation in [`docs/`](docs/README.md), with compile-checked samples.

---

## 30-second tour

```csharp
// .NET MAUI (Bodenberg.AppDimens.Maui.Sdps)
builder.UseAppDimensSdps();                    // MauiProgram.cs
// XAML: Padding="{sdp:16}" FontSize="{ssp:14}"   C#: 16.Sdp(), 14.Ssp()

// WPF (Bodenberg.AppDimens.Sdps.Wpf)
AppDimensWpf.Initialize(this);                 // MainWindow constructor
// XAML: Margin="{sdps:Sdp 16}" FontSize="{sdps:Ssp 14}"  (live on resize)

// Windows Forms (Bodenberg.AppDimens.Sdps.WinForms)
AppDimensWinForms.Initialize(this);
AppDimensWinForms.Apply(this, () => button1.Width = 120.SdpPx());

// WinUI 3 (Bodenberg.AppDimens.Sdps.WinUI)
AppDimensWinUI.Initialize(m_window);           // App.OnLaunched
// XAML: Margin="{sdps:Sdp Value=16}"

// .NET for Android (Bodenberg.AppDimens.Sdps)
// XML: android:padding="@dimen/_16sdp"     C#: DimenSdp.Sdp(this, 16) → pixels
```

---

## Repository layout

```
appdimens-net-binding/
├── docs/                              # Per-package guides + generated API references
├── appdimens-sdps-net/                # SDPS native: MAUI + WPF/WinForms/WinUI adapters (src/ tests/ samples/ scripts/)
├── appdimens-net/                     # AppDimens.Net (KMP port): core, strategies, MAUI/WPF/WinForms/WinUI
├── appdimens-sdps-net-binding/        # Android binding (Binding/, SmokeTest/, scripts/sync-aar-from-maven.sh)
├── appdimens-ssps-net-binding/        # same structure
├── appdimens-dynamic-net-binding/     # same structure
├── tools/                             # ApiDocGen (.NET API docs), aar-api-doc.py, sync-guides.py
├── scripts/generate-docs.sh           # Regenerates docs/*/API.md and mirrored guides
├── build-bindings.sh                  # Builds the three bindings (Release, net10.0-android36.0)
└── skill/SKILL.md                     # Maintainer / agent guide
```

---

## Requirements (to build this repository)

| Requirement | Notes |
|-------------|-------|
| **.NET SDK** | **10** (`global.json` per folder); SDK 8/9 runtimes to run the multi-target tests |
| **Native libraries** | build on Windows, Linux or macOS (`EnableWindowsTargeting` compiles WPF/WinForms/WinUI everywhere; desktop tests run on Windows) |
| **Android bindings** | Android workload (`dotnet workload install android`), JDK **17 or 21**, Android SDK platform **36+**, minimum API **24** |
| **Python 3** | `appdimens-sdps-net/scripts/generate-dimens.py` (bucket tables) and doc tools |

## Build, test and pack

```bash
# SDPS native (MAUI meta-package + WPF/WinForms/WinUI adapters): tests + pack + checks
cd appdimens-sdps-net && python3 scripts/generate-dimens.py && ./scripts/publish-nuget.sh

# AppDimens.Net family: tests + pack every package
cd appdimens-net && ./scripts/pack-nuget.sh

# Desktop adapter tests (Windows only)
dotnet test appdimens-sdps-net/tests/AppDimens.Sdps.Desktop.Tests
dotnet test appdimens-net/tests/AppDimens.Net.Desktop.Tests

# Android bindings
./build-bindings.sh
cd appdimens-sdps-net-binding && ./scripts/sync-aar-from-maven.sh 3.2.0   # refresh AAR + .pom/.module/-javadoc.jar

# Documentation
./scripts/generate-docs.sh
```

Packages land in `*/artifacts/nuget/` (ignored by git).

## CI / Release

* **CI** (`.github/workflows/ci.yml`, push/PR to `main` or manual): SDPS tests + pack verification, AppDimens.Net
  tests + pack, desktop tests on `windows-latest`, Android bindings for `net8/9/10-android`.
* **Release** (`.github/workflows/release.yml`, manual `workflow_dispatch`): builds, tests and packs **every** package,
  uploads them as the `nuget-packages` artifact and, with `push_nuget = true`, pushes them to NuGet.org
  (secret `NUGET_API_KEY`).

---

## Links

| Resource | URL |
|----------|-----|
| AppDimens (main project) | https://github.com/bodenberg/appdimens |
| Android SDPS / SSPS / Dynamic | https://github.com/bodenberg/appdimens-sdps · https://github.com/bodenberg/appdimens-ssps · https://github.com/bodenberg/appdimens-dynamic |
| KMP (reference for the AppDimens.Net port) | https://github.com/bodenberg/appdimens-kmp |
| This repository | https://github.com/bodenberg/appdimens-net-binding |

---

## Resumo em português

Este repositório contém o ecossistema **AppDimens para .NET** — dimensões responsivas para **MAUI, WPF, Windows Forms,
WinUI 3 e Android**:

* **Família SDPS** (`Bodenberg.AppDimens.Maui.Sdps` + adaptadores `.Sdps.Wpf`, `.Sdps.WinForms`, `.Sdps.WinUI`):
  paridade com o SDP do Android, `{sdp:16}` no MAUI, `{sdps:Sdp 16}` no WPF/WinUI, `120.SdpPx()` no WinForms.
* **Família AppDimens.Net** (port do appdimens-kmp, 12+ estratégias) com integrações para MAUI, WPF, WinForms e WinUI.
* **Bindings Android** (`Bodenberg.AppDimens.Sdps`, `.Ssps`, `.Dynamic`) com as AARs **3.2.0**.

Todos os pacotes estão na versão **3.6.1**. Para começar, veja a tabela [Which package do I need?](#which-package-do-i-need)
e abra o guia do seu framework em [docs/](docs/README.md) — cada guia é passo a passo, com exemplos completos para cada
caso, e tem a referência completa da API.

## License

**Apache 2.0**, consistent with the upstream AppDimens libraries.

# AppDimens .NET Bindings

Monorepo of **.NET 10 for Android** bindings and **native .NET** libraries for the [AppDimens](https://github.com/bodenberg/appdimens) responsive dimension ecosystem. **Android binding** packages target **`net10.0-android`** and embed upstream **Maven AARs**. Native packages (**`Bodenberg.AppDimens.Maui.Sdps`** and the **`AppDimens.Net.*`** family) target **`net8.0`**, **`net9.0`**, **`net10.0`** (and `net11.0` when built with the .NET 11 SDK), multi-platform.

Android implementation, XML naming, and feature documentation live in the **upstream repositories** listed below. This repository contains **binding projects**, **native ports**, **MSBuild transforms**, **smoke-test apps**, and **publish notes** only.

---

## Packages

| NuGet package | Folder | Android library (source) | Focus |
|---------------|--------|--------------------------|--------|
| [**Bodenberg.AppDimens.Sdps**](https://www.nuget.org/packages/Bodenberg.AppDimens.Sdps) | [`appdimens-sdps-net-binding/`](appdimens-sdps-net-binding/) | [appdimens-sdps](https://github.com/bodenberg/appdimens-sdps) | **Layout + typography** — SDP/HDP/WDP and SSP/HSP/WSP via `@dimen` grids and code APIs |
| [**Bodenberg.AppDimens.Ssps**](https://www.nuget.org/packages/Bodenberg.AppDimens.Ssps) | [`appdimens-ssps-net-binding/`](appdimens-ssps-net-binding/) | [appdimens-ssps](https://github.com/bodenberg/appdimens-ssps) | **Typography only** — SSP/HSP/WSP (smaller if you do not need layout dimens) |
| [**Bodenberg.AppDimens.Dynamic**](https://www.nuget.org/packages/Bodenberg.AppDimens.Dynamic) | [`appdimens-dynamic-net-binding/`](appdimens-dynamic-net-binding/) | [appdimens-dynamic](https://github.com/bodenberg/appdimens-dynamic) | **Code-only** scaling — 15 strategies, no pre-built `@dimen` XML grids |
| [**Bodenberg.AppDimens.Maui.Sdps**](https://www.nuget.org/packages/Bodenberg.AppDimens.Maui.Sdps) | [`appdimens-sdps-net/`](appdimens-sdps-net/) | [appdimens-sdps](https://github.com/bodenberg/appdimens-sdps) | **.NET MAUI nativo** — SDP/HDP/WDP/SSP sem binding Android |
| [**AppDimens.Net.Sdk**](https://www.nuget.org/packages/AppDimens.Net.Sdk) (meta-package + satellites) | [`appdimens-net/`](appdimens-net/) | [appdimens-kmp](https://github.com/bodenberg/appdimens-kmp) | **Port C# completo do AppDimens KMP** — 12+ estratégias, fast lane sem alocação, resize watcher, MAUI multiplataforma |

**Versões NuGet atuais:** todos os pacotes estão na versão **`3.6.0`** (veja cada README para a versão do AAR Maven embutido nos bindings: sdps/ssps **3.1.7**, dynamic **3.1.9**).

```bash
dotnet add package Bodenberg.AppDimens.Sdps --version 3.6.0
dotnet add package Bodenberg.AppDimens.Ssps --version 3.6.0
dotnet add package Bodenberg.AppDimens.Dynamic --version 3.6.0
dotnet add package Bodenberg.AppDimens.Maui.Sdps --version 3.6.0
dotnet add package AppDimens.Net.Sdk --version 3.6.0   # meta-package (AppDimens.Net.*)
```

### Which package should I use?

| Need | Package |
|------|---------|
| Responsive **margins, padding, sizes** and **text** with `@dimen/_16sdp`-style XML (Android binding) | **Sdps** |
| **Text / `sp` only**, smaller dependency | **Ssps** |
| **Runtime strategies** (percent, auto, fluid, …) without thousands of XML dimens (Android binding) | **Dynamic** |
| **MAUI multiplataforma** (Android, iOS, Windows, macOS) com `{sdp:16}` e APIs C# nativas | **Maui.Sdps** |
| **Paridade bit-a-bit com o appdimens-kmp** em C# puro (12+ estratégias, MAUI/Android/iOS, testável com `FakeAppDimensContext`) | **AppDimens.Net.Sdk** |

You can combine packages only when your app design allows overlapping responsibilities; most apps pick **one primary** approach per surface.

---

## Repository layout

Each module is a self-contained tree:

```
appdimens-net-binding/
├── appdimens-net/                     # C# port of AppDimens KMP (AppDimens.Net.*)
│   ├── src/                           # Core + satellite strategy packages
│   ├── samples/, benchlab/, tests/
│   └── DOCUMENTATION/                 # Modules, parity, performance, trimming/AOT
├── appdimens-sdps-net/                # Native MAUI SDP library (net8/9/10[/11])
├── appdimens-sdps-net-binding/
│   ├── AppDimens.Sdps.sln
│   ├── AppDimens.Sdps.Binding/        # NuGet + AAR binding
│   ├── AppDimens.Sdps.SmokeTest/      # Optional compile smoke APK
│   ├── Directory.Build.props          # Android SDK / JDK fallbacks (local builds)
│   └── README.md                      # Consumer documentation (packed into NuGet)
├── appdimens-ssps-net-binding/        # same structure as sdps binding
├── appdimens-dynamic-net-binding/     # same structure as sdps binding
└── build-bindings.sh                  # Builds all three bindings (Release)
```

| Project | Role |
|---------|------|
| `*.Binding` | Library binding project — produces `Bodenberg.AppDimens.*.nupkg` |
| `*.SmokeTest` | Minimal Android app to verify the binding compiles |
| `scripts/sync-aar-from-maven.sh` | Downloads the upstream AAR from Maven Central into `Jars/` |

---

## Requirements

| Requirement | Notes |
|-------------|--------|
| **.NET** | **10** with Android workload (`dotnet workload install android`) or MAUI |
| **Target framework** | Bindings: `net10.0-android` · Native libs: `net8.0` / `net9.0` / `net10.0` (+`net11.0` with SDK 11) |
| **Minimum Android API** | **24** (matches packaged AARs) |
| **JDK** | **17 or 21** for Xamarin.Android on .NET 10 |
| **Android SDK** | Platform **36+** for local binding builds |

Each binding subfolder includes `Directory.Build.props` with common paths (`ANDROID_HOME`, `~/Android/Sdk`, OpenJDK 17/21) so command-line builds work without extra environment setup on many Linux/macOS machines.

---

## Build locally

From any binding folder:

```bash
cd appdimens-sdps-net-binding   # or ssps / dynamic
dotnet build AppDimens.Sdps.sln -c Release
```

Or build all three bindings at once from the repository root:

```bash
./build-bindings.sh
```

Override the Android SDK if needed:

```bash
dotnet build -p:AndroidSdkDirectory=/path/to/Android/Sdk
```

Refresh the embedded AAR after a new Maven release:

```bash
./scripts/sync-aar-from-maven.sh <maven-version>
# Update Jars/*.aar and <AndroidLibrary Include=...> in the .csproj when the filename changes
```

### Smoke-test APKs (binding validation)

Minimal apps (`*.SmokeTest`) compile against each binding on **.NET 10**. To produce signed APKs for device/emulator testing:

```bash
dotnet publish AppDimens.Sdps.SmokeTest/AppDimens.Sdps.SmokeTest.csproj -c Release -f net10.0-android -p:AndroidPackageFormat=apk
```

Pre-built APKs are produced locally by the command above (filenames include the package id and version **3.6.0**) and are not committed to this repository.

### Publishing

Packages are packed per project (`dotnet pack -c Release`) and pushed to NuGet.org through the **Release** workflow (`.github/workflows/release.yml`, manual `workflow_dispatch`). CI (`.github/workflows/ci.yml`) builds the MAUI library, runs its test suite, verifies packing of all three bindings on every push to `main`.

---

## NuGet metadata

| NuGet field | URL |
|-------------|-----|
| **Project website** (all packages) | https://github.com/bodenberg/appdimens |
| **Source repository** | Per package → upstream Android/KMP repo (`appdimens-sdps`, `appdimens-ssps`, `appdimens-dynamic`, `appdimens-kmp`) |
| **Binding/port source** (this repo) | https://github.com/bodenberg/appdimens-net-binding |

---

## Documentation

| Topic | Where |
|-------|--------|
| **LLM digest do monorepo** | [LLMS.txt](LLMS.txt) |
| **Sdps** usage, C# namespaces, XML dimens | [appdimens-sdps-net-binding/README.md](appdimens-sdps-net-binding/README.md) |
| **Maui.Sdps** nativo (XAML, markup, builders) | [appdimens-sdps-net/README.md](appdimens-sdps-net/README.md) |
| **Ssps** typography APIs | [appdimens-ssps-net-binding/README.md](appdimens-ssps-net-binding/README.md) |
| **Dynamic** strategies & facilitators | [appdimens-dynamic-net-binding/README.md](appdimens-dynamic-net-binding/README.md) |
| **AppDimens.Net** (port KMP → C#): módulos, paridade, performance | [appdimens-net/README.md](appdimens-net/README.md) · [appdimens-net/DOCUMENTATION/](appdimens-net/DOCUMENTATION/) · [appdimens-net/LLMS.txt](appdimens-net/LLMS.txt) |
| **Dynamic** strategy guide (Android) | [appdimens-dynamic/DOCUMENTATION](https://github.com/bodenberg/appdimens-dynamic/tree/main/DOCUMENTATION) |
| **AppDimens** umbrella project | [appdimens](https://github.com/bodenberg/appdimens) |

---

## Links

| Resource | URL |
|----------|-----|
| AppDimens (main project) | https://github.com/bodenberg/appdimens |
| Android SDPS | https://github.com/bodenberg/appdimens-sdps |
| Android SSPS | https://github.com/bodenberg/appdimens-ssps |
| Android Dynamic | https://github.com/bodenberg/appdimens-dynamic |
| KMP (referência dos portings) | https://github.com/bodenberg/appdimens-kmp |
| **This repository** | https://github.com/bodenberg/appdimens-net-binding |

---

## License

**Apache 2.0**, consistent with the upstream AppDimens libraries.

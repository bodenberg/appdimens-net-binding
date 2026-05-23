# AppDimens .NET Bindings

Monorepo of **.NET 10 for Android** bindings and a **native .NET MAUI** library for the [AppDimens](https://github.com/bodenberg/appdimens) responsive dimension ecosystem. **Android binding** packages target **`net10.0-android`** and embed upstream **Maven AARs**. **MAUI** package **`Bodenberg.AppDimens.Maui.Sdps`** targets **`net8.0`**, **`net9.0`**, and **`net10.0`** (multi-platform).

Android implementation, XML naming, and feature documentation live in the **upstream repositories** listed below. This repository contains **binding projects**, **MSBuild transforms**, **smoke-test apps**, and **publish notes** only.

---

## Packages

| NuGet package | Folder | Android library (source) | Focus |
|---------------|--------|--------------------------|--------|
| [**Bodenberg.AppDimens.Sdps**](https://www.nuget.org/packages/Bodenberg.AppDimens.Sdps) | [`appdimens-sdps-net-binding/`](appdimens-sdps-net-binding/) | [appdimens-sdps](https://github.com/bodenberg/appdimens-sdps) | **Layout + typography** — SDP/HDP/WDP and SSP/HSP/WSP via `@dimen` grids and code APIs |
| [**Bodenberg.AppDimens.Ssps**](https://www.nuget.org/packages/Bodenberg.AppDimens.Ssps) | [`appdimens-ssps-net-binding/`](appdimens-ssps-net-binding/) | [appdimens-ssps](https://github.com/bodenberg/appdimens-ssps) | **Typography only** — SSP/HSP/WSP (smaller if you do not need layout dimens) |
| [**Bodenberg.AppDimens.Dynamic**](https://www.nuget.org/packages/Bodenberg.AppDimens.Dynamic) | [`appdimens-dynamic-net-binding/`](appdimens-dynamic-net-binding/) | [appdimens-dynamic](https://github.com/bodenberg/appdimens-dynamic) | **Code-only** scaling — 15 strategies, no pre-built `@dimen` XML grids |
| [**Bodenberg.AppDimens.Maui.Sdps**](https://www.nuget.org/packages/Bodenberg.AppDimens.Maui.Sdps) | [`appdimens-sdps-net/`](appdimens-sdps-net/) | [appdimens-sdps](https://github.com/bodenberg/appdimens-sdps) | **.NET MAUI nativo** — SDP/HDP/WDP/SSP sem binding Android (`net8` / `net9` / `net10`) |

**Versões NuGet atuais:** bindings Android **`3.5.1.4`** · MAUI nativo **`3.5.1.4`** (veja cada README para a versão do AAR Maven embutido nos bindings).

```bash
dotnet add package Bodenberg.AppDimens.Sdps --version 3.5.1.4
dotnet add package Bodenberg.AppDimens.Ssps --version 3.5.1.4
dotnet add package Bodenberg.AppDimens.Dynamic --version 3.5.1.4
dotnet add package Bodenberg.AppDimens.Maui.Sdps --version 3.5.1.4
```

### Which package should I use?

| Need | Package |
|------|---------|
| Responsive **margins, padding, sizes** and **text** with `@dimen/_16sdp`-style XML (Android binding) | **Sdps** |
| **Text / `sp` only**, smaller dependency | **Ssps** |
| **Runtime strategies** (percent, auto, fluid, …) without thousands of XML dimens | **Dynamic** |
| **MAUI multiplataforma** (Android, iOS, Windows, macOS) com `{sdp:16}` e APIs C# nativas | **Maui.Sdps** |

You can combine packages only when your app design allows overlapping responsibilities; most apps pick **one primary** approach per surface.

---

## Repository layout

Each binding is a self-contained tree:

```
appdimens-net-binding/
├── appdimens-sdps-net/              # Native MAUI SDP (net8/9/10)
├── appdimens-sdps-net-binding/
│   ├── AppDimens.Sdps.sln
│   ├── AppDimens.Sdps.Binding/      # NuGet + AAR binding
│   ├── AppDimens.Sdps.SmokeTest/    # Optional compile smoke APK
│   ├── Directory.Build.props        # Android SDK / JDK fallbacks (local builds)
│   ├── README.md                    # Consumer documentation (packed into NuGet)
├── appdimens-ssps-net-binding/      # same structure
└── appdimens-dynamic-net-binding/
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
| **Target framework** | Bindings: `net10.0-android` · MAUI: `net8.0` / `net9.0` / `net10.0` |
| **Minimum Android API** | **24** (matches packaged AARs) |
| **JDK** | **17 or 21** for Xamarin.Android on .NET 10 |
| **Android SDK** | Platform **36+** for local binding builds |

Each subfolder includes `Directory.Build.props` with common paths (`ANDROID_HOME`, `~/Android/Sdk`, OpenJDK 17/21) so command-line builds work without extra environment setup on many Linux/macOS machines.

---

## Build locally

From any binding folder:

```bash
cd appdimens-sdps-net-binding   # or ssps / dynamic
dotnet build AppDimens.Sdps.sln -c Release
```

Override the Android SDK if needed:

```bash
dotnet build -p:AndroidSdkDirectory=/path/to/Android/Sdk
```

Refresh the embedded AAR after a new Maven release:

```bash
./scripts/sync-aar-from-maven.sh 3.1.5
# Update Jars/*.aar and <AndroidLibrary Include=...> in the .csproj when the filename changes
```

### Smoke-test APKs (binding validation)

Minimal apps (`*.SmokeTest`) compile against each binding on **.NET 10**. To produce signed APKs for device/emulator testing:

```bash
dotnet publish AppDimens.Sdps.SmokeTest/AppDimens.Sdps.SmokeTest.csproj -c Release -f net10.0-android -p:AndroidPackageFormat=apk
```

Pre-built APKs for the current binding release are copied to [`artifacts/smoke-apks/`](artifacts/smoke-apks/) when generated locally (filenames include package id and version **3.5.1.4**).

---

## NuGet metadata

| NuGet field | URL |
|-------------|-----|
| **Project website** (all packages) | https://github.com/bodenberg/appdimens |
| **Source repository** | Per package → upstream Android repo (`appdimens-sdps`, `appdimens-ssps`, `appdimens-dynamic`) |
| **Binding source** (this repo) | https://github.com/bodenberg/appdimens-net-binding |

Publishing steps: [`NUGET-PUBLISH.md`](NUGET-PUBLISH.md) (MAUI nativo + bindings Android).

---

## Documentation

| Topic | Where |
|-------|--------|
| **Sdps** usage, C# namespaces, XML dimens | [appdimens-sdps-net-binding/README.md](appdimens-sdps-net-binding/README.md) |
| **Maui.Sdps** nativo (XAML, markup, builders) | [appdimens-sdps-net/README.md](appdimens-sdps-net/README.md) |
| **Ssps** typography APIs | [appdimens-ssps-net-binding/README.md](appdimens-ssps-net-binding/README.md) |
| **Dynamic** strategies & facilitators | [appdimens-dynamic-net-binding/README.md](appdimens-dynamic-net-binding/README.md) |
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
| **This repository** | https://github.com/bodenberg/appdimens-net-binding |

---

## License

**Apache 2.0**, consistent with the upstream AppDimens libraries.

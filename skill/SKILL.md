---
name: appdimens-net-binding
description: Working on the AppDimens .NET monorepo — native responsive-dimension libraries for .NET MAUI, WPF, Windows Forms and WinUI 3 (SDPS family in appdimens-sdps-net, AppDimens.Net family in appdimens-net) and the Android AAR bindings (Sdps/Ssps/Dynamic). Use for building, testing, versioning, AAR sync, packing/publishing NuGet packages, desktop adapters and documentation (docs/ guides + generated API references).
---

# AppDimens .NET — Monorepo Guide

Responsive-dimension ecosystem for .NET. **26 NuGet packages**, all versioned in lockstep (currently **3.6.1**):

* **SDPS family** (`appdimens-sdps-net/`) — Android SDP parity, native .NET: `Bodenberg.AppDimens.Maui.Sdps` (MAUI meta-package)
  + `Bodenberg.AppDimens.Sdps.Wpf` / `.WinForms` / `.WinUI` (desktop adapters, self-contained, no MAUI).
* **AppDimens.Net family** (`appdimens-net/`) — bit-for-bit C# port of appdimens-kmp: core `AppDimens.Net`, 13 strategy
  satellites, `AppDimens.Net.Sdk` meta-package, integrations `.Maui` / `.Wpf` / `.WinForms` / `.WinUI`.
* **Android bindings** — `Bodenberg.AppDimens.Sdps` / `.Ssps` / `.Dynamic` (`net10.0-android36.0`, AARs **3.2.0**).

## Layout

| Folder | What it is | NuGet |
|--------|-----------|-------|
| `appdimens-sdps-net/src/AppDimens.Maui*` | SDPS engine + MAUI layers. `AppDimens.Maui` (resolver) and `Core/Inverters/Responsive/Builders/Helpers/Resources` are **MAUI-free**; MAUI-only code lives in `AppDimens.Maui.Hosting` (UseAppDimensSdps, AppDimensSdpsWindow, MauiDisplayMetricsSource), `Extensions`, `Markup`, `Xaml`, `Converters` | `Bodenberg.AppDimens.Maui.Sdps` (packed from `AppDimens.Maui.Sdk`) |
| `appdimens-sdps-net/src/AppDimens.Sdps.{Wpf,WinForms,WinUI}` | Desktop adapters (shared settings in `src/AppDimens.Sdps.Desktop.props`) | `Bodenberg.AppDimens.Sdps.{Wpf,WinForms,WinUI}` |
| `appdimens-net/src/AppDimens.Net*` | Core, strategies, Sdk, Maui, Wpf, WinForms, WinUI (desktop settings in `src/AppDimens.Net.Desktop.props`) | `AppDimens.Net.*` |
| `appdimens-{sdps,ssps,dynamic}-net-binding/` | `<X>.Binding/` (csproj + `Jars/*.aar` + `Maven/` pom/module/javadoc + `Transforms/` + `Additions/`), `<X>.SmokeTest/`, `scripts/sync-aar-from-maven.sh` | `Bodenberg.AppDimens.{Sdps,Ssps,Dynamic}` |
| `docs/<PackageId>/` | `README.md` guide + generated `API.md` for every package; index `docs/README.md` | — |
| `tools/` | `ApiDocGen` (MetadataLoadContext API docs), `aar-api-doc.py` (javap → C# names), `sync-guides.py` | — |

## Architecture notes

- **SDPS host abstraction**: `AppDimensResolver` reads metrics from an `IDisplayMetricsSource` (`AppDimens.Maui.Core`).
  Default = `MauiDisplayMetricsSource`, resolved **by name** from `AppDimens.Maui.Hosting` (DynamicDependency keeps it under
  trimming); non-MAUI hosts call `AppDimensResolver.Instance.UseDisplaySource(...)` before `Initialize` (adapters do it).
  Never add a MAUI type reference to `AppDimens.Maui` — the WPF XAML compiler fails (MC1000) on referenced assemblies that
  expose MAUI types.
- **Self-contained packing**: projects with `AppDimensEmbedReferences=true` (and `AppDimens.Maui.Sdk`) embed all project
  references via `Directory.Pack.targets`, imported from `Directory.Build.targets` (must be evaluated after the project body).
- **AppDimens.Net hosts**: `HostDimensScope` (core) is the base of every window scope; `AppDimensHost.Activate(scope)` wires
  the cache watcher + ambient context.
- **Desktop markup**: WPF returns a live `Binding` to `DimenLiveSource.Version` through `DimenConverter`; WinUI keeps a weak
  registry and re-applies values on resize. Unhook window events on the window's own UI thread (Dispatcher/DispatcherQueue).
- **WinUI projects** reference `Microsoft.WindowsAppSDK` with `ExcludeAssets="build;buildTransitive"` (PRI/AppX tasks need
  Visual Studio's MSBuild; libraries only need the reference assemblies).
- **Generated code**: `scripts/generate-desktop-api.py` in each family regenerates `DimenFunction`, markup extensions and int
  extensions — edit the script, not the `*.g.cs`/`DimenFunction.cs` output.

## Build, test, pack

```bash
cd appdimens-sdps-net
python3 scripts/generate-dimens.py                  # REQUIRED before build/test (Generated/ buckets)
./scripts/publish-nuget.sh                          # tests + pack + checks: Maui.Sdps + Sdps.Wpf/WinForms/WinUI (--push to publish)

cd ../appdimens-net
./scripts/pack-nuget.sh                             # tests + pack every AppDimens.Net.* (--push to publish)

dotnet test appdimens-sdps-net/tests/AppDimens.Sdps.Desktop.Tests   # Windows only
dotnet test appdimens-net/tests/AppDimens.Net.Desktop.Tests         # Windows only

./build-bindings.sh                                 # 3 bindings, Release, net10.0-android36.0
./scripts/generate-docs.sh                          # docs/*/API.md + mirrored guides
```

Requirements: .NET 10 SDK (`global.json` per folder), runtimes 8/9 for multi-target tests; desktop libraries compile on any OS
(`EnableWindowsTargeting`), desktop tests run on Windows; bindings need the Android workload, JDK 17/21 and Android SDK 36+.

## CI / Release

- `ci.yml` (push/PR to main + manual): SDPS tests/pack, AppDimens.Net tests/pack, Windows desktop tests + WinUI build,
  bindings matrix net8/9/10-android (verifies the versioned AARs; does not re-download them).
- `release.yml` (manual `workflow_dispatch`, can be dispatched on any branch): packs **all 26 packages** into the
  `nuget-packages` artifact; `push_nuget=true` pushes to NuGet.org with `NUGET_API_KEY`; `commit_packages=true` commits
  the `.nupkg`/`.snupkg` files to `nupkgs/<version>/` on the dispatched branch (pull before pushing again). Bindings are packed for
  `net10.0-android36.0` (same as previous releases).

## Conventions

- **Versioning**: `<Version>` in the three Binding `.csproj` + `appdimens-sdps-net/Directory.Build.props` +
  `appdimens-net/Directory.Build.props` (+ SmokeTest/Sample `ApplicationDisplayVersion`). Keep everything in lockstep;
  bump the patch (+0.0.1) for maintenance releases.
- **AAR sync**: `./scripts/sync-aar-from-maven.sh <version>` (retries; `MAVEN_REPO=https://maven-central.storage-download.googleapis.com/maven2`
  mirror on HTTP 429) → update `<AndroidLibrary Include="Jars/...">` + release notes in the Binding csproj → README
  "Package version vs embedded AAR" → CI matrix `aar_version` → compare the public API (`javap -public` diff) and the POM
  dependencies against the Kotlin/AndroidX pins.
- **Binding code**: never hand-edit generated JNI wrappers; use `Transforms/Metadata.xml` and `Additions/*.cs`.
- **Docs**: package READMEs are what NuGet shows — keep them beginner-friendly (install → initialize → use → cheat sheet →
  Portuguese summary) and link to `docs/<PackageId>/`. `docs/AppDimens.Net*` guides are hand-written; `docs/Bodenberg.*`
  guides are mirrors of the package READMEs (`tools/sync-guides.py`); every `API.md` is generated. Run
  `./scripts/generate-docs.sh` after API or README changes. C# samples must compile (verify by extracting the code blocks
  into a scratch project referencing the packages).
- **Historical docs** (`appdimens-sdps-net/docs/AUDIT-*`, `appdimens-net/AUDIT_REPORT.md`, `appdimens-net/PLAN.md`) are dated
  snapshots — do not rewrite them; only living docs (READMEs, LLMS.txt, docs/, DOCUMENTATION/) are updated.

## Common tasks

- **Bump package version**: the `<Version>` locations above + install snippets in README.md / LLMS.txt / package READMEs.
- **New upstream Maven release**: see AAR sync above; rebuild + `./scripts/generate-docs.sh` (refreshes binding API.md).
- **New public API**: XML-doc it (shows up in `API.md`), regenerate docs, add a sample to the package guide.
- **Publish**: dispatch `release.yml` with `push_nuget=true`, or run the two pack scripts with `--push` plus the bindings
  (`dotnet pack <X>.Binding -c Release -p:TargetFrameworks=net10.0-android36.0`).

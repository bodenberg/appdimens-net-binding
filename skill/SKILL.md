---
name: appdimens-net-binding
description: Working on the AppDimens .NET monorepo — Android bindings (Sdps/Ssps/Dynamic), native MAUI SDP library (appdimens-sdps-net), and the C# KMP port (appdimens-net). Use for building, testing, versioning, AAR sync, and documentation updates in this repository.
---

# AppDimens .NET Bindings — Monorepo Guide

Responsive-dimension ecosystem for .NET: three **Android AAR bindings**, one **native MAUI SDP** library, and a **C# port of appdimens-kmp** (`AppDimens.Net.*`). Everything ships on NuGet under version **3.6.0**.

## Layout

| Folder | What it is | NuGet |
|--------|-----------|-------|
| `appdimens-sdps-net-binding/` | net10.0-android binding of appdimens-sdps (AAR **3.1.7**) | `Bodenberg.AppDimens.Sdps` |
| `appdimens-ssps-net-binding/` | net10.0-android binding of appdimens-ssps (AAR **3.1.7**) | `Bodenberg.AppDimens.Ssps` |
| `appdimens-dynamic-net-binding/` | net10.0-android binding of appdimens-dynamic (AAR **3.1.9**, core + 13 satellites) | `Bodenberg.AppDimens.Dynamic` |
| `appdimens-sdps-net/` | Native MAUI SDP library (no JNI/AAR); src + samples + tests + docs | `Bodenberg.AppDimens.Maui.Sdps` |
| `appdimens-net/` | C# port of appdimens-kmp: 16 lib projects (`src/AppDimens.Net*`), tests, samples, benchlab | `AppDimens.Net.Sdk` + satellites |

Each binding folder has: `<Name>.sln`, `<Name>.Binding/` (csproj + `Jars/*.aar` + `Maven/` metadata + `Transforms/` + `Additions/`), `<Name>.SmokeTest/`, `scripts/sync-aar-from-maven.sh`, `Directory.Build.props` (SDK/JDK path fallbacks).

## Build & test

```bash
./build-bindings.sh                                  # all 3 bindings, Release, net10.0-android36.0
dotnet build appdimens-sdps-net-binding/AppDimens.Sdps.sln -c Release   # single binding
dotnet publish <Module>.SmokeTest/<Module>.SmokeTest.csproj -c Release -f net10.0-android -p:AndroidPackageFormat=apk   # smoke APK

cd appdimens-sdps-net
python3 scripts/generate-dimens.py                   # REQUIRED before build/test (regenerates Generated/ buckets)
dotnet test tests/AppDimens.Maui.Tests/AppDimens.Maui.Tests.csproj -c Release

cd ../appdimens-net
dotnet test tests/AppDimens.Net.Tests                # parity/invariant suite
dotnet build AppDimens.Net.slnx                      # solution (libs net8/9/10; android heads need workload)
```

Requirements: .NET 10 SDK (`global.json` per folder), Android workload, JDK 17/21, Android SDK platform 36+. Bindings override nothing if paths are standard Linux/macOS (`Directory.Build.props` handles fallbacks).

## Conventions

- **Versioning**: NuGet version lives in each Binding `.csproj` `<Version>` and in `Directory.Build.props` of the native libs. Keep all packages in lockstep (currently **3.6.0**). SmokeTest csprogs carry `ApplicationDisplayVersion`.
- **AAR sync**: run `./scripts/sync-aar-from-maven.sh <maven-version>` inside the binding folder, then update `<AndroidLibrary Include="Jars/...">` in the csproj and the version references in that README + CI matrix in `.github/workflows/ci.yml`.
- **Binding code**: never hand-edit generated JNI wrappers; use `Transforms/Metadata.xml` for renames/removals and `Additions/*.cs` for C# helpers.
- **Docs language**: root README mixes EN/PT-BR intentionally; module READMEs are English. Keep tables for requirements/versions.
- **CI**: `.github/workflows/ci.yml` (build + tests + pack verify + vulnerable-package audit), `release.yml` (manual dispatch, optional NuGet push). Publishing = `dotnet pack -c Release` then the release workflow.
- **Historical docs** (`docs/AUDIT-*`, `AUDIT_REPORT.md`, `PLAN.md`) are dated snapshots — do not rewrite them when versions change; only living docs (READMEs, LLMS.txt, DOCUMENTATION/) get updated.

## Common tasks

- **Bump package version**: edit `<Version>` in the three Binding csprojs + `appdimens-sdps-net/Directory.Build.props` + `appdimens-net/Directory.Build.props`; update install snippets in all READMEs and root README.
- **New upstream Maven release**: sync script → csproj `AndroidLibrary` → README "Package version vs embedded AAR" table → CI matrix `aar_version`.
- **Doc updates must stay consistent with**: embedded AAR versions in `*/Jars/`, `<Version>` in csproj/props, and CI matrix values.

# Technical Audit Report

**Date:** 2026-05-21  
**Scope:** appdimens-net monorepo

## Resolved in this audit

| ID | Issue | Fix |
|----|-------|-----|
| R1 | Redundant `Generated/{sw,w,h}/` (~1071 files) | Layout v2: `Dimens.{N}.xaml` + `BucketRegistry` merged loader |
| R2 | No CI | `.github/workflows/ci.yml` |
| R3 | Hybrid mode untested | `HybridModeTests` |
| R4 | MauiVersion 9.x on net10 | `Directory.Build.props` → 10.0.20 |
| R5 | README claimed all net10-android | Root README corrected |
| R7 | Low test coverage | +25 tests (`ExtendedTests.cs`), 41 total |
| R8 | `XamlDimenParser` broken for standard lines | Fixed value extraction between `>` and `<` |
| R9 | `ProvideValue(null)` NRE | Null-safe `IServiceProvider` in XAML extensions |

## Open / operational

| ID | Issue | Mitigation |
|----|-------|------------|
| R6 | AARs not in git | CI runs `sync-aar-from-maven.sh` |
| R10 | NuGet 3.5.1.4 vs Maven 3.1.5 | Documented in binding READMEs |
| R11 | Benchmarks not in CI gate | Manual `dotnet run` on benchmarks project |

## Validation

- `dotnet test` — 41/41 pass (Release, net10.0)
- `generate-dimens.py` — 358 buckets + base + JSON
- `publish-nuget.sh` — pack checks include `Dimens.300.xaml`, `Dimens.Base.xaml`

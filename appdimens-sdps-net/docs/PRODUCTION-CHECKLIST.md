# Production Checklist

## Build & artifacts

- [ ] SDK 10.0.300+ (`global.json`)
- [ ] `python3 scripts/generate-dimens.py` → layout v2 (`buckets.json`, `Dimens.Base.xaml`, `Dimens.{N}.xaml`)
- [ ] `dotnet test` Release — all pass
- [ ] `./scripts/publish-nuget.sh` — pack checks pass
- [ ] `.nupkg` contains net8/net9/net10 DLLs, analyzer, Generated content

## Quality gates

- [ ] Zero test failures
- [ ] No high/critical CVE (`dotnet list package --vulnerable`)
- [ ] `MauiVersion` aligned: net10 → 10.0.20
- [ ] Documentation matches layout v2 (no `sw/w/h` folder references)

## Release

- [ ] Version bumped (semver)
- [ ] CHANGELOG updated
- [ ] Tag `v*` on git
- [ ] NuGet push + indexing verified

## Post-release

- [ ] Sample app smoke on Android emulator
- [ ] Monitor issues 48h

# Release Plan

## Versioning

- **MAUI:** `Bodenberg.AppDimens.Maui.Sdps` — semver in `Directory.Build.props`
- **Bindings:** independent `3.5.1.x` track; AAR Maven version documented per README

## Next release suggestion

**1.0.1** — current package version (NuGet packaging fixes)

## Steps

1. Complete `PRODUCTION-CHECKLIST.md`
2. Tag `v1.0.1` on `main` when publishing
3. GitHub Actions `release` workflow or `./scripts/publish-nuget.sh --push`
4. Verify https://www.nuget.org/packages/Bodenberg.AppDimens.Maui.Sdps

## Communication

- CHANGELOG.md
- Release notes: layout v2, parser fix, Maui 10 alignment, CI

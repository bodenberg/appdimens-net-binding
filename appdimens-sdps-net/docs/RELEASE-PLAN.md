# Release Plan

## Versioning

- **MAUI:** `Bodenberg.AppDimens.Maui.Sdps` — `3.5.1.x` track in `Directory.Build.props`
- **Bindings:** independent `3.5.1.x` track; AAR Maven version documented per README

## Next release suggestion

**3.5.1.4** — current package version (aligned with Android bindings)

## Steps

1. Complete `PRODUCTION-CHECKLIST.md`
2. Tag `v3.5.1.4` on `main` when publishing
3. GitHub Actions `release` workflow or `./scripts/publish-nuget.sh --push`
4. Verify https://www.nuget.org/packages/Bodenberg.AppDimens.Maui.Sdps

## Communication

- CHANGELOG.md
- Release notes: layout v2, parser fix, Maui 10 alignment, CI

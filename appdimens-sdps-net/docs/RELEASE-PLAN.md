# Release Plan

## Versioning

- All packages of the monorepo share one version (`<Version>` in `Directory.Build.props` of `appdimens-sdps-net` and
  `appdimens-net`, and in the three Android Binding `.csproj`). Maintenance releases bump the patch (+0.0.1).
- **SDPS family:** `Bodenberg.AppDimens.Maui.Sdps` + `Bodenberg.AppDimens.Sdps.Wpf` / `.WinForms` / `.WinUI`.
- **Bindings:** embedded AAR Maven version documented per README ("Package version vs embedded AAR").

## Current release

**3.6.1** — WPF, Windows Forms and WinUI 3 adapters; host-agnostic resolver (`IDisplayMetricsSource`); MAUI glue moved to
`AppDimens.Maui.Hosting`; `DimenFunction`; Android AARs 3.2.0; per-package documentation in `docs/`.

## Steps

1. Complete `PRODUCTION-CHECKLIST.md`
2. Dispatch the **Release** workflow (`.github/workflows/release.yml`) with `push_nuget=false` and check the
   `nuget-packages` artifact (27 packages)
3. Dispatch it again with `push_nuget=true` (or run `./scripts/publish-nuget.sh --push` here and
   `appdimens-net/scripts/pack-nuget.sh --push`, plus the bindings)
4. Tag `v3.6.1` on `main`
5. Verify https://www.nuget.org/packages/Bodenberg.AppDimens.Maui.Sdps and the desktop packages

## Communication

- Release notes: root README "What's new in 3.6.1"

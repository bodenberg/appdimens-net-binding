# Audit Risk Register

| ID | Finding | Severity | Root cause | Fix | Validation |
|----|---------|----------|------------|-----|------------|
| R1 | Redundant sw/w/h folders | High | Android layout parity | Merged `Dimens.{N}.xaml` layout v2 | BucketRegistryTests, generate script |
| R2 | No CI | Critical | Manual publish only | `.github/workflows/ci.yml` | PR gate green |
| R3 | Hybrid untested | High | Default mode not covered | HybridMode tests | dotnet test |
| R4 | MauiVersion 9 on net10 | High | Stale Directory.Build.props | MauiVersion 10.0.20 | multi-TFM build |
| R5 | README net10 claim | Medium | Copy error | README fix | doc review |
| R6 | AAR missing in clone | High | Binaries not in repo | CI sync-aar step | binding build job |
| R7 | Low test coverage | High | Single test file | Expanded test suite | coverlet threshold |

# Hardening Plan

## Security

- CI: `dotnet list package --vulnerable` on MAUI tests project
- No secrets in repository; `NUGET_API_KEY` only in CI secrets for release workflow
- Bindings: minimal Android permissions in smoke APKs

## Threading

- `ResourceBucketManager` — lock on bucket state
- `DimensionCache` — `ConcurrentDictionary`
- Test: `DimensionCacheStressTests.ConcurrentGetOrAdd_IsStable`

## Compatibility

- MAUI TFMs: net8.0, net9.0, net10.0
- Bindings: net10.0-android, min API 24
- Legacy dimens layout v1: loader fallback with warning log

## Parser robustness

- `XamlDimenParser` uses first `>` after `x:Key` for value boundary (fixed 2026-05-21)

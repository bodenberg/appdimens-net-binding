# Observability Plan

AppDimens is a client library — no hosted telemetry by default.

## Logging hook

Use `AppDimensLogging.Current = new MyLogger()` implementing `IAppDimensLogger`:

- `LogDebug` — bucket changes, cache behavior (optional)
- `LogWarning` — legacy layout v1 detected, missing `Generated/`

Built-in: `DebugAppDimensLogger` writes to `System.Diagnostics.Debug`.

## Crash reporting (host app)

Document in app README:

- Wrap `AppDimensSdps.Initialize` in try/catch for `FileNotFoundException` / `InvalidOperationException`
- Report to Sentry/AppCenter with `generatedPath` in breadcrumbs

## Metrics (optional)

Hosts may track:

- `appdimens.resolve.duration_ms`
- `appdimens.bucket.active_sw`

Not implemented in-library to keep zero dependencies.

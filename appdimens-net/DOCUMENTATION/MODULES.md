# Module map (AppDimens .NET)

## `AppDimens.Net` (core)

* **Common** — enums: `DpQualifier` (SmallWidth/Height/Width), `Inverter`
  (9 axis-swap rules), `Orientation`, `UiModeType` (incl. foldable synthetic
  states), `UnitType`.
* **Core** — `IAppDimensContext`, `ScreenConfiguration` (immutable snapshot:
  w/h/sw dp, dpi, fontScale, orientation, uiMode), `DimenMetrics`,
  `DimenCache` (snapshot-partitioned cache, fast lanes, watcher plumbing),
  `AppDimensAmbient`.
* **Code/Scaled** — the Scaled strategy: `Sdp/Sdpa/Sdpi/Sdpia`, height/width
  variants, `SdpPh/Lw/Lh/Pw…` inverters, facilitators (`SdpRotate`, `SdpMode`,
  `SdpQualifier`, `SdpScreen` + `*Raw`/plain forms), `ScaledDimension` builder,
  kernels in `DimenScaledKernels.cs`.

## Satellite strategies (`AppDimens.Net.{Name}`)

Uniform surface per strategy `{P}` ∈ {PS, PW, A, LOG, F, I, DGS, PRS, FL, FT? , D}:

```
{P}Sdp(ctx) {P}Sdpa(ctx) {P}Hdp(ctx) {P}Wdp(ctx)   // dp lanes (+Px variants)
{P}Sdpi(ctx) {P}Sdpa-ia …                          // ignore-multi-window variants
{P}HdpCustom(v, qualifier, inverter, imw, ar, k)   // full control
To{X}Dp(value, ctx, qualifier, inverter, imw, ar, k)  // generic kernel entry
{P}{X}RotateRaw / SpaceWDp (Percent)               // helpers
```

`Fit` exposes `ToFitDp` + the `DimenFit` builder with priority-driven
`.Screen(...)` entries and `ResolveAll(ctx)`.

## Specialized

* `AppDimens.Net.Resize` — `ResizeRangePx`/`ResizeMath` (bounded step generation
  ≤4096 + binary search), `DimenResize.AutoResizeTextPx/AutoResizeSquarePx`,
  `DimenPhysicalUnits` conversions.
* `AppDimens.Net.Maui` — `AppDimensMaui.AttachWindow/Detach/Init`,
  `WindowDimensScope` (event-driven resize watcher; listeners fire only on real
  config deltas), MAUI service extensions.

## Testing

* `FakeAppDimensContext` (in core): mutable context + `NotifyChange()` to drive
  watchers deterministically.

## Apps

* `samples/AppDimens.Sample(.Ui)` — M3 demo screen mirroring the KMP Compose demo.
* `benchlab/AppDimens.BenchLab.*` — dark benchmark dashboard.
* `samples/AppDimens.WebDemo` — Blazor WASM self-test suite.

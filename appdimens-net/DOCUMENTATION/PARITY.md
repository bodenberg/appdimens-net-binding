# Parity report — AppDimens .NET vs KMP/Dynamic

Verified numerically by `tests/AppDimens.Net.Tests` (48 tests, green) and in
browser by `samples/AppDimens.WebDemo` (21/21).

## Golden values (w=360 h=740 sw=360 dpi=420 fs=1.0)

| Call | Formula | Expected |
|---|---|---|
| `16.Sdp` | base·sw/300 | 19.2 |
| `48.Hdp` | base·h/300 | 118.4 |
| `24.PSdp` | base·sw/300 | 28.8 |
| `28.ToFluidDp` | lerp(base·0.8, base·1.2, t∈[320..768]) | 23.4 |
| `28.ASdp` | base·dim/300 for dim≤480 | 33.6 |
| `12.DSdp` | base·density(2.625) | 31.5 |
| `18.DGSdp` | base·√(min²+max²)/611.6305 | 24.2182 |
| `AutoResizeSquarePx(400dpx…100dpx)` | largest step ≤ inner-dp limit (single px→dp conversion) | 100 |

## Invariants

1. **Fast lane ≡ full path (bitwise)** — `ResolveSdpDp/Hdp/Wdp(+Px)` multiply in
   the exact kernel order `(base × dim) × ratio [× density]`. A previous version
   pre-multiplied `dim × ratio`, producing 1-ulp differences for some values —
   fixed in this release and covered by regression checks on the web suite.
2. **Suffix `i`** returns the unscaled base when multi-window constrained.
3. **Event-driven invalidation** — `SetConfig + NotifyChange()` re-scales without
   manual `ClearAll`; fast slots are invalidated per snapshot partition.
4. **Snapshot partitioning** — alternating contexts never cross-pollute cache
   entries (500-alternation test).
5. **Custom sensitivity-K** is never cached.
6. **Resize math** is bounded (≤4096 steps) and binary-searched; px→dp conversion
   happens exactly once (`AutoResizeSquarePx` regression).
7. **Builders/priorities** match Kotlin: qualifier+orientation > mode-only >
   qualifier-only > orientation-only; first sorted match wins.

## Known platform notes

* MAUI `WidthRequest`/`FontSize` are device-independent units; the Resize APIs
  take **pixels** — convert with `units × density` at the call site (demo shows
  the pattern).
* Parameterless extensions require `AppDimensMaui.AttachWindow(window)` **before**
  page construction (ambient otherwise throws `InvalidOperationException`) — this
  was the root cause of the historical BenchLab startup crash.

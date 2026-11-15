# Performance

Custom strategies cost the same as the built-ins when they only depend on the screen. Which form you pick decides the lane.

| Form | Per window | Per value | Lane |
|---|---|---|---|
| `CustomScaleStrategy.Create(s => …)`, any curve, rules, factor bounds, aspect ratio | lambda / curve once | one multiply | bypass (no cache lookup) |
| text formula shaped `base * E` (E reads no value) | `E` once | one multiply | bypass |
| `ScalingStrategy.Formula(s => …)`, other text formulas, `b.Transform(…)`, `b.ClampValue(…)` | | formula on a cache miss | shared `DimenCache` |

## How it works

- **Memo per window.** Each strategy keeps the last resolved window (rules applied, factor computed), keyed by the core `DimenMetrics` snapshot. A new window state (resize, rotation, DPI change, font scale, multi-window, UI mode) resolves again once.
- **Bypass lane.** A screen-only strategy builds a custom cache key flagged non-cacheable, so `16.Cdp(Product)` is a memo read plus one multiply.
- **Cached lane.** A value-dependent strategy stores each result in the shared `DimenCache` under `CalcType.Custom`, per window snapshot. The strategy's cache id is part of the key (`DimenCache.BuildCustomKey`), so two strategies never alias. The cache is invalidated automatically on resize; never clear it by hand.
- **Cache ids.** Each strategy instance gets one of 65 536 ids (`ScalingStrategies.MaxCacheId`). Past that, strategies still work but always compute. This is why strategies should be declared once and not rebuilt in layout or resize handlers.
- **Fast path for text.** `base * E` is detected when the formula is parsed (`Formula.IsProportional`); the result is bit for bit the same as evaluating it per value.
- **Thread safety.** Strategies are immutable; the registry, the memo and the cache are safe to use from any thread.

## Guidelines

1. Prefer a factor (`CustomScaleStrategy`, a curve, rules) over a per-value formula when the math is "base × something about the screen".
2. Use `b.ClampValue` only for real absolute limits (tap targets ≥ 48 dp, max text size).
3. Declare strategies once: `static readonly` fields in a `static class AppStrategies`, or a JSON document loaded at startup.
4. Keep lambdas pure and cheap. They run on memo or cache misses, on the calling thread.
5. Inline text formulas (`16.Cdp("…")`) are shared per text, so they are fine anywhere. Building strategies from dynamic strings in a loop is not: past 256 distinct texts each call builds a new strategy.

[Back to the custom docs](README.md)

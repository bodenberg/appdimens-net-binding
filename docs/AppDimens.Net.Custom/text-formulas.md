# Text formulas

A text formula is the same math as a [code formula](code-formulas.md), written as a string. Use it when the formula must live outside the code: in the strategies JSON, a design-token file, remote config or an A/B test. The language is identical on every AppDimens platform and gives the same numbers.

```csharp
var hud = ScalingStrategy.Formula("base * pow(sw / 360, 0.8)");     // strategy (shared per text)
float pad = 16.Cdp("base * (tablet ? 0.9 : 1)");                   // inline, dp
float px  = 16.CdpPx("base * (sw / 360)", ctx);                    // inline, px, explicit context
var same  = ScalingStrategies.Formula("base * ratio");              // registry form
var also  = CustomDimens.Formula("base * ratio");                   // bridge form
```

Every extension takes the formula text in place of a strategy: `Cdp` (dp), `CdpPx` (px), `Csp` (sp units), `CspDp` / `CspPx` (final text size), on `int` and `float` (`Cdp` also on `double`), with or without an explicit `IAppDimensContext`.

Inside a full configuration, text appears in two places:

| Where | Builder | JSON | Returns | Extra variables |
|---|---|---|---|---|
| Curve | `b.Curve = ScalingCurve.Expression("…")` | `"curve": {"type": "expression", "expression": "…"}` | the **factor** | `ratio axis ref refWidth refHeight` |
| Formula | `b.SetFormula("…")` | `"formula": "…"` | the **final value** | the curve variables + `base scaled factor` |

`ScalingStrategy.Formula("…")` and `16.Cdp("…")` build a strategy whose only setting is a formula.

## Language

| | |
|---|---|
| Numbers | `16`, `0.8`, `.5`, `1.5e3` (always `.` as decimal separator, whatever the culture) |
| Arithmetic | `+ - * / %` and `^` (power). `%` keeps the sign of the dividend |
| Comparison | `== != < <= > >=` (result 1 or 0) |
| Logic | `&& \|\| !` (non-zero is true) |
| Conditional | `cond ? a : b` |
| Grouping | `( … )` |
| Constants | `pi`, `e`, `true` (1), `false` (0) |

**Precedence**, from loosest to tightest: `? :` · `\|\|` · `&&` · comparisons · `+ -` · `* / %` · unary `- + !` · `^`. `^` is right-associative (`2 ^ 3 ^ 2` = 512), and unary minus binds looser than `^` (`-2 ^ 2` = −4).

### Functions

| Function | Result |
|---|---|
| `min(a, b, …)` · `max(a, b, …)` | smallest · largest argument |
| `clamp(x, lo, hi)` | `x` limited to `[lo, hi]` |
| `pow(x, y)` · `sqrt(x)` · `exp(x)` | power · square root · eˣ |
| `ln(x)` = `log(x)` · `log10(x)` · `log2(x)` | logarithms |
| `abs(x)` · `sign(x)` | absolute value · −1, 0 or 1 |
| `floor(x)` · `ceil(x)` · `round(x)` | rounding (`round` is half up: `floor(x + 0.5)`) |
| `lerp(a, b, t)` | `a + (b − a)·t` |
| `step(edge, x)` | 0 if `x < edge`, else 1 |
| `smoothstep(e0, e1, x)` | smooth 0→1 between `e0` and `e1` |

### Variables

| Variables | Meaning |
|---|---|
| `sw` `width` `height` `shortSide` `longSide` `diagonal` `perimeter` | window measures (dp) |
| `aspectRatio` `density` `fontScale` | long/short side · density · user font scale |
| `phone` `tablet` `desktop` `tv` `watch` `car` `vr` | device class flags (1/0) |
| `landscape` `portrait` `multiWindow` | orientation and split-screen flags (1/0) |
| `ratio` | axis / reference (with the defaults: `sw / 300`, the `Sdp` ratio) |
| `axis` · `ref` · `refWidth` · `refHeight` | axis value · reference value · reference frame (dp) |
| `base` | the value you pass (formulas only) |
| `scaled` | `base × factor` after value bounds (formulas only) |
| `factor` | the strategy factor (formulas only) |

`scaled` and `factor` let a formula post-process the configured strategy: `b.SetFormula("max(scaled, base)")` never goes below the design value, and `b.SetFormula("round(scaled)")` snaps to whole dp.

The code scope uses the same names in PascalCase (`s.Sw`, `s.Tablet`, `s.Pow`…), except the curve-only `axis` / `ref` / `refWidth` / `refHeight` and the formula-only `scaled` / `factor`, which have no code equivalent (use the builder's `b.Transform((baseValue, scaled, screen) => …)` for `scaled`).

## Errors

A formula is parsed when the strategy is created, so mistakes throw `ArgumentException` right away, with the position:

```
Invalid expression at 7: unknown variable 'sww' (allowed: aspectRatio, axis, base, car, …) in "base * sww"
Invalid expression at 0: 'clamp' takes 3 argument(s), got 2 in "clamp(base, 1)"
```

In JSON the message also carries the path, for example `brand.rules[0].set.curve.expression: Invalid expression at 0: …`. A factor that comes out NaN or infinite (for example, a curve expression dividing by zero) throws `InvalidOperationException` when the window resolves. A per-value formula's output is not checked: guard it yourself (`max(x, 1)`).

## Fast path

A formula shaped `base`, `base * E` or `E * base`, where `E` does not read `base`, `scaled` or `factor`, depends only on the screen. AppDimens evaluates `E` once per window, so each value costs one multiply, and the result is bit for bit the same as evaluating the formula per value (`Formula.IsProportional` tells you whether a formula qualifies). Any other shape works too; it is cached per value and window.

| Formula | Fast path |
|---|---|
| `base * pow(sw / 360, 0.8)` | yes |
| `base * (sw / 360)` | yes |
| `(tablet ? 0.9 : 1) * base` | yes |
| `base * sw / 360` | no: it reads as `(base * sw) / 360`. Add parentheses: `base * (sw / 360)` |
| `base + 2`, `round(base * ratio)` | no (genuinely per value) |

## Sharing

Without an id, `ScalingStrategy.Formula("…")`, `ScalingStrategies.Formula("…")`, `CustomDimens.Formula("…")` and `16.Cdp("…")` share one unregistered strategy per text (id `formula:<text>`), so they are cheap to call anywhere. Up to 256 different texts are shared; beyond that, a new strategy is built per call (still correct, but each one consumes a cache id). Pass an `id` (`ScalingStrategy.Formula("…", id: "hud")`) to get your own registered strategy (`register: false` to opt out of registration).

[Back to the custom docs](README.md)

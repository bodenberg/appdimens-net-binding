# Code formulas

Two one-line entry points let you write the scaling math in plain C#. The lambda receives a typed **scope** `s` whose members match the [text language](text-formulas.md) in PascalCase, so you only learn the names once. IntelliSense completes them, and a typo fails at compile time.

| | `CustomScaleStrategy.Create(s => …)` | `ScalingStrategy.Formula(s => …)` |
|---|---|---|
| You return | the **factor** (every value is multiplied by it) | the **final value** |
| Scope | `ScreenScope` | `FormulaScope` (= `ScreenScope` + `Base`) |
| Runs | once per window | once per value and window (cached) |
| Cost per value | one multiply | one cache lookup |
| JSON | no (use a text `expression` curve) | no (use a text `formula`) |

`CustomDimens.CustomScaleStrategy(s => …)` and `CustomDimens.Formula(s => …)` are the same factories.

## `CustomScaleStrategy.Create(s => …)`: programmable factor

```csharp
public static readonly ScalingStrategy Product = CustomScaleStrategy.Create(s => s.Clamp(s.Width / 360f, 0.85f, 1.80f));
public static readonly ScalingStrategy Living  = CustomScaleStrategy.Create(s => s.Tv ? s.Height / 540f : s.Ratio);
public static readonly ScalingStrategy Steps   = CustomScaleStrategy.Create(s => s.Sw switch
{
    < 360f => 0.9f,
    < 600f => s.Sw / 400f,
    _ => 1.5f + (s.Sw - 600f) / 2000f,
});
```

- Signature: `static ScalingStrategy Create(Func<ScreenScope, float> factor, string? id = null, bool? register = null)`.
- Without an `id` a unique `scale#N` id is generated and the strategy is not registered. With an id it is registered (`register: false` opts out), so `ScalingStrategies.Get("product")` finds it.
- Internally it is a code curve (`ScalingCurve.Custom`), so `ExplainCurrent()` shows `-> Custom = …`.
- A non-finite factor (NaN, infinity) throws `InvalidOperationException: custom strategy produced a non-finite factor (NaN)` when the window resolves.

## `ScalingStrategy.Formula(s => …)`: the final value

```csharp
public static readonly ScalingStrategy Hud  = ScalingStrategy.Formula(s => s.Base * s.Pow(s.Sw / 360f, 0.8f));
public static readonly ScalingStrategy Card = ScalingStrategy.Formula(s => s.Tablet ? s.Base * 1.2f : s.Base * s.Ratio);
public static readonly ScalingStrategy Snap = ScalingStrategy.Formula(s => s.Round(s.Base * s.Sw / 360f));   // whole dp only
public static readonly ScalingStrategy Min  = ScalingStrategy.Formula(s => s.Max(s.Base * s.Ratio, s.Base)); // never below the design value
```

- Signature: `static ScalingStrategy Formula(Func<FormulaScope, float> formula, string? id = null, bool? register = null)`.
- `Base` is the value you pass: in `16.Cdp(Hud)` it is `16f`. For `Csp`, the result then follows the user font scale (see [font scale](configuration.md#font-scale-csp)).
- Without an `id`, a unique `formula#N` id is generated. Internally it is a value transform, so `ExplainCurrent()` ends with `then transform`.

## Names on `s`

| Name | Type | Meaning |
|---|---|---|
| `Base` | `float` | the value being scaled (`ScalingStrategy.Formula` only) |
| `Sw` | `float` | smallest width of the window (dp) |
| `Width` · `Height` | `float` | window width · height (dp) |
| `ShortSide` · `LongSide` | `float` | shorter · longer side (dp) |
| `Diagonal` · `Perimeter` | `float` | `√(w² + h²)` · `w + h` (dp) |
| `AspectRatio` | `float` | `LongSide / ShortSide` |
| `Ratio` | `float` | `Sw / 300`, the ratio `Sdp` uses (`s.Base * s.Ratio` is exactly `Sdp`) |
| `Density` · `FontScale` | `float` | screen density · user font scale |
| `Phone` `Tablet` `Desktop` `Tv` `Watch` `Car` `Vr` | `bool` | device class of the window |
| `Landscape` · `Portrait` · `MultiWindow` | `bool` | orientation · split screen |
| `Device` | `DeviceClass` | the device class itself (for `switch`) |
| `Screen` | `ScreenSnapshot` | the whole snapshot (`WidthDp`, `HeightDp`, `SmallestWidthDp`, `IsLandscape`…) |
| `Pow(x, e)` `Sqrt` `Ln` `Log10` `Exp` `Abs` | `float` | math on `float`, no cast needed |
| `Round(x)` | `float` | half up: `floor(x + 0.5)` |
| `Min(a, b)` `Max(a, b)` `Clamp(x, lo, hi)` | `float` | bounds |
| `Lerp(a, b, t)` · `Smoothstep(e0, e1, x)` | `float` | blending |

The device class comes from the context UI mode, then the operating system (desktop OSes are `Desktop`), then the smallest width (`sw ≥ 600` is a tablet). See [device class](configuration.md#device-class).

## Rules for the lambda

- **Keep it pure.** It must not read mutable state, settings or time: the factor is memoized per window, and values are cached per window. Use a different strategy (or a rule) for a different state.
- **Declare it once.** Every call to `CustomScaleStrategy.Create(…)` or `ScalingStrategy.Formula(s => …)` creates a new strategy with its own cache id. Use a `static readonly` field or a field of a long-lived object, never a fresh lambda in a render, layout or resize handler.
- **Prefer the factor form when you can.** If your formula is `Base × (something about the screen)`, write the something as a `CustomScaleStrategy`: it costs one multiply instead of a cache lookup.
- **Return `float`.** Literals need the `f` suffix (`360f`, `0.8f`); the scope helpers take and return `float`, so no casts are needed.

## When to use something else

- The formula must live in JSON, a design-token file or remote config: [Text formulas](text-formulas.md).
- You need rules per device, bounds or a font-scale policy on top of a curve: [Configuration](configuration.md). The builder also accepts code inside it: `b.Curve = ScalingCurve.Custom((ratio, screen) => …)` and `b.Transform((baseValue, scaled, screen) => …)`.

[Back to the custom docs](README.md)

# Configuration

`ScalingStrategy.Define(id, b => { … })` builds a full strategy: the [code](code-formulas.md) and [text](text-formulas.md) one-liners are shortcuts for the simplest cases of it. Every field below also exists in the [JSON format](json.md), on every platform.

```csharp
public static readonly ScalingStrategy Brand = ScalingStrategy.Define("brand", b =>   // creates AND registers "brand"
{
    b.SetReference(360, 640);                    // design frame (dp)
    b.Axis = ScalingAxis.SmallestWidth;          // what to compare
    b.Curve = ScalingCurve.Power(0.8f);          // how the ratio becomes a factor
    b.Multiply(1.0f);
    b.Clamp(0.85f, 1.6f);                        // factor bounds
    b.ClampValue(10, 64);                        // dp bounds (value-dependent)
    b.EnableAspectRatio(sensitivity: 0.004f);    // long/thin screen correction
    b.ClampFontScale(1.3f);                      // Csp policy
    b.IgnoreMultiWindow = false;
    b.On(new[] { DeviceClass.Tablet, DeviceClass.Desktop }, r => r.Curve = ScalingCurve.Logarithmic(0.4f), "large");
});
```

Signature: `static ScalingStrategy Define(string id, Action<ScalingStrategyBuilder> configure, bool register = true)`. Pass `register: false` to build a throwaway strategy. The id must not be blank (`ArgumentException`).

## The formula

```
ratio  = axis(window) / axis(reference)
factor = clamp( curve(ratio) × multiplier × AR , minFactor, maxFactor )
scaled = clampValue( base × factor , minValue, maxValue )   // the configured result
value  = formula ? formula(base, scaled, factor) : scaled   // a formula replaces the result (it gets the original base)
```

## Fields

| Field | Builder | JSON | Default |
|---|---|---|---|
| Reference frame (dp) | `b.SetReference(360, 640)` | `"reference": {"width": 360, "height": 640}` | 300 × 533 |
| Axis | `b.Axis = ScalingAxis.Height` | `"axis": "height"` | `smallestWidth` |
| Curve | `b.Curve = ScalingCurve.Power(0.8f)` | `"curve": {"type": "power", "exponent": 0.8}` | `linear` |
| Multiplier | `b.Multiply(0.9f)` (or `b.Multiplier = 0.9f`) | `"multiplier": 0.9` | 1 |
| Factor bounds | `b.Clamp(0.85f, 1.6f)` · `b.Clamp(max: 1.8f)` | `"clamp": {"min": 0.85, "max": 1.6}` | none |
| Value bounds (dp, ≥ 0) | `b.ClampValue(10, 32)` · `b.ClampValue(min: 48)` | `"valueClamp": {"min": 10, "max": 32}` | none |
| Aspect ratio | `b.EnableAspectRatio(sensitivity: 0.004f)` | `"aspectRatio": {"enabled": true, "sensitivity": 0.004}` | off, k = 0.08 / 30 |
| Font scale (`Csp`) | `b.FollowFontScale()` · `b.IgnoreFontScale()` · `b.ClampFontScale(1.3f)` | `"fontScale": "follow"` · `"ignore"` · `{"max": 1.3}` | `follow` |
| Multi-window | `b.IgnoreMultiWindow = true` | `"ignoreMultiWindow": true` | false |
| Per-value formula | `b.SetFormula("max(scaled, base)")` | `"formula": "max(scaled, base)"` | none |
| Per-value code hook | `b.Transform((baseValue, scaled, screen) => …)` | (code only) | none |
| Rules | `b.On(…)`, `b.OnScreen(…)`, `b.Landscape(…)`, `b.Portrait(…)`, `b.MultiWindow(…)`, `b.LargeScreens(…)` | `"rules": [{"when": {…}, "set": {…}}]` | none |
| Parent | `BuiltInStrategies.Power.Extend("x", b => …)` | `"extends": "power"` | none |

`SetFormula` and `Transform` replace each other: the last one set wins.

## Axes

| Axis | `ScalingAxis` | JSON | Screen value | Reference value |
|---|---|---|---|---|
| Smallest width | `SmallestWidth` | `smallestWidth` | `sw` | `min(refW, refH)` |
| Width · height | `Width` · `Height` | `width` · `height` | window width · height | `refW` · `refH` |
| Shorter · longer side | `Min` · `Max` | `min` · `max` | shorter · longer side | `min` · `max` of the reference |
| Diagonal | `Diagonal` | `diagonal` | `√(w² + h²)` | `√(refW² + refH²)` |
| Perimeter | `Perimeter` | `perimeter` | `w + h` | `refW + refH` |

The built-in `Hdp` divides the height by **300**. A custom `Height` axis divides by `reference.height` (533 by default), which is what a designer drawing on a full frame expects.

## Curves

| Curve | C# | JSON | Factor |
|---|---|---|---|
| Linear | `ScalingCurve.Linear` | `"linear"` | `r` (= `Sdp` with the default reference) |
| Power | `ScalingCurve.Power(0.8f)` | `{"type": "power", "exponent": 0.8}` | `r^e` (built-in power uses 0.75) |
| Logarithmic | `ScalingCurve.Logarithmic(0.4f)` | `{"type": "logarithmic", "k": 0.4}` | `1 + k·ln(r)` |
| Interpolated | `ScalingCurve.Interpolated(0.5f)` | `{"type": "interpolated", "t": 0.5}` | `1 + (r − 1)·t` |
| Constant | `ScalingCurve.Constant(1f)` | `{"type": "constant", "factor": 1}` | `f` (`Constant(1f)` = plain dp) |
| Breakpoints | `ScalingCurve.Breakpoints((360, 1f), (600, 1.25f))` | `{"type": "breakpoints", "points": [{"at": 360, "factor": 1}, …]}` | piecewise linear over the **absolute** axis value (dp), clamped to the first and last points |
| Text expression | `ScalingCurve.Expression("pow(ratio, 0.8)")` | `{"type": "expression", "expression": "…"}` | your formula ([text formulas](text-formulas.md)) |
| Code | `ScalingCurve.Custom((ratio, screen) => …)` | (code only) | your formula, pure |

## Bounds

- `b.Clamp(min, max)` limits the **factor**. If rules leave `min > max`, the max wins. Declared configs are validated up front (`ArgumentException: minFactor must be <= maxFactor`).
- `b.ClampValue(min, max)` limits the **resulting dp**. The bounds apply to the magnitude, so a negative offset mirrors a positive one and 0 stays 0; they must be ≥ 0 (`ArgumentException: minValue must be >= 0`). Value bounds make the strategy value-dependent (cached per value).

## Aspect ratio

`b.EnableAspectRatio()` multiplies the factor by `1 + k·ln(AR / 1.78)`, where `AR = LongSide / ShortSide` and the default `k` is the built-in 0.08 / 30. Raise `k` (`sensitivity:`) for a stronger correction on very long or very square screens.

## Font scale (`Csp`)

| Policy | Effect on `14.Csp(brand)` |
|---|---|
| `b.FollowFontScale()` (default) | follows the user font scale, like `Ssp` |
| `b.IgnoreFontScale()` | fixed size; the system font scale is cancelled |
| `b.ClampFontScale(1.3f)` | follows it up to 1.3×, then stops |

`Csp` returns **sp units** (the toolkit applies the system font scale, and the policy is pre-compensated); `CspDp` / `CspPx` return the final size with the policy applied, for toolkits that do not scale fonts themselves.

## Multi-window

`b.IgnoreMultiWindow = true` keeps values unscaled in split screen and multi-window modes (`IAppDimensContext.IsInMultiWindowMode`): the factor is 1, and formulas and transforms are skipped. Value bounds still apply. To react to split screen yourself instead, use a rule (`b.MultiWindow(r => …)`) or read `s.MultiWindow` in a formula.

## Rules

A rule changes some fields when its condition matches the window. The rule body `r` is a `ScalingPatchBuilder` with the same field members as `b` (no nested rules).

```csharp
ScalingStrategy.Define("brand", b =>
{
    b.Curve = ScalingCurve.Power(0.8f);
    b.On(new[] { DeviceClass.Tablet, DeviceClass.Desktop }, r => r.Curve = ScalingCurve.Logarithmic(0.4f), "large");
    b.OnScreen(r => r.Multiply(0.9f), minSmallestWidth: 600, landscape: true);
    b.On(DeviceClass.Watch, r => { r.Axis = ScalingAxis.Min; r.SetReference(192, 192); r.Clamp(0.85f, 1.15f); });
    b.Landscape(r => r.Multiply(0.95f));
    b.MultiWindow(r => r.Clamp(max: 1.2f));
    b.LargeScreens(r => r.ClampFontScale(1.2f));     // sw >= 600, any device class
});
```

| Condition | Builder | JSON `when` |
|---|---|---|
| Device class | `b.On(DeviceClass.Tv, …)` · `b.On(new[] { … }, …)` | `"device": "tv"` or `["tablet", "desktop"]` |
| Orientation | `b.Landscape(…)` · `b.Portrait(…)` · `OnScreen(…, landscape: true)` | `"orientation": "landscape"` |
| Smallest width / width / height (dp) | `b.OnScreen(…, minSmallestWidth: 600, maxWidth: 1200)` | `"smallestWidth": {"min": 600}`, `"width": {"max": 1200}`, `"height": {…}` |
| Aspect ratio | `b.OnScreen(…, minAspectRatio: 2)` | `"aspectRatio": {"min": 2}` |
| Split screen | `b.MultiWindow(…)` · `OnScreen(…, multiWindow: true)` | `"multiWindow": true` |
| Any combination | `b.On(new ScreenCondition(devices: new[] { DeviceClass.Tv }, landscape: true), …)` | several keys in one `when` |

- A condition matches when **every** field set in it matches. Ranges are `min <= value < max`.
- All matching rules apply **in declaration order**. A later rule overrides the fields an earlier one set; fields a rule leaves out keep their current value.
- Applied rule names show in `ExplainCurrent()`. `Landscape`, `Portrait`, `MultiWindow` and `LargeScreens` are named after themselves; other rules without a `name` show as `#index`.
- Device classes: `Phone`, `Tablet`, `Desktop`, `Tv`, `Watch`, `Car`, `Vr` (JSON `phone` … `vr`).

## Device class

`CustomDimens.DefaultDeviceClass(ctx)` decides the class of a window:

1. the context `UiModeType`: `Television` → `Tv`, `Car`, `Watch`, `Desk` → `Desktop`, `VrHeadset` → `Vr`;
2. on Windows, macOS, Mac Catalyst and Linux: `Desktop`;
3. otherwise (Android, iOS): `sw ≥ 600` → `Tablet`, else `Phone`.

Override it for a kiosk build, a TV box or tests (return `null` to fall back to the default):

```csharp
CustomDimens.DeviceClassResolver = ctx => IsKioskBuild ? DeviceClass.Tv : null;
```

## Extending

```csharp
var soft    = BuiltInStrategies.Power.Extend("soft", b => b.Curve = ScalingCurve.Power(0.6f));
var myTv    = CustomPresets.TvSafe.Extend("myTv", b => b.On(DeviceClass.Tv, r => r.Clamp(max: 1.6f)));
var heights = AppStrategies.Brand.WithAxis(ScalingAxis.Height);   // "brand@height", cached per strategy
```

`Extend(id, configure, register = true)` keeps the parent's fields and rules (the parent's rules first) and lets you change any of them.

### Built-ins as configs

`BuiltInStrategies.Scaled`, `Power`, `Logarithmic`, `Interpolated`, `Diagonal`, `Perimeter` and `Fixed` (ids `scaled` … `fixed`). Their factors match the built-in packages on the default smallest-width path (checked by `CustomStrategyTests.BuiltIns_MatchCoreMetrics`), so they are a safe starting point. `ScalingStrategies.Default` is `BuiltInStrategies.Scaled` until you set it (same numbers as `Sdp`).

### Presets (`CustomPresets`)

| Preset | Id | What it does |
|---|---|---|
| `TabletFriendly` | `tabletFriendly` | Linear on phones; `Power(0.5)` on tablets and desktop; factor ≤ 1.8 |
| `CompactFirst` | `compactFirst` | Reference 360×640, `Interpolated(0.5)`, factor 0.9–1.4 |
| `TvSafe` | `tvSafe` | On TV: height axis vs 540 dp, factor 1–2 |
| `Wearable` | `wearable` | On watch: shorter side vs 192 dp, factor 0.85–1.15 |
| `GameHud` | `gameHud` | Shorter side, `Power(0.85)`, aspect ratio on, 0.75–2.5, fixed-size text |
| `ReadableText` | `readableText` | `Logarithmic(0.4)`, font scale capped at 1.3 |

Presets are not registered by default, but `ScalingStrategies.Get("tvSafe")` and JSON `extends` find them by id.

[Back to the custom docs](README.md)

# AppDimens.Net.Custom

**Your own scaling strategy: you choose the reference frame, axis, curve (or a free-form formula), limits, font-scale policy and rules per device and screen. Define it in C# or JSON. The JSON format is the same on every AppDimens platform.**

Part of **AppDimens for .NET** · requires `AppDimens.Net` (installed automatically) · included in `AppDimens.Net.Sdk`.
Works with .NET MAUI, WPF, Windows Forms, WinUI 3 and plain .NET (`net8.0` / `net9.0` / `net10.0`).

> **Unreleased.** On `main`; ships with the next AppDimens.Net release. API reference: [API.md](API.md).

## Table of contents

- [Quick start: just write the formula](#quick-start-just-write-the-formula)
- [What it does](#what-it-does)
- [Install](#install)
- [Define a strategy](#define-a-strategy)
- [Use it](#use-it)
- [Configuration fields](#configuration-fields)
- [Curves](#curves)
- [Free-form formulas](#free-form-formulas-no-limits-from-the-built-in-curves)
- [Rules](#rules)
- [JSON](#json)
- [Presets and built-ins](#presets-and-built-ins)
- [Device class](#device-class)
- [Performance and caching](#performance-and-caching)
- [Debug](#debug)
- [Português (resumo)](#português-resumo)

## Quick start: just write the formula

The easiest way is to write the math yourself, as ordinary C# on one line. Inside the lambda, IntelliSense completes everything you can use (`s.Base`, `s.Sw`, `s.Width`, `s.Tablet`, `s.Pow`, `s.Clamp`…), and a typo is a compile error. There is nothing to configure.

```csharp
using AppDimens.Net.Custom;

public static class AppStrategies
{
    // 1. Formula: the result is the final value (Base = the value you pass, e.g. 16)
    public static readonly ScalingStrategy Hud = ScalingStrategy.Formula(s => s.Base * s.Pow(s.Sw / 360f, 0.8f));
    public static readonly ScalingStrategy Card = ScalingStrategy.Formula(s => s.Tablet ? s.Base * 1.2f : s.Base * s.Ratio);

    // 2. Factor: the result multiplies every value
    public static readonly ScalingStrategy Product = CustomScaleStrategy.Create(s => s.Clamp(s.Width / 360f, 0.85f, 1.80f));
    public static readonly ScalingStrategy Living = CustomScaleStrategy.Create(s => s.Tv ? s.Height / 540f : s.Ratio);
}

var padding = 16.Cdp(AppStrategies.Product);   // 16 × factor
var score = 18.Csp(AppStrategies.Hud);          // text follows the user font scale
```

Declare each strategy once (for example as a `static readonly` field), then use it anywhere. `CustomDimens.Formula(…)` and `CustomDimens.CustomScaleStrategy(…)` are the same factories. Pass `id: "hud"` to give one a name and register it; otherwise it gets a unique `formula#N` / `scale#N` id.

### What you can use inside the lambda

| Name | Meaning |
|---|---|
| `Base` | the value you pass (`16` in `16.Cdp(Hud)`), only in `ScalingStrategy.Formula(s => …)` |
| `Sw` · `Width` · `Height` | smallest width · window width · window height (dp) |
| `ShortSide` · `LongSide` · `Diagonal` · `Perimeter` · `AspectRatio` | other window measures |
| `Ratio` | `Sw / 300`, the ratio `Sdp` uses (`Base * Ratio` is the same as `Sdp`) |
| `Phone` `Tablet` `Desktop` `Tv` `Watch` `Car` `Vr` · `Landscape` `Portrait` `MultiWindow` | `bool` |
| `Density` · `FontScale` · `Device` · `Screen` | screen density · user font scale · `DeviceClass` · the whole `ScreenSnapshot` |
| `Pow` `Sqrt` `Ln` `Log10` `Exp` `Abs` `Round` `Min` `Max` `Clamp` `Lerp` `Smoothstep` | math on `float`, no cast needed |

Anything else from C# or your own code also works (`switch`, `Math.Clamp`, your helpers). Strategies must be pure: they must not read mutable state, because results are memoized per window.

| Form | You return | Cost per value | JSON |
|---|---|---|---|
| `CustomScaleStrategy.Create(s => …)` | the **factor** | one multiply (runs once per window) | no |
| `ScalingStrategy.Formula(s => …)` | the final value | cached per value and window | no |
| `ScalingStrategy.Formula("…")` / `16.Cdp("…")` | the final value, as text | one multiply when shaped `base * (…)`, otherwise cached | yes |

### Text formulas (for JSON and remote config)

When the formula has to live outside the code, in a JSON file, a remote config or an A/B test, write the same thing as text. It uses the same names in camelCase (`base`, `sw`, `tablet`…; flags are 1/0):

```csharp
var hud = ScalingStrategy.Formula("base * pow(sw / 360, 0.8)");
var padding = 16.Cdp("base * (tablet ? 0.9 : 1) * clamp(sw / 360, 0.85, 1.6)");   // inline
```

Every helper takes the formula text: `Cdp` (dp), `CdpPx` (px), `Csp` (sp units), `CspDp` / `CspPx` (final text size), on `int` and `float`, with or without an explicit `IAppDimensContext`. `ScalingStrategies.Formula("…")` and `CustomDimens.Formula("…")` return the same shared strategy.

Text is checked when the strategy is created: an unknown name fails right away with its position and the list of valid names (e.g. `unknown variable 'sww' (allowed: aspectRatio, base, …)`). The same text works in the strategies JSON (`{"id": "hud", "formula": "base * pow(sw / 360, 0.8)"}`) and gives the same numbers on every AppDimens platform. Operators and functions are listed in [Free-form formulas](#free-form-formulas-no-limits-from-the-built-in-curves).

**Sharing.** Without an id, `ScalingStrategy.Formula("…")` returns one shared, unregistered strategy per formula text (id `formula:<text>`; up to 256 texts are kept, more still work but are built per call). With an id, `ScalingStrategy.Formula("…", id: "hud")` creates a new strategy and registers it (`register: false` to opt out).

**Fast path.** A text formula shaped `base * (…)`, where the `(…)` part does not read `base`, costs one multiply per value, because the `(…)` part is computed once per window and the shared cache is skipped. `base * sw / 360` is evaluated as written, `(base * sw) / 360`, so write `base * (sw / 360)` to get the fast path. In code, prefer `CustomScaleStrategy.Create(s => …)` for the same effect.

For rules per device, factor bounds or a font-scale policy, use the full configuration below.

## What it does

The built-in strategies (Scaled, Power, Logarithmic…) each follow one fixed growth law. A **custom strategy** is a configuration:

```
ratio  = axis(window) / axis(reference)
factor = clamp(curve(ratio) × multiplier × AR, minFactor, maxFactor)
dp     = formula(clampValue(base × factor))
```

Rules adjust any of these fields on TVs, watches, tablets, landscape, split-screen or screen-size ranges.

## Install

```bash
dotnet add package AppDimens.Net.Custom      # or AppDimens.Net.Sdk (everything)
```

## Define a strategy

```csharp
using AppDimens.Net.Custom;

public static class AppStrategies
{
    public static readonly ScalingStrategy Brand = ScalingStrategy.Define("brand", b =>
    {
        b.SetReference(360, 640);                       // design frame (dp)
        b.Curve = ScalingCurve.Power(0.8f);             // sublinear growth
        b.Clamp(0.85f, 1.6f);                           // factor bounds
        b.ClampFontScale(1.3f);                         // Csp follows font scale up to 1.3×
        b.On(new[] { DeviceClass.Tablet, DeviceClass.Desktop }, r => r.Curve = ScalingCurve.Logarithmic(0.4f), "large");
        b.Landscape(r => r.Multiply(0.95f));
    });
}
```

`Define` registers the strategy (`ScalingStrategies.Get("brand")`). Pass `register: false` for private ones. Extend a built-in or a preset: `BuiltInStrategies.Power.Extend("soft", b => b.Curve = ScalingCurve.Power(0.6f))`.

## Use it

Initialize your UI framework as usual (MAUI `AppDimensMaui.AttachWindow(window)` · WPF `AppDimensWpf.Initialize(this)` · WinForms `AppDimensWinForms.Initialize(this)` · WinUI `AppDimensWinUI.Initialize(window)`).

| Call | Returns |
|---|---|
| `16.Cdp(AppStrategies.Brand)` / `16.Cdp()` | dp (no argument: scoped strategy, else `ScalingStrategies.Default`) |
| `16.CdpPx(AppStrategies.Brand)` | pixels |
| `14.Csp(AppStrategies.Brand)` | **sp units**, like `Ssp`: the toolkit applies the system font scale, and the strategy policy is pre-compensated |
| `14.CspDp(brand)` / `14.CspPx(brand)` | final text size in dp / px (policy applied) |
| `16.Cdp(brand, ctx)` | explicit `IAppDimensContext` (no ambient window) |
| `brand.Dp(16)`, `brand.Sp(14)`, `brand.Px(16)` | strategy-first helpers |

```csharp
ScalingStrategies.Default = AppStrategies.Brand;   // every 16.Cdp() in the app
using (CustomDimens.Use(AppStrategies.Brand))      // or only inside this async flow
{
    BuildCard();
}
```

## Configuration fields

| Field | C# builder | JSON | Default |
|---|---|---|---|
| Reference frame (dp) | `b.SetReference(360, 640)` | `"reference": {"width": 360, "height": 640}` | 300 × 533 |
| Axis | `b.Axis = ScalingAxis.Height` | `"axis": "height"` | `smallestWidth` |
| Curve | `b.Curve = ScalingCurve.Power(0.8f)` | `"curve": {"type": "power", "exponent": 0.8}` | `linear` |
| Multiplier | `b.Multiply(0.9f)` | `"multiplier": 0.9` | 1 |
| Factor bounds | `b.Clamp(0.85f, 1.6f)` | `"clamp": {"min": 0.85, "max": 1.6}` | none |
| Value bounds (dp) | `b.ClampValue(10, 32)` | `"valueClamp": {"min": 10, "max": 32}` | none |
| Aspect ratio | `b.EnableAspectRatio(sensitivity: 0.004f)` | `"aspectRatio": {"enabled": true, "sensitivity": 0.004}` | off, k = 0.08 / 30 |
| Font scale (`Csp`) | `b.FollowFontScale()` · `b.IgnoreFontScale()` · `b.ClampFontScale(1.3f)` | `"fontScale": "follow"` · `"ignore"` · `{"max": 1.3}` | `follow` |
| Multi-window | `b.IgnoreMultiWindow = true` | `"ignoreMultiWindow": true` | false |
| Per-value formula | `b.SetFormula("max(scaled, base)")` | `"formula": "max(scaled, base)"` | none |
| Per-value hook (code) | `b.Transform((baseValue, scaled, screen) => …)` | — (code only) | none |
| Rules | `b.On(…)`, `b.OnScreen(…)`, `b.Landscape(…)`, `b.Portrait(…)`, `b.MultiWindow(…)`, `b.LargeScreens(…)` | `"rules": [{"when": {…}, "set": {…}}]` | none |
| Parent | `BuiltInStrategies.Power.Extend("x", …)` | `"extends": "power"` | — |

Axes: `SmallestWidth`, `Width`, `Height`, `Min`, `Max`, `Diagonal` (√(w² + h²)), `Perimeter` (w + h). The reference value is the same measure of the reference frame.

## Curves

| Curve | Factor |
|---|---|
| `ScalingCurve.Linear` | `r` (same as `Sdp` with the default reference) |
| `ScalingCurve.Power(e)` | `r^e` |
| `ScalingCurve.Logarithmic(k)` | `1 + k·ln(r)` |
| `ScalingCurve.Interpolated(t)` | `1 + (r − 1)·t` |
| `ScalingCurve.Constant(f)` | `f` |
| `ScalingCurve.Breakpoints((360, 1f), (600, 1.25f))` | piecewise linear over the **absolute** axis value (dp), clamped at the ends |
| `ScalingCurve.Expression("…")` | your formula, as text (JSON-serializable) — see below |
| `ScalingCurve.Custom((ratio, screen) => …)` | your formula, as code (pure; not serializable) |

## Free-form formulas (no limits from the built-in curves)

You are not limited to the built-in curves. Write the formula yourself, as **text** that also works in JSON and gives the same result on every AppDimens platform:

- **Curve expression** returns the **factor** from the screen. It is computed once per window, like any curve, and stays on the one-multiply fast path.
  `"curve": {"type": "expression", "expression": "pow(ratio, 0.8) * (tablet ? 0.9 : 1)"}`
- **Formula** returns the **final value** for each input. It can use `base`, `scaled` and `factor`, and replaces or post-processes the result. The strategy then becomes value-dependent, so results are cached per window.
  `"formula": "base * pow(sw / 360, 0.7) + 2"` · `"formula": "max(scaled, base)"`

| | |
|---|---|
| Operators | `+ - * / % ^` (`^` = power, right-associative; `%` keeps the dividend's sign), `== != < <= > >=`, `&& \|\| !`, `cond ? a : b`, `( )` |
| Functions | `min(a, b, …)` `max(a, b, …)` `clamp(x, lo, hi)` `pow` `sqrt` `ln`/`log` `log10` `log2` `exp` `abs` `sign` `floor` `ceil` `round` (half up) `lerp(a, b, t)` `step(edge, x)` `smoothstep(e0, e1, x)` |
| Screen variables | `sw` `width` `height` `shortSide` `longSide` `diagonal` `perimeter` `aspectRatio` `density` `fontScale`; flags (1/0): `landscape` `portrait` `multiWindow` `phone` `tablet` `desktop` `tv` `watch` `car` `vr`; constants `pi` `e`, `true` / `false` |
| Curve variables | screen variables + `ratio` (axis / reference), `axis` (dp), `ref` (reference dp), `refWidth`, `refHeight` |
| Formula variables | curve variables + `base` (input), `scaled` (base × factor after `valueClamp`), `factor` |

Expressions are parsed when the strategy is created. Unknown variables or functions, wrong arity and syntax errors fail early with the position and the JSON path (e.g. `brand.rules[0].set.curve.expression`). A non-finite factor (for example, a division by zero) throws when the strategy resolves. Rules can swap expressions per device like any other field, and `"formula": null` clears an inherited formula.

For anything the language cannot express, use a code-only delegate: `ScalingCurve.Custom((ratio, screen) => …)` (factor) or `b.Transform((baseValue, scaled, screen) => …)` (value). Both have full access to your code, but they cannot be serialized to JSON.

```csharp
var hud = ScalingStrategy.Define("hud", b =>
{
    b.Curve = ScalingCurve.Expression("clamp(pow(shortSide / 360, 0.85) * (1 + 0.05 * log(aspectRatio / 1.78)), 0.8, 2)");
    b.SetFormula("round(scaled)");                         // whole dp only
    b.On(DeviceClass.Tv, r => r.Curve = ScalingCurve.Expression("height / 540"));
});
```

## Rules

- A condition matches when **every** field set in it matches. Ranges are `min <= value < max` (dp).
- All matching rules apply **in declaration order**; a later rule overrides the fields an earlier one set.
- Applied rule names appear in `Explain` (`#index` for unnamed rules).
- If combined rules leave crossed bounds, the **max** bound wins. Declared configs are validated up front.

```csharp
b.OnScreen(r => r.Curve = ScalingCurve.Power(0.6f), minSmallestWidth: 600, maxSmallestWidth: 840, name: "medium");
b.On(new ScreenCondition(devices: new[] { DeviceClass.Tv }, landscape: true), r => r.Axis = ScalingAxis.Height);
```

## JSON

```json
{ "version": 1, "default": "brand",
  "strategies": [
    { "id": "brand", "extends": "power",
      "reference": { "width": 360, "height": 640 },
      "curve": { "type": "power", "exponent": 0.8 },
      "clamp": { "min": 0.85, "max": 1.6 },
      "rules": [ { "name": "large", "when": { "device": ["tablet", "desktop"] },
                   "set": { "curve": { "type": "expression", "expression": "1 + 0.4 * ln(ratio)" } } } ] } ] }
```

```csharp
ScalingStrategies.LoadJson(json);                     // registers every strategy, applies "default"
CustomExtensions.LoadFile("appdimens-strategies.json");
string exported = ScalingStrategies.ExportJson();
```

`extends` resolves, in order, to the same document, registered strategies, built-ins and presets; cycles are rejected. Errors carry the JSON path. The format is described by `custom-strategy.schema.json` in [appdimens-skills](https://github.com/bodenberg/appdimens-skills).

## Presets and built-ins

| Preset | Behavior |
|---|---|
| `CustomPresets.TabletFriendly` | linear on phones; `power(0.5)` on tablet/desktop; factor ≤ 1.8 |
| `CustomPresets.CompactFirst` | reference 360×640, `interpolated(0.5)`, factor 0.9–1.4 |
| `CustomPresets.TvSafe` | on TV: height axis vs 540 dp, factor 1–2 |
| `CustomPresets.Wearable` | on watch: shorter side vs 192 dp, ±15 % |
| `CustomPresets.GameHud` | shorter side, `power(0.85)`, aspect ratio, 0.75–2.5, fixed text |
| `CustomPresets.ReadableText` | `logarithmic(0.4)`, font scale up to 1.3× |

Built-ins as custom configs: `BuiltInStrategies.{Scaled, Power, Logarithmic, Interpolated, Diagonal, Perimeter, Fixed}` (same factors as the modules).

## Device class

`CustomDimens.DefaultDeviceClass` uses the context `UiModeType` first (`Television` → `tv`, `Car`, `Watch`, `Desk` → `desktop`, `VrHeadset` → `vr`). On Windows, macOS, Mac Catalyst and Linux it then returns `desktop`. Otherwise `sw ≥ 600` means `tablet`, else `phone`. To override it:

```csharp
CustomDimens.DeviceClassResolver = ctx => IsKioskBuild ? DeviceClass.Tv : null;   // null = default
```

## Performance and caching

- Each strategy memoizes its resolution per window snapshot (`DimenMetrics`). Factor-only strategies cost one multiply.
- A formula shaped `base * (…)` (with no `base`, `scaled` or `factor` inside the `(…)`) is screen-only: the `(…)` part is computed once per window, with exactly the same result as evaluating the formula per value.
- Value-dependent strategies (value clamps, other formulas, transforms) go through the shared `DimenCache` with `CalcType.Custom` keys. The strategy id is part of the key, so strategies never alias each other.
- Expressions are parsed once, when the strategy is created.

## Debug

```csharp
Console.WriteLine(AppStrategies.Brand.ExplainCurrent());
// [brand] tablet smallestWidth=800dp / ref 360 = 2.2222 -> Logarithmic(0.4) = 1.3194 => factor 1.3194 rules=[large]
```

## Android binding

The pure .NET `AppDimens.Net.Custom` package works on Android as well. The Kotlin `appdimens-dynamic-custom` AAR will be added to the Android bindings once it is published.

## Português (resumo)

Forma mais simples: uma linha de C# tipado, com autocomplete e os mesmos nomes da linguagem em texto: `CustomScaleStrategy.Create(s => s.Clamp(s.Width / 360f, 0.85f, 1.8f))` (fator, roda uma vez por janela) ou `ScalingStrategy.Formula(s => s.Base * s.Pow(s.Sw / 360f, 0.8f))` (valor final; `Base` = valor passado). Em texto, para JSON/config remota: `ScalingStrategy.Formula("base * pow(sw / 360, 0.8)")` ou `16.Cdp("…")`; fórmulas no formato `base * (…)` ficam no caminho rápido.

Estratégias de escala próprias: frame de referência, eixo, curva (ou **fórmula livre em texto**, `ScalingCurve.Expression("…")` / `b.SetFormula("…")`), limites, política de fonte e regras por dispositivo/tela, em C# ou JSON (mesmo formato em todas as plataformas AppDimens). Use `16.Cdp(brand)`, `14.Csp(brand)`, `ScalingStrategies.Default` ou `CustomDimens.Use(brand)`. Estratégias só de fator custam uma multiplicação; as dependentes do valor usam o `DimenCache`.

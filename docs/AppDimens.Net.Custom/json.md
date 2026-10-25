# JSON format

One JSON document describes any number of strategies. The same file loads on every AppDimens platform and gives the same numbers, so a design system can ship it as a design token, through remote config or for an A/B test.

```csharp
ScalingStrategies.LoadJson(json);                                   // registers all + applies "default"
CustomExtensions.LoadFile("appdimens-strategies.json");             // same, from a file path
var brand = ScalingStrategies.Require("brand");
string one = brand.ToJson();                                        // export one strategy
string all = ScalingStrategies.ExportJson();                        // every registered strategy + the default id
```

`LoadJson` and `LoadFile` return the parsed strategies as an `IReadOnlyDictionary<string, ScalingStrategy>`. In MAUI, read a packaged file with `FileSystem.OpenAppPackageFileAsync("appdimens-strategies.json")` and pass the text to `LoadJson`. `ScalingStrategyJson.Decode(json)` parses without registering anything (for tools and tests).

## Document

```json
{
  "$schema": "https://github.com/bodenberg/appdimens-skills/schemas/custom-strategy.schema.json",
  "version": 1,
  "default": "brand",
  "strategies": [
    {
      "id": "brand",
      "reference": { "width": 360, "height": 640 },
      "curve": { "type": "power", "exponent": 0.8 },
      "clamp": { "min": 0.85, "max": 1.6 },
      "fontScale": { "max": 1.3 },
      "rules": [
        { "name": "large", "when": { "device": ["tablet", "desktop"] },
          "set": { "curve": { "type": "logarithmic", "k": 0.4 } } },
        { "when": { "smallestWidth": { "min": 600 }, "orientation": "landscape" },
          "set": { "multiplier": 0.9 } }
      ]
    },
    { "id": "brandText", "extends": "brand", "valueClamp": { "min": 10, "max": 32 } },
    { "id": "hud", "formula": "base * pow(sw / 360, 0.8)" }
  ]
}
```

| Key | Meaning |
|---|---|
| `version` | format version, currently 1 (`ScalingStrategyJson.FormatVersion`); a higher version is rejected |
| `default` | id that becomes `ScalingStrategies.Default` (optional) |
| `strategies[]` | the strategies, each with a unique `id` |

A single strategy object or a bare array of strategies is also accepted.

## Strategy fields

| Field | Example | Notes |
|---|---|---|
| `id` | `"brand"` | required, unique in the document |
| `extends` | `"power"` | another strategy of the document, a registered one, a built-in (`scaled` `power` `logarithmic` `interpolated` `diagonal` `perimeter` `fixed`) or a preset (`tabletFriendly`…). The parent's rules come first |
| `reference` | `{"width": 360, "height": 640}` | design frame (dp) |
| `axis` | `"smallestWidth"` | `smallestWidth` `width` `height` `min` `max` `diagonal` `perimeter` |
| `curve` | `"linear"` or `{"type": …}` | see the [curve table](configuration.md#curves) |
| `multiplier` | `0.9` | |
| `clamp` | `{"min": 0.85, "max": 1.6}` | factor bounds; `null` clears inherited bounds |
| `valueClamp` | `{"min": 10, "max": 32}` | dp bounds, ≥ 0; `null` clears |
| `aspectRatio` | `true` or `{"enabled": true, "sensitivity": 0.004}` | |
| `sensitivity` | `0.004` | aspect-ratio k on its own (useful in a rule) |
| `fontScale` | `"follow"` · `"ignore"` · `{"max": 1.3}` | `Csp` policy |
| `ignoreMultiWindow` | `true` | |
| `formula` | `"max(scaled, base)"` | per-value [text formula](text-formulas.md); `null` clears an inherited one |
| `rules` | `[{"name": …, "when": {…}, "set": {…}}]` | `set` takes any field above except `id`, `extends` and `rules` |

## Rule conditions (`when`)

| Key | Example |
|---|---|
| `device` | `"tv"` or `["tablet", "desktop"]` (`phone` `tablet` `desktop` `tv` `watch` `car` `vr`) |
| `orientation` | `"landscape"` or `"portrait"` |
| `smallestWidth` · `width` · `height` · `aspectRatio` | `{"min": 600, "max": 840}`, with `min <= value < max` |
| `multiWindow` | `true` |

An omitted `when` matches every screen.

## Validation

Loading is strict and atomic: any error throws `ArgumentException` (with the JSON path) or `KeyNotFoundException` (unknown `default`), and **nothing** is registered. Examples (messages as thrown):

| Input | Error |
|---|---|
| `"curve": {"type": "cubic"}` | `brand.curve.type 'cubic' is not one of linear, power, logarithmic, interpolated, constant, breakpoints, expression` |
| `"when": "tablet"` | `brand.rules[0].when must be an object` |
| `"width": 600` in a condition | `brand.rules[0].when.width must be an object with min/max` |
| `"ignoreMultiWindow": "true"` | `brand.ignoreMultiWindow must be a boolean` |
| `"axis": "foo"` | `Unknown axis 'foo'. Expected one of [smallestWidth, width, height, min, max, diagonal, perimeter]` |
| `"valueClamp": {"max": -1}` | `maxValue must be >= 0` |
| `"formula": "base * sww"` | `brand.formula: Invalid expression at 7: unknown variable 'sww' (allowed: …)` |
| `"default": "missing"` | `KeyNotFoundException: No scaling strategy 'missing'. Registered: […]`; nothing is registered |
| duplicate ids · `extends` cycles · unknown parent | `Duplicate strategy id 'brand'` · `Cyclic 'extends': a -> b -> a` · `'x' extends unknown strategy 'y'` |
| malformed text | `Invalid JSON at <position>: …` |

Check a file before shipping it with the AppDimens kit (`npx appdimens-skills kit custom check <file>`), which uses the same rules and the [JSON schema](https://github.com/bodenberg/appdimens-skills/blob/main/schemas/custom-strategy.schema.json).

## Export

`strategy.ToJson()` writes one strategy object and `ScalingStrategies.ExportJson()` a whole document; both reload unchanged. Code lambdas (`CustomScaleStrategy.Create(s => …)`, `ScalingStrategy.Formula(s => …)`, `ScalingCurve.Custom(…)`, `b.Transform(…)`) cannot be exported and throw `InvalidOperationException` (`Custom curve lambdas cannot be encoded to JSON` / `ValueTransform lambdas cannot be encoded to JSON`). Use their text equivalents when you need JSON.

[Back to the custom docs](README.md)

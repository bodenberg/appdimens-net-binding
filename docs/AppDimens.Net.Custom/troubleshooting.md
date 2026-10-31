# Troubleshooting

## See what a strategy does

```csharp
Console.WriteLine(AppStrategies.Brand.ExplainCurrent());        // ambient window
Debug.WriteLine(AppStrategies.Brand.ExplainCurrent(ctx));       // explicit context
```

```
[brand] tablet smallestWidth=800dp / ref 360 = 2.2222 -> Logarithmic(0.4) = 1.3194 x 0.9 clamp[0.85, 1.6] => factor 1.1875 rules=[large, #1]
[formula:base * pow(sw / 360, 0.8)] phone smallestWidth=411dp / ref 300 = 1.37 -> Linear = 1.37 => factor 1.37 then Formula(base * pow(sw / 360, 0.8))
[formula#1] phone smallestWidth=411dp / ref 300 = 1.37 -> Linear = 1.37 => factor 1.37 then transform
```

The line reads left to right: device class, axis value and reference, the curve and its result, multiplier, aspect ratio, bounds, the final factor, then the per-value formula (`then Formula(…)`) or code formula / transform (`then transform`), and the applied rules. `CurrentFactor()` returns only the number. Without a window, use `strategy.Explain(new ScreenSnapshot(800, 1280))`.

## Common errors

| Exception and message | Cause | Fix |
|---|---|---|
| `ArgumentException: Invalid expression at 7: unknown variable 'sww' …` | typo in a text formula | use a name from the list in the message, or switch to a [code formula](code-formulas.md), which the compiler checks |
| `ArgumentException: … 'clamp' takes 3 argument(s), got 2` | wrong number of arguments | see the [function table](text-formulas.md#functions) |
| `InvalidOperationException: custom strategy produced a non-finite factor (NaN)` | division by zero, `ln` of 0, NaN from a lambda | guard the formula (`max(x, 1)`) or clamp the factor |
| `ArgumentException: <path> must be an object` / `must be a boolean` | wrong JSON type | see the [field tables](json.md#strategy-fields) |
| `ArgumentException: maxValue must be >= 0` | negative value bound | value bounds limit the magnitude; use a positive number |
| `ArgumentException: minFactor must be <= maxFactor` | crossed bounds in the declaration | swap them (crossed bounds produced by rules at runtime let the max win) |
| `KeyNotFoundException: No scaling strategy 'x'. Registered: […]` | id not registered (`Require`, JSON `default`) | register it, load the JSON first, or check the id |
| `InvalidOperationException: ValueTransform lambdas cannot be encoded to JSON` | `ToJson()` / `ExportJson()` with a code lambda | use the text form for strategies you export |
| `InvalidOperationException: AppDimens ambient context is not initialized …` | parameterless call before the UI framework was initialized | initialize it ([quick start](quick-start.md#2-create-the-window-context)) or pass an `IAppDimensContext` |

## Frequent questions

**My lambda result never changes.** The factor is memoized per window. A lambda that reads mutable state (a setting, the time) is not re-run until the window changes. Use separate strategies and pick one, or make the setting part of the screen (a rule).

**`16.Cdp(product)` and `16.Sdp()` differ.** Expected: the default reference is 300 dp of smallest width; `Ratio` is `Sw / 300`. `s.Base * s.Ratio` (or `BuiltInStrategies.Scaled`) gives exactly `Sdp`.

**My tablet rule never applies on Windows / macOS.** On desktop operating systems the device class is `desktop`. Target `DeviceClass.Desktop` too (`b.On(new[] { DeviceClass.Tablet, DeviceClass.Desktop }, …)`), use a size rule (`b.OnScreen(…, minSmallestWidth: 600)`), or set `CustomDimens.DeviceClassResolver`.

**My text formula is slow / misses the fast path.** Only `base * E` qualifies. `base * sw / 360` reads as `(base * sw) / 360`; write `base * (sw / 360)`.

**Strategies created in a handler leak cache ids.** `CustomScaleStrategy.Create(…)` and `ScalingStrategy.Formula(s => …)` build a new strategy on each call. Declare them once as `static readonly` fields. Inline text formulas (`16.Cdp("…")`) are shared per text and are fine.

**Fonts ignore my strategy.** `Csp` follows the user font scale by default. Use `b.IgnoreFontScale()` for fixed text, or `b.ClampFontScale(max)` to cap it.

**Are the numbers the same on Android / iOS / Flutter / Web?** Yes for everything that can be written as JSON (curves, rules, text formulas): a shared 540-case fixture checks every platform. Code lambdas run your own code, so they match as long as the code does.

[Back to the custom docs](README.md)

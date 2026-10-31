# AppDimens.Net.Custom — API reference

Guides: [overview](README.md) · [quick start](quick-start.md) · [usage reference](api-reference.md) (overload matrix, registry, debug helpers). This page lists the public types.

## Assemblies

| Package | Assembly | Namespace |
|---|---|---|
| `AppDimens.Net.Custom` | `AppDimens.Net.Custom.dll` | `AppDimens.Net.Custom` |

## Engine (portable — the same sources ship in the Unity port of appdimens-games)

| Type | Members |
|---|---|
| `ScalingStrategy` | `Define(id, Action<ScalingStrategyBuilder>, register = true)`, `Formula(Func<FormulaScope, float>, id?, register?)` (typed code, `formula#N`), `Formula(string formula, id?, register?)` (text; shared per text without an id), ctor `(id, ScalingConfig)`, `Id`, `Config`, `CacheId`, `IsValueDependent`, `Resolve(ScreenSnapshot)`, `Resolve(token, Func<ScreenSnapshot>)`, `Factor`, `Scale`, `Explain`, `Extend(id, …)`, `WithAxis(axis)`, `ToJson()` |
| `ScreenScope` / `FormulaScope` | typed lambda scopes with the text-language names in PascalCase: `Screen`, `Sw`, `Width`, `Height`, `ShortSide`, `LongSide`, `Diagonal`, `Perimeter`, `AspectRatio`, `Density`, `FontScale`, `Ratio`, `Device`; `bool` `Landscape` `Portrait` `MultiWindow` `Phone` `Tablet` `Desktop` `Tv` `Watch` `Car` `Vr`; `Pow` `Sqrt` `Ln` `Log10` `Exp` `Abs` `Round` `Min` `Max` `Clamp` `Lerp` `Smoothstep`; `FormulaScope` adds `Base` |
| `CustomScaleStrategy` | `Create(Func<ScreenScope, float>, id?, register?)`: programmable factor as a code curve (screen-only, once per window, `scale#N`) |
| `ScalingStrategies` | `Default`, `Register`, `Get`, `Require`, `Ids`, `Unregister`, `LoadJson`, `ExportJson`, `Formula(text)` (shared, bounded at 256 texts), `ResetForTesting`, `MaxCacheId` |
| `ScalingStrategyBuilder` / `ScalingPatchBuilder` | `Axis`, `Curve`, `Multiplier`, `FontScale`, `IgnoreMultiWindow`, `SetReference`, `Clamp`, `ClampValue`, `EnableAspectRatio`, `Transform`, `SetFormula`, `Multiply`, `FollowFontScale`, `IgnoreFontScale`, `ClampFontScale`; root only: `On(…)`, `OnScreen(…)`, `Landscape`, `Portrait`, `MultiWindow`, `LargeScreens` |
| `ScalingConfig` | immutable: `Reference`, `Axis`, `Curve`, `Multiplier`, `MinFactor`, `MaxFactor`, `MinValue`, `MaxValue`, `AspectRatio`, `Sensitivity`, `FontScale`, `IgnoreMultiWindow`, `Transform`, `Formula`, `Rules`, `IsValueDependent`, `Validate`, `Apply(patch)`, `With…` |
| `ScalingCurve` | `Linear`, `Power(e)`, `Logarithmic(k)`, `Interpolated(t)`, `Constant(f)`, `Breakpoints(...)`, `Expression(source)`, `Custom(CurveFunction)` |
| `ScalingExpression` | `Parse(source, allowed?)`, `Evaluate(Func<string, double>)`, `Variables(...)`, `Source`, `ScreenVariables`, `CurveVariables`, `FormulaVariables` |
| `Formula` | `new Formula(source)`, `Expression`, `IsProportional` (`base * E` with screen-only `E`: one multiply, no per-value cache), `AsTransform()` |
| `FontScalePolicy` | `Follow`, `Ignore`, `Clamp(max)`, `Effective(systemFontScale)` |
| `ScreenCondition`, `ScalingPatch`, `ScalingRule` | rule model (`ScalingRule.On(devices, patch, name)`) |
| `ScreenSnapshot`, `ReferenceScreen`, `Breakpoint` | inputs |
| `ScalingAxis`, `DeviceClass`, `CustomNames` | enums, JSON names, `FromAndroidUiMode`, `FromSmallestWidth` |
| `ResolvedScaling` | `Factor`, `AppliedRules`, `Config`, `Screen`, `Scale(base)`, `ScaleSpUnits(base)`, `ScaleSpDp(base)`, `Explain(id)` |
| `ScalingStrategyJson` | `Decode`, `DecodeStrategy`, `Encode`, `EncodeDocument`, `FormatVersion` |
| `BuiltInStrategies`, `CustomPresets` | see the guide |
| `MiniJson` | dependency-free JSON parser used by the codec |

## .NET bridge

| Type | Members |
|---|---|
| `CustomDimens` | `Formula(text, id?, register?)`, `Formula(Func<FormulaScope, float>, id?, register?)`, `CustomScaleStrategy(Func<ScreenScope, float>, id?, register?)`, `Current`, `Use(strategy)` (`IDisposable`, async-flow scoped), `DeviceClassResolver`, `DefaultDeviceClass(ctx)`, `ToScreenSnapshot(this DimenMetrics, DeviceClass)`, `ScreenSnapshot(this IAppDimensContext)`, `Resolve(this ScalingStrategy, IAppDimensContext)`, `CacheKey(this ScalingStrategy, base, valueType)` |
| `CustomExtensions` | `Cdp`, `CdpPx`, `Csp` (sp units), `CspDp`, `CspPx` on `int` / `float` / `double` with optional strategy and/or `IAppDimensContext`; every one also takes a formula string (`16f.Cdp("base * pow(sw / 360, 0.8)")`, `14.CspPx("…", ctx)`); `Dp`, `Sp`, `Px`, `CurrentFactor`, `ExplainCurrent` on `ScalingStrategy`; `LoadFile(path)` |

## Core additions (`AppDimens.Net`)

- `DimenCache.CalcType.Custom` (value 15), `DimenCache.MaxCustomStrategyKey`, `DimenCache.BuildCustomKey(baseValue, strategyKey, isLandscape, ignoreMultiWindows, valueType, cacheable)`. Bit 2 of a custom key marks it cacheable; factor-only keys bypass the cache.

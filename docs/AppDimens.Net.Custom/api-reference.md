# API reference

Usage-oriented reference of `AppDimens.Net.Custom` (namespace `AppDimens.Net.Custom`). The list of every public type and member is in [API.md](API.md).

## Building strategies

| Member | Returns | Notes |
|---|---|---|
| `CustomScaleStrategy.Create(Func<ScreenScope, float> factor, string? id = null, bool? register = null)` | `ScalingStrategy` | programmable factor, id `scale#N` without an id ([code formulas](code-formulas.md)) |
| `ScalingStrategy.Formula(Func<FormulaScope, float> formula, string? id = null, bool? register = null)` | `ScalingStrategy` | code formula, id `formula#N` without an id |
| `ScalingStrategy.Formula(string formula, string? id = null, bool? register = null)` | `ScalingStrategy` | text formula, shared per text without an id ([text formulas](text-formulas.md)) |
| `ScalingStrategy.Define(string id, Action<ScalingStrategyBuilder> configure, bool register = true)` | `ScalingStrategy` | full configuration ([configuration](configuration.md)) |
| `strategy.Extend(string id, Action<ScalingStrategyBuilder> configure, bool register = true)` | `ScalingStrategy` | child of an existing strategy |
| `strategy.WithAxis(ScalingAxis axis)` | `ScalingStrategy` | same strategy on another axis, id `"<id>@<axis>"`, cached per strategy, not registered |
| `new ScalingStrategy(string id, ScalingConfig config)` | `ScalingStrategy` | wrap a config built by hand (validated) |
| `BuiltInStrategies.Scaled` … `Fixed` · `CustomPresets.TabletFriendly` … | `ScalingStrategy` | ready-made parents |
| `CustomDimens.CustomScaleStrategy(…)` · `CustomDimens.Formula(…)` | `ScalingStrategy` | same factories, on the bridge class |

`register: null` (the default of the one-liners) means "register when an id is given".

## Registry: `ScalingStrategies`

| Member | Notes |
|---|---|
| `Default` | strategy used by `16.Cdp()` / `14.Csp()` without an argument and outside `CustomDimens.Use`; initially `BuiltInStrategies.Scaled` |
| `Register(strategy)` · `Unregister(id)` | add or replace · remove |
| `Get(id)` → `ScalingStrategy?` · `Require(id)` | lookup, including built-ins and presets; `Require` throws `KeyNotFoundException` |
| `Ids` | ids registered by the app |
| `Formula("text")` | shared text-formula strategy |
| `LoadJson(json)` · `CustomExtensions.LoadFile(path)` | register a [JSON document](json.md) and apply its `default`; atomic |
| `ExportJson()` | every registered strategy as a document |
| `MaxCacheId` · `ResetForTesting()` | 65 535 cache ids · clears registrations, shared formulas and the default (tests only) |

## Extensions: `CustomExtensions`

All return `float`. Without a strategy argument they use `CustomDimens.Current` (the `CustomDimens.Use` scope, else `ScalingStrategies.Default`). Without a context they use the ambient window (`AppDimensAmbient.Require()`).

| Call | Returns |
|---|---|
| `Cdp` | **dp** (logical units of MAUI, WPF, WinUI) |
| `CdpPx` | **px** (`dp × density`; canvas, WinForms) |
| `Csp` | **sp units**, like `Ssp`: the toolkit applies the system font scale, the strategy policy is pre-compensated |
| `CspDp` | final text size in dp (font-scale policy applied) |
| `CspPx` | final text size in px (font-scale policy applied) |

Overloads that exist (`v` is the receiver; `s` a `ScalingStrategy`; `"f"` a text formula; `ctx` an `IAppDimensContext`):

| Method | with `s` | with `s, ctx` | no strategy | with `ctx` | with `"f"` | with `"f", ctx` |
|---|---|---|---|---|---|---|
| `Cdp` | `int` `float` `double` | `int` `float` | `int` `float` `double` | `int` `float` | `int` `float` `double` | `int` `float` |
| `CdpPx` | `int` `float` | `int` `float` | `int` `float` | `int` `float` | `int` `float` | `int` `float` |
| `Csp` | `int` `float` | `int` `float` | `int` `float` | `int` `float` | `int` `float` | `int` `float` |
| `CspDp` | `int` `float` | `int` `float` | `int` `float` | — | `int` `float` | `int` `float` |
| `CspPx` | `int` `float` | `int` `float` | `int` `float` | — | `int` `float` | `int` `float` |

So `16.Cdp(s)`, `16f.Cdp(s)`, `16f.CdpPx(s)`, `14f.Csp(s)`, `14f.CspDp(s)`, `14f.CspPx(s)` and every text-formula form (`16f.Cdp("…")`, `14f.CspPx("…")`…) compile, with or without a context.

Strategy-first forms and debug helpers:

| Call | Returns |
|---|---|
| `s.Dp(16f)` · `s.Sp(14f)` · `s.Px(16f)` | same as `Cdp` · `Csp` · `CdpPx`, ambient window |
| `s.CurrentFactor(ctx = null)` | the factor for the window |
| `s.ExplainCurrent(ctx = null)` | one-line breakdown ([troubleshooting](troubleshooting.md)) |

## Bridge: `CustomDimens`

| Member | Notes |
|---|---|
| `Current` | the strategy parameterless calls use |
| `Use(strategy)` → `IDisposable` | makes `strategy` the default for this async flow (`AsyncLocal`) until disposed |
| `DeviceClassResolver` | `Func<IAppDimensContext, DeviceClass?>?`; `null` result falls back to `DefaultDeviceClass` |
| `DefaultDeviceClass(ctx)` | UI mode → desktop OS → smallest width ([device class](configuration.md#device-class)) |
| `ctx.ScreenSnapshot()` | the `ScreenSnapshot` strategies see for a context |
| `s.Resolve(ctx)` → `ResolvedScaling` | memoized per window snapshot |
| `metrics.ToScreenSnapshot(device)` · `s.CacheKey(base, valueType)` | low-level bridge to `DimenMetrics` / `DimenCache` |

## `IAppDimensContext`

Everything reads the window through `AppDimens.Net.Core.IAppDimensContext`: the ambient one set by `AppDimensMaui.AttachWindow`, `AppDimensWpf.Initialize`, `AppDimensWinForms.Initialize`, `AppDimensWinUI.Initialize` or `AppDimensHost.Activate(scope)`, or one you pass explicitly (a `WindowDimensScope` for a secondary MAUI window, a `HostDimensScope`, a `FakeAppDimensContext` in tests). The snapshot uses its `Configuration` (width, height, smallest width, density, font scale, orientation), `IsInMultiWindowMode` and `UiModeType`.

```csharp
using AppDimens.Net.Testing;

var ctx = new FakeAppDimensContext();
ctx.SetConfig(411, 891, densityDpi: 420);
float pad = 16.Cdp(AppStrategies.Product, ctx);
Console.WriteLine(AppStrategies.Product.ExplainCurrent(ctx));
```

## Strategy members

| Member | Notes |
|---|---|
| `Id` · `Config` · `CacheId` · `IsValueDependent` | identity · full config · shared-cache id (−1 past 65 535) · whether results depend on the value |
| `Resolve(screen)` → `ResolvedScaling` | memoized per window: `Factor`, `AppliedRules`, `Config`, `Screen`, `Scale(base)`, `ScaleSpUnits(base)`, `ScaleSpDp(base)`, `Explain(id)` |
| `Factor(screen)` · `Scale(base, screen)` · `Explain(screen)` | platform-free helpers (tests, previews) |
| `ToJson()` | export (throws `InvalidOperationException` for code lambdas) |

`new ScreenSnapshot(widthDp, heightDp, smallestWidthDp: null, density: 1f, fontScale: 1f, isLandscape: null, isMultiWindow: false, device: null)` builds a window by hand, for tests and previews; without `device`, `sw ≥ 600` is a tablet.

## Lower-level types

`ScalingConfig` (immutable; `With…`, `Apply(patch)`, `Validate()`), `ScalingCurve`, `ScalingExpression` (`Parse`, `Evaluate`), `Formula` (`IsProportional`), `FontScalePolicy` (`Follow`, `Ignore`, `Clamp(max)`), `ScreenCondition`, `ScalingPatch`, `ScalingRule`, `ReferenceScreen`, `Breakpoint`, `ScalingAxis`, `DeviceClass`, `CustomNames`, `ScalingStrategyJson` (`Decode`, `DecodeStrategy`, `Encode`, `EncodeDocument`), `MiniJson`. See [API.md](API.md). In the core, custom results use `DimenCache.CalcType.Custom`.

[Back to the custom docs](README.md)

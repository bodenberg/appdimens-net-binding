# Custom strategies (.NET): agent reference

Use when the user wants their own scaling (a formula, per-device rules, a shared JSON) instead of a built-in, in an app that references `AppDimens.Net.Custom` (or `AppDimens.Net.Sdk`). Docs: `docs/AppDimens.Net.Custom/` (start at `README.md`). Cross-platform skill: `appdimens-custom-strategies` in [appdimens-skills](https://github.com/bodenberg/appdimens-skills).

## Decide the form

1. **A built-in passes visual QA?** Keep it (`Sdp`, `FSdp`, `ASdp`, `PWSdp`…). Custom strategies are for when none fits.
2. **The user describes math** ("scale by width over 360, between 0.85 and 1.8"):
   - multiplies every value → `CustomScaleStrategy.Create(s => s.Clamp(s.Width / 360f, 0.85f, 1.8f))`
   - computes the final value from `Base` → `ScalingStrategy.Formula(s => s.Base * s.Pow(s.Sw / 360f, 0.8f))`
3. **The formula must live in JSON / remote config / design tokens** → text: `ScalingStrategy.Formula("base * pow(sw / 360, 0.8)")`, `16.Cdp("…")`, JSON `"formula"`.
4. **Different behavior per device / screen range / orientation, bounds, font-scale cap** → `ScalingStrategy.Define("id", b => { … b.On(DeviceClass.Tablet, r => …); })` or the JSON document.
5. **Several apps or platforms share it** → the JSON document, loaded once at startup (`ScalingStrategies.LoadJson(json)` / `CustomExtensions.LoadFile(path)`).

## Cheat sheet

```csharp
using AppDimens.Net.Custom;   // strategies + Cdp (dp) / CdpPx (px) / Csp (sp units) / CspDp / CspPx

public static class AppStrategies
{
    public static readonly ScalingStrategy Product = CustomScaleStrategy.Create(s => s.Clamp(s.Width / 360f, 0.85f, 1.8f));
    public static readonly ScalingStrategy Hud = ScalingStrategy.Formula(s => s.Base * s.Pow(s.Sw / 360f, 0.8f));
    public static readonly ScalingStrategy Brand = ScalingStrategy.Define("brand", b =>
    {
        b.SetReference(360, 640); b.Curve = ScalingCurve.Power(0.8f); b.Clamp(0.85f, 1.6f); b.ClampFontScale(1.3f);
        b.On(new[] { DeviceClass.Tablet, DeviceClass.Desktop }, r => r.Curve = ScalingCurve.Logarithmic(0.4f), "large");
    });
}
float pad = 16.Cdp(AppStrategies.Product);           // ambient window (initialize MAUI/WPF/WinForms/WinUI first)
float px  = 16.CdpPx(AppStrategies.Brand, ctx);      // explicit IAppDimensContext
ScalingStrategies.Default = AppStrategies.Brand;      // 16.Cdp() / 14.Csp() without argument
using (CustomDimens.Use(AppStrategies.Hud)) { … }     // async-flow scope
```

Scope names on `s` (PascalCase of the text names): `Base` (formula only) `Sw Width Height ShortSide LongSide Diagonal Perimeter AspectRatio Ratio Density FontScale Device Screen`, bools `Phone Tablet Desktop Tv Watch Car Vr Landscape Portrait MultiWindow`, math `Pow Sqrt Ln Log10 Exp Abs Round Min Max Clamp Lerp Smoothstep`. `Ratio = Sw / 300`, so `s.Base * s.Ratio` = `Sdp`. Text uses camelCase (`base`, `sw`, `tablet` as 1/0).

## Rules an agent must follow

- **Declare strategies once** (`static readonly` fields or JSON). Never call `CustomScaleStrategy.Create` / `ScalingStrategy.Formula(s => …)` in a layout, render or resize handler: each call is a new strategy and a new cache id. Inline text (`16.Cdp("…")`) is shared per text and is fine.
- **Lambdas are pure**: no state, settings or time inside. Different state → different strategy or a rule.
- **Prefer a factor** when the math is `Base × screen-term`: `CustomScaleStrategy`, a curve, or text shaped `base * (…)` all cost one multiply. `base * sw / 360` misses the text fast path; write `base * (sw / 360)`.
- **Value bounds** (`b.ClampValue`) only for real absolute limits (tap targets, max text); they must be ≥ 0.
- **Text vs code**: code lambdas cannot be exported to JSON (`ToJson()` throws `InvalidOperationException`); use text when the user mentions JSON, remote config, A/B tests or other platforms.
- **Units**: `Cdp` dp, `CdpPx` px (WinForms, canvas), `Csp` sp units (toolkit applies font scale), `CspDp` / `CspPx` final text size. `int` and `float` receivers have every overload, with or without a context.
- **Desktop OSes are `desktop`**: a tablet-only rule never fires on Windows/macOS/Linux. Add `DeviceClass.Desktop`, use `OnScreen(minSmallestWidth: 600)`, or set `CustomDimens.DeviceClassResolver`.
- **Errors**: parse/JSON errors throw `ArgumentException` (with position / JSON path), unknown ids `KeyNotFoundException`, a non-finite factor `InvalidOperationException`. `LoadJson` is atomic.

## Verify

- `AppStrategies.Brand.ExplainCurrent()` (or `ExplainCurrent(ctx)`) prints the axis, curve, factor, applied rules and formula; `strategy.Explain(new ScreenSnapshot(800, 1280))` without a window.
- Tests: `FakeAppDimensContext` + `16.Cdp(strategy, ctx)`; `dotnet test tests/AppDimens.Net.Tests --filter "FullyQualifiedName~CustomStrategyTests"` in `appdimens-net`.
- JSON files: `npx appdimens-skills kit custom check <file>` and `kit custom explain --formula "<text>" --devices medium-phone,medium-tablet`.
- Error messages and fixes: `docs/AppDimens.Net.Custom/troubleshooting.md`.

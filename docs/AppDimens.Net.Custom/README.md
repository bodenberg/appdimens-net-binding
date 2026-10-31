# AppDimens.Net.Custom — custom strategies

**Your own scaling strategy for .NET: write the math in one line of typed C#, or configure a full strategy (reference frame, axis, curve, bounds, font-scale policy, rules per device and screen) that loads from the same JSON on every AppDimens platform (Android, KMP, iOS, Flutter, React Native, Web, Games, .NET) with the same numbers.**

| | |
|---|---|
| **NuGet** | `AppDimens.Net.Custom` *(unreleased: on `main`, ships with the next AppDimens.Net release)* · included in `AppDimens.Net.Sdk` |
| **Frameworks** | `net8.0`, `net9.0`, `net10.0` (+ `net11.0` with SDK 11); MAUI, WPF, Windows Forms, WinUI 3, any `HostDimensScope` host |
| **Namespace** | `AppDimens.Net.Custom` |
| **Type list** | [API.md](API.md) (every public type) · usage reference: [api-reference.md](api-reference.md) |

```csharp
using AppDimens.Net.Custom;

public static class AppStrategies
{
    // Factor: the number that multiplies every value (runs once per window)
    public static readonly ScalingStrategy Product = CustomScaleStrategy.Create(s => s.Clamp(s.Width / 360f, 0.85f, 1.80f));
    // Formula: the final value (Base = the 16 in 16.Cdp(Hud))
    public static readonly ScalingStrategy Hud = ScalingStrategy.Formula(s => s.Base * s.Pow(s.Sw / 360f, 0.8f));
    // Text, for JSON and remote config
    public static readonly ScalingStrategy Remote = ScalingStrategy.Formula("base * pow(sw / 360, 0.8)");
    // Full configuration with rules
    public static readonly ScalingStrategy Brand = ScalingStrategy.Define("brand", b =>
    {
        b.SetReference(360, 640);
        b.Curve = ScalingCurve.Power(0.8f);
        b.Clamp(0.85f, 1.6f);
        b.On(new[] { DeviceClass.Tablet, DeviceClass.Desktop }, r => r.Curve = ScalingCurve.Logarithmic(0.4f), "large");
    });
}

float padding = 16.Cdp(AppStrategies.Product);   // dp
float score   = 18.Csp(AppStrategies.Hud);       // sp units (the toolkit applies the system font scale)
float border  = 2.CdpPx(AppStrategies.Brand);    // px
```

## Pick a form

| You want to… | Use | Read |
|---|---|---|
| Write the math yourself, in C#, with IntelliSense | `CustomScaleStrategy.Create(s => s.Clamp(s.Width / 360f, 0.85f, 1.8f))` (factor) · `ScalingStrategy.Formula(s => s.Base * s.Pow(s.Sw / 360f, 0.8f))` (value) | [Quick start](quick-start.md) · [Code formulas](code-formulas.md) |
| Keep the formula in JSON or remote config | `ScalingStrategy.Formula("base * pow(sw / 360, 0.8)")` · `16.Cdp("…")` | [Text formulas](text-formulas.md) |
| Different behavior per device, screen range or orientation, bounds, font-scale policy | `ScalingStrategy.Define("brand", b => { … })` | [Configuration](configuration.md) |
| Share one definition across apps and platforms | a strategies JSON file, `ScalingStrategies.LoadJson(json)` | [JSON format](json.md) |

## Documentation

| Guide | What it covers |
|---|---|
| [Quick start](quick-start.md) | install, window context and first strategy in five minutes |
| [Code formulas](code-formulas.md) | `CustomScaleStrategy.Create(s => …)`, `ScalingStrategy.Formula(s => …)` and every name on `s` |
| [Text formulas](text-formulas.md) | the formula language: operators, functions, variables, errors, fast path |
| [Configuration](configuration.md) | reference, axes, curves, bounds, aspect ratio, font scale, multi-window, rules, `Extend`, built-ins, presets, device class |
| [JSON format](json.md) | the cross-platform document, loading, exporting, validation |
| [API reference](api-reference.md) | `Cdp` / `CdpPx` / `Csp` / `CspDp` / `CspPx`, `IAppDimensContext`, `CustomDimens`, registry, debug helpers |
| [Performance](performance.md) | what each form costs, caching |
| [Cookbook](cookbook.md) | recipes: tablets, TV, watch, game HUD, readable text, desktop windows, A/B tests |
| [Troubleshooting](troubleshooting.md) | exception types and messages, `ExplainCurrent()`, FAQ |
| [API.md](API.md) | the list of public types and members |

## How it works, in one picture

```
ratio  = axis(window) / axis(reference)                       // e.g. sw / 300
factor = clamp( curve(ratio) × multiplier × AR , min, max )   // once per window
scaled = clampValue( base × factor )   // the configured result
value  = formula ? formula(base, scaled, factor) : scaled   // a formula replaces the result (it gets the original base)
```

`CustomScaleStrategy.Create(s => …)` replaces the whole `factor` line. `ScalingStrategy.Formula(…)` replaces the whole `value` line. The full configuration lets you set each piece and change it per device with rules. The numbers are the same on every AppDimens platform: a shared 540-case fixture checks it (`CustomStrategyTests.CrossPlatformFixture`).

## When to use it

- A design system shared by several apps that needs one growth law everywhere.
- A product whose UI must behave differently per device class: desktop windows, TV, tablet, kiosk.
- Remote-config or A/B experiments on scaling, by shipping a new JSON.
- Built-ins (`Sdp`, `FSdp`, `PWSdp`…) that are close but need a different reference frame, exponent or bounds.

## Trade-offs

- A strategy defined at runtime is only as safe as its definition. Keep your strategies in one place (a `static class AppStrategies` or the JSON document).
- Code lambdas cannot be exported to JSON; their text equivalents can.
- The aspect-ratio correction always uses the satellite law (`× (1 + k·ln(AR/1.78))`).
- On Windows, macOS and Linux the device class is `desktop` unless `CustomDimens.DeviceClassResolver` says otherwise ([device class](configuration.md#device-class)).

## Português (resumo)

Forma mais simples: uma linha de C# tipado, com autocomplete e os mesmos nomes da linguagem em texto: `CustomScaleStrategy.Create(s => s.Clamp(s.Width / 360f, 0.85f, 1.8f))` (fator, roda uma vez por janela) ou `ScalingStrategy.Formula(s => s.Base * s.Pow(s.Sw / 360f, 0.8f))` (valor final; `Base` = valor passado). Em texto, para JSON/config remota: `ScalingStrategy.Formula("base * pow(sw / 360, 0.8)")` ou `16.Cdp("…")`; fórmulas no formato `base * (…)` ficam no caminho rápido. Configuração completa com `ScalingStrategy.Define("brand", b => { … })` (referência, eixo, curva, limites, política de fonte, regras por dispositivo/tela) ou JSON (`ScalingStrategies.LoadJson`, mesmo formato em todas as plataformas AppDimens). Use `16.Cdp(brand)` (dp), `14.Csp(brand)` (unidades sp), `ScalingStrategies.Default` ou `CustomDimens.Use(brand)`. Guias: [início rápido](quick-start.md), [fórmulas em código](code-formulas.md), [fórmulas em texto](text-formulas.md), [configuração](configuration.md), [JSON](json.md), [API](api-reference.md), [desempenho](performance.md), [receitas](cookbook.md), [problemas](troubleshooting.md).

[Back to the documentation index](../README.md)

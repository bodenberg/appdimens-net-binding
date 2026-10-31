# AppDimens.Net.Custom

**Your own scaling strategy: write the math in one line of typed C#, or configure a full strategy (reference frame, axis, curve or free-form formula, limits, font-scale policy, rules per device and screen) that loads from the same JSON on every AppDimens platform with the same numbers.**

Part of **AppDimens for .NET** · requires `AppDimens.Net` (installed automatically) · included in `AppDimens.Net.Sdk`.
Works with .NET MAUI, WPF, Windows Forms, WinUI 3 and plain .NET (`net8.0` / `net9.0` / `net10.0`).

📖 **Documentation:** [custom-strategy guides](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.Custom/README.md) (quick start, code formulas, text formulas, configuration, JSON, API reference, performance, cookbook, troubleshooting) · **Types:** [API.md](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.Custom/API.md)

## Install

```bash
dotnet add package AppDimens.Net.Custom
```

## Use

Initialize your UI framework as usual (`AppDimensMaui.AttachWindow(window)`, `AppDimensWpf.Initialize(this)`, `AppDimensWinForms.Initialize(this)`, `AppDimensWinUI.Initialize(window)` or `AppDimensHost.Activate(scope)`), then:

```csharp
using AppDimens.Net.Custom;

var product = CustomScaleStrategy.Create(s => s.Clamp(s.Width / 360f, 0.85f, 1.80f));   // factor × every value
var hud = ScalingStrategy.Formula(s => s.Base * s.Pow(s.Sw / 360f, 0.8f));             // final value (Base = 16 in 16.Cdp)
float padding = 16.Cdp(product);   // dp
float score = 18.Csp(hud);         // sp units
```

The same as text, for JSON and remote config: `ScalingStrategy.Formula("base * pow(sw / 360, 0.8)")` or `16.Cdp("…")`. Rules per device, bounds and a font-scale policy: `ScalingStrategy.Define("brand", b => { … })`. Shared documents: `ScalingStrategies.LoadJson(json)`.

## Files

| File | Contents |
|---|---|
| `CustomEngine.cs` | `ScalingStrategy` (`Define`, `Formula`, `Extend`, `WithAxis`), `CustomScaleStrategy`, `ScreenScope` / `FormulaScope`, `ScalingStrategies` registry, `ResolvedScaling`, `BuiltInStrategies`, `CustomPresets` |
| `CustomModel.cs` | `ScalingConfig`, `ScalingCurve`, `ScreenSnapshot`, `ReferenceScreen`, `ScreenCondition`, `ScalingRule`, `FontScalePolicy`, `ScalingAxis`, `DeviceClass` |
| `CustomBuilder.cs` | `ScalingStrategyBuilder` / `ScalingPatchBuilder` (the `b` and `r` of `Define`) |
| `CustomExpression.cs` | the text formula language (`ScalingExpression`, `Formula`, `ExpressionCurve`) |
| `CustomJson.cs` | the cross-platform JSON codec (`ScalingStrategyJson`, `MiniJson`) |
| `CustomDimens.cs` | .NET bridge: `CustomDimens` (`Use`, `DeviceClassResolver`, `IAppDimensContext` snapshots) and the `Cdp` / `CdpPx` / `Csp` / `CspDp` / `CspPx` extensions |

## Tests

```bash
cd appdimens-net
dotnet test tests/AppDimens.Net.Tests --filter "FullyQualifiedName~CustomStrategyTests"
```

They include the cross-platform fixture (`tests/AppDimens.Net.Tests/Fixtures/custom-strategy-fixture.json`, 540 cases) that every AppDimens platform must reproduce.

## Português

Estratégias de escala próprias: fator em uma linha (`CustomScaleStrategy.Create(s => …)`), valor final (`ScalingStrategy.Formula(s => …)` ou em texto `ScalingStrategy.Formula("…")` / `16.Cdp("…")`), configuração completa (`ScalingStrategy.Define`) e JSON multiplataforma (`ScalingStrategies.LoadJson`). Guias completos no link acima.

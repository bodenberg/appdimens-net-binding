# AppDimens.Net.Custom

**Your own scaling strategy — reference frame, axis, curve or free-form formula, limits, font-scale policy and rules per device/screen — in C# or JSON (same format on every AppDimens platform).**

Part of **AppDimens for .NET** · requires `AppDimens.Net` (installed automatically).
Works with .NET MAUI, WPF, Windows Forms, WinUI 3 and plain .NET (`net8.0` / `net9.0` / `net10.0`).

📖 **Complete guide:** [AppDimens.Net.Custom documentation](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.Custom/README.md) · **API reference:** [API.md](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.Custom/API.md)

## Install

```bash
dotnet add package AppDimens.Net.Custom
```

## Use

The simplest way is to write the math as ordinary C# on one line. IntelliSense completes `s.Base`, `s.Sw`, `s.Width`, `s.Tablet`, `s.Pow`, `s.Clamp`…, and typos fail at compile time:

```csharp
using AppDimens.Net.Custom;

var product = CustomScaleStrategy.Create(s => s.Clamp(s.Width / 360f, 0.85f, 1.80f));   // factor × every value
var hud = ScalingStrategy.Formula(s => s.Base * s.Pow(s.Sw / 360f, 0.8f));             // final value (Base = 16 in 16.Cdp)
float padding = 16.Cdp(product);
float score = 18.Csp(hud);
var remote = ScalingStrategy.Formula("base * pow(sw / 360, 0.8)");                      // same thing as text, for JSON / remote config
```

For rules per device, factor bounds or a font-scale policy, use the full configuration:

```csharp
var brand = ScalingStrategy.Define("brand", b =>
{
    b.SetReference(360, 640);
    b.Curve = ScalingCurve.Power(0.8f);                       // or ScalingCurve.Expression("pow(ratio, 0.8)")
    b.Clamp(0.85f, 1.6f);
    b.On(DeviceClass.Tv, r => r.Curve = ScalingCurve.Expression("height / 540"));
});

float padding = 16.Cdp(brand);      // dp
float title   = 20.Csp(brand);      // sp units (font-scale policy applied)
ScalingStrategies.LoadJson(File.ReadAllText("appdimens-strategies.json"));
```

# Quick start

## 1. Install

```bash
dotnet add package AppDimens.Net.Custom      # or AppDimens.Net.Sdk (core + every strategy + Custom)
```

```csharp
using AppDimens.Net.Custom;   // strategies, Cdp / Csp extensions, ScalingStrategies registry
```

## 2. Create the window context

Like every AppDimens.Net function, parameterless calls (`16.Cdp(strategy)`) read the **ambient** window context. Initialize your UI framework once, as usual:

| Host | Call |
|---|---|
| .NET MAUI | `AppDimensMaui.AttachWindow(window)` in `App.CreateWindow` |
| WPF | `AppDimensWpf.Initialize(this)` before `InitializeComponent()` |
| Windows Forms | `AppDimensWinForms.Initialize(this)` after `InitializeComponent()` |
| WinUI 3 | `AppDimensWinUI.Initialize(window)` in `OnLaunched` |
| console, Avalonia, Uno, tests | `AppDimensHost.Activate(new HostDimensScope())` or a `FakeAppDimensContext` |

Without it, parameterless calls throw `InvalidOperationException: AppDimens ambient context is not initialized`. Every call also has an overload that takes an explicit `IAppDimensContext` (`16.Cdp(strategy, ctx)`), for secondary windows and tests.

## 3. Write the math (recommended)

Declare the strategy **once** (for example as a `static readonly` field), then use it anywhere.

```csharp
public static class AppStrategies
{
    // Factor: the number that multiplies every value
    public static readonly ScalingStrategy Product = CustomScaleStrategy.Create(s => s.Clamp(s.Width / 360f, 0.85f, 1.80f));

    // Formula: the final value (Base = the value you pass, the 16 in 16.Cdp)
    public static readonly ScalingStrategy Hud = ScalingStrategy.Formula(s => s.Base * s.Pow(s.Sw / 360f, 0.8f));
}
```

On `s`, IntelliSense completes `Width`, `Sw`, `Height`, `Ratio`, `Tablet`, `Tv`, `Landscape`, `Pow`, `Clamp`, `Min`, `Max`, `Lerp`… and a typo is a compile error. Any other C# works too (`switch` expressions, `Math.Clamp`, your own helpers). The full list is in [Code formulas](code-formulas.md).

## 4. Use it

```csharp
float padding = 16.Cdp(AppStrategies.Product);    // dp (MAUI, WPF, WinUI units)
float title   = 18.Csp(AppStrategies.Hud);        // sp units: the toolkit applies the system font scale
float border  = 2.CdpPx(AppStrategies.Product);   // px (canvas, WinForms)
float label   = 14.CspDp(AppStrategies.Hud);      // final text size in dp, font scale already applied
```

```csharp
// explicit context (secondary window, tests)
float pad2 = 16.Cdp(AppStrategies.Product, ctx);
```

Windows Forms works in pixels: use `CdpPx` / `CspPx`, inside `AppDimensWinForms.Apply(this, () => { … })` so values are re-applied on resize and DPI changes.

## 5. Make it the default (optional)

```csharp
ScalingStrategies.Default = AppStrategies.Product;    // app-wide: 16.Cdp() now uses Product
using (CustomDimens.Use(AppStrategies.Hud))           // only inside this async flow
{
    float lives = 14.Csp();
}
```

## 6. The same thing as text (for JSON and remote config)

```csharp
var remote = ScalingStrategy.Formula("base * pow(sw / 360, 0.8)");
float pad = 16.Cdp("base * (sw / 360)");             // inline, shared per text
```

The text form uses the same names in camelCase (flags are 1 or 0) and works in the strategies JSON (`{"id": "hud", "formula": "base * pow(sw / 360, 0.8)"}`). See [Text formulas](text-formulas.md).

## 7. Need more structure?

Rules per device class, factor bounds, a font-scale cap or a different reference frame go in the full configuration:

```csharp
public static readonly ScalingStrategy Brand = ScalingStrategy.Define("brand", b =>
{
    b.SetReference(360, 640);
    b.Curve = ScalingCurve.Power(0.8f);
    b.Clamp(0.85f, 1.6f);
    b.ClampFontScale(1.3f);
    b.On(new[] { DeviceClass.Tablet, DeviceClass.Desktop }, r => r.Curve = ScalingCurve.Logarithmic(0.4f));
});
```

See [Configuration](configuration.md) and [JSON format](json.md).

## 8. Check the numbers

```csharp
Console.WriteLine(AppStrategies.Product.ExplainCurrent());
// [scale#1] phone smallestWidth=411dp / ref 300 = 1.37 -> Custom = 1.1417 => factor 1.1417
```

[Back to the custom docs](README.md)

# Cookbook

Ready-to-use strategies. Each one shows the code form and, where it matters, the JSON form. Declare them as `static readonly` fields (here shortened to `var`).

## Grow on phones, calm down on tablets

```csharp
var calm = CustomScaleStrategy.Create(s => s.Sw < 600f ? s.Ratio : 2f + (s.Sw - 600f) / 1000f);
```

```csharp
var calm2 = CustomPresets.TabletFriendly;             // linear on phones, Power(0.5) on tablets/desktop, ≤ 1.8
```

## Design frame of 360 dp, never below 85% or above 160%

```csharp
var brand = CustomScaleStrategy.Create(s => s.Clamp(s.Sw / 360f, 0.85f, 1.6f));
```

```json
{ "id": "brand", "reference": { "width": 360, "height": 640 }, "clamp": { "min": 0.85, "max": 1.6 } }
```

## Sublinear growth (big screens grow slower)

```csharp
var soft = CustomScaleStrategy.Create(s => s.Pow(s.Sw / 360f, 0.75f));
```

```json
{ "id": "soft", "reference": { "width": 360, "height": 640 }, "curve": { "type": "power", "exponent": 0.75 } }
```

## Desktop windows: follow the window width, gently

On Windows, macOS and Linux every window is `desktop`, and resizing changes `Width` live.

```csharp
var window = ScalingStrategy.Define("window", b =>
{
    b.Axis = ScalingAxis.Width;
    b.SetReference(1280, 800);
    b.Curve = ScalingCurve.Power(0.5f);
    b.Clamp(0.8f, 1.25f);
});
```

## TV: scale by height, 10-foot UI

```csharp
var living = CustomScaleStrategy.Create(s => s.Tv ? s.Clamp(s.Height / 540f, 1f, 2f) : s.Ratio);
```

```json
{ "id": "living", "rules": [ { "when": { "device": "tv" },
  "set": { "axis": "height", "reference": { "width": 960, "height": 540 }, "clamp": { "min": 1, "max": 2 } } } ] }
```

The TV class comes from `UiModeType.Television`; on a desktop OS set `CustomDimens.DeviceClassResolver` for a TV or kiosk build.

## Watch: almost fixed sizes

```csharp
var wrist = CustomScaleStrategy.Create(s => s.Watch ? s.Clamp(s.ShortSide / 192f, 0.85f, 1.15f) : s.Ratio);
```

## Game HUD: shorter side, aspect-ratio aware

```csharp
var hud = ScalingStrategy.Define("hud", b =>
{
    b.Axis = ScalingAxis.Min;
    b.Curve = ScalingCurve.Power(0.85f);
    b.EnableAspectRatio();
    b.Clamp(0.75f, 2.5f);
    b.IgnoreFontScale();                    // HUD text never follows the system font size
});
```

## Readable text: grow slowly, cap the font scale

```csharp
var body = ScalingStrategy.Define("body", b => { b.Curve = ScalingCurve.Logarithmic(0.4f); b.ClampFontScale(1.3f); });
float size = 16.Csp(body);
```

## Minimum tap target

```csharp
var touch = ScalingStrategy.Define("touch", b => b.ClampValue(min: 48));
float side = 40.Cdp(touch);                 // never below 48 dp
```

## Whole dp only

```csharp
var snap = ScalingStrategy.Formula(s => s.Round(s.Base * s.Ratio));
```

```json
{ "id": "snap", "formula": "round(base * ratio)" }
```

## Never smaller than the design value

```csharp
var floor = ScalingStrategy.Formula(s => s.Max(s.Base * s.Ratio, s.Base));
```

## Split screen: keep the design values

```csharp
var steady = ScalingStrategy.Define("steady", b => b.IgnoreMultiWindow = true);
```

## Landscape phones: a bit smaller

```csharp
var brand2 = ScalingStrategy.Define("brand2", b =>
{
    b.Curve = ScalingCurve.Power(0.8f);
    b.OnScreen(r => r.Multiply(0.9f), maxSmallestWidth: 600, landscape: true);
});
```

## A/B test or remote config

```csharp
string json = await remoteConfig.GetStringAsync("appdimens_strategies");   // a strategies document
try { ScalingStrategies.LoadJson(json); }                                  // atomic: a bad document changes nothing
catch (Exception e) when (e is ArgumentException or KeyNotFoundException) { logger.LogWarning(e, "bad strategies JSON"); }
float pad = 16.Cdp(ScalingStrategies.Require("brand"));
```

## One growth law for a design system

Ship a single `appdimens-strategies.json` with the design tokens, load it in every app (`CustomExtensions.LoadFile(path)` or `ScalingStrategies.LoadJson(text)` here, `loadAsset` on Android, `loadJson` elsewhere), and use the ids only. Android, KMP, iOS, Flutter, React Native, Web and Games read the same file and get the same numbers.

[Back to the custom docs](README.md)

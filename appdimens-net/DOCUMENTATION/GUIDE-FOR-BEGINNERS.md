# AppDimens .NET — Beginner's Guide

## 1. The problem

`dp` (density-independent pixels) only compensate **screen density**. On a phone
and on a tablet the same `200dp` box looks proportionally different. AppDimens
scales dimensions with the **smallest window width** (`sw`) so proportions stay
consistent across phones, tablets, foldables, TVs and split-screen.

## 2. Install

```xml
<PackageReference Include="AppDimens.Net.Sdk" Version="2.0.0" />
```

(or reference the projects in this repo).

## 3. Bootstrap once per window

```csharp
using AppDimens.Net.Maui.Platform;

protected override Window CreateWindow(IActivationState? state)
{
    var window = new Window();
    AppDimensMaui.AttachWindow(window);   // MUST run before any page is created
    window.Page = new NavigationPage(new MainPage());
    return window;
}
```

The attach step:

1. builds a `WindowDimensScope` (reads window size, density, font scale);
2. initializes `DimenCache` and registers an event-driven watcher — every
   resize/rotation/split-screen change updates the ambient metrics automatically,
   no manual invalidation;
3. publishes the scope to `AppDimensAmbient`, which powers all parameterless calls.

## 4. Use it anywhere

| Call | Meaning |
|---|---|
| `16.Sdp()` | scaled by smallest width (default choice) |
| `16.Sdpa()` | same + aspect-ratio adjustment (very tall/short screens) |
| `48.Hdp()` / `100.Wdp()` | scale by height / width axis |
| `24.Sdpi()` | ignore multi-window constraints |
| `16.Ssp()` | scaled text size — respects system font scale |
| `60.SdpPh(ctx)` | smallest-width → portrait-height inverter |

Every call returns plain `float` dp/sp — use it for `Padding`, `WidthRequest`,
`FontSize`, etc.

## 5. Other strategies

Each satellite package exposes the same shape: `{P}Sdp`, `{P}Sdpa`, `{P}Hdp`,
`{P}Wdp`, `{P}Sdpi(a)` plus a generic `To{X}Dp(value, ctx, qualifier, inverter…)`:

```csharp
using AppDimens.Net.Code.Percent;   // 24.PSdp(ctx)
using AppDimens.Net.Code.Fluid;     // 28.ToFluidDp(ctx)
using AppDimens.Net.Code.Auto;      // 28.ASdp(ctx)
```

## 6. Builders (conditional overrides)

```csharp
var size = 16.ScaledDp()
    .Screen(DpQualifier.SmallWidth, 600, 24)  // sw≥600 → 24
    .Screen(UiModeType.Television, 40)        // TV → 40
    .Sdp();
```

## 7. Auto-resize (fit content to a box)

```csharp
float fontPx  = DimenResize.AutoResizeTextPx(text, innerBoxPx, minSp:10, maxSp:22, stepSp:1, maxLines:2, ctx);
float sideDp  = DimenResize.AutoResizeSquarePx(boxWpx, boxHpx, minDp:16, maxDp:100, stepDp:4, ctx);
```

## 8. Testing your own UI logic

```csharp
var ctx = new FakeAppDimensContext();
ctx.SetConfig(360, 740, densityDpi: 420);       // simulate a phone
float v = 16f.ToDynamicScaledDp(ctx);
ctx.SetConfig(600, 1000); ctx.NotifyChange();    // simulate resize; watchers fire
```

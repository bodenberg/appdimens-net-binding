---
name: appdimens-net
description: Use the AppDimens.Net family — the pure C# port of appdimens-kmp (AppDimens.Net core, AppDimens.Net.Sdk and the Auto/Density/Diagonal/Fill/Fit/Fluid/Interpolated/Logarithmic/Percent/Perimeter/Power/Resize/Units strategies, AppDimens.Net.Custom (own strategies: Cdp/Csp, JSON, free-form formulas), plus the AppDimens.Net.Maui/.Wpf/.WinForms/.WinUI integrations) — for responsive sizes in .NET apps (16.Sdp(), 24.FSdp(), 50.SpaceWDp(ctx), {ad:Sdp 16}, HostDimensScope, FakeAppDimensContext). Use when an app references AppDimens.Net* packages or namespaces, or the user wants scaling strategies (fluid, percent, auto, power…) or their own custom scaling strategy/config per device type in MAUI, WPF, WinForms, WinUI, Avalonia, Uno, console or tests.
---

# AppDimens.Net (KMP port) — core, strategies and UI integrations

Bit-for-bit C# port of [appdimens-kmp](https://github.com/bodenberg/appdimens-kmp). Numbers you write are sizes on a
**300 dp-wide reference screen**; results are `float` **dp** unless the name ends in `Px`. All packages **3.6.1**,
`net8.0/9.0/10.0` (+`net11.0`); trimming/NativeAOT safe.

Not this family: Android SDP buckets and `{sdp:16}` short markup → skill `appdimens-sdps-net`;
Android-only Kotlin bindings → skills `appdimens-android-sdps-ssps` / `appdimens-android-dynamic`.

## Install

```bash
dotnet add package AppDimens.Net.Sdk          # core + every strategy (or pick single packages)
dotnet add package AppDimens.Net.Maui         # or .Wpf / .WinForms / .WinUI — each brings core + Fluid + Percent
```

## Step 1 — create the window context (required)

Parameterless calls (`16.Sdp()`) use the **ambient context**; without it they throw
`InvalidOperationException: AppDimens ambient context is not initialized`.

| Host | Package | Call |
|------|---------|------|
| .NET MAUI | `AppDimens.Net.Maui` | `AppDimensMaui.AttachWindow(window)` in `App.CreateWindow`, **before** creating pages (`using AppDimens.Net.Maui.Platform;`) |
| WPF | `AppDimens.Net.Wpf` | `AppDimensWpf.Initialize(this)` **before** `InitializeComponent()` |
| Windows Forms | `AppDimens.Net.WinForms` | `AppDimensWinForms.Initialize(this)` after `InitializeComponent()` |
| WinUI 3 | `AppDimens.Net.WinUI` | `AppDimensWinUI.Initialize(window)` in `OnLaunched`, before `Activate()` |
| anything else | `AppDimens.Net` | `HostDimensScope` (below) |

```csharp
using AppDimens.Net;
using AppDimens.Net.Core;
var scope = AppDimensHost.Activate(new HostDimensScope { FontScale = 1.0f });
scope.Update(widthDp: 1280, heightDp: 720, density: 1.5);   // call from resize / DPI handlers; returns true if changed
using var reg = scope.RegisterConfigurationListener(() => Relayout());
IAppDimensContext ctx = AppDimensAmbient.Require();         // or AppDimensAmbient.Current (nullable)
```

Every function also has an overload taking an explicit `IAppDimensContext` (`16.Sdp(ctx)`), needed for `*Px` forms
and for secondary windows.

## Step 2 — the Scaled strategy (core, `using AppDimens.Net.Code.Scaled;`)

```csharp
float pad   = 16.Sdp();      // smallest width — default for padding, margins, icons
float bar   = 56.Hdp();      // height axis
float side  = 200.Wdp();     // width axis
float body  = 14.Ssp();      // text, follows user font scale (Hsp, Wsp)
float fixd  = 12.Sem();      // text ignoring font scale (Hem, Wem)
float px    = 2.SdpPx();     // physical pixels (canvas, WinForms)
// suffixes: a = aspect-ratio correction, i = ignore multi-window/split-screen shrinking, ia = both
float x = 16.Sdpa(); float y = 16.Sdpi(); float z = 16.Sdpia();
```

Inverters (axis by orientation): `60.SdpPh(ctx)`, `SdpLh`, `SdpPw`, `SdpLw`, `48.HdpLw(ctx)`, `120.WdpLh(ctx)`
(+ `a`/`ia`; optional `ar:`, `imw:`, `k:`). Facilitators: `16.SdpRotate(ctx, 24)`, `20.SdpMode(ctx, 32, UiModeType.Television)`,
`16.SdpQualifier(ctx, DpQualifier.SmallWidth, 600, 24)`, `120.SdpScreen(ctx, UiModeType.Normal, DpQualifier.SmallWidth, 600, Orientation.Landscape, 180)`
and `*Raw` variants (alternate value not scaled). Builder:

```csharp
using AppDimens.Net.Common;
float size = 16.ScaledDp()
    .Screen(DpQualifier.SmallWidth, 600, 24)
    .Screen(UiModeType.Television, 40)
    .Screen(Orientation.Landscape, 20)
    .AspectRatio().IgnoreMultiWindows()
    .Sdp();                                   // or .Hdp() / .Wdp() / .Px(); 14.ScaledSp() for text
```

Full control: `16.SdpCustom(ctx, qualifier, inverter, ignoreMultiWindows, applyAspectRatio)`,
`16.ToDynamicScaledDp(ctx, …)`. Enums (`AppDimens.Net.Common`): `DpQualifier`, `Inverter`, `Orientation`, `UiModeType`, `UnitType`.

## Strategies (one package each, same shape)

Each strategy `X` lives in `AppDimens.Net.Code.X` with `{P}Sdp/{P}Hdp/{P}Wdp`, suffixes `a`/`i`/`ia`, `Px(ctx)` forms,
`{P}SdpCustom(qualifier, inverter, imw, ar, k)`, `{P}SdpRotateRaw(ctx, rawAlternate, orientation)`, a static class
`DimenX` (`DimenX.WarmupCache(ctx)`) and a kernel `ToXDp/ToXPx`.

| Package | Prefix | `16` on phone 360 / tablet 800 / desktop 1280×800 | Use for |
|---------|--------|------|---------|
| core (Scaled) | `Sdp` | 19.2 / 42.7 / 42.7 | default, proportional |
| `.Auto` | `ASdp` | 19.2 / 30.2 / 30.2 | proportional, damped on big screens |
| `.Fluid` | `FSdp` | 13.4 / 19.2 / 19.2 | clamped 0.8×–1.2× — desktop windows, foldables |
| `.Logarithmic` | `LOGSdp` | 17.2 / 22.3 / 22.3 | gentle growth |
| `.Interpolated` | `ISdp` | 17.6 / 29.3 / 29.3 | linear between anchors |
| `.Power` | `PWSdp` | 18.3 / 33.4 / 33.4 | tunable with `k` (`PWSdpCustom(..., k: 1.4f)`) |
| `.Diagonal` | `DGSdp` | 22.9 / 39.5 / 39.5 | identical in portrait/landscape |
| `.Perimeter` | `PRSdp` | 22.3 / 40 / 40 | balances width + height |
| `.Fill` | `FLSdp` | 24 / 42.7 / 42.7 | fills its share of space |
| `.Percent` | `PSdp` | 19.2 / 42.7 / 42.7 | strictly linear; `50.SpaceWDp(ctx)` / `SpaceHDp` / `SpaceSwDp` = literal % of the window |
| `.Density` | `DSdp` | 16 / 16 / 16 | follows DPI, not window size |

Beginner rule: `Sdp` for layout, `Ssp` for text; switch an over-growing value to `FSdp` or `ASdp`.

**Custom strategies** (`AppDimens.Net.Custom`, unreleased — on `main`): when no built-in passes QA, or one growth law must be shared with other platforms (same JSON format), define your own instead of call-site `if`s:

```csharp
using AppDimens.Net.Custom;
var brand = ScalingStrategy.Define("brand", b =>
{
    b.SetReference(360, 640);
    b.Curve = ScalingCurve.Power(0.8f);                 // Linear/Power/Logarithmic/Interpolated/Constant/Breakpoints
    b.Clamp(0.85f, 1.6f);                               // factor bounds; ClampValue(min, max) bounds the dp
    b.On(DeviceClass.Tablet, r => r.Curve = ScalingCurve.Expression("1 + 0.4 * ln(ratio)"));  // free-form formula
    b.SetFormula("max(scaled, base)");                  // optional per-value formula (cached lane)
});
float pad = 16.Cdp(brand); float title = 20.Csp(brand);   // dp / sp units; CdpPx, CspDp, CspPx
ScalingStrategies.LoadJson(json);                         // or ScalingStrategies.Default = brand / CustomDimens.Use(brand)
```

Simplest form, for when the user just wants to write the math: `16.Cdp("base * pow(sw / 360, 0.8)")`, `var hud = ScalingStrategy.Formula("…")` or `CustomDimens.Formula((b, screen) => …)` (code, not JSON). A programmable **factor** in plain C#: `CustomScaleStrategy.Create(screen => Math.Clamp(screen.WidthDp / 360f, 0.85f, 1.8f))`, which runs once per window (one multiply per value). Text formulas shaped `base * (…)` (with no `base` inside the parentheses) stay on the one-multiply fast path; other shapes are cached per value.

Start from a built-in or preset (`BuiltInStrategies.Power.Extend("x", …)`, `CustomPresets.TabletFriendly`/`TvSafe`/`Wearable`/`GameHud`). Formula variables: `sw width height shortSide longSide diagonal perimeter aspectRatio density fontScale`, flags `landscape portrait multiWindow phone tablet desktop tv watch car vr`, `ratio axis ref` (curves) and `base scaled factor` (formulas). Verify with `brand.ExplainCurrent()`. On desktop OSes the device class is `desktop` unless `CustomDimens.DeviceClassResolver` says otherwise. Guide: docs/AppDimens.Net.Custom/README.md.

Special packages:

```csharp
using AppDimens.Net.Code.Fit;      // per-screen overrides; Ftsdp/Fthdp/Ftwdp/ResolveAll return PIXELS
float px = 16.FitScaledDp().Screen(DpQualifier.SmallWidth, 600, 24).Screen(Orientation.Landscape, 20).Ftsdp();
float dp = px / AppDimensAmbient.Require().Density;   // dp for MAUI/WPF/WinUI
// priority: UiMode rules > size-qualifier rules (declare largest threshold first) > orientation > base

using AppDimens.Net.Code.Resize;   // box sizes in px; result in the RANGE unit despite the "Px" name
float sp = DimenResize.AutoResizeTextPx("Black Friday", boxWidthPx: 600f, minSp: 12f, maxSp: 32f, stepSp: 1f, maxLines: 2, ctx: ctx); // sp
float side = DimenResize.AutoResizeSquarePx(300f, 180f, minDp: 24f, maxDp: 120f, stepDp: 4f, ctx: ctx);                             // dp

using AppDimens.Net.Code.Units;    // physical units (accuracy depends on OS-reported DPI)
float card = DimenPhysicalUnits.ToDpFromMm(85.6f);  // also ToPxFromCm, ToSpFromInch, RadiusFromDiameter(24f, UnitType.Mm)
```

## UI integrations

**MAUI** (`AppDimens.Net.Maui`): `AppDimensMaui.AttachWindow(window)` (returns `WindowDimensScope`; `AppDimensMaui.Primary`,
`Detach()`, `Init(app)`). XAML `xmlns:ad="clr-namespace:AppDimens.Net.Maui.Xaml;assembly=AppDimens.Net.Maui"` →
`{ad:Sdp 16}`, `{ad:Sdpa 96}`, `{ad:Hdp 48}`, `{ad:Wdp 100}`, `{ad:Ssp 22}`, `{ad:Fsdp 24}`, `{ad:Psdpx 50}`,
`{ad:Sdp Value=14, AspectRatio=True}`; converters `ad:SdpConverter`, `ad:SspConverter`. MAUI XAML is evaluated at page
build — for live updates set values in C# inside `AppDimensMaui.Primary.RegisterConfigurationListener(...)` (marshal with
`MainThread.BeginInvokeOnMainThread`). Secondary window: `new WindowDimensScope(window)` and pass it as `ctx`.

**WPF** (`AppDimens.Net.Wpf`): `xmlns:ad="clr-namespace:AppDimens.Net.Wpf;assembly=AppDimens.Net.Wpf"` → `{ad:Sdp 16}`,
`{ad:FSdp 80}`, `{ad:SpaceW 50}` — **live** bindings, also in styles/templates; options `Live=False`, `As=Thickness|CornerRadius|GridLength|Double`;
`ad:DimenConverter Function="Sdp"`; code-behind → re-apply in `AppDimensWpf.Changed`; `DimenFunction.FSdp.Evaluate(24)`.

**WinForms** (`AppDimens.Net.WinForms`): put sizing in `AppDimensWinForms.Apply(this, () => { ... })`; convert with
`DimenPx.ToPx(120.Sdp())` (pixels), `DimenPx.ToPt(14.Ssp())` (font points), `DimenPx.Padding(dp)`, `DimenPx.Size(w, h)`,
`DimenPx.Of(DimenFunction.SpaceW, 50)`; set `AutoScaleMode = AutoScaleMode.None`.

**WinUI 3** (`AppDimens.Net.WinUI`): `xmlns:ad="using:AppDimens.Net.WinUI"` → `{ad:Sdp Value=16}` (`Value=` required),
re-applied on resize; `AppDimensWinUI.Changed` for code-behind.

The 41 markup/`DimenFunction` names (WPF, WinForms, WinUI): Scaled `Sdp Sdpa Sdpi Sdpia Hdp Hdpa Hdpi Wdp Wdpa Wdpi Ssp Sspa Sem Hem Wem`,
Fluid `FSdp…FWdpia`, Percent `PSdp…PWdpia`, `SpaceW`, `SpaceH`. Other strategies: call them from C#.

## Testing

```csharp
using AppDimens.Net.Testing;
var ctx = new FakeAppDimensContext();
ctx.SetConfig(360, 800, densityDpi: 420);   // phone → 16.Sdp(ctx) == 19.2
ctx.SetConfig(800, 1280); ctx.NotifyChange();
ctx.IsInMultiWindowMode = true; ctx.UiModeType = UiModeType.Television;
AppDimensHost.Activate(ctx);                // make it ambient
```

Cache (`AppDimens.Net.Core.DimenCache`) is invalidated automatically on resize — never clear it manually after a
resize; `DiagnosticsEnabled`, `HitCount`, `MissCount`, `ClearAll()` exist for diagnostics.

References: [core guide](../../docs/AppDimens.Net/README.md) · [Sdk / strategy table](../../docs/AppDimens.Net.Sdk/README.md) ·
integrations [Maui](../../docs/AppDimens.Net.Maui/README.md), [Wpf](../../docs/AppDimens.Net.Wpf/README.md),
[WinForms](../../docs/AppDimens.Net.WinForms/README.md), [WinUI](../../docs/AppDimens.Net.WinUI/README.md) ·
per-strategy guides and `API.md` under `docs/AppDimens.Net.<Strategy>/`.

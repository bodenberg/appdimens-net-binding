# AppDimens.Net — complete guide

**The core of AppDimens for .NET**: context model, cache, the **Scaled** strategy (`Sdp`, `Hdp`, `Wdp`,
`Ssp`…), orientation inverters, conditional facilitators, the `ScaledDimension` builder, `HostDimensScope`
for any UI host, and `FakeAppDimensContext` for tests. Bit-for-bit port of
[appdimens-kmp](https://github.com/bodenberg/appdimens-kmp).

| | |
|---|---|
| **NuGet** | [`AppDimens.Net`](https://www.nuget.org/packages/AppDimens.Net) |
| **Frameworks** | `net8.0`, `net9.0`, `net10.0` (+ `net11.0` with SDK 11) |
| **Namespaces** | `AppDimens.Net` (ambient), `AppDimens.Net.Core` (context, cache, host scope), `AppDimens.Net.Common` (enums), `AppDimens.Net.Code.Scaled` (Scaled strategy), `AppDimens.Net.Testing` |
| **API reference** | [API.md](API.md) |
| **Related** | UI integrations: [Maui](../AppDimens.Net.Maui/README.md) · [Wpf](../AppDimens.Net.Wpf/README.md) · [WinForms](../AppDimens.Net.WinForms/README.md) · [WinUI](../AppDimens.Net.WinUI/README.md) — strategies: [all packages](../README.md#appdimensnet-family) |

---

## Table of contents

1. [Concepts in 2 minutes](#concepts-in-2-minutes)
2. [Install](#install)
3. [Create the window context](#create-the-window-context)
4. [The Scaled strategy — every function](#the-scaled-strategy--every-function)
5. [Text: `Ssp`, `Sem` and font scale](#text-ssp-sem-and-font-scale)
6. [Pixels (`Px`)](#pixels-px)
7. [Orientation inverters](#orientation-inverters)
8. [Conditional facilitators: Rotate, Mode, Qualifier, Screen](#conditional-facilitators-rotate-mode-qualifier-screen)
9. [The `ScaledDimension` builder](#the-scaleddimension-builder)
10. [Full control: `SdpCustom` and `ToDynamicScaledDp`](#full-control-sdpcustom-and-todynamicscaleddp)
11. [Custom hosts: `HostDimensScope`](#custom-hosts-hostdimensscope)
12. [Cache and diagnostics](#cache-and-diagnostics)
13. [Testing with `FakeAppDimensContext`](#testing-with-fakeappdimenscontext)
14. [Enums reference](#enums-reference)
15. [FAQ](#faq)
16. [Português (resumo)](#português-resumo)

---

## Concepts in 2 minutes

* **dp** — device-independent unit (1/160 inch on Android, 1/96 inch "DIP" on Windows). All results are dp unless the name ends in `Px`.
* **Design base** — numbers you write are sizes on a **300 dp-wide reference screen**. `16.Sdp()` is `16 × axis ÷ 300`:
  19.2 on a 360 dp phone, 32 on 600 dp, 48 on 900 dp.
* **Axis** — `Sdp` follows the **smallest width** (default), `Hdp` the **height**, `Wdp` the **width**.
* **Context** — an `IAppDimensContext` describes one window (size, density, font scale, orientation, UI mode).
  The UI integrations create it for you; the **ambient** context powers parameterless calls like `16.Sdp()`.
* **Suffixes** — `a` = aspect-ratio correction, `i` = ignore multi-window shrinking, `Px` = physical pixels.

## Install

```bash
dotnet add package AppDimens.Net
# or every strategy at once:
dotnet add package AppDimens.Net.Sdk
```

## Create the window context

| Framework | Package | One-time call |
|-----------|---------|---------------|
| .NET MAUI | `AppDimens.Net.Maui` | `AppDimensMaui.AttachWindow(window)` |
| WPF | `AppDimens.Net.Wpf` | `AppDimensWpf.Initialize(this)` |
| Windows Forms | `AppDimens.Net.WinForms` | `AppDimensWinForms.Initialize(this)` |
| WinUI 3 | `AppDimens.Net.WinUI` | `AppDimensWinUI.Initialize(window)` |
| Console, Avalonia, Uno, game loop, server-side rendering | `AppDimens.Net` | `HostDimensScope` (below) |

```csharp
using AppDimens.Net;
using AppDimens.Net.Core;

// Generic host: create, activate, and push the window size whenever it changes.
var scope = AppDimensHost.Activate(new HostDimensScope());
scope.Update(widthDp: 1280, heightDp: 720, density: 1.5);

IAppDimensContext ctx = AppDimensAmbient.Require();   // the ambient context (throws if none)
IAppDimensContext? maybe = AppDimensAmbient.Current;  // or null
```

## The Scaled strategy — every function

```csharp
using AppDimens.Net.Code.Scaled;

// Layout (dp)
float a = 16.Sdp();     // smallest width — default choice for padding, margins, icons
float b = 16.Sdpa();    // + aspect-ratio correction
float c = 16.Sdpi();    // ignores multi-window / split-screen shrinking
float d = 16.Sdpia();   // both
float e = 56.Hdp();     // height axis (headers, toolbars)
float f = 56.Hdpa();
float g = 56.Hdpi();
float h = 200.Wdp();    // width axis (side panels, banners)
float i = 200.Wdpa();
float j = 200.Wdpi();

// Doubles are accepted too
float k = 12.5.Sdp();
float l = 12.5.Sdpa();
```

With an explicit context (no ambient needed) every function has an overload taking `IAppDimensContext`:

```csharp
using AppDimens.Net.Code.Scaled;
using AppDimens.Net.Core;
using AppDimens.Net.Testing;

IAppDimensContext ctx = new FakeAppDimensContext();
float a = 16.Sdp(ctx);
float b = 48.Hdpia(ctx);
float c = 120.Wdpa(ctx);
float d = 12.5.Sdp(ctx);
```

| Phone 360×800 | Tablet 800×1280 | Desktop 1280×800 |
|---|---|---|
| `16.Sdp()` = 19.2 | 42.7 | 42.7 |
| `16.Hdp()` = 42.7 | 68.3 | 42.7 |
| `16.Wdp()` = 19.2 | 42.7 | 68.3 |

## Text: `Ssp`, `Sem` and font scale

```csharp
using AppDimens.Net.Code.Scaled;

float body    = 14.Ssp();    // scales like Sdp AND follows the user's font-scale setting
float bodyAr  = 14.Sspa();   // + aspect-ratio correction
float caption = 12.Sem();    // ignores the font-scale setting (fixed curve)
float hText   = 18.Hem();    // height axis, no font scale
float wText   = 18.Wem();    // width axis, no font scale
```

With a context: `14.Ssp(ctx)`, `14.Sspi(ctx)`, `14.Sspia(ctx)`, `18.Hsp(ctx)`, `18.Hspa(ctx)`, `18.Wsp(ctx)`, `18.Wspa(ctx)`.
On desktop the font scale comes from `HostDimensScope.FontScale` (default 1.0).

## Pixels (`Px`)

Use `Px` when drawing on a canvas (SkiaSharp, Android `Canvas`, WinForms `Graphics`) or when an API wants raw pixels:

```csharp
using AppDimens.Net;
using AppDimens.Net.Code.Scaled;

var ctx = AppDimensAmbient.Require();

float stroke   = 2.SdpPx();          // ambient
float strokeAr = 2.SdpaPx();
float radius   = 8.SdpPx(ctx);
float barH     = 56.HdpPx(ctx);
float colW     = 120.WdpaPx(ctx);
float frozen   = 16.SdpiPx(ctx);
```

## Orientation inverters

Inverters change the axis depending on orientation — e.g. "smallest width in portrait, height in landscape":

```csharp
using AppDimens.Net;
using AppDimens.Net.Code.Scaled;

var ctx = AppDimensAmbient.Require();

float a = 60.SdpPh(ctx);     // portrait: smallest width → landscape: height
float b = 60.SdpLh(ctx);     // landscape: height
float c = 60.SdpPw(ctx);     // portrait: width
float d = 60.SdpLw(ctx);     // landscape: width
float e = 48.HdpLw(ctx);     // height → width in landscape
float f = 48.HdpLwa(ctx);    // + aspect ratio
float g = 120.WdpLh(ctx);    // width → height in landscape
float h = 120.WdpLhia(ctx);  // + ignore multi-window + aspect ratio
```

All inverters accept optional `ar` (aspect ratio), `imw` (ignore multi-window) and `k` (sensitivity):
`60.SdpPh(ctx, ar: true, imw: false, k: 0.9f)`. The full list of rules is the `Inverter` enum.

## Conditional facilitators: Rotate, Mode, Qualifier, Screen

```csharp
using AppDimens.Net;
using AppDimens.Net.Code.Scaled;
using AppDimens.Net.Common;

var ctx = AppDimensAmbient.Require();

// Rotate: base value normally, another (scaled) value in landscape
float pad      = 16.SdpRotate(ctx, rotationValue: 24);
float padRaw   = 16.SdpRotateRaw(ctx, rawRotationValue: 24f);                         // alternate NOT scaled
float headerH  = 56.HdpRotate(ctx, 48, Orientation.Portrait);
float sideW    = 200.WdpRotate(ctx, 280);

// Mode: different value for a UI mode (TV, car, watch, foldables…)
float tvTitle  = 20.SdpMode(ctx, modeValue: 32, UiModeType.Television);
float carBtn   = 48.HdpMode(ctx, 64, UiModeType.Car);
float watchPad = 8.SdpModeRaw(ctx, rawModeValue: 4f, UiModeType.Watch);

// Qualifier: different value when an axis reaches a threshold (like Android values-sw600dp)
float gutter   = 16.SdpQualifier(ctx, DpQualifier.SmallWidth, qualifierValue: 600, alternateValue: 24);
float gutterRaw = 16.SdpQualifierRaw(ctx, DpQualifier.Width, 840, rawAlternateValue: 32f);

// Screen: UI mode + qualifier + orientation in one call
float hero = 120.SdpScreen(ctx, UiModeType.Normal, DpQualifier.SmallWidth, 600,
                           Orientation.Landscape, customValue: 180);
```

## The `ScaledDimension` builder

Declare a base value plus any number of rules; the most specific matching rule wins:

```csharp
using AppDimens.Net;
using AppDimens.Net.Code.Scaled;
using AppDimens.Net.Common;

float size = 16.ScaledDp()
    .Screen(DpQualifier.SmallWidth, 600, 24)        // sw ≥ 600 → 24
    .Screen(DpQualifier.SmallWidth, 840, 32)        // sw ≥ 840 → 32
    .Screen(UiModeType.Television, 40)              // TV → 40
    .Screen(Orientation.Landscape, 20)              // landscape → 20
    .AspectRatio()                                  // apply aspect-ratio correction
    .IgnoreMultiWindows()
    .Sdp();                                         // resolve on the smallest-width axis

float height = 48.ScaledDp().Screen(DpQualifier.Height, 900, 64).Hdp();
float width  = 120.ScaledDp().Screen(DpQualifier.Width, 1200, 240).Wdp();
float px     = 16.ScaledDp().Px();
float text   = 14.ScaledSp().Screen(DpQualifier.SmallWidth, 600, 18).Sdp();

var ctx = AppDimensAmbient.Require();
float explicitCtx = 16.ScaledDp().CustomSensitivity(0.8f).Resolve(ctx);
```

## Full control: `SdpCustom` and `ToDynamicScaledDp`

```csharp
using AppDimens.Net;
using AppDimens.Net.Code.Scaled;
using AppDimens.Net.Common;

var ctx = AppDimensAmbient.Require();

float a = 16.SdpCustom(ctx, qualifier: DpQualifier.Height, inverter: Inverter.SwToLh,
                       ignoreMultiWindows: true, applyAspectRatio: true);
float b = 16.SdpCustomPx(ctx, DpQualifier.Width);

// Generic kernel (also for float/double values)
float c = 16.ToDynamicScaledDp(ctx, DpQualifier.Width, Inverter.Default,
                               ignoreMultiWindows: false, applyAspectRatio: true, customSensitivityK: 0.9f);
float d = 12.5f.ToDynamicScaledPx(ctx);
```

## Custom hosts: `HostDimensScope`

`HostDimensScope` is what the WPF / WinForms / WinUI integrations are built on. Use it directly for any other host:

```csharp
using AppDimens.Net;
using AppDimens.Net.Code.Scaled;
using AppDimens.Net.Core;

var scope = AppDimensHost.Activate(new HostDimensScope { FontScale = 1.1f });

// call from your resize / DPI-change handler:
scope.Update(widthDp: 1024, heightDp: 768, density: 1.25);

// react to changes (listeners fire only when something really changed)
using var registration = scope.RegisterConfigurationListener(() => Console.WriteLine($"now {16.Sdp()}"));

bool changed = scope.Update(1280, 800, 1.25);   // true → listeners notified, cache refreshed
scope.Dispose();                                // stop watching
```

Subclass it to plug a native window: call `Update(...)` from the window's events and override `Dispose(bool)` to unhook.

## Cache and diagnostics

```csharp
using AppDimens.Net.Core;

DimenCache.CacheEnabled = true;          // default
DimenCache.DiagnosticsEnabled = true;    // count hits / misses
Console.WriteLine($"hits={DimenCache.HitCount} misses={DimenCache.MissCount} evictions={DimenCache.EvictionCount}");
DimenCache.ClearAll();                   // drop every cached value (normally automatic on resize)
```

The cache is partitioned by window snapshot and invalidated automatically by the window watcher — you never need
to clear it after a resize.

## Testing with `FakeAppDimensContext`

```csharp
using AppDimens.Net;
using AppDimens.Net.Code.Scaled;
using AppDimens.Net.Common;
using AppDimens.Net.Core;
using AppDimens.Net.Testing;

var ctx = new FakeAppDimensContext();
ctx.SetConfig(360, 800, densityDpi: 420);          // phone
float phone = 16.Sdp(ctx);                          // 19.2

ctx.SetConfig(800, 1280);                           // tablet
ctx.NotifyChange();                                 // fire watchers like a real resize
float tablet = 16.Sdp(ctx);                         // 42.7

ctx.IsInMultiWindowMode = true;                     // simulate split screen
ctx.UiModeType = UiModeType.Television;             // simulate a TV

AppDimensHost.Activate(ctx);                        // make it ambient → 16.Sdp() uses it
```

## Enums reference

| Enum | Values |
|------|--------|
| `DpQualifier` | `SmallWidth`, `Height`, `Width` |
| `Inverter` | `Default`, `PhToLw`, `PwToLh`, `LhToPw`, `LwToPh`, `SwToLh`, `SwToLw`, `SwToPh`, `SwToPw` |
| `Orientation` | `Portrait`, `Landscape`, `Default` |
| `UiModeType` | `Undefined`, `Normal`, `Television`, `Car`, `Watch`, … (+ foldable / flip states) — see [API.md](API.md) |
| `UnitType` | `Mm`, `Cm`, `Inch` |

## FAQ

**`InvalidOperationException: AppDimens ambient context is not initialized`** — create the window context first
(see [Create the window context](#create-the-window-context)) or pass a context explicitly.

**Do I need to clear the cache on resize?** — No. The window watcher invalidates it automatically.

**Is it thread-safe?** — Yes: snapshots are immutable and the cache is lock-free on the hit path.

**Trimming / NativeAOT?** — Supported; see [TRIMMING-AOT.md](https://github.com/bodenberg/appdimens-net-binding/blob/main/appdimens-net/DOCUMENTATION/TRIMMING-AOT.md).

---

## Português (resumo)

* `dotnet add package AppDimens.Net` (ou `AppDimens.Net.Sdk` com todas as estratégias).
* Crie o contexto da janela uma vez (MAUI/WPF/WinForms/WinUI têm pacotes próprios; outros hosts usam `HostDimensScope`).
* `using AppDimens.Net.Code.Scaled;` → `16.Sdp()` (tamanhos), `56.Hdp()` (altura), `200.Wdp()` (largura), `14.Ssp()` (texto).
* Sufixos: `a` (proporção), `i` (ignora multi-janela), `Px` (pixels). Facilitadores: `SdpRotate`, `SdpMode`, `SdpQualifier`, `SdpScreen`.
* Construtor condicional: `16.ScaledDp().Screen(DpQualifier.SmallWidth, 600, 24).Sdp()`.
* Testes: `FakeAppDimensContext` + `SetConfig(...)` + `NotifyChange()`.

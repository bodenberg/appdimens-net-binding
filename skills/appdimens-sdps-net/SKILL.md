---
name: appdimens-sdps-net
description: Use the AppDimens SDPS family for .NET — Bodenberg.AppDimens.Maui.Sdps (.NET MAUI) and its desktop adapters Bodenberg.AppDimens.Sdps.Wpf, .WinForms and .WinUI — to make sizes and text responsive with Android SDP/SSP parity ({sdp:16}, {ssp:14}, 16.Sdp(), 120.SdpPx(), inverters, builders, buckets). Use when an app references one of these packages, the code uses AppDimens.Maui / AppDimens.Sdps.* namespaces, or the user wants Android-style `_16sdp` sizing in MAUI, WPF, Windows Forms or WinUI 3.
---

# AppDimens SDPS for .NET (MAUI · WPF · WinForms · WinUI 3)

Native .NET port of [appdimens-sdps](https://github.com/bodenberg/appdimens-sdps): the numbers you already write
(`16`, `14`) are sizes on a **300 dp-wide reference screen**, scaled to the current screen/window
(`16` → ≈19 on a 360 dp phone, ≈32 on 600 dp). One engine (`AppDimensResolver`, MAUI-free), four packages, all **3.6.1**:

| Package | UI | TFMs | Setup | Markup / code |
|---------|----|------|-------|---------------|
| `Bodenberg.AppDimens.Maui.Sdps` | .NET MAUI (Android, iOS, macOS, Windows) | net8/9/10 (+net11) | `builder.UseAppDimensSdps()` | `{sdp:16}`, `{dimen:Sdp Value=16}`, `16.Sdp()` |
| `Bodenberg.AppDimens.Sdps.Wpf` | WPF | net8/9/10-windows | `AppDimensWpf.Initialize(this)` **before** `InitializeComponent()` | `{sdps:Sdp 16}` (live), `16.Sdp()` |
| `Bodenberg.AppDimens.Sdps.WinForms` | Windows Forms | net8/9/10-windows | `AppDimensWinForms.Initialize(this)` **after** `InitializeComponent()` | `120.SdpPx()`, `14.SspPt()` inside `Apply` |
| `Bodenberg.AppDimens.Sdps.WinUI` | WinUI 3 (WinAppSDK ≥ 1.6) | net8/9/10-windows10.0.19041.0 | `AppDimensWinUI.Initialize(window)` in `OnLaunched` | `{sdps:Sdp Value=16}`, `16.Sdp()` |

Desktop adapters embed the engine and bucket tables and do **not** reference MAUI.
Not this family: KMP strategies (`FSdp`, `PSdp`, `ASdp`…) → skill `appdimens-net`; Android-only XML `@dimen/_16sdp`
→ skill `appdimens-android-sdps-ssps`.

## Choosing the function

| Size… | Function | Notes |
|-------|----------|-------|
| padding, margins, icons, buttons (default) | `Sdp` | smallest side |
| follows height / width | `Hdp` / `Wdp` | headers, side panels |
| text (respects user font scale) | `Ssp` (`Hsp`, `Wsp`) | |
| text ignoring font scale | `Sem` (`Hem`, `Wem`) | |
| suffix `a` | `Sdpa`, `Sspa`… | aspect-ratio correction (non-16:9) |
| suffix `i` | `Sdpi`, `Hdpi`, `Wdpi`, `Sspi`, `Semi` (+ `ia`) | **frozen**: resolved against a baseline captured at init, ignores resizes; `AppDimensSdps.CaptureBaseline()` re-freezes |
| inverters | `SdpPh`, `SdpLw`, `SdpLh`, `SdpPw`, `HdpLw`, `HdpPw`, `WdpLh`, `WdpPh` (+`a`), `SspPh`… | swap axis by orientation (`InverterType`) |

Index range: sdp/hdp/wdp **-300…600** (negatives OK: `(-8).Sdp()`), ssp/sem **1…600**. All values are dp
(device-independent); `*Px` = physical pixels.

## .NET MAUI (`Bodenberg.AppDimens.Maui.Sdps`)

```csharp
// MauiProgram.cs — REQUIRED (without it metrics are zero and sizes are wrong)
using AppDimens.Maui;
using AppDimens.Maui.Core;
builder.UseMauiApp<App>().UseAppDimensSdps(o => o.ScalingMode = ScalingMode.Hybrid); // Hybrid is the default

// App.xaml.cs — follow window resizes (desktop, split screen, foldables)
protected override Window CreateWindow(IActivationState? state)
{
    var window = new Window(new AppShell());
    AppDimensSdpsWindow.Attach(window);
    return window;
}
```

XAML — every short prefix needs its own `xmlns` (all map to the same assembly); or use one `dimen` namespace:

```xml
xmlns:sdp="clr-namespace:AppDimens.Maui.Markup;assembly=AppDimens.Maui.Markup"
xmlns:ssp="clr-namespace:AppDimens.Maui.Markup;assembly=AppDimens.Maui.Markup"
xmlns:dimen="clr-namespace:AppDimens.Maui.Xaml;assembly=AppDimens.Maui.Xaml"

<VerticalStackLayout Padding="{sdp:16}" Spacing="{sdp:8}">
  <Label FontSize="{ssp:20}" />
  <BoxView HeightRequest="{dimen:Hdp Value=48}" />
  <Label Padding="{dimen:Sdp Value=16 Inverter=SwToPh}" />
  <Label FontSize="{scaled:14, Tablet=18, Landscape=16, Desktop=20}" />
  <BoxView WidthRequest="{sdpRotate:30, Rotation=45, Orientation=Landscape}" />
  <Label FontSize="{sdpQualifier:16, Qualified=20, QualifierType=SmallWidth, Threshold=600}" />
</VerticalStackLayout>
```

Tip: put all `xmlns:` prefixes in a `Resources/Styles/AppDimensImports.xaml` merged in `App.xaml`.

C# namespaces: `AppDimens.Maui` (`AppDimensSdps`, `AppDimensResolver`, `AppDimensSdpsWindow`), `AppDimens.Maui.Core`
(`AppDimensOptions`, `ScalingMode`, `DpQualifier`, `InverterType`, `UiModeType`), `AppDimens.Maui.Extensions`
(`16.Sdp()`, `PaddingSdp()`, `MarginSdp()`, `FontSsp()`), `AppDimens.Maui.Helpers` (`SdpRotate`, `SdpQualifier`,
`SdpMode`, `SdpScreen`), `AppDimens.Maui.Builders` (`Responsive`, `ScaledDp()`), `AppDimens.Maui.Converters`
(`SdpConverter`, `SspConverter`, `HdpConverter`…, `MmToPxConverter`, `DimenPhysicalUnits`).

```csharp
using AppDimens.Maui.Extensions;
using AppDimens.Maui.Builders;
using AppDimens.Maui.Core;

card.Padding = new Thickness(16.Sdp(), 8.Sdp());   // per-side: compute in C# (markup is uniform)
title.FontSize = 20.Ssp();
double size = Responsive.Value(14).Tablet(18).Landscape(16).Desktop(20).Sdp();
double tv = Responsive.Value(12).Screen(UiModeType.Television, DpQualifier.SmallWidth, 720, 24).Sdp();
AppDimensResolver.Instance.SetFontScale(1.3f);     // accessibility font scale for Ssp
```

**Rotation/resize rule (most common bug):** the resolver updates automatically, but XAML markup in MAUI is evaluated
**at page load** — it does not live-update. For values that must track rotation/resize, set them in C#
(`OnSizeAllocated`, `SizeChanged`, or a handler on `AppDimensResolver.Instance.Metrics.Changed`). On Android keep
`ConfigurationChanges` on `MainActivity` and call `AppDimensResolver.Instance.RefreshMetricsFromDevice()` from
`OnConfigurationChanged`.

Scaling modes: `Hybrid` (default: bucket if present, else formula), `Continuous` (`index/300 × axis`), `Bucket`
(Android `values-sw{N}dp` parity, ~358 buckets), `HybridPreferContinuous`. Trimming/AOT roots ship with the package.

## WPF (`Bodenberg.AppDimens.Sdps.Wpf`)

```csharp
using AppDimens.Sdps.Wpf;
public MainWindow() { AppDimensWpf.Initialize(this); InitializeComponent(); }
```

```xml
xmlns:sdps="clr-namespace:AppDimens.Sdps.Wpf;assembly=AppDimens.Sdps.Wpf"
<Button Width="{sdps:Sdp 120}" Height="{sdps:Hdp 40}" Margin="{sdps:Sdp 16}" FontSize="{sdps:Ssp 14}" />
<RowDefinition Height="{sdps:Hdp 56}" />            <!-- Thickness/CornerRadius/GridLength converted automatically -->
<sdps:DimenConverter x:Key="Sdp" Function="Sdp" />  <!-- {Binding X, Converter={StaticResource Sdp}, ConverterParameter=Ssp} -->
```

Markup is a **live binding** (works in styles/templates). Options: `Live=False`, `As=Thickness|CornerRadius|GridLength|Double`.
Code-behind values are not live: re-apply them in `AppDimensWpf.Changed += (_, _) => ...`. Also `AppDimensWpf.Refresh()`,
`AppDimensWpf.Detach()`, `AppDimensWpf.Initialize(window, new AppDimensOptions { ScalingMode = ScalingMode.Continuous })`.

## Windows Forms (`Bodenberg.AppDimens.Sdps.WinForms`)

```csharp
using AppDimens.Sdps.WinForms;
public MainForm()
{
    InitializeComponent();
    AutoScaleMode = AutoScaleMode.None;              // avoid double DPI scaling
    AppDimensWinForms.Initialize(this);
    AppDimensWinForms.Apply(this, () =>              // runs now + after every resize / DPI change
    {
        saveButton.Size = new Size(120.SdpPx(), 40.HdpPx());               // int pixels
        saveButton.Font = new Font(saveButton.Font.FontFamily, 14.SspPt()); // float points — never SspPx for fonts
        panel1.Padding  = DimenPx.Padding(16.Sdp());                       // DimenPx.ToPx/ToPt/Padding/Size take dp
    });
}
```

Every function has three forms: `Name()` (dp), `NamePx()` (pixels), `NamePt()` (points).

## WinUI 3 (`Bodenberg.AppDimens.Sdps.WinUI`)

```csharp
using AppDimens.Sdps.WinUI;
m_window = new MainWindow();
AppDimensWinUI.Initialize(m_window);   // before Activate()
m_window.Activate();
```

```xml
xmlns:sdps="using:AppDimens.Sdps.WinUI"
<Grid Padding="{sdps:Sdp Value=16}" RowSpacing="{sdps:Sdp Value=8}">   <!-- WinUI requires Value= -->
<sdps:DimenConverter x:Key="Sdp" Function="Sdp" />                     <!-- works with {x:Bind} -->
```

Markup values are re-applied on resize (weak registry). Code-behind: `AppDimensWinUI.Changed`.

## Custom hosts (Avalonia, Uno, game loops, tests)

```csharp
using AppDimens.Maui;
using AppDimens.Maui.Core;
var source = new ManualDisplayMetricsSource(widthDp: 1280, heightDp: 720, density: 1.5);
AppDimensResolver.Instance.UseDisplaySource(source);   // BEFORE Initialize
AppDimensSdps.Initialize();
double pad = AppDimensSdps.Sdp(16);
source.Update(1600, 900, 1.5);                          // from your resize handler
```

Or implement `IDisplayMetricsSource`. `DimenFunction` enum evaluates any of the 30 functions: `DimenFunction.Sdp.Evaluate(16)`.

## Troubleshooting

| Symptom | Fix |
|---------|-----|
| Every size wrong / tiny (MAUI) | `UseAppDimensSdps()` missing |
| `Type sdp not found` | missing `xmlns:sdp=...` on that page |
| MAUI desktop values don't follow window | `AppDimensSdpsWindow.Attach(window)` + set live values in C# |
| WPF/WinUI code values don't update | re-apply in `AppDimensWpf.Changed` / `AppDimensWinUI.Changed` |
| WinForms 2× too big on 200 % DPI | `AutoScaleMode = AutoScaleMode.None` |
| Text too big on tablets | `Ssp` for text, `Sdp` for layout; `i` suffix for constant values |

References: [MAUI guide](../../docs/Bodenberg.AppDimens.Maui.Sdps/README.md) ·
[WPF](../../docs/Bodenberg.AppDimens.Sdps.Wpf/README.md) · [WinForms](../../docs/Bodenberg.AppDimens.Sdps.WinForms/README.md) ·
[WinUI](../../docs/Bodenberg.AppDimens.Sdps.WinUI/README.md) · full API: `docs/<PackageId>/API.md`.

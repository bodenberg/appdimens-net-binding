# AppDimens .NET — Beginner's Guide

This guide assumes **no previous knowledge** of AppDimens. Follow the section for your UI framework,
copy the code, run the app, resize the window — done. A short Portuguese summary is at the end.

---

## 1. The problem (and the fix)

When you write `Width="200"` or `FontSize="14"`, the size is **fixed**: it looks fine on the screen you
designed for, too small on a big monitor or tablet, and too big on a small phone or a narrow window.

AppDimens fixes that with one idea:

> Keep writing the numbers you already know — AppDimens treats them as sizes on a
> **300-unit-wide reference screen** and scales them to the real window.

| Window (smaller side) | `16.Sdp()` becomes |
|-----------------------|--------------------|
| 300 | 16 |
| 360 (typical phone) | ≈ 19 |
| 600 (tablet / small window) | ≈ 32 |
| 900 (desktop window) | ≈ 48 |

Results are in **device-independent units** (dp on Android, DIPs/effective pixels on Windows), so they are
already correct for any display density/DPI.

---

## 2. Install

| Framework | Command |
|-----------|---------|
| .NET MAUI | `dotnet add package AppDimens.Net.Maui` |
| WPF | `dotnet add package AppDimens.Net.Wpf` |
| Windows Forms | `dotnet add package AppDimens.Net.WinForms` |
| WinUI 3 | `dotnet add package AppDimens.Net.WinUI` |
| Every strategy (fluid, percent, auto, power…) | `dotnet add package AppDimens.Net.Sdk` |

In Visual Studio you can also use **Manage NuGet Packages… → Browse** and search the package name.

---

## 3. Bootstrap once (pick your framework)

### .NET MAUI — in `App.xaml.cs`

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

### WPF — in `MainWindow.xaml.cs`

```csharp
using AppDimens.Net.Wpf;

public MainWindow()
{
    AppDimensWpf.Initialize(this);   // before InitializeComponent()
    InitializeComponent();
}
```

### Windows Forms — in your form constructor

```csharp
using AppDimens.Net.WinForms;

public MainForm()
{
    InitializeComponent();
    AppDimensWinForms.Initialize(this);
}
```

### WinUI 3 — in `App.xaml.cs`

```csharp
using AppDimens.Net.WinUI;

protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
{
    m_window = new MainWindow();
    AppDimensWinUI.Initialize(m_window);
    m_window.Activate();
}
```

What this one line does:

1. creates a **window scope** that reads the window size, density/DPI and font scale;
2. initializes the cache and an event-driven **watcher** — every resize, rotation, DPI change or split-screen
   change updates the values automatically (no manual invalidation);
3. publishes the scope as the **ambient context** that powers every parameterless call like `16.Sdp()`.

---

## 4. Use it

### In C# (all frameworks)

```csharp
using AppDimens.Net.Code.Scaled;

myButton.WidthRequest = 120.Sdp();   // MAUI   (WPF/WinUI: myButton.Width = 120.Sdp();)
myLabel.FontSize      = 14.Ssp();
```

Windows Forms works in **pixels** and **points**:

```csharp
using AppDimens.Net.Code.Scaled;
using AppDimens.Net.WinForms;

AppDimensWinForms.Apply(this, () =>          // runs now + after every resize
{
    button1.Width = DimenPx.ToPx(120.Sdp());
    label1.Font   = new Font(label1.Font.FontFamily, DimenPx.ToPt(14.Ssp()));
});
```

### In XAML

| Framework | Namespace | Example |
|-----------|-----------|---------|
| MAUI | `xmlns:appdimens="clr-namespace:AppDimens.Net.Maui.Xaml;assembly=AppDimens.Net.Maui"` | `Padding="{appdimens:Sdp Value=16}"` |
| WPF | `xmlns:ad="clr-namespace:AppDimens.Net.Wpf;assembly=AppDimens.Net.Wpf"` | `Margin="{ad:Sdp 16}"` (updates live) |
| WinUI 3 | `xmlns:ad="using:AppDimens.Net.WinUI"` | `Margin="{ad:Sdp Value=16}"` (re-applied on resize) |

---

## 5. Which function do I use?

| I want to size… | Call | Notes |
|---|---|---|
| Padding, margins, spacing, icons, buttons — **default choice** | `16.Sdp()` | follows the *smaller* side of the window |
| Same, slightly bigger on very tall/wide windows | `16.Sdpa()` | aspect-ratio curve |
| Something that follows the **height** (headers, toolbars) | `48.Hdp()` | |
| Something that follows the **width** (side menus, banners) | `100.Wdp()` | |
| A size that ignores split-screen/multi-window shrinking | `24.Sdpi()` | |
| **Text** | `16.Ssp()` | respects the system font scale |
| Text that ignores the font-scale setting | `16.Sem()` | |
| Gentle growth on huge screens | `24.FSdp()` | `using AppDimens.Net.Code.Fluid;` |
| Percentage-based | `24.PSdp()` / `50.SpaceWDp(ctx)` | `using AppDimens.Net.Code.Percent;` |

Every call returns a plain `float` in device-independent units — use it for `Padding`, `Width`,
`WidthRequest`, `FontSize`, etc.

---

## 6. Common mistakes

| Symptom | Fix |
|---------|-----|
| `InvalidOperationException: AppDimens ambient context is not initialized` | Call the bootstrap line of section 3 **before** creating pages/controls that use `16.Sdp()`. |
| Values don't change when the window is resized | XAML: WPF and WinUI update automatically. Values set in C#: re-apply them in the framework's `Changed` event (`AppDimensWpf.Changed`, `AppDimensWinUI.Changed`) or inside `AppDimensWinForms.Apply`. |
| Everything is huge on a big monitor | Use `Sdp` (smaller side) for most sizes; use `Wdp`/`Hdp` only where you really want width/height scaling; consider `FSdp` for gentler growth. |
| Windows Forms controls twice as big on 200 % DPI | Set `AutoScaleMode = AutoScaleMode.None` on forms sized by AppDimens. |

---

## 7. Other strategies

Each satellite package exposes the same shape: `{P}Sdp`, `{P}Sdpa`, `{P}Hdp`, `{P}Wdp`, `{P}Sdpi(a)` plus a
generic `To{X}Dp(value, ctx, qualifier, inverter…)`:

```csharp
using AppDimens.Net.Code.Percent;   // 24.PSdp()
using AppDimens.Net.Code.Fluid;     // 28.FSdp(), 28.ToFluidDp(ctx)
using AppDimens.Net.Code.Auto;      // 28.ASdp()
```

## 8. Builders (conditional overrides)

```csharp
using AppDimens.Net.Code.Scaled;
using AppDimens.Net.Common;

var size = 16.ScaledDp()
    .Screen(DpQualifier.SmallWidth, 600, 24)  // sw ≥ 600 → 24
    .Screen(UiModeType.Television, 40)        // TV → 40
    .Sdp();
```

## 9. Auto-resize (fit content to a box)

```csharp
using AppDimens.Net;
using AppDimens.Net.Code.Resize;

var ctx = AppDimensAmbient.Require();
float fontSp = DimenResize.AutoResizeTextPx("Hello world", boxWidthPx: 600f, minSp: 10, maxSp: 22, stepSp: 1, maxLines: 2, ctx: ctx);  // sp
float sideDp = DimenResize.AutoResizeSquarePx(300f, 200f, minDp: 16, maxDp: 100, stepDp: 4, ctx: ctx);   // dp (box sizes are px)
```

## 10. Testing your own UI logic

```csharp
using AppDimens.Net.Code.Scaled;
using AppDimens.Net.Testing;

var ctx = new FakeAppDimensContext();
ctx.SetConfig(360, 740, densityDpi: 420);       // simulate a phone
float v = 16f.ToDynamicScaledDp(ctx);
ctx.SetConfig(600, 1000); ctx.NotifyChange();    // simulate a resize; watchers fire
```

Custom hosts can use `HostDimensScope` directly:

```csharp
using AppDimens.Net.Core;

var scope = AppDimensHost.Activate(new HostDimensScope());
scope.Update(widthDp: 1024, heightDp: 768, density: 1.25);
```

---

## Português (resumo)

1. **Instale** o pacote do seu framework: `AppDimens.Net.Maui`, `AppDimens.Net.Wpf`, `AppDimens.Net.WinForms`
   ou `AppDimens.Net.WinUI`.
2. **Inicialize uma vez** (seção 3): MAUI `AppDimensMaui.AttachWindow(window)`, WPF `AppDimensWpf.Initialize(this)`,
   WinForms `AppDimensWinForms.Initialize(this)`, WinUI `AppDimensWinUI.Initialize(window)`.
3. **Use** os mesmos números de sempre: `16.Sdp()` para tamanhos/margens, `14.Ssp()` para texto,
   `56.Hdp()`/`200.Wdp()` para seguir altura/largura. No XAML: `{ad:Sdp 16}` (WPF) ou `{ad:Sdp Value=16}` (WinUI).
4. No **WinForms**, converta para pixels/pontos: `DimenPx.ToPx(120.Sdp())`, `DimenPx.ToPt(14.Ssp())`, dentro de
   `AppDimensWinForms.Apply(this, () => { ... })`.

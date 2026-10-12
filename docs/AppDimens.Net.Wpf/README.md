# AppDimens.Net.Wpf — complete guide

**WPF integration for [AppDimens.Net](https://www.nuget.org/packages/AppDimens.Net)** — the pure C#
port of AppDimens KMP. Sizes written as `{ad:Sdp 16}` (or `16.Sdp()` in C#) follow the window
size and the monitor DPI, and XAML values update **live** while the window is resized.

Includes the **scaled** (`Sdp`, `Hdp`, `Wdp`, `Ssp`…), **fluid** (`FSdp`…) and **percent**
(`PSdp`…, `SpaceW`, `SpaceH`) strategies out of the box.

| | |
|---|---|
| **Frameworks** | `net8.0-windows`, `net9.0-windows`, `net10.0-windows` (+ `net11.0-windows` with SDK 11) |
| **Depends on** | `AppDimens.Net`, `AppDimens.Net.Fluid`, `AppDimens.Net.Percent` (installed automatically) |

---

## Step 1 — Install

```bash
dotnet add package AppDimens.Net.Wpf
```

## Step 2 — Initialize (one line)

```csharp
using System.Windows;
using AppDimens.Net.Wpf;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        AppDimensWpf.Initialize(this);   // ← must run before InitializeComponent()
        InitializeComponent();
    }
}
```

## Step 3 — XAML

```xml
<Window ...
        xmlns:ad="clr-namespace:AppDimens.Net.Wpf;assembly=AppDimens.Net.Wpf">

    <StackPanel Margin="{ad:Sdp 16}">
        <TextBlock Text="Hello" FontSize="{ad:Ssp 22}" />
        <Button Content="Save" Width="{ad:Sdp 120}" Height="{ad:Hdp 40}" Padding="{ad:Sdp 8}" />
        <Border Width="{ad:SpaceW 50}" Height="{ad:FSdp 80}" CornerRadius="{ad:Sdp 12}" Background="#EEE" />
    </StackPanel>
</Window>
```

* `Margin`, `Padding`, `CornerRadius`, `GridLength` are converted automatically.
* `{ad:SpaceW 50}` = 50 % of the window width; `{ad:FSdp 80}` = fluid scaling (grows slower on huge screens).

## Step 4 — C#

```csharp
using AppDimens.Net.Code.Scaled;   // Sdp, Hdp, Wdp, Ssp …
using AppDimens.Net.Code.Fluid;    // FSdp, FHdp …
using AppDimens.Net.Code.Percent;  // PSdp, SpaceWDp …
using AppDimens.Net.Wpf;           // AppDimensWpf

saveButton.Width  = 120.Sdp();
titleText.FontSize = 22.Ssp();
banner.Height     = 80.FSdp();

// re-apply code-behind values after resizes
AppDimensWpf.Changed += (_, _) => titleText.FontSize = 22.Ssp();
```

---

## Which function?

| Need | Function | XAML |
|------|----------|------|
| Most sizes (padding, icons, buttons) | `Sdp` | `{ad:Sdp 16}` |
| Follow window height / width | `Hdp` / `Wdp` | `{ad:Hdp 56}` |
| Text | `Ssp` | `{ad:Ssp 14}` |
| Text ignoring the font-scale setting | `Sem` / `Hem` / `Wem` | `{ad:Sem 12}` |
| Grows more gently on very large windows | `FSdp`, `FHdp`, `FWdp` | `{ad:FSdp 24}` |
| Percent-based scaling | `PSdp`, `PHdp`, `PWdp` | `{ad:PSdp 24}` |
| Literal % of the window | `SpaceW` / `SpaceH` | `{ad:SpaceW 50}` |
| Aspect-ratio curve / ignore multi-window | suffix `a` / `i` | `{ad:Sdpa 16}` |

41 functions are available; see the `DimenFunction` enum. Options: `Live=False`, `As=Thickness|CornerRadius|GridLength|Double`.

## Bindings

```xml
<Window.Resources>
    <ad:DimenConverter x:Key="Dimen" Function="Sdp" />
</Window.Resources>
<Border Width="{Binding CardWidth, Converter={StaticResource Dimen}}" />
<TextBlock FontSize="{Binding TitleSize, Converter={StaticResource Dimen}, ConverterParameter=Ssp}" />
```

## Troubleshooting

| Problem | Fix |
|---------|-----|
| `AppDimens ambient context is not initialized` in C# | Call `AppDimensWpf.Initialize(this)` first (XAML initializes automatically with the screen size). |
| Code-behind values don't update | Re-apply them in `AppDimensWpf.Changed`. |

---

## All 41 functions

Every function is available as a markup extension (`{ad:Name 16}`), in `DimenConverter` (`Function="Name"`) and in
`DimenFunction` (`DimenFunction.Name.Evaluate(16)`):

| Group | Functions |
|-------|-----------|
| Scaled (layout) | `Sdp`, `Sdpa`, `Sdpi`, `Sdpia`, `Hdp`, `Hdpa`, `Hdpi`, `Wdp`, `Wdpa`, `Wdpi` |
| Scaled (text) | `Ssp`, `Sspa`, `Sem`, `Hem`, `Wem` |
| Fluid | `FSdp`, `FSdpa`, `FSdpi`, `FSdpia`, `FHdp`, `FHdpa`, `FHdpi`, `FHdpia`, `FWdp`, `FWdpa`, `FWdpi`, `FWdpia` |
| Percent | `PSdp`, `PSdpa`, `PSdpi`, `PSdpia`, `PHdp`, `PHdpa`, `PHdpi`, `PHdpia`, `PWdp`, `PWdpa`, `PWdpi`, `PWdpia` |
| Literal % of the window | `SpaceW`, `SpaceH` |

## Styles, templates and triggers

Markup values stay live inside styles and templates:

```xml
<Window.Resources>
    <Style TargetType="Button">
        <Setter Property="Margin"   Value="{ad:Sdp 4}" />
        <Setter Property="Padding"  Value="{ad:Sdp 8}" />
        <Setter Property="FontSize" Value="{ad:Ssp 14}" />
    </Style>
    <ControlTemplate x:Key="Card" TargetType="ContentControl">
        <Border CornerRadius="{ad:Sdp 12}" Padding="{ad:Sdp 16}" Background="#EEE">
            <ContentPresenter />
        </Border>
    </ControlTemplate>
</Window.Resources>
```

## Options

| Option | Example | Meaning |
|--------|---------|---------|
| `Live` | `{ad:Sdp 16, Live=False}` | compute once (no binding) |
| `As` | `{ad:Sdp 16, As=Thickness}` | force `Double`, `Thickness`, `CornerRadius` or `GridLength` |

## Evaluate any function from code

```csharp
using AppDimens.Net.Wpf;

double size = DimenFunction.FSdp.Evaluate(24);     // same as {ad:FSdp 24}
var scope = AppDimensWpf.Scope;                     // current WpfWindowDimensScope (or null)
scope?.Refresh();                                   // re-read the window size manually
AppDimensWpf.Detach();                              // stop tracking (ambient context cleared)
```

## How it works

`AppDimensWpf.Initialize(window)` creates a `WpfWindowDimensScope` (a `HostDimensScope`) fed by `SizeChanged`,
`DpiChanged` and `Loaded`, activates it as the ambient context and bumps `DimenLiveSource.Version` after every change;
each `{ad:...}` value is a one-way binding to that version through `DimenConverter`, so WPF re-evaluates it on resize.
Before the window is laid out, the screen work area is used.

📚 API reference: [API.md](API.md) · Core concepts: [AppDimens.Net](../AppDimens.Net/README.md)

## Português (resumo)

`dotnet add package AppDimens.Net.Wpf` → no construtor da janela `AppDimensWpf.Initialize(this);` antes de
`InitializeComponent()` → no XAML `xmlns:ad="clr-namespace:AppDimens.Net.Wpf;assembly=AppDimens.Net.Wpf"` e
use `{ad:Sdp 16}`, `{ad:Ssp 14}`, `{ad:SpaceW 50}` (50 % da largura). Em C#: `120.Sdp()` com
`using AppDimens.Net.Code.Scaled;`.


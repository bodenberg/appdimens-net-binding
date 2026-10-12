# Bodenberg.AppDimens.Sdps.Wpf

📖 **Documentation:** [Bodenberg.AppDimens.Sdps.Wpf guide](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/Bodenberg.AppDimens.Sdps.Wpf/README.md) · [complete API reference](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/Bodenberg.AppDimens.Sdps.Wpf/API.md) · [all AppDimens .NET packages](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/README.md)

**Responsive sizes for WPF.** Write `{sdps:Sdp 16}` instead of `16` and your margins, buttons and
text grow on big windows and shrink on small ones — automatically, while the user resizes the
window or drags it to another monitor.

Same engine as [`Bodenberg.AppDimens.Maui.Sdps`](https://www.nuget.org/packages/Bodenberg.AppDimens.Maui.Sdps)
(Android `sdp`/`ssp` parity), packaged for WPF with **no .NET MAUI dependency**.

| | |
|---|---|
| **Frameworks** | `net8.0-windows`, `net9.0-windows`, `net10.0-windows` (+ `net11.0-windows` with SDK 11) |
| **Works with** | Any WPF app (`<UseWPF>true</UseWPF>`), any monitor DPI |
| **License** | Apache-2.0 |

---

## Table of contents

1. [How it works (30-second version)](#how-it-works-30-second-version)
2. [Step 1 — Install](#step-1--install)
3. [Step 2 — Initialize (one line)](#step-2--initialize-one-line)
4. [Step 3 — Use it in XAML](#step-3--use-it-in-xaml)
5. [Step 4 — Use it in C#](#step-4--use-it-in-c)
6. [Complete example window](#complete-example-window)
7. [Which function should I use?](#which-function-should-i-use)
8. [Styles, templates and resources](#styles-templates-and-resources)
9. [Bindings with numbers from your ViewModel](#bindings-with-numbers-from-your-viewmodel)
10. [Advanced options](#advanced-options)
11. [Troubleshooting](#troubleshooting)
12. [Português (resumo)](#português-resumo)

---

## How it works (30-second version)

* You keep designing with the numbers you already know (`16` for padding, `14` for text…).
* AppDimens treats those numbers as sizes on a **300-unit-wide reference screen** and scales them to the
  current window: when the window's smaller side is 600 units, `{sdps:Sdp 16}` becomes about `32`.
* WPF units are device-independent (1/96 inch), so the result is already correct on 100 %, 150 % or 200 % DPI.
* When the window is resized, every `{sdps:...}` value is **recalculated automatically**.

---

## Step 1 — Install

**Visual Studio:** right-click your WPF project → **Manage NuGet Packages…** → **Browse** → search
`Bodenberg.AppDimens.Sdps.Wpf` → **Install**.

**Command line:**

```bash
dotnet add package Bodenberg.AppDimens.Sdps.Wpf
```

---

## Step 2 — Initialize (one line)

Open `MainWindow.xaml.cs` and add **one line before `InitializeComponent()`**:

```csharp
using System.Windows;
using AppDimens.Sdps.Wpf;

namespace MyApp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        AppDimensWpf.Initialize(this);   // ← sizes now follow THIS window
        InitializeComponent();
    }
}
```

> 💡 Forgot this line? It still works — the values use the screen size instead of the window size and
> do not follow window resizes.

---

## Step 3 — Use it in XAML

1. Add the namespace to the root element of your XAML file:

```xml
xmlns:sdps="clr-namespace:AppDimens.Sdps.Wpf;assembly=AppDimens.Sdps.Wpf"
```

2. Replace fixed numbers with `{sdps:Function Number}`:

```xml
<!-- BEFORE -->
<Button Width="120" Height="40" Margin="16" FontSize="14" Content="Save" />

<!-- AFTER -->
<Button Width="{sdps:Sdp 120}"
        Height="{sdps:Hdp 40}"
        Margin="{sdps:Sdp 16}"
        FontSize="{sdps:Ssp 14}"
        Content="Save" />
```

`Margin`, `Padding`, `CornerRadius`, `BorderThickness` and `Grid` row/column sizes are converted
automatically — `Margin="{sdps:Sdp 16}"` means 16 on all four sides.

---

## Step 4 — Use it in C#

```csharp
using AppDimens.Sdps.Wpf;     // 16.Sdp(), 14.Ssp() …

myButton.Width    = 120.Sdp();
myButton.Margin   = new Thickness(16.Sdp());
myTitle.FontSize  = 20.Ssp();
```

Values set from code are **not** updated automatically. Re-apply them when the window changes:

```csharp
using AppDimens.Sdps.Wpf;

AppDimensWpf.Changed += (_, _) => ApplySizes();

void ApplySizes()
{
    myButton.Width = 120.Sdp();
    myTitle.FontSize = 20.Ssp();
}
```

---

## Complete example window

```xml
<Window x:Class="MyApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:sdps="clr-namespace:AppDimens.Sdps.Wpf;assembly=AppDimens.Sdps.Wpf"
        Title="AppDimens demo" Width="900" Height="600">

    <Grid Margin="{sdps:Sdp 16}">
        <Grid.RowDefinitions>
            <RowDefinition Height="{sdps:Hdp 56}" />   <!-- header follows window HEIGHT -->
            <RowDefinition Height="*" />
        </Grid.RowDefinitions>
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="{sdps:Wdp 200}" /> <!-- side menu follows window WIDTH -->
            <ColumnDefinition Width="*" />
        </Grid.ColumnDefinitions>

        <TextBlock Grid.ColumnSpan="2" Text="Dashboard"
                   FontSize="{sdps:Ssp 22}" VerticalAlignment="Center" />

        <StackPanel Grid.Row="1">
            <Button Content="Home"     Margin="{sdps:Sdp 4}" Padding="{sdps:Sdp 8}" FontSize="{sdps:Ssp 14}" />
            <Button Content="Settings" Margin="{sdps:Sdp 4}" Padding="{sdps:Sdp 8}" FontSize="{sdps:Ssp 14}" />
        </StackPanel>

        <Border Grid.Row="1" Grid.Column="1" Background="#EEE"
                CornerRadius="{sdps:Sdp 12}" Padding="{sdps:Sdp 16}">
            <TextBlock TextWrapping="Wrap" FontSize="{sdps:Ssp 14}"
                       Text="Resize the window: every size above is recalculated." />
        </Border>
    </Grid>
</Window>
```

---

## Which function should I use?

| I want to size… | Use | Example |
|-----------------|-----|---------|
| Padding, margins, icons, cards — **most cases** | `Sdp` (follows the *smaller* side of the window) | `{sdps:Sdp 16}` |
| Things that follow the window **height** (header, toolbar) | `Hdp` | `{sdps:Hdp 56}` |
| Things that follow the window **width** (side menu, banner) | `Wdp` | `{sdps:Wdp 200}` |
| **Text** | `Ssp` | `{sdps:Ssp 14}` |
| Text by height / width | `Hsp` / `Wsp` | `{sdps:Hsp 18}` |
| Text that ignores the user font scale | `Sem`, `Hem`, `Wem` | `{sdps:Sem 12}` |
| Slightly bigger on very wide/tall windows | add **`a`**: `Sdpa`, `Sspa`, `Hspa`… | `{sdps:Sdpa 16}` |
| A size that must **stay fixed** after startup (does not follow resizes) | add **`i`**: `Sdpi`, `Hdpi`, `Wdpi`, `Sspi`… | `{sdps:Sdpi 16}` |

All 30 functions are available as `{sdps:Name value}` in XAML, as `value.Name()` in C#, and as
`DimenFunction.Name` for converters.

---

## Styles, templates and resources

Markup works inside styles and templates too, and stays live:

```xml
<Window.Resources>
    <Style TargetType="Button">
        <Setter Property="Margin"   Value="{sdps:Sdp 4}" />
        <Setter Property="Padding"  Value="{sdps:Sdp 8}" />
        <Setter Property="FontSize" Value="{sdps:Ssp 14}" />
    </Style>
</Window.Resources>
```

---

## Bindings with numbers from your ViewModel

`DimenConverter` scales a bound number:

```xml
<Window.Resources>
    <sdps:DimenConverter x:Key="Sdp" Function="Sdp" />
</Window.Resources>

<Border Width="{Binding CardWidth, Converter={StaticResource Sdp}}" />
<!-- or choose the function per binding: -->
<TextBlock FontSize="{Binding TitleSize, Converter={StaticResource Sdp}, ConverterParameter=Ssp}" />
```

---

## Advanced options

| Property / API | What it does |
|----------------|--------------|
| `Live="False"` | Computes the value once (no binding): `{sdps:Sdp 16, Live=False}` |
| `As="Thickness"` | Forces the output type (`Double`, `Thickness`, `CornerRadius`, `GridLength`) |
| `AppDimensWpf.Initialize(window, options)` | `new AppDimensOptions { ScalingMode = ScalingMode.Continuous }` for smooth (non-bucket) scaling |
| `AppDimensWpf.Refresh()` | Re-reads the window size manually |
| `AppDimensWpf.Detach()` | Stops tracking the window |
| `AppDimensSdps.CaptureBaseline()` | Re-freezes the baseline used by the `*i` functions |
| `AppDimensSdps.Resolver.SetFontScale(1.2f)` | Applies an app-level text zoom to `Ssp`/`Hsp`/`Wsp` |

---

## Troubleshooting

| Problem | Fix |
|---------|-----|
| `The name "Sdp" does not exist in namespace` | Check the `xmlns:sdps="clr-namespace:AppDimens.Sdps.Wpf;assembly=AppDimens.Sdps.Wpf"` line. Rebuild once. |
| Values do not change when resizing | Call `AppDimensWpf.Initialize(this)` in the window constructor. Values set from C# must be re-applied in `AppDimensWpf.Changed`. |
| Everything looks too big on a large monitor | Use `Sdp` for most things (it follows the *smaller* side). Use `Wdp`/`Hdp` only where you want width/height scaling. |
| I want the old fixed value for one control | Just keep the plain number — you can mix fixed and scaled values. |

---

## Português (resumo)

1. Instale: `dotnet add package Bodenberg.AppDimens.Sdps.Wpf`
2. No construtor da janela, **antes** de `InitializeComponent()`: `AppDimensWpf.Initialize(this);`
3. No XAML adicione `xmlns:sdps="clr-namespace:AppDimens.Sdps.Wpf;assembly=AppDimens.Sdps.Wpf"` e troque
   números fixos por `{sdps:Sdp 16}` (margens/tamanhos), `{sdps:Ssp 14}` (texto), `{sdps:Hdp 56}` (altura),
   `{sdps:Wdp 200}` (largura).
4. Em C#: `botao.Width = 120.Sdp();` e, para atualizar ao redimensionar, use `AppDimensWpf.Changed += ...`.

Os valores do XAML se atualizam sozinhos quando a janela muda de tamanho ou de monitor.

---

**Links:** [Repository](https://github.com/bodenberg/appdimens-net-binding) ·
[MAUI version](https://www.nuget.org/packages/Bodenberg.AppDimens.Maui.Sdps) ·
[Android library](https://github.com/bodenberg/appdimens-sdps)

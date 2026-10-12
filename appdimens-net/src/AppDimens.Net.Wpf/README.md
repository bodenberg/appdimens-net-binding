# AppDimens.Net.Wpf

📖 **Documentation:** [AppDimens.Net.Wpf guide](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.Wpf/README.md) · [complete API reference](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.Wpf/API.md) · [all AppDimens .NET packages](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/README.md)

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

## Português (resumo)

`dotnet add package AppDimens.Net.Wpf` → no construtor da janela `AppDimensWpf.Initialize(this);` antes de
`InitializeComponent()` → no XAML `xmlns:ad="clr-namespace:AppDimens.Net.Wpf;assembly=AppDimens.Net.Wpf"` e
use `{ad:Sdp 16}`, `{ad:Ssp 14}`, `{ad:SpaceW 50}` (50 % da largura). Em C#: `120.Sdp()` com
`using AppDimens.Net.Code.Scaled;`.

**More:** [AppDimens.Net core](https://www.nuget.org/packages/AppDimens.Net) ·
[Repository](https://github.com/bodenberg/appdimens-net-binding)

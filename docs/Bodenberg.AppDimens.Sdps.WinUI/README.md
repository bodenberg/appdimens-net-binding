# Bodenberg.AppDimens.Sdps.WinUI

> 📄 This guide is the package README of **Bodenberg.AppDimens.Sdps.WinUI** (mirrored from [`appdimens-sdps-net/src/AppDimens.Sdps.WinUI/README.md`](https://github.com/bodenberg/appdimens-net-binding/blob/main/appdimens-sdps-net/src/AppDimens.Sdps.WinUI/README.md) by `scripts/generate-docs.sh`). Complete API reference: [API.md](API.md) · [All packages](../README.md)

**Responsive sizes for WinUI 3 (Windows App SDK).** Write `{sdps:Sdp Value=16}` instead of `16`
and your margins, buttons and text adapt to the window — and are re-applied automatically when the
user resizes the window or moves it to a monitor with another scale.

Same engine as [`Bodenberg.AppDimens.Maui.Sdps`](https://www.nuget.org/packages/Bodenberg.AppDimens.Maui.Sdps)
(Android `sdp`/`ssp` parity), packaged for WinUI 3 with **no .NET MAUI dependency**.

| | |
|---|---|
| **Frameworks** | `net8.0-windows10.0.19041.0`, `net9.0-…`, `net10.0-…` (min. Windows 10 1809 / 17763) |
| **Windows App SDK** | 1.6 or newer (your app keeps its own version) |
| **License** | Apache-2.0 |

---

## Table of contents

1. [How it works](#how-it-works)
2. [Step 1 — Install](#step-1--install)
3. [Step 2 — Initialize in `App.OnLaunched`](#step-2--initialize-in-apponlaunched)
4. [Step 3 — Use it in XAML](#step-3--use-it-in-xaml)
5. [Step 4 — Use it in C#](#step-4--use-it-in-c)
6. [Complete example page](#complete-example-page)
7. [Which function should I use?](#which-function-should-i-use)
8. [Bindings and `x:Bind`](#bindings-and-xbind)
9. [Troubleshooting](#troubleshooting)
10. [Português (resumo)](#português-resumo)

---

## How it works

* Keep the numbers you already use (`16` for padding, `14` for text…).
* AppDimens treats them as sizes on a **300-unit-wide reference screen** and scales them to the window.
* WinUI units are device-independent (effective pixels), so the result is right at any display scale.
* Values set with `{sdps:...}` in XAML are **re-applied automatically** when the window changes.

> WinUI markup extensions need the property name: always write `Value=` → `{sdps:Sdp Value=16}`.

---

## Step 1 — Install

```bash
dotnet add package Bodenberg.AppDimens.Sdps.WinUI
```

or **Manage NuGet Packages…** → search `Bodenberg.AppDimens.Sdps.WinUI` → **Install**.

---

## Step 2 — Initialize in `App.OnLaunched`

Open `App.xaml.cs` and add **one line** after the window is created:

```csharp
using AppDimens.Sdps.WinUI;

protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
{
    m_window = new MainWindow();
    AppDimensWinUI.Initialize(m_window);   // ← sizes now follow THIS window
    m_window.Activate();
}
```

---

## Step 3 — Use it in XAML

1. Add the namespace to the root element (`Window`, `Page` or `UserControl`):

```xml
xmlns:sdps="using:AppDimens.Sdps.WinUI"
```

2. Replace fixed numbers:

```xml
<!-- BEFORE -->
<Button Width="120" Height="40" Margin="16" FontSize="14" Content="Save" />

<!-- AFTER -->
<Button Width="{sdps:Sdp Value=120}"
        Height="{sdps:Hdp Value=40}"
        Margin="{sdps:Sdp Value=16}"
        FontSize="{sdps:Ssp Value=14}"
        Content="Save" />
```

`Margin`, `Padding`, `CornerRadius` and `Grid` row/column sizes are converted automatically.

---

## Step 4 — Use it in C#

```csharp
using AppDimens.Sdps.WinUI;   // 16.Sdp(), 14.Ssp() …

saveButton.Width   = 120.Sdp();
saveButton.Margin  = new Thickness(16.Sdp());
titleText.FontSize = 22.Ssp();

// values set from code: re-apply after a resize
AppDimensWinUI.Changed += (_, _) => titleText.FontSize = 22.Ssp();
```

---

## Complete example page

```xml
<Page x:Class="MyApp.MainPage"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
      xmlns:sdps="using:AppDimens.Sdps.WinUI">

    <Grid Padding="{sdps:Sdp Value=16}" RowSpacing="{sdps:Sdp Value=8}">
        <Grid.RowDefinitions>
            <RowDefinition Height="{sdps:Hdp Value=56}" />
            <RowDefinition Height="*" />
        </Grid.RowDefinitions>

        <TextBlock Text="Dashboard" FontSize="{sdps:Ssp Value=22}" VerticalAlignment="Center" />

        <Border Grid.Row="1" CornerRadius="{sdps:Sdp Value=12}" Padding="{sdps:Sdp Value=16}"
                Background="{ThemeResource CardBackgroundFillColorDefaultBrush}">
            <StackPanel Spacing="{sdps:Sdp Value=8}">
                <TextBlock Text="Resize the window: every size is recalculated."
                           FontSize="{sdps:Ssp Value=14}" TextWrapping="Wrap" />
                <Button Content="Save" Width="{sdps:Sdp Value=120}" />
            </StackPanel>
        </Border>
    </Grid>
</Page>
```

---

## Which function should I use?

| I want to size… | Use | Example |
|-----------------|-----|---------|
| Padding, margins, icons, cards — **most cases** | `Sdp` | `{sdps:Sdp Value=16}` |
| Things that follow the window **height** | `Hdp` | `{sdps:Hdp Value=56}` |
| Things that follow the window **width** | `Wdp` | `{sdps:Wdp Value=200}` |
| **Text** | `Ssp` | `{sdps:Ssp Value=14}` |
| Text by height / width | `Hsp` / `Wsp` | `{sdps:Hsp Value=18}` |
| Text ignoring the user font scale | `Sem` | `{sdps:Sem Value=12}` |
| Slightly bigger on very wide/tall windows | add **`a`**: `Sdpa`, `Sspa` | `{sdps:Sdpa Value=16}` |
| Fixed after startup (ignores resizes) | add **`i`**: `Sdpi`, `Sspi` | `{sdps:Sdpi Value=16}` |

Extra options: `Live=False` (compute once) and `As=Thickness|CornerRadius|GridLength|Double` (force the type).

---

## Bindings and `x:Bind`

```xml
<Page.Resources>
    <sdps:DimenConverter x:Key="Sdp" Function="Sdp" />
</Page.Resources>

<Border Width="{x:Bind ViewModel.CardWidth, Converter={StaticResource Sdp}}" />
<TextBlock FontSize="{x:Bind ViewModel.TitleSize, Converter={StaticResource Sdp}, ConverterParameter=Ssp}" />
```

---

## Troubleshooting

| Problem | Fix |
|---------|-----|
| `The type 'Sdp' was not found` | Use `xmlns:sdps="using:AppDimens.Sdps.WinUI"` and the `Value=` syntax. |
| Sizes don't follow the window | Call `AppDimensWinUI.Initialize(window)` in `OnLaunched`, before `Activate()`. |
| Values set in C# don't update | Re-apply them in `AppDimensWinUI.Changed`. |

---

## Português (resumo)

1. Instale: `dotnet add package Bodenberg.AppDimens.Sdps.WinUI`
2. Em `App.OnLaunched`, depois de criar a janela: `AppDimensWinUI.Initialize(m_window);`
3. No XAML: `xmlns:sdps="using:AppDimens.Sdps.WinUI"` e troque números por `{sdps:Sdp Value=16}`
   (tamanhos), `{sdps:Ssp Value=14}` (texto), `{sdps:Hdp Value=56}` (altura), `{sdps:Wdp Value=200}` (largura).
4. Em C#: `botao.Width = 120.Sdp();` — para atualizar no redimensionamento use `AppDimensWinUI.Changed`.

---

**Links:** [Repository](https://github.com/bodenberg/appdimens-net-binding) ·
[WPF version](https://www.nuget.org/packages/Bodenberg.AppDimens.Sdps.Wpf) ·
[MAUI version](https://www.nuget.org/packages/Bodenberg.AppDimens.Maui.Sdps)

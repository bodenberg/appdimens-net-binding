# AppDimens.Net.Maui

**.NET MAUI integration for AppDimens.Net** — a live window scope (follows window size, rotation, density and
split screen on Android, iOS, macOS and Windows), XAML markup extensions and value converters.

| | |
|---|---|
| **NuGet** | [`AppDimens.Net.Maui`](https://www.nuget.org/packages/AppDimens.Net.Maui) (brings `AppDimens.Net`, `.Fluid`, `.Percent`) |
| **Namespaces** | `AppDimens.Net.Maui.Platform` (bootstrap), `AppDimens.Net.Maui.Xaml` (markup + converters) |
| **Frameworks** | `net8.0`, `net9.0`, `net10.0` (+ `net11.0`) — consumed by MAUI apps on every platform |
| **API reference** | [API.md](API.md) |

---

## Table of contents

1. [Install](#install)
2. [Step 1 — attach the window](#step-1--attach-the-window)
3. [Step 2 — use it in C#](#step-2--use-it-in-c)
4. [Step 3 — use it in XAML](#step-3--use-it-in-xaml)
5. [Converters (MVVM)](#converters-mvvm)
6. [Reacting to resize / rotation](#reacting-to-resize--rotation)
7. [Multiple windows](#multiple-windows)
8. [Split screen and font scale](#split-screen-and-font-scale)
9. [Complete example page](#complete-example-page)
10. [Troubleshooting](#troubleshooting)
11. [Português (resumo)](#português-resumo)

---

## Install

```bash
dotnet add package AppDimens.Net.Maui
# optional: every other strategy (Auto, Power, Logarithmic, …)
dotnet add package AppDimens.Net.Sdk
```

## Step 1 — attach the window

In `App.xaml.cs`, attach **before** any page that uses AppDimens is created:

```csharp
using AppDimens.Net.Maui.Platform;

public partial class App : Application
{
    public App() => InitializeComponent();

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window();
        AppDimensMaui.AttachWindow(window);              // ambient context + resize watcher
        window.Page = new NavigationPage(new MainPage());
        return window;
    }
}
```

`AttachWindow` returns the `WindowDimensScope`; `AppDimensMaui.Primary` gives it back later, `AppDimensMaui.Detach()`
removes it, and `AppDimensMaui.Init(app)` attaches the first window of an `Application` in one call.

## Step 2 — use it in C#

```csharp
using AppDimens.Net.Code.Scaled;
using AppDimens.Net.Code.Fluid;
using AppDimens.Net.Code.Percent;

card.Padding         = new Thickness(16.Sdp());
titleLabel.FontSize  = 22.Ssp();
banner.HeightRequest = 120.Hdp();
sidePanel.WidthRequest = 30.SpaceWDp(AppDimens.Net.AppDimensAmbient.Require());  // 30 % of the window width
heroImage.WidthRequest = 240.FSdp();                                            // fluid: never explodes on desktop
```

## Step 3 — use it in XAML

```xml
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:ad="clr-namespace:AppDimens.Net.Maui.Xaml;assembly=AppDimens.Net.Maui"
             x:Class="MyApp.MainPage">

    <VerticalStackLayout Padding="{ad:Sdp 16}" Spacing="{ad:Sdp 8}">
        <Label Text="Title" FontSize="{ad:Ssp 22}" />
        <BoxView HeightRequest="{ad:Hdp 4}" />
        <Image WidthRequest="{ad:Sdpa 96}" HeightRequest="{ad:Sdpa 96}" />
        <Border WidthRequest="{ad:Psdpx 50}" />                    <!-- 50 % of the window width -->
        <Button HeightRequest="{ad:Fsdp 48}" Text="Fluid button" />
        <Label FontSize="{ad:Sdp Value=14, AspectRatio=True}" />
        <Label FontSize="{ad:Sdp Value=14, IgnoreMultiWindows=True}" />
    </VerticalStackLayout>
</ContentPage>
```

| Extension | Meaning |
|-----------|---------|
| `{ad:Sdp 16}` | smallest-width scaled (`AspectRatio=True`, `IgnoreMultiWindows=True` options) |
| `{ad:Sdpa 16}` | Sdp with aspect-ratio curve |
| `{ad:Hdp 48}` / `{ad:Wdp 100}` | height / width axis |
| `{ad:Ssp 16}` | text (follows font scale) |
| `{ad:Fsdp 24}` | fluid strategy |
| `{ad:Psdpx 50}` | literal percentage of the window width |

> XAML values are computed when the page is built. To follow live resizes, set the values in C# inside the
> change handler (next sections) or rebuild the page.

## Converters (MVVM)

```xml
<ContentPage.Resources>
    <ad:SdpConverter x:Key="Sdp" />
    <ad:SdpConverter x:Key="SdpAr" AspectRatio="True" />
    <ad:SspConverter x:Key="Ssp" />
</ContentPage.Resources>

<Label FontSize="{Binding TitleSize, Converter={StaticResource Ssp}}" />
<BoxView HeightRequest="{Binding CardHeight, Converter={StaticResource Sdp}}" />
```

## Reacting to resize / rotation

```csharp
using AppDimens.Net.Code.Scaled;
using AppDimens.Net.Maui.Platform;

var scope = AppDimensMaui.Primary!;
IDisposable registration = scope.RegisterConfigurationListener(() =>
{
    // called after every real change (rotation, resize, density, split screen)
    MainThread.BeginInvokeOnMainThread(() => titleLabel.FontSize = 22.Ssp());
});
// registration.Dispose() to stop listening
```

## Multiple windows

```csharp
using AppDimens.Net.Code.Scaled;
using AppDimens.Net.Maui.Platform;

// The ambient context follows the last attached window. For a secondary window use its own scope explicitly:
var secondary = new WindowDimensScope(new Window());
float pad = 16.Sdp(secondary);
secondary.Dispose();
```

## Split screen and font scale

```csharp
using AppDimens.Net.Maui.Platform;

// Custom multi-window detection (default: heuristic based on window vs. screen size)
AppDimensMaui.AttachWindow(new Window(), multiWindowProbe: () => false);

// Force a font scale (previews, accessibility tests)
WindowDimensScope.FontScaleOverride = 1.3;
AppDimensMaui.Primary?.Update();
```

## Complete example page

```csharp
using AppDimens.Net.Code.Scaled;
using AppDimens.Net.Maui.Platform;

public class DashboardPage : ContentPage
{
    readonly Label _title = new() { Text = "Dashboard" };
    readonly Border _card = new() { Content = new Label { Text = "Resize me" } };

    public DashboardPage()
    {
        Content = new VerticalStackLayout { Children = { _title, _card } };
        ApplySizes();
        AppDimensMaui.Primary?.RegisterConfigurationListener(() => MainThread.BeginInvokeOnMainThread(ApplySizes));
    }

    void ApplySizes()
    {
        Padding          = new Thickness(16.Sdp());
        _title.FontSize  = 24.Ssp();
        _card.Padding    = new Thickness(16.Sdp());
        _card.HeightRequest = 120.Hdp();
    }
}
```

## Troubleshooting

| Problem | Fix |
|---------|-----|
| `AppDimens ambient context is not initialized` | `AppDimensMaui.AttachWindow(window)` must run before the page is constructed. |
| Values don't follow window resizes on desktop | XAML is evaluated once — set values in C# inside `RegisterConfigurationListener`. |
| Want Android `@dimen`-style buckets / `{sdp:16}` short markup | Use [Bodenberg.AppDimens.Maui.Sdps](../Bodenberg.AppDimens.Maui.Sdps/README.md). |

---

## Português (resumo)

`dotnet add package AppDimens.Net.Maui` → em `App.CreateWindow`: `AppDimensMaui.AttachWindow(window)` **antes** de criar
páginas → C#: `16.Sdp()`, `22.Ssp()`, `120.Hdp()` → XAML: `xmlns:ad="clr-namespace:AppDimens.Net.Maui.Xaml;assembly=AppDimens.Net.Maui"`
e `{ad:Sdp 16}`, `{ad:Ssp 22}`. Para atualizar ao redimensionar, use `AppDimensMaui.Primary.RegisterConfigurationListener(...)`.

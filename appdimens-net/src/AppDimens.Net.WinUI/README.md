# AppDimens.Net.WinUI

📖 **Documentation:** [AppDimens.Net.WinUI guide](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.WinUI/README.md) · [complete API reference](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.WinUI/API.md) · [all AppDimens .NET packages](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/README.md)

**WinUI 3 (Windows App SDK) integration for [AppDimens.Net](https://www.nuget.org/packages/AppDimens.Net)** —
the pure C# port of AppDimens KMP. `{ad:Sdp Value=16}` values follow the window size and display
scale and are re-applied automatically when the window changes.

| | |
|---|---|
| **Frameworks** | `net8.0-windows10.0.19041.0`, `net9.0-…`, `net10.0-…` (min. Windows 10 1809) |
| **Windows App SDK** | 1.6 or newer (your app keeps its own version) |
| **Depends on** | `AppDimens.Net`, `AppDimens.Net.Fluid`, `AppDimens.Net.Percent` |

---

## Step 1 — Install

```bash
dotnet add package AppDimens.Net.WinUI
```

## Step 2 — Initialize in `App.OnLaunched`

```csharp
using AppDimens.Net.WinUI;

protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
{
    m_window = new MainWindow();
    AppDimensWinUI.Initialize(m_window);
    m_window.Activate();
}
```

## Step 3 — XAML

```xml
<Page ...
      xmlns:ad="using:AppDimens.Net.WinUI">

    <StackPanel Padding="{ad:Sdp Value=16}" Spacing="{ad:Sdp Value=8}">
        <TextBlock Text="Hello" FontSize="{ad:Ssp Value=22}" />
        <Button Content="Save" Width="{ad:Sdp Value=120}" Height="{ad:Hdp Value=40}" />
        <Border Width="{ad:SpaceW Value=50}" Height="{ad:FSdp Value=80}" CornerRadius="{ad:Sdp Value=12}" />
    </StackPanel>
</Page>
```

WinUI markup extensions need `Value=`. `Thickness`, `CornerRadius` and `GridLength` are converted automatically.
Before `Initialize` runs, values are used unscaled (no crash in the designer).

## Step 4 — C#

```csharp
using AppDimens.Net.Code.Scaled;
using AppDimens.Net.Code.Fluid;

saveButton.Width   = 120.Sdp();
titleText.FontSize = 22.Ssp();
AppDimensWinUI.Changed += (_, _) => titleText.FontSize = 22.Ssp();   // re-apply after resize
```

## Which function?

| Need | Function | XAML |
|------|----------|------|
| Most sizes | `Sdp` | `{ad:Sdp Value=16}` |
| Follow height / width | `Hdp` / `Wdp` | `{ad:Wdp Value=200}` |
| Text | `Ssp` | `{ad:Ssp Value=14}` |
| Gentle growth on big windows | `FSdp` | `{ad:FSdp Value=24}` |
| Percent strategy / literal % | `PSdp` / `SpaceW`, `SpaceH` | `{ad:SpaceW Value=50}` |

Converter for `{Binding}` / `{x:Bind}`: `<ad:DimenConverter x:Key="Dimen" Function="Sdp" />`.

## Português (resumo)

`dotnet add package AppDimens.Net.WinUI` → em `App.OnLaunched`: `AppDimensWinUI.Initialize(m_window);` → no XAML
`xmlns:ad="using:AppDimens.Net.WinUI"` e `{ad:Sdp Value=16}`, `{ad:Ssp Value=14}`. Em C#: `120.Sdp()`.

**More:** [AppDimens.Net core](https://www.nuget.org/packages/AppDimens.Net) ·
[Repository](https://github.com/bodenberg/appdimens-net-binding)

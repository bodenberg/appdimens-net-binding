# AppDimens.Net.Maui

**.NET MAUI integration for AppDimens.Net — live window scope, XAML markup extensions and converters.**

Brings `AppDimens.Net`, `AppDimens.Net.Fluid` and `AppDimens.Net.Percent` · `net8.0`–`net10.0` · Android, iOS, macOS, Windows.

📖 **Complete guide:** [AppDimens.Net.Maui documentation](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.Maui/README.md) · **API reference:** [API.md](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.Maui/API.md)

## Install

```bash
dotnet add package AppDimens.Net.Maui
```

## 1. Attach the window (App.xaml.cs)

```csharp
using AppDimens.Net.Maui.Platform;

public partial class App : Application
{
    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window();
        AppDimensMaui.AttachWindow(window);              // BEFORE creating pages
        window.Page = new NavigationPage(new MainPage());
        return window;
    }
}
```

## 2. C#

```csharp
using AppDimens.Net.Code.Scaled;

card.Padding        = new Thickness(16.Sdp());
titleLabel.FontSize = 22.Ssp();
banner.HeightRequest = 120.Hdp();
```

## 3. XAML

```xml
<ContentPage xmlns:ad="clr-namespace:AppDimens.Net.Maui.Xaml;assembly=AppDimens.Net.Maui" ...>
    <VerticalStackLayout Padding="{ad:Sdp 16}" Spacing="{ad:Sdp 8}">
        <Label Text="Title" FontSize="{ad:Ssp 22}" />
        <Border WidthRequest="{ad:Psdpx 50}" />   <!-- 50 % of the window width -->
        <Button HeightRequest="{ad:Fsdp 48}" />   <!-- fluid -->
    </VerticalStackLayout>
</ContentPage>
```

Extensions: `Sdp`, `Sdpa`, `Hdp`, `Wdp`, `Ssp`, `Fsdp`, `Psdpx` · converters: `SdpConverter`, `SspConverter`.

## Português

Em `CreateWindow`: `AppDimensMaui.AttachWindow(window)` antes de criar páginas; depois `16.Sdp()` em C# ou `{ad:Sdp 16}` no XAML.

---

Apache-2.0 — © Jean Bodenberg · [Repository](https://github.com/bodenberg/appdimens-net-binding) · Formulas are bit-for-bit ports of [appdimens-kmp](https://github.com/bodenberg/appdimens-kmp)

<div align="center">

# 🪟 AppDimens.Net.Maui

**MAUI bootstrap — window attach, event-driven resize watcher, XAML markup extensions and converters.**

Part of the **AppDimens for .NET** family · requires [AppDimens.Net](https://www.nuget.org/packages/AppDimens.Net/) (core).

[![NuGet](https://img.shields.io/nuget/v/AppDimens.Net.Maui.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/AppDimens.Net.Maui/)
[![License](https://img.shields.io/badge/license-Apache--2.0-blue.svg?style=flat-square)](https://github.com/bodenberg/appdimens-net-binding/blob/main/LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%209.0%20%7C%2010.0-8A2BE2.svg?style=flat-square)](https://dotnet.microsoft.com/)

</div>

---

## 🎯 What is it?

The glue between the AppDimens engine and **.NET MAUI**: one call attaches your `Window`
and installs an ambient context with a snapshot-partitioned cache and an **event-driven
resize watcher** (listeners fire only on real configuration deltas). Also includes XAML
markup extensions and value converters so layouts can scale without code-behind.

## 📦 Installation

```bash
dotnet add package AppDimens.Net.Maui
```

> Depends on [AppDimens.Net](https://www.nuget.org/packages/AppDimens.Net/) (pulled automatically).

## 🚀 Getting started

```csharp
using AppDimens.Net.Maui.Platform;

protected override Window CreateWindow(IActivationState? state)
{
    var window = new Window();
    AppDimensMaui.AttachWindow(window);      // BEFORE creating any page!
    window.Page = new NavigationPage(new MainPage());
    return window;
}
```

> ⚠️ Parameterless scaling extensions (`16.Sdp()`) resolve through the ambient scope —
> attach the window before constructing pages that use them.

## 🧭 API

### Bootstrap — `AppDimensMaui`

| Member | Purpose |
|---|---|
| `AttachWindow(window, multiWindowProbe?)` | installs ambient context + watcher; returns the scope |
| `Detach()` | removes the primary scope |
| `Init(application)` | app-level convenience init |
| `Primary` | current primary `WindowDimensScope` |

### Context — `WindowDimensScope`

| Member | Purpose |
|---|---|
| implements `IAppDimensContext` | density, screen dp, orientation, fontScale |
| `RegisterConfigurationListener(Action)` | resize/config-change subscription |
| `Update()` | manual refresh |
| `FontScaleOverride` | force a font-scale for previews/tests |

### XAML markup extensions (`AppDimens.Net.Maui.Xaml`)

```xml
xmlns:d="clr-namespace:AppDimens.Net.Maui.Xaml;assembly=AppDimens.Net.Maui"

<Label Text="Hello"
       FontSize="{d:Ssp 16}"
       Padding="{d:Sdp 16}" />
```

Available: `SdpExtension`, `SdpaExtension`, `HdpExtension`, `WdpExtension`,
`SspExtension`, `FsdpExtension`, `PsdpxExtension` and friends.

### Converters

`SdpConverter` · `SspConverter` — bind raw numbers to scaled values in MVVM layouts.

## 💡 When to use

| Scenario | Recommendation |
|---|---|
| Any MAUI app using AppDimens | ✅ this package |
| Console/tests/headless engines | core only, explicit contexts |
| Everything-in-one reference | [Sdk meta-package](https://www.nuget.org/packages/AppDimens.Net.Sdk/) |

## 📚 Full package family

[AppDimens.Net](https://www.nuget.org/packages/AppDimens.Net/) ·
[Percent](https://www.nuget.org/packages/AppDimens.Net.Percent/) ·
[Power](https://www.nuget.org/packages/AppDimens.Net.Power/) ·
[Auto](https://www.nuget.org/packages/AppDimens.Net.Auto/) ·
[Logarithmic](https://www.nuget.org/packages/AppDimens.Net.Logarithmic/) ·
[Fluid](https://www.nuget.org/packages/AppDimens.Net.Fluid/) ·
[Interpolated](https://www.nuget.org/packages/AppDimens.Net.Interpolated/) ·
[Diagonal](https://www.nuget.org/packages/AppDimens.Net.Diagonal/) ·
[Perimeter](https://www.nuget.org/packages/AppDimens.Net.Perimeter/) ·
[Fill](https://www.nuget.org/packages/AppDimens.Net.Fill/) ·
[Fit](https://www.nuget.org/packages/AppDimens.Net.Fit/) ·
[Density](https://www.nuget.org/packages/AppDimens.Net.Density/) ·
[Resize](https://www.nuget.org/packages/AppDimens.Net.Resize/) ·
[Units](https://www.nuget.org/packages/AppDimens.Net.Units/) ·
[Sdk meta-package](https://www.nuget.org/packages/AppDimens.Net.Sdk/)

---

Apache-2.0 — © Jean Bodenberg · [Repository](https://github.com/bodenberg/appdimens-net-binding) · Formulas are bit-a-bit ports of [appdimens-kmp](https://github.com/bodenberg/appdimens-kmp)

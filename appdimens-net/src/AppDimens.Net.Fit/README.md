# AppDimens.Net.Fit

**`DimenFit` builder — one base value plus per-screen overrides (size class, orientation, UI mode).**

Part of **AppDimens for .NET** · requires `AppDimens.Net` · MAUI, WPF, Windows Forms, WinUI 3, plain .NET (`net8.0`–`net10.0`).

📖 **Complete guide:** [AppDimens.Net.Fit documentation](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.Fit/README.md) · **API reference:** [API.md](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.Fit/API.md)

## Install

```bash
dotnet add package AppDimens.Net.Fit
```

## Use

```csharp
using AppDimens.Net;
using AppDimens.Net.Code.Fit;
using AppDimens.Net.Common;

float paddingPx = 16.FitScaledDp()
    .Screen(UiModeType.Television, 40)          // 1st priority: UI mode
    .Screen(DpQualifier.SmallWidth, 600, 24)    // 2nd: size class (declare largest threshold first)
    .Screen(Orientation.Landscape, 20)          // 3rd: orientation
    .ApplyAspectRatio()
    .Ftsdp();                                   // pixels — also Fthdp(), Ftwdp(), ResolveAll(ctx)

float paddingDp = paddingPx / AppDimensAmbient.Require().Density;   // dp for MAUI / WPF / WinUI
```

## Português

Construtor de regras por tela: `16.FitScaledDp().Screen(DpQualifier.SmallWidth, 600, 24).Ftsdp()` (retorna **pixels**).

---

Apache-2.0 — © Jean Bodenberg · [Repository](https://github.com/bodenberg/appdimens-net-binding) · Formulas are bit-for-bit ports of [appdimens-kmp](https://github.com/bodenberg/appdimens-kmp)

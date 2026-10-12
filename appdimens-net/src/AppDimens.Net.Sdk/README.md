# AppDimens.Net.Sdk

**Meta-package — the AppDimens.Net core plus every scaling strategy in one reference.**

📖 **Complete guide:** [AppDimens.Net.Sdk documentation](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.Sdk/README.md)

## Install

```bash
dotnet add package AppDimens.Net.Sdk
dotnet add package AppDimens.Net.Maui      # or AppDimens.Net.Wpf / .WinForms / .WinUI for your UI framework
```

## Includes

`AppDimens.Net` (Scaled: `Sdp`, `Hdp`, `Wdp`, `Ssp`) · `Auto` (`ASdp`) · `Density` (`DSdp`) · `Diagonal` (`DGSdp`) ·
`Fill` (`FLSdp`) · `Fit` (`FitScaledDp()`) · `Fluid` (`FSdp`) · `Interpolated` (`ISdp`) · `Logarithmic` (`LOGSdp`) ·
`Percent` (`PSdp`, `SpaceWDp`) · `Perimeter` (`PRSdp`) · `Power` (`PWSdp`) · `Resize` · `Units`.

```csharp
using AppDimens.Net.Code.Scaled;
using AppDimens.Net.Code.Fluid;
using AppDimens.Net.Code.Auto;

float padding = 16.Sdp();    // layout
float title   = 22.Ssp();    // text
float hero    = 240.FSdp();  // never explodes on desktop
float card    = 320.ASdp();  // damped on tablets
```

| `16` on… | Phone 360×800 | Tablet 800×1280 |
|---|--:|--:|
| `Sdp` | 19.2 | 42.7 |
| `ASdp` | 19.2 | 30.2 |
| `FSdp` | 13.4 | 19.2 |
| `LOGSdp` | 17.2 | 22.3 |

## Português

Instala o núcleo e todas as estratégias. Regra simples: `Sdp` para layout, `Ssp` para texto, `FSdp`/`ASdp` se algo ficar grande demais.

---

Apache-2.0 — © Jean Bodenberg · [Repository](https://github.com/bodenberg/appdimens-net-binding) · Formulas are bit-for-bit ports of [appdimens-kmp](https://github.com/bodenberg/appdimens-kmp)

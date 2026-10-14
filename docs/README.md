# AppDimens .NET — documentation

One page per NuGet package. Every package has a **guide** (step by step, with complete examples for each use case)
and an **API reference** listing every public type and member.

> **Not sure where to start?** Pick your UI framework in [Which package do I need?](#which-package-do-i-need), open its
> guide and follow the steps — every code sample in these guides is compile-checked.
>
> **Using an AI coding agent?** Give it the skill for your library set from [`skills/`](../skills/README.md)
> (one per set: SDPS native, AppDimens.Net, Android Sdps/Ssps, Android Dynamic).

---

## Which package do I need?

| I am building… | Install | Guide |
|----------------|---------|-------|
| A **.NET MAUI** app (Android, iOS, Windows, macOS) with `{sdp:16}` XAML and Android-style buckets | `Bodenberg.AppDimens.Maui.Sdps` | [guide](Bodenberg.AppDimens.Maui.Sdps/README.md) |
| A **.NET MAUI** app with the KMP strategies (fluid, percent, auto, power…) | `AppDimens.Net.Maui` (+ `AppDimens.Net.Sdk`) | [guide](AppDimens.Net.Maui/README.md) |
| A **WPF** app | `Bodenberg.AppDimens.Sdps.Wpf` **or** `AppDimens.Net.Wpf` | [SDPS](Bodenberg.AppDimens.Sdps.Wpf/README.md) · [AppDimens.Net](AppDimens.Net.Wpf/README.md) |
| A **Windows Forms** app | `Bodenberg.AppDimens.Sdps.WinForms` **or** `AppDimens.Net.WinForms` | [SDPS](Bodenberg.AppDimens.Sdps.WinForms/README.md) · [AppDimens.Net](AppDimens.Net.WinForms/README.md) |
| A **WinUI 3** app | `Bodenberg.AppDimens.Sdps.WinUI` **or** `AppDimens.Net.WinUI` | [SDPS](Bodenberg.AppDimens.Sdps.WinUI/README.md) · [AppDimens.Net](AppDimens.Net.WinUI/README.md) |
| A native **.NET for Android** app (Activities, XML layouts `@dimen/_16sdp`) | `Bodenberg.AppDimens.Sdps` / `.Ssps` / `.Dynamic` | see [Android bindings](#android-bindings) |
| A console / Avalonia / Uno / game / test project | `AppDimens.Net` (`HostDimensScope`) | [guide](AppDimens.Net/README.md#custom-hosts-hostdimensscope) |

**SDPS or AppDimens.Net?** Both scale the same way for `Sdp`/`Ssp`. Choose **SDPS** (`Bodenberg.AppDimens.*Sdps*`) for
Android parity (pre-computed buckets, `{sdp:16}` short markup, 30 functions incl. frozen `*i` values). Choose
**AppDimens.Net** for the KMP parity and the 12+ strategies (fluid, percent, auto, power, logarithmic…).

---

## SDPS family (native .NET, Android SDP parity)

| Package | What it is | Guide | API |
|---------|-----------|-------|-----|
| `Bodenberg.AppDimens.Maui.Sdps` | .NET MAUI: `{sdp:16}`, `16.Sdp()`, builders, inverters, converters, buckets | [guide](Bodenberg.AppDimens.Maui.Sdps/README.md) | [API](Bodenberg.AppDimens.Maui.Sdps/API.md) |
| `Bodenberg.AppDimens.Sdps.Wpf` | WPF: live `{sdps:Sdp 16}` markup, `16.Sdp()` | [guide](Bodenberg.AppDimens.Sdps.Wpf/README.md) | [API](Bodenberg.AppDimens.Sdps.Wpf/API.md) |
| `Bodenberg.AppDimens.Sdps.WinForms` | Windows Forms: `120.SdpPx()`, `14.SspPt()`, `Apply(...)` | [guide](Bodenberg.AppDimens.Sdps.WinForms/README.md) | [API](Bodenberg.AppDimens.Sdps.WinForms/API.md) |
| `Bodenberg.AppDimens.Sdps.WinUI` | WinUI 3: `{sdps:Sdp Value=16}` re-applied on resize | [guide](Bodenberg.AppDimens.Sdps.WinUI/README.md) | [API](Bodenberg.AppDimens.Sdps.WinUI/API.md) |

## AppDimens.Net family

| Package | What it is | Guide | API |
|---------|-----------|-------|-----|
| `AppDimens.Net` | Core + Scaled strategy, builders, inverters, cache, `HostDimensScope`, testing | [guide](AppDimens.Net/README.md) | [API](AppDimens.Net/API.md) |
| `AppDimens.Net.Sdk` | Meta-package: core + every strategy | [guide](AppDimens.Net.Sdk/README.md) | — |
| `AppDimens.Net.Maui` | .NET MAUI integration | [guide](AppDimens.Net.Maui/README.md) | [API](AppDimens.Net.Maui/API.md) |
| `AppDimens.Net.Wpf` | WPF integration (live markup, 41 functions) | [guide](AppDimens.Net.Wpf/README.md) | [API](AppDimens.Net.Wpf/API.md) |
| `AppDimens.Net.WinForms` | Windows Forms integration | [guide](AppDimens.Net.WinForms/README.md) | [API](AppDimens.Net.WinForms/API.md) |
| `AppDimens.Net.WinUI` | WinUI 3 integration | [guide](AppDimens.Net.WinUI/README.md) | [API](AppDimens.Net.WinUI/API.md) |
| `AppDimens.Net.Auto` | Auto strategy (`ASdp`) — proportional, damped on big screens | [guide](AppDimens.Net.Auto/README.md) | [API](AppDimens.Net.Auto/API.md) |
| `AppDimens.Net.Density` | Density strategy (`DSdp`) — follows DPI | [guide](AppDimens.Net.Density/README.md) | [API](AppDimens.Net.Density/API.md) |
| `AppDimens.Net.Diagonal` | Diagonal strategy (`DGSdp`) — rotation-stable | [guide](AppDimens.Net.Diagonal/README.md) | [API](AppDimens.Net.Diagonal/API.md) |
| `AppDimens.Net.Fill` | Fill strategy (`FLSdp`) | [guide](AppDimens.Net.Fill/README.md) | [API](AppDimens.Net.Fill/API.md) |
| `AppDimens.Net.Fit` | `DimenFit` builder — per-screen overrides | [guide](AppDimens.Net.Fit/README.md) | [API](AppDimens.Net.Fit/API.md) |
| `AppDimens.Net.Fluid` | Fluid strategy (`FSdp`) — clamped 0.8×–1.2× | [guide](AppDimens.Net.Fluid/README.md) | [API](AppDimens.Net.Fluid/API.md) |
| `AppDimens.Net.Interpolated` | Interpolated strategy (`ISdp`) | [guide](AppDimens.Net.Interpolated/README.md) | [API](AppDimens.Net.Interpolated/API.md) |
| `AppDimens.Net.Logarithmic` | Logarithmic strategy (`LOGSdp`) — gentle growth | [guide](AppDimens.Net.Logarithmic/README.md) | [API](AppDimens.Net.Logarithmic/API.md) |
| `AppDimens.Net.Percent` | Percent strategy (`PSdp`) + literal % (`SpaceWDp`) | [guide](AppDimens.Net.Percent/README.md) | [API](AppDimens.Net.Percent/API.md) |
| `AppDimens.Net.Perimeter` | Perimeter strategy (`PRSdp`) | [guide](AppDimens.Net.Perimeter/README.md) | [API](AppDimens.Net.Perimeter/API.md) |
| `AppDimens.Net.Power` | Power strategy (`PWSdp`) with sensitivity `k` | [guide](AppDimens.Net.Power/README.md) | [API](AppDimens.Net.Power/API.md) |
| `AppDimens.Net.Resize` | Auto-fit text / squares in boxes | [guide](AppDimens.Net.Resize/README.md) | [API](AppDimens.Net.Resize/API.md) |
| `AppDimens.Net.Units` | mm / cm / inch → dp / px / sp | [guide](AppDimens.Net.Units/README.md) | [API](AppDimens.Net.Units/API.md) |

## Android bindings

`net10.0-android` bindings that embed the upstream Kotlin libraries (AAR **3.2.0**). Android only — for other
platforms use the native packages above.

| Package | What it is | Guide | API |
|---------|-----------|-------|-----|
| `Bodenberg.AppDimens.Sdps` | Layout `@dimen/_16sdp` + `DimenSdp` / `DimenSsp` code APIs | [guide](Bodenberg.AppDimens.Sdps/README.md) | [API](Bodenberg.AppDimens.Sdps/API.md) |
| `Bodenberg.AppDimens.Ssps` | Text only: `@dimen/_14ssp` + `DimenSsp` | [guide](Bodenberg.AppDimens.Ssps/README.md) | [API](Bodenberg.AppDimens.Ssps/API.md) |
| `Bodenberg.AppDimens.Dynamic` | Code-only scaling, 15 strategies | [guide](Bodenberg.AppDimens.Dynamic/README.md) | [API](Bodenberg.AppDimens.Dynamic/API.md) |

---

## Keeping these docs up to date

```bash
./scripts/generate-docs.sh   # rebuilds every API.md and re-mirrors the package-README guides
```

* `API.md` files are generated — never edit them by hand.
* Guides of `Bodenberg.*` packages mirror their package READMEs (edit the README, then regenerate).
* Guides of `AppDimens.Net.*` live here; their short NuGet READMEs link back to them.
* When a public API or setup call changes, update the matching agent skill in [`skills/`](../skills/README.md) too.
  Maintainer guide: [MAINTAINING.md](../MAINTAINING.md).

## Português

Cada pacote tem um **guia** (passo a passo, com exemplos completos) e uma **referência de API** completa. Comece pela
tabela [Which package do I need?](#which-package-do-i-need): escolha seu framework (MAUI, WPF, Windows Forms, WinUI 3 ou
Android nativo), abra o guia e siga os passos. Todos os exemplos de código dos guias são verificados por compilação.
Para agentes de IA há uma skill por conjunto de bibliotecas em [`skills/`](../skills/README.md).

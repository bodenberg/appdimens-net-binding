# AppDimens.Net.WinForms

📖 **Documentation:** [AppDimens.Net.WinForms guide](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.WinForms/README.md) · [complete API reference](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.WinForms/API.md) · [all AppDimens .NET packages](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/README.md)

**Windows Forms integration for [AppDimens.Net](https://www.nuget.org/packages/AppDimens.Net)** —
the pure C# port of AppDimens KMP. Sizes follow the form's client size and the monitor DPI, and are
recalculated every time the form is resized.

| | |
|---|---|
| **Frameworks** | `net8.0-windows`, `net9.0-windows`, `net10.0-windows` (+ `net11.0-windows` with SDK 11) |
| **Depends on** | `AppDimens.Net`, `AppDimens.Net.Fluid`, `AppDimens.Net.Percent` (installed automatically) |

---

## Step 1 — Install

```bash
dotnet add package AppDimens.Net.WinForms
```

## Step 2 — Initialize and size controls

```csharp
using AppDimens.Net.Code.Scaled;   // 16.Sdp(), 14.Ssp() (dp)
using AppDimens.Net.Code.Fluid;    // 24.FSdp()
using AppDimens.Net.WinForms;      // AppDimensWinForms, DimenPx

public partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
        AppDimensWinForms.Initialize(this);

        // Runs now and after every resize / DPI change:
        AppDimensWinForms.Apply(this, () =>
        {
            saveButton.Width  = DimenPx.ToPx(120.Sdp());       // dp → pixels
            saveButton.Height = DimenPx.ToPx(40.Hdp());
            saveButton.Font   = new Font(saveButton.Font.FontFamily, DimenPx.ToPt(14.Ssp())); // dp → points
            panel1.Padding    = DimenPx.Padding(16.Sdp());
            banner.Height     = DimenPx.Of(DimenFunction.FSdp, 80); // any function straight to pixels
        });
    }
}
```

## Units cheat sheet

| Value | What it is | Use with |
|-------|------------|----------|
| `16.Sdp()` | dp (device-independent, 1/96 inch) | input of `DimenPx` |
| `DimenPx.ToPx(dp)` | pixels for the form's monitor | `Width`, `Height`, `Size`, `Location` |
| `DimenPx.ToPt(dp)` | font points | `new Font(family, size)` |
| `DimenPx.Padding(dp)` / `DimenPx.Size(w, h)` | `Padding` / `Size` | padding, margins, sizes |
| `16.SdpPx()` | pixels (core API) | same as `DimenPx.ToPx(16.Sdp())` |

## Which function?

| Need | Function |
|------|----------|
| Most sizes | `Sdp` |
| Follow form height / width | `Hdp` / `Wdp` |
| Text | `Ssp` |
| Grows gently on large forms | `FSdp` (fluid) |
| Percent of the form | `SpaceW` / `SpaceH` via `DimenPx.Of(DimenFunction.SpaceW, 50)` |

## Tips

* Put **all** AppDimens assignments inside `Apply` so they are refreshed on resize.
* If a control looks scaled twice on high-DPI monitors, set `AutoScaleMode = AutoScaleMode.None` on that form.

## Português (resumo)

`dotnet add package AppDimens.Net.WinForms` → no construtor: `AppDimensWinForms.Initialize(this);` e coloque os
tamanhos dentro de `AppDimensWinForms.Apply(this, () => { ... })`. Converta dp em pixels com
`DimenPx.ToPx(120.Sdp())` e fontes em pontos com `DimenPx.ToPt(14.Ssp())`.

**More:** [AppDimens.Net core](https://www.nuget.org/packages/AppDimens.Net) ·
[Repository](https://github.com/bodenberg/appdimens-net-binding)

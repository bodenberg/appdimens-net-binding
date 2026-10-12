# AppDimens.Net.WinForms — complete guide

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

---

## Complete example form

```csharp
using System.Drawing;
using System.Windows.Forms;
using AppDimens.Net.Code.Scaled;
using AppDimens.Net.WinForms;

public class MainForm : Form
{
    private readonly Label _title = new() { Text = "Dashboard", AutoSize = true };
    private readonly Button _save = new() { Text = "Save" };
    private readonly FlowLayoutPanel _panel = new() { Dock = DockStyle.Fill };

    public MainForm()
    {
        Text = "AppDimens demo";
        ClientSize = new Size(900, 600);
        AutoScaleMode = AutoScaleMode.None;          // AppDimens handles DPI
        _panel.Controls.AddRange([_title, _save]);
        Controls.Add(_panel);

        AppDimensWinForms.Initialize(this);
        AppDimensWinForms.Apply(this, ApplySizes);
    }

    private void ApplySizes()
    {
        _panel.Padding = DimenPx.Padding(16.Sdp());
        _title.Font    = new Font(Font.FontFamily, DimenPx.ToPt(22.Ssp()), FontStyle.Bold);
        _save.Size     = DimenPx.Size(120.Sdp(), 40.Hdp());
        _save.Font     = new Font(Font.FontFamily, DimenPx.ToPt(14.Ssp()));
    }
}
```

## Any function to pixels

```csharp
using AppDimens.Net.WinForms;

int fluid   = DimenPx.Of(DimenFunction.FSdp, 24);     // fluid strategy, pixels
int percent = DimenPx.Of(DimenFunction.SpaceW, 50);   // 50 % of the form width, pixels
double dp   = DimenFunction.Hdp.Evaluate(48);         // dp value
```

## Scope control

```csharp
using AppDimens.Net.WinForms;

var scope = AppDimensWinForms.Scope;     // current FormDimensScope (or null)
scope?.Refresh();                        // re-read client size / DPI
AppDimensWinForms.Changed += (_, _) => { /* custom reaction */ };
AppDimensWinForms.Detach();              // stop tracking
```

## How it works

`FormDimensScope` converts the form's client size from pixels to dp (`pixels ÷ (DeviceDpi / 96)`) on `Resize`,
`DpiChanged` and `HandleCreated`; `Apply` re-runs your layout callback on the UI thread after every change and stops
when the owner control is disposed.

📚 API reference: [API.md](API.md) · Core concepts: [AppDimens.Net](../AppDimens.Net/README.md)

## Português (resumo)

`dotnet add package AppDimens.Net.WinForms` → no construtor: `AppDimensWinForms.Initialize(this);` e coloque os
tamanhos dentro de `AppDimensWinForms.Apply(this, () => { ... })`. Converta dp em pixels com
`DimenPx.ToPx(120.Sdp())` e fontes em pontos com `DimenPx.ToPt(14.Ssp())`.


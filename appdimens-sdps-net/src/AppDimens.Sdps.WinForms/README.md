# Bodenberg.AppDimens.Sdps.WinForms

**Responsive sizes for Windows Forms.** Instead of hard-coding `button1.Width = 120`, write
`button1.Width = 120.SdpPx()` and the size adapts to the form size and to the monitor DPI —
and is recalculated every time the user resizes the form.

Same engine as [`Bodenberg.AppDimens.Maui.Sdps`](https://www.nuget.org/packages/Bodenberg.AppDimens.Maui.Sdps)
(Android `sdp`/`ssp` parity), packaged for Windows Forms with **no .NET MAUI dependency**.

| | |
|---|---|
| **Frameworks** | `net8.0-windows`, `net9.0-windows`, `net10.0-windows` (+ `net11.0-windows` with SDK 11) |
| **Works with** | Any Windows Forms app (`<UseWindowsForms>true</UseWindowsForms>`), any DPI mode |
| **License** | Apache-2.0 |

---

## Table of contents

1. [How it works](#how-it-works)
2. [Step 1 — Install](#step-1--install)
3. [Step 2 — Initialize](#step-2--initialize)
4. [Step 3 — Size your controls inside `Apply`](#step-3--size-your-controls-inside-apply)
5. [Complete example form](#complete-example-form)
6. [Pixels, points and dp — which helper?](#pixels-points-and-dp--which-helper)
7. [Which function should I use?](#which-function-should-i-use)
8. [Tips for the Visual Studio designer](#tips-for-the-visual-studio-designer)
9. [Troubleshooting](#troubleshooting)
10. [Português (resumo)](#português-resumo)

---

## How it works

* You keep the numbers you already use (`120` for a button width, `14` for text…).
* AppDimens treats them as sizes on a **300-unit-wide reference screen** and scales them to your form.
* Windows Forms works in **pixels**, so the helpers ending in **`Px`** convert the result to pixels for the
  current monitor DPI (100 %, 125 %, 150 %…), and helpers ending in **`Pt`** give font **points**.

---

## Step 1 — Install

**Visual Studio:** right-click the project → **Manage NuGet Packages…** → **Browse** → search
`Bodenberg.AppDimens.Sdps.WinForms` → **Install**.

```bash
dotnet add package Bodenberg.AppDimens.Sdps.WinForms
```

---

## Step 2 — Initialize

In your main form constructor, **after** `InitializeComponent()`:

```csharp
using AppDimens.Sdps.WinForms;

public partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
        AppDimensWinForms.Initialize(this);   // ← sizes now follow THIS form
    }
}
```

---

## Step 3 — Size your controls inside `Apply`

Put all your AppDimens sizing inside **one** `Apply` block. It runs immediately and again every time
the form is resized or moved to a monitor with a different DPI:

```csharp
AppDimensWinForms.Apply(this, () =>
{
    saveButton.Width  = 120.SdpPx();          // width in pixels
    saveButton.Height = 40.HdpPx();
    saveButton.Font   = new Font(saveButton.Font.FontFamily, 14.SspPt());  // font in points

    panel1.Padding    = DimenPx.Padding(16.Sdp());  // same padding on all sides
    pictureBox1.Size  = DimenPx.Size(48.Sdp(), 48.Sdp());
});
```

That's it. 🎉

---

## Complete example form

```csharp
using System.Drawing;
using System.Windows.Forms;
using AppDimens.Sdps.WinForms;

public class MainForm : Form
{
    private readonly Label _title = new() { Text = "Dashboard", AutoSize = true };
    private readonly Button _save = new() { Text = "Save" };
    private readonly FlowLayoutPanel _panel = new() { Dock = DockStyle.Fill };

    public MainForm()
    {
        Text = "AppDimens demo";
        ClientSize = new Size(900, 600);
        _panel.Controls.AddRange([_title, _save]);
        Controls.Add(_panel);

        AppDimensWinForms.Initialize(this);
        AppDimensWinForms.Apply(this, ApplySizes);
    }

    private void ApplySizes()
    {
        _panel.Padding = DimenPx.Padding(16.Sdp());
        _title.Font    = new Font(Font.FontFamily, 22.SspPt(), FontStyle.Bold);
        _save.Size     = new Size(120.SdpPx(), 40.HdpPx());
        _save.Font     = new Font(Font.FontFamily, 14.SspPt());
        _save.Margin   = DimenPx.Padding(8.Sdp());
    }
}
```

---

## Pixels, points and dp — which helper?

| Helper | Returns | Use for |
|--------|---------|---------|
| `120.SdpPx()` / `40.HdpPx()` / `200.WdpPx()` | `int` **pixels** | `Width`, `Height`, `Location`, `Size` |
| `14.SspPt()` | `float` **points** | `new Font(family, size)` |
| `16.Sdp()` | `double` **dp** (device-independent) | input of `DimenPx` helpers |
| `DimenPx.ToPx(dp)` / `DimenPx.ToPt(dp)` | pixels / points | convert any dp value |
| `DimenPx.Padding(dp)` / `DimenPx.Size(w, h)` | `Padding` / `Size` | padding, margins, sizes |

Every one of the 30 functions has the three forms: `Name()`, `NamePx()`, `NamePt()`.

---

## Which function should I use?

| I want to size… | Use |
|-----------------|-----|
| Padding, margins, icons, buttons — **most cases** | `Sdp` → `120.SdpPx()` |
| Things that follow the form **height** | `Hdp` → `40.HdpPx()` |
| Things that follow the form **width** | `Wdp` → `200.WdpPx()` |
| **Text** | `Ssp` → `14.SspPt()` |
| Text ignoring the user font scale | `Sem` → `14.SemPt()` |
| Slightly bigger on very wide/tall forms | add **`a`**: `16.SdpaPx()` |
| Fixed after startup (ignores resizes) | add **`i`**: `16.SdpiPx()` |

---

## Tips for the Visual Studio designer

* Keep designing visually; AppDimens only overrides the properties you set inside `Apply`.
* Avoid fighting the built-in scaling: set `AutoScaleMode = AutoScaleMode.None` (or `Dpi`) on forms
  whose sizes are fully controlled by AppDimens, so Windows does not scale the same controls twice.
* Docking/anchoring (`Dock`, `Anchor`, `TableLayoutPanel`) still work — AppDimens is for the *sizes*.

---

## Troubleshooting

| Problem | Fix |
|---------|-----|
| Sizes don't change on resize | Put the assignments inside `AppDimensWinForms.Apply(this, …)`. |
| Everything is twice as big on a 200 % monitor | Two scalers are active: set `AutoScaleMode = AutoScaleMode.None` on that form. |
| Text too big/small | Use `SspPt()` (points) for fonts, not `SspPx()`. |

---

## Português (resumo)

1. Instale: `dotnet add package Bodenberg.AppDimens.Sdps.WinForms`
2. No construtor do formulário, depois de `InitializeComponent()`: `AppDimensWinForms.Initialize(this);`
3. Coloque os tamanhos dentro de `AppDimensWinForms.Apply(this, () => { ... });` — ele roda agora e a
   cada redimensionamento:
   * Larguras/alturas em **pixels**: `botao.Width = 120.SdpPx();`
   * Fontes em **pontos**: `label.Font = new Font(label.Font.FontFamily, 14.SspPt());`
   * Padding: `painel.Padding = DimenPx.Padding(16.Sdp());`

---

**Links:** [Repository](https://github.com/bodenberg/appdimens-net-binding) ·
[WPF version](https://www.nuget.org/packages/Bodenberg.AppDimens.Sdps.Wpf) ·
[MAUI version](https://www.nuget.org/packages/Bodenberg.AppDimens.Maui.Sdps)

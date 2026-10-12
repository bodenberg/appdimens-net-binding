# AppDimens.Net.Resize

**Auto-resize math — the largest font size or square side that fits a box (bounded steps + binary search).**

Part of **AppDimens for .NET** · requires `AppDimens.Net` · `net8.0`–`net10.0`.

📖 **Complete guide:** [AppDimens.Net.Resize documentation](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.Resize/README.md) · **API reference:** [API.md](https://github.com/bodenberg/appdimens-net-binding/blob/main/docs/AppDimens.Net.Resize/API.md)

## Install

```bash
dotnet add package AppDimens.Net.Resize
```

## Use

```csharp
using AppDimens.Net;
using AppDimens.Net.Code.Resize;

var ctx = AppDimensAmbient.Require();

float fontSp = DimenResize.AutoResizeTextPx("Black Friday — 70% OFF", boxWidthPx: 600f,
    minSp: 12f, maxSp: 32f, stepSp: 1f, maxLines: 2, ctx: ctx);          // result in sp
float iconDp = DimenResize.AutoResizeSquarePx(300f, 180f, minDp: 24f, maxDp: 120f, stepDp: 4f, ctx: ctx);  // result in dp
var (innerW, innerH) = DimenResize.InnerMaxDimensionsPx(400f, 200f, 16f, 16f, 8f, 8f);
float best = new ResizeRangePx(10f, 40f, 2f).ResolveFitting(px => px * 12 <= 300);
```

Box sizes are pixels; results come back in the unit of your range (text → sp, square → dp) despite the `Px` suffix.

## Português

`DimenResize.AutoResizeTextPx(...)` encontra o maior tamanho de fonte (sp) que cabe na caixa; `AutoResizeSquarePx(...)` faz o mesmo para ícones (dp).

---

Apache-2.0 — © Jean Bodenberg · [Repository](https://github.com/bodenberg/appdimens-net-binding) · Formulas are bit-for-bit ports of [appdimens-kmp](https://github.com/bodenberg/appdimens-kmp)

# AppDimens .NET — Plano de Conversão Completa (v2)

> Conversão fiel das bibliotecas **appdimens-kmp** (Kotlin Multiplatform, v1.0.1) e
> **appdimens-dynamic** (Android, v3.1.9) para **.NET / .NET MAUI multiplataforma**,
> substituindo a biblioteca MAUI atual (`appdimens-sdps-net/AppDimens.Maui.*`), que é
> baseada em grade XML/dimen e está deprecada.

Branch: `feat/appdimens-net-v2`

---

## 1. Análise das fontes (o que foi extraído)

| Fonte | Versão | O que foi aproveitado |
|---|---|---|
| `appdimens-kmp` | 1.0.1 | Arquitetura multiplataforma inteira: `ScreenConfiguration`, `AppDimensContext`, `DimenMetrics` (fatores pré-computados), `DimenCache` particionado por snapshot com fast slots, kernels especializados sem branches, watcher de configuração event-driven, correções de race da 1.0.1, API Compose→traduzida para MAUI (markup/extensions/converters), sample + benchlab, estrutura de documentação |
| `appdimens-dynamic` | 3.1.9 | Fórmulas canônicas idênticas ao KMP; facilitadores Plain (`*RotatePlain/*ModePlain/...`); catálogo completo de sufixos `a/i/ia`; inverters; catálogo COMPOSE-API-CONVENTIONS |
| `appdimens-sdps-net` (MAUI atual) | — | Lições: markup `{sdp:16}` XAML, converters; **arquitetura descartada** (grade de recursos XML por índice — não responsiva a resize em runtime) |

### Matemática portada bit-a-bit (`float`, ordem legacy `base * fator * densidade`)

```
INV_BASE_RATIO       = 0.0033333334f   // 1/300
ADJUSTMENT_SCALE     = 0.10/30         SENSITIVITY_DEFAULT = 0.08/30
BASE_WIDTH_DP = 300   BASE_HEIGHT_DP = 533
BASE_DIAGONAL_DP = √(300²+533²)=611.6305f   BASE_PERIMETER_DP = 833
REFERENCE_ASPECT_RATIO = 1.78f

scale                = sw * INV_BASE_RATIO
widthFactor/heightFactor = w/h * INV_BASE_RATIO
normalizedAR         = (max/min)/1.78          logAR = ln(normalizedAR)
arMultiplierDefault  = 1 + SENSITIVITY_DEFAULT * logAR
scaledArDefault      = 1 + (sw-300)*(ADJUSTMENT_SCALE + SENSITIVITY_DEFAULT*logAR)
powerScale           = (sw/300)^0.75           interpolatedScale = 1+(sw/300-1)*0.5
diagonalScale        = √(min²+max²)/611.6305   perimeterScale    = (min+max)/833
logarithmicScale     = sw>300 ? 1+0.4·ln(sw/300) : sw>0 ? 1−0.4·ln(300/sw) : 1

FLUID : clamp lerp min=base·0.8 max=base·1.2 em dim∈[320..768]
AUTO  : dim≤480 → dim/300 ; senão 480/300+0.4·ln(1+(dim−480)/300)
FILL  : max(minSide/300, maxSide/533)   (ignora qualifier/inverter)
DENSITY: base · density
PERCENT: base·dim/300 ; com AR: base·(1+(dim−300)(ADJ+k·logAR))
SPACE : percent/100 · dim  (i → retorna o próprio percent se multi-janela)
UNITS : mm→dp = mm·xdpi/25.4/density ; cm=mm·10 ; sp divide por (density·fontScale)
RESIZE: passos binários maior-cabe (ResizeRangePx, máx 4096 passos, ε=step·1e-4)
```

Sufixo **`i`** (`ignoreMultiWindows=true`): heurística `(sw − cw) ≥ sw·0.1` ativa ⇒
**retorna o valor-base sem escalar** — é exatamente o comportamento pedido:
redimensionamento ajusta tudo automaticamente, *exceto* as variantes `…i`.

## 2. Arquitetura destino (.NET)

```
AppDimens.Net.sln
├── src/
│   ├── AppDimens.Net/            ← PRINCIPAL (common+core+scaled+plain; net8.0 puro)
│   ├── AppDimens.Net.Maui/       ← cola MAUI (contextos por OS, watcher, XAML/markup)
│   ├── AppDimens.Net.Percent|Power|Fluid|Auto|Density|Diagonal|Fill|Fit|
│   │      Interpolated|Logarithmic|Perimeter|Resize|Units  ← 13 satélites (net8.0 puro)
│   └── AppDimens.Net.Sdk/        ← meta-pacote "BOM" (referencia todos)
├── tests/AppDimens.Net.Tests/    ← xUnit: paridade, kernels, cache/race, builders
├── samples/AppDimens.Sample/     ← app demo multiplataforma (UI compartilhada + heads)
└── benchlab/AppDimens.BenchLab/  ← micro-benchmark comparativo + export de relatório
```

- **Toda a matemática é `net8.0` puro** → compila/testa em qualquer SO; os heads MAUI
  (Android/iOS/MacCatalyst/Windows) só fazem bootstrap.
- **Paridade de API**: mesmos nomes do KMP em PascalCase (`Sdp()`, `Sdpa()`, `HdpLw()`,
  `SdpRotate()`, `ScaledDp()` builder, `SpaceW()`, `Asdp()`, …), namespaces espelhando
  pacotes Kotlin (`AppDimens.Net.Code.Scaled`, `.Code.Percent`, …).
- **Performance = KMP**: kernels especializados sem branches; fast lane = 1 leitura
  `Volatile` + comparação de identidade + 2 multiplicações float; cache particionado por
  snapshot (4×512) publicado como referência única atômica (`FastPartitionSlot`);
  chaves 64-bit empacotadas idênticas; bypass de multiplicação barata nos caminhos
  PERCENT/SCALED/DENSITY/DIAGONAL/INTERPOLATED/PERIMETER (+condicional POWER/LOG).
- **Auto-resize**: watcher event-driven — `DeviceDisplay.MainDisplayInfoChanged`,
  `Window.SizeChanged` e listener registrável invalidam os fast slots sincronamente;
  nenhuma amostragem no caminho quente. Variantes `…i` ignoram a reescala em
  multi-janela (comportamento preservado).

## 3. Ordem de execução (pedido do usuário)

1. Módulo principal (`Common`+`Core`+`Code.Scaled`) ✔ etapa 2
2. Satélites ✔ etapa 3
3. Camada MAUI ✔ etapa 4
4. Sample App ✔ etapa 5
5. BenchLab ✔ etapa 6
6. Testes completos + auditoria ✔ etapas 7–8
7. Documentação nível KMP/Dynamic ✔ etapa 9

## 4. Critérios de aceite

- [ ] `dotnet build` verde na solução completa (Linux: libs+testes; heads documentados p/ CI Win/macOS)
- [ ] Suíte de testes 100% verde: paridade numérica vs valores-golden derivados das fórmulas KMP,
      kernels especializados, builders/prioridades 1–4, inverters, cache (race entre snapshots),
      custom-K nunca cacheado, watchers/invalidez
- [ ] Fast lane sem alocação por resolução (hit path)
- [ ] Documentação: README, GUIDE-FOR-BEGINNERS, PERFORMANCE(+COMPARATIVE), TRIMMING-AOT,
      DOCUMENTATION/* (por estratégia + MODULES + MATHEMATICS + PRD/PDR + index + API conventions),
      LLMS.txt, CHANGELOG

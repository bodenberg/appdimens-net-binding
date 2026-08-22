# Auditoria Final — AppDimens .NET 3.6.0

Data: 2026-08-22 · Escopo: paridade KMP, performance, estabilidade de apps,
build multi-TFM, testes (máquina/navegador/dispositivos ADB).

## 1. Defeitos encontrados e corrigidos

| # | Severidade | Defeito | Correção | Cobertura |
|---|---|---|---|---|
| 1 | Alta | `DimenResize.AutoResizeSquarePx` convertia px→dp e depois comparava `candidate × density ≤ limite-dp` (densidade aplicada 2×) → caixas menores que o necessário | Conversão única px→dp (paridade KMP: `side ≤ min(maxW,maxH)`) | Teste de regressão `Auto_resize_square_returns_largest_side_that_fits_inner_box` + web `resize-square-px-limit` |
| 2 | Alta | BenchLab antigo crashava no arranque (`InvalidOperationException` em `ScaledExtensions.Ssp` ← `BenchMainPage..ctor`): páginas construídas **antes** de `AttachWindow` | UI nova sem fontes `.Ssp()` no ctor + leituras ambientais protegidas; README documenta a ordem obrigatória | Emulador + Xiaomi: launch limpo, logcat sem FATAL pós-fix |
| 3 | Média | Fast lanes `ResolveSdpDp/Hdp/Wdp(+Px)` pré-multiplicavam `dim×ratio` → diferença de 1 ulp vs caminho completo (associatividade IEEE754) | Ordem exata do kernel `(base × dim) × ratio [× density]` | Web suite `sdp-fast-lane==full-path(8..100)` bitwise, 21/21 |
| 4 | Baixa | Teste `DimenFit_falls_back_to_base_when_nothing_matches` dependia de estado ambiente vazado entre classes de teste (flaky) | Hermeticizado com save/restore do ambiente | 3× execuções verdes consecutivas |
| 5 | Baixa | Timers de UI (MainPage/CacheInfo) nunca paravam em `OnDisappearing` → ticks em background (risco de ANR/bateria) | Stop/unsubscribe no ciclo de vida | Navegação end-to-end sem ANR no logcat |
| 6 | Baixa | BenchLab podia reentrar em runs simultâneos (substituía probe em voo, cascata de timeouts — mesmo bug documentado no controller KMP) | Guard `BenchmarkRunner.IsRunning` + `RunAsync` chunked com progresso | Execução dupla no emulador: 2ª chamada é no-op |

## 2. Resultados de teste

### Máquina (Linux, net10.0)
- `dotnet build AppDimens.Net.slnx` — **verde, 0 avisos/0 erros**
- `dotnet test` — **48/48** (estável em execuções repetidas)

### Navegador (Blazor WASM + Chromium headless)
- `samples/AppDimens.WebDemo` — **21/21 aprovados, 0 erros de console**
  (fast-lane bitwise, 5 satélites, resize math, watcher, partição de cache)

### Emulador Android (API 36.1, 1080×2400 @420dpi)
- Sample: navegação completa (seções 1–5, estratégias, builders, cache) — sem
  ANR/crash; valores conferidos (16.sdp=21.92 = 16×411/300; 48.hdp=146.24)
- BenchLab: benchmark completo **271 ms** — fast lane 8.2–10.2 ns/op
  (~100–120M ops/s) vs caminho cached 69.4 ns/op vs legado XML-grid 34.6 ns/op;
  **0 B alocados** em todas as linhas
- logcat: 0 FATAL / 0 ANR para `com.bodenberg.*`

### Dispositivo físico (Xiaomi, adb-tls)
- Ambos os APKs instalados e lançados com sucesso (`Status: ok`)
- Crash histórico do BenchLab antigo (14:41) confirmado como causa-raiz #2 e
  eliminado; processo novo estável
- Injeção de toque bloqueada pelo MIUI (INJECT_EVENTS) — limitação do aparelho,
  não do app; validação visual feita via screenshot do Sample

## 3. Paridade numérica (golden, sw=360 dpi=420)

| Chamada | Esperado | Obtido |
|---|---|---|
| 16.Sdp | 19.2 | 19.2 |
| 48.Hdp | 118.4 | 118.4 |
| 24.PSdp | 28.8 | 28.8 |
| 28.ToFluidDp | 23.4 | 23.4 |
| 28.ASdp | 33.6 | 33.6 |
| 12.DSdp | 31.5 | 31.5 |
| 18.DGSdp | 24.2182 | 24.2182 |
| AutoResizeSquarePx(400dpx,100dpx,16..200,4) | 100 | 100 |

## 4. Build multi-TFM

- Linux: libs `net8.0/net9.0/net10.0` + heads `net10.0-android36.1` (APKs
  Release assinados para Sample e BenchLab)
- Windows/macOS: TFMs `net8.0-android/ios/maccatalyst/windows10.0.19041`
  habilitados por condição no csproj (documentado para CI)

## 5. Versão

- Todas as 16 bibliotecas em **3.6.0** via `Directory.Build.props`
  (`AppDimens.Net`, `.Percent`, `.Power`, `.Auto`, `.Logarithmic`, `.Fluid`,
  `.Interpolated`, `.Diagonal`, `.Perimeter`, `.Fit`, `.Fill`, `.Density`,
  `.Resize`, `.Units`, `.Maui`, `.Sdk`)

## 6. Documentação entregue

`README.md`, `CHANGELOG.md`, `LLMS.txt`, `DOCUMENTATION/GUIDE-FOR-BEGINNERS.md`,
`DOCUMENTATION/MODULES.md`, `DOCUMENTATION/PARITY.md`,
`DOCUMENTATION/PERFORMANCE.md`, `DOCUMENTATION/TRIMMING-AOT.md`.

## 7. Riscos residuais / recomendações

- CI multi-OS (Windows/macOS) para compilar os heads iOS/MacCatalyst/Windows —
  código pronto, só falta runner.
- Adicionar testes de golden para `Perimeter/Logarithmic/Interpolated/Power`
  com valores do KMP oficial (hoje cobertos por invariantes + demo).
- Considerar `IsAotCompatible=true` explícito nos csproj das libs.
